using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using RequestModel = App.Models.IRequest.Request;
using App.Models.IRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Services;
using System.Security.Claims;
using Request.Services;

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("request/[action]")]
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RequestController> _logger;
        private readonly WorkflowService _workflowService;
        private readonly IWorkflowStepService _workflowStepService;
        private readonly INotificationService _notificationService;

        public RequestController(AppDbContext context, UserManager<AppUser> userManager, ILogger<RequestController> logger, WorkflowService workflowService, IWorkflowStepService workflowStepService, INotificationService notificationService)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _workflowService = workflowService;
            _workflowStepService = workflowStepService;
            _notificationService = notificationService;
        }

        [HttpGet("/request/home")]
        public async Task<IActionResult> Index(string searchString)
        {
            var userId = _userManager.GetUserId(User);
            var currentUser = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var requests = _context.Requests
                .Include(r => r.AssignedUser)
                    .ThenInclude(u => u.Department)
                .Include(r => r.Status)
                .Include(r => r.Priority)
                .Include(r => r.User)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.User)
                .Include(r => r.Workflow)
                    .ThenInclude(w => w.Steps)
                .AsQueryable();

            var allRequests = await requests.ToListAsync();
            _logger.LogInformation($"[Index] Tổng số request trong DB: {allRequests.Count}");
            foreach (var req in allRequests)
            {
                _logger.LogInformation($"[Index] RequestID: {req.RequestID}, Title: {req.Title}, UsersId: {req.UsersId}, AssignedUserId: {req.AssignedUserId}, StatusID: {req.StatusID}, WorkflowID: {req.WorkflowID}, CreatedAt: {req.CreatedAt}");
            }

            // Nếu không phải admin thì chỉ lấy request mà user là người gửi hoặc có vai trò phụ trách
            if (!User.IsInRole("Administrator"))
            {
                requests = requests.Where(r => 
                    // User là người gửi yêu cầu
                    r.UsersId == userId ||
                    // Hoặc user có vai trò phụ trách trong bước hiện tại của workflow
                    r.Workflow.Steps.Any(s =>
                        s.StepOrder == r.CurrentStepOrder &&
                        (
                            (!string.IsNullOrEmpty(s.AssignedUserId) && s.AssignedUserId == userId) ||
                            (!string.IsNullOrEmpty(s.RequiredRoleId) && userRoles.Contains(s.RequiredRoleId))
                        )
                    )
                );
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                requests = requests.Where(r => r.Title.Contains(searchString) || r.Description.Contains(searchString));
            }

            var resultList = await requests.OrderByDescending(r => r.CreatedAt).ToListAsync();
            _logger.LogInformation($"[Index] Số request trả về view: {resultList.Count}");
            foreach (var req in resultList)
            {
                _logger.LogInformation($"[Index] Request hiển thị: {req.RequestID} - {req.Title}");
            }



            ViewBag.AllUsers = _userManager.Users.ToList();
            ViewBag.AllWorkflows = _context.Workflows.ToList();
            ViewBag.AllStatuses = _context.Status.ToList();
            return View(await requests.OrderByDescending(r => r.CreatedAt).ToListAsync());
        }

        [HttpGet("/request/details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.Workflow)
                    .ThenInclude(w => w.Steps)
                .Include(r => r.AssignedUser)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.RequestID == id);
            if (request == null)
            {
                return NotFound();
            }

            // Lấy lịch sử bước
            var stepHistories = _context.RequestStepHistories
                .Where(h => h.RequestID == request.RequestID)
                .OrderBy(h => h.ActionTime)
                .ToList();

            ViewBag.StepHistories = stepHistories;

            return View(request);
        }
        [TempData]
        public string? StatusMessage { set; get; }

        [HttpGet("/request/create")]
        [AllowAnonymous]
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            // Lấy user có vai trò admin
            var adminUser = _userManager.GetUsersInRoleAsync("Administrator").Result.FirstOrDefault();
            var defaultAssignedUserId = adminUser?.Id ?? _userManager.Users.FirstOrDefault()?.Id;

            var request = new RequestModel
            {
                UsersId = userId,
                AssignedUserId = defaultAssignedUserId
            };

            ViewBag.IsAdminOrMember = User.IsInRole("Administrator") || User.IsInRole("Member");
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description");
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName");
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>().Where(w => w.IsActive), "WorkflowID", "WorkflowName");
            ViewData["Users"] = new SelectList(_userManager.Users.ToList(), "Id", "UserName", request.AssignedUserId);

            return View(request);
        }

        [HttpPost("/request/create")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestID,Title,Description,AttachmentURL,IsApproved,StatusID,PriorityID,WorkflowID,UsersId,AssignedUserId")] RequestModel request)
        {
            // Nếu thiếu AssignedUserId thì gán mặc định là admin
            if (string.IsNullOrEmpty(request.AssignedUserId))
            {
                var adminUser = await _userManager.GetUsersInRoleAsync("Administrator");
                request.AssignedUserId = adminUser.FirstOrDefault()?.Id ?? (await _userManager.Users.FirstOrDefaultAsync())?.Id;
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(request.UsersId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng. Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }

                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                var firstStep = _context.WorkflowSteps
                    .FirstOrDefault(s => s.WorkflowID == request.WorkflowID && s.StepOrder == 1);

                if (firstStep == null)
                {
                    TempData["ErrorMessage"] = "Quy trình chưa có bước bắt đầu (StepOrder = 1). Vui lòng kiểm tra lại quy trình!";
                    // Truyền lại các ViewData khi ModelState không hợp lệ
                    ViewData["WorkflowID"] = new SelectList(_context.Workflows.Where(w => w.IsActive), "WorkflowID", "WorkflowName", request.WorkflowID);
                    ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description", request.PriorityID);
                    ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName", request.StatusID);
                    ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
                    return View(request);
                }

                // Nếu tìm thấy bước đầu tiên thì gán như cũ
                request.CurrentStepOrder = firstStep.StepOrder;
                request.StatusID = firstStep.StatusID;
                if (firstStep.RequiredRoleId == "approver")
                {
                    request.RoleId = "approver";
                    request.AssignedUserId = null;
                }
                else
                {
                    request.RoleId = "user";  // Set default role for non-approver steps
                    request.AssignedUserId = firstStep.AssignedUserId;
                }

                _context.Add(request);
                await _context.SaveChangesAsync();

                // Tạo thông báo cho người tạo yêu cầu
                await _notificationService.CreateRequestNotificationAsync(request, "created", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Tạo thông báo cho bước đầu tiên của workflow
                if (firstStep != null)
                {
                    await _notificationService.CreateWorkflowStepNotificationAsync(request, firstStep, "step_started");
                }

                TempData["StatusMessage"] = "Tạo mới yêu cầu thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Truyền lại các ViewData khi ModelState không hợp lệ
            ViewData["WorkflowID"] = new SelectList(_context.Workflows.Where(w => w.IsActive), "WorkflowID", "WorkflowName", request.WorkflowID);
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description", request.PriorityID);
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName", request.StatusID);
            ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
            return View(request);
        }

        [HttpGet("/request/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Request ID is null when trying to edit.");
                return NotFound(); // Ensure id is not null
            }

            var request = await _context.Requests
                .Include(r => r.User)  // Include the requester
                .Include(r => r.AssignedUser)  // Include the assigned user
                .FirstOrDefaultAsync(r => r.RequestID == id); // Fetch the request based on RequestID

            if (request == null)
            {
                _logger.LogWarning($"Request with ID {id} not found.");
                return NotFound(); // If no matching request is found
            }

            // Ensure ViewData["Users"] is populated with the list of users for the dropdown
            ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "PriorityName", request.PriorityID);

            return View(request); // Pass the request to the view
        }

        [HttpPost("/request/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestID,Title,Description,AttachmentURL,IsApproved,StatusID,PriorityID,WorkflowID,UsersId,AssignedUserId")] RequestModel request)
        {
            if (id != request.RequestID)
            {
                _logger.LogWarning($"Request ID mismatch: {id} != {request.RequestID}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Thêm log này để xem lỗi gì
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"ModelState error for {key}: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    request.UpdatedAt = DateTime.UtcNow;
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                    
                    // Tạo thông báo cho người cập nhật yêu cầu
                    await _notificationService.CreateRequestNotificationAsync(request, "updated", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                    
                    TempData["StatusMessage"] = "Cập nhật trạng thái thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestExists(request.RequestID))
                    {
                        _logger.LogWarning($"Request with ID {id} not found during update.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError($"Concurrency error while updating request with ID {id}");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "PriorityName", request.PriorityID);

            return View(request);
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.RequestID == id);
        }

        [HttpGet("/request/delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.Workflow)
                .FirstOrDefaultAsync(m => m.RequestID == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost("/request/delete/{id?}"), ActionName("DeleteConfirmedRequest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request != null)
            {
                _context.Requests.Remove(request);
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Xóa trạng thái thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/request/viewhome")]
        public async Task<IActionResult> ViewHome()
        {
            var appDbContext = _context.Requests.Include(r => r.Priority).Include(r => r.Status).Include(r => r.Workflow);
            return View("ViewHome", await appDbContext.ToListAsync());
        }

        [HttpPost("/request/delete-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple([FromForm] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "No items selected for deletion.";
                return RedirectToAction("Index");
            }

            try
            {
                // First delete all associated comments
                var comments = _context.Comments.Where(c => c.RequestId.HasValue && ids.Contains(c.RequestId.Value));
                _context.Comments.RemoveRange(comments);

                // Then delete the requests
                var requests = _context.Requests.Where(r => ids.Contains(r.RequestID));
                _context.Requests.RemoveRange(requests);

                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Deleted selected requests successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the requests: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
        [HttpPost("/request/assign-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMultiple([FromForm] int[] ids, [FromForm] string assignedUserId)
        {
            if (ids == null || ids.Length == 0 || string.IsNullOrEmpty(assignedUserId))
                return BadRequest();

            var requests = _context.Requests.Where(r => ids.Contains(r.RequestID));
            foreach (var req in requests)
                req.AssignedUserId = assignedUserId;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/request/transition-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransitionMultiple([FromForm] int[] ids, [FromForm] int workflowId, [FromForm] int statusId)
        {
            if (ids == null || ids.Length == 0)
                return BadRequest();

            var status = await _context.Status.FindAsync(statusId);
            var requests = _context.Requests.Where(r => ids.Contains(r.RequestID));
            foreach (var req in requests)
            {
                req.WorkflowID = workflowId;
                req.StatusID = statusId;
                req.UpdatedAt = DateTime.UtcNow;
                
                // Nếu trạng thái là Closed thì cập nhật ClosedAt
                if (status?.StatusName == "Closed")
                {
                    req.ClosedAt = DateTime.UtcNow;
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/request/approve-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("No requests selected");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var requestId in ids)
            {
                var request = await _context.Requests
                    .Include(r => r.Workflow)
                    .ThenInclude(w => w.Steps)
                    .FirstOrDefaultAsync(r => r.RequestID == requestId);

                if (request == null)
                    continue;

                var currentStep = request.Workflow?.Steps
                    .FirstOrDefault(s => s.StepOrder == request.CurrentStepOrder);

                if (currentStep == null)
                    continue;

                // Check if user has the required role for this step
                if (!userRoles.Contains(currentStep.RequiredRoleId))
                    continue;

                // Process the approval
                await _workflowStepService.ProcessStep(
                    requestId,
                    currentStep.StepID,
                    userId,
                    "approve",
                    "Approved via bulk action"
                );
            }

            return Ok(new { message = "Requests processed successfully" });
        }

        [HttpPost("/request/add-comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromForm] int requestId, [FromForm] string comment)
        {
            try
            {
                if (string.IsNullOrEmpty(comment))
                    return BadRequest("Comment cannot be empty");

                var request = await _context.Requests.FindAsync(requestId);
                if (request == null)
                    return NotFound("Request not found");

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var newComment = new Comment
                {
                    RequestId = requestId,
                    UserId = currentUser.Id,
                    Content = comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    userName = currentUser.UserName,
                    comment = comment,
                    createdAt = newComment.CreatedAt.ToString("MMM dd, yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToNextStep(int requestId, string note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = _workflowService.MoveToNextStep(requestId, userId, note);
                if (result)
                {
                    // Lấy request sau khi đã cập nhật
                    var request = await _context.Requests
                        .Include(r => r.Status)
                        .Include(r => r.Workflow)
                            .ThenInclude(w => w.Steps)
                        .FirstOrDefaultAsync(r => r.RequestID == requestId);

                    if (request != null)
                    {
                        // Nếu request đang ở trạng thái In Progress
                        if (request.Status?.StatusName == "In Progress")
                        {
                            var currentStep = request.Workflow?.Steps
                                .FirstOrDefault(s => s.StepOrder == request.CurrentStepOrder);

                            if (currentStep != null)
                            {
                                // Tạo thông báo cho người được chỉ định xử lý
                                if (!string.IsNullOrEmpty(currentStep.AssignedUserId))
                                {
                                    var notification = new Notification
                                    {
                                        Title = "Yêu cầu mới cần xử lý",
                                        Content = $"Yêu cầu #{request.RequestID} - {request.Title} đang ở bước {currentStep.StepName}",
                                        CreatedAt = DateTime.UtcNow,
                                        Type = "in_progress",
                                        RequestId = request.RequestID,
                                        UserId = currentStep.AssignedUserId,
                                        IsRead = false
                                    };
                                    _context.Notifications.Add(notification);
                                }

                                // Tạo thông báo cho người có role phù hợp
                                if (!string.IsNullOrEmpty(currentStep.RequiredRoleId))
                                {
                                    var usersWithRole = await _userManager.GetUsersInRoleAsync(currentStep.RequiredRoleId);
                                    foreach (var user in usersWithRole)
                                    {
                                        var notification = new Notification
                                        {
                                            Title = "Yêu cầu mới cần xử lý",
                                            Content = $"Yêu cầu #{request.RequestID} - {request.Title} đang ở bước {currentStep.StepName}",
                                            CreatedAt = DateTime.UtcNow,
                                            Type = "in_progress",
                                            RequestId = request.RequestID,
                                            UserId = user.Id,
                                            IsRead = false
                                        };
                                        _context.Notifications.Add(notification);
                                    }
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    TempData["StatusMessage"] = "Chuyển bước thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Chuyển bước thất bại!";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện bước này!";
            }
            return RedirectToAction("Details", new { id = requestId });
        }

        [HttpGet("/request/my-tasks")]
        public async Task<IActionResult> MyTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            System.Diagnostics.Debug.WriteLine($"[MyTasks] UserId: {userId}, Roles: {string.Join(",", userRoles)}");

            // Lấy tất cả các yêu cầu mà user là người gửi hoặc có vai trò phụ trách
            var allRequests = await _context.Requests
                .Include(r => r.Workflow)
                    .ThenInclude(w => w.Steps)
                .Include(r => r.Status)
                .Include(r => r.User)
                .Include(r => r.Priority)
                .Include(r => r.AssignedUser)
                .Where(r => 
                    // User là người gửi yêu cầu
                    r.UsersId == userId ||
                    // Hoặc user có vai trò phụ trách trong bước hiện tại của workflow
                    r.Workflow.Steps.Any(s =>
                        s.StepOrder == r.CurrentStepOrder &&
                        (
                            (!string.IsNullOrEmpty(s.AssignedUserId) && s.AssignedUserId == userId) ||
                            (!string.IsNullOrEmpty(s.RequiredRoleId) && userRoles.Contains(s.RequiredRoleId))
                        )
                    )
                )
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Log từng request và step hiện tại
            foreach (var req in allRequests)
            {
                var step = req.Workflow?.Steps?.FirstOrDefault(s => s.StepOrder == req.CurrentStepOrder);
                System.Diagnostics.Debug.WriteLine($"[MyTasks] RequestID: {req.RequestID}, CurrentStepOrder: {req.CurrentStepOrder}, Step.AssignedUserId: {step?.AssignedUserId}, Step.RequiredRoleId: {step?.RequiredRoleId}");
                var totalSteps = req.Workflow?.Steps?.Count ?? 0;
                req.RemainingSteps = totalSteps - req.CurrentStepOrder;
                var nextStep = req.Workflow?.Steps
                    .OrderBy(s => s.StepOrder)
                    .FirstOrDefault(s => s.StepOrder > req.CurrentStepOrder);
                if (nextStep != null)
                {
                    req.NextAssignee = nextStep.AssignedUserId;
                    req.NextStepName = nextStep.StepName;
                }
            }

            // Lấy các request mà user đã approve ở bất kỳ bước nào
            var approvedRequestIds = await _context.RequestApprovals
                .Where(ra => ra.ApprovedByUserId == userId)
                .Select(ra => ra.RequestId)
                .ToListAsync();

            var viewModel = new MyTasksViewModel
            {
                // Các yêu cầu cần xử lý (user có vai trò phụ trách và chưa closed)
                RequestsToProcess = allRequests.Where(r => 
                {
                    var currentStep = r.Workflow?.Steps?.FirstOrDefault(s => s.StepOrder == r.CurrentStepOrder);
                    
                    System.Diagnostics.Debug.WriteLine($"[MyTasks Debug] RequestID: {r.RequestID}");
                    System.Diagnostics.Debug.WriteLine($"[MyTasks Debug] CurrentStepOrder: {r.CurrentStepOrder}");
                    System.Diagnostics.Debug.WriteLine($"[MyTasks Debug] Step.AssignedUserId: {currentStep?.AssignedUserId}");
                    System.Diagnostics.Debug.WriteLine($"[MyTasks Debug] UserId: {userId}");
                    System.Diagnostics.Debug.WriteLine($"[MyTasks Debug] Status: {r.Status?.StatusName}");

                    // Kiểm tra xem user có phải là người được chỉ định phụ trách bước hiện tại không
                    // VÀ request chưa ở trạng thái closed
                    var hasPermission = currentStep?.AssignedUserId == userId && 
                                      r.Status?.StatusName != "Closed" &&
                                      r.Status?.StatusName != "Completed" &&
                                      r.Status?.StatusName != "Rejected";

                    System.Diagnostics.Debug.WriteLine($"[MyTasks Debug] HasPermission: {hasPermission}");
                    
                    return hasPermission;
                }).ToList(),

                // Các yêu cầu đã gửi và đang ở bước đầu tiên hoặc cần bổ sung thông tin
                MySubmittedRequests = allRequests.Where(r =>
                    r.UsersId == userId && 
                    (r.CurrentStepOrder == 1 || r.Status?.StatusName == "Need More Info")).ToList(),

                // Các yêu cầu đã gửi và đang ở các bước tiếp theo (không bao gồm Need More Info)
                RequestsWaitingForOthers = allRequests.Where(r =>
                    r.UsersId == userId && 
                    r.CurrentStepOrder > 1 && 
                    r.Status?.StatusName != "Need More Info").ToList(),

                // Các yêu cầu đã hoàn thành, phê duyệt hoặc từ chối
                DoneRequests = allRequests.Where(r => 
                    approvedRequestIds.Contains(r.RequestID) ||
                    (r.Status?.StatusName == "Closed" ||
                     r.Status?.StatusName == "Completed" ||
                     r.Status?.StatusName == "Rejected" ||
                     r.Status?.StatusName == "Approved")
                ).ToList()
            };

            return View(viewModel);
        }

        [HttpPost("/request/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest([FromBody] ApproveRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var request = await _context.Requests
                    .Include(r => r.Workflow)
                        .ThenInclude(w => w.Steps)
                    .Include(r => r.Status)
                    .FirstOrDefaultAsync(r => r.RequestID == model.RequestId);

                if (request == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy yêu cầu!" });
                }

                // Kiểm tra xem người dùng có quyền approve không
                var currentStep = request.Workflow?.Steps
                    .FirstOrDefault(s => s.StepOrder == request.CurrentStepOrder);

                if (currentStep == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy bước hiện tại!" });
                }

                // Kiểm tra quyền approve
                var currentUser = await _userManager.GetUserAsync(User);
                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                if (!string.IsNullOrEmpty(currentStep.RequiredRoleId) && 
                    !currentUserRoles.Contains(currentStep.RequiredRoleId) &&
                    currentStep.AssignedUserId != userId)
                {
                    return StatusCode(403, new { success = false, message = "Bạn không có quyền approve yêu cầu này!" });
                }

                // Lấy bước tiếp theo
                var nextStep = request.Workflow.Steps
                    .FirstOrDefault(s => s.StepOrder == currentStep.StepOrder + 1);

                if (nextStep == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy bước tiếp theo!" });
                }

                // Cập nhật trạng thái và bước
                request.CurrentStepOrder = nextStep.StepOrder;
                request.StatusID = nextStep.StatusID;
                request.AssignedUserId = nextStep.AssignedUserId;
                request.UpdatedAt = DateTime.UtcNow;

                // Lưu lịch sử
                _context.RequestStepHistories.Add(new RequestStepHistory
                {
                    RequestID = model.RequestId,
                    StepOrder = currentStep.StepOrder,
                    ActionByUserId = userId,
                    ActionTime = DateTime.UtcNow,
                    Note = model.Comment,
                    StatusID = request.StatusID ?? 0
                });

                // Thêm comment nếu có
                if (!string.IsNullOrEmpty(model.Comment))
                {
                    _context.Comments.Add(new Comment
                    {
                        RequestId = model.RequestId,
                        UserId = userId,
                        Content = model.Comment,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Thêm phản hồi cho khách hàng nếu có
                if (!string.IsNullOrEmpty(model.CustomerResponse))
                {
                    _context.Comments.Add(new Comment
                    {
                        RequestId = model.RequestId,
                        UserId = userId,
                        Content = $"Phản hồi cho khách hàng: {model.CustomerResponse}",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Cập nhật thông tin bổ sung
                request.Resolution = model.Resolution;
                request.LinkedIssues = model.LinkedIssues;
                request.IssueType = model.Issue;

                // Lưu thông tin phê duyệt
                _context.RequestApprovals.Add(new RequestApproval
                {
                    RequestId = request.RequestID,
                    ApprovedByUserId = userId,
                    ApprovedAt = DateTime.UtcNow,
                    Note = model.Comment
                });

                await _context.SaveChangesAsync();

                // Tạo thông báo cho người phê duyệt
                var approvalNotification = new Notification
                {
                    Title = "Yêu cầu đã được phê duyệt",
                    Content = $"Bạn đã phê duyệt yêu cầu #{request.RequestID} - {request.Title}",
                    CreatedAt = DateTime.UtcNow,
                    Type = "approved",
                    RequestId = request.RequestID,
                    UserId = userId,
                    IsRead = false
                };
                _context.Notifications.Add(approvalNotification);

                // Tạo thông báo cho người xử lý bước tiếp theo
                if (!string.IsNullOrEmpty(nextStep.AssignedUserId))
                {
                    var nextStepNotification = new Notification
                    {
                        Title = "Yêu cầu mới cần xử lý",
                        Content = $"Yêu cầu #{request.RequestID} - {request.Title} đã được phê duyệt và đang ở bước {nextStep.StepName}",
                        CreatedAt = DateTime.UtcNow,
                        Type = "in_progress",
                        RequestId = request.RequestID,
                        UserId = nextStep.AssignedUserId,
                        IsRead = false
                    };
                    _context.Notifications.Add(nextStepNotification);
                }

                // Tạo thông báo cho người có role phù hợp với bước tiếp theo
                if (!string.IsNullOrEmpty(nextStep.RequiredRoleId))
                {
                    var allUsers = await _userManager.Users.ToListAsync();
                    foreach (var nextUser in allUsers)
                    {
                        var nextUserRoles = await _userManager.GetRolesAsync(nextUser);
                        if (nextUserRoles.Contains(nextStep.RequiredRoleId))
                        {
                            await _notificationService.CreateNotificationAsync(
                                "Yêu cầu mới cần xử lý",
                                $"Yêu cầu #{request.RequestID} - {request.Title} đang ở bước {nextStep.StepName}",
                                "in_progress",
                                nextUser.Id,
                                request.RequestID
                            );
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Yêu cầu đã được phê duyệt thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving request");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi phê duyệt yêu cầu" });
            }
        }

        [HttpPost("/request/reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject([FromBody] RejectRequestModel model)
        {
            try
            {
                var request = await _context.Requests
                    .Include(r => r.Workflow)
                        .ThenInclude(w => w.Steps)
                    .FirstOrDefaultAsync(r => r.RequestID == model.RequestId);

                if (request == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                if (model.Action == "requestInfo")
                {
                    // Cập nhật trạng thái yêu cầu bổ sung thông tin
                    request.IsAdditionalInfoRequested = true;
                    request.AdditionalInfoRequest = model.AdditionalInfo;
                    request.StatusID = _context.Status.FirstOrDefault(s => s.StatusName == "Need More Info")?.StatusID;

                    // Tạo comment
                    var comment = new Comment
                    {
                        RequestId = request.RequestID,
                        UserId = currentUser.Id,
                        Content = model.Comment,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Comments.Add(comment);

                    // Tạo thông báo
                    await _notificationService.CreateNotificationAsync(
                        "Yêu cầu bổ sung thông tin",
                        $"Yêu cầu #{request.RequestID} - {request.Title} cần bổ sung thông tin: {model.AdditionalInfo}",
                        "need_more_info",
                        request.UsersId,
                        request.RequestID
                    );
                }
                else
                {
                    // Cập nhật trạng thái từ chối
                    request.StatusID = _context.Status.FirstOrDefault(s => s.StatusName == "Rejected")?.StatusID;
                    request.ClosedAt = DateTime.UtcNow;

                    // Tạo comment
                    var comment = new Comment
                    {
                        RequestId = request.RequestID,
                        UserId = currentUser.Id,
                        Content = model.Comment,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Comments.Add(comment);

                    // Tạo thông báo
                    await _notificationService.CreateNotificationAsync(
                        "Yêu cầu bị từ chối",
                        $"Yêu cầu #{request.RequestID} - {request.Title} đã bị từ chối. Lý do: {model.Reason}",
                        "rejected",
                        request.UsersId,
                        request.RequestID
                    );
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting request");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý yêu cầu" });
            }
        }

        [HttpPost("/request/add-additional-info")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdditionalInfo([FromBody] AdditionalInfoModel model)
        {
            try
            {
                var request = await _context.Requests
                    .Include(r => r.Workflow)
                        .ThenInclude(w => w.Steps)
                    .FirstOrDefaultAsync(r => r.RequestID == model.RequestId);

                if (request == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Cập nhật thông tin bổ sung
                request.Description += $"\n\nThông tin bổ sung ({DateTime.UtcNow:dd/MM/yyyy HH:mm}):\n{model.Content}";
                request.IsAdditionalInfoRequested = false;
                request.AdditionalInfoRequest = null;
                request.StatusID = _context.Status.FirstOrDefault(s => s.StatusName == "In Progress")?.StatusID;

                // Tạo comment
                var comment = new Comment
                {
                    RequestId = request.RequestID,
                    UserId = currentUser.Id,
                    Content = model.Comment,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Comments.Add(comment);

                // Tạo thông báo
                await _notificationService.CreateNotificationAsync(
                    "Thông tin bổ sung đã được gửi",
                    $"Yêu cầu #{request.RequestID} - {request.Title} đã được bổ sung thông tin",
                    "info_added",
                    request.AssignedUserId,
                    request.RequestID
                );

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding additional info");
                return Json(new { success = false, message = "Có lỗi xảy ra khi bổ sung thông tin" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessStep([FromBody] ProcessStepRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentRequest = await _context.Requests
                .Include(r => r.Workflow)
                .ThenInclude(w => w.Steps)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.RequestID == request.RequestId);

            if (currentRequest == null)
                return NotFound("Request not found");

            var currentStep = currentRequest.Workflow?.Steps
                .FirstOrDefault(s => s.StepOrder == currentRequest.CurrentStepOrder);

            if (currentStep == null)
                return BadRequest("Current step not found");

            // Check if user has the required role for this step
            var currentUser = await _userManager.FindByIdAsync(userId);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            if (!currentUserRoles.Contains(currentStep.RequiredRoleId))
                return BadRequest("You don't have permission to process this step");

            // Process the step
            var result = await _workflowStepService.ProcessStep(
                request.RequestId,
                currentStep.StepID,
                userId,
                request.Action,
                request.Note
            );

            if (!result)
                return BadRequest("Failed to process step");

            // Lấy request sau khi đã cập nhật
            var requestAfterProcessing = await _context.Requests
                .Include(r => r.Workflow)
                .ThenInclude(w => w.Steps)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.RequestID == request.RequestId);

            if (requestAfterProcessing != null)
            {
                var nextStep = requestAfterProcessing.Workflow?.Steps
                    .FirstOrDefault(s => s.StepOrder == requestAfterProcessing.CurrentStepOrder);

                if (nextStep != null)
                {
                    // Tạo thông báo cho người được chỉ định xử lý bước tiếp theo
                    if (!string.IsNullOrEmpty(nextStep.AssignedUserId))
                    {
                        var nextStepNotification = new Notification
                        {
                            Title = "Yêu cầu mới cần xử lý",
                            Content = $"Yêu cầu #{requestAfterProcessing.RequestID} - {requestAfterProcessing.Title} đang ở bước {nextStep.StepName}",
                            CreatedAt = DateTime.UtcNow,
                            Type = "in_progress",
                            RequestId = requestAfterProcessing.RequestID,
                            UserId = nextStep.AssignedUserId,
                            IsRead = false
                        };
                        _context.Notifications.Add(nextStepNotification);
                    }

                    // Tạo thông báo cho người có role phù hợp với bước tiếp theo
                    if (!string.IsNullOrEmpty(nextStep.RequiredRoleId))
                    {
                        var allUsers = await _userManager.Users.ToListAsync();
                        foreach (var nextUser in allUsers)
                        {
                            var nextUserRoles = await _userManager.GetRolesAsync(nextUser);
                            if (nextUserRoles.Contains(nextStep.RequiredRoleId))
                            {
                                var roleNotification = new Notification
                                {
                                    Title = "Yêu cầu mới cần xử lý",
                                    Content = $"Yêu cầu #{requestAfterProcessing.RequestID} - {requestAfterProcessing.Title} đang ở bước {nextStep.StepName}",
                                    CreatedAt = DateTime.UtcNow,
                                    Type = "in_progress",
                                    RequestId = requestAfterProcessing.RequestID,
                                    UserId = nextUser.Id,
                                    IsRead = false
                                };
                                _context.Notifications.Add(roleNotification);
                            }
                        }
                    }

                    // Lưu tất cả thông báo vào database
                    await _context.SaveChangesAsync();
                }
            }

            TempData["StatusMessage"] = "Chuyển bước thành công!";
            return Ok(new { message = "Step processed successfully" });
        }

        [HttpGet("/request/get-notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        content = n.Content,
                        time = n.CreatedAt,
                        type = n.Type,
                        requestId = n.RequestId,
                        read = n.IsRead
                    })
                    .ToListAsync();

                return Json(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("/request/mark-notification-as-read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead([FromBody] int notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                    return NotFound();

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("/request/mark-all-notifications-as-read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
