using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    public interface IClientService
    {
        Task<ClientDashboardViewModel> GetDashboardViewModelAsync(string userId);
        Task<ClientSubscriptionViewModel> GetSubscriptionViewModelAsync(string userId);
        Task<ClientAnalyticsViewModel> GetAnalyticsViewModelAsync(string userId);
        Task<AddBusinessViewModel> PrepareEditBusinessViewModelAsync(int businessId, string userId);
        Task<AddBusinessViewModel> PrepareEditBusinessViewModelWithFormDataAsync(int businessId, string userId, AddBusinessViewModel formData);
        
        // Admin Message methods
        Task<ContactAdminViewModel> GetContactAdminViewModelAsync(string userId);
        Task<AdminMessage> CreateAdminMessageAsync(string userId, int topicId, string subject, string message);
        Task<AdminMessageTopic?> GetAdminMessageTopicAsync(int topicId);
    }
}