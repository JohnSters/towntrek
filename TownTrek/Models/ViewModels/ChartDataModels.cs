using System.Text.Json.Serialization;

namespace TownTrek.Models.ViewModels
{
    /// <summary>
    /// Pre-formatted chart data for Chart.js consumption
    /// </summary>
    public class ChartDataResponse
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartDataset> Datasets { get; set; } = new();
    }

    /// <summary>
    /// Chart dataset configuration for Chart.js
    /// </summary>
    public class ChartDataset
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
        public int BorderWidth { get; set; } = 1;
    }

    /// <summary>
    /// Request model for chart data endpoints
    /// </summary>
    public class ChartDataRequest
    {
        public int Days { get; set; } = 30;
        public string? Platform { get; set; }
        public string? BusinessId { get; set; }
    }

    /// <summary>
    /// Views chart data response
    /// </summary>
    public class ViewsChartDataResponse : ChartDataResponse
    {
        public string ChartType { get; set; } = "line";
        public string TimeRange { get; set; } = string.Empty;
        public int TotalViews { get; set; }
        public double AverageViewsPerDay { get; set; }
    }

    /// <summary>
    /// Reviews chart data response
    /// </summary>
    public class ReviewsChartDataResponse : ChartDataResponse
    {
        public string ChartType { get; set; } = "bar";
        public string TimeRange { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public double AverageReviewsPerDay { get; set; }
    }

    /// <summary>
    /// Performance metrics chart data response
    /// </summary>
    public class PerformanceChartDataResponse : ChartDataResponse
    {
        public string ChartType { get; set; } = "line";
        public string TimeRange { get; set; } = string.Empty;
        public double AverageEngagementScore { get; set; }
        public double GrowthRate { get; set; }
    }
}
