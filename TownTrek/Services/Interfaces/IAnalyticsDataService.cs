using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service responsible for data access operations for analytics
    /// </summary>
    public interface IAnalyticsDataService
    {
        /// <summary>
        /// Gets user's businesses for analytics
        /// </summary>
        Task<List<Business>> GetUserBusinessesAsync(string userId);

        /// <summary>
        /// Gets business reviews for analytics
        /// </summary>
        Task<List<BusinessReview>> GetBusinessReviewsAsync(List<int> businessIds, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets business favorites for analytics
        /// </summary>
        Task<List<FavoriteBusiness>> GetBusinessFavoritesAsync(List<int> businessIds, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets business view logs for analytics
        /// </summary>
        Task<List<BusinessViewLog>> GetBusinessViewLogsAsync(List<int> businessIds, DateTime? startDate = null, DateTime? endDate = null, string? platform = null);

        /// <summary>
        /// Gets category benchmarks data
        /// </summary>
        Task<List<Business>> GetCategoryBusinessesAsync(string category, int? excludeBusinessId = null);

        /// <summary>
        /// Gets competitor businesses for a given business
        /// </summary>
        Task<List<Business>> GetCompetitorBusinessesAsync(int businessId, string category, string town);

        /// <summary>
        /// Gets analytics snapshots for a business
        /// </summary>
        Task<List<AnalyticsSnapshot>> GetAnalyticsSnapshotsAsync(int businessId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets user by ID
        /// </summary>
        Task<ApplicationUser?> GetUserAsync(string userId);

        /// <summary>
        /// Gets business by ID with ownership validation
        /// </summary>
        Task<Business?> GetBusinessAsync(int businessId, string userId);
    }
}
