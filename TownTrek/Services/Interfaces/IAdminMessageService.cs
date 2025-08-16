using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    public interface IAdminMessageService
    {
        // Client-side methods
        Task<ContactAdminViewModel> GetContactAdminViewModelAsync(string userId);
        Task<AdminMessage> CreateMessageAsync(string userId, int topicId, string subject, string message);
        Task<List<AdminMessage>> GetUserMessagesAsync(string userId);
        Task<AdminMessageTopic?> GetTopicByIdAsync(int topicId);
        
        // Admin-side methods
        Task<AdminMessagesViewModel> GetAdminMessagesViewModelAsync(AdminMessageFilters? filters = null);
        Task<AdminMessageDetailsViewModel> GetMessageDetailsAsync(int messageId);
        Task<bool> UpdateMessageStatusAsync(int messageId, string status, string? adminUserId = null);
        Task<bool> RespondToMessageAsync(int messageId, string response, string adminUserId);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<AdminMessageStats> GetMessageStatsAsync();
        
        // Utility methods
        Task<List<AdminMessageTopic>> GetActiveTopicsAsync();
        Task<List<AdminMessage>> GetRecentMessagesAsync(int count = 5);
    }
}