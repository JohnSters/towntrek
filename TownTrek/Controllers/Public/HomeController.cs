using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers.Public;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View("~/Views/Public/Index.cshtml");
    }

    public IActionResult Privacy()
    {
        return View("~/Views/Public/Privacy.cshtml");
    }

    public IActionResult Download()
    {
        return View("~/Views/Public/Download.cshtml");
    }

    public IActionResult Terms()
    {
        return View("~/Views/Public/Terms.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
