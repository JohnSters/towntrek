using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.Exceptions;
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
        IAnalyticsErrorHandler errorHandler,
        ILogger<BusinessMetricsService> logger) : IBusinessMetricsService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<BusinessMetricsService> _logger = logger;

        public async Task RecordBusinessViewAsync(int businessId)
        {
            await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                await _viewTrackingService.LogBusinessViewAsync(businessId, null, "Web", null, null, null, null);
                await _eventService.RecordBusinessViewEventAsync(businessId);
            }, string.Empty, "RecordBusinessView", new Dictionary<string, object> { ["BusinessId"] = businessId });
        }

        public async Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate parameters
                var dateRangeValidation = _validationService.ValidateDateRange(startDate, endDate);
                if (!dateRangeValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        dateRangeValidation.ErrorMessage ?? "Invalid date range",
                        string.Empty,
                        "DateRange",
                        "InvalidRange",
                        new Dictionary<string, object> { ["BusinessId"] = businessId, ["StartDate"] = startDate, ["EndDate"] = endDate }
                    );
                    throw new AnalyticsValidationException(dateRangeValidation.ErrorMessage ?? "Invalid date range", "DateRange", "InvalidRange");
                }

                var platformValidation = _validationService.ValidatePlatform(platform);
                if (!platformValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        platformValidation.ErrorMessage ?? "Invalid platform",
                        string.Empty,
                        "Platform",
                        "InvalidPlatform",
                        new Dictionary<string, object> { ["BusinessId"] = businessId, ["Platform"] = platform ?? string.Empty }
                    );
                    throw new AnalyticsValidationException(platformValidation.ErrorMessage ?? "Invalid platform", "Platform", "InvalidPlatform");
                }

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(new List<int> { businessId }, startDate, endDate, platform);

                return new ViewStatistics
                {
                    TotalViews = viewLogs.Count,
                    UniqueVisitors = viewLogs.Select(v => v.IpAddress).Distinct().Count(),
                    AverageViewsPerDay = viewLogs.Count / Math.Max(1, (endDate - startDate).Days),
                    PeakDayViews = viewLogs.GroupBy(v => v.ViewedAt.Date).Max(g => g.Count()),
                    PeakDayDate = viewLogs.GroupBy(v => v.ViewedAt.Date).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? DateTime.UtcNow,
                    PlatformBreakdown = viewLogs.GroupBy(v => v.Platform).ToDictionary(g => g.Key ?? "Unknown", g => g.Count())
                };
            }, string.Empty, "GetBusinessViewStatistics", new Dictionary<string, object> { ["BusinessId"] = businessId, ["StartDate"] = startDate, ["EndDate"] = endDate, ["Platform"] = platform ?? string.Empty });
        }

        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate parameters
                var daysValidation = _validationService.ValidateAnalyticsDays(days);
                if (!daysValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        daysValidation.ErrorMessage ?? "Invalid days parameter",
                        userId,
                        "Days",
                        "InvalidDays",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(daysValidation.ErrorMessage ?? "Invalid days parameter", "Days", "InvalidDays");
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
            }, userId, "GetViewsOverTime", new Dictionary<string, object> { ["Days"] = days });
        }

        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate parameters
                var daysValidation = _validationService.ValidateAnalyticsDays(days);
                if (!daysValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        daysValidation.ErrorMessage ?? "Invalid days parameter",
                        userId,
                        "Days",
                        "InvalidDays",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(daysValidation.ErrorMessage ?? "Invalid days parameter", "Days", "InvalidDays");
                }

                var platformValidation = _validationService.ValidatePlatform(platform);
                if (!platformValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        platformValidation.ErrorMessage ?? "Invalid platform",
                        userId,
                        "Platform",
                        "InvalidPlatform",
                        new Dictionary<string, object> { ["Platform"] = platform ?? string.Empty }
                    );
                    throw new AnalyticsValidationException(platformValidation.ErrorMessage ?? "Invalid platform", "Platform", "InvalidPlatform");
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
            }, userId, "GetViewsOverTimeByPlatform", new Dictionary<string, object> { ["Days"] = days, ["Platform"] = platform ?? string.Empty });
        }

        public async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate parameters
                var daysValidation = _validationService.ValidateAnalyticsDays(days);
                if (!daysValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        daysValidation.ErrorMessage ?? "Invalid days parameter",
                        userId,
                        "Days",
                        "InvalidDays",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(daysValidation.ErrorMessage ?? "Invalid days parameter", "Days", "InvalidDays");
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
            }, userId, "GetReviewsOverTime", new Dictionary<string, object> { ["Days"] = days });
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
