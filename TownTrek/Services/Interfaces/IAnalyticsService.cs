using TownTrek.Models.ViewModels;

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
        Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string userId);
        Task RecordBusinessViewAsync(int businessId);
    }
}