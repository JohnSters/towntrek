using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class BusinessReviewResponse
    {
        public int Id { get; set; }

        [Required]
        public int BusinessReviewId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Business owner who responded

        [Required]
        [StringLength(1000, ErrorMessage = "Response cannot exceed 1000 characters")]
        public string Response { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual BusinessReview BusinessReview { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}