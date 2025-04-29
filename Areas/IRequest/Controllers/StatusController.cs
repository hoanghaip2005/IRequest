using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using StatusModel = App.Models.IRequest.Status;
using Microsoft.AspNetCore.Authorization;
using Request.Migrations;

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("status/[action]")]
    public class StatusController : Controller
    {
        private readonly AppDbContext _context;

        public StatusController(AppDbContext context)
        {
            _context = context;
            StatusMessage = string.Empty;
        }

        [HttpGet("/status/home")]
        public async Task<IActionResult> Index(string searchString)
        {
            var statuses = from s in _context.Status
                           select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                statuses = statuses.Where(s => s.StatusName.Contains(searchString) ||
                                                s.Description.Contains(searchString));
            }

            return View(await statuses.ToListAsync());
        }

        [HttpGet("/status/details/{id?}")]
        public async Task<IActionResult> DetailStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Status
                .FirstOrDefaultAsync(m => m.StatusID == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        [TempData]
        public string StatusMessage { set; get; }
        [HttpGet("/status/Create/")]
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("/status/Create/")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatusID,StatusName,Description,IsFinal")] StatusModel status)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    status.CreatedAt = DateTime.UtcNow;
                    _context.Add(status);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Tạo mới trạng thái thành công.";
                    return RedirectToAction("Index", "Status");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi lưu trạng thái: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.");
            }
            return View(status);
        }

        [HttpGet("/status/EditStatus/{id?}")]
        public async Task<IActionResult> EditStatus(int? id)
        {
            if (id == null)
            {
                return NotFound("Khong tin trang");
            }

            var status = await _context.Status.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }
            return View(status);
        }

        [HttpPost("/status/EditStatus/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, [Bind("StatusID,StatusName,Description,IsFinal")] StatusModel status)
        {
            var existingStatus = await _context.Status.FindAsync(id);
            if (existingStatus == null || id != status.StatusID)
            {
                return NotFound("Không tìm thấy trạng thái hoặc ID không khớp.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingStatus.StatusName = status.StatusName;
                    existingStatus.Description = status.Description;
                    existingStatus.IsFinal = status.IsFinal;

                    _context.Update(existingStatus);
                    await _context.SaveChangesAsync();

                    TempData["StatusMessage"] = "Cập nhật trạng thái thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusExists(status.StatusID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(status);
        }

        [HttpGet("/status/delete/{id?}")]
        public async Task<IActionResult> DeleteStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Status
                .FirstOrDefaultAsync(m => m.StatusID == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        [HttpPost("/status/delete/{id?}"), ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedStatus(int id)
        {
            var status = await _context.Status.FindAsync(id);
            if (status != null)
            {
                _context.Status.Remove(status);
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Xóa trạng thái thành công.";
            return RedirectToAction(nameof(Index));
        }

        private bool StatusExists(int id)
        {
            return _context.Status.Any(e => e.StatusID == id);
        }
    }
}
