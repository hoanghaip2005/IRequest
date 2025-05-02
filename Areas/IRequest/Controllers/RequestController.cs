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

namespace Request.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("request/[action]")]
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RequestController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
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
            // if (ModelState.IsValid)
            // {
            //     var user = _userManager.GetUserId(User);  // Lấy thông tin người dùng đã đăng nhập
            //     string userId = user; // Gán UserId từ bảng Users

            //     // Gán UserId vào đối tượng request
            //     request.UsersId = userId;
            //     if (!User.IsInRole("Admin") && !User.IsInRole("Member"))
            //     {
            //         // Nếu không phải Admin hoặc Member, gán giá trị mặc định cho PriorityID, StatusID, và WorkflowID
            //         request.PriorityID = 1; // Gán ID mặc định (thay thế bằng giá trị hợp lý)
            //         request.StatusID = 1; // Gán ID mặc định (thay thế bằng giá trị hợp lý)
            //         request.WorkflowID = 1; // Gán ID mặc định (thay thế bằng giá trị hợp lý)
            //     }
            //     request.CreatedAt = DateTime.UtcNow;
            //     request.UpdatedAt = DateTime.UtcNow;
            //     _context.Add(request);
            //     await _context.SaveChangesAsync();
            //     TempData["StatusMessage"] = "Tạo mới trạng thái thành công.";
            //     return RedirectToAction("Index", "Home");
            // }
            // ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description", request.PriorityID);
            // ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName", request.StatusID);
            // ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName", request.WorkflowID);
            // return View(request);

            if (ModelState.IsValid)
            {
                // Lấy thông tin người dùng đã đăng nhập
                var user = await _userManager.GetUserAsync(User);  // Lấy thông tin người dùng đã đăng nhập
                string userId = user?.Id; // Lấy UserId từ bảng Users

                // Nếu người dùng không tồn tại (chưa đăng nhập), có thể redirect đến trang đăng nhập
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng. Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }

                // Gán UserId vào đối tượng Request
                request.UsersId = userId;

                // Kiểm tra vai trò của người dùng và gán giá trị mặc định cho PriorityID, StatusID, và WorkflowID nếu không phải Admin hoặc Member
                if (!User.IsInRole("Admin") && !User.IsInRole("Member"))
                {
                    // Gán ID mặc định cho các trường Priority, Status, Workflow
                    request.PriorityID = 1; // Gán ID mặc định (thay thế bằng giá trị hợp lý)
                    request.StatusID = 1;   // Gán ID mặc định (thay thế bằng giá trị hợp lý)
                    request.WorkflowID = 1; // Gán ID mặc định (thay thế bằng giá trị hợp lý)
                }

                // Cập nhật thời gian tạo và cập nhật
                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                // Thêm yêu cầu vào context và lưu vào cơ sở dữ liệu
                _context.Add(request);
                await _context.SaveChangesAsync();

                // Thông báo tạo mới thành công
                TempData["StatusMessage"] = "Tạo mới yêu cầu thành công.";
                return RedirectToAction("Index", "Home");
            }

            // Nếu Model không hợp lệ, truyền lại dữ liệu cho View
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
