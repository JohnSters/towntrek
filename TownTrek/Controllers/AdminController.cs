using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
 

namespace TownTrek.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard - Main admin overview
        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalTowns = await _context.Towns.CountAsync(),
                TotalBusinesses = await _context.Businesses.CountAsync(),
                ActiveBusinesses = await _context.Businesses.CountAsync(b => b.Status == "Active"),
                PendingApprovals = await _context.Businesses.CountAsync(b => b.Status == "Pending"),
                TotalPopulation = await _context.Towns.Where(t => t.Population.HasValue).SumAsync(t => t.Population!.Value),
                TownsWithLandmarks = await _context.Towns.CountAsync(t => !string.IsNullOrEmpty(t.Landmarks))
            };

            return View(stats);
        }

        // Towns Management
        public async Task<IActionResult> Towns()
        {
            var towns = await _context.Towns
                .Include(t => t.Businesses)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(towns);
        }

        public IActionResult AddTown()
        {
            var model = new AddTownViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTown(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if town already exists
                var existingTown = await _context.Towns
                    .FirstOrDefaultAsync(t => t.Name == model.Name && t.Province == model.Province);

                if (existingTown != null)
                {
                    ModelState.AddModelError("", "A town with this name already exists in the selected province.");
                    return View(model);
                }

                var town = new Town
                {
                    Name = model.Name,
                    Province = model.Province,
                    PostalCode = model.PostalCode,
                    Description = model.Description,
                    Population = model.Population,
                    Landmarks = model.Landmarks,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Towns.Add(town);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Town '{model.Name}' has been added successfully!";
                return RedirectToAction(nameof(Towns));
            }

            return View(model);
        }

        public async Task<IActionResult> EditTown(int id)
        {
            var town = await _context.Towns.FindAsync(id);
            if (town == null)
            {
                return NotFound();
            }

            var model = new AddTownViewModel
            {
                Id = town.Id,
                Name = town.Name,
                Province = town.Province,
                PostalCode = town.PostalCode,
                Description = town.Description,
                Population = town.Population,
                Landmarks = town.Landmarks,
                Latitude = town.Latitude,
                Longitude = town.Longitude
            };

            return View("AddTown", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTown(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                var town = await _context.Towns.FindAsync(model.Id);
                if (town == null)
                {
                    return NotFound();
                }

                // Check if another town with same name/province exists
                var existingTown = await _context.Towns
                    .FirstOrDefaultAsync(t => t.Name == model.Name && t.Province == model.Province && t.Id != model.Id);

                if (existingTown != null)
                {
                    ModelState.AddModelError("", "A town with this name already exists in the selected province.");
                    return View("AddTown", model);
                }

                town.Name = model.Name;
                town.Province = model.Province;
                town.PostalCode = model.PostalCode;
                town.Description = model.Description;
                town.Population = model.Population;
                town.Landmarks = model.Landmarks;
                town.Latitude = model.Latitude;
                town.Longitude = model.Longitude;
                town.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Town '{model.Name}' has been updated successfully!";
                return RedirectToAction(nameof(Towns));
            }

            return View("AddTown", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTown(int id)
        {
            var town = await _context.Towns
                .Include(t => t.Businesses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (town == null)
            {
                return NotFound();
            }

            if (town.Businesses.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{town.Name}' because it has associated businesses.";
                return RedirectToAction(nameof(Towns));
            }

            _context.Towns.Remove(town);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Town '{town.Name}' has been deleted successfully!";
            return RedirectToAction(nameof(Towns));
        }

        // Business Management (review/approve)
        public async Task<IActionResult> Businesses()
        {
            var businesses = await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Businesses/Index.cshtml", businesses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBusiness(int id)
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
            return RedirectToAction(nameof(Businesses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBusiness(int id)
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
            return RedirectToAction(nameof(Businesses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendBusiness(int id)
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
            return RedirectToAction(nameof(Businesses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBusiness(int id)
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
            return RedirectToAction(nameof(Businesses));
        }

        // User Management (placeholder)
        public IActionResult Users()
        {
            return View();
        }

        // Settings
        public IActionResult Settings()
        {
            return View();
        }

    }
}