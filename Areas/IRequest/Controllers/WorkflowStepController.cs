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

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("workflowStep/[action]")]
    public class WorkflowStepController : Controller
    {
        private readonly AppDbContext _context;

        public WorkflowStepController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/workflowStep")]
        public async Task<IActionResult> Index(string searchString)
        {
            var workflowSteps = _context.WorkflowSteps.Include(w => w.statsus).Include(w => w.Workflow).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                workflowSteps = workflowSteps.Where(w => w.StepName.Contains(searchString) ||
                                                         w.Workflow.WorkflowName.Contains(searchString));
            }

            return View(await workflowSteps.ToListAsync());
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
        [AllowAnonymous]
        public IActionResult Create()
        {
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName");
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName");
            return View();
        }

        [HttpPost("/workflowStep/create")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StepID,StepName,WorkflowID,StepOrder,TimeLimitHours,ApprovalRequired,StatusID")] WorkflowStep workflowStep)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workflowStep);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Thêm mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
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

            var workflowStep = await _context.WorkflowSteps.FindAsync(id);
            if (workflowStep == null)
            {
                return NotFound();
            }
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
            return View(workflowStep);
        }


        [HttpPost("/workflowStep/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StepID,StepName,WorkflowID,StepOrder,TimeLimitHours,ApprovalRequired,StatusID")] WorkflowStep workflowStep)
        {
            if (id != workflowStep.StepID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workflowStep);
                    TempData["StatusMessage"] = "Cập nhật thành công!";
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
            ViewData["StatusID"] = new SelectList(_context.Status, "StatusID", "StatusName", workflowStep.StatusID);
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", workflowStep.WorkflowID);
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
    }
}
