using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class BusinessReview
    {
        public int Id { get; set; }

        [Required]
        public int BusinessId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; } // 1-5 stars

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = true; // Auto-approve for now
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Business Business { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class FavoriteBusiness
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int BusinessId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Business Business { get; set; } = null!;
    }
}
