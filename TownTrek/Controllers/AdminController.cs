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
        // Settings
        public IActionResult Settings()
        {
            return View();
        }

    }
}