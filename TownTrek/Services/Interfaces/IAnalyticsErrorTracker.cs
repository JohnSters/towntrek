namespace TownTrek.Services.Interfaces;

public interface IAnalyticsErrorTracker
{
    /// <summary>
    /// Tracks an analytics-related error
    /// </summary>
    Task TrackErrorAsync(string userId, string errorType, string errorMessage, string? stackTrace = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Tracks a database error in analytics
    /// </summary>
    Task TrackDatabaseErrorAsync(string userId, string queryName, string errorMessage, string? stackTrace = null);

    /// <summary>
    /// Tracks a chart rendering error
    /// </summary>
    Task TrackChartErrorAsync(string userId, string chartType, string errorMessage, string? stackTrace = null);

    /// <summary>
    /// Tracks a cache error
    /// </summary>
    Task TrackCacheErrorAsync(string userId, string operation, string errorMessage, string? stackTrace = null);

    /// <summary>
    /// Gets error statistics for the last 24 hours
    /// </summary>
    Task<AnalyticsErrorStats> GetErrorStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets recent errors with details
    /// </summary>
    Task<List<AnalyticsErrorInfo>> GetRecentErrorsAsync(int limit = 50);

    /// <summary>
    /// Gets error trends over time
    /// </summary>
    Task<List<ErrorTrend>> GetErrorTrendsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets error breakdown by type
    /// </summary>
    Task<Dictionary<string, int>> GetErrorBreakdownAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Checks if error rate is above threshold
    /// </summary>
    Task<bool> IsErrorRateHighAsync(double threshold = 5.0);

    /// <summary>
    /// Gets critical errors that need immediate attention
    /// </summary>
    Task<List<AnalyticsErrorInfo>> GetCriticalErrorsAsync();
}

public class AnalyticsErrorStats
{
    public int TotalErrors { get; set; }
    public int DatabaseErrors { get; set; }
    public int ChartErrors { get; set; }
    public int CacheErrors { get; set; }
    public int OtherErrors { get; set; }
    public double ErrorRate => TotalRequests > 0 ? (double)TotalErrors / TotalRequests * 100 : 0;
    public int TotalRequests { get; set; }
    public List<ErrorTrend> Trends { get; set; } = new();
}

public class ErrorTrend
{
    public DateTime Timestamp { get; set; }
    public int ErrorCount { get; set; }
    public int RequestCount { get; set; }
    public double ErrorRate => RequestCount > 0 ? (double)ErrorCount / RequestCount * 100 : 0;
    public Dictionary<string, int> ErrorTypes { get; set; } = new();
}

public class AnalyticsErrorInfo
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime OccurredAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
}
