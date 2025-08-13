using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    public interface ISubscriptionManagementService
    {
        Task<bool> AssignSubscriptionAsync(string userId, string tierName);
        Task<bool> ActivateSubscriptionAsync(string userId);
        Task<bool> DeactivateSubscriptionAsync(string userId);
        Task<bool> UpdateSubscriptionTierAsync(string userId, string newTierName);
        Task<Subscription?> CreateSubscriptionRecordAsync(string userId, int subscriptionTierId);
        Task<bool> SyncUserSubscriptionFlagsAsync(string userId);
    }
}