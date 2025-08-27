using System.Text.Json.Serialization;

namespace TownTrek.Models.ViewModels
{
    /// <summary>
    /// Request model for comparative analysis
    /// </summary>
    public class ComparativeAnalysisRequest
    {
        /// <summary>
        /// Type of comparison to perform
        /// </summary>
        [JsonPropertyName("comparisonType")]
        public string ComparisonType { get; set; } = "PeriodOverPeriod"; // PeriodOverPeriod, YearOverYear, CustomRange
        
        /// <summary>
        /// Business ID for business-specific analysis (null for overview)
        /// </summary>
        [JsonPropertyName("businessId")]
        public int? BusinessId { get; set; }
        
        /// <summary>
        /// Current period start date
        /// </summary>
        [JsonPropertyName("currentPeriodStart")]
        public DateTime CurrentPeriodStart { get; set; }
        
        /// <summary>
        /// Current period end date
        /// </summary>
        [JsonPropertyName("currentPeriodEnd")]
        public DateTime CurrentPeriodEnd { get; set; }
        
        /// <summary>
        /// Previous period start date (auto-calculated for standard comparisons)
        /// </summary>
        [JsonPropertyName("previousPeriodStart")]
        public DateTime? PreviousPeriodStart { get; set; }
        
        /// <summary>
        /// Previous period end date (auto-calculated for standard comparisons)
        /// </summary>
        [JsonPropertyName("previousPeriodEnd")]
        public DateTime? PreviousPeriodEnd { get; set; }
        
        /// <summary>
        /// Platform filter (Web, Mobile, API, null for all)
        /// </summary>
        [JsonPropertyName("platform")]
        public string? Platform { get; set; }
    }

    /// <summary>
    /// Response model for comparative analysis
    /// </summary>
    public class ComparativeAnalysisResponse
    {
        /// <summary>
        /// Type of comparison performed
        /// </summary>
        public string ComparisonType { get; set; } = string.Empty;
        
        /// <summary>
        /// Current period data
        /// </summary>
        public PeriodData CurrentPeriod { get; set; } = new();
        
        /// <summary>
        /// Previous period data
        /// </summary>
        public PeriodData PreviousPeriod { get; set; } = new();
        
        /// <summary>
        /// Comparison metrics showing changes between periods
        /// </summary>
        public ComparisonMetrics ComparisonMetrics { get; set; } = new();
        
        /// <summary>
        /// Chart data for visualization
        /// </summary>
        public ComparativeChartData ChartData { get; set; } = new();
        
        /// <summary>
        /// Insights and recommendations based on comparison
        /// </summary>
        public List<string> Insights { get; set; } = new();
        
        /// <summary>
        /// Business-specific data (if applicable)
        /// </summary>
        public BusinessComparisonData? BusinessData { get; set; }
    }

    /// <summary>
    /// Data for a specific time period
    /// </summary>
    public class PeriodData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalViews { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
        public double AverageRating { get; set; }
        public double EngagementScore { get; set; }
        public double AverageViewsPerDay { get; set; }
        public double AverageReviewsPerDay { get; set; }
        public double AverageFavoritesPerDay { get; set; }
        public int PeakDayViews { get; set; }
        public DateTime? PeakDayDate { get; set; }
        public int LowDayViews { get; set; }
        public DateTime? LowDayDate { get; set; }
    }

    /// <summary>
    /// Metrics comparing two periods
    /// </summary>
    public class ComparisonMetrics
    {
        public double ViewsChangePercent { get; set; }
        public double ReviewsChangePercent { get; set; }
        public double FavoritesChangePercent { get; set; }
        public double RatingChangePercent { get; set; }
        public double EngagementChangePercent { get; set; }
        public double AverageViewsPerDayChangePercent { get; set; }
        public double AverageReviewsPerDayChangePercent { get; set; }
        public double AverageFavoritesPerDayChangePercent { get; set; }
        
        // Additional properties needed by AnalyticsService
        public double ViewsGrowthPercentage { get; set; }
        public double ReviewsGrowthPercentage { get; set; }
        public double RatingGrowthPercentage { get; set; }
        public double EngagementGrowthPercentage { get; set; }
        public string OverallPerformanceChange { get; set; } = string.Empty;
        
        /// <summary>
        /// Trend indicators: "Improving", "Declining", "Stable"
        /// </summary>
        public string OverallTrend { get; set; } = string.Empty;
        
        /// <summary>
        /// Performance rating: "Excellent", "Good", "Fair", "Poor"
        /// </summary>
        public string PerformanceRating { get; set; } = string.Empty;
        
        /// <summary>
        /// Key areas of improvement or concern
        /// </summary>
        public List<string> KeyChanges { get; set; } = new();
    }

    /// <summary>
    /// Chart data for comparative analysis visualization
    /// </summary>
    public class ComparativeChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<ComparativeChartDataset> Datasets { get; set; } = new();
        public string ChartType { get; set; } = "bar";
        public string TimeRange { get; set; } = string.Empty;
    }

    /// <summary>
    /// Chart dataset for comparative analysis
    /// </summary>
    public class ComparativeChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<double> Data { get; set; } = new();
        
        [JsonPropertyName("borderColor")]
        public string BorderColor { get; set; } = string.Empty;
        
        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = string.Empty;
        
        [JsonPropertyName("fill")]
        public bool Fill { get; set; } = false;
        
        [JsonPropertyName("tension")]
        public double Tension { get; set; } = 0.4;
        
        [JsonPropertyName("borderWidth")]
        public int BorderWidth { get; set; } = 2;
    }

    /// <summary>
    /// Business-specific comparison data
    /// </summary>
    public class BusinessComparisonData
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        // Additional properties needed by AnalyticsService
        public int CurrentPeriodViews { get; set; }
        public int PreviousPeriodViews { get; set; }
        public int CurrentPeriodReviews { get; set; }
        public int PreviousPeriodReviews { get; set; }
        
        /// <summary>
        /// Comparison against category average
        /// </summary>
        public CategoryComparison CategoryComparison { get; set; } = new();
        
        /// <summary>
        /// Comparison against all user businesses
        /// </summary>
        public PortfolioComparison PortfolioComparison { get; set; } = new();
    }

    /// <summary>
    /// Comparison against category average
    /// </summary>
    public class CategoryComparison
    {
        public double CategoryAverageViews { get; set; }
        public double CategoryAverageReviews { get; set; }
        public double CategoryAverageRating { get; set; }
        public double ViewsVsCategory { get; set; } // Percentage difference
        public double ReviewsVsCategory { get; set; } // Percentage difference
        public double RatingVsCategory { get; set; } // Percentage difference
        public string CategoryPerformance { get; set; } = string.Empty; // "Above Average", "Average", "Below Average"
    }

    /// <summary>
    /// Comparison against user's portfolio
    /// </summary>
    public class PortfolioComparison
    {
        public double PortfolioAverageViews { get; set; }
        public double PortfolioAverageReviews { get; set; }
        public double PortfolioAverageRating { get; set; }
        public double ViewsVsPortfolio { get; set; } // Percentage difference
        public double ReviewsVsPortfolio { get; set; } // Percentage difference
        public double RatingVsPortfolio { get; set; } // Percentage difference
        public int PortfolioRank { get; set; } // Rank among user's businesses
        public int TotalPortfolioBusinesses { get; set; }
    }

    /// <summary>
    /// Predefined comparison periods
    /// </summary>
    public static class ComparisonPeriods
    {
        public static readonly Dictionary<string, (int CurrentDays, int PreviousDays)> Periods = new()
        {
            ["WeekOverWeek"] = (7, 7),
            ["MonthOverMonth"] = (30, 30),
            ["QuarterOverQuarter"] = (90, 90),
            ["YearOverYear"] = (365, 365),
            ["Last30VsPrevious30"] = (30, 30),
            ["Last7VsPrevious7"] = (7, 7),
            ["Last90VsPrevious90"] = (90, 90)
        };
    }
}
