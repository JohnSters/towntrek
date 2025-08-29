using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for validating user subscriptions and managing access control
    /// </summary>
    public interface ISubscriptionAuthService
    {
        /// <summary>
        /// Validates a user's subscription status and returns authentication result
        /// </summary>
        Task<SubscriptionAuthResult> ValidateUserSubscriptionAsync(string userId);
        
        /// <summary>
        /// Gets the subscription tier for a specific user
        /// </summary>
        Task<SubscriptionTier?> GetUserSubscriptionTierAsync(string userId);
        
        /// <summary>
        /// Gets the subscription limits for a specific user
        /// </summary>
        Task<SubscriptionLimits> GetUserLimitsAsync(string userId);
        
        /// <summary>
        /// Checks if a user can access a specific feature
        /// </summary>
        Task<bool> CanAccessFeatureAsync(string userId, string featureKey);
        
        /// <summary>
        /// Gets the redirect URL for a user based on their subscription status
        /// </summary>
        Task<string> GetRedirectUrlForUserAsync(string userId);
    }

    /// <summary>
    /// Represents the result of a subscription authentication operation
    /// </summary>
    public class SubscriptionAuthResult
    {
        /// <summary>
        /// Indicates whether the user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
        
        /// <summary>
        /// Indicates whether the user has an active subscription
        /// </summary>
        public bool HasActiveSubscription { get; set; }
        
        /// <summary>
        /// Indicates whether the user's payment is valid
        /// </summary>
        public bool IsPaymentValid { get; set; }
        
        /// <summary>
        /// The current payment status
        /// </summary>
        public string? PaymentStatus { get; set; }
        
        /// <summary>
        /// The user's subscription tier
        /// </summary>
        public SubscriptionTier? SubscriptionTier { get; set; }
        
        /// <summary>
        /// Redirect URL for payment or subscription management
        /// </summary>
        public string? RedirectUrl { get; set; }
        
        /// <summary>
        /// Error message if authentication failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// The user's subscription limits
        /// </summary>
        public SubscriptionLimits? Limits { get; set; }

        /// <summary>
        /// Creates a successful authentication result
        /// </summary>
        public static SubscriptionAuthResult Success(SubscriptionTier tier, SubscriptionLimits limits)
        {
            return new SubscriptionAuthResult
            {
                IsAuthenticated = true,
                HasActiveSubscription = true,
                IsPaymentValid = true,
                SubscriptionTier = tier,
                Limits = limits
            };
        }

        /// <summary>
        /// Creates a result indicating payment is required
        /// </summary>
        public static SubscriptionAuthResult PaymentRequired(string redirectUrl, string paymentStatus)
        {
            return new SubscriptionAuthResult
            {
                IsAuthenticated = true,
                HasActiveSubscription = false,
                IsPaymentValid = false,
                PaymentStatus = paymentStatus,
                RedirectUrl = redirectUrl,
                ErrorMessage = "Payment required to access client dashboard"
            };
        }

        /// <summary>
        /// Creates a result indicating no subscription is found
        /// </summary>
        public static SubscriptionAuthResult NoSubscription()
        {
            return new SubscriptionAuthResult
            {
                IsAuthenticated = true,
                HasActiveSubscription = false,
                IsPaymentValid = false,
                ErrorMessage = "No active subscription found"
            };
        }

        /// <summary>
        /// Creates an unauthorized result with the specified message
        /// </summary>
        public static SubscriptionAuthResult Unauthorized(string message)
        {
            return new SubscriptionAuthResult
            {
                IsAuthenticated = false,
                HasActiveSubscription = false,
                IsPaymentValid = false,
                ErrorMessage = message
            };
        }
    }
}