using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class AdminMessageService : IAdminMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminMessageService> _logger;

        public AdminMessageService(ApplicationDbContext context, ILogger<AdminMessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ContactAdminViewModel> GetContactAdminViewModelAsync(string userId)
        {
            var availableTopics = await GetActiveTopicsAsync();
            var userMessages = await GetUserMessagesAsync(userId);

            return new ContactAdminViewModel
            {
                AvailableTopics = availableTopics,
                UserMessages = userMessages.Take(5).ToList() // Show last 5 messages
            };
        }

        public async Task<AdminMessage> CreateMessageAsync(string userId, int topicId, string subject, string message)
        {
            var topic = await GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                throw new ArgumentException("Invalid topic selected");
            }

            var adminMessage = new AdminMessage
            {
                UserId = userId,
                TopicId = topicId,
                Subject = subject,
                Message = message,
                Status = "Open",
                Priority = topic.Priority,
                CreatedAt = DateTime.UtcNow
            };

            _context.AdminMessages.Add(adminMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New admin message created: {MessageId} by user {UserId}", adminMessage.Id, userId);

            return adminMessage;
        }

        public async Task<List<AdminMessage>> GetUserMessagesAsync(string userId)
        {
            return await _context.AdminMessages
                .Include(m => m.Topic)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<AdminMessageTopic?> GetTopicByIdAsync(int topicId)
        {
            return await _context.AdminMessageTopics
                .FirstOrDefaultAsync(t => t.Id == topicId && t.IsActive);
        }

        public async Task<AdminMessagesViewModel> GetAdminMessagesViewModelAsync(AdminMessageFilters? filters = null)
        {
            var query = _context.AdminMessages
                .Include(m => m.User)
                .Include(m => m.Topic)
                .Include(m => m.ResolvedByUser)
                .Include(m => m.ResponseByUser)
                .AsQueryable();

            // Apply filters
            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.Status))
                    query = query.Where(m => m.Status == filters.Status);

                if (!string.IsNullOrEmpty(filters.Priority))
                    query = query.Where(m => m.Priority == filters.Priority);

                if (filters.TopicId.HasValue)
                    query = query.Where(m => m.TopicId == filters.TopicId.Value);

                if (filters.FromDate.HasValue)
                    query = query.Where(m => m.CreatedAt >= filters.FromDate.Value);

                if (filters.ToDate.HasValue)
                    query = query.Where(m => m.CreatedAt <= filters.ToDate.Value);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    var searchTerm = filters.SearchTerm.ToLower();
                    query = query.Where(m => 
                        m.Subject.ToLower().Contains(searchTerm) ||
                        m.Message.ToLower().Contains(searchTerm) ||
                        m.User.Email.ToLower().Contains(searchTerm));
                }
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var stats = await GetMessageStatsAsync();
            var topics = await GetActiveTopicsAsync();

            return new AdminMessagesViewModel
            {
                Messages = messages,
                Stats = stats,
                Filters = filters ?? new AdminMessageFilters(),
                Topics = topics
            };
        }

        public async Task<AdminMessageDetailsViewModel> GetMessageDetailsAsync(int messageId)
        {
            var message = await _context.AdminMessages
                .Include(m => m.User)
                .Include(m => m.Topic)
                .Include(m => m.ResolvedByUser)
                .Include(m => m.ResponseByUser)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                throw new ArgumentException("Message not found");
            }

            // Get related messages from the same user
            var relatedMessages = await _context.AdminMessages
                .Include(m => m.Topic)
                .Where(m => m.UserId == message.UserId && m.Id != messageId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            return new AdminMessageDetailsViewModel
            {
                Message = message,
                RelatedMessages = relatedMessages
            };
        }

        public async Task<bool> UpdateMessageStatusAsync(int messageId, string status, string? adminUserId = null)
        {
            var message = await _context.AdminMessages.FindAsync(messageId);
            if (message == null) return false;

            message.Status = status;

            if (status == "Resolved" && adminUserId != null)
            {
                message.ResolvedAt = DateTime.UtcNow;
                message.ResolvedBy = adminUserId;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} status updated to {Status} by admin {AdminUserId}", 
                messageId, status, adminUserId);

            return true;
        }

        public async Task<bool> RespondToMessageAsync(int messageId, string response, string adminUserId)
        {
            var message = await _context.AdminMessages.FindAsync(messageId);
            if (message == null) return false;

            message.AdminResponse = response;
            message.ResponseAt = DateTime.UtcNow;
            message.ResponseBy = adminUserId;
            
            // Update status to InProgress if it was Open
            if (message.Status == "Open")
            {
                message.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Response added to message {MessageId} by admin {AdminUserId}", 
                messageId, adminUserId);

            return true;
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _context.AdminMessages.FindAsync(messageId);
            if (message == null) return false;

            _context.AdminMessages.Remove(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} deleted", messageId);

            return true;
        }

        public async Task<AdminMessageStats> GetMessageStatsAsync()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var stats = new AdminMessageStats
            {
                TotalMessages = await _context.AdminMessages.CountAsync(),
                OpenMessages = await _context.AdminMessages.CountAsync(m => m.Status == "Open"),
                InProgressMessages = await _context.AdminMessages.CountAsync(m => m.Status == "InProgress"),
                ResolvedMessages = await _context.AdminMessages.CountAsync(m => m.Status == "Resolved"),
                CriticalMessages = await _context.AdminMessages.CountAsync(m => m.Priority == "Critical"),
                HighPriorityMessages = await _context.AdminMessages.CountAsync(m => m.Priority == "High"),
                MediumPriorityMessages = await _context.AdminMessages.CountAsync(m => m.Priority == "Medium"),
                LowPriorityMessages = await _context.AdminMessages.CountAsync(m => m.Priority == "Low"),
                MessagesToday = await _context.AdminMessages.CountAsync(m => m.CreatedAt >= today),
                MessagesThisWeek = await _context.AdminMessages.CountAsync(m => m.CreatedAt >= weekStart)
            };

            return stats;
        }

        public async Task<List<AdminMessageTopic>> GetActiveTopicsAsync()
        {
            return await _context.AdminMessageTopics
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<List<AdminMessage>> GetRecentMessagesAsync(int count = 5)
        {
            return await _context.AdminMessages
                .Include(m => m.User)
                .Include(m => m.Topic)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}