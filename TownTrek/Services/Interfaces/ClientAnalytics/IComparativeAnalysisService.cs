using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces.ClientAnalytics
{
    /// <summary>
    /// Service interface for comparative analysis functionality
    /// </summary>
    public interface IComparativeAnalysisService
    {
        /// <summary>
        /// Get comparative analysis based on request parameters
        /// </summary>
        Task<ComparativeAnalysisResponse> GetComparativeAnalysisAsync(string userId, ComparativeAnalysisRequest request);
        
        /// <summary>
        /// Get period over period comparison (week-over-week, month-over-month, etc.)
        /// </summary>
        Task<ComparativeAnalysisResponse> GetPeriodOverPeriodComparisonAsync(string userId, int? businessId = null, string comparisonType = "MonthOverMonth", string? platform = null);
        
        /// <summary>
        /// Get year over year comparison
        /// </summary>
        Task<ComparativeAnalysisResponse> GetYearOverYearComparisonAsync(string userId, int? businessId = null, string? platform = null);
        
        /// <summary>
        /// Get custom range comparison between two specified periods
        /// </summary>
        Task<ComparativeAnalysisResponse> GetCustomRangeComparisonAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? businessId = null, string? platform = null);
    }
}
