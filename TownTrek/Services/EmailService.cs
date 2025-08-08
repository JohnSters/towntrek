using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    // Placeholder implementation - replace with actual email service
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendPriceChangeNotificationAsync(string email, string firstName, string tierName, decimal oldPrice, decimal newPrice, DateTime effectiveDate)
        {
            // TODO: Implement actual email sending
            _logger.LogInformation("Price change notification would be sent to {Email} for tier {TierName}: R{OldPrice} -> R{NewPrice}, effective {EffectiveDate}", 
                email, tierName, oldPrice, newPrice, effectiveDate);
            
            await Task.CompletedTask;
        }

        public async Task SendSubscriptionExpiredEmailAsync(string email, string firstName)
        {
            _logger.LogInformation("Subscription expired notification would be sent to {Email}", email);
            await Task.CompletedTask;
        }

        public async Task SendPaymentFailedReminderAsync(Subscription subscription)
        {
            _logger.LogInformation("Payment failed reminder would be sent for subscription {SubscriptionId}", subscription.Id);
            await Task.CompletedTask;
        }
    }
}