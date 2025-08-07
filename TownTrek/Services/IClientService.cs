using TownTrek.Models.ViewModels;

namespace TownTrek.Services
{
    public interface IClientService
    {
        Task<ClientDashboardViewModel> GetDashboardViewModelAsync(string userId);
        Task<ClientSubscriptionViewModel> GetSubscriptionViewModelAsync(string userId);
        Task<ClientAnalyticsViewModel> GetAnalyticsViewModelAsync(string userId);
        Task<AddBusinessViewModel> PrepareEditBusinessViewModelAsync(int businessId, string userId);
        Task<AddBusinessViewModel> PrepareEditBusinessViewModelWithFormDataAsync(int businessId, string userId, AddBusinessViewModel formData);
    }
}