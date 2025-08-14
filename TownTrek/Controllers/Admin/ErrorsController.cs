using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TownTrek.Models.ViewModels;
using TownTrek.Services;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class ErrorsController : Controller
    {
        private readonly IDatabaseErrorLogger _errorLogger;

        public ErrorsController(IDatabaseErrorLogger errorLogger)
        {
            _errorLogger = errorLogger;
        }

        // GET: AdminErrors
        public async Task<IActionResult> Index(ErrorLogFilter? filter)
        {
            filter ??= new ErrorLogFilter();
            
            var errors = await _errorLogger.GetErrorLogsAsync(filter);
            
            ViewBag.Filter = filter;
            ViewBag.ErrorTypes = new[]
            {
                "Exception",
                "NotFound", 
                "Unauthorized",
                "Argument",
                "Api"
            };
            ViewBag.Severities = new[]
            {
                "Warning",
                "Error",
                "Critical"
            };
            
            return View(errors);
        }

        // GET: AdminErrors/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var error = await _errorLogger.GetErrorByIdAsync(id);
            if (error == null)
            {
                return NotFound();
            }

            return View(error);
        }

        // POST: AdminErrors/Resolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(long id, string? notes)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            await _errorLogger.MarkAsResolvedAsync(id, userId, notes);
            
            TempData["SuccessMessage"] = "Error has been marked as resolved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AdminErrors/Unresolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unresolve(long id)
        {
            await _errorLogger.MarkAsUnresolvedAsync(id);
            
            TempData["SuccessMessage"] = "Error has been marked as unresolved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: AdminErrors/Stats
        public async Task<IActionResult> Stats()
        {
            var stats = await _errorLogger.GetErrorStatsAsync(TimeSpan.FromDays(30));
            return View(stats);
        }
    }
}