using Microsoft.AspNetCore.SignalR;
using TownTrek.Hubs;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    /// <summary>
    /// Service for managing real-time analytics updates via SignalR
    /// </summary>
    public class RealTimeAnalyticsService : IRealTimeAnalyticsService
    {
        private readonly IHubContext<AnalyticsHub> _hubContext;
        private readonly ILogger<RealTimeAnalyticsService> _logger;

        public RealTimeAnalyticsService(
            IHubContext<AnalyticsHub> hubContext,
            ILogger<RealTimeAnalyticsService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendClientAnalyticsUpdateAsync(string userId, ClientAnalyticsViewModel analytics)
        {
            try
            {
                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceiveClientAnalyticsUpdate", analytics);
                
                _logger.LogInformation("Sent client analytics update to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending client analytics update to user {UserId}", userId);
            }
        }

        public async Task SendBusinessAnalyticsUpdateAsync(int businessId, string userId, BusinessAnalyticsData analytics)
        {
            try
            {
                await _hubContext.Clients.Group($"business_{businessId}_{userId}")
                    .SendAsync("ReceiveBusinessAnalyticsUpdate", businessId, analytics);
                
                _logger.LogInformation("Sent business analytics update for business {BusinessId} to user {UserId}", businessId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending business analytics update for business {BusinessId} to user {UserId}", businessId, userId);
            }
        }

        public async Task SendViewsChartUpdateAsync(string userId, ViewsChartDataResponse chartData)
        {
            try
            {
                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceiveViewsChartUpdate", chartData);
                
                _logger.LogInformation("Sent views chart update to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending views chart update to user {UserId}", userId);
            }
        }

        public async Task SendReviewsChartUpdateAsync(string userId, ReviewsChartDataResponse chartData)
        {
            try
            {
                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceiveReviewsChartUpdate", chartData);
                
                _logger.LogInformation("Sent reviews chart update to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reviews chart update to user {UserId}", userId);
            }
        }

        public async Task SendAnalyticsNotificationAsync(string userId, string title, string message, string type = "info")
        {
            try
            {
                var notification = new
                {
                    title,
                    message,
                    type,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceiveAnalyticsNotification", notification);
                
                _logger.LogInformation("Sent analytics notification to user {UserId}: {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending analytics notification to user {UserId}", userId);
            }
        }

        public async Task SendPerformanceInsightsUpdateAsync(string userId, List<BusinessPerformanceInsight> insights)
        {
            try
            {
                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceivePerformanceInsightsUpdate", insights);
                
                _logger.LogInformation("Sent performance insights update to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending performance insights update to user {UserId}", userId);
            }
        }

        public async Task SendCompetitorInsightsUpdateAsync(string userId, List<CompetitorInsight> insights)
        {
            try
            {
                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceiveCompetitorInsightsUpdate", insights);
                
                _logger.LogInformation("Sent competitor insights update to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending competitor insights update to user {UserId}", userId);
            }
        }

        public async Task SendCategoryBenchmarksUpdateAsync(string userId, string category, CategoryBenchmarkData benchmarks)
        {
            try
            {
                await _hubContext.Clients.Group($"analytics_{userId}")
                    .SendAsync("ReceiveCategoryBenchmarksUpdate", category, benchmarks);
                
                _logger.LogInformation("Sent category benchmarks update for {Category} to user {UserId}", category, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending category benchmarks update for {Category} to user {UserId}", category, userId);
            }
        }

        public async Task BroadcastAnalyticsUpdateAsync(string message, object data)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveBroadcastUpdate", message, data);
                _logger.LogInformation("Broadcasted analytics update: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting analytics update: {Message}", message);
            }
        }

        public Task<int> GetActiveConnectionsCountAsync()
        {
            try
            {
                // Note: This is a simplified implementation
                // In a production environment, you might want to track connections more precisely
                var connections = _hubContext.Clients.All;
                // This is a placeholder - actual connection counting would require additional infrastructure
                return Task.FromResult(0); // Placeholder return
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active connections count");
                return Task.FromResult(0);
            }
        }
    }
}
