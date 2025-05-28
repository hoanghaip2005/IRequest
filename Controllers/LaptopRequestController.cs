using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Request.Controllers
{
    public class LaptopRequestController : Controller
    {
        private readonly ILaptopRequestService _laptopRequestService;

        public LaptopRequestController(ILaptopRequestService laptopRequestService)
        {
            _laptopRequestService = laptopRequestService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateRequest(string employeeId, string employeeName, string department, 
            string reason, string deviceType, string deviceSpecs, int priorityId)
        {
            try
            {
                var requestId = _laptopRequestService.CreateRequest(employeeId, employeeName, department, 
                    reason, deviceType, deviceSpecs, priorityId);
                return Json(new { success = true, requestId = requestId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ApproveByManager(int requestId, string managerId, string managerName, 
            bool approve, string note = null)
        {
            try
            {
                _laptopRequestService.ApproveByManager(requestId, managerId, managerName, approve, note);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ApproveByIT(int requestId, string itManagerId, string itManagerName, 
            bool approve, string note = null)
        {
            try
            {
                _laptopRequestService.ApproveByIT(requestId, itManagerId, itManagerName, approve, note);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CheckInventory(int requestId, string inventoryStaffId, 
            bool available, string note = null)
        {
            try
            {
                _laptopRequestService.CheckInventory(requestId, inventoryStaffId, available, note);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ConfigureDevice(int requestId, string itStaffId, string configNote)
        {
            try
            {
                _laptopRequestService.ConfigureDevice(requestId, itStaffId, configNote);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult HandOver(int requestId, string itStaffId, string handoverNote)
        {
            try
            {
                _laptopRequestService.HandOver(requestId, itStaffId, handoverNote);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CompleteRequest(int requestId, string employeeConfirmNote)
        {
            try
            {
                _laptopRequestService.CompleteRequest(requestId, employeeConfirmNote);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult RejectRequest(int requestId, string rejectorId, 
            string rejectorName, string reason)
        {
            try
            {
                _laptopRequestService.RejectRequest(requestId, rejectorId, rejectorName, reason);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 