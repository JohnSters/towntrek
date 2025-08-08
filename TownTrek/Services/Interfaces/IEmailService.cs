using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPriceChangeNotificationAsync(string email, string firstName, string tierName, decimal oldPrice, decimal newPrice, DateTime effectiveDate);
        Task SendSubscriptionExpiredEmailAsync(string email, string firstName);
        Task SendPaymentFailedReminderAsync(Subscription subscription);
    }
}