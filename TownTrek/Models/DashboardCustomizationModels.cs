using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models
{
    /// <summary>
    /// User's dashboard customization preferences stored in database
    /// </summary>
    public class DashboardPreferences
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        // Widget visibility preferences
        public bool ShowViewsCard { get; set; } = true;
        public bool ShowReviewsCard { get; set; } = true;
        public bool ShowFavoritesCard { get; set; } = true;
        public bool ShowEngagementCard { get; set; } = true;
        public bool ShowPerformanceChart { get; set; } = true;
        public bool ShowViewsChart { get; set; } = true;
        public bool ShowReviewsChart { get; set; } = true;
        
        // Layout preferences
        [MaxLength(50)]
        public string LayoutType { get; set; } = "default"; // default, compact, detailed
        public int RefreshInterval { get; set; } = 0; // 0 = disabled, 30, 60, 300 seconds
        
        // Date range preferences
        [MaxLength(10)]
        public string DefaultDateRange { get; set; } = "30"; // 7, 30, 90, 365 days
        
        // Business focus preferences
        public int? FocusedBusinessId { get; set; } // null = show all businesses
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [ForeignKey("FocusedBusinessId")]
        public virtual Business? FocusedBusiness { get; set; }
    }

    /// <summary>
    /// Saved dashboard view configurations
    /// </summary>
    public class SavedDashboardView
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        // View configuration
        [MaxLength(10)]
        public string DateRange { get; set; } = "30";
        public int? BusinessId { get; set; }
        [MaxLength(50)]
        public string LayoutType { get; set; } = "default";
        [Column(TypeName = "nvarchar(max)")]
        public string WidgetConfiguration { get; set; } = string.Empty; // JSON string
        
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }
    }
}
