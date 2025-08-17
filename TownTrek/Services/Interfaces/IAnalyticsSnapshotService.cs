using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing analytics snapshots and historical data
    /// </summary>
    public interface IAnalyticsSnapshotService
    {
        /// <summary>
        /// Creates daily snapshots for all active businesses
        /// </summary>
        /// <param name="snapshotDate">The date for the snapshot (defaults to yesterday)</param>
        /// <returns>Number of snapshots created</returns>
        Task<int> CreateDailySnapshotsAsync(DateTime? snapshotDate = null);

        /// <summary>
        /// Creates a snapshot for a specific business
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="snapshotDate">The date for the snapshot</param>
        /// <returns>The created snapshot</returns>
        Task<AnalyticsSnapshot?> CreateBusinessSnapshotAsync(int businessId, DateTime snapshotDate);

        /// <summary>
        /// Gets historical snapshots for a business
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of snapshots</returns>
        Task<List<AnalyticsSnapshot>> GetBusinessSnapshotsAsync(int businessId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates growth rates using historical data
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="currentPeriodDays">Number of days for current period</param>
        /// <param name="previousPeriodDays">Number of days for previous period</param>
        /// <returns>Growth rate data</returns>
        Task<GrowthRateData> CalculateGrowthRatesAsync(int businessId, int currentPeriodDays = 30, int previousPeriodDays = 30);

        /// <summary>
        /// Cleans up old snapshots based on retention policy
        /// </summary>
        /// <param name="retentionDays">Number of days to retain (default 730 = 2 years)</param>
        /// <returns>Number of snapshots deleted</returns>
        Task<int> CleanupOldSnapshotsAsync(int retentionDays = 730);

        /// <summary>
        /// Gets aggregated weekly/monthly data for long-term trends
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="aggregationType">Weekly or Monthly</param>
        /// <param name="months">Number of months to look back</param>
        /// <returns>Aggregated trend data</returns>
        Task<List<AggregatedAnalyticsData>> GetAggregatedTrendsAsync(int businessId, string aggregationType, int months = 12);
    }

    /// <summary>
    /// Growth rate data for analytics
    /// </summary>
    public class GrowthRateData
    {
        public decimal ViewsGrowthRate { get; set; }
        public decimal ReviewsGrowthRate { get; set; }
        public decimal FavoritesGrowthRate { get; set; }
        public decimal RatingGrowthRate { get; set; }
        public decimal EngagementGrowthRate { get; set; }
        public int CurrentPeriodViews { get; set; }
        public int PreviousPeriodViews { get; set; }
        public int CurrentPeriodReviews { get; set; }
        public int PreviousPeriodReviews { get; set; }
        public int CurrentPeriodFavorites { get; set; }
        public int PreviousPeriodFavorites { get; set; }
        public decimal CurrentPeriodRating { get; set; }
        public decimal PreviousPeriodRating { get; set; }
        public decimal CurrentPeriodEngagement { get; set; }
        public decimal PreviousPeriodEngagement { get; set; }
    }

    /// <summary>
    /// Aggregated analytics data for trends
    /// </summary>
    public class AggregatedAnalyticsData
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public int TotalViews { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
        public decimal AverageRating { get; set; }
        public decimal AverageEngagement { get; set; }
    }
}
