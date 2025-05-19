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
        private readonly ILogger<RequestController> _logger;

        public RequestController(AppDbContext context, UserManager<AppUser> userManager, ILogger<RequestController> logger)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("/request/home")]
        public async Task<IActionResult> Index(string searchString)
        {
            var requests = _context.Requests
                                    .Include(r => r.AssignedUser)
                                        .ThenInclude(u => u.Department)
                                    .Include(r => r.Status)        // Bao gồm trạng thái
                                    .Include(r => r.Priority)      // Bao gồm độ ưu tiên
                                    .Include(r => r.User)          // Bao gồm người gửi yêu cầu
                                    .Include(r => r.Comments)      // Bao gồm comments
                                        .ThenInclude(c => c.User)  // Bao gồm thông tin người comment
                                    .AsQueryable();

            // Nếu không phải admin thì chỉ lấy request của user hiện tại
            if (!User.IsInRole("Administrator"))
            {
                var currentUserId = _userManager.GetUserId(User);
                requests = requests.Where(r => r.UsersId == currentUserId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                requests = requests.Where(r => r.Title.Contains(searchString) || r.Description.Contains(searchString));
            }

            // 
            ViewBag.AllUsers = _userManager.Users.ToList();
            ViewBag.AllWorkflows = _context.Workflows.ToList();
            ViewBag.AllStatuses = _context.Status.ToList();
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
            var userId = _userManager.GetUserId(User);  // Lấy UserId từ người dùng đã đăng nhập

            var request = new RequestModel
            {
                UsersId = userId  // Gán giá trị UserId từ người dùng đã đăng nhập
            };

            // Cập nhật giá trị AssignedUserId nếu bạn muốn gán mặc định một người nào đó cho việc tạo yêu cầu
            var defaultAssignedUserId = _userManager.Users.FirstOrDefault()?.Id;
            request.AssignedUserId = defaultAssignedUserId;

            // Truyền danh sách các mục lựa chọn vào ViewData để hiển thị trong dropdown
            ViewData["PriorityID"] = new SelectList(_context.Set<Priority>(), "PriorityID", "Description");
            ViewData["StatusID"] = new SelectList(_context.Set<Status>(), "StatusID", "StatusName");
            ViewData["WorkflowID"] = new SelectList(_context.Set<Workflow>(), "WorkflowID", "WorkflowName");

            // Lấy danh sách người dùng từ UserManager và gán vào ViewData["Users"]
            ViewData["Users"] = new SelectList(_userManager.Users.ToList(), "Id", "UserName", request.AssignedUserId);

            return View(request);
        }



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
        [HttpPost("/request/create")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestID,Title,Description,AttachmentURL,IsApproved,StatusID,PriorityID,WorkflowID,UsersId,AssignedUserId")] RequestModel request)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(request.UsersId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng. Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }

                // If AssignedUserId is empty, you may want to assign a default user (e.g., admin)
                if (string.IsNullOrEmpty(request.AssignedUserId))
                {
                    TempData["ErrorMessage"] = "Bạn phải chọn người phụ trách yêu cầu.";
                    return RedirectToAction("Create");
                }

                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                _context.Add(request);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = "Tạo mới yêu cầu thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
            return View(request);
        }

        [HttpGet("/request/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Request ID is null when trying to edit.");
                return NotFound(); // Ensure id is not null
            }

            var request = await _context.Requests
                .Include(r => r.User)  // Include the requester
                .Include(r => r.AssignedUser)  // Include the assigned user
                .FirstOrDefaultAsync(r => r.RequestID == id); // Fetch the request based on RequestID

            if (request == null)
            {
                _logger.LogWarning($"Request with ID {id} not found.");
                return NotFound(); // If no matching request is found
            }

            // Ensure ViewData["Users"] is populated with the list of users for the dropdown
            ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "PriorityName", request.PriorityID);


            return View(request); // Pass the request to the view
        }

        [HttpPost("/request/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestID,Title,Description,AttachmentURL,IsApproved,StatusID,PriorityID,WorkflowID,UsersId,AssignedUserId")] RequestModel request)
        {
            if (id != request.RequestID)
            {
                _logger.LogWarning($"Request ID mismatch: {id} != {request.RequestID}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Thêm log này để xem lỗi gì
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"ModelState error for {key}: {error.ErrorMessage}");
                    }
                }
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
                        _logger.LogWarning($"Request with ID {id} not found during update.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError($"Concurrency error while updating request with ID {id}");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }


            ViewData["Users"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName", request.AssignedUserId);
            ViewData["PriorityID"] = new SelectList(_context.Priorities, "PriorityID", "PriorityName", request.PriorityID);

            return View(request);
        }



        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.RequestID == id);
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

        [HttpPost("/request/delete-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple([FromForm] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "No items selected for deletion.";
                return RedirectToAction("Index");
            }

            var requests = _context.Requests.Where(r => ids.Contains(r.RequestID));
            _context.Requests.RemoveRange(requests);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Deleted selected requests successfully.";
            return RedirectToAction("Index");
        }
        [HttpPost("/request/assign-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMultiple([FromForm] int[] ids, [FromForm] string assignedUserId)
        {
            if (ids == null || ids.Length == 0 || string.IsNullOrEmpty(assignedUserId))
                return BadRequest();

            var requests = _context.Requests.Where(r => ids.Contains(r.RequestID));
            foreach (var req in requests)
                req.AssignedUserId = assignedUserId;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/request/transition-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransitionMultiple([FromForm] int[] ids, [FromForm] int workflowId, [FromForm] int statusId)
        {
            if (ids == null || ids.Length == 0)
                return BadRequest();

            var requests = _context.Requests.Where(r => ids.Contains(r.RequestID));
            foreach (var req in requests)
            {
                req.WorkflowID = workflowId;
                req.StatusID = statusId;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/request/approve-multiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveMultiple([FromForm] int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                    return BadRequest("No IDs provided.");

                var approvedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Approved");
                if (approvedStatus == null)
                    return BadRequest("Status 'Approved' not found.");

                var requests = _context.Requests.Where(r => ids.Contains(r.RequestID)).ToList();
                if (requests == null)
                    return BadRequest("Requests query returned null.");
                if (requests.Count == 0)
                    return BadRequest("No requests found for the given IDs.");

                foreach (var req in requests)
                {
                    if (req == null)
                        return BadRequest("A request in the list is null.");
                    req.StatusID = approvedStatus.StatusID;
                    req.UpdatedAt = DateTime.UtcNow;
                }

                approvedStatus.IsFinal = true;

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message + " - " + ex.StackTrace);
            }
        }

        [HttpPost("/request/add-comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromForm] int requestId, [FromForm] string comment)
        {
            try
            {
                if (string.IsNullOrEmpty(comment))
                    return BadRequest("Comment cannot be empty");

                var request = await _context.Requests.FindAsync(requestId);
                if (request == null)
                    return NotFound("Request not found");

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var newComment = new Comment
                {
                    RequestId = requestId,
                    UserId = currentUser.Id,
                    Content = comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    userName = currentUser.UserName,
                    comment = comment,
                    createdAt = newComment.CreatedAt.ToString("MMM dd, yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
