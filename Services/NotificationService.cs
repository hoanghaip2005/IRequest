using App.Models.IRequest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using RequestModel = App.Models.IRequest.Request;
using Microsoft.AspNetCore.Identity;

namespace Request.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string title, string content, string type, string userId, int? requestId = null);
        Task CreateRequestNotificationAsync(RequestModel request, string type, string userId);
        Task CreateWorkflowStepNotificationAsync(RequestModel request, WorkflowStep step, string type);
        Task CreateApprovalNotificationAsync(RequestModel request, string userId, string action);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NotificationService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateNotificationAsync(string title, string content, string type, string userId, int? requestId = null)
        {
            var notification = new Notification
            {
                Title = title,
                Content = content,
                Type = type,
                UserId = userId,
                RequestId = requestId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateRequestNotificationAsync(RequestModel request, string type, string userId)
        {
            string title = "";
            string content = "";

            switch (type)
            {
                case "created":
                    title = "Yêu cầu mới được tạo";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã được tạo";
                    break;
                case "updated":
                    title = "Yêu cầu được cập nhật";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã được cập nhật";
                    break;
                case "closed":
                    title = "Yêu cầu đã đóng";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã được đóng";
                    break;
            }

            await CreateNotificationAsync(title, content, type, userId, request.RequestID);

            if (type == "created" && !string.IsNullOrEmpty(request.AssignedUserId))
            {
                var assignedUserTitle = "Yêu cầu mới cần xử lý";
                var assignedUserContent = $"Bạn được chỉ định xử lý yêu cầu #{request.RequestID} - {request.Title}";
                await CreateNotificationAsync(assignedUserTitle, assignedUserContent, "in_progress", request.AssignedUserId, request.RequestID);
            }
        }

        public async Task CreateWorkflowStepNotificationAsync(RequestModel request, WorkflowStep step, string type)
        {
            string title = "";
            string content = "";

            switch (type)
            {
                case "step_started":
                    title = "Yêu cầu mới cần xử lý";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã được chuyển đến bước {step.StepName} và cần bạn xử lý";
                    break;
                case "step_completed":
                    title = "Yêu cầu đã hoàn thành bước";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã hoàn thành bước {step.StepName}";
                    break;
                case "step_timeout":
                    title = "Cảnh báo: Yêu cầu quá thời gian xử lý";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã vượt quá thời gian xử lý cho phép ({step.TimeLimitHours} giờ)";
                    break;
            }

            // Gửi thông báo cho người được chỉ định trong bước tiếp theo
            if (!string.IsNullOrEmpty(step.AssignedUserId))
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    Type = type,
                    UserId = step.AssignedUserId,
                    RequestId = request.RequestID,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            // Gửi thông báo cho người có role phù hợp
            if (!string.IsNullOrEmpty(step.RequiredRoleId))
            {
                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Contains(step.RequiredRoleId))
                    {
                        var notification = new Notification
                        {
                            Title = title,
                            Content = content,
                            Type = type,
                            UserId = user.Id,
                            RequestId = request.RequestID,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                        _context.Notifications.Add(notification);
                    }
                }
            }

            // Gửi thông báo cho người tạo yêu cầu
            if (type == "step_completed" || type == "step_started")
            {
                var creatorNotification = new Notification
                {
                    Title = type == "step_completed" ? "Yêu cầu của bạn đã được xử lý" : "Yêu cầu của bạn đã được chuyển tiếp",
                    Content = type == "step_completed" 
                        ? $"Yêu cầu #{request.RequestID} - {request.Title} đã được xử lý ở bước {step.StepName}"
                        : $"Yêu cầu #{request.RequestID} - {request.Title} đã được chuyển đến bước {step.StepName}",
                    Type = type,
                    UserId = request.UsersId,
                    RequestId = request.RequestID,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(creatorNotification);
            }

            // Lưu tất cả thông báo vào database
            await _context.SaveChangesAsync();
        }

        public async Task CreateApprovalNotificationAsync(RequestModel request, string userId, string action)
        {
            string title = "";
            string content = "";

            switch (action)
            {
                case "approved":
                    title = "Yêu cầu đã được phê duyệt";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã được phê duyệt";
                    break;
                case "rejected":
                    title = "Yêu cầu đã bị từ chối";
                    content = $"Yêu cầu #{request.RequestID} - {request.Title} đã bị từ chối";
                    break;
            }

            await CreateNotificationAsync(title, content, action, userId, request.RequestID);
        }
    }
} 