using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRegistrationService _registrationService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger, 
        IRegistrationService registrationService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _registrationService = registrationService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Register()
    {
        // Get available subscription tiers for the registration page
        var subscriptionTiers = await _registrationService.GetAvailableSubscriptionTiersAsync();
        ViewBag.SubscriptionTiers = subscriptionTiers;
        
        return View();
    }

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
            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // Attempt to sign in
            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully", email);
                
                // Check if user is admin and redirect accordingly
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    // Redirect to client dashboard or home page
                    return RedirectToAction("Dashboard", "Client");
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
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    public IActionResult Download()
    {
        return View();
    }

    public IActionResult Terms()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
