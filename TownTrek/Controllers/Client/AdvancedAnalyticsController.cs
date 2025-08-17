using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize]
    [Route("client/advanced-analytics")]
    public class AdvancedAnalyticsController : Controller
    {
        private readonly IAdvancedAnalyticsService _advancedAnalyticsService;
        private readonly ILogger<AdvancedAnalyticsController> _logger;

        public AdvancedAnalyticsController(
            IAdvancedAnalyticsService advancedAnalyticsService,
            ILogger<AdvancedAnalyticsController> logger)
        {
            _advancedAnalyticsService = advancedAnalyticsService;
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

        [HttpGet("predictive")]
        public async Task<IActionResult> PredictiveAnalytics(int forecastDays = 30)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
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

        [HttpGet("anomalies")]
        public async Task<IActionResult> Anomalies(int analysisDays = 30)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _advancedAnalyticsService.DetectAnomaliesAsync(userId, analysisDays);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting anomalies");
                return BadRequest(new { error = "Failed to detect anomalies" });
            }
        }

        [HttpGet("custom-metrics")]
        public async Task<IActionResult> CustomMetrics()
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

        [HttpPost("custom-metrics")]
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

        [HttpPost("anomalies/{anomalyId}/acknowledge")]
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
    }
}
