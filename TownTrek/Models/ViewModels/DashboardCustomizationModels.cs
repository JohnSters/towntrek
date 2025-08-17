using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    /// <summary>
    /// User's dashboard customization preferences DTO
    /// </summary>
    public class DashboardPreferencesDto
    {
        public int Id { get; set; }
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
        public string LayoutType { get; set; } = "default"; // default, compact, detailed
        public int RefreshInterval { get; set; } = 0; // 0 = disabled, 30, 60, 300 seconds
        
        // Date range preferences
        public string DefaultDateRange { get; set; } = "30"; // 7, 30, 90, 365 days
        
        // Business focus preferences
        public int? FocusedBusinessId { get; set; } // null = show all businesses
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request model for updating dashboard preferences
    /// </summary>
    public class UpdateDashboardPreferencesRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public bool? ShowViewsCard { get; set; }
        public bool? ShowReviewsCard { get; set; }
        public bool? ShowFavoritesCard { get; set; }
        public bool? ShowEngagementCard { get; set; }
        public bool? ShowPerformanceChart { get; set; }
        public bool? ShowViewsChart { get; set; }
        public bool? ShowReviewsChart { get; set; }
        
        public string? LayoutType { get; set; }
        public int? RefreshInterval { get; set; }
        public string? DefaultDateRange { get; set; }
        public int? FocusedBusinessId { get; set; }
    }

    /// <summary>
    /// Saved dashboard view configuration DTO
    /// </summary>
    public class SavedDashboardViewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // View configuration
        public string DateRange { get; set; } = "30";
        public int? BusinessId { get; set; }
        public string LayoutType { get; set; } = "default";
        public string WidgetConfiguration { get; set; } = string.Empty; // JSON string
        
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request model for creating/updating saved views
    /// </summary>
    public class SaveDashboardViewRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string DateRange { get; set; } = "30";
        
        public int? BusinessId { get; set; }
        public string LayoutType { get; set; } = "default";
        public string WidgetConfiguration { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>
    /// Response model for dashboard customization data
    /// </summary>
    public class DashboardCustomizationResponse
    {
        public DashboardPreferencesDto Preferences { get; set; } = new();
        public List<SavedDashboardViewDto> SavedViews { get; set; } = new();
        public SavedDashboardViewDto? CurrentView { get; set; }
        public List<string> AvailableLayouts { get; set; } = new() { "default", "compact", "detailed" };
        public List<string> AvailableDateRanges { get; set; } = new() { "7", "30", "90", "365" };
        public List<int> AvailableRefreshIntervals { get; set; } = new() { 0, 30, 60, 300 };
    }
}
