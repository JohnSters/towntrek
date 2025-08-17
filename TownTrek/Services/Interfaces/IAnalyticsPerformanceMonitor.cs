using System.Diagnostics;

namespace TownTrek.Services.Interfaces;

public interface IAnalyticsPerformanceMonitor
{
    /// <summary>
    /// Tracks analytics page load performance
    /// </summary>
    Task TrackAnalyticsPageLoadAsync(string userId, TimeSpan loadTime, bool isSuccess, string? errorMessage = null);

    /// <summary>
    /// Tracks database query performance for analytics
    /// </summary>
    Task TrackDatabaseQueryAsync(string queryName, TimeSpan duration, bool isSuccess, string? errorMessage = null);

    /// <summary>
    /// Tracks chart rendering performance
    /// </summary>
    Task TrackChartRenderingAsync(string chartType, TimeSpan renderTime, bool isSuccess, string? errorMessage = null);

    /// <summary>
    /// Tracks user engagement with analytics features
    /// </summary>
    Task TrackUserEngagementAsync(string userId, string feature, TimeSpan interactionTime);

    /// <summary>
    /// Gets performance statistics for the last 24 hours
    /// </summary>
    Task<AnalyticsPerformanceStats> GetPerformanceStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets slow query analysis
    /// </summary>
    Task<List<SlowQueryInfo>> GetSlowQueriesAsync(int limit = 10);

    /// <summary>
    /// Gets user engagement analytics
    /// </summary>
    Task<UserEngagementStats> GetUserEngagementStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Records a performance metric
    /// </summary>
    Task RecordMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null);
}

public class AnalyticsPerformanceStats
{
    public double AveragePageLoadTime { get; set; }
    public double AverageQueryTime { get; set; }
    public double AverageChartRenderTime { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;
    public List<PerformanceTrend> Trends { get; set; } = new();
}

public class PerformanceTrend
{
    public DateTime Timestamp { get; set; }
    public double PageLoadTime { get; set; }
    public double QueryTime { get; set; }
    public double ChartRenderTime { get; set; }
    public int RequestCount { get; set; }
}

public class SlowQueryInfo
{
    public string QueryName { get; set; } = string.Empty;
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime LastExecuted { get; set; }
    public string? LastErrorMessage { get; set; }
}

public class UserEngagementStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
    public List<UserEngagementTrend> Trends { get; set; } = new();
}

public class UserEngagementTrend
{
    public DateTime Timestamp { get; set; }
    public int ActiveUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
}
