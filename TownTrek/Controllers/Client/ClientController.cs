using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Attributes;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "ClientAccess")]
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

        // Contact Admin - Allow clients to send messages to administrators
        [HttpGet]
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> ContactAdmin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await _clientService.GetContactAdminViewModelAsync(userId);
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> ContactAdmin(ContactAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                model = await _clientService.GetContactAdminViewModelAsync(userId);
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _clientService.CreateAdminMessageAsync(userId, model.TopicId, model.Subject, model.Message);
                
                TempData["SuccessMessage"] = "Your message has been sent to the administrators. We'll respond as soon as possible.";
                return RedirectToAction(nameof(ContactAdmin));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin message for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "There was an error sending your message. Please try again.";
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                model = await _clientService.GetContactAdminViewModelAsync(userId);
                return View(model);
            }
        }

        // AJAX endpoint to get topic details
        [HttpGet]
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> GetTopicDetails(int topicId)
        {
            var topic = await _clientService.GetAdminMessageTopicAsync(topicId);
            if (topic == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = topic.Id,
                name = topic.Name,
                description = topic.Description,
                priority = topic.Priority,
                colorClass = topic.ColorClass,
                iconClass = topic.IconClass
            });
        }

        // Profile/Settings/Billing handled by dedicated controllers

    }
}