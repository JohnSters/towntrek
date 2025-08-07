using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;

namespace TownTrek.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IRegistrationService registrationService,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _userManager = userManager;
            _registrationService = registrationService;
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentNotificationAsync(string paymentId, string paymentStatus, string token)
        {
            try
            {
                if (!int.TryParse(paymentId, out var subscriptionId))
                {
                    return PaymentResult.Error("Invalid payment ID format");
                }

                var subscription = await _context.Subscriptions
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == subscriptionId);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription not found for payment ID: {PaymentId}", paymentId);
                    return PaymentResult.Error("Subscription not found");
                }

                if (paymentStatus == "COMPLETE")
                {
                    await ActivateSubscriptionAsync(subscription, token);
                    return PaymentResult.Success(subscription.User);
                }
                else
                {
                    await HandleFailedPaymentAsync(subscription);
                    return PaymentResult.Error("Payment failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment notification for payment ID: {PaymentId}", paymentId);
                return PaymentResult.Error("Error processing payment");
            }
        }

        public async Task<bool> ValidatePayFastSignatureAsync(IFormCollection formData)
        {
            // TODO: Implement proper PayFast signature validation
            // For now, return true for sandbox testing
            await Task.CompletedTask;
            return true;
        }

        public async Task<PaymentResult> HandleSuccessfulPaymentAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return PaymentResult.Error("User not found");
                }

                // Check if user has an active subscription
                if (user.HasActiveSubscription)
                {
                    return PaymentResult.Success(user);
                }

                return PaymentResult.Error("No active subscription found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling successful payment for user: {UserId}", userId);
                return PaymentResult.Error("Error processing successful payment");
            }
        }

        private async Task ActivateSubscriptionAsync(Subscription subscription, string token)
        {
            subscription.IsActive = true;
            subscription.PaymentStatus = "Completed"; // Use "Completed" to match our validation logic
            subscription.PayFastToken = token;
            subscription.LastPaymentDate = DateTime.UtcNow;
            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);

            var user = subscription.User;
            if (user != null)
            {
                user.HasActiveSubscription = true;
                user.SubscriptionStartDate = DateTime.UtcNow;
                user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);

                // Add appropriate role based on tier
                var tier = await _registrationService.GetSubscriptionTierByIdAsync(subscription.SubscriptionTierId);
                if (tier != null)
                {
                    user.CurrentSubscriptionTier = tier.Name;
                    
                    // Add tier-specific role
                    var roleName = $"Client-{tier.Name}";
                    await _userManager.AddToRoleAsync(user, roleName);
                }

                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Subscription activated for user {UserId} with status 'Completed'", subscription.UserId);
        }

        private async Task HandleFailedPaymentAsync(Subscription subscription)
        {
            subscription.PaymentStatus = "Failed";
            subscription.IsActive = false;
            
            // Update user status
            var user = subscription.User;
            if (user != null)
            {
                user.HasActiveSubscription = false;
                await _userManager.UpdateAsync(user);
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogWarning("Payment failed for subscription {SubscriptionId}", subscription.Id);
        }
    }
}