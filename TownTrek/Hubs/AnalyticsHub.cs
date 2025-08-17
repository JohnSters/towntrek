using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TownTrek.Hubs
{
    /// <summary>
    /// SignalR hub for real-time analytics updates
    /// </summary>
    public class AnalyticsHub : Hub
    {
        private readonly ILogger<AnalyticsHub> _logger;

        public AnalyticsHub(ILogger<AnalyticsHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group for analytics updates
                await Groups.AddToGroupAsync(Context.ConnectionId, $"analytics_{userId}");
                _logger.LogInformation("User {UserId} connected to AnalyticsHub", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"analytics_{userId}");
                _logger.LogInformation("User {UserId} disconnected from AnalyticsHub", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific business analytics group
        /// </summary>
        public async Task JoinBusinessGroup(int businessId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"business_{businessId}_{userId}");
                _logger.LogInformation("User {UserId} joined business group {BusinessId}", userId, businessId);
            }
        }

        /// <summary>
        /// Leave a specific business analytics group
        /// </summary>
        public async Task LeaveBusinessGroup(int businessId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"business_{businessId}_{userId}");
                _logger.LogInformation("User {UserId} left business group {BusinessId}", userId, businessId);
            }
        }

        /// <summary>
        /// Update refresh interval for a user
        /// </summary>
        public async Task UpdateRefreshInterval(int intervalSeconds)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Store the refresh interval in the connection metadata
                Context.Items["RefreshInterval"] = intervalSeconds;
                _logger.LogInformation("User {UserId} updated refresh interval to {Interval}s", userId, intervalSeconds);
            }
        }
    }
}
