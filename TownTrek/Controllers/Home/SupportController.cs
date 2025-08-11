using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers.Home
{
    [Authorize]
    [Route("Client/[action]")] // Maintain existing routes for backward compatibility
    public class SupportController(ILogger<SupportController> logger) : Controller
    {
        private readonly ILogger<SupportController> _logger = logger;

        // Backward-compat endpoints left intentionally if linked externally
        // Prefer new Client routes in Client/SupportController and Client/DocumentationController
    }
}