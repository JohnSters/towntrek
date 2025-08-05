using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;

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
                ActiveBusinesses = await _context.Businesses.CountAsync(b => b.IsActive),
                PendingApprovals = await _context.Businesses.CountAsync(b => !b.IsApproved),
                TotalPopulation = await _context.Towns.Where(t => t.Population.HasValue).SumAsync(t => t.Population!.Value),
                TownsWithLandmarks = await _context.Towns.CountAsync(t => !string.IsNullOrEmpty(t.Landmarks))
            };

            return View(stats);
        }

        // Towns Management
        public async Task<IActionResult> Towns()
        {
            var towns = await _context.Towns
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

        // Business Management
        public async Task<IActionResult> Businesses()
        {
            var businesses = await _context.Businesses
                .Include(b => b.Town)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(businesses);
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