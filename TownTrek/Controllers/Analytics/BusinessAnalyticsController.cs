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
    public class BusinessAnalyticsController(
        IAnalyticsService analyticsService,
        IBusinessService businessService,
        IAnalyticsUsageTracker usageTracker,
        IComparativeAnalysisService comparativeAnalysisService,
        ILogger<BusinessAnalyticsController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IBusinessService _businessService = businessService;
        private readonly IAnalyticsUsageTracker _usageTracker = usageTracker;
        private readonly IComparativeAnalysisService _comparativeAnalysisService = comparativeAnalysisService;
        private readonly ILogger<BusinessAnalyticsController> _logger = logger;

        // Category benchmarks
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> Benchmarks(string category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    TempData["ErrorMessage"] = "No businesses found.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                var benchmarks = await _analyticsService.GetCategoryBenchmarksAsync(category, userBusinesses.Select(b => b.Id).ToList());
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "CategoryBenchmarks", TimeSpan.FromMilliseconds(100));
                
                return View(benchmarks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category benchmarks for category {Category} and user {UserId}", category, userId);
                TempData["ErrorMessage"] = "Unable to load benchmarks. Please try again later.";
                return RedirectToAction("Index", "ClientAnalytics");
            }
        }

        // Competitor analysis
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> Competitors()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    TempData["ErrorMessage"] = "No businesses found.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                var competitorInsights = await _analyticsService.GetCompetitorInsightsAsync(userBusinesses.Select(b => b.Id).ToList());
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "CompetitorAnalysis", TimeSpan.FromMilliseconds(100));
                
                return View(competitorInsights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor insights for user {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to load competitor analysis. Please try again later.";
                return RedirectToAction("Index", "ClientAnalytics");
            }
        }

        // Comparative analysis
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ComparativeAnalysis(int? businessId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    TempData["ErrorMessage"] = "No businesses found.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                // If no specific business selected, use the first one
                if (!businessId.HasValue)
                {
                    businessId = userBusinesses.First().Id;
                }
                else
                {
                    // Verify user owns this business
                    if (!userBusinesses.Any(b => b.Id == businessId.Value))
                    {
                        TempData["ErrorMessage"] = "Business not found or access denied.";
                        return RedirectToAction("Index", "ClientAnalytics");
                    }
                }

                var comparativeData = await _analyticsService.GetComparativeAnalysisDataAsync(businessId.Value);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ComparativeAnalysis", TimeSpan.FromMilliseconds(100));
                
                return View(comparativeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comparative analysis for business {BusinessId} and user {UserId}", businessId, userId);
                TempData["ErrorMessage"] = "Unable to load comparative analysis. Please try again later.";
                return RedirectToAction("Index", "ClientAnalytics");
            }
        }

        // Comparative analysis data for AJAX calls
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ComparativeAnalysisData(string comparisonType = "MonthOverMonth", int? businessId = null, string? platform = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                // If no specific business selected, use the first one
                if (!businessId.HasValue)
                {
                    businessId = userBusinesses.First().Id;
                }
                else
                {
                    // Verify user owns this business
                    if (!userBusinesses.Any(b => b.Id == businessId.Value))
                    {
                        return Json(new { success = false, message = "Business not found or access denied." });
                    }
                }

                var data = await _analyticsService.GetComparativeAnalysisDataAsync(businessId.Value, comparisonType, platform);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ComparativeAnalysisData", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comparative analysis data for business {BusinessId} and user {UserId}", businessId, userId);
                return Json(new { success = false, message = "Unable to load comparative data." });
            }
        }

        // Year over year comparison
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> YearOverYearComparison(int? businessId = null, string? platform = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                // If no specific business selected, use the first one
                if (!businessId.HasValue)
                {
                    businessId = userBusinesses.First().Id;
                }
                else
                {
                    // Verify user owns this business
                    if (!userBusinesses.Any(b => b.Id == businessId.Value))
                    {
                        return Json(new { success = false, message = "Business not found or access denied." });
                    }
                }

                var comparisonData = await _comparativeAnalysisService.GetYearOverYearComparisonAsync(userId, businessId.Value, platform);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "YearOverYearComparison", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = comparisonData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading year over year comparison for business {BusinessId} and user {UserId}", businessId, userId);
                return Json(new { success = false, message = "Unable to load comparison data." });
            }
        }

        // Custom range comparison
        [EnableRateLimiting("AnalyticsRateLimit")]
        [HttpPost]
        public async Task<IActionResult> CustomRangeComparison([FromBody] ComparativeAnalysisRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                // Verify user owns this business
                if (!userBusinesses.Any(b => b.Id == request.BusinessId))
                {
                    return Json(new { success = false, message = "Business not found or access denied." });
                }

                var comparisonData = await _comparativeAnalysisService.GetCustomRangeComparisonAsync(
                    userId,
                    request.CurrentPeriodStart, 
                    request.CurrentPeriodEnd, 
                    request.PreviousPeriodStart ?? DateTime.MinValue, 
                    request.PreviousPeriodEnd ?? DateTime.MinValue, 
                    request.BusinessId, 
                    request.Platform);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "CustomRangeComparison", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, data = comparisonData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading custom range comparison for business {BusinessId} and user {UserId}", request.BusinessId, userId);
                return Json(new { success = false, message = "Unable to load comparison data." });
            }
        }
    }
}
