using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<ClientAnalyticsViewModel> GetClientAnalyticsAsync(string userId);
        Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(int businessId, string userId);
        Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30);
        Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30);
        Task<List<BusinessPerformanceInsight>> GetPerformanceInsightsAsync(string userId);
        Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category);
        Task<CategoryBenchmarks?> GetDetailedCategoryBenchmarksAsync(string userId, string category);
        Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string userId);
        Task RecordBusinessViewAsync(int businessId);
        
        // Platform-specific analytics methods
        Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null);
        Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null);
        
        // Chart data processing methods (Phase 2.2)
        Task<ViewsChartDataResponse> GetViewsChartDataAsync(string userId, int days = 30, string? platform = null);
        Task<ReviewsChartDataResponse> GetReviewsChartDataAsync(string userId, int days = 30);
    }
}