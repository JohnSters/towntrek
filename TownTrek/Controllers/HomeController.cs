using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRegistrationService _registrationService;

    public HomeController(ILogger<HomeController> logger, IRegistrationService registrationService)
    {
        _logger = logger;
        _registrationService = registrationService;
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
