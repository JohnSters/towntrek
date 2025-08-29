using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing subscription tiers and pricing
    /// </summary>
    public interface ISubscriptionTierService
    {
        /// <summary>
        /// Gets all subscription tiers
        /// </summary>
        Task<List<SubscriptionTierViewModel>> GetAllTiersAsync();
        
        /// <summary>
        /// Gets a subscription tier by its ID
        /// </summary>
        Task<SubscriptionTierViewModel?> GetTierByIdAsync(int id);
        
        /// <summary>
        /// Creates a new subscription tier
        /// </summary>
        Task<ServiceResult> CreateTierAsync(SubscriptionTierViewModel model, string adminUserId);
        
        /// <summary>
        /// Updates an existing subscription tier
        /// </summary>
        Task<ServiceResult> UpdateTierAsync(SubscriptionTierViewModel model, string adminUserId);
        
        /// <summary>
        /// Updates the price of a subscription tier
        /// </summary>
        Task<ServiceResult> UpdateTierPriceAsync(PriceChangeViewModel model, string adminUserId);
        
        /// <summary>
        /// Deactivates a subscription tier
        /// </summary>
        Task<ServiceResult> DeactivateTierAsync(int tierId, string adminUserId);
        
        /// <summary>
        /// Gets active subscription tiers available for registration
        /// </summary>
        Task<List<SubscriptionTier>> GetActiveTiersForRegistrationAsync();
        
        /// <summary>
        /// Gets the price change model for a specific tier
        /// </summary>
        Task<PriceChangeViewModel> GetPriceChangeModelAsync(int tierId);
        
        /// <summary>
        /// Gets the subscription tier list view model for admin interface
        /// </summary>
        Task<SubscriptionTierListViewModel> GetTierListViewModelAsync();
    }
}