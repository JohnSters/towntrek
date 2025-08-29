using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces.ClientAnalytics
{
    /// <summary>
    /// Service responsible for validating analytics parameters and requests
    /// </summary>
    public interface IAnalyticsValidationService
    {
        /// <summary>
        /// Validates user ID and ensures user exists
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidateUserIdAsync(string userId);

        /// <summary>
        /// Validates business ID and ensures user owns the business
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidateBusinessOwnershipAsync(int businessId, string userId);

        /// <summary>
        /// Validates analytics days parameter
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateAnalyticsDays(int days);

        /// <summary>
        /// Validates platform parameter
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidatePlatform(string? platform);

        /// <summary>
        /// Validates date range parameters
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Validates comparative analysis request
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateComparativeAnalysisRequest(ComparativeAnalysisRequest request);

        /// <summary>
        /// Validates chart data request parameters
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateChartDataRequest(string userId, int days, string? platform = null);
    }
}
