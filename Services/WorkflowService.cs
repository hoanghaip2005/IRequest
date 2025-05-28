using App.Models;
using App.Models.IRequest;
using System;
using System.Linq;

namespace Services
{
    public class WorkflowService
    {
        private readonly AppDbContext _context;
        public WorkflowService(AppDbContext context) { _context = context; }

        public bool MoveToNextStep(int requestId, string currentUserId, string note = null)
        {
            var request = _context.Requests.Find(requestId);
            if (request == null) return false;

            var currentStep = _context.WorkflowSteps
                .FirstOrDefault(s => s.WorkflowID == request.WorkflowID && s.StepOrder == request.CurrentStepOrder);

            if (currentStep == null || currentStep.AssignedUserId != currentUserId)
                throw new UnauthorizedAccessException();

            // Lưu lịch sử
            _context.RequestStepHistories.Add(new RequestStepHistory
            {
                RequestID = requestId,
                StepOrder = currentStep.StepOrder,
                ActionByUserId = currentUserId,
                ActionTime = DateTime.UtcNow,
                Note = string.IsNullOrEmpty(note) ? "Chuyển bước" : note,
                StatusID = request.StatusID ?? 0
            });

            // Tìm bước tiếp theo
            var nextStep = _context.WorkflowSteps
                .FirstOrDefault(s => s.WorkflowID == request.WorkflowID && s.StepOrder == currentStep.StepOrder + 1);

            if (nextStep != null)
            {
                request.CurrentStepOrder = nextStep.StepOrder;
                request.StatusID = nextStep.StatusID;
                request.AssignedUserId = nextStep.AssignedUserId;
            }
            else
            {
                // Nếu là bước cuối
                var closedStatus = _context.Status.FirstOrDefault(s => s.StatusName == "Closed");
                request.StatusID = closedStatus?.StatusID ?? request.StatusID;
                request.AssignedUserId = null;
            }

            _context.SaveChanges();
            return true;
        }
    }
}
