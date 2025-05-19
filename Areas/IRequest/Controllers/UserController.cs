// Controller: UserController
// using System.Data.Entity.Infrastructure;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/user/edit/{id?}")]
    public async Task<IActionResult> EditUserDepartment(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Lấy danh sách phòng ban cho dropdown
        ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", user.DepartmentID);
        
        return View(user);
    }

    [HttpPost("/user/edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUserDepartment(string id, [Bind("Id,UserName,Email,PhoneNumber,DepartmentID")] AppUser user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "User updated successfully.";
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
            return RedirectToAction(nameof(Index));
        }
        
        // Reload departments for dropdown
        ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", user.DepartmentID);
        
        return View(user);
    }
}
