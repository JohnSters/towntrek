using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminTownsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminTownsController> _logger;

        public AdminTownsController(ApplicationDbContext context, ILogger<AdminTownsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Towns (List all towns)
        public async Task<IActionResult> Index()
        {
            var towns = await _context.Towns
                .Include(t => t.Businesses)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .ToListAsync();

            // Conventional view discovery: Views/Admin/Towns/Index.cshtml
            return View(towns);
        }

        // GET: Admin/Towns/Create (Show create form)
        public IActionResult Create()
        {
            var model = new AddTownViewModel();
            // Conventional view discovery: Views/Admin/Towns/Create.cshtml
            return View(model);
        }

        // POST: Admin/Towns/Create (Process create form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var town = new Town
                    {
                        Name = model.Name,
                        Province = model.Province,
                        PostalCode = model.PostalCode,
                        Population = model.Population,
                        Description = model.Description,
                        Landmarks = model.Landmarks,
                        Latitude = model.Latitude,
                        Longitude = model.Longitude,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Towns.Add(town);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Town '{model.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating town: {TownName}", model.Name);
                    TempData["ErrorMessage"] = "An error occurred while creating the town. Please try again.";
                }
            }

            return View(model);
        }

        // GET: Admin/Towns/Edit/5 (Show edit form)
        public async Task<IActionResult> Edit(int id)
        {
            var town = await _context.Towns.FindAsync(id);
            if (town == null)
            {
                TempData["ErrorMessage"] = "Town not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AddTownViewModel
            {
                Id = town.Id,
                Name = town.Name,
                Province = town.Province,
                PostalCode = town.PostalCode,
                Population = town.Population,
                Description = town.Description,
                Landmarks = town.Landmarks,
                Latitude = town.Latitude,
                Longitude = town.Longitude
            };

            // Conventional view discovery: Views/Admin/Towns/Edit.cshtml
            return View(model);
        }

        // POST: Admin/Towns/Edit/5 (Process edit form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var town = await _context.Towns.FindAsync(model.Id);
                    if (town == null)
                    {
                        TempData["ErrorMessage"] = "Town not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    town.Name = model.Name;
                    town.Province = model.Province;
                    town.PostalCode = model.PostalCode;
                    town.Population = model.Population;
                    town.Description = model.Description;
                    town.Landmarks = model.Landmarks;
                    town.Latitude = model.Latitude;
                    town.Longitude = model.Longitude;
                    town.UpdatedAt = DateTime.UtcNow;

                    _context.Towns.Update(town);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Town '{model.Name}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating town: {TownId}", model.Id);
                    TempData["ErrorMessage"] = "An error occurred while updating the town. Please try again.";
                }
            }

            return View(model);
        }

        // POST: Admin/Towns/Delete (Delete town)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var town = await _context.Towns
                    .Include(t => t.Businesses)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (town == null)
                {
                    TempData["ErrorMessage"] = "Town not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if town has businesses
                if (town.Businesses.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete town with existing businesses. Please remove all businesses first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Towns.Remove(town);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Town '{town.Name}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting town: {TownId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the town. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Towns/Details/5 (Optional - View town details)
        public async Task<IActionResult> Details(int id)
        {
            var town = await _context.Towns
                .Include(t => t.Businesses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (town == null)
            {
                TempData["ErrorMessage"] = "Town not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(town);
        }
    }
}