using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<List<SubscriptionTier>> GetAvailableSubscriptionTiersAsync();
        Task<SubscriptionTier?> GetSubscriptionTierByIdAsync(int tierId);
        Task<RegistrationResult> RegisterMemberAsync(RegisterViewModel model);
        Task<RegistrationResult> RegisterBusinessOwnerAsync(RegisterViewModel model);
        Task<RegistrationResult> RegisterTrialUserAsync(RegisterViewModel model);
        Task<string> GeneratePayFastPaymentDataAsync(SubscriptionTier tier, ApplicationUser user);
    }

    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public ApplicationUser? User { get; set; }
        public string? PaymentUrl { get; set; }
        public bool RequiresPayment { get; set; }

        public static RegistrationResult Success(ApplicationUser user) => new() { IsSuccess = true, User = user };
        public static RegistrationResult SuccessWithPayment(ApplicationUser user, string paymentUrl) => new() 
        { 
            IsSuccess = true, 
            User = user, 
            PaymentUrl = paymentUrl, 
            RequiresPayment = true 
        };
        public static RegistrationResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}