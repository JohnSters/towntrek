using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;

namespace TownTrek.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard - Main overview page
        public IActionResult Dashboard()
        {
            return View();
        }

        // Business Management
        public IActionResult ManageBusinesses()
        {
            return View();
        }

        public async Task<IActionResult> AddBusiness()
        {
            // Load towns for dropdown
            ViewBag.Towns = await _context.Towns
                .Where(t => t.IsActive)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .Select(t => new { t.Id, t.Name, t.Province })
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public IActionResult AddBusiness(AddBusinessViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Process the form submission
                // Validate the model, save to database, etc.
                
                // For now, redirect back to dashboard on success
                TempData["SuccessMessage"] = "Business added successfully!";
                return RedirectToAction("Dashboard");
            }
            
            // If model is invalid, return to form with validation errors
            return View(model);
        }

        public IActionResult EditBusiness(int id)
        {
            ViewBag.BusinessId = id;
            return View();
        }

        // Profile Management
        public IActionResult Profile()
        {
            return View();
        }

        // Subscription & Billing
        public IActionResult Subscription()
        {
            return View();
        }

        public IActionResult Billing()
        {
            return View();
        }

        // Analytics & Reports
        public IActionResult Analytics()
        {
            return View();
        }

        // Support & Help
        public IActionResult Support()
        {
            return View();
        }

        public IActionResult Documentation()
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