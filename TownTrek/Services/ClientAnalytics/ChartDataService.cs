using TownTrek.Constants;
using TownTrek.Models.Exceptions;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces.ClientAnalytics;

namespace TownTrek.Services.ClientAnalytics
{
    /// <summary>
    /// Service responsible for processing and formatting analytics data for chart visualization.
    /// This service transforms raw analytics data into Chart.js compatible format for client-side rendering.
    /// </summary>
    /// <remarks>
    /// The service handles data validation, error handling, and analytics event tracking
    /// while providing pre-formatted chart data for views and reviews analytics.
    /// </remarks>
    public class ChartDataService(
        IClientAnalyticsService analyticsService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        IAnalyticsErrorHandler errorHandler,
        ILogger<ChartDataService> logger) : IChartDataService
    {
        // Core dependencies injected via constructor
        private readonly IClientAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<ChartDataService> _logger = logger;

        /// <summary>
        /// Retrieves and formats views analytics data for Chart.js visualization.
        /// </summary>
        /// <param name="userId">The unique identifier of the user requesting the data</param>
        /// <param name="days">Number of days to retrieve data for (default: 30 days)</param>
        /// <param name="platform">Optional platform filter (e.g., "web", "mobile")</param>
        /// <returns>
        /// A <see cref="ViewsChartDataResponse"/> containing formatted chart data with labels and datasets
        /// ready for Chart.js rendering
        /// </returns>
        /// <exception cref="AnalyticsValidationException">
        /// Thrown when the request parameters are invalid or validation fails
        /// </exception>
        /// <remarks>
        /// This method performs the following operations:
        /// 1. Validates input parameters before processing
        /// 2. Records analytics access for audit purposes
        /// 3. Retrieves raw views data from the analytics service
        /// 4. Transforms the data into Chart.js compatible format
        /// 5. Applies consistent styling using predefined color constants
        /// </remarks>
        public async Task<ViewsChartDataResponse> GetViewsChartDataAsync(string userId, int days = 30, string? platform = null)
        {
            // Wrap the entire operation in error handling to ensure consistent error reporting
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Validate input parameters before processing
                var validation = _validationService.ValidateChartDataRequest(userId, days, platform);
                if (!validation.IsValid)
                {
                    // Log validation failure and record analytics event for monitoring
                    await _errorHandler.HandleValidationExceptionAsync(
                        validation.ErrorMessage ?? "Invalid chart data request",
                        userId,
                        "ChartDataRequest",
                        "InvalidRequest",
                        new Dictionary<string, object> { ["Days"] = days, ["Platform"] = platform ?? string.Empty }
                    );
                    throw new AnalyticsValidationException(validation.ErrorMessage ?? "Invalid chart data request", "ChartDataRequest", "InvalidRequest");
                }

                // Step 2: Record analytics access event for audit trail and usage tracking
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsChartData", new { Days = days, Platform = platform });

                // Step 3: Retrieve raw views data from the analytics service
                var viewsData = await _analyticsService.GetViewsOverTimeByPlatformAsync(userId, days, platform);
                
                // Step 4: Transform raw data into Chart.js compatible format
                return new ViewsChartDataResponse
                {
                    // Format dates for chart labels using consistent date formatting
                    Labels = viewsData.Select(d => d.Date.ToString(AnalyticsConstants.DateFormats.ShortDate ?? "MMM dd")).ToList(),
                    Datasets = new List<ChartDataset>
                    {
                        new ChartDataset
                        {
                            Label = "Views",
                            Data = viewsData.Select(d => (double)d.Views).ToList(),
                            // Apply consistent branding colors from constants
                            BorderColor = AnalyticsConstants.ChartColors.LapisLazuli,
                            BackgroundColor = AnalyticsConstants.ChartColors.LapisLazuli + AnalyticsConstants.ChartOpacity.Light,
                            Tension = 0.4 // Smooth curve for better visual appeal
                        }
                    }
                };
            }, userId, "GetViewsChartData", new Dictionary<string, object> { ["Days"] = days, ["Platform"] = platform ?? string.Empty });
        }

        /// <summary>
        /// Retrieves and formats reviews analytics data for Chart.js visualization.
        /// </summary>
        /// <param name="userId">The unique identifier of the user requesting the data</param>
        /// <param name="days">Number of days to retrieve data for (default: 30 days)</param>
        /// <returns>
        /// A <see cref="ReviewsChartDataResponse"/> containing formatted chart data with labels and datasets
        /// ready for Chart.js rendering
        /// </returns>
        /// <exception cref="AnalyticsValidationException">
        /// Thrown when the request parameters are invalid or validation fails
        /// </exception>
        /// <remarks>
        /// This method performs the following operations:
        /// 1. Validates input parameters using the validation service
        /// 2. Records analytics access for audit purposes
        /// 3. Retrieves raw reviews data from the analytics service
        /// 4. Transforms the data into Chart.js compatible format
        /// 5. Applies consistent styling using predefined color constants
        /// 
        /// The reviews chart displays average ratings over time, providing insights into
        /// business performance and customer satisfaction trends.
        /// </remarks>
        public async Task<ReviewsChartDataResponse> GetReviewsChartDataAsync(string userId, int days = 30)
        {
            // Wrap the entire operation in error handling to ensure consistent error reporting
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Validate input parameters before processing
                var validation = _validationService.ValidateChartDataRequest(userId, days);
                if (!validation.IsValid)
                {
                    // Log validation failure and record analytics event for monitoring
                    await _errorHandler.HandleValidationExceptionAsync(
                        validation.ErrorMessage ?? "Invalid chart data request",
                        userId,
                        "ChartDataRequest",
                        "InvalidRequest",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(validation.ErrorMessage ?? "Invalid chart data request", "ChartDataRequest", "InvalidRequest");
                }

                // Step 2: Record analytics access event for audit trail and usage tracking
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ReviewsChartData", new { Days = days });

                // Step 3: Retrieve raw reviews data from the analytics service
                var reviewsData = await _analyticsService.GetReviewsOverTimeAsync(userId, days);
                
                // Step 4: Transform raw data into Chart.js compatible format
                return new ReviewsChartDataResponse
                {
                    // Format dates for chart labels using consistent date formatting
                    Labels = reviewsData.Select(d => d.Date.ToString(AnalyticsConstants.DateFormats.ShortDate ?? "MMM dd")).ToList(),
                    Datasets =
                    [
                        new ChartDataset
                        {
                            Label = "Average Rating",
                            Data = reviewsData.Select(d => d.AverageRating).ToList(),
                            // Apply consistent branding colors from constants (different from views for visual distinction)
                            BorderColor = AnalyticsConstants.ChartColors.HunyadiYellow,
                            BackgroundColor = AnalyticsConstants.ChartColors.HunyadiYellow + AnalyticsConstants.ChartOpacity.Light,
                            Tension = 0.4 // Smooth curve for better visual appeal
                        }
                    ]
                };
            }, userId, "GetReviewsChartData", new Dictionary<string, object> { ["Days"] = days });
        }
    }
}
