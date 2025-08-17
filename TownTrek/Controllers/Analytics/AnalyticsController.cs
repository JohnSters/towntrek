using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Services.Interfaces;

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
        ILogger<AnalyticsController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsCacheService _analyticsCacheService = analyticsCacheService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ITrialService _trialService = trialService;
        private readonly IBusinessService _businessService = businessService;
        private readonly ILogger<AnalyticsController> _logger = logger;

        // Analytics Dashboard - available to all non-trial authenticated clients with active subscription
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
        public async Task<IActionResult> ViewsOverTimeData(int days = 30)
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
        public async Task<IActionResult> ReviewsOverTimeData(int days = 30)
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
        public async Task<IActionResult> ViewsChartData(int days = 30, string? platform = null)
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
        public async Task<IActionResult> ReviewsChartData(int days = 30)
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
    }
}