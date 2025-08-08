using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TownTrek.Data;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/businesses")]
    public class AdminBusinessesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminBusinessesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /admin/businesses
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var businesses = await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Businesses/Index.cshtml", businesses);
        }

        // POST /admin/businesses/{id}/approve
        [HttpPost("{id}/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Active";
            business.ApprovedAt = DateTime.UtcNow;
            business.ApprovedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been approved and is now live!";
            return RedirectToAction(nameof(Index));
        }

        // POST /admin/businesses/{id}/reject
        [HttpPost("{id}/reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Inactive";
            business.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been rejected.";
            return RedirectToAction(nameof(Index));
        }

        // POST /admin/businesses/{id}/suspend
        [HttpPost("{id}/suspend")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Suspended";
            business.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been suspended.";
            return RedirectToAction(nameof(Index));
        }

        // POST /admin/businesses/{id}/delete
        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Deleted";
            business.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}


