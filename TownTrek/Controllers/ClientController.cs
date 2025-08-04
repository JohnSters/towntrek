using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers
{
    public class ClientController : Controller
    {
        // Dashboard - Main overview page
        public IActionResult Dashboard()
        {
            return View();
        }

        // Business Management
        public IActionResult Businesses()
        {
            return View();
        }

        public IActionResult CreateBusiness()
        {
            return View();
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