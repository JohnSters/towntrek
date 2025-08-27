using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.AdminAnalytics;

public class AnalyticsHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IAnalyticsPerformanceMonitor _performanceMonitor;
    private readonly IAnalyticsErrorTracker _errorTracker;
    private readonly ILogger<AnalyticsHealthCheck> _logger;

    public AnalyticsHealthCheck(
        ApplicationDbContext context,
        ICacheService cacheService,
        IAnalyticsPerformanceMonitor performanceMonitor,
        IAnalyticsErrorTracker errorTracker,
        ILogger<AnalyticsHealthCheck> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _performanceMonitor = performanceMonitor;
        _errorTracker = errorTracker;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthData = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            // Check database connectivity
            var dbHealth = await CheckDatabaseHealthAsync();
            healthData["Database"] = dbHealth;
            if (!dbHealth.IsHealthy)
                issues.Add($"Database: {dbHealth.Description}");

            // Check cache health
            var cacheHealth = await CheckCacheHealthAsync();
            healthData["Cache"] = cacheHealth;
            if (!cacheHealth.IsHealthy)
                issues.Add($"Cache: {cacheHealth.Description}");

            // Check performance metrics
            var performanceHealth = await CheckPerformanceHealthAsync();
            healthData["Performance"] = performanceHealth;
            if (!performanceHealth.IsHealthy)
                issues.Add($"Performance: {performanceHealth.Description}");

            // Check error rates
            var errorHealth = await CheckErrorHealthAsync();
            healthData["ErrorRate"] = errorHealth;
            if (!errorHealth.IsHealthy)
                issues.Add($"Error Rate: {errorHealth.Description}");

            // Determine overall health
            if (issues.Count == 0)
            {
                return HealthCheckResult.Healthy("Analytics system is healthy", healthData);
            }
            else if (issues.Count <= 2)
            {
                return HealthCheckResult.Degraded($"Analytics system has {issues.Count} issues: {string.Join(", ", issues)}");
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Analytics system has {issues.Count} critical issues: {string.Join(", ", issues)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during analytics health check");
            return HealthCheckResult.Unhealthy("Analytics health check failed", ex);
        }
    }

    private async Task<(bool IsHealthy, string Description, object Data)> CheckDatabaseHealthAsync()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test basic connectivity
            await _context.Database.CanConnectAsync();
            
            // Test analytics-specific queries
            var viewLogCount = await _context.BusinessViewLogs.CountAsync();
            var performanceLogCount = await _context.AnalyticsPerformanceLogs.CountAsync();
            var errorLogCount = await _context.AnalyticsErrorLogs.CountAsync();
            
            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            var data = new
            {
                ResponseTimeMs = responseTime,
                ViewLogsCount = viewLogCount,
                PerformanceLogsCount = performanceLogCount,
                ErrorLogsCount = errorLogCount
            };

            if (responseTime > 5000) // 5 seconds
            {
                return (false, $"Database response time is slow: {responseTime}ms", data);
            }

            return (true, "Database is healthy", data);
        }
        catch (Exception ex)
        {
            return (false, $"Database connection failed: {ex.Message}", new { Error = ex.Message });
        }
    }

    private async Task<(bool IsHealthy, string Description, object Data)> CheckCacheHealthAsync()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test cache operations
            var testKey = "health_check_test";
            var testValue = DateTime.UtcNow.ToString();
            
            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromMinutes(1));
            var retrievedValue = await _cacheService.GetAsync<string>(testKey);
            await _cacheService.RemoveAsync(testKey);
            
            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            var data = new
            {
                ResponseTimeMs = responseTime,
                CacheWorking = retrievedValue == testValue
            };

            if (responseTime > 1000) // 1 second
            {
                return (false, $"Cache response time is slow: {responseTime}ms", data);
            }

            if (retrievedValue != testValue)
            {
                return (false, "Cache read/write test failed", data);
            }

            return (true, "Cache is healthy", data);
        }
        catch (Exception ex)
        {
            return (false, $"Cache health check failed: {ex.Message}", new { Error = ex.Message });
        }
    }

    private async Task<(bool IsHealthy, string Description, object Data)> CheckPerformanceHealthAsync()
    {
        try
        {
            // Get performance stats for the last hour
            var fromDate = DateTime.UtcNow.AddHours(-1);
            var toDate = DateTime.UtcNow;
            
            var performanceStats = await _performanceMonitor.GetPerformanceStatsAsync(fromDate, toDate);
            
            var data = new
            {
                performanceStats.AveragePageLoadTime,
                performanceStats.AverageQueryTime,
                performanceStats.AverageChartRenderTime,
                performanceStats.SuccessRate,
                performanceStats.TotalRequests
            };

            var issues = new List<string>();

            if (performanceStats.AveragePageLoadTime > 3000) // 3 seconds
                issues.Add($"Slow page load time: {performanceStats.AveragePageLoadTime:F0}ms");

            if (performanceStats.AverageQueryTime > 1000) // 1 second
                issues.Add($"Slow query time: {performanceStats.AverageQueryTime:F0}ms");

            if (performanceStats.AverageChartRenderTime > 2000) // 2 seconds
                issues.Add($"Slow chart render time: {performanceStats.AverageChartRenderTime:F0}ms");

            if (performanceStats.SuccessRate < 95) // 95% success rate
                issues.Add($"Low success rate: {performanceStats.SuccessRate:F1}%");

            if (issues.Count > 0)
            {
                return (false, $"Performance issues: {string.Join(", ", issues)}", data);
            }

            return (true, "Performance is healthy", data);
        }
        catch (Exception ex)
        {
            return (false, $"Performance health check failed: {ex.Message}", new { Error = ex.Message });
        }
    }

    private async Task<(bool IsHealthy, string Description, object Data)> CheckErrorHealthAsync()
    {
        try
        {
            // Check error rate for the last hour
            var isErrorRateHigh = await _errorTracker.IsErrorRateHighAsync(5.0); // 5% threshold
            
            var errorStats = await _errorTracker.GetErrorStatsAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            
            var data = new
            {
                errorStats.ErrorRate,
                errorStats.TotalErrors,
                errorStats.DatabaseErrors,
                errorStats.ChartErrors,
                errorStats.CacheErrors
            };

            if (isErrorRateHigh)
            {
                return (false, $"High error rate: {errorStats.ErrorRate:F1}%", data);
            }

            if (errorStats.DatabaseErrors > 10) // More than 10 database errors in the last hour
            {
                return (false, $"High database error count: {errorStats.DatabaseErrors}", data);
            }

            return (true, "Error rate is healthy", data);
        }
        catch (Exception ex)
        {
            return (false, $"Error health check failed: {ex.Message}", new { Error = ex.Message });
        }
    }
}
