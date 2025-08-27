using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TownTrek.Hubs;

namespace TownTrek.Services.AdminAnalytics
{
    /// <summary>
    /// Background service to clean up stale SignalR connections
    /// </summary>
    public class AdminConnectionCleanupBackgroundService(ILogger<AdminConnectionCleanupBackgroundService> logger) : BackgroundService
    {
        private readonly ILogger<AdminConnectionCleanupBackgroundService> _logger = logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Clean up every 5 minutes
        
        // Performance monitoring
        private DateTime _lastSuccessfulRun = DateTime.MinValue;
        private int _consecutiveFailures = 0;
        private readonly int _maxConsecutiveFailures = 5;
        private readonly TimeSpan _baseRetryDelay = TimeSpan.FromMinutes(2);
        private long _totalConnectionsCleaned = 0;
        private long _totalCleanupRuns = 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Connection Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformConnectionCleanupAsync();
                    
                    // Reset failure counter on success
                    _consecutiveFailures = 0;
                    _lastSuccessfulRun = DateTime.UtcNow;
                    _totalCleanupRuns++;
                }
                catch (OperationCanceledException)
                {
                    // Service is being stopped
                    break;
                }
                catch (Exception ex)
                {
                    _consecutiveFailures++;
                    _logger.LogError(ex, "Error during connection cleanup (Failure #{FailureCount})", _consecutiveFailures);
                    
                    // Exponential backoff with maximum delay of 15 minutes
                    var retryDelay = TimeSpan.FromTicks(_baseRetryDelay.Ticks * (long)Math.Pow(2, Math.Min(_consecutiveFailures - 1, 3)));
                    retryDelay = TimeSpan.FromTicks(Math.Min(retryDelay.Ticks, TimeSpan.FromMinutes(15).Ticks));
                    
                    _logger.LogWarning("Retrying connection cleanup in {RetryDelay} minutes", retryDelay.TotalMinutes);
                    await Task.Delay(retryDelay, stoppingToken);
                    
                    // Check if we should stop retrying
                    if (_consecutiveFailures >= _maxConsecutiveFailures)
                    {
                        _logger.LogCritical("Maximum consecutive failures ({MaxFailures}) reached for connection cleanup. Service may need manual intervention.", _maxConsecutiveFailures);
                    }
                }

                // Wait for next cleanup cycle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Analytics Connection Cleanup Service stopped");
        }

        private Task PerformConnectionCleanupAsync()
        {
            var startTime = DateTime.UtcNow;
            var memoryBefore = GC.GetTotalMemory(false);
            
            try
            {
                // Get connection statistics before cleanup
                var statsBefore = AnalyticsHub.GetConnectionStatistics();
                
                // Perform cleanup
                AnalyticsHub.CleanupStaleConnections();
                
                // Get connection statistics after cleanup
                var statsAfter = AnalyticsHub.GetConnectionStatistics();
                
                // Calculate cleanup results
                var cleanedCount = statsBefore.TotalConnections - statsAfter.TotalConnections;
                _totalConnectionsCleaned += cleanedCount;
                
                var duration = DateTime.UtcNow - startTime;
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                // Log cleanup results with performance metrics
                if (cleanedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {CleanedCount} stale connections in {Duration}ms (Memory: {MemoryUsed}KB). Active: {ActiveCount}, Total: {TotalCount}", 
                        cleanedCount, duration.TotalMilliseconds, memoryUsed / 1024, statsAfter.ActiveConnections, statsAfter.TotalConnections);
                }
                else
                {
                    _logger.LogDebug("No stale connections found in {Duration}ms (Memory: {MemoryUsed}KB). Active: {ActiveCount}, Total: {TotalCount}", 
                        duration.TotalMilliseconds, memoryUsed / 1024, statsAfter.ActiveConnections, statsAfter.TotalConnections);
                }
                
                // Log performance metrics
                _logger.LogInformation("Connection cleanup performance - Duration: {Duration}ms, Memory used: {MemoryUsed}KB, Total cleaned: {TotalCleaned}, Success rate: {SuccessRate}%", 
                    duration.TotalMilliseconds, memoryUsed / 1024, _totalConnectionsCleaned, 
                    _lastSuccessfulRun != DateTime.MinValue ? 100 : 0);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                _logger.LogError(ex, "Error during connection cleanup after {Duration}ms (Memory: {MemoryUsed}KB)", 
                    duration.TotalMilliseconds, memoryUsed / 1024);
                throw; // Re-throw to trigger retry logic
            }

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Get service health status for monitoring
        /// </summary>
        public (bool IsHealthy, string Status, DateTime LastRun, int Failures, long TotalCleaned, long TotalRuns) GetHealthStatus()
        {
            var isHealthy = _consecutiveFailures < _maxConsecutiveFailures;
            var status = isHealthy ? "Healthy" : "Degraded";
            
            return (isHealthy, status, _lastSuccessfulRun, _consecutiveFailures, _totalConnectionsCleaned, _totalCleanupRuns);
        }
    }
}
