using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for handling user registration and subscription tier management
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Gets all available subscription tiers
        /// </summary>
        Task<List<SubscriptionTier>> GetAvailableSubscriptionTiersAsync();
        
        /// <summary>
        /// Gets a subscription tier by its ID
        /// </summary>
        Task<SubscriptionTier?> GetSubscriptionTierByIdAsync(int tierId);
        
        /// <summary>
        /// Registers a new member user
        /// </summary>
        Task<RegistrationResult> RegisterMemberAsync(RegisterViewModel model);
        
        /// <summary>
        /// Registers a new business owner user
        /// </summary>
        Task<RegistrationResult> RegisterBusinessOwnerAsync(RegisterViewModel model);
        
        /// <summary>
        /// Registers a new trial user
        /// </summary>
        Task<RegistrationResult> RegisterTrialUserAsync(RegisterViewModel model);
        
        /// <summary>
        /// Generates PayFast payment data for a subscription tier and user
        /// </summary>
        Task<string> GeneratePayFastPaymentDataAsync(SubscriptionTier tier, ApplicationUser user);
        
        /// <summary>
        /// Builds PayFast form fields for payment processing
        /// </summary>
        Task<Dictionary<string, string>> BuildPayFastFormFieldsAsync(SubscriptionTier tier, ApplicationUser user, int paymentId);
        
        /// <summary>
        /// Builds PayFast form fields for an existing user
        /// </summary>
        Task<Dictionary<string, string>?> BuildPayFastFormFieldsForUserAsync(string userId);
    }

    /// <summary>
    /// Represents the result of a registration operation
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Indicates whether the registration operation was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Error message if the registration operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// The user created during registration
        /// </summary>
        public ApplicationUser? User { get; set; }
        
        /// <summary>
        /// Payment URL if payment is required
        /// </summary>
        public string? PaymentUrl { get; set; }
        
        /// <summary>
        /// Indicates whether payment is required for the registration
        /// </summary>
        public bool RequiresPayment { get; set; }

        /// <summary>
        /// Creates a successful registration result
        /// </summary>
        public static RegistrationResult Success(ApplicationUser user) => new() { IsSuccess = true, User = user };
        
        /// <summary>
        /// Creates a successful registration result that requires payment
        /// </summary>
        public static RegistrationResult SuccessWithPayment(ApplicationUser user, string paymentUrl) => new() 
        { 
            IsSuccess = true, 
            User = user, 
            PaymentUrl = paymentUrl, 
            RequiresPayment = true 
        };
        
        /// <summary>
        /// Creates an error registration result with the specified message
        /// </summary>
        public static RegistrationResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}