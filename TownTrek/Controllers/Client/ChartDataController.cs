using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces.ClientAnalytics;
using TownTrek.Services.Interfaces.AdminAnalytics;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "PaidClientAccess")] // Only allow paid clients (Basic, Standard, Premium) + Admin
    [Route("Client/[controller]/[action]")]
    public class ChartDataController(
        IClientAnalyticsService analyticsService,
        IChartDataService chartDataService,
        IBusinessService businessService,
        IAnalyticsUsageTracker usageTracker,
        ILogger<ChartDataController> logger) : Controller
    {
        private readonly IClientAnalyticsService _analyticsService = analyticsService;
        private readonly IChartDataService _chartDataService = chartDataService;
        private readonly IBusinessService _businessService = businessService;
        private readonly IAnalyticsUsageTracker _usageTracker = usageTracker;
        private readonly ILogger<ChartDataController> _logger = logger;

        // Views over time data for charts
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ViewsOverTimeData(int days = 30)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                var viewsData = await _analyticsService.GetViewsOverTimeDataAsync(userId, days);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ViewsOverTimeChart", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = viewsData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading views over time data for user {UserId}", userId);
                return Json(new { success = false, message = "Unable to load views data." });
            }
        }

        // Reviews over time data for charts
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ReviewsOverTimeData(int days = 30)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                var reviewsData = await _analyticsService.GetReviewsOverTimeDataAsync(userId, days);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ReviewsOverTimeChart", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = reviewsData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews over time data for user {UserId}", userId);
                return Json(new { success = false, message = "Unable to load reviews data." });
            }
        }

        // Views over time by platform
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ViewsOverTimeByPlatform(int days = 30, string? platform = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                var platformData = await _analyticsService.GetViewsOverTimeByPlatformAsync(userId, days, platform);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "PlatformViewsChart", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = platformData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading platform views data for user {UserId}", userId);
                return Json(new { success = false, message = "Unable to load platform data." });
            }
        }

        // Business view statistics
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> BusinessViewStatistics(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                // Verify user owns this business
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any(b => b.Id == businessId))
                {
                    return Json(new { success = false, message = "Business not found or access denied." });
                }

                var statistics = await _analyticsService.GetBusinessViewStatisticsAsync(businessId, startDate, endDate, platform);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "BusinessViewStatistics", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading business view statistics for business {BusinessId} and user {UserId}", businessId, userId);
                return Json(new { success = false, message = "Unable to load statistics." });
            }
        }

        // Views chart data
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ViewsChartData(int days = 30, string? platform = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                var chartData = await _chartDataService.GetViewsChartDataAsync(userId, days, platform);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ViewsChart", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading views chart data for user {UserId}", userId);
                return Json(new { success = false, message = "Unable to load chart data." });
            }
        }

        // Reviews chart data
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ReviewsChartData(int days = 30)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                var chartData = await _chartDataService.GetReviewsChartDataAsync(userId, days);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ReviewsChart", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews chart data for user {UserId}", userId);
                return Json(new { success = false, message = "Unable to load chart data." });
            }
        }
    }
}
