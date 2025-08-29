using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing admin messages and communication between clients and administrators
    /// </summary>
    public interface IAdminMessageService
    {
        /// <summary>
        /// Gets the contact admin view model for a user
        /// </summary>
        Task<ContactAdminViewModel> GetContactAdminViewModelAsync(string userId);
        
        /// <summary>
        /// Creates a new admin message from a user
        /// </summary>
        Task<AdminMessage> CreateMessageAsync(string userId, int topicId, string subject, string message);
        
        /// <summary>
        /// Gets all messages for a specific user
        /// </summary>
        Task<List<AdminMessage>> GetUserMessagesAsync(string userId);
        
        /// <summary>
        /// Gets a message topic by its ID
        /// </summary>
        Task<AdminMessageTopic?> GetTopicByIdAsync(int topicId);
        
        /// <summary>
        /// Gets the admin messages view model with optional filters
        /// </summary>
        Task<AdminMessagesViewModel> GetAdminMessagesViewModelAsync(AdminMessageFilters? filters = null);
        
        /// <summary>
        /// Gets detailed view model for a specific message
        /// </summary>
        Task<AdminMessageDetailsViewModel> GetMessageDetailsAsync(int messageId);
        
        /// <summary>
        /// Updates the status of a message
        /// </summary>
        Task<bool> UpdateMessageStatusAsync(int messageId, string status, string? adminUserId = null);
        
        /// <summary>
        /// Responds to a user message
        /// </summary>
        Task<bool> RespondToMessageAsync(int messageId, string response, string adminUserId);
        
        /// <summary>
        /// Deletes a message
        /// </summary>
        Task<bool> DeleteMessageAsync(int messageId);
        
        /// <summary>
        /// Gets message statistics for admin dashboard
        /// </summary>
        Task<AdminMessageStats> GetMessageStatsAsync();
        
        /// <summary>
        /// Gets all active message topics
        /// </summary>
        Task<List<AdminMessageTopic>> GetActiveTopicsAsync();
        
        /// <summary>
        /// Gets recent messages for quick overview
        /// </summary>
        Task<List<AdminMessage>> GetRecentMessagesAsync(int count = 5);
    }
}