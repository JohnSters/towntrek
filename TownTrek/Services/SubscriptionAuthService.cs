using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class SubscriptionAuthService : ISubscriptionAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriptionAuthService> _logger;
        private readonly IRegistrationService _registrationService;

        public SubscriptionAuthService(
            ApplicationDbContext context,
            ILogger<SubscriptionAuthService> logger,
            IRegistrationService registrationService)
        {
            _context = context;
            _logger = logger;
            _registrationService = registrationService;
        }

        public async Task<SubscriptionAuthResult> ValidateUserSubscriptionAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Subscriptions.Where(s => s.IsActive))
                    .ThenInclude(s => s.SubscriptionTier)
                    .ThenInclude(st => st.Limits)
                    .Include(u => u.Subscriptions.Where(s => s.IsActive))
                    .ThenInclude(s => s.SubscriptionTier)
                    .ThenInclude(st => st.Features)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return SubscriptionAuthResult.Unauthorized("User not found");
                }

                var activeSubscription = user.Subscriptions.FirstOrDefault(s => s.IsActive);

                // Check if user has HasActiveSubscription flag set (for users with roles but incomplete subscription records)
                if (activeSubscription == null)
                {
                    if (user.HasActiveSubscription && !string.IsNullOrEmpty(user.CurrentSubscriptionTier))
                    {
                        // User has subscription flag but no database record - create a temporary result for development
                        _logger.LogInformation("User {UserId} has HasActiveSubscription=true but no database record, using tier: {Tier}", 
                            userId, user.CurrentSubscriptionTier);
                        
                        // Get tier by name for temporary access (using ToUpper for case-insensitive comparison)
                        var tierName = user.CurrentSubscriptionTier.ToUpper();
                        _logger.LogInformation("Looking for subscription tier: '{TierName}' (normalized: '{NormalizedTierName}') for user {UserId}", 
                            user.CurrentSubscriptionTier, tierName, userId);
                        
                        var tier = await _context.SubscriptionTiers
                            .Include(st => st.Limits)
                            .Include(st => st.Features)
                            .FirstOrDefaultAsync(st => st.Name.ToUpper() == tierName);
                        
                        if (tier != null)
                        {
                            var userLimits = await GetUserLimitsWithUsageAsync(userId, tier);
                            return SubscriptionAuthResult.Success(tier, userLimits);
                        }
                        else
                        {
                            _logger.LogWarning("Subscription tier '{TierName}' not found in database for user {UserId}", 
                                user.CurrentSubscriptionTier, userId);
                            // Fall back to free tier limits
                            var freeLimits = await GetFreeTierLimitsAsync(userId);
                            return new SubscriptionAuthResult
                            {
                                IsAuthenticated = true,
                                HasActiveSubscription = true,
                                IsPaymentValid = true,
                                PaymentStatus = "Active",
                                SubscriptionTier = null,
                                Limits = freeLimits
                            };
                        }
                    }
                    
                    _logger.LogWarning("User {UserId} has no active subscription", userId);
                    return SubscriptionAuthResult.NoSubscription();
                }

                // Check payment status
                if (!IsPaymentStatusValid(activeSubscription.PaymentStatus))
                {
                    _logger.LogWarning("User {UserId} has invalid payment status: {PaymentStatus}", 
                        userId, activeSubscription.PaymentStatus);
                    
                    // Allow dashboard access for pending payments (with warning), but block for failed/rejected
                    if (activeSubscription.PaymentStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        // Get user limits with current usage for pending payments
                        var pendingLimits = await GetUserLimitsWithUsageAsync(userId, activeSubscription.SubscriptionTier);
                        
                        // Return success but with payment invalid flag for warning display
                        return new SubscriptionAuthResult
                        {
                            IsAuthenticated = true,
                            HasActiveSubscription = true,
                            IsPaymentValid = false,
                            PaymentStatus = activeSubscription.PaymentStatus,
                            SubscriptionTier = activeSubscription.SubscriptionTier,
                            Limits = pendingLimits,
                            RedirectUrl = await GeneratePaymentUrlAsync(activeSubscription)
                        };
                    }
                    else
                    {
                        // For failed/rejected payments, require payment
                        var paymentUrl = await GeneratePaymentUrlAsync(activeSubscription);
                        return SubscriptionAuthResult.PaymentRequired(paymentUrl, activeSubscription.PaymentStatus);
                    }
                }

                // Check subscription expiry
                if (activeSubscription.EndDate.HasValue && activeSubscription.EndDate.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("User {UserId} subscription has expired on {EndDate}", 
                        userId, activeSubscription.EndDate.Value);
                    
                    // Mark subscription as inactive
                    activeSubscription.IsActive = false;
                    await _context.SaveChangesAsync();
                    
                    return SubscriptionAuthResult.NoSubscription();
                }

                // Get user limits with current usage
                var limits = await GetUserLimitsWithUsageAsync(userId, activeSubscription.SubscriptionTier);

                return SubscriptionAuthResult.Success(activeSubscription.SubscriptionTier, limits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating subscription for user {UserId}", userId);
                return SubscriptionAuthResult.Unauthorized("Error validating subscription");
            }
        }



        public async Task<SubscriptionTier?> GetUserSubscriptionTierAsync(string userId)
        {
            var (user, tier) = await GetUserAndSubscriptionTierAsync(userId);
            return tier;
        }

        public async Task<SubscriptionLimits> GetUserLimitsAsync(string userId)
        {
            var (user, tier) = await GetUserAndSubscriptionTierAsync(userId);
            
            if (tier == null)
            {
                return await GetFreeTierLimitsAsync(userId);
            }

            return await GetUserLimitsWithUsageAsync(userId, tier);
        }

        private async Task<(ApplicationUser? user, SubscriptionTier? tier)> GetUserAndSubscriptionTierAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Subscriptions.Where(s => s.IsActive))
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Limits)
                .Include(u => u.Subscriptions.Where(s => s.IsActive))
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Features)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return (null, null);

            var activeSubscription = user.Subscriptions.FirstOrDefault(s => s.IsActive);
            SubscriptionTier? tier = null;

            // If no active subscription record, check if user has subscription flags set
            if (activeSubscription == null && user.HasActiveSubscription && !string.IsNullOrEmpty(user.CurrentSubscriptionTier))
            {
                var tierName = user.CurrentSubscriptionTier.ToUpper();
                tier = await _context.SubscriptionTiers
                    .Include(st => st.Limits)
                    .Include(st => st.Features)
                    .FirstOrDefaultAsync(st => st.Name.ToUpper() == tierName);
            }
            else
            {
                tier = activeSubscription?.SubscriptionTier;
            }

            return (user, tier);
        }

        public async Task<bool> CanAccessFeatureAsync(string userId, string featureKey)
        {
            var tier = await GetUserSubscriptionTierAsync(userId);
            
            if (tier == null) 
            {
                // Check if user has subscription flags set but no database record
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user?.HasActiveSubscription == true && !string.IsNullOrEmpty(user.CurrentSubscriptionTier))
                {
                    // For development/testing - allow features based on tier name
                    var tierName = user.CurrentSubscriptionTier.ToUpper();
                    
                    // Analytics access based on tier
                    if (featureKey == "BasicAnalytics" && (tierName == "BASIC" || tierName == "STANDARD" || tierName == "PREMIUM"))
                    {
                        return true;
                    }
                    
                    if (featureKey == "StandardAnalytics" && (tierName == "STANDARD" || tierName == "PREMIUM"))
                    {
                        return true;
                    }
                    
                    if (featureKey == "PremiumAnalytics" && tierName == "PREMIUM")
                    {
                        return true;
                    }
                    
                    // Other features based on tier
                    if (tierName == "PREMIUM")
                    {
                        var premiumFeatures = new[] { "BasicSupport", "PrioritySupport", "DedicatedSupport", 
                                                    "BasicAnalytics", "StandardAnalytics", "PremiumAnalytics", "FeaturedPlacement", "PDFUploads" };
                        if (premiumFeatures.Contains(featureKey))
                            return true;
                    }
                    else if (tierName == "STANDARD")
                    {
                        var standardFeatures = new[] { "BasicSupport", "PrioritySupport", "BasicAnalytics", "StandardAnalytics", "PDFUploads" };
                        if (standardFeatures.Contains(featureKey))
                            return true;
                    }
                    else if (tierName == "BASIC")
                    {
                        var basicFeatures = new[] { "BasicSupport", "BasicAnalytics" };
                        if (basicFeatures.Contains(featureKey))
                            return true;
                    }
                }
                
                return false;
            }

            var feature = tier.Features.FirstOrDefault(f => f.FeatureKey == featureKey);
            return feature?.IsEnabled ?? false;
        }

        public async Task<string> GetRedirectUrlForUserAsync(string userId)
        {
            var result = await ValidateUserSubscriptionAsync(userId);

            if (!result.IsAuthenticated)
            {
                return "/Auth/Login";
            }

            // Check if user has Client roles even without active subscription (for development/incomplete PayFast integration)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.HasActiveSubscription == true || !string.IsNullOrEmpty(user?.CurrentSubscriptionTier))
            {
                return "/Client/Dashboard";
            }

            if (!result.HasActiveSubscription)
            {
                return "/Client/Subscription";
            }

            if (!result.IsPaymentValid && !string.IsNullOrEmpty(result.RedirectUrl))
            {
                // Allow dashboard access for pending payments, redirect for failed/rejected
                if (result.PaymentStatus?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return "/Client/Dashboard";
                }
                return result.RedirectUrl;
            }

            return "/Client/Dashboard";
        }

        private bool IsPaymentStatusValid(string paymentStatus)
        {
            var validStatuses = new[] { "Completed", "Active", "Paid" };
            return validStatuses.Contains(paymentStatus, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<string> GeneratePaymentUrlAsync(Subscription subscription)
        {
            try
            {
                // Get the subscription tier and user for payment URL generation
                var subscriptionWithDetails = await _context.Subscriptions
                    .Include(s => s.SubscriptionTier)
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == subscription.Id);

                if (subscriptionWithDetails?.SubscriptionTier != null && subscriptionWithDetails.User != null)
                {
                    // Use the existing RegistrationService to generate PayFast URL
                    return await _registrationService.GeneratePayFastPaymentDataAsync(
                        subscriptionWithDetails.SubscriptionTier, 
                        subscriptionWithDetails.User);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PayFast URL for subscription {SubscriptionId}", subscription.Id);
            }

            // Fallback to a generic payment processing URL
            return $"/Payment/Process?subscriptionId={subscription.Id}";
        }

        private async Task<SubscriptionLimits> GetUserLimitsWithUsageAsync(string userId, SubscriptionTier tier)
        {
            // Get current usage counts
            var businessCount = await _context.Businesses
                .CountAsync(b => b.UserId == userId && b.Status != "Deleted");

            var imageCount = await _context.BusinessImages
                .CountAsync(bi => bi.Business.UserId == userId);

            // PDF count would be implemented when PDF upload feature is added
            var pdfCount = 0;

            var limits = new SubscriptionLimits
            {
                MaxBusinesses = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxBusinesses")?.LimitValue ?? 1,
                MaxImages = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxImages")?.LimitValue ?? 5,
                MaxPDFs = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxPDFs")?.LimitValue ?? 0,
                HasBasicSupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "BasicSupport")?.IsEnabled ?? false,
                HasPrioritySupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "PrioritySupport")?.IsEnabled ?? false,
                HasDedicatedSupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "DedicatedSupport")?.IsEnabled ?? false,
                HasBasicAnalytics = tier.Features.FirstOrDefault(f => f.FeatureKey == "BasicAnalytics")?.IsEnabled ?? false,
                HasAdvancedAnalytics = tier.Features.FirstOrDefault(f => f.FeatureKey == "AdvancedAnalytics")?.IsEnabled ?? false,
                HasFeaturedPlacement = tier.Features.FirstOrDefault(f => f.FeatureKey == "FeaturedPlacement")?.IsEnabled ?? false,
                HasPDFUploads = tier.Features.FirstOrDefault(f => f.FeatureKey == "PDFUploads")?.IsEnabled ?? false,
                CurrentBusinessCount = businessCount,
                CurrentImageCount = imageCount,
                CurrentPDFCount = pdfCount
            };

            return limits;
        }

        private async Task<SubscriptionLimits> GetFreeTierLimitsAsync(string userId)
        {
            var businessCount = await _context.Businesses
                .CountAsync(b => b.UserId == userId && b.Status != "Deleted");

            var imageCount = await _context.BusinessImages
                .CountAsync(bi => bi.Business.UserId == userId);

            return new SubscriptionLimits
            {
                MaxBusinesses = 1,
                MaxImages = 3,
                MaxPDFs = 0,
                HasBasicSupport = false,
                HasPrioritySupport = false,
                HasDedicatedSupport = false,
                HasBasicAnalytics = false,
                HasAdvancedAnalytics = false,
                HasFeaturedPlacement = false,
                HasPDFUploads = false,
                CurrentBusinessCount = businessCount,
                CurrentImageCount = imageCount,
                CurrentPDFCount = 0
            };
        }
    }
}