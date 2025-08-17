using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class AnalyticsEmailReport
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } = string.Empty; // "Overview", "Business", "Weekly", "Monthly"

        [Required]
        [StringLength(20)]
        public string Frequency { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly", "Once"

        public int? BusinessId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSentAt { get; set; }

        public DateTime? NextScheduledAt { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? EmailAddress { get; set; }

        [StringLength(1000)]
        public string? CustomMessage { get; set; }

        public int SendCount { get; set; } = 0;

        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Business? Business { get; set; }
    }
}
