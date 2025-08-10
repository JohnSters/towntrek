using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    public interface ISubscriptionTierService
    {
        Task<List<SubscriptionTierViewModel>> GetAllTiersAsync();
        Task<SubscriptionTierViewModel?> GetTierByIdAsync(int id);
        Task<ServiceResult> CreateTierAsync(SubscriptionTierViewModel model, string adminUserId);
        Task<ServiceResult> UpdateTierAsync(SubscriptionTierViewModel model, string adminUserId);
        Task<ServiceResult> UpdateTierPriceAsync(PriceChangeViewModel model, string adminUserId);
        Task<ServiceResult> DeactivateTierAsync(int tierId, string adminUserId);
        Task<List<SubscriptionTier>> GetActiveTiersForRegistrationAsync();
        Task<PriceChangeViewModel> GetPriceChangeModelAsync(int tierId);
        Task<SubscriptionTierListViewModel> GetTierListViewModelAsync();
    }
}