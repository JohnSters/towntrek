using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    /// <summary>
    /// Response model for predictive analytics forecasts
    /// </summary>
    public class PredictiveAnalyticsResponse
    {
        public List<ForecastData> ViewsForecast { get; set; } = new();
        public List<ForecastData> EngagementForecast { get; set; } = new();
        public List<ForecastData> RevenueForecast { get; set; } = new();
        public SeasonalPattern SeasonalPatterns { get; set; } = new();
        public GrowthPrediction GrowthPrediction { get; set; } = new();
        public DateTime ForecastGeneratedAt { get; set; } = DateTime.UtcNow;
        public int ForecastDays { get; set; } = 30;
        public double ConfidenceLevel { get; set; } = 0.85;
    }

    /// <summary>
    /// Individual forecast data point
    /// </summary>
    public class ForecastData
    {
        public DateTime Date { get; set; }
        public double PredictedValue { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Seasonal pattern analysis
    /// </summary>
    public class SeasonalPattern
    {
        public Dictionary<string, double> WeeklyPattern { get; set; } = new();
        public Dictionary<string, double> MonthlyPattern { get; set; } = new();
        public Dictionary<string, double> QuarterlyPattern { get; set; } = new();
        public double SeasonalityStrength { get; set; }
        public string PrimarySeason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Growth prediction analysis
    /// </summary>
    public class GrowthPrediction
    {
        public double ShortTermGrowthRate { get; set; } // Next 30 days
        public double MediumTermGrowthRate { get; set; } // Next 90 days
        public double LongTermGrowthRate { get; set; } // Next 365 days
        public DateTime PredictedPeakDate { get; set; }
        public double PredictedPeakValue { get; set; }
        public List<string> GrowthFactors { get; set; } = new();
        public List<string> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// Response model for anomaly detection
    /// </summary>
    public class AnomalyDetectionResponse
    {
        public List<AnomalyData> Anomalies { get; set; } = new();
        public AnomalySummary Summary { get; set; } = new();
        public Dictionary<string, double> BaselineMetrics { get; set; } = new();
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public int AnalysisDays { get; set; } = 30;
    }

    /// <summary>
    /// Individual anomaly data point
    /// </summary>
    public class AnomalyData
    {
        public DateTime Date { get; set; }
        public string MetricType { get; set; } = string.Empty; // Views, Reviews, Engagement, etc.
        public double ActualValue { get; set; }
        public double ExpectedValue { get; set; }
        public double Deviation { get; set; }
        public double DeviationPercentage { get; set; }
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string? Context { get; set; }
        public List<string> PossibleCauses { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Summary of anomaly analysis
    /// </summary>
    public class AnomalySummary
    {
        public int TotalAnomalies { get; set; }
        public int CriticalAnomalies { get; set; }
        public int HighAnomalies { get; set; }
        public int MediumAnomalies { get; set; }
        public int LowAnomalies { get; set; }
        public double AverageDeviation { get; set; }
        public string MostAffectedMetric { get; set; } = string.Empty;
        public DateTime MostRecentAnomaly { get; set; }
    }

    /// <summary>
    /// Response model for custom metrics
    /// </summary>
    public class CustomMetricsResponse
    {
        public List<CustomMetric> UserMetrics { get; set; } = new();
        public List<CustomMetric> SystemMetrics { get; set; } = new();
        public List<GoalTracking> GoalProgress { get; set; } = new();
        public CustomMetricsSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Custom metric definition and calculation
    /// </summary>
    public class CustomMetric
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Views, Engagement, Revenue, etc.
        public bool IsUserDefined { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastCalculated { get; set; }
        public double CurrentValue { get; set; }
        public double PreviousValue { get; set; }
        public double ChangePercentage { get; set; }
        public List<MetricDataPoint> HistoricalData { get; set; } = new();
    }

    /// <summary>
    /// Historical data point for custom metrics
    /// </summary>
    public class MetricDataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// Goal tracking for custom metrics
    /// </summary>
    public class GoalTracking
    {
        public int MetricId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public double TargetValue { get; set; }
        public double CurrentValue { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime? AchievedDate { get; set; }
        public string Status { get; set; } = "In Progress"; // In Progress, Achieved, Behind Schedule, At Risk
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Summary of custom metrics
    /// </summary>
    public class CustomMetricsSummary
    {
        public int TotalMetrics { get; set; }
        public int UserDefinedMetrics { get; set; }
        public int SystemMetrics { get; set; }
        public int ActiveGoals { get; set; }
        public int AchievedGoals { get; set; }
        public double AverageGoalProgress { get; set; }
        public List<string> TopPerformingMetrics { get; set; } = new();
        public List<string> MetricsNeedingAttention { get; set; } = new();
    }

    /// <summary>
    /// Request model for creating custom metrics
    /// </summary>
    public class CreateCustomMetricRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Formula { get; set; } = string.Empty;

        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for setting goals
    /// </summary>
    public class SetGoalRequest
    {
        [Required]
        public int MetricId { get; set; }

        [Required]
        public double TargetValue { get; set; }

        [Required]
        public DateTime TargetDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
