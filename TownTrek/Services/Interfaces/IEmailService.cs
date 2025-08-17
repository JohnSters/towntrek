using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendEmailConfirmationAsync(string email, string firstName, string confirmationUrl);
        Task SendPaymentSuccessEmailAsync(Subscription subscription);
        Task SendPaymentFailedEmailAsync(Subscription subscription);
        Task SendPriceChangeNotificationAsync(string email, string firstName, string tierName, decimal oldPrice, decimal newPrice, DateTime effectiveDate);
        Task SendSubscriptionExpiredEmailAsync(string email, string firstName);
        Task SendPaymentFailedReminderAsync(Subscription subscription);
        Task<bool> SendEmailWithAttachmentAsync(string email, string subject, string body, byte[] attachmentData, string fileName, string contentType);
    }
}