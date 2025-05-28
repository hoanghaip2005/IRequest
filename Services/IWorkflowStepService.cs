using App.Models.IRequest;
using System.Threading.Tasks;

namespace Request.Services
{
    public interface IWorkflowStepService
    {
        Task<bool> ProcessStep(int requestId, int stepId, string userId, string action, string note = null);
        Task<bool> ValidateStep(int requestId, int stepId, string userId);
        Task<WorkflowStep> GetNextStep(int requestId);
        Task<bool> ProcessApproval(App.Models.IRequest.Request request, WorkflowStep step, string userId, string note);
        Task<bool> ProcessRejection(App.Models.IRequest.Request request, WorkflowStep step, string userId, string note);
    }
} 