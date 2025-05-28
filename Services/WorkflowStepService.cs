using App.Models;
using App.Models.IRequest;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Request.Services
{
    public class WorkflowStepService : IWorkflowStepService
    {
        private readonly AppDbContext _context;
        private readonly RequestHistoryService _historyService;
        private readonly UserManager<AppUser> _userManager;

        public WorkflowStepService(AppDbContext context, RequestHistoryService historyService, UserManager<AppUser> userManager)
        {
            _context = context;
            _historyService = historyService;
            _userManager = userManager;
        }

        public async Task<bool> ProcessStep(int requestId, int stepId, string userId, string action, string note = null)
        {
            try
            {
                var request = await _context.Requests
                    .Include(r => r.Workflow)
                    .ThenInclude(w => w.Steps)
                    .FirstOrDefaultAsync(r => r.RequestID == requestId);

                if (request == null)
                    return false;

                var currentStep = request.Workflow?.Steps
                    .FirstOrDefault(s => s.StepID == stepId);

                if (currentStep == null)
                    return false;

                // Get the next step
                var nextStep = request.Workflow.Steps
                    .FirstOrDefault(s => s.StepOrder == currentStep.StepOrder + 1);

                // Update request status and step
                request.CurrentStepOrder = nextStep?.StepOrder ?? currentStep.StepOrder;
                request.StatusID = nextStep?.StatusID ?? currentStep.StatusID;
                request.UpdatedAt = DateTime.UtcNow;

                // If this is an approval action, record it
                if (action.ToLower() == "approve")
                {
                    request.ApprovedAt = DateTime.UtcNow;
                    _context.RequestApprovals.Add(new RequestApproval
                    {
                        RequestId = requestId,
                        ApprovedByUserId = userId,
                        ApprovedAt = DateTime.UtcNow,
                        Note = note
                    });
                }

                // Add step history
                _context.RequestStepHistories.Add(new RequestStepHistory
                {
                    RequestID = requestId,
                    StepOrder = currentStep.StepOrder,
                    ActionByUserId = userId,
                    ActionTime = DateTime.UtcNow,
                    Note = note,
                    StatusID = request.StatusID ?? 0
                });

                // Add comment if provided
                if (!string.IsNullOrEmpty(note))
                {
                    _context.Comments.Add(new Comment
                    {
                        RequestId = requestId,
                        UserId = userId,
                        Content = note,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidateStep(int requestId, int stepId, string userId)
        {
            var request = await _context.Requests
                .Include(r => r.Workflow)
                .ThenInclude(w => w.Steps)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null) return false;

            var step = request.Workflow.Steps.FirstOrDefault(s => s.StepID == stepId);
            if (step == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);

            // Kiểm tra nếu user là IT Staff và request đang ở bước 4
            var isITStaff = userRoles.Contains("ITStaff");
            var isStep4 = step.StepOrder == 4;
            
            return isITStaff && isStep4;
        }

        public async Task<WorkflowStep> GetNextStep(int requestId)
        {
            var request = await _context.Requests
                .Include(r => r.Workflow)
                .ThenInclude(w => w.Steps)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null) return null;

            return request.Workflow.Steps
                .OrderBy(s => s.StepOrder)
                .FirstOrDefault(s => s.StepOrder > request.CurrentStepOrder);
        }

        public async Task<bool> ProcessApproval(App.Models.IRequest.Request request, WorkflowStep step, string userId, string note)
        {
            try
            {
                // Update request status
                request.StatusID = step.StatusID;

                // Set ApprovedAt if this is an approval step
                if (step.ApprovalRequired)
                {
                    request.ApprovedAt = DateTime.UtcNow;
                }

                // Get next step
                var nextStep = await GetNextStep(request.RequestID);
                if (nextStep != null)
                {
                    request.CurrentStepOrder = nextStep.StepOrder;
                    request.AssignedUserId = nextStep.AssignedUserId;
                }
                else
                {
                    // Handle completion
                    var completedStatus = await _context.Status
                        .FirstOrDefaultAsync(s => s.StatusName == "Completed");
                    if (completedStatus != null)
                    {
                        request.StatusID = completedStatus.StatusID;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ProcessRejection(App.Models.IRequest.Request request, WorkflowStep step, string userId, string note)
        {
            try
            {
                // Update request status to rejected
                var rejectedStatus = await _context.Status
                    .FirstOrDefaultAsync(s => s.StatusName == "Rejected");
                if (rejectedStatus != null)
                {
                    request.StatusID = rejectedStatus.StatusID;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 