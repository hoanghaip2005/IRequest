using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using WorkflowModel = App.Models.IRequest.Workflow;
using Microsoft.AspNetCore.Authorization;

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("priority/[action]")]
    public class WorkflowController : Controller
    {
        private readonly AppDbContext _context;

        public WorkflowController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/workflow/home")]
        public async Task<IActionResult> Index(string searchString)
        {
            var workflows = _context.Workflows.Include(w => w.Priority).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                workflows = workflows.Where(w => w.WorkflowName.Contains(searchString) ||
                                                  w.Description.Contains(searchString));
            }

            return View(await workflows.ToListAsync());
        }

        // GET: Workflow/Details/5
        [HttpGet("/workflow/details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflow = await _context.Workflows
                .Include(w => w.Priority)
                .FirstOrDefaultAsync(m => m.WorkflowID == id);
            if (workflow == null)
            {
                return NotFound();
            }

            return View(workflow);
        }

        // GET: Workflow/Create
        [HttpGet("/workflow/create")]
        [AllowAnonymous]
        public IActionResult Create()
        {
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "Description");
            return View();
        }

        [HttpPost("/workflow/create")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkflowID,WorkflowName,PriorityID,Description,IsActive")] WorkflowModel workflow)
        {
            if (ModelState.IsValid)
            {
                // Ghi log để kiểm tra giá trị PriorityID
                Console.WriteLine($"PriorityID: {workflow.PriorityID}");

                _context.Add(workflow);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Tạo mới trạng thái thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "Description", workflow.PriorityID);
            return View(workflow);
        }

        // GET: Workflow/Edit/5
        [HttpGet("/workflow/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflow = await _context.Workflows.FindAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "Description", workflow.PriorityID);
            return View(workflow);
        }

        [HttpPost("/workflow/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkflowID,WorkflowName,PriorityID,Description,IsActive")] WorkflowModel workflow)
        {
            if (id != workflow.WorkflowID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workflow);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Cập nhật trạng thái thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkflowExists(workflow.WorkflowID))
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
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "Description", workflow.PriorityID);
            return View(workflow);
        }

        // GET: Workflow/Delete/5
        [HttpGet("/workflow/delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflow = await _context.Workflows
                .Include(w => w.Priority)
                .FirstOrDefaultAsync(m => m.WorkflowID == id);
            if (workflow == null)
            {
                return NotFound();
            }
            return View(workflow);
        }

        // POST: Workflow/Delete/5
        [HttpPost("/workflow/delete/{id?}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workflow = await _context.Workflows.FindAsync(id);
            if (workflow != null)
            {
                _context.Workflows.Remove(workflow);
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Xóa trạng thái thành công.";
            return RedirectToAction(nameof(Index));
        }

        private bool WorkflowExists(int id)
        {
            return _context.Workflows.Any(e => e.WorkflowID == id);
        }
    }
}
