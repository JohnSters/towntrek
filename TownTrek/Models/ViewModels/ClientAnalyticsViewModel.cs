using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class ClientAnalyticsViewModel
    {
        // User and subscription info
        public ApplicationUser User { get; set; } = null!;
        public string SubscriptionTier { get; set; } = string.Empty;
        public bool HasBasicAnalytics { get; set; }
        // Deprecated in favor of AdvancedAnalytics
        public bool HasStandardAnalytics { get; set; }
        public bool HasPremiumAnalytics { get; set; }
        
        // Business data
        public List<Business> Businesses { get; set; } = new();
        public List<BusinessAnalyticsData> BusinessAnalytics { get; set; } = new();
        
        // Overview metrics
        public AnalyticsOverview Overview { get; set; } = new();
        
        // Time-based data (for charts)
        public List<ViewsOverTimeData> ViewsOverTime { get; set; } = new();
        public List<ReviewsOverTimeData> ReviewsOverTime { get; set; } = new();
        
        // Performance insights
        public List<BusinessPerformanceInsight> PerformanceInsights { get; set; } = new();
        
        // Comparison data (Premium only)
        public CategoryBenchmarkData? CategoryBenchmarks { get; set; }
        public List<CompetitorInsight> CompetitorInsights { get; set; } = new();
    }

    public class BusinessAnalyticsData
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        // View metrics
        public int TotalViews { get; set; }
        public int ViewsThisMonth { get; set; }
        public int ViewsLastMonth { get; set; }
        public double ViewsGrowthRate { get; set; }
        
        // Review metrics
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsThisMonth { get; set; }
        public int ReviewsLastMonth { get; set; }
        
        // Engagement metrics
        public int TotalFavorites { get; set; }
        public int FavoritesThisMonth { get; set; }
        public double EngagementScore { get; set; }
        
        // Performance indicators
        public string PerformanceTrend { get; set; } = string.Empty; // "up", "down", "stable"
        public List<string> Recommendations { get; set; } = new();
    }

    public class AnalyticsOverview
    {
        // Total metrics
        public int TotalViews { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
        public double OverallRating { get; set; }
        
        // This month metrics
        public int ViewsThisMonth { get; set; }
        public int ReviewsThisMonth { get; set; }
        public int FavoritesThisMonth { get; set; }
        
        // Growth rates
        public double ViewsGrowthRate { get; set; }
        public double ReviewsGrowthRate { get; set; }
        public double FavoritesGrowthRate { get; set; }
        
        // Performance indicators
        public string TopPerformingBusiness { get; set; } = string.Empty;
        public string MostReviewedBusiness { get; set; } = string.Empty;
        public string MostFavoritedBusiness { get; set; } = string.Empty;
        
        // Insights
        public List<string> KeyInsights { get; set; } = new();
        public List<string> ActionableRecommendations { get; set; } = new();
    }

    public class ViewsOverTimeData
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
    }

    public class ReviewsOverTimeData
    {
        public DateTime Date { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
    }

    public class BusinessPerformanceInsight
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string InsightType { get; set; } = string.Empty; // "opportunity", "warning", "success"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionRecommendation { get; set; } = string.Empty;
        public int Priority { get; set; } // 1-5, 5 being highest
    }

    // Premium-only features
    public class CategoryBenchmarkData
    {
        public string Category { get; set; } = string.Empty;
        public double AverageViewsInCategory { get; set; }
        public double AverageRatingInCategory { get; set; }
        public double AverageReviewsInCategory { get; set; }
        public string YourPerformanceVsAverage { get; set; } = string.Empty; // "above", "below", "average"
        public List<BenchmarkMetric> Metrics { get; set; } = new();
    }

    public class BenchmarkMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public double YourValue { get; set; }
        public double CategoryAverage { get; set; }
        public double PercentilRank { get; set; } // 0-100
        public string Performance { get; set; } = string.Empty; // "excellent", "good", "average", "below_average", "poor"
    }

    public class CompetitorInsight
    {
        public string Category { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public int CompetitorCount { get; set; }
        public double AverageCompetitorRating { get; set; }
        public string MarketPosition { get; set; } = string.Empty; // "leader", "competitive", "challenger", "niche"
        public List<string> OpportunityAreas { get; set; } = new();
        public int YourRank { get; set; }
        public int TotalCompetitors { get; set; }
        public double MarketSharePercentage { get; set; }
        public string KeyInsight { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    // Extended model for detailed benchmarks view
    public class CategoryBenchmarks
    {
        public string Category { get; set; } = string.Empty;
        public string YourPerformanceVsAverage { get; set; } = string.Empty; // "above", "below", "average"
        
        // Your metrics
        public int YourAverageViews { get; set; }
        public int YourAverageReviews { get; set; }
        public double YourAverageRating { get; set; }
        
        // Category averages
        public int CategoryAverageViews { get; set; }
        public int CategoryAverageReviews { get; set; }
        public double CategoryAverageRating { get; set; }
        
        // Insights and recommendations
        public List<string> Insights { get; set; } = new();
    }
}