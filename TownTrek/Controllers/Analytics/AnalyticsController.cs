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
        ISubscriptionAuthService subscriptionAuthService,
        ITrialService trialService,
        IBusinessService businessService,
        ILogger<AnalyticsController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
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
                
                var analyticsModel = await _analyticsService.GetClientAnalyticsAsync(userId);
                
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

                var businessAnalytics = await _analyticsService.GetBusinessAnalyticsAsync(id, userId);
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

                var data = await _analyticsService.GetViewsOverTimeAsync(userId, days);
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

                var data = await _analyticsService.GetReviewsOverTimeAsync(userId, days);
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
                var business = await _businessService.GetBusinessAsync(businessId);
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

                var insights = await _analyticsService.GetCompetitorInsightsAsync(userId);
                return View(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor insights");
                TempData["ErrorMessage"] = "Unable to load competitor insights. Please try again.";
                return RedirectToAction("Index");
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