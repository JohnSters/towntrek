using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class SubscriptionManagementService : ISubscriptionManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriptionManagementService> _logger;

        public SubscriptionManagementService(ApplicationDbContext context, ILogger<SubscriptionManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AssignSubscriptionAsync(string userId, string tierName)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for subscription assignment", userId);
                    return false;
                }

                var tier = await _context.SubscriptionTiers
                    .FirstOrDefaultAsync(t => t.Name.ToUpper() == tierName.ToUpper() && t.IsActive);

                if (tier == null)
                {
                    _logger.LogWarning("Subscription tier {TierName} not found", tierName);
                    return false;
                }

                // Update user subscription flags
                user.HasActiveSubscription = true;
                user.CurrentSubscriptionTier = tier.Name;
                user.SubscriptionStartDate = DateTime.UtcNow;
                user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1); // Default to 1 month

                // Create or update subscription record
                var existingSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                if (existingSubscription != null)
                {
                    // Deactivate existing subscription
                    existingSubscription.IsActive = false;
                    existingSubscription.EndDate = DateTime.UtcNow;
                }

                // Create new subscription record
                var subscription = new Subscription
                {
                    UserId = userId,
                    SubscriptionTierId = tier.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true,
                    PaymentStatus = "Active", // For manual assignment
                    MonthlyPrice = tier.MonthlyPrice,
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully assigned {TierName} subscription to user {UserId}", tierName, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning subscription {TierName} to user {UserId}", tierName, userId);
                return false;
            }
        }

        public async Task<bool> ActivateSubscriptionAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.HasActiveSubscription = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Activated subscription for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateSubscriptionAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.HasActiveSubscription = false;
                user.SubscriptionEndDate = DateTime.UtcNow;

                // Deactivate subscription records
                var activeSubscriptions = await _context.Subscriptions
                    .Where(s => s.UserId == userId && s.IsActive)
                    .ToListAsync();

                foreach (var subscription in activeSubscriptions)
                {
                    subscription.IsActive = false;
                    subscription.EndDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated subscription for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateSubscriptionTierAsync(string userId, string newTierName)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var newTier = await _context.SubscriptionTiers
                    .FirstOrDefaultAsync(t => t.Name.ToUpper() == newTierName.ToUpper() && t.IsActive);

                if (newTier == null)
                {
                    _logger.LogWarning("New subscription tier {TierName} not found", newTierName);
                    return false;
                }

                // Update user tier
                user.CurrentSubscriptionTier = newTier.Name;

                // Update active subscription record
                var activeSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                if (activeSubscription != null)
                {
                    activeSubscription.SubscriptionTierId = newTier.Id;
                    activeSubscription.MonthlyPrice = newTier.MonthlyPrice;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated subscription tier to {TierName} for user {UserId}", newTierName, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription tier for user {UserId}", userId);
                return false;
            }
        }

        public async Task<Subscription?> CreateSubscriptionRecordAsync(string userId, int subscriptionTierId)
        {
            try
            {
                var tier = await _context.SubscriptionTiers.FindAsync(subscriptionTierId);
                if (tier == null) return null;

                var subscription = new Subscription
                {
                    UserId = userId,
                    SubscriptionTierId = subscriptionTierId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true,
                    PaymentStatus = "Pending",
                    MonthlyPrice = tier.MonthlyPrice,
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                return subscription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription record for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> SyncUserSubscriptionFlagsAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var activeSubscription = await _context.Subscriptions
                    .Include(s => s.SubscriptionTier)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                if (activeSubscription != null)
                {
                    // Sync user flags with subscription record
                    user.HasActiveSubscription = true;
                    user.CurrentSubscriptionTier = activeSubscription.SubscriptionTier.Name;
                    user.SubscriptionStartDate = activeSubscription.StartDate;
                    user.SubscriptionEndDate = activeSubscription.EndDate;
                }
                else
                {
                    // No active subscription found
                    user.HasActiveSubscription = false;
                    user.CurrentSubscriptionTier = null;
                    user.SubscriptionStartDate = null;
                    user.SubscriptionEndDate = null;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing subscription flags for user {UserId}", userId);
                return false;
            }
        }
    }
}