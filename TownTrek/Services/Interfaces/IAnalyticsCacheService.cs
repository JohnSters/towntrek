using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Specialized cache service for analytics data with appropriate expiration strategies
    /// </summary>
    public interface IAnalyticsCacheService
    {
        /// <summary>
        /// Get or set client analytics dashboard data
        /// </summary>
        Task<ClientAnalyticsViewModel?> GetClientAnalyticsAsync(string userId);

        /// <summary>
        /// Get or set business analytics data
        /// </summary>
        Task<BusinessAnalyticsData?> GetBusinessAnalyticsAsync(int businessId, string userId);

        /// <summary>
        /// Get or set views chart data
        /// </summary>
        Task<ViewsChartDataResponse?> GetViewsChartDataAsync(string userId, int days, string? platform);

        /// <summary>
        /// Get or set reviews chart data
        /// </summary>
        Task<ReviewsChartDataResponse?> GetReviewsChartDataAsync(string userId, int days);

        /// <summary>
        /// Get or set views over time data
        /// </summary>
        Task<List<ViewsOverTimeData>?> GetViewsOverTimeAsync(string userId, int days);

        /// <summary>
        /// Get or set reviews over time data
        /// </summary>
        Task<List<ReviewsOverTimeData>?> GetReviewsOverTimeAsync(string userId, int days);

        /// <summary>
        /// Get or set performance insights data
        /// </summary>
        Task<List<BusinessPerformanceInsight>?> GetPerformanceInsightsAsync(string userId);

        /// <summary>
        /// Get or set category benchmarks data
        /// </summary>
        Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category);

        /// <summary>
        /// Get or set competitor insights data
        /// </summary>
        Task<List<CompetitorInsight>?> GetCompetitorInsightsAsync(string userId);

        /// <summary>
        /// Invalidate all analytics cache for a specific user
        /// </summary>
        Task InvalidateUserAnalyticsAsync(string userId);

        /// <summary>
        /// Invalidate business-specific analytics cache
        /// </summary>
        Task InvalidateBusinessAnalyticsAsync(int businessId);

        /// <summary>
        /// Invalidate chart data cache for a specific user
        /// </summary>
        Task InvalidateChartDataAsync(string userId);

        /// <summary>
        /// Warm up cache for frequently accessed analytics data
        /// </summary>
        Task WarmUpAnalyticsCacheAsync(List<string> userIds);

        /// <summary>
        /// Get analytics cache statistics
        /// </summary>
        Task<AnalyticsCacheStatistics> GetAnalyticsCacheStatisticsAsync();
    }

    /// <summary>
    /// Analytics-specific cache statistics
    /// </summary>
    public class AnalyticsCacheStatistics
    {
        public long DashboardCacheHits { get; set; }
        public long DashboardCacheMisses { get; set; }
        public long ChartDataCacheHits { get; set; }
        public long ChartDataCacheMisses { get; set; }
        public long BusinessAnalyticsCacheHits { get; set; }
        public long BusinessAnalyticsCacheMisses { get; set; }
        public double OverallHitRate { get; set; }
        public int CachedUsers { get; set; }
        public int CachedBusinesses { get; set; }
        public DateTime LastWarmUp { get; set; }
        public TimeSpan AverageCacheTime { get; set; }
    }
}
