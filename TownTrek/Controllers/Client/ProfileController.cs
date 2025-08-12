using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize]
    [Route("Client/Profile")] // Preserve existing URL structure
    public class ProfileController(
        UserManager<ApplicationUser> userManager,
        ISubscriptionAuthService subscriptionAuthService,
        ILogger<ProfileController> logger) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ILogger<ProfileController> _logger = logger;

        // GET /Client/Profile
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // GET /Client/Profile/Edit
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
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

            return View("EditProfile", model);
        }

        // POST /Client/Profile/Edit
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.ViewModels.EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditProfile", model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Location = model.Location;
                user.ProfilePictureUrl = model.ProfilePictureUrl;

                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        ModelState.AddModelError("Email", "This email address is already in use.");
                        return View("EditProfile", model);
                    }

                    user.Email = model.Email;
                    user.UserName = model.Email;
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("EditProfile", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                return View("EditProfile", model);
            }
        }

        // GET /Client/Profile/Settings
        [HttpGet("Settings")]
        public IActionResult Settings()
        {
            return View("Settings");
        }
    }
}


