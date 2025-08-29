using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces.ClientAnalytics
{
    /// <summary>
    /// Service for business-specific metrics and view tracking functionality
    /// </summary>
    public interface IBusinessMetricsService
    {
        /// <summary>
        /// Records a business view event
        /// </summary>
        Task RecordBusinessViewAsync(int businessId);
        
        /// <summary>
        /// Gets view statistics for a business within a date range
        /// </summary>
        Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null);
        
        /// <summary>
        /// Gets views over time data for a user's businesses
        /// </summary>
        Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30);
        
        /// <summary>
        /// Gets views over time data filtered by platform
        /// </summary>
        Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null);
        
        /// <summary>
        /// Gets reviews over time data for a user's businesses
        /// </summary>
        Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30);
    }
}
