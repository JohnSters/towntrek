using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class BusinessViewLog
    {
        public int Id { get; set; }

        [Required]
        public int BusinessId { get; set; }

        public string? UserId { get; set; } // NULL for anonymous views

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(500)]
        public string? Referrer { get; set; }

        [StringLength(100)]
        public string? SessionId { get; set; }

        [StringLength(20)]
        public string Platform { get; set; } = "Web"; // "Web", "Mobile", "API"

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Business Business { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
