using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class TrialAuditLog
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Action { get; set; } = string.Empty; // Started, Checked, Expired, Converted, Tampered
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? Details { get; set; }
        
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
        
        // Security fields
        public long? TrialStartTicks { get; set; }
        public long? TrialEndTicks { get; set; }
        public int? DaysRemaining { get; set; }
        public int? CheckCount { get; set; }
        
        // Navigation property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}