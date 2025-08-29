using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces.ClientAnalytics
{
    /// <summary>
    /// Main orchestration service for client analytics functionality
    /// </summary>
    public interface IClientAnalyticsService
    {
        /// <summary>
        /// Gets comprehensive analytics dashboard data for a user
        /// </summary>
        Task<ClientAnalyticsViewModel> GetClientAnalyticsAsync(string userId);
        
        /// <summary>
        /// Gets analytics data for a specific business
        /// </summary>
        Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(int businessId, string userId);
        
        /// <summary>
        /// Gets views over time data for a user's businesses
        /// </summary>
        Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30);
        
        /// <summary>
        /// Gets reviews over time data for a user's businesses
        /// </summary>
        Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30);

        /// <summary>
        /// Gets category benchmark data for a user
        /// </summary>
        Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category);
        
        /// <summary>
        /// Gets detailed category benchmarks for a user
        /// </summary>
        Task<CategoryBenchmarks?> GetDetailedCategoryBenchmarksAsync(string userId, string category);
        
        /// <summary>
        /// Gets competitor insights for a user
        /// </summary>
        Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string userId);
        
        /// <summary>
        /// Records a business view event
        /// </summary>
        Task RecordBusinessViewAsync(int businessId);
        
        /// <summary>
        /// Gets views over time data filtered by platform
        /// </summary>
        Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null);
        
        /// <summary>
        /// Gets view statistics for a business within a date range
        /// </summary>
        Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null);
        
        /// <summary>
        /// Gets views over time data for a user's businesses (alias method)
        /// </summary>
        Task<List<ViewsOverTimeData>> GetViewsOverTimeDataAsync(string userId, int days = 30);
        
        /// <summary>
        /// Gets reviews over time data for a user's businesses (alias method)
        /// </summary>
        Task<List<ReviewsOverTimeData>> GetReviewsOverTimeDataAsync(string userId, int days = 30);
        
        /// <summary>
        /// Gets comparative analysis data for a user
        /// </summary>
        Task<ComparativeAnalysisResponse> GetComparativeAnalysisDataAsync(string userId, string comparisonType, DateTime? fromDate = null, DateTime? toDate = null);
    }
}