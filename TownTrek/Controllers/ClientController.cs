using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Attributes;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;

namespace TownTrek.Controllers
{
    [Authorize]
    public class ClientController(
        IClientService clientService,
        IBusinessService businessService,
        ISubscriptionAuthService subscriptionAuthService,
        UserManager<ApplicationUser> userManager,
        ILogger<ClientController> logger) : Controller
    {
        private readonly IClientService _clientService = clientService;
        private readonly IBusinessService _businessService = businessService;
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

        // Business Management
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> ManageBusinesses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var businesses = await _businessService.GetUserBusinessesAsync(userId);
            
            // Get user limits for display
            var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
            ViewBag.UserLimits = limits;
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
            return View(businesses);
        }

        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> AddBusiness()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Check if user can add more businesses
            if (!await _businessService.CanUserAddBusinessAsync(userId))
            {
                var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
                TempData["ErrorMessage"] = $"You have reached your subscription limit for businesses ({limits.CurrentBusinessCount}/{limits.MaxBusinesses}). Please upgrade your plan.";
                return RedirectToAction("Subscription");
            }

            var model = await _businessService.GetAddBusinessViewModelAsync(userId);
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBusiness(AddBusinessViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (ModelState.IsValid)
            {
                var result = await _businessService.CreateBusinessAsync(model, userId);
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Business listing created successfully! It will be reviewed and activated within 24 hours.";
                    return RedirectToAction("ManageBusinesses");
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
            }
            
            // Reload the view model data if there are validation errors
            model = await _businessService.GetAddBusinessViewModelAsync(userId);
            return View(model);
        }

        public async Task<IActionResult> EditBusiness(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var model = await _clientService.PrepareEditBusinessViewModelAsync(id, userId);
                
                // Set subscription tier for layout display
                var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
                
                return View("EditBusiness", model);
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] = "Business not found or you don't have permission to edit it.";
                return RedirectToAction("ManageBusinesses");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBusiness(int id, AddBusinessViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (ModelState.IsValid)
            {
                var result = await _businessService.UpdateBusinessAsync(id, model, userId);
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Business listing updated successfully!";
                    return RedirectToAction("ManageBusinesses");
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
            }
            
            // If there are validation errors, reload the business data and preserve the form data
            try
            {
                var baseModel = await _clientService.PrepareEditBusinessViewModelWithFormDataAsync(id, userId, model);
                return View("AddBusiness", baseModel);
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("ManageBusinesses");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _businessService.DeleteBusinessAsync(id, userId);
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Business listing deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction("ManageBusinesses");
        }

        // AJAX endpoints for dynamic form behavior
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(string category)
        {
            var subCategories = await _businessService.GetSubCategoriesAsync(category);
            return Json(subCategories);
        }

        [HttpPost]
        public IActionResult ValidateAddress([FromBody] AddressValidationRequest request)
        {
            // This would integrate with Google Maps Geocoding API
            // For now, return a mock response
            return Json(new { 
                isValid = true, 
                latitude = -26.2041, 
                longitude = 28.0473,
                formattedAddress = request.Address
            });
        }

        // Profile Management
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

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

            return View(model);
        }

        public IActionResult Billing()
        {
            return View();
        }

        // Analytics & Reports
        [RequireActiveSubscription(requiredFeature: "BasicAnalytics")]
        public async Task<IActionResult> Analytics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var analyticsModel = await _clientService.GetAnalyticsViewModelAsync(userId);

            return View(analyticsModel);
        }

        // Support & Help
        public IActionResult Support()
        {
            return View();
        }

        public IActionResult Documentation()
        {
            return View();
        }

        // Settings
        public IActionResult Settings()
        {
            return View();
        }

    }
}