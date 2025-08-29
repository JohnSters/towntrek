using TownTrek.Models;

namespace TownTrek.Services.Interfaces.AdminAnalytics
{
    /// <summary>
    /// Service for auditing analytics access and data exports for security and compliance
    /// </summary>
    public interface IAnalyticsAuditService
    {
        /// <summary>
        /// Log analytics page access
        /// </summary>
        Task LogAnalyticsAccessAsync(string userId, string action, string? businessId = null, string? platform = null);

        /// <summary>
        /// Log data export/download
        /// </summary>
        Task LogDataExportAsync(string userId, string exportType, string? businessId = null, string? format = null);

        /// <summary>
        /// Log suspicious activity
        /// </summary>
        Task LogSuspiciousActivityAsync(string userId, string activity, string details, string ipAddress);

        /// <summary>
        /// Get audit logs for a user (admin only)
        /// </summary>
        Task<List<AnalyticsAuditLog>> GetUserAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get all audit logs (admin only)
        /// </summary>
        Task<List<AnalyticsAuditLog>> GetAllAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null);

        /// <summary>
        /// Get suspicious activity logs (admin only)
        /// </summary>
        Task<List<AnalyticsAuditLog>> GetSuspiciousActivityLogsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Clean up old audit logs (retention policy)
        /// </summary>
        Task<int> CleanupOldAuditLogsAsync(int retentionDays = 365);
    }
}
