using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using System.Security.Claims;

using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "PaidClientAccess")] // Only allow paid clients (Basic, Standard, Premium) + Admin
    [Route("Client/Analytics/[action]")]
    public class ClientAnalyticsController(
        IAnalyticsService analyticsService,
        IAnalyticsCacheService analyticsCacheService,
        ISubscriptionAuthService subscriptionAuthService,
        ITrialService trialService,
        IBusinessService businessService,
        IAnalyticsAuditService analyticsAuditService,
        IAnalyticsUsageTracker usageTracker,
        ILogger<ClientAnalyticsController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsCacheService _analyticsCacheService = analyticsCacheService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ITrialService _trialService = trialService;
        private readonly IBusinessService _businessService = businessService;
        private readonly IAnalyticsAuditService _analyticsAuditService = analyticsAuditService;
        private readonly IAnalyticsUsageTracker _usageTracker = usageTracker;
        private readonly ILogger<ClientAnalyticsController> _logger = logger;

        // Analytics Dashboard - available to all non-trial authenticated clients with active subscription
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> Index()
        {
            var startTime = DateTime.UtcNow;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isSuccess = false;

            try
            {
                // Block trial users from accessing analytics
                var trialStatus = await _trialService.GetTrialStatusAsync(userId);
                if (trialStatus.IsTrialUser && !trialStatus.IsExpired)
                {
                    TempData["ErrorMessage"] = "Analytics are not available during the trial period. Please upgrade to access analytics.";
                    return RedirectToAction("Index", "Subscription");
                }

                // Log analytics access
                await _analyticsAuditService.LogAnalyticsAccessAsync(userId, "AnalyticsDashboard");

                // Track feature usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "AnalyticsDashboard", TimeSpan.FromMilliseconds(100));

                // Fast-path: If user has no businesses, show empty state without invoking full analytics
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (userBusinesses == null || userBusinesses.Count == 0)
                {
                    ViewBag.NoBusinesses = true;
                    isSuccess = true;
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
                isSuccess = true;
                
                return View(analyticsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                try
                {
                    var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                    if (userBusinesses == null || userBusinesses.Count == 0)
                    {
                        ViewBag.NoBusinesses = true;
                        isSuccess = true;
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
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Error in fallback analytics dashboard for user {UserId}", userId);
                }

                TempData["ErrorMessage"] = "Unable to load analytics dashboard. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Analytics dashboard request completed for user {UserId} in {Duration}ms. Success: {IsSuccess}", 
                    userId, duration.TotalMilliseconds, isSuccess);
            }
        }

        // Business-specific analytics
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> Business(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                // Verify user owns this business
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any(b => b.Id == id))
                {
                    TempData["ErrorMessage"] = "Business not found or access denied.";
                    return RedirectToAction("Index");
                }

                var businessAnalytics = await _analyticsService.GetBusinessAnalyticsAsync(id, userId);
                return View(businessAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading business analytics for business {BusinessId} and user {UserId}", id, userId);
                TempData["ErrorMessage"] = "Unable to load business analytics. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        // Get user businesses for analytics
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> GetUserBusinesses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var businesses = await _businessService.GetUserBusinessesAsync(userId);
                return Json(new { success = true, businesses = businesses.Select(b => new { b.Id, b.Name, b.Category }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user businesses for analytics for user {UserId}", userId);
                return Json(new { success = false, message = "Unable to load businesses." });
            }
        }

        // Track usage for analytics
        [EnableRateLimiting("AnalyticsRateLimit")]
        [HttpPost]
        public async Task<IActionResult> TrackUsage([FromBody] UsageTrackingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                await _usageTracker.TrackFeatureUsageAsync(userId, request.FeatureName, TimeSpan.FromMilliseconds(request.Duration));
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking usage for user {UserId} and feature {FeatureName}", userId, request.FeatureName);
                return Json(new { success = false, message = "Unable to track usage." });
            }
        }
    }
}
