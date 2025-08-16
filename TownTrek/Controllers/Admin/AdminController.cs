using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;


namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabaseErrorLogger _errorLogger;
        private readonly IAdminMessageService _adminMessageService;

        public AdminController(
            ApplicationDbContext context, 
            IDatabaseErrorLogger errorLogger,
            IAdminMessageService adminMessageService)
        {
            _context = context;
            _errorLogger = errorLogger;
            _adminMessageService = adminMessageService;
        }

        // Dashboard - Main admin overview
        public async Task<IActionResult> Dashboard()
        {
            // Get basic statistics
            var stats = new AdminDashboardViewModel
            {
                TotalTowns = await _context.Towns.CountAsync(),
                TotalBusinesses = await _context.Businesses.CountAsync(),
                ActiveBusinesses = await _context.Businesses.CountAsync(b => b.Status == "Active"),
                PendingApprovals = await _context.Businesses.CountAsync(b => b.Status == "Pending"),
                TotalPopulation = await _context.Towns.Where(t => t.Population.HasValue).SumAsync(t => t.Population!.Value),
                TownsWithLandmarks = await _context.Towns.CountAsync(t => !string.IsNullOrEmpty(t.Landmarks))
            };

            // Get error statistics
            try
            {
                var errorStats = await _errorLogger.GetErrorStatsAsync(TimeSpan.FromDays(30));
                stats.ErrorStats = errorStats;
                stats.CriticalErrorsLast24Hours = errorStats.CriticalErrorsLast24Hours;
                stats.UnresolvedErrorsTotal = errorStats.UnresolvedErrors;
                stats.RecentErrors = errorStats.RecentErrors;
            }
            catch (Exception)
            {
                // If error statistics fail, continue with basic dashboard
                stats.CriticalErrorsLast24Hours = 0;
                stats.UnresolvedErrorsTotal = 0;
                stats.RecentErrors = new List<RecentErrorActivity>();
            }

            // Get message statistics
            try
            {
                stats.MessageStats = await _adminMessageService.GetMessageStatsAsync();
                stats.RecentMessages = await _adminMessageService.GetRecentMessagesAsync(5);
            }
            catch (Exception)
            {
                // If message statistics fail, continue with basic dashboard
                stats.MessageStats = new AdminMessageStats();
                stats.RecentMessages = new List<AdminMessage>();
            }

            return View(stats);
        }
        // Settings
        public IActionResult Settings()
        {
            return View();
        }

    }
}