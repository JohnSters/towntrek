using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces.ClientAnalytics;

namespace TownTrek.Services
{
    /// <summary>
    /// Service for managing dashboard customization features
    /// </summary>
    public class DashboardCustomizationService : IDashboardCustomizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardCustomizationService> _logger;

        public DashboardCustomizationService(
            ApplicationDbContext context,
            ILogger<DashboardCustomizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardCustomizationResponse> GetDashboardCustomizationAsync(string userId)
        {
            try
            {
                var preferences = await GetUserPreferencesAsync(userId);
                var savedViews = await GetSavedViewsAsync(userId);
                var currentView = savedViews.FirstOrDefault(v => v.IsDefault);

                return new DashboardCustomizationResponse
                {
                    Preferences = preferences,
                    SavedViews = savedViews,
                    CurrentView = currentView,
                    AvailableLayouts = GetAvailableLayouts(),
                    AvailableDateRanges = GetAvailableDateRanges(),
                    AvailableRefreshIntervals = GetAvailableRefreshIntervals()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard customization for user {UserId}", userId);
                throw;
            }
        }

        public async Task<DashboardPreferences> GetUserPreferencesAsync(string userId)
        {
            try
            {
                var preferences = await _context.DashboardPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (preferences == null)
                {
                    // Create default preferences
                    preferences = new DashboardPreferences
                    {
                        UserId = userId,
                        ShowViewsCard = true,
                        ShowReviewsCard = true,
                        ShowFavoritesCard = true,
                        ShowEngagementCard = true,
                        ShowPerformanceChart = true,
                        ShowViewsChart = true,
                        ShowReviewsChart = true,
                        LayoutType = "default",
                        RefreshInterval = 0,
                        DefaultDateRange = "30",
                        FocusedBusinessId = null,
                        LastUpdated = DateTime.UtcNow
                    };

                    _context.DashboardPreferences.Add(preferences);
                    await _context.SaveChangesAsync();
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences for user {UserId}", userId);
                throw;
            }
        }

        public async Task<DashboardPreferences> UpdatePreferencesAsync(UpdateDashboardPreferencesRequest request)
        {
            try
            {
                var preferences = await GetUserPreferencesAsync(request.UserId);

                // Update only the properties that are provided
                if (request.ShowViewsCard.HasValue)
                    preferences.ShowViewsCard = request.ShowViewsCard.Value;
                if (request.ShowReviewsCard.HasValue)
                    preferences.ShowReviewsCard = request.ShowReviewsCard.Value;
                if (request.ShowFavoritesCard.HasValue)
                    preferences.ShowFavoritesCard = request.ShowFavoritesCard.Value;
                if (request.ShowEngagementCard.HasValue)
                    preferences.ShowEngagementCard = request.ShowEngagementCard.Value;
                if (request.ShowPerformanceChart.HasValue)
                    preferences.ShowPerformanceChart = request.ShowPerformanceChart.Value;
                if (request.ShowViewsChart.HasValue)
                    preferences.ShowViewsChart = request.ShowViewsChart.Value;
                if (request.ShowReviewsChart.HasValue)
                    preferences.ShowReviewsChart = request.ShowReviewsChart.Value;
                if (!string.IsNullOrEmpty(request.LayoutType))
                    preferences.LayoutType = request.LayoutType;
                if (request.RefreshInterval.HasValue)
                    preferences.RefreshInterval = request.RefreshInterval.Value;
                if (!string.IsNullOrEmpty(request.DefaultDateRange))
                    preferences.DefaultDateRange = request.DefaultDateRange;
                if (request.FocusedBusinessId.HasValue)
                    preferences.FocusedBusinessId = request.FocusedBusinessId.Value;

                preferences.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preferences for user {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<List<SavedDashboardView>> GetSavedViewsAsync(string userId)
        {
            try
            {
                return await _context.SavedDashboardViews
                    .Where(v => v.UserId == userId)
                    .OrderByDescending(v => v.IsDefault)
                    .ThenByDescending(v => v.LastUsed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved views for user {UserId}", userId);
                return new List<SavedDashboardView>();
            }
        }

        public async Task<SavedDashboardView> SaveDashboardViewAsync(SaveDashboardViewRequest request, string userId)
        {
            try
            {
                // If this is set as default, unset other default views
                if (request.IsDefault)
                {
                    var existingDefaults = await _context.SavedDashboardViews
                        .Where(v => v.UserId == userId && v.IsDefault)
                        .ToListAsync();
                    
                    foreach (var existing in existingDefaults)
                    {
                        existing.IsDefault = false;
                    }
                }

                var savedView = new SavedDashboardView
                {
                    UserId = userId,
                    Name = request.Name,
                    Description = request.Description,
                    DateRange = request.DateRange,
                    BusinessId = request.BusinessId,
                    LayoutType = request.LayoutType,
                    WidgetConfiguration = request.WidgetConfiguration,
                    IsDefault = request.IsDefault,
                    CreatedAt = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow
                };

                _context.SavedDashboardViews.Add(savedView);
                await _context.SaveChangesAsync();

                return savedView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving dashboard view for user {UserId}", userId);
                throw;
            }
        }

        public async Task<SavedDashboardView> UpdateSavedViewAsync(int viewId, SaveDashboardViewRequest request, string userId)
        {
            try
            {
                var savedView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Id == viewId && v.UserId == userId);

                if (savedView == null)
                    throw new InvalidOperationException("Saved view not found");

                // If this is set as default, unset other default views
                if (request.IsDefault)
                {
                    var existingDefaults = await _context.SavedDashboardViews
                        .Where(v => v.UserId == userId && v.IsDefault && v.Id != viewId)
                        .ToListAsync();
                    
                    foreach (var existing in existingDefaults)
                    {
                        existing.IsDefault = false;
                    }
                }

                savedView.Name = request.Name;
                savedView.Description = request.Description;
                savedView.DateRange = request.DateRange;
                savedView.BusinessId = request.BusinessId;
                savedView.LayoutType = request.LayoutType;
                savedView.WidgetConfiguration = request.WidgetConfiguration;
                savedView.IsDefault = request.IsDefault;
                savedView.LastUsed = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return savedView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating saved view {ViewId} for user {UserId}", viewId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteSavedViewAsync(int viewId, string userId)
        {
            try
            {
                var savedView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Id == viewId && v.UserId == userId);

                if (savedView == null)
                    return false;

                _context.SavedDashboardViews.Remove(savedView);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting saved view {ViewId} for user {UserId}", viewId, userId);
                return false;
            }
        }

        public async Task<bool> SetDefaultViewAsync(int viewId, string userId)
        {
            try
            {
                // Unset all existing default views
                var existingDefaults = await _context.SavedDashboardViews
                    .Where(v => v.UserId == userId && v.IsDefault)
                    .ToListAsync();
                
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }

                // Set the new default view
                var savedView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Id == viewId && v.UserId == userId);

                if (savedView == null)
                    return false;

                savedView.IsDefault = true;
                savedView.LastUsed = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default view {ViewId} for user {UserId}", viewId, userId);
                return false;
            }
        }

        public async Task<SavedDashboardView?> LoadSavedViewAsync(int viewId, string userId)
        {
            try
            {
                var savedView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Id == viewId && v.UserId == userId);

                if (savedView != null)
                {
                    savedView.LastUsed = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return savedView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved view {ViewId} for user {UserId}", viewId, userId);
                return null;
            }
        }

        public async Task<bool> ResetToDefaultAsync(string userId)
        {
            try
            {
                // Delete all saved views
                var savedViews = await _context.SavedDashboardViews
                    .Where(v => v.UserId == userId)
                    .ToListAsync();
                
                _context.SavedDashboardViews.RemoveRange(savedViews);

                // Reset preferences to default
                var preferences = await GetUserPreferencesAsync(userId);
                preferences.ShowViewsCard = true;
                preferences.ShowReviewsCard = true;
                preferences.ShowFavoritesCard = true;
                preferences.ShowEngagementCard = true;
                preferences.ShowPerformanceChart = true;
                preferences.ShowViewsChart = true;
                preferences.ShowReviewsChart = true;
                preferences.LayoutType = "default";
                preferences.RefreshInterval = 0;
                preferences.DefaultDateRange = "30";
                preferences.FocusedBusinessId = null;
                preferences.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting dashboard to default for user {UserId}", userId);
                return false;
            }
        }

        public List<string> GetAvailableLayouts()
        {
            return new List<string> { "default", "compact", "detailed" };
        }

        public List<string> GetAvailableDateRanges()
        {
            return new List<string> { "7", "30", "90", "365" };
        }

        public List<int> GetAvailableRefreshIntervals()
        {
            return new List<int> { 0, 30, 60, 300 };
        }
    }
}
