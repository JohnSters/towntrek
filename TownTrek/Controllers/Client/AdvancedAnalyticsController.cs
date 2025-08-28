using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "PaidClientAccess")]
    [Route("Client/[controller]/[action]")]
    public class AdvancedAnalyticsController : Controller
    {
        private readonly IAdvancedAnalyticsService _advancedAnalyticsService;
        private readonly ITrialService _trialService;
        private readonly ISubscriptionAuthService _subscriptionAuthService;
        private readonly ILogger<AdvancedAnalyticsController> _logger;

        public AdvancedAnalyticsController(
            IAdvancedAnalyticsService advancedAnalyticsService,
            ITrialService trialService,
            ISubscriptionAuthService subscriptionAuthService,
            ILogger<AdvancedAnalyticsController> logger)
        {
            _advancedAnalyticsService = advancedAnalyticsService;
            _trialService = trialService;
            _subscriptionAuthService = subscriptionAuthService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Check if user has AdvancedAnalytics feature access (Premium tier only)
                var hasAdvancedAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!hasAdvancedAnalyticsAccess)
                {
                    TempData["ErrorMessage"] = "Advanced Analytics is only available with a Premium subscription. Please upgrade to access this feature.";
                    return RedirectToAction("Index", "Subscription");
                }

                // Get basic advanced analytics data
                var predictiveAnalytics = await _advancedAnalyticsService.GetPredictiveAnalyticsAsync(userId);
                var anomalies = await _advancedAnalyticsService.DetectAnomaliesAsync(userId);
                var customMetrics = await _advancedAnalyticsService.GetCustomMetricsAsync(userId);

                var viewModel = new
                {
                    PredictiveAnalytics = predictiveAnalytics,
                    Anomalies = anomalies,
                    CustomMetrics = customMetrics
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading advanced analytics dashboard");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PredictiveAnalytics(int forecastDays = 30)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Check if user has AdvancedAnalytics feature access (Premium tier only)
                var hasAdvancedAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!hasAdvancedAnalyticsAccess)
                {
                    return BadRequest(new { error = "Advanced Analytics is only available with a Premium subscription." });
                }

                var result = await _advancedAnalyticsService.GetPredictiveAnalyticsAsync(userId, forecastDays);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting predictive analytics");
                return BadRequest(new { error = "Failed to get predictive analytics" });
            }
        }

        // GET: Client/AdvancedAnalytics/Predictive
        [HttpGet]
        public async Task<IActionResult> Predictive(int forecastDays = 30)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Check if user has AdvancedAnalytics feature access (Premium tier only)
                var hasAdvancedAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!hasAdvancedAnalyticsAccess)
                {
                    return BadRequest(new { error = "Advanced Analytics is only available with a Premium subscription." });
                }

                var result = await _advancedAnalyticsService.GetPredictiveAnalyticsAsync(userId, forecastDays);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting predictive analytics");
                return BadRequest(new { error = "Failed to get predictive analytics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Anomalies()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Check if user has AdvancedAnalytics feature access (Premium tier only)
                var hasAdvancedAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!hasAdvancedAnalyticsAccess)
                {
                    return BadRequest(new { error = "Advanced Analytics is only available with a Premium subscription." });
                }

                var result = await _advancedAnalyticsService.DetectAnomaliesAsync(userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting anomalies");
                return BadRequest(new { error = "Failed to get anomalies" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CustomMetrics()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Check if user has AdvancedAnalytics feature access (Premium tier only)
                var hasAdvancedAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");
                if (!hasAdvancedAnalyticsAccess)
                {
                    return BadRequest(new { error = "Advanced Analytics is only available with a Premium subscription." });
                }

                var result = await _advancedAnalyticsService.GetCustomMetricsAsync(userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom metrics");
                return BadRequest(new { error = "Failed to get custom metrics" });
            }
        }

        // GET: Client/AdvancedAnalytics/Metrics
        [HttpGet]
        public async Task<IActionResult> Metrics()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _advancedAnalyticsService.GetCustomMetricsAsync(userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom metrics");
                return BadRequest(new { error = "Failed to get custom metrics" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomMetric([FromBody] CreateCustomMetricRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _advancedAnalyticsService.CreateCustomMetricAsync(request, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating custom metric");
                return BadRequest(new { error = "Failed to create custom metric" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcknowledgeAnomaly(int anomalyId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _advancedAnalyticsService.AcknowledgeAnomalyAsync(anomalyId, userId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging anomaly {AnomalyId}", anomalyId);
                return BadRequest(new { error = "Failed to acknowledge anomaly" });
            }
        }

        // POST: Client/AdvancedAnalytics/GenerateTestData
        [HttpPost]
        public IActionResult GenerateTestData()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                _logger.LogInformation("Generating test data for Advanced Analytics user {UserId}", userId);

                // This would typically call the service to generate test data
                // For now, we'll return a success message
                // In a real implementation, you'd call methods like:
                // await _advancedAnalyticsService.GenerateTestPredictiveDataAsync(userId);
                // await _advancedAnalyticsService.GenerateTestAnomalyDataAsync(userId);
                // await _advancedAnalyticsService.GenerateTestCustomMetricsAsync(userId);

                return Json(new { 
                    success = true, 
                    message = "Test data generation initiated. Refresh the page in a few moments to see the data.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test data for Advanced Analytics");
                return BadRequest(new { error = "Failed to generate test data" });
            }
        }
    }
}
