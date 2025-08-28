using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class ClientAnalyticsViewModel
    {
        // User and subscription info (simplified to avoid circular references)
        public UserInfo User { get; set; } = new();
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
        

        
        // Comparison data (Premium only)
        public CategoryBenchmarkData? CategoryBenchmarks { get; set; }
        public List<CompetitorInsight> CompetitorInsights { get; set; } = new();
    }

    // Simplified user info to avoid circular references
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string AuthenticationMethod { get; set; } = string.Empty;
        public string? CurrentSubscriptionTier { get; set; }
        public bool HasActiveSubscription { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsTrialUser { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool TrialExpired { get; set; }
    }

    public class BusinessAnalyticsData
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        
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
        public double ReviewsGrowthRate { get; set; }
        public double RatingGrowthRate { get; set; }
        
        // Engagement metrics
        public int TotalFavorites { get; set; }
        public int FavoritesThisMonth { get; set; }
        public int FavoritesLastMonth { get; set; }
        public double FavoritesGrowthRate { get; set; }
        public double EngagementScore { get; set; }
        
        // Performance indicators
        public string PerformanceTrend { get; set; } = string.Empty; // "up", "down", "stable"
        public double PerformanceRating { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class AnalyticsOverview
    {
        // Total metrics
        public int TotalViews { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
        public double OverallRating { get; set; }
        public int TotalBusinesses { get; set; }
        public double AverageRating { get; set; }
        public double AverageEngagementScore { get; set; }
        
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
        public int Reviews { get; set; } // Alias for ReviewCount for compatibility
        public double AverageRating { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
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
        
        // Additional properties for compatibility
        public int UserBusinessesCount { get; set; }
        public int CategoryTotalBusinesses { get; set; }
        public double AverageCategoryRating { get; set; }
        public double UserAverageRating { get; set; }
        public double PerformanceRating { get; set; }
        
        // Additional properties needed by ClientAnalyticsService
        public int UserBusinessCount { get; set; }
        public int TotalBusinessCount { get; set; }
        public double AverageViews { get; set; }
        public double AverageReviews { get; set; }
        public double AverageRating { get; set; }
        public double UserAverageViews { get; set; }
        public double UserAverageReviews { get; set; }
        
        // Additional properties needed by ClientAnalyticsService
        public List<CompetitorInsight> TopPerformers { get; set; } = new();
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
        
        // Additional properties for compatibility
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public int CompetitorsCount { get; set; } // Alias for CompetitorCount
        
        // Additional properties needed by ClientAnalyticsService
        public double AverageCompetitorViews { get; set; }
        public double AverageCompetitorReviews { get; set; }
        public int UserViews { get; set; }
        public int UserReviews { get; set; }
        public double UserRating { get; set; }
        public int ViewsRank { get; set; }
        public int ReviewsRank { get; set; }
        public int RatingRank { get; set; }
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
        
        // Additional properties for compatibility
        public int UserBusinessesCount { get; set; }
        public int CategoryTotalBusinesses { get; set; }
        public double AverageCategoryRating { get; set; }
        public double UserAverageRating { get; set; }
        public double PerformanceRating { get; set; }
        public List<BenchmarkMetric> DetailedMetrics { get; set; } = new();
        
        // Additional properties needed by ClientAnalyticsService
        public int UserBusinessCount { get; set; }
        public int TotalBusinessCount { get; set; }
        public double AverageViews { get; set; }
        public double AverageReviews { get; set; }
        public double AverageRating { get; set; }
        public double UserAverageViews { get; set; }
        public double UserAverageReviews { get; set; }
        
        // Additional properties needed by ClientAnalyticsService
        public List<CompetitorInsight> TopPerformers { get; set; } = new();
    }


}