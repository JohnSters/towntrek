using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Background service for creating daily analytics snapshots
    /// </summary>
    public class AnalyticsSnapshotBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnalyticsSnapshotBackgroundService> _logger;
        private readonly TimeSpan _dailyRunTime = new(2, 0, 0); // Run at 2 AM UTC

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
                    _logger.LogError(ex, "Error in analytics snapshot background service");
                    
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Analytics Snapshot Background Service stopped");
        }

        private async Task CreateDailySnapshotsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var snapshotService = scope.ServiceProvider.GetRequiredService<IAnalyticsSnapshotService>();

                _logger.LogInformation("Starting daily analytics snapshot creation");
                
                var snapshotsCreated = await snapshotService.CreateDailySnapshotsAsync();
                
                _logger.LogInformation("Daily analytics snapshot creation completed. Created {Count} snapshots", snapshotsCreated);

                // Also run cleanup of old snapshots (keep 2 years)
                var snapshotsDeleted = await snapshotService.CleanupOldSnapshotsAsync(730);
                
                if (snapshotsDeleted > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} old analytics snapshots", snapshotsDeleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily analytics snapshots");
            }
        }
    }
}
