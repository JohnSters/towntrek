using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Attributes;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Analytics
{
    [Authorize]
    [Route("Client/[controller]/[action]")]
    public class AnalyticsController(
        IClientService clientService,
        ILogger<AnalyticsController> logger) : Controller
    {
        private readonly IClientService _clientService = clientService;
        private readonly ILogger<AnalyticsController> _logger = logger;

        // Analytics & Reports
        [RequireActiveSubscription(requiredFeature: "BasicAnalytics")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var analyticsModel = await _clientService.GetAnalyticsViewModelAsync(userId);

            return View(analyticsModel);
        }
    }
}