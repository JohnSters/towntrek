namespace TownTrek.Services.Interfaces;

public interface IAnalyticsUsageTracker
{
    /// <summary>
    /// Tracks when a user accesses analytics features
    /// </summary>
    Task TrackFeatureUsageAsync(string userId, string featureName, TimeSpan? duration = null, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Tracks analytics page views
    /// </summary>
    Task TrackPageViewAsync(string userId, string pageName, TimeSpan? duration = null);

    /// <summary>
    /// Tracks chart interactions
    /// </summary>
    Task TrackChartInteractionAsync(string userId, string chartType, string interactionType, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Tracks data export usage
    /// </summary>
    Task TrackDataExportAsync(string userId, string exportType, string format, int recordCount);

    /// <summary>
    /// Tracks filter usage
    /// </summary>
    Task TrackFilterUsageAsync(string userId, string filterType, string filterValue);

    /// <summary>
    /// Gets usage statistics for analytics features
    /// </summary>
    Task<AnalyticsUsageStats> GetUsageStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets most used features
    /// </summary>
    Task<List<FeatureUsageInfo>> GetMostUsedFeaturesAsync(int limit = 10);

    /// <summary>
    /// Gets user engagement metrics
    /// </summary>
    Task<UserEngagementMetrics> GetUserEngagementMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets unused features
    /// </summary>
    Task<List<string>> GetUnusedFeaturesAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets feature adoption rates
    /// </summary>
    Task<Dictionary<string, double>> GetFeatureAdoptionRatesAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets user session analytics
    /// </summary>
    Task<SessionAnalytics> GetSessionAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

public class AnalyticsUsageStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public double UserEngagementRate => TotalUsers > 0 ? (double)ActiveUsers / TotalUsers * 100 : 0;
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
    public Dictionary<string, double> AverageSessionDuration { get; set; } = new();
    public List<UsageTrend> Trends { get; set; } = new();
}

public class UsageTrend
{
    public DateTime Timestamp { get; set; }
    public int ActiveUsers { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
    public double AverageSessionDuration { get; set; }
}

public class FeatureUsageInfo
{
    public string FeatureName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageDuration { get; set; }
    public DateTime LastUsed { get; set; }
    public double AdoptionRate { get; set; }
}

public class UserEngagementMetrics
{
    public int TotalSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public double AveragePagesPerSession { get; set; }
    public double BounceRate { get; set; }
    public Dictionary<string, int> TopPages { get; set; } = new();
    public Dictionary<string, int> TopFeatures { get; set; } = new();
}

public class SessionAnalytics
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> SessionByHour { get; set; } = new();
    public Dictionary<string, int> SessionByDay { get; set; } = new();
    public List<SessionTrend> Trends { get; set; } = new();
}

public class SessionTrend
{
    public DateTime Timestamp { get; set; }
    public int SessionCount { get; set; }
    public double AverageDuration { get; set; }
    public int ActiveUsers { get; set; }
}
