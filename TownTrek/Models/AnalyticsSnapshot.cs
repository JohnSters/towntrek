using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models
{
    /// <summary>
    /// Represents a daily snapshot of business analytics data for historical tracking and trend analysis
    /// </summary>
    public class AnalyticsSnapshot
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The business this snapshot belongs to
        /// </summary>
        [Required]
        public int BusinessId { get; set; }

        /// <summary>
        /// The date this snapshot was taken (date only, no time)
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime SnapshotDate { get; set; }

        /// <summary>
        /// Total views for this business on the snapshot date
        /// </summary>
        [Required]
        public int TotalViews { get; set; } = 0;

        /// <summary>
        /// Total reviews for this business on the snapshot date
        /// </summary>
        [Required]
        public int TotalReviews { get; set; } = 0;

        /// <summary>
        /// Total favorites for this business on the snapshot date
        /// </summary>
        [Required]
        public int TotalFavorites { get; set; } = 0;

        /// <summary>
        /// Average rating for this business on the snapshot date
        /// </summary>
        [Column(TypeName = "decimal(3,2)")]
        public decimal? AverageRating { get; set; }

        /// <summary>
        /// Calculated engagement score (0-100) based on views, reviews, and favorites
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? EngagementScore { get; set; }

        /// <summary>
        /// When this snapshot was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Business Business { get; set; } = null!;
    }
}
