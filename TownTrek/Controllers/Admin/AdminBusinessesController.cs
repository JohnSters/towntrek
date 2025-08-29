using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminBusinessesController : Controller
    {
        private readonly IAdminBusinessService _adminBusinessService;

        public AdminBusinessesController(IAdminBusinessService adminBusinessService)
        {
            _adminBusinessService = adminBusinessService;
        }

        // GET: /AdminBusinesses/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var businesses = await _adminBusinessService.GetAllBusinessesForAdminAsync();
            return View(businesses);
        }

        // POST: /AdminBusinesses/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _adminBusinessService.ApproveBusinessAsync(id, User.Identity?.Name ?? "Unknown");
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Business has been approved and is now live!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminBusinesses/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _adminBusinessService.RejectBusinessAsync(id);
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Business has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminBusinesses/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id)
        {
            var result = await _adminBusinessService.SuspendBusinessAsync(id);
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Business has been suspended.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminBusinesses/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminBusinessService.DeleteBusinessAsync(id);
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Business has been deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}


