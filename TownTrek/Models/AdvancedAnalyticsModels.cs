using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models
{
    /// <summary>
    /// Custom metric definition stored in database
    /// </summary>
    public class CustomMetric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Formula { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public bool IsUserDefined { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastCalculated { get; set; }

        public double CurrentValue { get; set; }

        public double PreviousValue { get; set; }

        public double ChangePercentage { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<CustomMetricGoal> Goals { get; set; } = new List<CustomMetricGoal>();
        public virtual ICollection<CustomMetricDataPoint> HistoricalData { get; set; } = new List<CustomMetricDataPoint>();
    }

    /// <summary>
    /// Historical data points for custom metrics
    /// </summary>
    public class CustomMetricDataPoint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomMetricId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double Value { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Context { get; set; } // JSON context data

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CustomMetricId")]
        public virtual CustomMetric CustomMetric { get; set; } = null!;
    }

    /// <summary>
    /// Goals for custom metrics
    /// </summary>
    public class CustomMetricGoal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomMetricId { get; set; }

        [Required]
        public double TargetValue { get; set; }

        [Required]
        public DateTime TargetDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? AchievedDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "In Progress"; // In Progress, Achieved, Behind Schedule, At Risk

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomMetricId")]
        public virtual CustomMetric CustomMetric { get; set; } = null!;
    }

    /// <summary>
    /// Anomaly detection results
    /// </summary>
    public class AnomalyDetection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int? BusinessId { get; set; }

        [Required]
        [MaxLength(100)]
        public string MetricType { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double ActualValue { get; set; }

        [Required]
        public double ExpectedValue { get; set; }

        [Required]
        public double Deviation { get; set; }

        [Required]
        public double DeviationPercentage { get; set; }

        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "Medium";

        [MaxLength(1000)]
        public string? Context { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? PossibleCauses { get; set; } // JSON array of possible causes

        [Column(TypeName = "nvarchar(max)")]
        public string? Recommendations { get; set; } // JSON array of recommendations

        public bool IsAcknowledged { get; set; } = false;

        public DateTime? AcknowledgedAt { get; set; }

        [MaxLength(450)]
        public string? AcknowledgedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }

        [ForeignKey("AcknowledgedBy")]
        public virtual ApplicationUser? AcknowledgedByUser { get; set; }
    }

    /// <summary>
    /// Predictive analytics forecasts
    /// </summary>
    public class PredictiveForecast
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int? BusinessId { get; set; }

        [Required]
        [MaxLength(100)]
        public string MetricType { get; set; } = string.Empty;

        [Required]
        public DateTime ForecastDate { get; set; }

        [Required]
        public double PredictedValue { get; set; }

        [Required]
        public double LowerBound { get; set; }

        [Required]
        public double UpperBound { get; set; }

        [Required]
        public double Confidence { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }
    }

    /// <summary>
    /// Seasonal pattern analysis
    /// </summary>
    public class SeasonalPattern
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int? BusinessId { get; set; }

        [Required]
        [MaxLength(100)]
        public string MetricType { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PatternType { get; set; } = string.Empty; // Weekly, Monthly, Quarterly

        [Required]
        [MaxLength(20)]
        public string Period { get; set; } = string.Empty; // Monday, January, Q1, etc.

        [Required]
        public double AverageValue { get; set; }

        [Required]
        public double Deviation { get; set; }

        [Required]
        public double Strength { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }
    }
}
