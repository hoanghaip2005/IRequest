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

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("request/[action]")]
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;

        public RequestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/request/home")]
        public async Task<IActionResult> Index(string searchString)
        {
            var requests = _context.Requests.Include(r => r.Priority).Include(r => r.Status).Include(r => r.Workflow).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                requests = requests.Where(r => r.Title.Contains(searchString) || r.Description.Contains(searchString));
            }

            return View(await requests.ToListAsync());
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
                .FirstOrDefaultAsync(m => m.RequestID == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }
       [TempData]
        public string? StatusMessage { set; get; }

        [HttpGet("/request/create")]
        [AllowAnonymous]
        public IActionResult Create()
        {
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description");
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName");
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName");
            return View();
        }

        [HttpPost("/request/create")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestID,Title,Description,AttachmentURL,IsApproved,StatusID,PriorityID,WorkflowID")] RequestModel request)
        {
            if (ModelState.IsValid)
            {
                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
                _context.Add(request);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Tạo mới trạng thái thành công.";
                return RedirectToAction("Index", "Home");
            }
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description", request.PriorityID);
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName", request.StatusID);
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", request.WorkflowID);
            return View(request);
        }

        [HttpGet("/request/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description", request.PriorityID);
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName", request.StatusID);
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", request.WorkflowID);
            return View(request);
        }

        [HttpPost("/request/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestID,Title,Description,AttachmentURL,IsApproved,StatusID,PriorityID,WorkflowID,CreatedAt")] RequestModel request)
        {
            if (id != request.RequestID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    request.UpdatedAt = DateTime.UtcNow;
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Cập nhật trạng thái thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestExists(request.RequestID))
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
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description", request.PriorityID);
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName", request.StatusID);
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", request.WorkflowID);
            return View(request);
        }

        private bool RequestExists(int requestID)
        {
            throw new NotImplementedException();
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
    }
}
