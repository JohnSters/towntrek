using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Attributes;
using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize]
    [Route("Client/[action]")]
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
        [HttpGet]
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
            
            // Subscription tier now resolved in TopUserMenu view component when needed

            return View(dashboardModel);
        }

        // Profile/Settings/Billing handled by dedicated controllers

    }
}