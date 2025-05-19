using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;

namespace App.Area.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext dbContext , UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }
        [TempData]
        public string StatusMessage { get; set; }

        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xoa Database thanh cong" : "Khong xoa duoc";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "cap nhat database thanh cong";
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> SeedDataAsync()
        {
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename = (string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if (rfound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }
            }

            // Tạo tài khoản admin
            var useradmin = await _userManager.FindByNameAsync("admin");
            if (useradmin == null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(useradmin, "admin123");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
            }

            var userEditor = await _userManager.FindByNameAsync("Editor");
            if (userEditor == null)
            {
                userEditor = new AppUser()
                {
                    UserName = "editor",
                    Email = "editor@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userEditor, "admin123");
                await _userManager.AddToRoleAsync(userEditor, RoleName.Administrator);
            }

            // Tạo tài khoản nhân viên phê duyệt
            var userApprover = await _userManager.FindByNameAsync("approver");
            if (userApprover == null)
            {
                userApprover = new AppUser()
                {
                    UserName = "approver",
                    Email = "approver@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userApprover, "approver123");
                await _userManager.AddToRoleAsync(userApprover, RoleName.Approver);
            }

            // Tạo tài khoản trưởng phòng
            var userManager = await _userManager.FindByNameAsync("manager");
            if (userManager == null)
            {
                userManager = new AppUser()
                {
                    UserName = "manager",
                    Email = "manager@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userManager, "manager123");
                await _userManager.AddToRoleAsync(userManager, RoleName.Manager);
            }

            StatusMessage = "Đã tạo xong các tài khoản mẫu";
            return RedirectToAction(nameof(Index));
        }
    }
}
