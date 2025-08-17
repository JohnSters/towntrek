using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "PaidClientAccess")] // Only allow paid clients (Basic, Standard, Premium) + Admin
    [Route("Client/[controller]/[action]")]
    public class AnalyticsController(
        IAnalyticsService analyticsService,
        IAnalyticsCacheService analyticsCacheService,
        ISubscriptionAuthService subscriptionAuthService,
        ITrialService trialService,
        IBusinessService businessService,
        IAnalyticsAuditService analyticsAuditService,
        IAnalyticsExportService analyticsExportService,
        ILogger<AnalyticsController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsCacheService _analyticsCacheService = analyticsCacheService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ITrialService _trialService = trialService;
        private readonly IBusinessService _businessService = businessService;
        private readonly IAnalyticsAuditService _analyticsAuditService = analyticsAuditService;
        private readonly IAnalyticsExportService _analyticsExportService = analyticsExportService;
        private readonly ILogger<AnalyticsController> _logger = logger;

        // Analytics Dashboard - available to all non-trial authenticated clients with active subscription
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users from accessing analytics
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Analytics are not available during the trial period. Please upgrade to access analytics.";
                    return RedirectToAction("Index", "Subscription");
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "AnalyticsDashboard");

                // Fast-path: If user has no businesses, show empty state without invoking full analytics
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (userBusinesses == null || userBusinesses.Count == 0)
                {
                    ViewBag.NoBusinesses = true;
                    return View(new Models.ViewModels.ClientAnalyticsViewModel
                    {
                        Businesses = new List<Models.Business>(),
                        BusinessAnalytics = new List<Models.ViewModels.BusinessAnalyticsData>(),
                        Overview = new Models.ViewModels.AnalyticsOverview(),
                        ViewsOverTime = new List<Models.ViewModels.ViewsOverTimeData>(),
                        ReviewsOverTime = new List<Models.ViewModels.ReviewsOverTimeData>(),
                        PerformanceInsights = new List<Models.ViewModels.BusinessPerformanceInsight>(),
                        CategoryBenchmarks = null,
                        CompetitorInsights = new List<Models.ViewModels.CompetitorInsight>(),
                        SubscriptionTier = string.Empty,
                        HasBasicAnalytics = true,
                        HasStandardAnalytics = false,
                        HasPremiumAnalytics = true
                    });
                }
                
                var analyticsModel = await _analyticsCacheService.GetClientAnalyticsAsync(userId);
                
                return View(analyticsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                    if (userBusinesses == null || userBusinesses.Count == 0)
                    {
                        ViewBag.NoBusinesses = true;
                        return View(new Models.ViewModels.ClientAnalyticsViewModel
                        {
                            Businesses = new List<Models.Business>(),
                            BusinessAnalytics = new List<Models.ViewModels.BusinessAnalyticsData>(),
                            Overview = new Models.ViewModels.AnalyticsOverview()
                        });
                    }
                }
                catch { /* swallow and fall back to generic error */ }

                TempData["ErrorMessage"] = "Unable to load analytics data. Please try again.";
                return RedirectToAction("Dashboard", "Client");
            }
        }

        // Business-specific analytics - available to all non-trial authenticated clients with active subscription
        public async Task<IActionResult> Business(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users from accessing analytics
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Analytics are not available during the trial period. Please upgrade to access analytics.";
                    return RedirectToAction("Index", "Subscription");
                }

                var businessAnalytics = await _analyticsCacheService.GetBusinessAnalyticsAsync(id, userId);
                return View(businessAnalytics);
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = "Business not found or access denied.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading business analytics for business {BusinessId}", id);
                TempData["ErrorMessage"] = "Unable to load business analytics. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // API endpoint for views over time data (for charts)
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> ViewsOverTimeData(int days = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Input validation
                if (days < 1 || days > 365)
                {
                    _logger.LogWarning("Invalid days parameter: {Days} for user {UserId}", days, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid days parameter: {days}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Days parameter must be between 1 and 365" });
                }

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ViewsOverTimeData", null, "Web");

                var data = await _analyticsCacheService.GetViewsOverTimeAsync(userId, days);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading views over time data");
                return Json(new { error = "Unable to load data" });
            }
        }

        // API endpoint for reviews over time data (for charts)
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> ReviewsOverTimeData(int days = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Input validation
                if (days < 1 || days > 365)
                {
                    _logger.LogWarning("Invalid days parameter: {Days} for user {UserId}", days, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid days parameter: {days}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Days parameter must be between 1 and 365" });
                }

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ReviewsOverTimeData", null, "Web");

                var data = await _analyticsCacheService.GetReviewsOverTimeAsync(userId, days);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews over time data");
                return Json(new { error = "Unable to load data" });
            }
        }

        // API endpoint for platform-specific views over time data (for mobile app integration)
        public async Task<IActionResult> ViewsOverTimeByPlatform(int days = 30, string? platform = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                var data = await _analyticsService.GetViewsOverTimeByPlatformAsync(userId, days, platform);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading platform-specific views over time data");
                return Json(new { error = "Unable to load data" });
            }
        }

        // API endpoint for business view statistics (for mobile app integration)
        public async Task<IActionResult> BusinessViewStatistics(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                // Verify user owns this business
                var business = await _businessService.GetBusinessByIdAsync(businessId);
                if (business == null || business.UserId != userId)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Access denied" });
                }

                var statistics = await _analyticsService.GetBusinessViewStatisticsAsync(businessId, startDate, endDate, platform);
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading business view statistics");
                return Json(new { error = "Unable to load data" });
            }
        }

        // ===== CHART DATA API ENDPOINTS (Phase 2.2) =====

        /// <summary>
        /// Get pre-formatted views chart data for Chart.js
        /// </summary>
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> ViewsChartData(int days = 30, string? platform = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Input validation
                if (days < 1 || days > 365)
                {
                    _logger.LogWarning("Invalid days parameter: {Days} for user {UserId}", days, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid days parameter: {days}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Days parameter must be between 1 and 365" });
                }

                // Validate platform parameter
                if (!string.IsNullOrEmpty(platform) && !new[] { "Web", "Mobile", "API" }.Contains(platform))
                {
                    _logger.LogWarning("Invalid platform parameter: {Platform} for user {UserId}", platform, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid platform parameter: {platform}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Platform parameter must be Web, Mobile, or API" });
                }

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ViewsChartData", null, platform ?? "Web");

                var chartData = await _analyticsCacheService.GetViewsChartDataAsync(userId, days, platform);
                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading views chart data");
                return Json(new { error = "Unable to load chart data" });
            }
        }

        /// <summary>
        /// Get pre-formatted reviews chart data for Chart.js
        /// </summary>
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> ReviewsChartData(int days = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Input validation
                if (days < 1 || days > 365)
                {
                    _logger.LogWarning("Invalid days parameter: {Days} for user {UserId}", days, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid days parameter: {days}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Days parameter must be between 1 and 365" });
                }

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ReviewsChartData", null, "Web");

                var chartData = await _analyticsCacheService.GetReviewsChartDataAsync(userId, days);
                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews chart data");
                return Json(new { error = "Unable to load chart data" });
            }
        }

        // Advanced analytics features - now available to all non-trial authenticated clients with active subscription
        public async Task<IActionResult> Benchmarks(string category)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Analytics are not available during the trial period. Please upgrade to access analytics.";
                    return RedirectToAction("Index", "Subscription");
                }

                var benchmarks = await _analyticsService.GetDetailedCategoryBenchmarksAsync(userId, category);
                
                if (benchmarks == null)
                {
                    TempData["InfoMessage"] = "Not enough data available for category benchmarks.";
                    return RedirectToAction("Index");
                }

                return View(benchmarks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category benchmarks");
                TempData["ErrorMessage"] = "Unable to load benchmark data. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // Competitor insights - now available to all non-trial authenticated clients with active subscription
        public async Task<IActionResult> Competitors()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Analytics are not available during the trial period. Please upgrade to access analytics.";
                    return RedirectToAction("Index", "Subscription");
                }

                var insights = await _analyticsCacheService.GetCompetitorInsightsAsync(userId);
                return View(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor insights");
                TempData["ErrorMessage"] = "Unable to load competitor insights. Please try again.";
                return RedirectToAction("Index");
            }
        }



        // Test endpoint for creating analytics snapshots (remove in production)
        [HttpPost]
        public async Task<IActionResult> CreateTestSnapshot()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Only allow admin users or for testing
                if (!User.IsInRole("Admin"))
                {
                    return Json(new { error = "Access denied" });
                }

                var snapshotService = HttpContext.RequestServices.GetRequiredService<IAnalyticsSnapshotService>();
                
                // Create snapshots for yesterday
                var snapshotsCreated = await snapshotService.CreateDailySnapshotsAsync(DateTime.UtcNow.Date.AddDays(-1));
                
                return Json(new { 
                    success = true, 
                    message = $"Created {snapshotsCreated} snapshots for yesterday",
                    snapshotsCreated = snapshotsCreated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test snapshots");
                return Json(new { error = "Unable to create snapshots" });
            }
        }

        // ===== CACHE MANAGEMENT ENDPOINTS =====

        /// <summary>
        /// Invalidate all analytics cache for the current user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _analyticsCacheService.InvalidateUserAnalyticsAsync(userId);
                
                return Json(new { success = true, message = "Analytics cache invalidated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating analytics cache");
                return Json(new { success = false, message = "Failed to invalidate cache" });
            }
        }

        /// <summary>
        /// Get cache statistics (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CacheStatistics()
        {
            try
            {
                var stats = await _analyticsCacheService.GetAnalyticsCacheStatisticsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return Json(new { error = "Failed to get cache statistics" });
            }
        }

        /// <summary>
        /// Warm up cache for active users (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> WarmUpCache()
        {
            try
            {
                // Get list of active users (simplified - in production you'd get this from a service)
                var activeUsers = new List<string>(); // TODO: Get from user service
                
                await _analyticsCacheService.WarmUpAnalyticsCacheAsync(activeUsers);
                
                return Json(new { success = true, message = "Cache warm-up completed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up cache");
                return Json(new { success = false, message = "Failed to warm up cache" });
            }
        }

        // ===== ANALYTICS AUDIT ENDPOINTS (Admin Only) =====

        /// <summary>
        /// Get analytics audit logs for a specific user (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserAuditLogs(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var logs = await _analyticsAuditService.GetUserAuditLogsAsync(userId, startDate, endDate);
                return Json(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user audit logs for {UserId}", userId);
                return Json(new { success = false, message = "Failed to get audit logs" });
            }
        }

        /// <summary>
        /// Get all analytics audit logs (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllAuditLogs(DateTime? startDate = null, DateTime? endDate = null, string? userId = null)
        {
            try
            {
                var logs = await _analyticsAuditService.GetAllAuditLogsAsync(startDate, endDate, userId);
                return Json(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all audit logs");
                return Json(new { success = false, message = "Failed to get audit logs" });
            }
        }

        /// <summary>
        /// Get suspicious activity logs (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SuspiciousActivityLogs(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var logs = await _analyticsAuditService.GetSuspiciousActivityLogsAsync(startDate, endDate);
                return Json(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suspicious activity logs");
                return Json(new { success = false, message = "Failed to get suspicious activity logs" });
            }
        }

        /// <summary>
        /// Clean up old audit logs (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupAuditLogs(int retentionDays = 365)
        {
            try
            {
                var count = await _analyticsAuditService.CleanupOldAuditLogsAsync(retentionDays);
                return Json(new { success = true, message = $"Cleaned up {count} old audit logs" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up audit logs");
                return Json(new { success = false, message = "Failed to clean up audit logs" });
            }
        }

        // Debug action to check subscription status (remove in production)
        public async Task<IActionResult> Debug()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            var tier = await _subscriptionAuthService.GetUserSubscriptionTierAsync(userId);
            var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
            var canAccessBasic = true;
            var canAccessAdvanced = true;

            var debugInfo = new
            {
                UserId = userId,
                IsAuthenticated = authResult.IsAuthenticated,
                HasActiveSubscription = authResult.HasActiveSubscription,
                IsPaymentValid = authResult.IsPaymentValid,
                PaymentStatus = authResult.PaymentStatus,
                TierName = tier?.Name,
                TierDisplayName = tier?.DisplayName,
                CanAccessBasicAnalytics = canAccessBasic,
                CanAccessAdvancedAnalytics = canAccessAdvanced,
                Limits = new
                {
                    limits.HasBasicAnalytics,
                    limits.HasAdvancedAnalytics,
                    limits.MaxBusinesses,
                    limits.CurrentBusinessCount
                }
            };

            return Json(debugInfo);
        }

        // ===== EXPORT AND SHARING ENDPOINTS (Phase 3.1) =====

        /// <summary>
        /// Export business analytics as PDF
        /// </summary>
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ExportBusinessPdf(int businessId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Export features are not available during the trial period.";
                    return RedirectToAction("Business", new { id = businessId });
                }

                // Log export activity
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ExportBusinessPdf", businessId.ToString());

                var pdfData = await _analyticsExportService.GenerateBusinessAnalyticsPdfAsync(businessId, userId, fromDate, toDate);
                
                var fileName = $"business-analytics-{businessId}-{DateTime.UtcNow:yyyy-MM-dd}.pdf";
                return File(pdfData, "application/pdf", fileName);
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = "Business not found or access denied.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting business PDF for business {BusinessId}", businessId);
                TempData["ErrorMessage"] = "Unable to generate PDF. Please try again.";
                return RedirectToAction("Business", new { id = businessId });
            }
        }

        /// <summary>
        /// Export client analytics overview as PDF
        /// </summary>
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ExportOverviewPdf(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Export features are not available during the trial period.";
                    return RedirectToAction("Index");
                }

                // Log export activity
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ExportOverviewPdf");

                var pdfData = await _analyticsExportService.GenerateClientAnalyticsPdfAsync(userId, fromDate, toDate);
                
                var fileName = $"analytics-overview-{DateTime.UtcNow:yyyy-MM-dd}.pdf";
                return File(pdfData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting overview PDF for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "Unable to generate PDF. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export analytics data as CSV
        /// </summary>
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ExportCsv(string dataType, DateTime? fromDate = null, DateTime? toDate = null, int? businessId = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Export features are not available during the trial period.";
                    return RedirectToAction("Index");
                }

                // Validate data type
                if (!new[] { "views", "reviews", "performance" }.Contains(dataType.ToLower()))
                {
                    TempData["ErrorMessage"] = "Invalid data type specified.";
                    return RedirectToAction("Index");
                }

                // Log export activity
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ExportCsv", $"{dataType}-{businessId}");

                var csvData = await _analyticsExportService.ExportAnalyticsCsvAsync(userId, dataType, fromDate, toDate, businessId);
                
                var fileName = $"analytics-{dataType}-{DateTime.UtcNow:yyyy-MM-dd}.csv";
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV for user {UserId}, data type {DataType}", User.FindFirstValue(ClaimTypes.NameIdentifier), dataType);
                TempData["ErrorMessage"] = "Unable to export data. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Generate shareable dashboard link
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> GenerateShareableLink(string dashboardType, int? businessId = null, DateTime? expiresAt = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    return Json(new { success = false, message = "Shareable links are not available during the trial period." });
                }

                // Validate dashboard type
                if (!new[] { "overview", "business", "benchmarks", "competitors" }.Contains(dashboardType.ToLower()))
                {
                    return Json(new { success = false, message = "Invalid dashboard type specified." });
                }

                // Log activity
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "GenerateShareableLink", $"{dashboardType}-{businessId}");

                var linkToken = await _analyticsExportService.GenerateShareableLinkAsync(userId, dashboardType, businessId, expiresAt);
                
                var shareableUrl = Url.Action("SharedDashboard", "Public", new { token = linkToken }, Request.Scheme, Request.Host.Value);
                
                return Json(new { 
                    success = true, 
                    linkToken = linkToken,
                    shareableUrl = shareableUrl,
                    message = "Shareable link generated successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shareable link for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Json(new { success = false, message = "Unable to generate shareable link. Please try again." });
            }
        }

        /// <summary>
        /// Schedule email report
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ScheduleEmailReport(string reportType, string frequency, int? businessId = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    return Json(new { success = false, message = "Email reports are not available during the trial period." });
                }

                // Validate parameters
                if (!new[] { "overview", "business" }.Contains(reportType.ToLower()))
                {
                    return Json(new { success = false, message = "Invalid report type specified." });
                }

                if (!new[] { "daily", "weekly", "monthly", "once" }.Contains(frequency.ToLower()))
                {
                    return Json(new { success = false, message = "Invalid frequency specified." });
                }

                // Log activity
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ScheduleEmailReport", $"{reportType}-{frequency}-{businessId}");

                var success = await _analyticsExportService.ScheduleEmailReportAsync(userId, reportType, frequency, businessId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Email report scheduled successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to schedule email report. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling email report for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Json(new { success = false, message = "Unable to schedule email report. Please try again." });
            }
        }

        /// <summary>
        /// Send immediate email report
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> SendEmailReport(string reportType, int? businessId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    return Json(new { success = false, message = "Email reports are not available during the trial period." });
                }

                // Validate report type
                if (!new[] { "overview", "business" }.Contains(reportType.ToLower()))
                {
                    return Json(new { success = false, message = "Invalid report type specified." });
                }

                // Log activity
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "SendEmailReport", $"{reportType}-{businessId}");

                var success = await _analyticsExportService.SendEmailReportAsync(userId, reportType, businessId, fromDate, toDate);
                
                if (success)
                {
                    return Json(new { success = true, message = "Email report sent successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to send email report. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email report for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Json(new { success = false, message = "Unable to send email report. Please try again." });
            }
        }

        // ===== COMPARATIVE ANALYSIS ENDPOINTS (Phase 4.2) =====

        /// <summary>
        /// Get comparative analysis dashboard
        /// </summary>
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ComparativeAnalysis(int? businessId = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Comparative analysis is not available during the trial period. Please upgrade to access analytics.";
                    return RedirectToAction("Index", "Subscription");
                }

                // Validate business ownership if specified
                if (businessId.HasValue)
                {
                    var business = await _businessService.GetBusinessByIdAsync(businessId.Value);
                    if (business == null || business.UserId != userId)
                    {
                        TempData["ErrorMessage"] = "Business not found or access denied.";
                        return RedirectToAction("Index");
                    }
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ComparativeAnalysis", businessId?.ToString());

                // Get default comparison (month-over-month)
                var defaultComparison = await _analyticsService.GetPeriodOverPeriodComparisonAsync(userId, businessId, "MonthOverMonth");

                ViewBag.BusinessId = businessId;
                ViewBag.ComparisonTypes = new[] { "WeekOverWeek", "MonthOverMonth", "QuarterOverQuarter", "YearOverYear" };
                ViewBag.Platforms = new[] { "All", "Web", "Mobile", "API" };

                return View(defaultComparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comparative analysis for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "Unable to load comparative analysis. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// API endpoint for comparative analysis data
        /// </summary>
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> ComparativeAnalysisData(string comparisonType = "MonthOverMonth", int? businessId = null, string? platform = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Input validation
                if (!new[] { "WeekOverWeek", "MonthOverMonth", "QuarterOverQuarter", "YearOverYear" }.Contains(comparisonType))
                {
                    _logger.LogWarning("Invalid comparison type: {ComparisonType} for user {UserId}", comparisonType, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid comparison type: {comparisonType}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Invalid comparison type" });
                }

                if (!string.IsNullOrEmpty(platform) && !new[] { "All", "Web", "Mobile", "API" }.Contains(platform))
                {
                    _logger.LogWarning("Invalid platform parameter: {Platform} for user {UserId}", platform, userId);
                    await _analyticsAuditService.LogSuspiciousActivityAsync(userId, "InvalidParameter", $"Invalid platform parameter: {platform}", Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    return BadRequest(new { error = "Invalid platform parameter" });
                }

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Comparative analysis is not available during the trial period." });
                }

                // Validate business ownership if specified
                if (businessId.HasValue)
                {
                    var business = await _businessService.GetBusinessByIdAsync(businessId.Value);
                    if (business == null || business.UserId != userId)
                    {
                        Response.StatusCode = 403;
                        return Json(new { error = "Access denied" });
                    }
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "ComparativeAnalysisData", businessId?.ToString(), platform ?? "All");

                // Normalize platform parameter
                var normalizedPlatform = platform == "All" ? null : platform;

                var comparisonData = await _analyticsService.GetPeriodOverPeriodComparisonAsync(userId, businessId, comparisonType, normalizedPlatform);
                return Json(comparisonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comparative analysis data");
                return Json(new { error = "Unable to load comparison data" });
            }
        }

        /// <summary>
        /// API endpoint for year-over-year comparison
        /// </summary>
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> YearOverYearComparison(int? businessId = null, string? platform = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Comparative analysis is not available during the trial period." });
                }

                // Validate business ownership if specified
                if (businessId.HasValue)
                {
                    var business = await _businessService.GetBusinessByIdAsync(businessId.Value);
                    if (business == null || business.UserId != userId)
                    {
                        Response.StatusCode = 403;
                        return Json(new { error = "Access denied" });
                    }
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "YearOverYearComparison", businessId?.ToString(), platform ?? "All");

                // Normalize platform parameter
                var normalizedPlatform = platform == "All" ? null : platform;

                var comparisonData = await _analyticsService.GetYearOverYearComparisonAsync(userId, businessId, normalizedPlatform);
                return Json(comparisonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading year-over-year comparison data");
                return Json(new { error = "Unable to load comparison data" });
            }
        }

        /// <summary>
        /// API endpoint for custom range comparison
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("ChartDataRateLimit")]
        public async Task<IActionResult> CustomRangeComparison([FromBody] ComparativeAnalysisRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Input validation
                if (request.CurrentPeriodStart >= request.CurrentPeriodEnd)
                {
                    return BadRequest(new { error = "Current period start must be before end" });
                }

                if (request.PreviousPeriodStart >= request.PreviousPeriodEnd)
                {
                    return BadRequest(new { error = "Previous period start must be before end" });
                }

                if ((request.CurrentPeriodEnd - request.CurrentPeriodStart).Days > 365)
                {
                    return BadRequest(new { error = "Period cannot exceed 365 days" });
                }

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Comparative analysis is not available during the trial period." });
                }

                // Validate business ownership if specified
                if (request.BusinessId.HasValue)
                {
                    var business = await _businessService.GetBusinessByIdAsync(request.BusinessId.Value);
                    if (business == null || business.UserId != userId)
                    {
                        Response.StatusCode = 403;
                        return Json(new { error = "Access denied" });
                    }
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "CustomRangeComparison", request.BusinessId?.ToString(), request.Platform ?? "All");

                // Normalize platform parameter
                request.Platform = request.Platform == "All" ? null : request.Platform;

                var comparisonData = await _analyticsService.GetComparativeAnalysisAsync(userId, request);
                return Json(comparisonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading custom range comparison data");
                return Json(new { error = "Unable to load comparison data" });
            }
        }

        /// <summary>
        /// Get user businesses for filter dropdown
        /// </summary>
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> GetUserBusinesses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Block trial users
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    Response.StatusCode = 403;
                    return Json(new { error = "Analytics are not available during the trial period." });
                }

                var businesses = await _businessService.GetUserBusinessesAsync(userId);
                var businessList = businesses?.Select(b => new { id = b.Id, name = b.Name }).ToList();
                if (businessList == null)
                {
                    return Json(new List<object>());
                }

                return Json(businessList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user businesses");
                return Json(new List<object>());
            }
        }
    }
}