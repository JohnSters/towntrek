using TownTrek.Services.Interfaces;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.ClientAnalytics.RealTime
{
    /// <summary>
    /// Background service for periodic real-time analytics updates
    /// </summary>
    public class ClientRealTimeBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ClientRealTimeBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<ClientRealTimeBackgroundService> _logger = logger;
        private readonly Dictionary<string, int> _userRefreshIntervals = [];
        private readonly Dictionary<string, DateTime> _lastUpdateTimes = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RealTimeAnalyticsBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRealTimeUpdatesAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Check every 30 seconds
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RealTimeAnalyticsBackgroundService");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait longer on error
                }
            }

            _logger.LogInformation("RealTimeAnalyticsBackgroundService stopped");
        }

        private async Task ProcessRealTimeUpdatesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var realTimeService = scope.ServiceProvider.GetRequiredService<IRealTimeAnalyticsService>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IClientAnalyticsService>();

            // Get users with active refresh intervals
            var activeUsers = _userRefreshIntervals.Keys.ToList();
            var now = DateTime.UtcNow;

            foreach (var userId in activeUsers)
            {
                try
                {
                    var refreshInterval = GetUserRefreshInterval(userId);
                    if (refreshInterval <= 0) continue;

                    var lastUpdate = GetLastUpdateTime(userId);
                    if (now - lastUpdate >= TimeSpan.FromSeconds(refreshInterval))
                    {
                        await SendAnalyticsUpdateAsync(userId, realTimeService, analyticsService);
                        UpdateLastUpdateTime(userId, now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing real-time update for user {UserId}", userId);
                }
            }
        }

        private async Task SendAnalyticsUpdateAsync(string userId, IRealTimeAnalyticsService realTimeService, IClientAnalyticsService analyticsService)
        {
            try
            {
                // Get updated analytics data
                var analytics = await analyticsService.GetClientAnalyticsAsync(userId);
                
                // Send real-time update
                await realTimeService.SendClientAnalyticsUpdateAsync(userId, analytics);

                // Check for significant changes and send notifications
                await CheckForSignificantChangesAsync(userId, analytics, realTimeService, analyticsService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending analytics update for user {UserId}", userId);
            }
        }

        private async Task CheckForSignificantChangesAsync(string userId, ClientAnalyticsViewModel analytics, IRealTimeAnalyticsService realTimeService, IClientAnalyticsService analyticsService)
        {
            try
            {
                // Check for significant view increases
                foreach (var businessAnalytics in analytics.BusinessAnalytics)
                {
                    var previousViews = await GetPreviousViewsAsync(businessAnalytics.BusinessId);
                    var currentViews = businessAnalytics.TotalViews;
                    
                    if (currentViews > previousViews)
                    {
                        var increase = currentViews - previousViews;
                        var percentageIncrease = (double)increase / previousViews * 100;
                        
                        if (percentageIncrease >= 10) // 10% or more increase
                        {
                            await realTimeService.SendAnalyticsNotificationAsync(
                                userId,
                                "Views Surge! 📈",
                                $"{businessAnalytics.BusinessName} had a {percentageIncrease:F1}% increase in views!",
                                "success"
                            );
                        }
                    }
                }

                // Check for new reviews
                foreach (var businessAnalytics in analytics.BusinessAnalytics)
                {
                    var previousReviews = await GetPreviousReviewsAsync(businessAnalytics.BusinessId);
                    var currentReviews = businessAnalytics.TotalReviews;
                    
                    if (currentReviews > previousReviews)
                    {
                        var newReviews = currentReviews - previousReviews;
                        await realTimeService.SendAnalyticsNotificationAsync(
                            userId,
                            "New Reviews! ⭐",
                            $"{businessAnalytics.BusinessName} received {newReviews} new review{(newReviews > 1 ? "s" : "")}!",
                            "info"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for significant changes for user {UserId}", userId);
            }
        }

        private Task<int> GetPreviousViewsAsync(int businessId)
        {
            // This would typically query a cache or database for previous values
            // For now, return 0 as placeholder
            return Task.FromResult(0);
        }

        private Task<int> GetPreviousReviewsAsync(int businessId)
        {
            // This would typically query a cache or database for previous values
            // For now, return 0 as placeholder
            return Task.FromResult(0);
        }

        public void SetUserRefreshInterval(string userId, int intervalSeconds)
        {
            _userRefreshIntervals[userId] = intervalSeconds;
            _logger.LogInformation("Set refresh interval for user {UserId} to {Interval}s", userId, intervalSeconds);
        }

        public int GetUserRefreshInterval(string userId)
        {
            return _userRefreshIntervals.TryGetValue(userId, out var interval) ? interval : 0;
        }

        private DateTime GetLastUpdateTime(string userId)
        {
            return _lastUpdateTimes.TryGetValue(userId, out var time) ? time : DateTime.MinValue;
        }

        private void UpdateLastUpdateTime(string userId, DateTime time)
        {
            _lastUpdateTimes[userId] = time;
        }

        public void RemoveUser(string userId)
        {
            _userRefreshIntervals.Remove(userId);
            _lastUpdateTimes.Remove(userId);
            _logger.LogInformation("Removed user {UserId} from real-time updates", userId);
        }
    }
}
