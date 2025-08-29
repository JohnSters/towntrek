using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for client-side operations and dashboard functionality
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Gets the client dashboard view model for a user
        /// </summary>
        Task<ClientDashboardViewModel> GetDashboardViewModelAsync(string userId);
        
        /// <summary>
        /// Gets the client subscription view model for a user
        /// </summary>
        Task<ClientSubscriptionViewModel> GetSubscriptionViewModelAsync(string userId);
        
        /// <summary>
        /// Gets the client analytics view model for a user
        /// </summary>
        Task<ClientAnalyticsViewModel> GetAnalyticsViewModelAsync(string userId);
        
        /// <summary>
        /// Prepares the edit business view model for a user
        /// </summary>
        Task<AddBusinessViewModel> PrepareEditBusinessViewModelAsync(int businessId, string userId);
        
        /// <summary>
        /// Prepares the edit business view model with form data for a user
        /// </summary>
        Task<AddBusinessViewModel> PrepareEditBusinessViewModelWithFormDataAsync(int businessId, string userId, AddBusinessViewModel formData);
        
        /// <summary>
        /// Gets the contact admin view model for a user
        /// </summary>
        Task<ContactAdminViewModel> GetContactAdminViewModelAsync(string userId);
        
        /// <summary>
        /// Creates an admin message from a user
        /// </summary>
        Task<AdminMessage> CreateAdminMessageAsync(string userId, int topicId, string subject, string message);
        
        /// <summary>
        /// Gets an admin message topic by ID
        /// </summary>
        Task<AdminMessageTopic?> GetAdminMessageTopicAsync(int topicId);
    }
}