using TownTrek.Services.Interfaces;

namespace TownTrek.Services.SharedAnalytics
{
    /// <summary>
    /// Background service for creating daily analytics snapshots
    /// </summary>
    public class AnalyticsSnapshotBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnalyticsSnapshotBackgroundService> _logger;
        private readonly TimeSpan _dailyRunTime = new(2, 0, 0); // Run at 2 AM UTC
        
        // Performance monitoring
        private DateTime _lastSuccessfulRun = DateTime.MinValue;
        private int _consecutiveFailures = 0;
        private readonly int _maxConsecutiveFailures = 3;
        private readonly TimeSpan _baseRetryDelay = TimeSpan.FromMinutes(15);

        public AnalyticsSnapshotBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AnalyticsSnapshotBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Snapshot Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var nextRun = now.Date.Add(_dailyRunTime);
                    
                    // If it's already past today's run time, schedule for tomorrow
                    if (now >= nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation("Next analytics snapshot run scheduled for {NextRun} (in {Delay} hours)", 
                        nextRun, delay.TotalHours.ToString("F1"));

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await CreateDailySnapshotsAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Service is being stopped
                    break;
                }
                catch (Exception ex)
                {
                    _consecutiveFailures++;
                    _logger.LogError(ex, "Error in analytics snapshot background service (Failure #{FailureCount})", _consecutiveFailures);
                    
                    // Exponential backoff with maximum delay of 2 hours
                    var retryDelay = TimeSpan.FromTicks(_baseRetryDelay.Ticks * (long)Math.Pow(2, Math.Min(_consecutiveFailures - 1, 4)));
                    retryDelay = TimeSpan.FromTicks(Math.Min(retryDelay.Ticks, TimeSpan.FromHours(2).Ticks));
                    
                    _logger.LogWarning("Retrying in {RetryDelay} minutes", retryDelay.TotalMinutes);
                    await Task.Delay(retryDelay, stoppingToken);
                }
            }

            _logger.LogInformation("Analytics Snapshot Background Service stopped");
        }

        private async Task CreateDailySnapshotsAsync()
        {
            var startTime = DateTime.UtcNow;
            var memoryBefore = GC.GetTotalMemory(false);
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var snapshotService = scope.ServiceProvider.GetRequiredService<IAnalyticsSnapshotService>();

                _logger.LogInformation("Starting daily analytics snapshot creation");
                
                var snapshotsCreated = await snapshotService.CreateDailySnapshotsAsync();
                
                // Reset failure counter on success
                _consecutiveFailures = 0;
                _lastSuccessfulRun = DateTime.UtcNow;
                
                var duration = DateTime.UtcNow - startTime;
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                _logger.LogInformation("Daily analytics snapshot creation completed. Created {Count} snapshots in {Duration}ms (Memory: {MemoryUsed}KB)", 
                    snapshotsCreated, duration.TotalMilliseconds, memoryUsed / 1024);

                // Also run cleanup of old snapshots (keep 2 years)
                var cleanupStartTime = DateTime.UtcNow;
                var snapshotsDeleted = await snapshotService.CleanupOldSnapshotsAsync(730);
                var cleanupDuration = DateTime.UtcNow - cleanupStartTime;
                
                if (snapshotsDeleted > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} old analytics snapshots in {Duration}ms", 
                        snapshotsDeleted, cleanupDuration.TotalMilliseconds);
                }
                
                // Log performance metrics
                _logger.LogInformation("Snapshot service performance - Total duration: {TotalDuration}ms, Memory used: {MemoryUsed}KB, Success rate: {SuccessRate}%", 
                    (DateTime.UtcNow - startTime).TotalMilliseconds, memoryUsed / 1024, 
                    _lastSuccessfulRun != DateTime.MinValue ? 100 : 0);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                _logger.LogError(ex, "Error creating daily analytics snapshots after {Duration}ms (Memory: {MemoryUsed}KB)", 
                    duration.TotalMilliseconds, memoryUsed / 1024);
                
                // Check if we should stop retrying
                if (_consecutiveFailures >= _maxConsecutiveFailures)
                {
                    _logger.LogCritical("Maximum consecutive failures ({MaxFailures}) reached. Service may need manual intervention.", _maxConsecutiveFailures);
                }
            }
        }
        
        /// <summary>
        /// Get service health status for monitoring
        /// </summary>
        public (bool IsHealthy, string Status, DateTime LastRun, int Failures) GetHealthStatus()
        {
            var isHealthy = _consecutiveFailures < _maxConsecutiveFailures;
            var status = isHealthy ? "Healthy" : "Degraded";
            
            return (isHealthy, status, _lastSuccessfulRun, _consecutiveFailures);
        }
    }
}
