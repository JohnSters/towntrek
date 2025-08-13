using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Auth
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            ILogger<AuthController> logger,
            IRegistrationService registrationService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _registrationService = registrationService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var subscriptionTiers = await _registrationService.GetAvailableSubscriptionTiersAsync();
            ViewBag.SubscriptionTiers = subscriptionTiers;
            
            var model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var subscriptionTiers = await _registrationService.GetAvailableSubscriptionTiersAsync();
                ViewBag.SubscriptionTiers = subscriptionTiers;
                return View(model);
            }

            try
            {
                RegistrationResult result;

                if (model.IsBusinessOwner)
                {
                    result = await _registrationService.RegisterBusinessOwnerAsync(model);
                }
                else
                {
                    result = await _registrationService.RegisterMemberAsync(model);
                }

                if (result.IsSuccess)
                {
                    if (result.RequiresPayment && !string.IsNullOrEmpty(result.PaymentUrl))
                    {
                        _logger.LogInformation("Redirecting business owner {Email} to PayFast: {PaymentUrl}", model.Email, result.PaymentUrl);
                        TempData["PendingUserId"] = result.User?.Id;
                        TempData["SuccessMessage"] = "Account created successfully! Please complete your payment to activate your subscription.";
                        
                        return Redirect(result.PaymentUrl);
                    }
                    else
                    {
                        _logger.LogInformation("Member {Email} registered successfully, signing in", model.Email);
                        await _signInManager.SignInAsync(result.User!, isPersistent: false);
                        TempData["SuccessMessage"] = "Welcome to TownTrek! Your account has been created successfully.";
                        return RedirectToAction("Index", "Public");
                    }
                }
                else
                {
                    _logger.LogWarning("Registration failed for {Email}: {Error}", model.Email, result.ErrorMessage);
                    ModelState.AddModelError("", result.ErrorMessage ?? "Registration failed. Please try again.");
                    var subscriptionTiers = await _registrationService.GetAvailableSubscriptionTiersAsync();
                    ViewBag.SubscriptionTiers = subscriptionTiers;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                var subscriptionTiers = await _registrationService.GetAvailableSubscriptionTiersAsync();
                ViewBag.SubscriptionTiers = subscriptionTiers;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }

                // If RememberMe is checked, extend session to 7 days, otherwise use default 8 hours
                var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", email);
                    
                    // Update last login time
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Ensure users with active subscriptions have proper roles
                    if (user.HasActiveSubscription && !string.IsNullOrEmpty(user.CurrentSubscriptionTier))
                    {
                        var expectedClientRole = $"Client-{user.CurrentSubscriptionTier}";
                        if (!roles.Contains(expectedClientRole))
                        {
                            await _userManager.AddToRoleAsync(user, expectedClientRole);
                            _logger.LogInformation("Added {Role} role to user {Email}", expectedClientRole, user.Email);
                        }
                        // Refresh roles after adding
                        roles = await _userManager.GetRolesAsync(user);
                    }
                    
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (roles.Any(r => r.StartsWith("Client-")) || user.HasActiveSubscription)
                    {
                        // For business owners/clients, check subscription status and redirect accordingly
                        var subscriptionAuthService = HttpContext.RequestServices.GetRequiredService<ISubscriptionAuthService>();
                        var redirectUrl = await subscriptionAuthService.GetRedirectUrlForUserAsync(user.Id);
                        
                        if (redirectUrl == "/Client/Dashboard")
                        {
                            return RedirectToAction("Dashboard", "Client");
                        }
                        else if (redirectUrl.StartsWith("/"))
                        {
                            return Redirect(redirectUrl);
                        }
                        else
                        {
                            return RedirectToAction("Dashboard", "Client");
                        }
                    }
                    else
                    {
                        // For regular members, redirect to public browsing landing
                        return RedirectToAction("Index", "Public");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for {Email}", email);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.Identity?.Name;
            
            // Sign out the user (clears authentication cookie and all claims)
            await _signInManager.SignOutAsync();
            
            // Clear any additional cookies that might exist
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith(".AspNetCore") || cookie.StartsWith("Identity"))
                {
                    Response.Cookies.Delete(cookie);
                }
            }
            
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            
            // Redirect to home page with cache-busting to prevent back button issues
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // TODO: Implement forgot password functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            // Implementation for password reset
            return View();
        }
    }
}