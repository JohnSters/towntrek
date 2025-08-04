using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;

namespace TownTrek.Controllers;

public class TownsController : Controller
{
    private readonly TownTrekDbContext _context;

    public TownsController(TownTrekDbContext context)
    {
        _context = context;
    }

    // GET: Towns
    public async Task<IActionResult> Index()
    {
        var towns = await _context.Towns.ToListAsync();
        return View(towns);
    }

    // GET: Towns/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var town = await _context.Towns.FirstOrDefaultAsync(m => m.Id == id);
        if (town == null)
        {
            return NotFound();
        }

        return View(town);
    }

    // GET: Towns/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Towns/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,State,Country,Latitude,Longitude,Population")] Town town)
    {
        if (ModelState.IsValid)
        {
            town.CreatedAt = DateTime.UtcNow;
            _context.Add(town);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(town);
    }

    // GET: Towns/Test - Simple test endpoint to verify database connection
    public async Task<IActionResult> Test()
    {
        try
        {
            // Test database connection
            var canConnect = await _context.Database.CanConnectAsync();
            var townCount = await _context.Towns.CountAsync();
            
            ViewBag.CanConnect = canConnect;
            ViewBag.TownCount = townCount;
            ViewBag.DatabaseName = _context.Database.GetDbConnection().Database;
            
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View();
        }
    }
}