using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    public interface IAnalyticsExportService
    {
        /// <summary>
        /// Generate PDF report for business analytics
        /// </summary>
        Task<byte[]> GenerateBusinessAnalyticsPdfAsync(int businessId, string userId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Generate PDF report for client analytics overview
        /// </summary>
        Task<byte[]> GenerateClientAnalyticsPdfAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Export analytics data as CSV
        /// </summary>
        Task<byte[]> ExportAnalyticsCsvAsync(string userId, string dataType, DateTime? fromDate = null, DateTime? toDate = null, int? businessId = null);

        /// <summary>
        /// Generate shareable dashboard link
        /// </summary>
        Task<string> GenerateShareableLinkAsync(string userId, string dashboardType, int? businessId = null, DateTime? expiresAt = null);

        /// <summary>
        /// Validate shareable link
        /// </summary>
        Task<bool> ValidateShareableLinkAsync(string linkToken);

        /// <summary>
        /// Get analytics data for shareable link
        /// </summary>
        Task<object?> GetShareableLinkDataAsync(string linkToken);

        /// <summary>
        /// Schedule email report
        /// </summary>
        Task<bool> ScheduleEmailReportAsync(string userId, string reportType, string frequency, int? businessId = null);

        /// <summary>
        /// Send immediate email report
        /// </summary>
        Task<bool> SendEmailReportAsync(string userId, string reportType, int? businessId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
