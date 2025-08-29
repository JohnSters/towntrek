using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for sending various types of emails to users
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a welcome email to a new user
        /// </summary>
        Task SendWelcomeEmailAsync(string email, string firstName);
        
        /// <summary>
        /// Sends an email confirmation link to a user
        /// </summary>
        Task SendEmailConfirmationAsync(string email, string firstName, string confirmationUrl);
        
        /// <summary>
        /// Sends a payment success notification email
        /// </summary>
        Task SendPaymentSuccessEmailAsync(Subscription subscription);
        
        /// <summary>
        /// Sends a payment failed notification email
        /// </summary>
        Task SendPaymentFailedEmailAsync(Subscription subscription);
        
        /// <summary>
        /// Sends a price change notification email
        /// </summary>
        Task SendPriceChangeNotificationAsync(string email, string firstName, string tierName, decimal oldPrice, decimal newPrice, DateTime effectiveDate);
        
        /// <summary>
        /// Sends a subscription expired notification email
        /// </summary>
        Task SendSubscriptionExpiredEmailAsync(string email, string firstName);
        
        /// <summary>
        /// Sends a payment failed reminder email
        /// </summary>
        Task SendPaymentFailedReminderAsync(Subscription subscription);
        
        /// <summary>
        /// Sends an email with an attachment
        /// </summary>
        Task<bool> SendEmailWithAttachmentAsync(string email, string subject, string body, byte[] attachmentData, string fileName, string contentType);
    }
}