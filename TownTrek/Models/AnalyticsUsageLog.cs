using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models;

public class AnalyticsUsageLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UsageType { get; set; } = string.Empty; // FeatureUsage, PageView, ChartInteraction, DataExport, FilterUsage

    [Required]
    [MaxLength(200)]
    public string FeatureName { get; set; } = string.Empty; // Specific feature name

    public double? Duration { get; set; } // Duration in milliseconds

    [MaxLength(100)]
    public string? InteractionType { get; set; } // click, hover, export, filter, etc.

    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; } // JSON metadata

    [MaxLength(100)]
    public string? Platform { get; set; } // Web, Mobile, API

    [MaxLength(100)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(100)]
    public string? SessionId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}
