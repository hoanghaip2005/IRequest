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

namespace Request.Areas_IRequest_Controllers_
{
    [Area("IRequest")]
    [Route("department/[action]")]
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/department/home")]
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            // Setting up sort parameters
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "Name" ? "Name_desc" : "Name";
            ViewData["CreatedAtSortParm"] = sortOrder == "CreatedAt" ? "CreatedAt_desc" : "CreatedAt";

            // Get the list of departments and apply filtering
            var departments = _context.Departments.AsQueryable();

            // Apply sorting logic based on sortOrder
            switch (sortOrder)
            {
                case "Name":
                    departments = departments.OrderBy(d => d.Name);  // Sort by Name Ascending
                    break;
                case "Name_desc":
                    departments = departments.OrderByDescending(d => d.Name);  // Sort by Name Descending
                    break;
                case "CreatedAt":
                    departments = departments.OrderBy(d => d.CreatedAt);  // Sort by CreatedAt Ascending
                    break;
                case "CreatedAt_desc":
                    departments = departments.OrderByDescending(d => d.CreatedAt);  // Sort by CreatedAt Descending
                    break;
                default:
                    departments = departments.OrderBy(d => d.Name);  // Default sorting by Name
                    break;
            }

            // Filter departments if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                departments = departments.Where(d => d.Name.Contains(searchString) || d.Description.Contains(searchString));
            }

            // Return sorted list to the view
            return View(await departments.ToListAsync());
        }



        [HttpGet("/department/details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                   .Include(d => d.Users) // Bao gồm danh sách người dùng trực thuộc phòng ban
                   .FirstOrDefaultAsync(d => d.DepartmentID == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpGet("/department/create")]
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("/department/create")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentID,Name,Description,CreatedAt,IsActive")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Tạo mới trạng thái thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        [HttpGet("/department/add-user/{departmentId}")]
        public IActionResult AddUser(int departmentId)
        {
            var department = _context.Departments
                                    .Include(d => d.Users)
                                    .FirstOrDefault(d => d.DepartmentID == departmentId);

            if (department == null)
            {
                return NotFound();
            }

            // Get the users who are not in any department
            var usersNotInDepartment = _context.Users
                                            .Where(u => u.DepartmentID == null)
                                            .ToList();

            // Pass the list of users to the view
            ViewData["UsersNotInDepartment"] = usersNotInDepartment;

            return View(department);
        }

        // Xử lý thêm người dùng vào phòng ban
        [HttpPost("/department/add-user/{departmentId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(int departmentId, string userId)
        {
            var department = await _context.Departments
                                           .Include(d => d.Users)
                                           .FirstOrDefaultAsync(d => d.DepartmentID == departmentId);

            if (department == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Gán người dùng vào phòng ban
            user.DepartmentID = departmentId;
            department.AssignedUserId = user.Id;
            _context.Update(department);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "User added to the department successfully.";
            return RedirectToAction("Details", new { id = departmentId });
        }

        [HttpPost("/department/remove-user/{departmentId}/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromDepartment(int departmentId, string userId)
        {
            var department = await _context.Departments
                                            .Include(d => d.Users)
                                            .FirstOrDefaultAsync(d => d.DepartmentID == departmentId);

            var user = await _context.Users.FindAsync(userId);

            if (department == null || user == null)
            {
                return NotFound();  // If either the department or user is not found
            }

            if (user.DepartmentID != departmentId)
            {
                return BadRequest("User is not in this department.");
            }

            user.DepartmentID = null; // Remove user from the department

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "User removed from the department successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(u => u.Id == userId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Details", "Department", new { id = departmentId });
        }



        [HttpGet("/department/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost("/department/edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentID,Name,Description,CreatedAt,IsActive")] Department department)
        {
            if (id != department.DepartmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Cập nhật trạng thái thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentID))
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
            return View(department);
        }

        [HttpGet("/department/edit-user/{userId}")]
        public async Task<IActionResult> EditUserDepartment(string userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách phòng ban cho dropdown
            ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", user.DepartmentID);

            return View(user);
        }

        [HttpPost("/department/edit-user/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserDepartment(string userId, [Bind("Id, UserName, Email, DepartmentID")] AppUser user)
        {
            if (userId != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "User's department updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(u => u.Id == user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirect to the Department's details page after updating
                return RedirectToAction("Details", "Department", new { id = user.DepartmentID });
            }

            // Reload departments for dropdown
            ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", user.DepartmentID);
            return View(user);
        }



        [HttpGet("/department/delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost("/department/delete/{id?}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Xóa trạng thái thành công.";
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentID == id);
        }



    }
}
