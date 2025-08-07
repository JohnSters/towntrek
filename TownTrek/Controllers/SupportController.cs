using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers
{
    [Authorize]
    [Route("Client/[action]")] // Maintain existing routes for backward compatibility
    public class SupportController(ILogger<SupportController> logger) : Controller
    {
        private readonly ILogger<SupportController> _logger = logger;

        // Support & Help
        public IActionResult Support()
        {
            return View("~/Views/Client/Support.cshtml");
        }

        public IActionResult Documentation()
        {
            return View("~/Views/Client/Documentation.cshtml");
        }
    }
}