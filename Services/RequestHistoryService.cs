using App.Models;
using App.Models.IRequest;
using Microsoft.EntityFrameworkCore;

namespace Request.Services
{
    public class RequestHistoryService
    {
        private readonly AppDbContext _context;

        public RequestHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateRequestHistory(int requestId, int stepId, string userId)
        {
            var requestHistory = new RequestHistory
            {
                RequestID = requestId,
                StepID = stepId,
                UserID = userId,
                StartTime = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.RequestHistories.Add(requestHistory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequestHistory(int requestId, int stepId, string status, string note = null)
        {
            var history = await _context.RequestHistories
                .Where(rh => rh.RequestID == requestId && rh.StepID == stepId && rh.EndTime == null)
                .FirstOrDefaultAsync();

            if (history != null)
            {
                history.EndTime = DateTime.UtcNow;
                history.Status = status;
                history.Note = note;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CheckOverdueRequests()
        {
            var currentTime = DateTime.UtcNow;
            var overdueRequests = await _context.RequestHistories
                .Include(rh => rh.WorkflowStep)
                .Where(rh => rh.EndTime == null && rh.Status == "Pending")
                .ToListAsync();

            foreach (var request in overdueRequests)
            {
                if (request.WorkflowStep?.TimeLimitHours != null)
                {
                    var timeLimit = request.WorkflowStep.TimeLimitHours.Value;
                    var processingTime = (currentTime - request.StartTime).TotalHours;

                    if (processingTime > timeLimit)
                    {
                        request.Status = "Overdue";
                        request.Note = $"Vượt quá thời gian xử lý ({timeLimit} giờ)";
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<RequestHistory>> GetRequestHistory(int requestId)
        {
            return await _context.RequestHistories
                .Include(rh => rh.WorkflowStep)
                .Include(rh => rh.User)
                .Where(rh => rh.RequestID == requestId)
                .OrderByDescending(rh => rh.StartTime)
                .ToListAsync();
        }

        public async Task<List<RequestHistory>> GetOverdueRequests()
        {
            return await _context.RequestHistories
                .Include(rh => rh.Request)
                .Include(rh => rh.WorkflowStep)
                .Include(rh => rh.User)
                .Where(rh => rh.Status == "Overdue")
                .OrderByDescending(rh => rh.StartTime)
                .ToListAsync();
        }
    }
} 