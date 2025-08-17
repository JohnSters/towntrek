using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service responsible for tracking analytics events for event sourcing
    /// </summary>
    public interface IAnalyticsEventService
    {
        /// <summary>
        /// Records an analytics event
        /// </summary>
        Task RecordAnalyticsEventAsync(string eventType, string userId, int? businessId = null, object? eventData = null);

        /// <summary>
        /// Records a business view event
        /// </summary>
        Task RecordBusinessViewEventAsync(int businessId, string? userId = null, string? platform = null);

        /// <summary>
        /// Records an analytics data access event
        /// </summary>
        Task RecordAnalyticsAccessEventAsync(string userId, string analyticsType, object? parameters = null);

        /// <summary>
        /// Records an analytics export event
        /// </summary>
        Task RecordAnalyticsExportEventAsync(string userId, string exportType, object? parameters = null);

        /// <summary>
        /// Records an analytics error event
        /// </summary>
        Task RecordAnalyticsErrorEventAsync(string userId, string errorType, string errorMessage, object? context = null);

        /// <summary>
        /// Gets analytics events for a user within a date range
        /// </summary>
        Task<List<AnalyticsEvent>> GetAnalyticsEventsAsync(string userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets analytics events for a business within a date range
        /// </summary>
        Task<List<AnalyticsEvent>> GetBusinessAnalyticsEventsAsync(int businessId, DateTime startDate, DateTime endDate);
    }
}
