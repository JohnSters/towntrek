using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TownTrek.Services.Analytics;

public class AnalyticsErrorTracker : IAnalyticsErrorTracker
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AnalyticsErrorTracker> _logger;

    public AnalyticsErrorTracker(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AnalyticsErrorTracker> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task TrackErrorAsync(string userId, string errorType, string errorMessage, string? stackTrace = null, Dictionary<string, object>? context = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsErrorLog
            {
                UserId = userId,
                ErrorType = errorType,
                ErrorCategory = "General",
                ErrorMessage = errorMessage,
                StackTrace = stackTrace,
                Context = context != null ? JsonSerializer.Serialize(context) : null,
                Severity = DetermineSeverity(errorType, errorMessage),
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsErrorLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogError("Analytics error tracked: {UserId}, {ErrorType}, {ErrorMessage}", 
                userId, errorType, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking analytics error for user {UserId}", userId);
        }
    }

    public async Task TrackDatabaseErrorAsync(string userId, string queryName, string errorMessage, string? stackTrace = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsErrorLog
            {
                UserId = userId,
                ErrorType = "Database",
                ErrorCategory = queryName,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace,
                Severity = "High", // Database errors are typically high severity
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsErrorLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogError("Database error tracked: {UserId}, {QueryName}, {ErrorMessage}", 
                userId, queryName, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking database error for user {UserId}", userId);
        }
    }

    public async Task TrackChartErrorAsync(string userId, string chartType, string errorMessage, string? stackTrace = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsErrorLog
            {
                UserId = userId,
                ErrorType = "Chart",
                ErrorCategory = chartType,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace,
                Severity = "Medium", // Chart errors are typically medium severity
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsErrorLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Chart error tracked: {UserId}, {ChartType}, {ErrorMessage}", 
                userId, chartType, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking chart error for user {UserId}", userId);
        }
    }

    public async Task TrackCacheErrorAsync(string userId, string operation, string errorMessage, string? stackTrace = null)
    {
        try
        {
            var requestContext = GetRequestContext();
            var log = new AnalyticsErrorLog
            {
                UserId = userId,
                ErrorType = "Cache",
                ErrorCategory = operation,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace,
                Severity = "Low", // Cache errors are typically low severity
                Platform = requestContext.Platform,
                UserAgent = requestContext.UserAgent,
                IpAddress = requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnalyticsErrorLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Cache error tracked: {UserId}, {Operation}, {ErrorMessage}", 
                userId, operation, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking cache error for user {UserId}", userId);
        }
    }

    public async Task<AnalyticsErrorStats> GetErrorStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var query = _context.AnalyticsErrorLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate);

            var stats = new AnalyticsErrorStats
            {
                TotalErrors = await query.CountAsync(),
                DatabaseErrors = await query.CountAsync(log => log.ErrorType == "Database"),
                ChartErrors = await query.CountAsync(log => log.ErrorType == "Chart"),
                CacheErrors = await query.CountAsync(log => log.ErrorType == "Cache"),
                OtherErrors = await query.CountAsync(log => !new[] { "Database", "Chart", "Cache" }.Contains(log.ErrorType)),
                TotalRequests = await _context.AnalyticsPerformanceLogs
                    .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                    .CountAsync()
            };

            // Get trends (hourly data for the last 24 hours)
            var trends = await query
                .GroupBy(log => new { log.CreatedAt.Date, log.CreatedAt.Hour })
                .Select(g => new ErrorTrend
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    ErrorCount = g.Count(),
                    RequestCount = _context.AnalyticsPerformanceLogs
                        .Where(log => log.CreatedAt.Date == g.Key.Date && log.CreatedAt.Hour == g.Key.Hour)
                        .Count(),
                    ErrorTypes = g.GroupBy(log => log.ErrorType)
                        .ToDictionary(x => x.Key, x => x.Count())
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            stats.Trends = trends;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error stats");
            return new AnalyticsErrorStats();
        }
    }

    public async Task<List<AnalyticsErrorInfo>> GetRecentErrorsAsync(int limit = 50)
    {
        try
        {
            var errorLogs = await _context.AnalyticsErrorLogs
                .OrderByDescending(log => log.CreatedAt)
                .Take(limit)
                .ToListAsync();

            var errors = errorLogs.Select(log => new AnalyticsErrorInfo
            {
                Id = log.Id,
                UserId = log.UserId,
                ErrorType = log.ErrorType,
                ErrorMessage = log.ErrorMessage,
                StackTrace = log.StackTrace,
                Context = !string.IsNullOrEmpty(log.Context) ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.Context) ?? new Dictionary<string, object>() : new Dictionary<string, object>(),
                OccurredAt = log.CreatedAt,
                IsResolved = log.IsResolved,
                ResolvedAt = log.ResolvedAt,
                ResolvedBy = log.ResolvedBy,
                Severity = log.Severity
            }).ToList();

            return errors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent errors");
            return new List<AnalyticsErrorInfo>();
        }
    }

    public async Task<List<ErrorTrend>> GetErrorTrendsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var trends = await _context.AnalyticsErrorLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .GroupBy(log => new { log.CreatedAt.Date, log.CreatedAt.Hour })
                .Select(g => new ErrorTrend
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    ErrorCount = g.Count(),
                    RequestCount = 0, // Will be calculated separately
                    ErrorTypes = g.GroupBy(log => log.ErrorType)
                        .ToDictionary(x => x.Key, x => x.Count())
                })
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error trends");
            return new List<ErrorTrend>();
        }
    }

    public async Task<Dictionary<string, int>> GetErrorBreakdownAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-1);
            toDate ??= DateTime.UtcNow;

            var breakdown = await _context.AnalyticsErrorLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .GroupBy(log => log.ErrorType)
                .Select(g => new { ErrorType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ErrorType, x => x.Count);

            return breakdown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error breakdown");
            return new Dictionary<string, int>();
        }
    }

    public async Task<bool> IsErrorRateHighAsync(double threshold = 5.0)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddHours(-1); // Last hour
            var toDate = DateTime.UtcNow;

            var errorCount = await _context.AnalyticsErrorLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .CountAsync();

            var requestCount = await _context.AnalyticsPerformanceLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .CountAsync();

            if (requestCount == 0) return false;

            var errorRate = (double)errorCount / requestCount * 100;
            return errorRate > threshold;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking error rate");
            return false;
        }
    }

    public async Task<List<AnalyticsErrorInfo>> GetCriticalErrorsAsync()
    {
        try
        {
            var criticalErrorLogs = await _context.AnalyticsErrorLogs
                .Where(log => log.Severity == "Critical" && !log.IsResolved)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync();

            var criticalErrors = criticalErrorLogs.Select(log => new AnalyticsErrorInfo
            {
                Id = log.Id,
                UserId = log.UserId,
                ErrorType = log.ErrorType,
                ErrorMessage = log.ErrorMessage,
                StackTrace = log.StackTrace,
                Context = !string.IsNullOrEmpty(log.Context) ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.Context) ?? new Dictionary<string, object>() : new Dictionary<string, object>(),
                OccurredAt = log.CreatedAt,
                IsResolved = log.IsResolved,
                ResolvedAt = log.ResolvedAt,
                ResolvedBy = log.ResolvedBy,
                Severity = log.Severity
            }).ToList();

            return criticalErrors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical errors");
            return new List<AnalyticsErrorInfo>();
        }
    }

    private (string? Platform, string? UserAgent, string? IpAddress) GetRequestContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return (null, null, null);

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        
        // Determine platform based on user agent
        var platform = "Web";
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
            platform = "Mobile";
        else if (httpContext.Request.Headers.ContainsKey("X-API-Key"))
            platform = "API";

        return (platform, userAgent, ipAddress);
    }

    private string DetermineSeverity(string errorType, string errorMessage)
    {
        // Determine severity based on error type and message
        if (errorType == "Database" || errorMessage.Contains("connection") || errorMessage.Contains("timeout"))
            return "High";
        else if (errorType == "Chart" || errorMessage.Contains("render"))
            return "Medium";
        else if (errorType == "Cache" || errorMessage.Contains("cache"))
            return "Low";
        else
            return "Medium";
    }
}
