using TownTrek.Models;

namespace TownTrek.Services.Interfaces.ClientAnalytics
{
    /// <summary>
    /// Service for managing dashboard customization features
    /// </summary>
    public interface IDashboardCustomizationService
    {
        /// <summary>
        /// Get user's dashboard customization data
        /// </summary>
        Task<DashboardCustomizationResponse> GetDashboardCustomizationAsync(string userId);

        /// <summary>
        /// Get user's dashboard preferences
        /// </summary>
        Task<DashboardPreferences> GetUserPreferencesAsync(string userId);

        /// <summary>
        /// Update user's dashboard preferences
        /// </summary>
        Task<DashboardPreferences> UpdatePreferencesAsync(UpdateDashboardPreferencesRequest request);

        /// <summary>
        /// Get user's saved dashboard views
        /// </summary>
        Task<List<SavedDashboardView>> GetSavedViewsAsync(string userId);

        /// <summary>
        /// Save a new dashboard view
        /// </summary>
        Task<SavedDashboardView> SaveDashboardViewAsync(SaveDashboardViewRequest request, string userId);

        /// <summary>
        /// Update an existing saved dashboard view
        /// </summary>
        Task<SavedDashboardView> UpdateSavedViewAsync(int viewId, SaveDashboardViewRequest request, string userId);

        /// <summary>
        /// Delete a saved dashboard view
        /// </summary>
        Task<bool> DeleteSavedViewAsync(int viewId, string userId);

        /// <summary>
        /// Set a saved view as default
        /// </summary>
        Task<bool> SetDefaultViewAsync(int viewId, string userId);

        /// <summary>
        /// Load a saved dashboard view
        /// </summary>
        Task<SavedDashboardView?> LoadSavedViewAsync(int viewId, string userId);

        /// <summary>
        /// Reset user's dashboard to default settings
        /// </summary>
        Task<bool> ResetToDefaultAsync(string userId);

        /// <summary>
        /// Get available layout options
        /// </summary>
        List<string> GetAvailableLayouts();

        /// <summary>
        /// Get available date range options
        /// </summary>
        List<string> GetAvailableDateRanges();

        /// <summary>
        /// Get available refresh interval options
        /// </summary>
        List<int> GetAvailableRefreshIntervals();
    }

    /// <summary>
    /// Response model for dashboard customization data
    /// </summary>
    public class DashboardCustomizationResponse
    {
        public DashboardPreferences Preferences { get; set; } = new();
        public List<SavedDashboardView> SavedViews { get; set; } = new();
        public SavedDashboardView? CurrentView { get; set; }
        public List<string> AvailableLayouts { get; set; } = new() { "default", "compact", "detailed" };
        public List<string> AvailableDateRanges { get; set; } = new() { "7", "30", "90", "365" };
        public List<int> AvailableRefreshIntervals { get; set; } = new() { 0, 30, 60, 300 };
    }

    /// <summary>
    /// Request model for updating dashboard preferences
    /// </summary>
    public class UpdateDashboardPreferencesRequest
    {
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
    /// Request model for creating/updating saved views
    /// </summary>
    public class SaveDashboardViewRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DateRange { get; set; } = "30";
        public int? BusinessId { get; set; }
        public string LayoutType { get; set; } = "default";
        public string WidgetConfiguration { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }
}
