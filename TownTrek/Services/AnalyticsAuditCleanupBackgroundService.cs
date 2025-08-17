using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    /// <summary>
    /// Background service to automatically clean up old analytics audit logs
    /// </summary>
    public class AnalyticsAuditCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnalyticsAuditCleanupBackgroundService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(7); // Run weekly
        private readonly int _retentionDays = 365; // Keep logs for 1 year

        public AnalyticsAuditCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AnalyticsAuditCleanupBackgroundService> logger)
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during analytics audit cleanup");
                }

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Analytics audit cleanup background service stopped");
        }

        private async Task CleanupOldAuditLogsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<IAnalyticsAuditService>();

            try
            {
                var count = await auditService.CleanupOldAuditLogsAsync(_retentionDays);
                _logger.LogInformation("Analytics audit cleanup completed: {Count} old logs removed", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old analytics audit logs");
            }
        }
    }
}
