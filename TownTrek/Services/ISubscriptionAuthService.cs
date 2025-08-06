using TownTrek.Models;

namespace TownTrek.Services
{
    public interface ISubscriptionAuthService
    {
        Task<SubscriptionAuthResult> ValidateUserSubscriptionAsync(string userId);
        Task<SubscriptionTier?> GetUserSubscriptionTierAsync(string userId);
        Task<SubscriptionLimits> GetUserLimitsAsync(string userId);
        Task<bool> CanAccessFeatureAsync(string userId, string featureKey);
        Task<string> GetRedirectUrlForUserAsync(string userId);
    }

    public class SubscriptionAuthResult
    {
        public bool IsAuthenticated { get; set; }
        public bool HasActiveSubscription { get; set; }
        public bool IsPaymentValid { get; set; }
        public string? PaymentStatus { get; set; }
        public SubscriptionTier? SubscriptionTier { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public SubscriptionLimits? Limits { get; set; }

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

    public class SubscriptionLimits
    {
        public int MaxBusinesses { get; set; }
        public int MaxImages { get; set; }
        public int MaxPDFs { get; set; }
        public bool HasBasicSupport { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasDedicatedSupport { get; set; }
        public bool HasBasicAnalytics { get; set; }
        public bool HasAdvancedAnalytics { get; set; }
        public bool HasFeaturedPlacement { get; set; }
        public bool HasPDFUploads { get; set; }
        public int CurrentBusinessCount { get; set; }
        public int CurrentImageCount { get; set; }
        public int CurrentPDFCount { get; set; }
    }
}