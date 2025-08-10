using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResult> CreateBusinessAlertAsync(int businessId, string alertType, string title, string message, DateTime? expiresAt = null, bool isPushNotification = false);
        Task<List<BusinessAlert>> GetActiveAlertsForBusinessAsync(int businessId);
        Task<List<BusinessAlert>> GetActiveAlertsForTownAsync(int townId);
        Task<ServiceResult> SendMarketDayReminderAsync(int marketBusinessId);
        Task<ServiceResult> SendEventUpdateAsync(int eventBusinessId, string updateType, string message);
        Task<ServiceResult> SendWeatherAlertAsync(int businessId, string weatherCondition);
    }
}