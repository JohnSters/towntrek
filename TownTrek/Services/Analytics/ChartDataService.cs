using TownTrek.Constants;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Service for chart data processing and formatting
    /// </summary>
    public class ChartDataService : IChartDataService
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IAnalyticsValidationService _validationService;
        private readonly IAnalyticsEventService _eventService;
        private readonly ILogger<ChartDataService> _logger;

        public ChartDataService(
            IAnalyticsService analyticsService,
            IAnalyticsValidationService validationService,
            IAnalyticsEventService eventService,
            ILogger<ChartDataService> logger)
        {
            _analyticsService = analyticsService;
            _validationService = validationService;
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Gets pre-formatted views chart data for Chart.js
        /// </summary>
        public async Task<ViewsChartDataResponse> GetViewsChartDataAsync(string userId, int days = 30, string? platform = null)
        {
            try
            {
                // Validate parameters
                var validation = _validationService.ValidateChartDataRequest(userId, days, platform);
                if (!validation.IsValid)
                {
                    throw new ArgumentException(validation.ErrorMessage);
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsChartData", new { Days = days, Platform = platform });

                var viewsData = await _analyticsService.GetViewsOverTimeByPlatformAsync(userId, days, platform);
                
                return new ViewsChartDataResponse
                {
                    Labels = viewsData.Select(d => d.Date.ToString(AnalyticsConstants.DateFormats.ShortDate)).ToList(),
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views chart data for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetViewsChartData", ex.Message, new { UserId = userId, Days = days, Platform = platform });
                throw;
            }
        }

        /// <summary>
        /// Gets pre-formatted reviews chart data for Chart.js
        /// </summary>
        public async Task<ReviewsChartDataResponse> GetReviewsChartDataAsync(string userId, int days = 30)
        {
            try
            {
                // Validate parameters
                var validation = _validationService.ValidateChartDataRequest(userId, days);
                if (!validation.IsValid)
                {
                    throw new ArgumentException(validation.ErrorMessage);
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ReviewsChartData", new { Days = days });

                var reviewsData = await _analyticsService.GetReviewsOverTimeAsync(userId, days);
                
                return new ReviewsChartDataResponse
                {
                    Labels = reviewsData.Select(d => d.Date.ToString(AnalyticsConstants.DateFormats.ShortDate)).ToList(),
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews chart data for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetReviewsChartData", ex.Message, new { UserId = userId, Days = days });
                throw;
            }
        }
    }
}
