using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers
{
    [Authorize]
    [Route("Client/[action]")] // Maintain existing routes for backward compatibility
    public class AnalyticsController(
        IClientService clientService,
        ILogger<AnalyticsController> logger) : Controller
    {
        private readonly IClientService _clientService = clientService;
        private readonly ILogger<AnalyticsController> _logger = logger;

        // Analytics & Reports
        [RequireActiveSubscription(requiredFeature: "BasicAnalytics")]
        public async Task<IActionResult> Analytics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var analyticsModel = await _clientService.GetAnalyticsViewModelAsync(userId);

            return View("~/Views/Client/Analytics/Index.cshtml", analyticsModel);
        }
    }
}