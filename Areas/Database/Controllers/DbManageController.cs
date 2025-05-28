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

            // Tạo tài khoản nhân viên IT
            var userITStaff = await _userManager.FindByNameAsync("itstaff");
            if (userITStaff == null)
            {
                userITStaff = new AppUser()
                {
                    UserName = "itstaff",
                    Email = "itstaff@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userITStaff, "itstaff123");
                await _userManager.AddToRoleAsync(userITStaff, RoleName.ITStaff);
            }

            // Tạo tài khoản trưởng phòng IT
            var userITManager = await _userManager.FindByNameAsync("itmanager");
            if (userITManager == null)
            {
                userITManager = new AppUser()
                {
                    UserName = "itmanager",
                    Email = "itmanager@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userITManager, "itmanager123");
                await _userManager.AddToRoleAsync(userITManager, RoleName.ITManager);
            }

            // Tạo tài khoản trưởng phòng ban
            var userDepartmentHead = await _userManager.FindByNameAsync("depthead");
            if (userDepartmentHead == null)
            {
                userDepartmentHead = new AppUser()
                {
                    UserName = "depthead",
                    Email = "depthead@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userDepartmentHead, "depthead123");
                await _userManager.AddToRoleAsync(userDepartmentHead, RoleName.DepartmentHead);
            }

            // Tạo tài khoản nhân viên tài chính
            var userFinanceStaff = await _userManager.FindByNameAsync("financestaff");
            if (userFinanceStaff == null)
            {
                userFinanceStaff = new AppUser()
                {
                    UserName = "financestaff",
                    Email = "financestaff@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userFinanceStaff, "financestaff123");
                await _userManager.AddToRoleAsync(userFinanceStaff, RoleName.FinanceStaff);
            }

            // Tạo tài khoản trưởng phòng tài chính
            var userFinanceManager = await _userManager.FindByNameAsync("financemanager");
            if (userFinanceManager == null)
            {
                userFinanceManager = new AppUser()
                {
                    UserName = "financemanager",
                    Email = "financemanager@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userFinanceManager, "financemanager123");
                await _userManager.AddToRoleAsync(userFinanceManager, RoleName.FinanceManager);
            }

            // Tạo tài khoản nhân viên nhân sự
            var userHRStaff = await _userManager.FindByNameAsync("hrstaff");
            if (userHRStaff == null)
            {
                userHRStaff = new AppUser()
                {
                    UserName = "hrstaff",
                    Email = "hrstaff@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userHRStaff, "hrstaff123");
                await _userManager.AddToRoleAsync(userHRStaff, RoleName.HRStaff);
            }

            // Tạo tài khoản trưởng phòng nhân sự
            var userHRManager = await _userManager.FindByNameAsync("hrmanager");
            if (userHRManager == null)
            {
                userHRManager = new AppUser()
                {
                    UserName = "hrmanager",
                    Email = "hrmanager@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userHRManager, "hrmanager123");
                await _userManager.AddToRoleAsync(userHRManager, RoleName.HRManager);
            }

            // Tạo tài khoản nhân viên mua sắm
            var userProcurementStaff = await _userManager.FindByNameAsync("procurementstaff");
            if (userProcurementStaff == null)
            {
                userProcurementStaff = new AppUser()
                {
                    UserName = "procurementstaff",
                    Email = "procurementstaff@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userProcurementStaff, "procurementstaff123");
                await _userManager.AddToRoleAsync(userProcurementStaff, RoleName.ProcurementStaff);
            }

            // Tạo tài khoản trưởng phòng mua sắm
            var userProcurementManager = await _userManager.FindByNameAsync("procurementmanager");
            if (userProcurementManager == null)
            {
                userProcurementManager = new AppUser()
                {
                    UserName = "procurementmanager",
                    Email = "procurementmanager@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(userProcurementManager, "procurementmanager123");
                await _userManager.AddToRoleAsync(userProcurementManager, RoleName.ProcurementManager);
            }

            StatusMessage = "Đã tạo xong các tài khoản mẫu";
            return RedirectToAction(nameof(Index));
        }
    }
}
