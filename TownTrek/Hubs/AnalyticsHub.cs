using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace TownTrek.Hubs
{
    /// <summary>
    /// SignalR hub for real-time analytics updates with improved connection management
    /// </summary>
    public class AnalyticsHub : Hub
    {
        private readonly ILogger<AnalyticsHub> _logger;
        private static readonly ConcurrentDictionary<string, ConnectionInfo> _activeConnections = new();
        private static readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(100, 100); // Max 100 concurrent connections

        public AnalyticsHub(ILogger<AnalyticsHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthenticated connection attempt from {ConnectionId}", connectionId);
                Context.Abort();
                return;
            }

            // Check connection limits
            if (!await _connectionSemaphore.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                _logger.LogWarning("Connection limit reached for user {UserId}", userId);
                Context.Abort();
                return;
            }

            try
            {
                // Track connection info
                var connectionInfo = new ConnectionInfo
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    Groups = new HashSet<string>()
                };

                _activeConnections.TryAdd(connectionId, connectionInfo);

                // Add user to their personal group for analytics updates
                await Groups.AddToGroupAsync(connectionId, $"analytics_{userId}");
                connectionInfo.Groups.Add($"analytics_{userId}");

                _logger.LogInformation("User {UserId} connected to AnalyticsHub (Connection: {ConnectionId}, Active: {ActiveCount})", 
                    userId, connectionId, _activeConnections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during connection setup for user {UserId}", userId);
                _connectionSemaphore.Release();
                throw;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var connectionId = Context.ConnectionId;

            try
            {
                // Clean up connection tracking
                if (_activeConnections.TryRemove(connectionId, out var connectionInfo))
                {
                    // Remove from all groups
                    foreach (var group in connectionInfo.Groups)
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, group);
                    }

                    _logger.LogInformation("User {UserId} disconnected from AnalyticsHub (Connection: {ConnectionId}, Active: {ActiveCount})", 
                        userId, connectionId, _activeConnections.Count);
                }

                // Release connection semaphore
                _connectionSemaphore.Release();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnection cleanup for user {UserId}", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific business analytics group with improved error handling
        /// </summary>
        public async Task JoinBusinessGroup(int businessId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthenticated group join attempt for business {BusinessId}", businessId);
                return;
            }

            try
            {
                var groupName = $"business_{businessId}_{userId}";
                await Groups.AddToGroupAsync(connectionId, groupName);

                // Update connection tracking
                if (_activeConnections.TryGetValue(connectionId, out var connectionInfo))
                {
                    connectionInfo.Groups.Add(groupName);
                    connectionInfo.LastActivity = DateTime.UtcNow;
                }

                _logger.LogInformation("User {UserId} joined business group {BusinessId} (Connection: {ConnectionId})", 
                    userId, businessId, connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining business group {BusinessId} for user {UserId}", businessId, userId);
                throw;
            }
        }

        /// <summary>
        /// Leave a specific business analytics group with improved error handling
        /// </summary>
        public async Task LeaveBusinessGroup(int businessId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthenticated group leave attempt for business {BusinessId}", businessId);
                return;
            }

            try
            {
                var groupName = $"business_{businessId}_{userId}";
                await Groups.RemoveFromGroupAsync(connectionId, groupName);

                // Update connection tracking
                if (_activeConnections.TryGetValue(connectionId, out var connectionInfo))
                {
                    connectionInfo.Groups.Remove(groupName);
                    connectionInfo.LastActivity = DateTime.UtcNow;
                }

                _logger.LogInformation("User {UserId} left business group {BusinessId} (Connection: {ConnectionId})", 
                    userId, businessId, connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving business group {BusinessId} for user {UserId}", businessId, userId);
                throw;
            }
        }

        /// <summary>
        /// Ping method for connection health checks
        /// </summary>
        public Task Ping()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                // Update connection tracking
                if (_activeConnections.TryGetValue(connectionId, out var connectionInfo))
                {
                    connectionInfo.LastActivity = DateTime.UtcNow;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Update refresh interval for a user with validation
        /// </summary>
        public Task UpdateRefreshInterval(int intervalSeconds)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthenticated refresh interval update attempt");
                return Task.CompletedTask;
            }

            // Validate interval
            if (intervalSeconds < 0 || intervalSeconds > 3600) // Max 1 hour
            {
                _logger.LogWarning("Invalid refresh interval {Interval} for user {UserId}", intervalSeconds, userId);
                return Task.CompletedTask;
            }

            try
            {
                // Store the refresh interval in the connection metadata
                Context.Items["RefreshInterval"] = intervalSeconds;

                // Update connection tracking
                if (_activeConnections.TryGetValue(connectionId, out var connectionInfo))
                {
                    connectionInfo.LastActivity = DateTime.UtcNow;
                }

                _logger.LogInformation("User {UserId} updated refresh interval to {Interval}s (Connection: {ConnectionId})", 
                    userId, intervalSeconds, connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refresh interval for user {UserId}", userId);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get connection statistics for monitoring
        /// </summary>
        public static ConnectionStatistics GetConnectionStatistics()
        {
            var now = DateTime.UtcNow;
            var activeConnections = _activeConnections.Values.ToList();
            
            return new ConnectionStatistics
            {
                TotalConnections = activeConnections.Count,
                ActiveConnections = activeConnections.Count(c => now.Subtract(c.LastActivity).TotalMinutes < 5),
                AverageGroupsPerConnection = activeConnections.Any() ? activeConnections.Average(c => c.Groups.Count) : 0,
                OldestConnection = activeConnections.Any() ? activeConnections.Min(c => c.ConnectedAt) : DateTime.UtcNow
            };
        }

        /// <summary>
        /// Clean up stale connections (called by background service)
        /// </summary>
        public static void CleanupStaleConnections()
        {
            var now = DateTime.UtcNow;
            var staleConnections = _activeConnections
                .Where(kvp => now.Subtract(kvp.Value.LastActivity).TotalMinutes > 30)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var connectionId in staleConnections)
            {
                _activeConnections.TryRemove(connectionId, out _);
            }

            if (staleConnections.Any())
            {
                // Note: Actual connection cleanup is handled by SignalR's built-in mechanisms
                // This just cleans up our tracking
            }
        }
    }

    /// <summary>
    /// Connection information for tracking
    /// </summary>
    public class ConnectionInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public HashSet<string> Groups { get; set; } = new();
    }

    /// <summary>
    /// Connection statistics for monitoring
    /// </summary>
    public class ConnectionStatistics
    {
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public double AverageGroupsPerConnection { get; set; }
        public DateTime OldestConnection { get; set; }
    }
}
