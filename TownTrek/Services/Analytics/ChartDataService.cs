using TownTrek.Constants;
using TownTrek.Models.Exceptions;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Service for chart data processing and formatting
    /// </summary>
    public class ChartDataService(
        IAnalyticsService analyticsService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        IAnalyticsErrorHandler errorHandler,
        ILogger<ChartDataService> logger) : IChartDataService
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<ChartDataService> _logger = logger;

        /// <summary>
        /// Gets pre-formatted views chart data for Chart.js
        /// </summary>
        public async Task<ViewsChartDataResponse> GetViewsChartDataAsync(string userId, int days = 30, string? platform = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate parameters
                var validation = _validationService.ValidateChartDataRequest(userId, days, platform);
                if (!validation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        validation.ErrorMessage ?? "Invalid chart data request",
                        userId,
                        "ChartDataRequest",
                        "InvalidRequest",
                        new Dictionary<string, object> { ["Days"] = days, ["Platform"] = platform ?? string.Empty }
                    );
                    throw new AnalyticsValidationException(validation.ErrorMessage ?? "Invalid chart data request", "ChartDataRequest", "InvalidRequest");
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsChartData", new { Days = days, Platform = platform });

                var viewsData = await _analyticsService.GetViewsOverTimeByPlatformAsync(userId, days, platform);
                
                return new ViewsChartDataResponse
                {
                    Labels = viewsData.Select(d => d.Date.ToString(AnalyticsConstants.DateFormats.ShortDate ?? "MMM dd")).ToList(),
                    Datasets = new List<ChartDataset>
                    {
                        new ChartDataset
                        {
                            Label = "Views",
                            Data = viewsData.Select(d => (double)d.Views).ToList(),
                            BorderColor = AnalyticsConstants.ChartColors.LapisLazuli,
                            BackgroundColor = AnalyticsConstants.ChartColors.LapisLazuli + AnalyticsConstants.ChartOpacity.Light,
                            Tension = 0.4
                        }
                    }
                };
            }, userId, "GetViewsChartData", new Dictionary<string, object> { ["Days"] = days, ["Platform"] = platform ?? string.Empty });
        }

        /// <summary>
        /// Gets pre-formatted reviews chart data for Chart.js
        /// </summary>
        public async Task<ReviewsChartDataResponse> GetReviewsChartDataAsync(string userId, int days = 30)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate parameters
                var validation = _validationService.ValidateChartDataRequest(userId, days);
                if (!validation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        validation.ErrorMessage ?? "Invalid chart data request",
                        userId,
                        "ChartDataRequest",
                        "InvalidRequest",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(validation.ErrorMessage ?? "Invalid chart data request", "ChartDataRequest", "InvalidRequest");
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ReviewsChartData", new { Days = days });

                var reviewsData = await _analyticsService.GetReviewsOverTimeAsync(userId, days);
                
                return new ReviewsChartDataResponse
                {
                    Labels = reviewsData.Select(d => d.Date.ToString(AnalyticsConstants.DateFormats.ShortDate ?? "MMM dd")).ToList(),
                    Datasets = new List<ChartDataset>
                    {
                        new ChartDataset
                        {
                            Label = "Reviews",
                            Data = reviewsData.Select(d => (double)d.Reviews).ToList(),
                            BorderColor = AnalyticsConstants.ChartColors.HunyadiYellow,
                            BackgroundColor = AnalyticsConstants.ChartColors.HunyadiYellow + AnalyticsConstants.ChartOpacity.Light,
                            Tension = 0.4
                        }
                    }
                };
            }, userId, "GetReviewsChartData", new Dictionary<string, object> { ["Days"] = days });
        }
    }
}
