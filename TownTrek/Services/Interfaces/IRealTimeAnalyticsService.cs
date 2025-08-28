using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing real-time analytics updates via SignalR
    /// </summary>
    public interface IRealTimeAnalyticsService
    {
        /// <summary>
        /// Send real-time update for client analytics dashboard
        /// </summary>
        Task SendClientAnalyticsUpdateAsync(string userId, ClientAnalyticsViewModel analytics);

        /// <summary>
        /// Send real-time update for business analytics
        /// </summary>
        Task SendBusinessAnalyticsUpdateAsync(int businessId, string userId, BusinessAnalyticsData analytics);

        /// <summary>
        /// Send real-time update for views chart data
        /// </summary>
        Task SendViewsChartUpdateAsync(string userId, ViewsChartDataResponse chartData);

        /// <summary>
        /// Send real-time update for reviews chart data
        /// </summary>
        Task SendReviewsChartUpdateAsync(string userId, ReviewsChartDataResponse chartData);

        /// <summary>
        /// Send real-time notification for significant changes
        /// </summary>
        Task SendAnalyticsNotificationAsync(string userId, string title, string message, string type = "info");

        /// <summary>


        /// <summary>
        /// Send real-time update for competitor insights
        /// </summary>
        Task SendCompetitorInsightsUpdateAsync(string userId, List<CompetitorInsight> insights);

        /// <summary>
        /// Send real-time update for category benchmarks
        /// </summary>
        Task SendCategoryBenchmarksUpdateAsync(string userId, string category, CategoryBenchmarkData benchmarks);

        /// <summary>
        /// Broadcast analytics update to all connected users (admin only)
        /// </summary>
        Task BroadcastAnalyticsUpdateAsync(string message, object data);

        /// <summary>
        /// Get active connections count for analytics
        /// </summary>
        Task<int> GetActiveConnectionsCountAsync();
    }
}
