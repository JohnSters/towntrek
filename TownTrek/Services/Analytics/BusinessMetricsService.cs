using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Service for business-specific metrics and view tracking functionality
    /// </summary>
    public class BusinessMetricsService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        IViewTrackingService viewTrackingService,
        ILogger<BusinessMetricsService> logger) : IBusinessMetricsService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly ILogger<BusinessMetricsService> _logger = logger;

        public async Task RecordBusinessViewAsync(int businessId)
        {
            try
            {
                await _viewTrackingService.LogBusinessViewAsync(businessId, null, "Web", null, null, null, null);
                await _eventService.RecordBusinessViewEventAsync(businessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording business view for BusinessId {BusinessId}", businessId);
                // Don't throw - view recording should not break the main functionality
            }
        }

        public async Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            try
            {
                // Validate parameters
                var dateRangeValidation = _validationService.ValidateDateRange(startDate, endDate);
                if (!dateRangeValidation.IsValid)
                {
                    throw new ArgumentException(dateRangeValidation.ErrorMessage);
                }

                var platformValidation = _validationService.ValidatePlatform(platform);
                if (!platformValidation.IsValid)
                {
                    throw new ArgumentException(platformValidation.ErrorMessage, nameof(platform));
                }

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(new List<int> { businessId }, startDate, endDate, platform);

                return new ViewStatistics
                {
                    TotalViews = viewLogs.Count,
                    UniqueVisitors = viewLogs.Select(v => v.IpAddress).Distinct().Count(),
                    AverageViewsPerDay = viewLogs.Count / Math.Max(1, (endDate - startDate).Days),
                    PeakDayViews = viewLogs.GroupBy(v => v.ViewedAt.Date).Max(g => g.Count()),
                    PeakDayDate = viewLogs.GroupBy(v => v.ViewedAt.Date).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? DateTime.UtcNow,
                    PlatformBreakdown = viewLogs.GroupBy(v => v.Platform).ToDictionary(g => g.Key, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting business view statistics for BusinessId {BusinessId}", businessId);
                throw;
            }
        }

        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30)
        {
            try
            {
                // Validate parameters
                var daysValidation = _validationService.ValidateAnalyticsDays(days);
                if (!daysValidation.IsValid)
                {
                    throw new ArgumentException(daysValidation.ErrorMessage, nameof(days));
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsOverTime", new { Days = days });

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return new List<ViewsOverTimeData>();

                var businessIds = businesses.Select(b => b.Id).ToList();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, startDate, endDate);

                return ProcessViewsOverTimeData(viewLogs, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views over time for UserId {UserId}, Days {Days}", userId, days);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetViewsOverTime", ex.Message, new { UserId = userId, Days = days });
                throw;
            }
        }

        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null)
        {
            try
            {
                // Validate parameters
                var daysValidation = _validationService.ValidateAnalyticsDays(days);
                if (!daysValidation.IsValid)
                {
                    throw new ArgumentException(daysValidation.ErrorMessage, nameof(days));
                }

                var platformValidation = _validationService.ValidatePlatform(platform);
                if (!platformValidation.IsValid)
                {
                    throw new ArgumentException(platformValidation.ErrorMessage, nameof(platform));
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsOverTimeByPlatform", new { Days = days, Platform = platform });

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return new List<ViewsOverTimeData>();

                var businessIds = businesses.Select(b => b.Id).ToList();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, startDate, endDate, platform);

                return ProcessViewsOverTimeData(viewLogs, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views over time by platform for UserId {UserId}, Days {Days}, Platform {Platform}", userId, days, platform);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetViewsOverTimeByPlatform", ex.Message, new { UserId = userId, Days = days, Platform = platform });
                throw;
            }
        }

        public async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30)
        {
            try
            {
                // Validate parameters
                var daysValidation = _validationService.ValidateAnalyticsDays(days);
                if (!daysValidation.IsValid)
                {
                    throw new ArgumentException(daysValidation.ErrorMessage, nameof(days));
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ReviewsOverTime", new { Days = days });

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return new List<ReviewsOverTimeData>();

                var businessIds = businesses.Select(b => b.Id).ToList();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var reviews = await _dataService.GetBusinessReviewsAsync(businessIds, startDate, endDate);

                return ProcessReviewsOverTimeData(reviews, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews over time for UserId {UserId}, Days {Days}", userId, days);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetReviewsOverTime", ex.Message, new { UserId = userId, Days = days });
                throw;
            }
        }

        // Private helper methods
        private List<ViewsOverTimeData> ProcessViewsOverTimeData(List<BusinessViewLog> viewLogs, DateTime startDate, DateTime endDate)
        {
            var result = new List<ViewsOverTimeData>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayViews = viewLogs.Count(v => v.ViewedAt.Date == currentDate);
                result.Add(new ViewsOverTimeData
                {
                    Date = currentDate,
                    Views = dayViews
                });
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        private List<ReviewsOverTimeData> ProcessReviewsOverTimeData(List<BusinessReview> reviews, DateTime startDate, DateTime endDate)
        {
            var result = new List<ReviewsOverTimeData>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayReviews = reviews.Where(r => r.CreatedAt.Date == currentDate).ToList();
                result.Add(new ReviewsOverTimeData
                {
                    Date = currentDate,
                    Reviews = dayReviews.Count,
                    AverageRating = dayReviews.Any() ? dayReviews.Average(r => r.Rating) : 0
                });
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }
    }
}
