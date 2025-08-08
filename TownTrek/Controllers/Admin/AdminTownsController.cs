using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/towns")]
    public class AdminTownsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminTownsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /admin/towns
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var towns = await _context.Towns
                .Include(t => t.Businesses)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View("~/Views/Admin/Towns.cshtml", towns);
        }

        // GET /admin/towns/new
        [HttpGet("new")]
        public IActionResult New()
        {
            var model = new AddTownViewModel();
            return View("~/Views/Admin/AddTown.cshtml", model);
        }

        // POST /admin/towns
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingTown = await _context.Towns
                    .FirstOrDefaultAsync(t => t.Name == model.Name && t.Province == model.Province);

                if (existingTown != null)
                {
                    ModelState.AddModelError("", "A town with this name already exists in the selected province.");
                    return View("~/Views/Admin/AddTown.cshtml", model);
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
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/AddTown.cshtml", model);
        }

        // GET /admin/towns/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
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

            return View("~/Views/Admin/AddTown.cshtml", model);
        }

        // POST /admin/towns/{id}
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                var town = await _context.Towns.FindAsync(id);
                if (town == null)
                {
                    return NotFound();
                }

                var existingTown = await _context.Towns
                    .FirstOrDefaultAsync(t => t.Name == model.Name && t.Province == model.Province && t.Id != id);

                if (existingTown != null)
                {
                    ModelState.AddModelError("", "A town with this name already exists in the selected province.");
                    return View("~/Views/Admin/AddTown.cshtml", model);
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
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/AddTown.cshtml", model);
        }

        // POST /admin/towns/{id}/delete
        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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
                return RedirectToAction(nameof(Index));
            }

            _context.Towns.Remove(town);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Town '{town.Name}' has been deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}


