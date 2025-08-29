using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing business notifications and alerts
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Creates a business alert with optional push notification
        /// </summary>
        Task<ServiceResult> CreateBusinessAlertAsync(int businessId, string alertType, string title, string message, DateTime? expiresAt = null, bool isPushNotification = false);
        
        /// <summary>
        /// Gets all active alerts for a specific business
        /// </summary>
        Task<List<BusinessAlert>> GetActiveAlertsForBusinessAsync(int businessId);
        
        /// <summary>
        /// Gets all active alerts for a specific town
        /// </summary>
        Task<List<BusinessAlert>> GetActiveAlertsForTownAsync(int townId);
        
        /// <summary>
        /// Sends a market day reminder notification
        /// </summary>
        Task<ServiceResult> SendMarketDayReminderAsync(int marketBusinessId);
        
        /// <summary>
        /// Sends an event update notification
        /// </summary>
        Task<ServiceResult> SendEventUpdateAsync(int eventBusinessId, string updateType, string message);
        
        /// <summary>
        /// Sends a weather alert notification
        /// </summary>
        Task<ServiceResult> SendWeatherAlertAsync(int businessId, string weatherCondition);
    }
}