using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Attributes
{
    public class RequireActiveSubscriptionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string? _requiredFeature;
        private readonly bool _allowFreeTier;

        public RequireActiveSubscriptionAttribute(string? requiredFeature = null, bool allowFreeTier = false)
        {
            _requiredFeature = requiredFeature;
            _allowFreeTier = allowFreeTier;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var subscriptionAuthService = context.HttpContext.RequestServices
                .GetRequiredService<ISubscriptionAuthService>();

            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var authResult = await subscriptionAuthService.ValidateUserSubscriptionAsync(userId);

            // If user is not authenticated, redirect to login
            if (!authResult.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // If no active subscription and free tier is not allowed, redirect to subscription page
            if (!authResult.HasActiveSubscription && !_allowFreeTier)
            {
                var controller = context.Controller as Controller;
                controller?.TempData.Add("ErrorMessage", "An active subscription is required to access this feature.");
                context.Result = new RedirectToActionResult("Subscription", "Client", null);
                return;
            }

            // If payment is not valid, redirect to payment (except for pending payments)
            if (authResult.HasActiveSubscription && !authResult.IsPaymentValid && !string.IsNullOrEmpty(authResult.RedirectUrl))
            {
                // Allow access for pending payments but show warning
                if (authResult.PaymentStatus?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var controller = context.Controller as Controller;
                    controller?.TempData.Add("WarningMessage", $"Your payment is pending. Please complete your payment to ensure uninterrupted service.");
                    // Continue to allow access
                }
                else
                {
                    // For failed/rejected payments, redirect to payment
                    var controller = context.Controller as Controller;
                    controller?.TempData.Add("ErrorMessage", $"Payment required. Status: {authResult.PaymentStatus}");
                    context.Result = new RedirectResult(authResult.RedirectUrl);
                    return;
                }
            }

            // Check specific feature access if required
            if (!string.IsNullOrEmpty(_requiredFeature))
            {
                var hasFeature = await subscriptionAuthService.CanAccessFeatureAsync(userId, _requiredFeature);
                if (!hasFeature)
                {
                    var controller = context.Controller as Controller;
                    controller?.TempData.Add("ErrorMessage", "Your subscription plan does not include this feature. Please upgrade to access it.");
                    context.Result = new RedirectToActionResult("Subscription", "Client", null);
                    return;
                }
            }

            // Add subscription info to ViewData for use in views
            if (context.Controller is Controller controllerInstance)
            {
                controllerInstance.ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
                controllerInstance.ViewData["UserLimits"] = authResult.Limits;
                
                // Add user name info to ViewData for layout display
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    controllerInstance.ViewData["UserFirstName"] = user.FirstName;
                    controllerInstance.ViewData["UserLastName"] = user.LastName;
                    controllerInstance.ViewData["UserFullName"] = $"{user.FirstName} {user.LastName}".Trim();
                }
            }

            await next();
        }
    }
}