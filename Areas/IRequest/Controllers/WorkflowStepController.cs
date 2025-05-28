using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.IRequest;
using Microsoft.AspNetCore.Authorization;
using Request.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("workflow-step/[action]")]
    public class WorkflowStepController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWorkflowStepService _workflowStepService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<WorkflowStepController> _logger;

        public WorkflowStepController(
            AppDbContext context, 
            IWorkflowStepService workflowStepService, 
            UserManager<AppUser> userManager,
            ILogger<WorkflowStepController> logger)
        {
            _context = context;
            _workflowStepService = workflowStepService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("/workflowStep")]
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                _logger.LogInformation("Loading WorkflowSteps with AssignedUser data");
                
                var workflowSteps = _context.WorkflowSteps
                    .Include(w => w.statsus)
                    .Include(w => w.Workflow)
                    .Include(w => w.RequiredRole)
                    .Include(w => w.AssignedUser)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    workflowSteps = workflowSteps.Where(w => w.StepName.Contains(searchString) ||
                                                             w.Workflow.WorkflowName.Contains(searchString));
                }

                var result = await workflowSteps.ToListAsync();
                
                _logger.LogInformation($"Loaded {result.Count} WorkflowSteps");
                foreach (var step in result)
                {
                    _logger.LogInformation($"StepID: {step.StepID}, StepName: {step.StepName}, AssignedUser: {step.AssignedUser?.UserName ?? "Not Assigned"}");
                }

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading WorkflowSteps");
                throw;
            }
        }

        [HttpGet("/workflowStep/details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflowStep = await _context.WorkflowSteps
                .Include(w => w.statsus)
                .Include(w => w.Workflow)
                .Include(w => w.RequiredRole)
                .Include(w => w.AssignedUser)
                .FirstOrDefaultAsync(m => m.StepID == id);
            if (workflowStep == null)
            {
                return NotFound();
            }

            return View(workflowStep);
        }
        [TempData]
        public string StatusMessage { set; get; }

        [HttpGet("/workflowStep/create")]
        public IActionResult Create()
        {
            var workflows = _context.Workflows
                .Where(w => w.IsActive)
                .OrderBy(w => w.WorkflowName)
                .ToList();

            ViewData["WorkflowID"] = new SelectList(workflows, "WorkflowID", "WorkflowName");
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName");
            ViewData["RequiredRoleId"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["AssignedUserId"] = new SelectList(_userManager.Users, "Id", "UserName");
            return View();
        }

        [HttpPost("/workflowStep/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StepID,StepName,WorkflowID,StepOrder,RequiredRoleId,TimeLimitHours,ApprovalRequired,StatusID,AssignedUserId")] WorkflowStep workflowStep)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Workflow có tồn tại và đang active không
                var workflow = await _context.Workflows.FindAsync(workflowStep.WorkflowID);
                if (workflow == null || !workflow.IsActive)
                {
                    ModelState.AddModelError("WorkflowID", "Quy trình không tồn tại hoặc đã bị vô hiệu hóa");
                    var workflows = _context.Workflows
                        .Where(w => w.IsActive)
                        .OrderBy(w => w.WorkflowName)
                        .ToList();
                    ViewData["WorkflowID"] = new SelectList(workflows, "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
                    ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
                    ViewData["RequiredRoleId"] = new SelectList(_context.Roles, "Id", "Name", workflowStep.RequiredRoleId);
                    ViewData["AssignedUserId"] = new SelectList(_userManager.Users, "Id", "UserName", workflowStep.AssignedUserId);
                    return View(workflowStep);
                }

                _context.Add(workflowStep);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var activeWorkflows = _context.Workflows
                .Where(w => w.IsActive)
                .OrderBy(w => w.WorkflowName)
                .ToList();
            ViewData["WorkflowID"] = new SelectList(activeWorkflows, "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
            ViewData["RequiredRoleId"] = new SelectList(_context.Roles, "Id", "Name", workflowStep.RequiredRoleId);
            ViewData["AssignedUserId"] = new SelectList(_userManager.Users, "Id", "UserName", workflowStep.AssignedUserId);
            return View(workflowStep);
        }

        // GET: WorkflowStep/Edit/5
        [HttpGet("/workflowStep/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflowStep = await _context.WorkflowSteps
                .Include(w => w.AssignedUser)
                .FirstOrDefaultAsync(w => w.StepID == id);
            if (workflowStep == null)
            {
                return NotFound();
            }

            var activeWorkflows = _context.Workflows
                .Where(w => w.IsActive)
                .OrderBy(w => w.WorkflowName)
                .ToList();

            ViewData["WorkflowID"] = new SelectList(activeWorkflows, "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
            ViewData["RequiredRoleId"] = new SelectList(_context.Roles, "Id", "Name", workflowStep.RequiredRoleId);
            ViewData["AssignedUserId"] = new SelectList(_userManager.Users, "Id", "UserName", workflowStep.AssignedUserId);
            return View(workflowStep);
        }


        [HttpPost("/workflowStep/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StepID,StepName,WorkflowID,StepOrder,RequiredRoleId,TimeLimitHours,ApprovalRequired,StatusID,AssignedUserId")] WorkflowStep workflowStep)
        {
            if (id != workflowStep.StepID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem Workflow có tồn tại và đang active không
                    var workflow = await _context.Workflows.FindAsync(workflowStep.WorkflowID);
                    if (workflow == null || !workflow.IsActive)
                    {
                        ModelState.AddModelError("WorkflowID", "Quy trình không tồn tại hoặc đã bị vô hiệu hóa");
                        var activeWorkflows = _context.Workflows
                            .Where(w => w.IsActive)
                            .OrderBy(w => w.WorkflowName)
                            .ToList();
                        ViewData["WorkflowID"] = new SelectList(activeWorkflows, "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
                        ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
                        ViewData["RequiredRoleId"] = new SelectList(_context.Roles, "Id", "Name", workflowStep.RequiredRoleId);
                        ViewData["AssignedUserId"] = new SelectList(_userManager.Users, "Id", "UserName", workflowStep.AssignedUserId);
                        return View(workflowStep);
                    }

                    _context.Update(workflowStep);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkflowStepExists(workflowStep.StepID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var workflows = _context.Workflows
                .Where(w => w.IsActive)
                .OrderBy(w => w.WorkflowName)
                .ToList();
            ViewData["WorkflowID"] = new SelectList(workflows, "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
            ViewData["RequiredRoleId"] = new SelectList(_context.Roles, "Id", "Name", workflowStep.RequiredRoleId);
            ViewData["AssignedUserId"] = new SelectList(_userManager.Users, "Id", "UserName", workflowStep.AssignedUserId);
            return View(workflowStep);
        }

        // GET: WorkflowStep/Delete/5
        [HttpGet("/workflowStep/delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflowStep = await _context.WorkflowSteps
                .Include(w => w.statsus)
                .Include(w => w.Workflow)
                .Include(w => w.AssignedUser)
                .FirstOrDefaultAsync(m => m.StepID == id);
            if (workflowStep == null)
            {
                return NotFound();
            }

            return View(workflowStep);
        }

        // POST: WorkflowStep/Delete/5
        [HttpPost("/workflowStep/delete/{id?}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workflowStep = await _context.WorkflowSteps.FindAsync(id);
            if (workflowStep != null)
            {
                _context.WorkflowSteps.Remove(workflowStep);
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool WorkflowStepExists(int id)
        {
            return _context.WorkflowSteps.Any(e => e.StepID == id);
        }

        [HttpPost("/workflowStep/process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessStep([FromBody] ProcessStepRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _workflowStepService.ProcessStep(
                request.RequestId,
                request.StepId,
                userId,
                request.Action,
                request.Note
            );

            if (!result)
                return BadRequest("Không thể xử lý bước này");

            return Ok(new { message = "Xử lý thành công" });
        }
    }
}
