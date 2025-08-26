using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TownTrek.Services.Analytics;

public class AnalyticsUsageTracker : IAnalyticsUsageTracker
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AnalyticsUsageTracker> _logger;

    public AnalyticsUsageTracker(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AnalyticsUsageTracker> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task TrackFeatureUsageAsync(string userId, string featureName, TimeSpan? duration = null, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsUsageLog
            {
                UserId = userId,
                UsageType = "FeatureUsage",
                FeatureName = featureName,
                Duration = duration?.TotalMilliseconds,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                SessionId = requestContext.SessionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsUsageLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Feature usage tracked: {UserId}, {FeatureName}, {Duration}ms", 
                userId, featureName, duration?.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking feature usage for user {UserId}, feature {FeatureName}", userId, featureName);
        }
    }

    public async Task TrackPageViewAsync(string userId, string pageName, TimeSpan? duration = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsUsageLog
            {
                UserId = userId,
                UsageType = "PageView",
                FeatureName = pageName,
                Duration = duration?.TotalMilliseconds,
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                SessionId = requestContext.SessionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsUsageLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking page view for user {UserId}, page {PageName}", userId, pageName);
        }
    }

    public async Task TrackChartInteractionAsync(string userId, string chartType, string interactionType, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsUsageLog
            {
                UserId = userId,
                UsageType = "ChartInteraction",
                FeatureName = chartType,
                InteractionType = interactionType,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                SessionId = requestContext.SessionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsUsageLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking chart interaction for user {UserId}, chart {ChartType}", userId, chartType);
        }
    }

    public async Task TrackDataExportAsync(string userId, string exportType, string format, int recordCount)
    {
        try
        {
            var requestContext = GetRequestContext();
            var metadata = new Dictionary<string, object>
            {
                ["format"] = format,
                ["recordCount"] = recordCount
            };

            var log = new AnalyticsUsageLog
            {
                UserId = userId,
                UsageType = "DataExport",
                FeatureName = exportType,
                InteractionType = format,
                Metadata = JsonSerializer.Serialize(metadata),
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                SessionId = requestContext.SessionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsUsageLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Data export tracked: {UserId}, {ExportType}, {Format}, {RecordCount} records", 
                userId, exportType, format, recordCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking data export for user {UserId}", userId);
        }
    }

    public async Task TrackFilterUsageAsync(string userId, string filterType, string filterValue)
    {
        try
        {
            var requestContext = GetRequestContext();
            var metadata = new Dictionary<string, object>
            {
                ["filterValue"] = filterValue
            };

            var log = new AnalyticsUsageLog
            {
                UserId = userId,
                UsageType = "FilterUsage",
                FeatureName = filterType,
                InteractionType = "filter",
                Metadata = JsonSerializer.Serialize(metadata),
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                SessionId = requestContext.SessionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsUsageLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking filter usage for user {UserId}, filter {FilterType}", userId, filterType);
        }
    }

    public async Task<AnalyticsUsageStats> GetUsageStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var query = _context.AnalyticsUsageLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate);

            var stats = new AnalyticsUsageStats
            {
                TotalUsers = await query.Select(log => log.UserId).Distinct().CountAsync(),
                ActiveUsers = await query.Select(log => log.UserId).Distinct().CountAsync()
            };

            // Get feature usage breakdown
            var featureUsage = await query
                .GroupBy(log => log.FeatureName)
                .Select(g => new { Feature = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Feature, x => x.Count);

            stats.FeatureUsage = featureUsage;

            // Get average session duration by feature
            var sessionDuration = await query
                .Where(log => log.Duration.HasValue)
                .GroupBy(log => log.FeatureName)
                .Select(g => new { Feature = g.Key, AvgDuration = g.Average(log => log.Duration!.Value) })
                .ToDictionaryAsync(x => x.Feature, x => x.AvgDuration);

            stats.AverageSessionDuration = sessionDuration;

            // Get trends (hourly data for the last 24 hours)
            var trends = await query
                .GroupBy(log => new { log.CreatedAt.Date, log.CreatedAt.Hour })
                .Select(g => new UsageTrend
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    ActiveUsers = g.Select(log => log.UserId).Distinct().Count(),
                    FeatureUsage = g.GroupBy(log => log.FeatureName)
                        .ToDictionary(x => x.Key, x => x.Count()),
                    AverageSessionDuration = g.Where(log => log.Duration.HasValue).Average(log => log.Duration!.Value)
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            stats.Trends = trends;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats");
            return new AnalyticsUsageStats();
        }
    }

    public async Task<List<FeatureUsageInfo>> GetMostUsedFeaturesAsync(int limit = 10)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-7); // Last 7 days
            var toDate = DateTime.UtcNow;

            var features = await _context.AnalyticsUsageLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .GroupBy(log => log.FeatureName)
                .Select(g => new FeatureUsageInfo
                {
                    FeatureName = g.Key,
                    UsageCount = g.Count(),
                    UniqueUsers = g.Select(log => log.UserId).Distinct().Count(),
                    AverageDuration = g.Where(log => log.Duration.HasValue).Any() ? g.Where(log => log.Duration.HasValue).Average(log => log.Duration!.Value) : 0,
                    LastUsed = g.Max(log => log.CreatedAt),
                    AdoptionRate = (double)g.Select(log => log.UserId).Distinct().Count() / 
                                  _context.AnalyticsUsageLogs.Select(log => log.UserId).Distinct().Count() * 100
                })
                .OrderByDescending(f => f.UsageCount)
                .Take(limit)
                .ToListAsync();

            return features;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most used features");
            return new List<FeatureUsageInfo>();
        }
    }

    public async Task<UserEngagementMetrics> GetUserEngagementMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var query = _context.AnalyticsUsageLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate);

            var metrics = new UserEngagementMetrics
            {
                TotalSessions = await query.Select(log => log.SessionId).Distinct().CountAsync(),
                AverageSessionDuration = await query.Where(log => log.Duration.HasValue).AverageAsync(log => log.Duration!.Value),
                AveragePagesPerSession = await query.Where(log => log.UsageType == "PageView").CountAsync() / 
                                        (double)await query.Select(log => log.SessionId).Distinct().CountAsync()
            };

            // Calculate bounce rate (sessions with only one page view)
            var sessionsWithOnePage = await query
                .Where(log => log.UsageType == "PageView")
                .GroupBy(log => log.SessionId)
                .Where(g => g.Count() == 1)
                .CountAsync();

            var totalSessions = await query.Select(log => log.SessionId).Distinct().CountAsync();
            metrics.BounceRate = totalSessions > 0 ? (double)sessionsWithOnePage / totalSessions * 100 : 0;

            // Get top pages
            var topPages = await query
                .Where(log => log.UsageType == "PageView")
                .GroupBy(log => log.FeatureName)
                .Select(g => new { Page = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.Page, x => x.Count);

            metrics.TopPages = topPages;

            // Get top features
            var topFeatures = await query
                .Where(log => log.UsageType == "FeatureUsage")
                .GroupBy(log => log.FeatureName)
                .Select(g => new { Feature = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.Feature, x => x.Count);

            metrics.TopFeatures = topFeatures;

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user engagement metrics");
            return new UserEngagementMetrics();
        }
    }

    public async Task<List<string>> GetUnusedFeaturesAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-30); // Last 30 days
            toDate ??= DateTime.UtcNow;

            // Get all available features
            var allFeatures = new List<string>
            {
                "AnalyticsDashboard", "BusinessAnalytics", "ViewsChart", "ReviewsChart", 
                "PerformanceChart", "ExportData", "FilterByDate", "FilterByBusiness",
                "ChartZoom", "ChartExport", "DataDownload", "ReportGeneration"
            };

            // Get used features in the time period
            var usedFeatures = await _context.AnalyticsUsageLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .Select(log => log.FeatureName)
                .Distinct()
                .ToListAsync();

            // Return unused features
            return allFeatures.Except(usedFeatures).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused features");
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, double>> GetFeatureAdoptionRatesAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-30); // Last 30 days
            toDate ??= DateTime.UtcNow;

            var totalUsers = await _context.AnalyticsUsageLogs
                .Select(log => log.UserId)
                .Distinct()
                .CountAsync();

            var adoptionRates = await _context.AnalyticsUsageLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .GroupBy(log => log.FeatureName)
                .Select(g => new
                {
                    Feature = g.Key,
                    AdoptionRate = (double)g.Select(log => log.UserId).Distinct().Count() / totalUsers * 100
                })
                .ToDictionaryAsync(x => x.Feature, x => x.AdoptionRate);

            return adoptionRates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature adoption rates");
            return new Dictionary<string, double>();
        }
    }

    public async Task<SessionAnalytics> GetSessionAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var query = _context.AnalyticsUsageLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate);

            var analytics = new SessionAnalytics
            {
                TotalSessions = await query.Select(log => log.SessionId).Distinct().CountAsync(),
                ActiveSessions = await query.Select(log => log.SessionId).Distinct().CountAsync(),
                AverageSessionDuration = await query.Where(log => log.Duration.HasValue).AverageAsync(log => log.Duration!.Value)
            };

            // Get sessions by hour
            var sessionsByHour = await query
                .GroupBy(log => log.CreatedAt.Hour)
                .Select(g => new { Hour = g.Key.ToString(), Count = g.Select(log => log.SessionId).Distinct().Count() })
                .ToDictionaryAsync(x => x.Hour, x => x.Count);

            analytics.SessionByHour = sessionsByHour;

            // Get sessions by day
            var sessionsByDay = await query
                .GroupBy(log => log.CreatedAt.DayOfWeek)
                .Select(g => new { Day = g.Key.ToString(), Count = g.Select(log => log.SessionId).Distinct().Count() })
                .ToDictionaryAsync(x => x.Day, x => x.Count);

            analytics.SessionByDay = sessionsByDay;

            // Get trends
            var trends = await query
                .GroupBy(log => new { log.CreatedAt.Date, log.CreatedAt.Hour })
                .Select(g => new SessionTrend
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    SessionCount = g.Select(log => log.SessionId).Distinct().Count(),
                    AverageDuration = g.Where(log => log.Duration.HasValue).Any() ? g.Where(log => log.Duration.HasValue).Average(log => log.Duration!.Value) : 0,
                    ActiveUsers = g.Select(log => log.UserId).Distinct().Count()
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            analytics.Trends = trends;

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session analytics");
            return new SessionAnalytics();
        }
    }

    private (string? Platform, string? UserAgent, string? IpAddress, string? SessionId) GetRequestContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return (null, null, null, null);

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var sessionId = httpContext.Session.Id;
        
        // Determine platform based on user agent
        var platform = "Web";
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
            platform = "Mobile";
        else if (httpContext.Request.Headers.ContainsKey("X-API-Key"))
            platform = "API";

        return (platform, userAgent, ipAddress, sessionId);
    }
}
