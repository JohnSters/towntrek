using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class AnalyticsShareableLink
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LinkToken { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DashboardType { get; set; } = string.Empty; // "Overview", "Business", "Benchmarks", "Competitors"

        public int? BusinessId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int AccessCount { get; set; } = 0;

        public DateTime? LastAccessedAt { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Business? Business { get; set; }
    }
}
