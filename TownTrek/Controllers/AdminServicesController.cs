using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;

namespace TownTrek.Controllers
{
    [Authorize]
    public class AdminServicesController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            var services = await _context.ServiceDefinitions
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();
            return View("~/Views/Admin/Services/Index.cshtml", services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string key, string name)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Key and Name are required.";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.ServiceDefinitions.AnyAsync(s => s.Key == key);
            if (exists)
            {
                TempData["ErrorMessage"] = "A service with this key already exists.";
                return RedirectToAction(nameof(Index));
            }

            _context.ServiceDefinitions.Add(new ServiceDefinition
            {
                Key = key.Trim(),
                Name = name.Trim(),
                IsActive = true
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Service created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var service = await _context.ServiceDefinitions.FindAsync(id);
            if (service != null)
            {
                service.IsActive = !service.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Service {(service.IsActive ? "activated" : "deactivated")}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


