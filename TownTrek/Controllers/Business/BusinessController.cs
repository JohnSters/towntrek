
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Attributes;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Business
{
    [Authorize]
    [Route("Client/Business/[action]")]
    public class BusinessController(
        IBusinessService businessService,
        IClientService clientService,
        ISubscriptionAuthService subscriptionAuthService,
        ILogger<BusinessController> logger) : Controller
    {
        private readonly IBusinessService _businessService = businessService;
        private readonly IClientService _clientService = clientService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ILogger<BusinessController> _logger = logger;

        // Business Management
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> ManageBusinesses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var businesses = await _businessService.GetUserBusinessesAsync(userId);
            
            // Get user limits for display (still needed on page)
            var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
            ViewBag.UserLimits = limits;
            
            return View(businesses);
        }

        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Check if user can add more businesses
            if (!await _businessService.CanUserAddBusinessAsync(userId))
            {
                var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
                TempData["ErrorMessage"] = $"You have reached your subscription limit for businesses ({limits.CurrentBusinessCount}/{limits.MaxBusinesses}). Please upgrade your plan.";
                return RedirectToAction(actionName: "Index", controllerName: "Subscription");
            }

            var model = await _businessService.GetAddBusinessViewModelAsync(userId);
            
            // Header data resolved by TopUserMenu view component
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddBusinessViewModel model)
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

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var model = await _clientService.PrepareEditBusinessViewModelAsync(id, userId);
                
                return View(model);
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] = "Business not found or you don't have permission to edit it.";
                return RedirectToAction("ManageBusinesses");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddBusinessViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (ModelState.IsValid)
            {
                var result = await _businessService.UpdateBusinessAsync(id, model, userId);
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Business listing updated successfully! Your business has been set to 'Pending' status and will be reviewed by our team before going live.";
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
                return View(baseModel);
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
            try
            {
                _logger.LogInformation("GetSubCategories called with category: {Category}", category);
                var subCategories = await _businessService.GetSubCategoriesAsync(category);
                _logger.LogInformation("Found {Count} subcategories for category: {Category}", subCategories.Count, category);
                return Json(subCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subcategories for category: {Category}", category);
                return Json(new List<object>());
            }
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
    }
}