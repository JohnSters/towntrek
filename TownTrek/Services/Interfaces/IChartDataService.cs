using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for chart data processing and formatting
    /// </summary>
    public interface IChartDataService
    {
        /// <summary>
        /// Gets pre-formatted views chart data for Chart.js
        /// </summary>
        Task<ViewsChartDataResponse> GetViewsChartDataAsync(string userId, int days = 30, string? platform = null);
        
        /// <summary>
        /// Gets pre-formatted reviews chart data for Chart.js
        /// </summary>
        Task<ReviewsChartDataResponse> GetReviewsChartDataAsync(string userId, int days = 30);
    }
}
