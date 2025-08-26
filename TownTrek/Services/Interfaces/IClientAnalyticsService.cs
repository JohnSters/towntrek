using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for client-specific analytics functionality
    /// </summary>
    public interface IClientAnalyticsService
    {
        /// <summary>
        /// Gets comprehensive analytics data for a client user
        /// </summary>
        Task<ClientAnalyticsViewModel> GetClientAnalyticsAsync(string userId);
        
        /// <summary>
        /// Gets analytics data for a specific business
        /// </summary>
        Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(int businessId, string userId);
        
        /// <summary>
        /// Gets performance insights for user's businesses
        /// </summary>
        Task<List<BusinessPerformanceInsight>> GetPerformanceInsightsAsync(string userId);
        
        /// <summary>
        /// Gets category benchmarks for a specific category
        /// </summary>
        Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category);
        
        /// <summary>
        /// Gets detailed category benchmarks
        /// </summary>
        Task<CategoryBenchmarks?> GetDetailedCategoryBenchmarksAsync(string userId, string category);
        
        /// <summary>
        /// Gets competitor insights for user's businesses
        /// </summary>
        Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string userId);
    }
}
