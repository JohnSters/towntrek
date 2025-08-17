using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    /// <summary>
    /// Model for tracking analytics access and data exports for security and compliance
    /// </summary>
    public class AnalyticsAuditLog
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? BusinessId { get; set; }
        
        [MaxLength(20)]
        public string? Platform { get; set; }
        
        [MaxLength(50)]
        public string? ExportType { get; set; }
        
        [MaxLength(20)]
        public string? Format { get; set; }
        
        [MaxLength(1000)]
        public string? Details { get; set; }
        
        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool IsSuspicious { get; set; } = false;

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
    }
}
