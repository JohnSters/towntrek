using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Analytics
{
    [Authorize]
    [Route("Client/[controller]/[action]")]
    public class AnalyticsController(
        IAnalyticsService analyticsService,
        ISubscriptionAuthService subscriptionAuthService,
        ILogger<AnalyticsController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ILogger<AnalyticsController> _logger = logger;

        // Analytics Dashboard - Basic analytics for Standard+ subscribers
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Log subscription validation for debugging
                var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                _logger.LogInformation("Analytics access attempt - User: {UserId}, HasActiveSubscription: {HasActive}, IsPaymentValid: {PaymentValid}, Tier: {Tier}", 
                    userId, authResult.HasActiveSubscription, authResult.IsPaymentValid, authResult.SubscriptionTier?.Name ?? "None");
                
                // Check if user can access any level of analytics
                var canAccessBasicAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                _logger.LogInformation("BasicAnalytics access check for user {UserId}: {CanAccess}", userId, canAccessBasicAnalytics);
                
                if (!canAccessBasicAnalytics)
                {
                    TempData["ErrorMessage"] = "Analytics access requires an active subscription. Please check your subscription status.";
                    return RedirectToAction("Plans", "Public");
                }

                var analyticsModel = await _analyticsService.GetClientAnalyticsAsync(userId);
                return View(analyticsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "Unable to load analytics data. Please try again.";
                return RedirectToAction("Dashboard", "Client");
            }
        }

        // Business-specific analytics - Basic analytics for Standard+ subscribers
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> Business(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Check if user can access basic analytics
                var canAccessBasicAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                if (!canAccessBasicAnalytics)
                {
                    TempData["ErrorMessage"] = "Analytics access requires a Standard or Premium subscription.";
                    return RedirectToAction("Index");
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
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> ViewsOverTimeData(int days = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Check if user can access basic analytics
                var canAccessBasicAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                if (!canAccessBasicAnalytics)
                {
                    return Json(new { error = "Analytics access requires a Standard or Premium subscription." });
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
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> ReviewsOverTimeData(int days = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Check if user can access basic analytics
                var canAccessBasicAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                if (!canAccessBasicAnalytics)
                {
                    return Json(new { error = "Analytics access requires a Standard or Premium subscription." });
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

        // Premium analytics features - Advanced analytics for Premium subscribers only
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> Benchmarks(string category)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Check if user can access advanced analytics
                var canAccessAdvancedAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!canAccessAdvancedAnalytics)
                {
                    TempData["ErrorMessage"] = "Advanced analytics requires a Premium subscription. Please upgrade to access this feature.";
                    return RedirectToAction("Index");
                }

                var benchmarks = await _analyticsService.GetCategoryBenchmarksAsync(userId, category);
                
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

        // Premium competitor insights - Advanced analytics for Premium subscribers only
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> Competitors()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Check if user can access advanced analytics
                var canAccessAdvancedAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!canAccessAdvancedAnalytics)
                {
                    TempData["ErrorMessage"] = "Advanced analytics requires a Premium subscription. Please upgrade to access this feature.";
                    return RedirectToAction("Index");
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
        [RequireActiveSubscription(allowFreeTier: false)]
        public async Task<IActionResult> Debug()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            var tier = await _subscriptionAuthService.GetUserSubscriptionTierAsync(userId);
            var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
            var canAccessBasic = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
            var canAccessAdvanced = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");

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