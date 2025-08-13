using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TownTrek.Data;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers.Home;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<TownTrek.Models.ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<TownTrek.Models.ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Reuse role logic pattern from AdminUsersController to avoid duplication
        var totalBusinesses = await _context.Businesses.CountAsync(b => b.Status != "Deleted");
        var totalTowns = await _context.Towns.CountAsync();

        var membersAll = await _userManager.GetUsersInRoleAsync("Member");
        var cbAll = await _userManager.GetUsersInRoleAsync("Client-Basic");
        var csAll = await _userManager.GetUsersInRoleAsync("Client-Standard");
        var cpAll = await _userManager.GetUsersInRoleAsync("Client-Premium");
        var totalClients = cbAll.Select(u => u.Id).Concat(csAll.Select(u => u.Id)).Concat(cpAll.Select(u => u.Id)).Distinct().Count();
        var totalHappyCustomers = membersAll.Count + totalClients;

        var model = new HomeStatsViewModel
        {
            TotalBusinesses = totalBusinesses,
            TotalTowns = totalTowns,
            TotalHappyCustomers = totalHappyCustomers
        };

        return View(model);
    }

    public IActionResult Privacy()
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
