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
            
            // Subscription tier now resolved in TopUserMenu view component when needed

            return View(dashboardModel);
        }

        // Profile endpoints moved to ProfileController to align with conventions

        // Edit Profile - GET
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Profile");
            }

            var model = new Models.ViewModels.EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Location = user.Location,
                ProfilePictureUrl = user.ProfilePictureUrl,
                UserName = user.UserName ?? string.Empty,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                AuthenticationMethod = user.AuthenticationMethod
            };

            // Subscription tier now resolved in TopUserMenu view component when needed
            
            return View("~/Views/Client/Profile/EditProfile.cshtml", model);
        }

        // Edit Profile - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Models.ViewModels.EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Set subscription tier for layout display
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
                
                return View("~/Views/Client/Profile/EditProfile.cshtml", model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Profile");
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Location = model.Location;
                user.ProfilePictureUrl = model.ProfilePictureUrl;

                // Handle email change if different
                if (user.Email != model.Email)
                {
                    // Check if email is already taken
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        ModelState.AddModelError("Email", "This email address is already in use.");
                        
                        // Subscription tier now resolved in TopUserMenu view component when needed
                        
                        return View("~/Views/Client/Profile/EditProfile.cshtml", model);
                    }

                    user.Email = model.Email;
                    user.UserName = model.Email; // Keep username in sync with email
                }

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    
                    // Subscription tier now resolved in TopUserMenu view component when needed
                    
                    return View("~/Views/Client/Profile/EditProfile.cshtml", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                
                // Subscription tier now resolved in TopUserMenu view component when needed
                
                return View("~/Views/Client/Profile/EditProfile.cshtml", model);
            }
        }

        // Settings
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Subscription tier now resolved in TopUserMenu view component when needed
            
            return View("~/Views/Client/Profile/Settings.cshtml");
        }

        // Billing
        public async Task<IActionResult> Billing()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Subscription tier now resolved in TopUserMenu view component when needed
            
            return View("~/Views/Client/Subscription/Billing.cshtml");
        }

    }
}