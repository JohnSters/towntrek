namespace TownTrek.Constants
{
    /// <summary>
    /// Constants used throughout the analytics system
    /// </summary>
    public static class AnalyticsConstants
    {
        // Time periods
        public const int DefaultAnalyticsDays = 30;
        public const int MaxAnalyticsDays = 365;
        public const int MinAnalyticsDays = 1;
        
        // Growth rate calculation periods
        public const int DefaultGrowthRateDays = 30;
        
        // Rating thresholds
        public const double LowRatingThreshold = 3.0;
        public const double GoodRatingThreshold = 4.0;
        public const double ExcellentRatingThreshold = 4.5;
        public const double PoorRatingThreshold = 3.5;
        
        // View count thresholds
        public const int LowVisibilityThreshold = 50;
        public const int GoodVisibilityThreshold = 100;
        
        // Engagement thresholds
        public const int MinFavoritesThreshold = 5;
        public const int StrongEngagementThreshold = 50;
        
        // Performance thresholds
        public const double ExcellentPerformanceRatio = 1.5;
        public const double GoodPerformanceRatio = 1.2;
        public const double AveragePerformanceRatio = 0.8;
        public const double BelowAveragePerformanceRatio = 0.5;
        
        // Market position thresholds
        public const double LeaderRatingDifference = 0.5;
        public const double ChallengerRatingDifference = 0.5;
        
        // Percentage calculations
        public const double PercentageMultiplier = 100.0;
        public const double EngagementScoreMultiplier = 2.0;
        
        // Benchmark thresholds
        public const int MinCategoryBusinessesForBenchmark = 5;
        public const double DefaultPercentile = 50.0;
        
        // Change thresholds for insights
        public const double SignificantViewsChangeThreshold = 20.0;
        public const double SignificantReviewsChangeThreshold = 30.0;
        public const double SignificantRatingChangeThreshold = 10.0;
        public const double SignificantRatingChangeThresholdForKeyChanges = 5.0;
        
        // Performance rating thresholds
        public const double ExcellentChangeThreshold = 20.0;
        public const double GoodChangeThreshold = 10.0;
        public const double FairChangeThreshold = -10.0;
        
        // Metrics count for averaging
        public const int MetricsCountForAveraging = 5;
        
        // Chart colors (following design system)
        public static class ChartColors
        {
            public const string LapisLazuli = "#33658a";
            public const string CarolinaBlue = "#86bbd8";
            public const string HunyadiYellow = "#f6ae2d";
            public const string OrangePantone = "#f26419";
            public const string Charcoal = "#2f4858";
            public const string DarkGray = "#6c757d";
            public const string LightGray = "#e9ecef";
        }
        
        // Chart color opacity
        public static class ChartOpacity
        {
            public const string Light = "20";
            public const string Medium = "40";
            public const string Heavy = "80";
        }
        
        // Placeholder multipliers for category/portfolio comparisons
        public static class PlaceholderMultipliers
        {
            public const double CategoryViewsMultiplier = 0.8;
            public const double CategoryReviewsMultiplier = 0.9;
            public const double CategoryRatingMultiplier = 0.95;
            public const double PortfolioViewsMultiplier = 1.1;
            public const double PortfolioReviewsMultiplier = 1.05;
            public const double PortfolioRatingMultiplier = 1.02;
        }
        
        // Date format patterns
        public static class DateFormats
        {
            public const string ShortDate = "MMM dd";
            public const string LongDate = "ddd, MMM dd";
        }
        
        // Business status values
        public static class BusinessStatus
        {
            public const string Active = "Active";
            public const string Deleted = "Deleted";
        }
        
        // Platform values
        public static class Platforms
        {
            public const string Web = "Web";
            public const string Mobile = "Mobile";
            public const string Api = "API";
            public const string All = "All";
        }
        
        // Performance trend values
        public static class PerformanceTrends
        {
            public const string Up = "up";
            public const string Down = "down";
            public const string Stable = "stable";
        }
        
        // Overall performance values
        public static class OverallPerformance
        {
            public const string Above = "above";
            public const string Below = "below";
            public const string Average = "average";
        }
        
        // Metric performance values
        public static class MetricPerformance
        {
            public const string Excellent = "excellent";
            public const string Good = "good";
            public const string Average = "average";
            public const string BelowAverage = "below_average";
            public const string Poor = "poor";
        }
        
        // Market position values
        public static class MarketPosition
        {
            public const string Leader = "leader";
            public const string Competitive = "competitive";
            public const string Challenger = "challenger";
            public const string Niche = "niche";
        }
        
        // Overall trend values
        public static class OverallTrend
        {
            public const string Improving = "Improving";
            public const string Declining = "Declining";
            public const string Stable = "Stable";
        }
        
        // Performance rating values
        public static class PerformanceRating
        {
            public const string Excellent = "Excellent";
            public const string Good = "Good";
            public const string Fair = "Fair";
            public const string Poor = "Poor";
        }
        
        // Category performance values
        public static class CategoryPerformance
        {
            public const string AboveAverage = "Above Average";
            public const string BelowAverage = "Below Average";
        }
    }
}
