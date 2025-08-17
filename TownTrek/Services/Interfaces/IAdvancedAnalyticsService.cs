using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for advanced analytics features including predictive analytics, anomaly detection, and custom metrics
    /// </summary>
    public interface IAdvancedAnalyticsService
    {
        #region Predictive Analytics

        /// <summary>
        /// Generates predictive analytics forecasts for a user's businesses
        /// </summary>
        Task<PredictiveAnalyticsResponse> GetPredictiveAnalyticsAsync(string userId, int forecastDays = 30);

        /// <summary>
        /// Generates predictive analytics for a specific business
        /// </summary>
        Task<PredictiveAnalyticsResponse> GetBusinessPredictiveAnalyticsAsync(int businessId, string userId, int forecastDays = 30);

        /// <summary>
        /// Analyzes seasonal patterns in business data
        /// </summary>
        Task<SeasonalPattern> AnalyzeSeasonalPatternsAsync(string userId, string metricType = "Views", int analysisDays = 365);

        /// <summary>
        /// Generates growth predictions for business metrics
        /// </summary>
        Task<GrowthPrediction> GenerateGrowthPredictionAsync(string userId, string metricType = "Views");

        #endregion

        #region Anomaly Detection

        /// <summary>
        /// Detects anomalies in business analytics data
        /// </summary>
        Task<AnomalyDetectionResponse> DetectAnomaliesAsync(string userId, int analysisDays = 30);

        /// <summary>
        /// Detects anomalies for a specific business
        /// </summary>
        Task<AnomalyDetectionResponse> DetectBusinessAnomaliesAsync(int businessId, string userId, int analysisDays = 30);

        /// <summary>
        /// Acknowledges an anomaly detection
        /// </summary>
        Task<bool> AcknowledgeAnomalyAsync(int anomalyId, string userId);

        /// <summary>
        /// Gets unacknowledged anomalies for a user
        /// </summary>
        Task<List<AnomalyData>> GetUnacknowledgedAnomaliesAsync(string userId);

        #endregion

        #region Custom Metrics

        /// <summary>
        /// Gets all custom metrics for a user
        /// </summary>
        Task<CustomMetricsResponse> GetCustomMetricsAsync(string userId);

        /// <summary>
        /// Creates a new custom metric
        /// </summary>
        Task<CustomMetric> CreateCustomMetricAsync(CreateCustomMetricRequest request, string userId);

        /// <summary>
        /// Updates an existing custom metric
        /// </summary>
        Task<CustomMetric> UpdateCustomMetricAsync(int metricId, CreateCustomMetricRequest request, string userId);

        /// <summary>
        /// Deletes a custom metric
        /// </summary>
        Task<bool> DeleteCustomMetricAsync(int metricId, string userId);

        /// <summary>
        /// Calculates the current value of a custom metric
        /// </summary>
        Task<double> CalculateCustomMetricAsync(int metricId, string userId);

        /// <summary>
        /// Sets a goal for a custom metric
        /// </summary>
        Task<GoalTracking> SetGoalAsync(SetGoalRequest request, string userId);

        /// <summary>
        /// Updates goal progress for all custom metrics
        /// </summary>
        Task UpdateGoalProgressAsync(string userId);

        /// <summary>
        /// Gets system-defined metrics
        /// </summary>
        Task<List<CustomMetric>> GetSystemMetricsAsync();

        #endregion

        #region Data Management

        /// <summary>
        /// Runs daily advanced analytics calculations
        /// </summary>
        Task RunDailyAdvancedAnalyticsAsync();

        /// <summary>
        /// Cleans up old advanced analytics data
        /// </summary>
        Task CleanupOldDataAsync(int retentionDays = 730);

        /// <summary>
        /// Validates custom metric formulas
        /// </summary>
        Task<bool> ValidateCustomMetricFormulaAsync(string formula, string userId);

        #endregion
    }
}
