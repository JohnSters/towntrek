using TownTrek.Models;

namespace TownTrek.Services.Interfaces.ClientAnalytics
{
    public interface IViewTrackingService
    {
        /// <summary>
        /// Logs a business page view
        /// </summary>
        /// <param name="businessId">The ID of the business being viewed</param>
        /// <param name="userId">The user ID (null for anonymous views)</param>
        /// <param name="platform">The platform where the view occurred (Web, Mobile, API)</param>
        /// <param name="ipAddress">The IP address of the viewer</param>
        /// <param name="userAgent">The user agent string</param>
        /// <param name="referrer">The referrer URL</param>
        /// <param name="sessionId">The session ID</param>
        /// <returns>Task representing the async operation</returns>
        Task LogBusinessViewAsync(int businessId, string? userId, string platform, string? ipAddress, string? userAgent, string? referrer, string? sessionId);

        /// <summary>
        /// Gets view statistics for a business
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="startDate">Start date for the period</param>
        /// <param name="endDate">End date for the period</param>
        /// <param name="platform">Optional platform filter</param>
        /// <returns>View statistics</returns>
        Task<ViewStatistics> GetViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null);

        /// <summary>
        /// Gets views over time for charting
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="days">Number of days to look back</param>
        /// <param name="platform">Optional platform filter</param>
        /// <returns>Views over time data</returns>
        Task<List<DailyViews>> GetViewsOverTimeAsync(int businessId, int days = 30, string? platform = null);
    }

    public class ViewStatistics
    {
        public int TotalViews { get; set; }
        public int UniqueVisitors { get; set; }
        public int WebViews { get; set; }
        public int MobileViews { get; set; }
        public int ApiViews { get; set; }
        public double AverageViewsPerDay { get; set; }
        public DateTime? LastViewed { get; set; }
        
        // Additional properties for analytics service compatibility
        public int ViewsThisMonth { get; set; }
        public int ViewsLastMonth { get; set; }
        public double ViewsGrowthRate { get; set; }
        public int PeakDayViews { get; set; }
        public DateTime PeakDayDate { get; set; }
        public Dictionary<string, int> PlatformBreakdown { get; set; } = new();
    }

    public class DailyViews
    {
        public DateTime Date { get; set; }
        public int TotalViews { get; set; }
        public int WebViews { get; set; }
        public int MobileViews { get; set; }
        public int ApiViews { get; set; }
    }
}
