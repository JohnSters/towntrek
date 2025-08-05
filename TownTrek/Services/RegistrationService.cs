using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;

namespace TownTrek.Services
{
    public interface IRegistrationService
    {
        Task<List<SubscriptionTier>> GetAvailableSubscriptionTiersAsync();
        Task<SubscriptionTier?> GetSubscriptionTierByIdAsync(int tierId);
        Task<string> GeneratePayFastPaymentDataAsync(SubscriptionTier tier, ApplicationUser user);
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<RegistrationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<SubscriptionTier>> GetAvailableSubscriptionTiersAsync()
        {
            return await _context.SubscriptionTiers
                .Where(t => t.IsActive)
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<SubscriptionTier?> GetSubscriptionTierByIdAsync(int tierId)
        {
            return await _context.SubscriptionTiers
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .FirstOrDefaultAsync(t => t.Id == tierId && t.IsActive);
        }

        public async Task<string> GeneratePayFastPaymentDataAsync(SubscriptionTier tier, ApplicationUser user)
        {
            // This method will generate the secure PayFast payment form data
            // For now, return a placeholder - implement PayFast integration later
            await Task.CompletedTask;
            
            var paymentData = new
            {
                merchant_id = _configuration["PayFast:MerchantId"],
                merchant_key = _configuration["PayFast:MerchantKey"],
                return_url = $"{_configuration["BaseUrl"]}/payment/success",
                cancel_url = $"{_configuration["BaseUrl"]}/payment/cancel",
                notify_url = $"{_configuration["BaseUrl"]}/payment/notify",
                
                // Subscription details
                subscription_type = "1", // Recurring
                billing_date = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd"),
                recurring_amount = tier.MonthlyPrice.ToString("F2"),
                frequency = "3", // Monthly
                cycles = "0", // Indefinite
                
                // Order details
                m_payment_id = Guid.NewGuid().ToString(),
                amount = tier.MonthlyPrice.ToString("F2"),
                item_name = $"TownTrek {tier.DisplayName}",
                item_description = $"Monthly subscription for {tier.DisplayName}"
            };

            _logger.LogInformation("Generated PayFast payment data for user {UserId}, tier {TierName}, amount R{Amount}", 
                user.Id, tier.Name, tier.MonthlyPrice);

            return System.Text.Json.JsonSerializer.Serialize(paymentData);
        }
    }
}