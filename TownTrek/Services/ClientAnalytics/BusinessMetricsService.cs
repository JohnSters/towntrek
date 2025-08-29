using TownTrek.Models;
using TownTrek.Models.Exceptions;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces.ClientAnalytics;

namespace TownTrek.Services.ClientAnalytics
{
    /// <summary>
    /// Service responsible for business-specific metrics calculation and view tracking functionality.
    /// This service provides comprehensive analytics for business performance, including view statistics,
    /// time-series data analysis, and review metrics.
    /// </summary>
    /// <remarks>
    /// The service handles:
    /// - Business view tracking and recording
    /// - View statistics calculation (total views, unique visitors, peak days)
    /// - Time-series data processing for views and reviews
    /// - Platform-specific analytics filtering
    /// - Data validation and error handling for all analytics operations
    /// </remarks>
    public class BusinessMetricsService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        IViewTrackingService viewTrackingService,
        IAnalyticsErrorHandler errorHandler,
        ILogger<BusinessMetricsService> logger) : IBusinessMetricsService
    {
        // Core dependencies injected via constructor
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<BusinessMetricsService> _logger = logger;

        /// <summary>
        /// Records a business view event for analytics tracking.
        /// </summary>
        /// <param name="businessId">The unique identifier of the business being viewed</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method performs two key operations:
        /// 1. Logs the business view through the view tracking service
        /// 2. Records an analytics event for business view tracking
        /// 
        /// The view is recorded with default web platform and minimal tracking data
        /// to ensure privacy while maintaining analytics accuracy.
        /// </remarks>
        public async Task RecordBusinessViewAsync(int businessId)
        {
            await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Log the business view with minimal tracking data for privacy
                await _viewTrackingService.LogBusinessViewAsync(businessId, null, "Web", null, null, null, null);
                // Step 2: Record analytics event for business view tracking
                await _eventService.RecordBusinessViewEventAsync(businessId);
            }, string.Empty, "RecordBusinessView", new Dictionary<string, object> { ["BusinessId"] = businessId });
        }

        /// <summary>
        /// Retrieves comprehensive view statistics for a specific business within a date range.
        /// </summary>
        /// <param name="businessId">The unique identifier of the business</param>
        /// <param name="startDate">The start date for the statistics period</param>
        /// <param name="endDate">The end date for the statistics period</param>
        /// <param name="platform">Optional platform filter (e.g., "web", "mobile")</param>
        /// <returns>
        /// A <see cref="ViewStatistics"/> object containing comprehensive view metrics including
        /// total views, unique visitors, average daily views, peak day information, and platform breakdown
        /// </returns>
        /// <exception cref="AnalyticsValidationException">
        /// Thrown when the date range is invalid or platform parameter is invalid
        /// </exception>
        /// <remarks>
        /// This method calculates the following metrics:
        /// - Total views: Count of all view events
        /// - Unique visitors: Count of distinct IP addresses
        /// - Average views per day: Total views divided by date range
        /// - Peak day views: Maximum views in a single day
        /// - Peak day date: The date with the highest view count
        /// - Platform breakdown: Views grouped by platform type
        /// </remarks>
        public async Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Validate date range parameters
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

                // Step 2: Validate platform parameter if provided
                var (IsValid, ErrorMessage) = _validationService.ValidatePlatform(platform);
                if (!IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        ErrorMessage ?? "Invalid platform",
                        string.Empty,
                        "Platform",
                        "InvalidPlatform",
                        new Dictionary<string, object> { ["BusinessId"] = businessId, ["Platform"] = platform ?? string.Empty }
                    );
                    throw new AnalyticsValidationException(ErrorMessage ?? "Invalid platform", "Platform", "InvalidPlatform");
                }

                // Step 3: Retrieve view logs for the specified business and date range
                var viewLogs = await _dataService.GetBusinessViewLogsAsync(new List<int> { businessId }, startDate, endDate, platform);

                // Step 4: Calculate comprehensive view statistics
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

        /// <summary>
        /// Retrieves time-series view data for all businesses owned by a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="days">Number of days to retrieve data for (default: 30 days)</param>
        /// <returns>
        /// A list of <see cref="ViewsOverTimeData"/> objects containing daily view counts
        /// for the specified time period
        /// </returns>
        /// <exception cref="AnalyticsValidationException">
        /// Thrown when the days parameter is invalid
        /// </exception>
        /// <remarks>
        /// This method:
        /// 1. Validates the days parameter
        /// 2. Records analytics access for audit purposes
        /// 3. Retrieves all businesses owned by the user
        /// 4. Calculates daily view counts for the specified period
        /// 5. Returns empty list if user has no businesses
        /// </remarks>
        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Validate days parameter
                var (IsValid, ErrorMessage) = _validationService.ValidateAnalyticsDays(days);
                if (!IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        ErrorMessage ?? "Invalid days parameter",
                        userId,
                        "Days",
                        "InvalidDays",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(ErrorMessage ?? "Invalid days parameter", "Days", "InvalidDays");
                }

                // Step 2: Record analytics access event for audit trail
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsOverTime", new { Days = days });

                // Step 3: Retrieve user's businesses
                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (businesses.Count == 0) return new List<ViewsOverTimeData>();

                // Step 4: Calculate date range and retrieve view logs
                var businessIds = businesses.Select(b => b.Id).ToList();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, startDate, endDate);

                // Step 5: Process and return time-series data
                return ProcessViewsOverTimeData(viewLogs, startDate, endDate);
            }, userId, "GetViewsOverTime", new Dictionary<string, object> { ["Days"] = days });
        }

        /// <summary>
        /// Retrieves time-series view data filtered by platform for all businesses owned by a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="days">Number of days to retrieve data for (default: 30 days)</param>
        /// <param name="platform">Optional platform filter (e.g., "web", "mobile")</param>
        /// <returns>
        /// A list of <see cref="ViewsOverTimeData"/> objects containing daily view counts
        /// filtered by the specified platform
        /// </returns>
        /// <exception cref="AnalyticsValidationException">
        /// Thrown when the days parameter or platform parameter is invalid
        /// </exception>
        /// <remarks>
        /// This method provides platform-specific analytics by:
        /// 1. Validating both days and platform parameters
        /// 2. Recording analytics access for audit purposes
        /// 3. Filtering view data by the specified platform
        /// 4. Processing time-series data for the filtered results
        /// 
        /// Useful for understanding user behavior across different platforms.
        /// </remarks>
        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Validate days parameter
                var (IsValid, ErrorMessage) = _validationService.ValidateAnalyticsDays(days);
                if (!IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        ErrorMessage ?? "Invalid days parameter",
                        userId,
                        "Days",
                        "InvalidDays",
                        new Dictionary<string, object> { ["Days"] = days }
                    );
                    throw new AnalyticsValidationException(ErrorMessage ?? "Invalid days parameter", "Days", "InvalidDays");
                }

                // Step 2: Validate platform parameter
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

                // Step 3: Record analytics access event for audit trail
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ViewsOverTimeByPlatform", new { Days = days, Platform = platform });

                // Step 4: Retrieve user's businesses
                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return [];

                // Step 5: Calculate date range and retrieve platform-filtered view logs
                var businessIds = businesses.Select(b => b.Id).ToList();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, startDate, endDate, platform);

                // Step 6: Process and return time-series data
                return ProcessViewsOverTimeData(viewLogs, startDate, endDate);
            }, userId, "GetViewsOverTimeByPlatform", new Dictionary<string, object> { ["Days"] = days, ["Platform"] = platform ?? string.Empty });
        }

        /// <summary>
        /// Retrieves time-series review data for all businesses owned by a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="days">Number of days to retrieve data for (default: 30 days)</param>
        /// <returns>
        /// A list of <see cref="ReviewsOverTimeData"/> objects containing daily review counts
        /// and average ratings for the specified time period
        /// </returns>
        /// <exception cref="AnalyticsValidationException">
        /// Thrown when the days parameter is invalid
        /// </exception>
        /// <remarks>
        /// This method provides insights into review patterns by:
        /// 1. Validating the days parameter
        /// 2. Recording analytics access for audit purposes
        /// 3. Calculating daily review counts and average ratings
        /// 4. Providing comprehensive review metrics over time
        /// 
        /// Useful for understanding customer satisfaction trends and review patterns.
        /// </remarks>
        public async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Step 1: Validate days parameter
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

                // Step 2: Record analytics access event for audit trail
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ReviewsOverTime", new { Days = days });

                // Step 3: Retrieve user's businesses
                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return [];

                // Step 4: Calculate date range and retrieve review data
                var businessIds = businesses.Select(b => b.Id).ToList();
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var reviews = await _dataService.GetBusinessReviewsAsync(businessIds, startDate, endDate);

                // Step 5: Process and return time-series review data
                return ProcessReviewsOverTimeData(reviews, startDate, endDate);
            }, userId, "GetReviewsOverTime", new Dictionary<string, object> { ["Days"] = days });
        }

        // Private helper methods for data processing

        /// <summary>
        /// Processes raw view logs into time-series data with daily view counts.
        /// </summary>
        /// <param name="viewLogs">List of business view log entries</param>
        /// <param name="startDate">Start date for the time series</param>
        /// <param name="endDate">End date for the time series</param>
        /// <returns>List of daily view data points</returns>
        /// <remarks>
        /// This method ensures complete time-series data by:
        /// - Iterating through each day in the date range
        /// - Counting views for each specific day
        /// - Including days with zero views to maintain data continuity
        /// - Returning data in chronological order
        /// </remarks>
        private static List<ViewsOverTimeData> ProcessViewsOverTimeData(List<BusinessViewLog> viewLogs, DateTime startDate, DateTime endDate)
        {
            var result = new List<ViewsOverTimeData>();
            var currentDate = startDate.Date;

            // Iterate through each day in the date range to ensure complete time series
            while (currentDate <= endDate.Date)
            {
                // Count views for the current day
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

        /// <summary>
        /// Processes raw review data into time-series data with daily review counts and average ratings.
        /// </summary>
        /// <param name="reviews">List of business review entries</param>
        /// <param name="startDate">Start date for the time series</param>
        /// <param name="endDate">End date for the time series</param>
        /// <returns>List of daily review data points with counts and average ratings</returns>
        /// <remarks>
        /// This method calculates comprehensive review metrics by:
        /// - Iterating through each day in the date range
        /// - Counting reviews for each specific day
        /// - Calculating average rating for days with reviews
        /// - Setting average rating to 0 for days without reviews
        /// - Including days with zero reviews to maintain data continuity
        /// </remarks>
        private static List<ReviewsOverTimeData> ProcessReviewsOverTimeData(List<BusinessReview> reviews, DateTime startDate, DateTime endDate)
        {
            var result = new List<ReviewsOverTimeData>();
            var currentDate = startDate.Date;

            // Iterate through each day in the date range to ensure complete time series
            while (currentDate <= endDate.Date)
            {
                // Filter reviews for the current day
                var dayReviews = reviews.Where(r => r.CreatedAt.Date == currentDate).ToList();
                var reviewCount = dayReviews.Count;
                // Calculate average rating for the day (0 if no reviews)
                var averageRating = dayReviews.Any() ? dayReviews.Average(r => r.Rating) : 0;
                
                result.Add(new ReviewsOverTimeData
                {
                    Date = currentDate,
                    Reviews = reviewCount,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating
                });
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }
    }
}
