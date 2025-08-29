using TownTrek.Services.Interfaces.AdminAnalytics;

namespace TownTrek.Services.AdminAnalytics
{
    /// <summary>
    /// Background service to automatically clean up old analytics audit logs
    /// </summary>
    public class AdminAuditCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdminAuditCleanupBackgroundService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(7); // Run weekly
        private readonly int _retentionDays = 365; // Keep logs for 1 year
        
        // Performance monitoring
        private DateTime _lastSuccessfulRun = DateTime.MinValue;
        private int _consecutiveFailures = 0;
        private readonly int _maxConsecutiveFailures = 3;
        private readonly TimeSpan _baseRetryDelay = TimeSpan.FromHours(1);

        public AdminAuditCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AdminAuditCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics audit cleanup background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldAuditLogsAsync();
                    
                    // Reset failure counter on success
                    _consecutiveFailures = 0;
                    _lastSuccessfulRun = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _consecutiveFailures++;
                    _logger.LogError(ex, "Error during analytics audit cleanup (Failure #{FailureCount})", _consecutiveFailures);
                    
                    // Exponential backoff with maximum delay of 6 hours
                    var retryDelay = TimeSpan.FromTicks(_baseRetryDelay.Ticks * (long)Math.Pow(2, Math.Min(_consecutiveFailures - 1, 3)));
                    retryDelay = TimeSpan.FromTicks(Math.Min(retryDelay.Ticks, TimeSpan.FromHours(6).Ticks));
                    
                    _logger.LogWarning("Retrying audit cleanup in {RetryDelay} hours", retryDelay.TotalHours);
                    await Task.Delay(retryDelay, stoppingToken);
                    
                    // Check if we should stop retrying
                    if (_consecutiveFailures >= _maxConsecutiveFailures)
                    {
                        _logger.LogCritical("Maximum consecutive failures ({MaxFailures}) reached for audit cleanup. Service may need manual intervention.", _maxConsecutiveFailures);
                    }
                }

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Analytics audit cleanup background service stopped");
        }

        private async Task CleanupOldAuditLogsAsync()
        {
            var startTime = DateTime.UtcNow;
            var memoryBefore = GC.GetTotalMemory(false);
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var auditService = scope.ServiceProvider.GetRequiredService<IAnalyticsAuditService>();

                _logger.LogInformation("Starting analytics audit cleanup for logs older than {RetentionDays} days", _retentionDays);

                var count = await auditService.CleanupOldAuditLogsAsync(_retentionDays);
                
                var duration = DateTime.UtcNow - startTime;
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                _logger.LogInformation("Analytics audit cleanup completed: {Count} old logs removed in {Duration}ms (Memory: {MemoryUsed}KB)", 
                    count, duration.TotalMilliseconds, memoryUsed / 1024);
                
                // Log performance metrics
                _logger.LogInformation("Audit cleanup performance - Duration: {Duration}ms, Memory used: {MemoryUsed}KB, Logs removed: {Count}, Success rate: {SuccessRate}%", 
                    duration.TotalMilliseconds, memoryUsed / 1024, count, 
                    _lastSuccessfulRun != DateTime.MinValue ? 100 : 0);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                _logger.LogError(ex, "Failed to cleanup old analytics audit logs after {Duration}ms (Memory: {MemoryUsed}KB)", 
                    duration.TotalMilliseconds, memoryUsed / 1024);
                throw; // Re-throw to trigger retry logic
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
