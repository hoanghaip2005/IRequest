using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommentModel = App.Models.IRequest.Comment;
using App.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.IRequest.Controllers
{
    [Area("IRequest")]
    [Route("comment/[action]")]
    public class CommentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;


        public CommentController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: IRequest/Comment/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments
                .Include(c => c.Request)
                .Include(c => c.User)
                .ToListAsync();

            return View(comments);
        }

        // GET: IRequest/Comment/Create?requestId=123
        [HttpGet]
        public IActionResult Create(int requestId)
        {
            var comment = new CommentModel { RequestId = requestId };
            ViewBag.RequestId = requestId;
            return View(comment);
        }

        // POST: IRequest/Comment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,RequestId")] CommentModel comment)
        {
            if (ModelState.IsValid)
            {
                comment.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                comment.CreatedAt = DateTime.UtcNow;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                // Sau khi thêm, chuyển về trang chi tiết request hoặc danh sách comment
                return RedirectToAction("Details", "Request", new { area = "IRequest", id = comment.RequestId });
            }
            ViewBag.RequestId = comment.RequestId;
            return View(comment);
        }

        // POST: IRequest/Comment/Edit/5
        [HttpPost("comment/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] CommentModel comment)
        {
            if (id != comment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Comments.Any(e => e.CommentId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return BadRequest();
        }

        // POST: IRequest/Comment/Delete/5
        [HttpPost("comment/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
