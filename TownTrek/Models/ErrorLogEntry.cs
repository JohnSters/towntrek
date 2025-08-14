using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class ErrorLogEntry
    {
        public long Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string ErrorType { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? StackTrace { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(500)]
        public string? RequestPath { get; set; }

        [StringLength(1000)]
        public string? UserAgent { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty;

        public bool IsResolved { get; set; } = false;

        [StringLength(450)]
        public string? ResolvedBy { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ApplicationUser? ResolvedByUser { get; set; }
    }

    public enum ErrorType
    {
        Exception,
        NotFound,
        Unauthorized,
        Argument,
        Api
    }

    public enum ErrorSeverity
    {
        Warning,
        Error,
        Critical
    }
}