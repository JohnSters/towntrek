using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Security.Claims;
using System.Text.Json;

using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces.AdminAnalytics;

namespace TownTrek.Controllers.Admin;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/[controller]/[action]")]
public class AnalyticsMonitoringController : Controller
{
    private readonly IAnalyticsPerformanceMonitor _performanceMonitor;
    private readonly IAnalyticsErrorTracker _errorTracker;
    private readonly IAnalyticsUsageTracker _usageTracker;
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<AnalyticsMonitoringController> _logger;

    public AnalyticsMonitoringController(
        IAnalyticsPerformanceMonitor performanceMonitor,
        IAnalyticsErrorTracker errorTracker,
        IAnalyticsUsageTracker usageTracker,
        HealthCheckService healthCheckService,
        ILogger<AnalyticsMonitoringController> logger)
    {
        _performanceMonitor = performanceMonitor;
        _errorTracker = errorTracker;
        _usageTracker = usageTracker;
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    // GET: Admin/AnalyticsMonitoring/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var viewModel = new AnalyticsMonitoringDashboardViewModel
            {
                PerformanceStats = await _performanceMonitor.GetPerformanceStatsAsync(),
                ErrorStats = await _errorTracker.GetErrorStatsAsync(),
                UsageStats = await _usageTracker.GetUsageStatsAsync(),
                HealthStatus = await GetHealthStatusAsync(),
                RecentErrors = await _errorTracker.GetRecentErrorsAsync(10),
                SlowQueries = await _performanceMonitor.GetSlowQueriesAsync(10),
                MostUsedFeatures = await _usageTracker.GetMostUsedFeaturesAsync(10),
                UnusedFeatures = await _usageTracker.GetUnusedFeaturesAsync()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading analytics monitoring dashboard");
            TempData["ErrorMessage"] = "Unable to load monitoring dashboard. Please try again.";
            return RedirectToAction("Index", "Admin");
        }
    }

    // GET: Admin/AnalyticsMonitoring/Performance
    public async Task<IActionResult> Performance(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var performanceStats = await _performanceMonitor.GetPerformanceStatsAsync(fromDate, toDate);
            var slowQueries = await _performanceMonitor.GetSlowQueriesAsync(20);
            var userEngagement = await _performanceMonitor.GetUserEngagementStatsAsync(fromDate, toDate);

            var viewModel = new AnalyticsPerformanceViewModel
            {
                PerformanceStats = performanceStats,
                SlowQueries = slowQueries,
                UserEngagement = userEngagement,
                FromDate = fromDate ?? DateTime.UtcNow.AddDays(-1),
                ToDate = toDate ?? DateTime.UtcNow
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading performance monitoring data");
            TempData["ErrorMessage"] = "Unable to load performance data. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    // GET: Admin/AnalyticsMonitoring/Errors
    public async Task<IActionResult> Errors(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var errorStats = await _errorTracker.GetErrorStatsAsync(fromDate, toDate);
            var recentErrors = await _errorTracker.GetRecentErrorsAsync(50);
            var errorTrends = await _errorTracker.GetErrorTrendsAsync(fromDate, toDate);
            var errorBreakdown = await _errorTracker.GetErrorBreakdownAsync(fromDate, toDate);
            var criticalErrors = await _errorTracker.GetCriticalErrorsAsync();

            var viewModel = new AnalyticsErrorsViewModel
            {
                ErrorStats = errorStats,
                RecentErrors = recentErrors,
                ErrorTrends = errorTrends,
                ErrorBreakdown = errorBreakdown,
                CriticalErrors = criticalErrors,
                FromDate = fromDate ?? DateTime.UtcNow.AddDays(-1),
                ToDate = toDate ?? DateTime.UtcNow
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading error monitoring data");
            TempData["ErrorMessage"] = "Unable to load error data. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    // GET: Admin/AnalyticsMonitoring/Usage
    public async Task<IActionResult> Usage(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var usageStats = await _usageTracker.GetUsageStatsAsync(fromDate, toDate);
            var mostUsedFeatures = await _usageTracker.GetMostUsedFeaturesAsync(20);
            var userEngagement = await _usageTracker.GetUserEngagementMetricsAsync(fromDate, toDate);
            var sessionAnalytics = await _usageTracker.GetSessionAnalyticsAsync(fromDate, toDate);
            var featureAdoption = await _usageTracker.GetFeatureAdoptionRatesAsync(fromDate, toDate);
            var unusedFeatures = await _usageTracker.GetUnusedFeaturesAsync(fromDate, toDate);

            var viewModel = new AnalyticsUsageViewModel
            {
                UsageStats = usageStats,
                MostUsedFeatures = mostUsedFeatures,
                UserEngagement = userEngagement,
                SessionAnalytics = sessionAnalytics,
                FeatureAdoption = featureAdoption,
                UnusedFeatures = unusedFeatures,
                FromDate = fromDate ?? DateTime.UtcNow.AddDays(-7),
                ToDate = toDate ?? DateTime.UtcNow
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading usage monitoring data");
            TempData["ErrorMessage"] = "Unable to load usage data. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    // GET: Admin/AnalyticsMonitoring/Health
    public async Task<IActionResult> Health()
    {
        try
        {
            var healthStatus = await GetHealthStatusAsync();
            return View(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading health check data");
            TempData["ErrorMessage"] = "Unable to load health check data. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    // POST: Admin/AnalyticsMonitoring/ResolveError
    [HttpPost]
    public IActionResult ResolveError(int errorId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // This would typically update the error log to mark it as resolved
            // For now, we'll just log the action
            _logger.LogInformation("Admin {AdminUserId} resolved error {ErrorId}", userId, errorId);
            
            TempData["SuccessMessage"] = "Error marked as resolved.";
            return RedirectToAction("Errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving error {ErrorId}", errorId);
            TempData["ErrorMessage"] = "Unable to resolve error. Please try again.";
            return RedirectToAction("Errors");
        }
    }

    // POST: Admin/AnalyticsMonitoring/GenerateTestData
    [HttpPost]
    public async Task<IActionResult> GenerateTestData()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            _logger.LogInformation("Generating test data for user {UserId}", userId);
            
            // Generate some test performance data
            await _performanceMonitor.TrackAnalyticsPageLoadAsync(userId, TimeSpan.FromMilliseconds(1500), true);
            await _performanceMonitor.TrackAnalyticsPageLoadAsync(userId, TimeSpan.FromMilliseconds(800), true);
            await _performanceMonitor.TrackAnalyticsPageLoadAsync(userId, TimeSpan.FromMilliseconds(2200), false, "Timeout error");
            
            // Generate some test usage data
            await _usageTracker.TrackFeatureUsageAsync(userId, "AnalyticsDashboard", TimeSpan.FromMilliseconds(5000));
            await _usageTracker.TrackFeatureUsageAsync(userId, "ViewsChart", TimeSpan.FromMilliseconds(2000));
            await _usageTracker.TrackFeatureUsageAsync(userId, "ReviewsChart", TimeSpan.FromMilliseconds(1500));
            
            // Generate some test error data
            await _errorTracker.TrackErrorAsync(userId, "Chart", "Chart rendering failed", "Stack trace here");
            await _errorTracker.TrackErrorAsync(userId, "Database", "Query timeout", "Database error stack trace");
            
            _logger.LogInformation("Test data generated successfully for user {UserId}", userId);
            
            TempData["SuccessMessage"] = "Test data generated successfully. Refresh the dashboard to see the results.";
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test data");
            TempData["ErrorMessage"] = "Unable to generate test data. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    // GET: Admin/AnalyticsMonitoring/TestSystem
    [HttpGet]
    public async Task<IActionResult> TestSystem()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Test performance tracking
            await _performanceMonitor.TrackAnalyticsPageLoadAsync(userId, TimeSpan.FromMilliseconds(1000), true);
            
            // Test usage tracking
            await _usageTracker.TrackFeatureUsageAsync(userId, "TestFeature", TimeSpan.FromMilliseconds(2000));
            
            // Test error tracking
            await _errorTracker.TrackErrorAsync(userId, "Test", "Test error", "Test stack trace");
            
            // Get stats immediately
            var performanceStats = await _performanceMonitor.GetPerformanceStatsAsync();
            var usageStats = await _usageTracker.GetUsageStatsAsync();
            var errorStats = await _errorTracker.GetErrorStatsAsync();
            
            var result = new
            {
                Success = true,
                PerformanceStats = performanceStats,
                UsageStats = usageStats,
                ErrorStats = errorStats,
                Message = "Test completed successfully"
            };
            
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing analytics system");
            return Json(new { Success = false, Error = ex.Message });
        }
    }

    // GET: Admin/AnalyticsMonitoring/ExportData
    public async Task<IActionResult> ExportData(string dataType, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-30);
            toDate ??= DateTime.UtcNow;

            byte[] data;
            string fileName;
            string contentType;

            switch (dataType.ToLower())
            {
                case "performance":
                    var performanceStats = await _performanceMonitor.GetPerformanceStatsAsync(fromDate, toDate);
                    data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(performanceStats, new JsonSerializerOptions { WriteIndented = true }));
                    fileName = $"analytics-performance-{fromDate:yyyy-MM-dd}-{toDate:yyyy-MM-dd}.json";
                    contentType = "application/json";
                    break;

                case "errors":
                    var errorStats = await _errorTracker.GetErrorStatsAsync(fromDate, toDate);
                    data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorStats, new JsonSerializerOptions { WriteIndented = true }));
                    fileName = $"analytics-errors-{fromDate:yyyy-MM-dd}-{toDate:yyyy-MM-dd}.json";
                    contentType = "application/json";
                    break;

                case "usage":
                    var usageStats = await _usageTracker.GetUsageStatsAsync(fromDate, toDate);
                    data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(usageStats, new JsonSerializerOptions { WriteIndented = true }));
                    fileName = $"analytics-usage-{fromDate:yyyy-MM-dd}-{toDate:yyyy-MM-dd}.json";
                    contentType = "application/json";
                    break;

                default:
                    TempData["ErrorMessage"] = "Invalid data type specified.";
                    return RedirectToAction("Dashboard");
            }

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {DataType} data", dataType);
            TempData["ErrorMessage"] = "Unable to export data. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    private async Task<HealthReport> GetHealthStatusAsync()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync(reg => reg.Name == "analytics");
        return healthReport;
    }
}
