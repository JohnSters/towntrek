using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TownTrek.Data;
using TownTrek.Models;
using Microsoft.AspNetCore.Http;
using TownTrek.Services.Interfaces.AdminAnalytics;

namespace TownTrek.Services.AdminAnalytics;

public class AdminPerformanceMonitorService : IAnalyticsPerformanceMonitor
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AdminPerformanceMonitorService> _logger;

    public AdminPerformanceMonitorService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AdminPerformanceMonitorService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task TrackAnalyticsPageLoadAsync(string userId, TimeSpan loadTime, bool isSuccess, string? errorMessage = null)
    {
        try
        {
            var context = GetRequestContext();
            var log = new AnalyticsPerformanceLog
            {
                UserId = userId,
                MetricType = "PageLoad",
                MetricName = "AnalyticsDashboard",
                Value = loadTime.TotalMilliseconds,
                Unit = "ms",
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Platform = context.Platform,
                UserAgent = context.UserAgent,
                IpAddress = context.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsPerformanceLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Analytics page load tracked: {UserId}, {LoadTime}ms, Success: {IsSuccess}", 
                userId, loadTime.TotalMilliseconds, isSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking analytics page load for user {UserId}", userId);
        }
    }

    public async Task TrackDatabaseQueryAsync(string queryName, TimeSpan duration, bool isSuccess, string? errorMessage = null)
    {
        try
        {
            var context = GetRequestContext();
            var log = new AnalyticsPerformanceLog
            {
                UserId = context.UserId ?? "system",
                MetricType = "DatabaseQuery",
                MetricName = queryName,
                Value = duration.TotalMilliseconds,
                Unit = "ms",
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Platform = context.Platform,
                UserAgent = context.UserAgent,
                IpAddress = context.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsPerformanceLogs.Add(log);
            await _context.SaveChangesAsync();

            if (!isSuccess)
            {
                _logger.LogWarning("Slow database query detected: {QueryName}, {Duration}ms", 
                    queryName, duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking database query performance for {QueryName}", queryName);
        }
    }

    public async Task TrackChartRenderingAsync(string chartType, TimeSpan renderTime, bool isSuccess, string? errorMessage = null)
    {
        try
        {
            var context = GetRequestContext();
            var log = new AnalyticsPerformanceLog
            {
                UserId = context.UserId ?? "system",
                MetricType = "ChartRender",
                MetricName = chartType,
                Value = renderTime.TotalMilliseconds,
                Unit = "ms",
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Platform = context.Platform,
                UserAgent = context.UserAgent,
                IpAddress = context.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsPerformanceLogs.Add(log);
            await _context.SaveChangesAsync();

            if (!isSuccess)
            {
                _logger.LogWarning("Chart rendering error: {ChartType}, {ErrorMessage}", 
                    chartType, errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking chart rendering performance for {ChartType}", chartType);
        }
    }

    public async Task TrackUserEngagementAsync(string userId, string feature, TimeSpan interactionTime)
    {
        try
        {
            var context = GetRequestContext();
            var log = new AnalyticsPerformanceLog
            {
                UserId = userId,
                MetricType = "UserEngagement",
                MetricName = feature,
                Value = interactionTime.TotalMilliseconds,
                Unit = "ms",
                IsSuccess = true,
                Platform = context.Platform,
                UserAgent = context.UserAgent,
                IpAddress = context.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsPerformanceLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user engagement for user {UserId}, feature {Feature}", userId, feature);
        }
    }

    public async Task<AnalyticsPerformanceStats> GetPerformanceStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var query = _context.AnalyticsPerformanceLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate);

            var stats = new AnalyticsPerformanceStats
            {
                TotalRequests = await query.CountAsync(),
                SuccessfulRequests = await query.CountAsync(log => log.IsSuccess),
                FailedRequests = await query.CountAsync(log => !log.IsSuccess),
                AveragePageLoadTime = await query
                    .Where(log => log.MetricType == "PageLoad")
                    .AverageAsync(log => log.Value),
                AverageQueryTime = await query
                    .Where(log => log.MetricType == "DatabaseQuery")
                    .AverageAsync(log => log.Value),
                AverageChartRenderTime = await query
                    .Where(log => log.MetricType == "ChartRender")
                    .AverageAsync(log => log.Value)
            };

            // Get trends (hourly data for the last 24 hours)
            var trends = await query
                .GroupBy(log => new { log.CreatedAt.Date, log.CreatedAt.Hour })
                .Select(g => new PerformanceTrend
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    RequestCount = g.Count(),
                    PageLoadTime = g.Where(log => log.MetricType == "PageLoad").Average(log => log.Value),
                    QueryTime = g.Where(log => log.MetricType == "DatabaseQuery").Average(log => log.Value),
                    ChartRenderTime = g.Where(log => log.MetricType == "ChartRender").Average(log => log.Value)
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            stats.Trends = trends;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance stats");
            return new AnalyticsPerformanceStats();
        }
    }

    public async Task<List<SlowQueryInfo>> GetSlowQueriesAsync(int limit = 10)
    {
        try
        {
            var slowQueries = await _context.AnalyticsPerformanceLogs
                .Where(log => log.MetricType == "DatabaseQuery" && log.Value > 1000) // Queries taking more than 1 second
                .GroupBy(log => log.MetricName)
                .Select(g => new SlowQueryInfo
                {
                    QueryName = g.Key,
                    AverageDuration = TimeSpan.FromMilliseconds(g.Average(log => log.Value)),
                    MaxDuration = TimeSpan.FromMilliseconds(g.Max(log => log.Value)),
                    ExecutionCount = g.Count(),
                    LastExecuted = g.Max(log => log.CreatedAt),
                    LastErrorMessage = g.Where(log => !log.IsSuccess).OrderByDescending(log => log.CreatedAt).Select(log => log.ErrorMessage).FirstOrDefault()
                })
                .OrderByDescending(q => q.AverageDuration)
                .Take(limit)
                .ToListAsync();

            return slowQueries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slow queries");
            return new List<SlowQueryInfo>();
        }
    }

    public async Task<UserEngagementStats> GetUserEngagementStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var query = _context.AnalyticsPerformanceLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate && log.MetricType == "UserEngagement");

            var stats = new UserEngagementStats
            {
                TotalUsers = await query.Select(log => log.UserId).Distinct().CountAsync(),
                ActiveUsers = await query.Select(log => log.UserId).Distinct().CountAsync(),
                AverageSessionDuration = await query.AverageAsync(log => log.Value)
            };

            // Get feature usage breakdown
            var featureUsage = await query
                .GroupBy(log => log.MetricName)
                .Select(g => new { Feature = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Feature, x => x.Count);

            stats.FeatureUsage = featureUsage;

            // Get trends
            var trends = await query
                .GroupBy(log => new { log.CreatedAt.Date, log.CreatedAt.Hour })
                .Select(g => new UserEngagementTrend
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    ActiveUsers = g.Select(log => log.UserId).Distinct().Count(),
                    AverageSessionDuration = g.Average(log => log.Value),
                    FeatureUsage = g.GroupBy(log => log.MetricName)
                        .ToDictionary(x => x.Key, x => x.Count())
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            stats.Trends = trends;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user engagement stats");
            return new UserEngagementStats();
        }
    }

    public async Task RecordMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        try
        {
            var context = GetRequestContext();
            var log = new AnalyticsPerformanceLog
            {
                UserId = context.UserId ?? "system",
                MetricType = "Custom",
                MetricName = metricName,
                Value = value,
                Unit = "count",
                IsSuccess = true,
                Context = tags != null ? JsonSerializer.Serialize(tags) : null,
                Platform = context.Platform,
                UserAgent = context.UserAgent,
                IpAddress = context.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsPerformanceLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metric {MetricName}", metricName);
        }
    }

    private (string? UserId, string? Platform, string? UserAgent, string? IpAddress) GetRequestContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return (null, null, null, null);

        var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        
        // Determine platform based on user agent
        var platform = "Web";
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
            platform = "Mobile";
        else if (httpContext.Request.Headers.ContainsKey("X-API-Key"))
            platform = "API";

        return (userId, platform, userAgent, ipAddress);
    }
}
