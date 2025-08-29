using Microsoft.Extensions.Diagnostics.HealthChecks;

using TownTrek.Services.Interfaces.AdminAnalytics;

namespace TownTrek.Models.ViewModels
{
    public class AnalyticsMonitoringDashboardViewModel
    {
        public AnalyticsPerformanceStats PerformanceStats { get; set; } = new();
        public AnalyticsErrorStats ErrorStats { get; set; } = new();
        public AnalyticsUsageStats UsageStats { get; set; } = new();
        public HealthReport? HealthStatus { get; set; }
        public List<AnalyticsErrorInfo> RecentErrors { get; set; } = new();
        public List<SlowQueryInfo> SlowQueries { get; set; } = new();
        public List<FeatureUsageInfo> MostUsedFeatures { get; set; } = new();
        public List<string> UnusedFeatures { get; set; } = new();
    }

    public class AnalyticsPerformanceViewModel
    {
        public AnalyticsPerformanceStats PerformanceStats { get; set; } = new();
        public List<SlowQueryInfo> SlowQueries { get; set; } = new();
        public UserEngagementStats UserEngagement { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class AnalyticsErrorsViewModel
    {
        public AnalyticsErrorStats ErrorStats { get; set; } = new();
        public List<AnalyticsErrorInfo> RecentErrors { get; set; } = new();
        public List<ErrorTrend> ErrorTrends { get; set; } = new();
        public Dictionary<string, int> ErrorBreakdown { get; set; } = new();
        public List<AnalyticsErrorInfo> CriticalErrors { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class AnalyticsUsageViewModel
    {
        public AnalyticsUsageStats UsageStats { get; set; } = new();
        public List<FeatureUsageInfo> MostUsedFeatures { get; set; } = new();
        public UserEngagementMetrics UserEngagement { get; set; } = new();
        public SessionAnalytics SessionAnalytics { get; set; } = new();
        public Dictionary<string, double> FeatureAdoption { get; set; } = new();
        public List<string> UnusedFeatures { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
