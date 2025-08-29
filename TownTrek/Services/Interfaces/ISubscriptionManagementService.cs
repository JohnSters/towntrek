using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing user subscriptions and subscription-related operations
    /// </summary>
    public interface ISubscriptionManagementService
    {
        /// <summary>
        /// Assigns a subscription tier to a user
        /// </summary>
        Task<bool> AssignSubscriptionAsync(string userId, string tierName);
        
        /// <summary>
        /// Activates a user's subscription
        /// </summary>
        Task<bool> ActivateSubscriptionAsync(string userId);
        
        /// <summary>
        /// Deactivates a user's subscription
        /// </summary>
        Task<bool> DeactivateSubscriptionAsync(string userId);
        
        /// <summary>
        /// Updates a user's subscription tier
        /// </summary>
        Task<bool> UpdateSubscriptionTierAsync(string userId, string newTierName);
        
        /// <summary>
        /// Creates a new subscription record for a user
        /// </summary>
        Task<Subscription?> CreateSubscriptionRecordAsync(string userId, int subscriptionTierId);
        
        /// <summary>
        /// Synchronizes user subscription flags with their current subscription status
        /// </summary>
        Task<bool> SyncUserSubscriptionFlagsAsync(string userId);
    }
}