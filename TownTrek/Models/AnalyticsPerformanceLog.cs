using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models;

public class AnalyticsPerformanceLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MetricType { get; set; } = string.Empty; // PageLoad, DatabaseQuery, ChartRender, UserEngagement

    [Required]
    [MaxLength(200)]
    public string MetricName { get; set; } = string.Empty; // Specific metric name

    public double Value { get; set; } // Duration in milliseconds, count, etc.

    [MaxLength(50)]
    public string? Unit { get; set; } // ms, count, percentage, etc.

    public bool IsSuccess { get; set; } = true;

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    [MaxLength(100)]
    public string? Platform { get; set; } // Web, Mobile, API

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Context { get; set; } // JSON context data

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}
