using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Controllers
{
    [Authorize]
    public class ClientController(
        IClientService clientService,
        ISubscriptionAuthService subscriptionAuthService,
        UserManager<ApplicationUser> userManager,
        ILogger<ClientController> logger) : Controller
    {
        private readonly IClientService _clientService = clientService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<ClientController> _logger = logger;

        // Dashboard - Main overview page
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Get subscription validation result for payment warnings
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            
            // If payment is pending or rejected, show appropriate message
            if (authResult.HasActiveSubscription && !authResult.IsPaymentValid)
            {
                TempData["WarningMessage"] = $"Your payment status is {authResult.PaymentStatus}. Please complete your payment to access all features.";
                if (!string.IsNullOrEmpty(authResult.RedirectUrl))
                {
                    ViewBag.PaymentUrl = authResult.RedirectUrl;
                }
            }

            var dashboardModel = await _clientService.GetDashboardViewModelAsync(userId);
            
            // Set subscription tier for layout display
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;

            return View(dashboardModel);
        }

        // Profile Management
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.GetUserAsync(User);
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
            return View(user);
        }

        // Settings
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
            return View();
        }

    }
}