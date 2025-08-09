using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
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
            if (user != null)
            {
                if (user.HasActiveSubscription)
                {
                    TempData.Remove("SuccessMessage");
                }
                
                // Add user name info to ViewData for layout display
                ViewData["UserFirstName"] = user.FirstName;
                ViewData["UserLastName"] = user.LastName;
                ViewData["UserFullName"] = $"{user.FirstName} {user.LastName}".Trim();
            }

            return View("~/Views/Client/Subscription/Index.cshtml", model);
        }

        public async Task<IActionResult> Billing()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user != null)
            {
                // Add user name info to ViewData for layout display
                ViewData["UserFirstName"] = user.FirstName;
                ViewData["UserLastName"] = user.LastName;
                ViewData["UserFullName"] = $"{user.FirstName} {user.LastName}".Trim();
            }
            
            return View("~/Views/Client/Subscription/Billing.cshtml");
        }
    }
}