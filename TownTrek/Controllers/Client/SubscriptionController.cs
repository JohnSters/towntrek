using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "ClientAccess")]
    [Route("Client/[controller]/[action]")] // Conventional: /Client/Subscription/Index
    public class SubscriptionController(
        IClientService clientService,
        UserManager<ApplicationUser> userManager,
        ILogger<SubscriptionController> logger) : Controller
    {
        private readonly IClientService _clientService = clientService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<SubscriptionController> _logger = logger;

        // Subscription & Billing
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await _clientService.GetSubscriptionViewModelAsync(userId);

            // Subscription tier for header is resolved in the TopUserMenu view component

            // Clear any inappropriate success messages for existing users
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (user.HasActiveSubscription)
                {
                    TempData.Remove("SuccessMessage");
                }
                
                // Header data is resolved in the TopUserMenu view component
            }

            // With custom view location expander, discovery will find Views/Client/Subscription/Index.cshtml
            // Pass the model to avoid null reference in the view
            return View(model);
        }

        public async Task<IActionResult> Billing()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.FindByIdAsync(userId);
            
            // Header data is resolved in the TopUserMenu view component
            
            // With custom view location expander, discovery will find Views/Client/Subscription/Billing.cshtml
            return View();
        }
    }
}