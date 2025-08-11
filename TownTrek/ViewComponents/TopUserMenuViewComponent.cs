using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.ViewComponents
{
    public class TopUserMenuViewComponent(
        UserManager<ApplicationUser> userManager,
        ISubscriptionAuthService subscriptionAuthService) : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new TopUserMenuViewModel();

            var principal = HttpContext.User;
            model.IsAuthenticated = principal?.Identity?.IsAuthenticated == true;

            if (model.IsAuthenticated)
            {
                var userId = principal!.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var firstInitial = string.IsNullOrWhiteSpace(user.FirstName) ? "U" : user.FirstName[0].ToString().ToUpper();
                    var lastInitial = string.IsNullOrWhiteSpace(user.LastName) ? string.Empty : user.LastName[0].ToString().ToUpper();
                    model.Initials = firstInitial + lastInitial;
                    model.DisplayName = string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                        ? (user.UserName ?? "User")
                        : ($"{user.FirstName} {user.LastName}".Trim());

                    var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                    model.SubscriptionTier = authResult.SubscriptionTier;
                    model.DisplayRole = authResult.SubscriptionTier?.DisplayName ?? (principal.IsInRole("Admin") ? "System Admin" : "Free Tier");
                    
                    // Determine user type
                    model.IsBusinessOwner = authResult.SubscriptionTier != null && authResult.HasActiveSubscription;
                    model.IsMember = !model.IsBusinessOwner && !principal.IsInRole("Admin");
                }
            }

            // Use default view discovery for view components: Views/Shared/Components/TopUserMenu/Default.cshtml
            return View(model);
        }
    }
}


