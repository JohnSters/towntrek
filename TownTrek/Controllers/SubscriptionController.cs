using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Controllers
{
    [Authorize]
    [Route("Client/[action]")] // Maintain existing routes for backward compatibility
    public class SubscriptionController(
        IClientService clientService,
        UserManager<ApplicationUser> userManager,
        ILogger<SubscriptionController> logger) : Controller
    {
        private readonly IClientService _clientService = clientService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<SubscriptionController> _logger = logger;

        // Subscription & Billing
        public async Task<IActionResult> Subscription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await _clientService.GetSubscriptionViewModelAsync(userId);

            // Set subscription tier for layout display
            ViewData["UserSubscriptionTier"] = model.CurrentSubscription?.SubscriptionTier;

            // Clear any inappropriate success messages for existing users
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.HasActiveSubscription)
            {
                TempData.Remove("SuccessMessage");
            }

            return View("~/Views/Client/Subscription/Index.cshtml", model);
        }

        public IActionResult Billing()
        {
            return View("~/Views/Client/Subscription/Billing.cshtml");
        }
    }
}