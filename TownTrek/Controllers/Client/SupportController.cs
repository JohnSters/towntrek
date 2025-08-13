using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "ClientAccess")]
    [Route("Client/Support/[action]")]
    public class SupportController(ILogger<SupportController> logger) : Controller
    {
        private readonly ILogger<SupportController> _logger = logger;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}


