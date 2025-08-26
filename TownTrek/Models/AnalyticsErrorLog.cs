using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models;

public class AnalyticsErrorLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ErrorType { get; set; } = string.Empty; // Database, Chart, Cache, General

    [Required]
    [MaxLength(200)]
    public string ErrorCategory { get; set; } = string.Empty; // Specific error category

    [Required]
    [MaxLength(500)]
    public string ErrorMessage { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? StackTrace { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Context { get; set; } // JSON context data

    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical

    public bool IsResolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(450)]
    public string? ResolvedBy { get; set; }

    [MaxLength(100)]
    public string? Platform { get; set; } // Web, Mobile, API

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }

    [ForeignKey("ResolvedBy")]
    public virtual ApplicationUser? ResolvedByUser { get; set; }
}
