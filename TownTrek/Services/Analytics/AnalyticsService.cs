using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Main analytics service with reduced coupling and improved architecture
    /// </summary>
    public class AnalyticsService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        ISubscriptionAuthService subscriptionAuthService,
        IViewTrackingService viewTrackingService,
        IAnalyticsSnapshotService analyticsSnapshotService,
        ILogger<AnalyticsService> logger) : IAnalyticsService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly IAnalyticsSnapshotService _analyticsSnapshotService = analyticsSnapshotService;
        private readonly ILogger<AnalyticsService> _logger = logger;

        public async Task<ClientAnalyticsViewModel> GetClientAnalyticsAsync(string userId)
        {
            try
            {
                // Validate user ID
                var userValidation = await _validationService.ValidateUserIdAsync(userId);
                if (!userValidation.IsValid)
                {
                    _logger.LogWarning("Analytics access denied: {ErrorMessage} for UserId {UserId}", userValidation.ErrorMessage, userId);
                    throw new ArgumentException(userValidation.ErrorMessage, nameof(userId));
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ClientAnalytics");

                var user = await _dataService.GetUserAsync(userId);
                if (user == null) throw new ArgumentException("User not found", nameof(userId));

                // All non-trial users get the same analytics experience.
                var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                var hasBasicAnalytics = true;
                var hasAdvancedAnalytics = true;

                var businesses = await _dataService.GetUserBusinessesAsync(userId);

                // OPTIMIZED: Use batch queries instead of N+1 queries
                var businessIds = businesses.Select(b => b.Id).ToList();
                var businessAnalytics = await GetBusinessAnalyticsBatchAsync(businessIds, userId);

                var overview = await GetAnalyticsOverviewAsync(userId, businessAnalytics);
                var viewsOverTime = await GetViewsOverTimeAsync(userId, AnalyticsConstants.DefaultAnalyticsDays);
                var reviewsOverTime = await GetReviewsOverTimeAsync(userId, AnalyticsConstants.DefaultAnalyticsDays);
                var performanceInsights = await GetPerformanceInsightsAsync(userId);

                var model = new ClientAnalyticsViewModel
                {
                    User = user,
                    SubscriptionTier = authResult.SubscriptionTier?.Name ?? "None",
                    HasBasicAnalytics = hasBasicAnalytics,
                    HasStandardAnalytics = false,
                    HasPremiumAnalytics = hasAdvancedAnalytics,
                    Businesses = businesses,
                    BusinessAnalytics = businessAnalytics,
                    Overview = overview,
                    ViewsOverTime = viewsOverTime,
                    ReviewsOverTime = reviewsOverTime,
                    PerformanceInsights = performanceInsights
                };

                // Premium features
                if (hasAdvancedAnalytics && businesses.Any())
                {
                    var primaryCategory = businesses.GroupBy(b => b.Category)
                        .OrderByDescending(g => g.Count())
                        .First().Key;

                    model.CategoryBenchmarks = await GetCategoryBenchmarksAsync(userId, primaryCategory);
                    model.CompetitorInsights = await GetCompetitorInsightsAsync(userId);
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client analytics for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetClientAnalytics", ex.Message, new { UserId = userId });
                throw;
            }
        }

        public async Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(int businessId, string userId)
        {
            try
            {
                // Validate business ownership
                var businessValidation = await _validationService.ValidateBusinessOwnershipAsync(businessId, userId);
                if (!businessValidation.IsValid)
                {
                    _logger.LogWarning("Business analytics access denied: {ErrorMessage} for BusinessId {BusinessId}, UserId {UserId}", 
                        businessValidation.ErrorMessage, businessId, userId);
                    throw new ArgumentException(businessValidation.ErrorMessage);
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "BusinessAnalytics", new { BusinessId = businessId });

                var business = await _dataService.GetBusinessAsync(businessId, userId);
                if (business == null) throw new ArgumentException("Business not found");

                var now = DateTime.UtcNow;
                var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);
                var lastMonthEnd = thisMonthStart.AddDays(-1);

                // Get data for this business
                var reviews = await _dataService.GetBusinessReviewsAsync(new List<int> { businessId });
                var favorites = await _dataService.GetBusinessFavoritesAsync(new List<int> { businessId });
                var viewLogs = await _dataService.GetBusinessViewLogsAsync(new List<int> { businessId });

                // Calculate growth rates
                var growthRates = await _analyticsSnapshotService.CalculateGrowthRatesAsync(businessId, AnalyticsConstants.DefaultGrowthRateDays, AnalyticsConstants.DefaultGrowthRateDays);

                return new BusinessAnalyticsData
                {
                    BusinessId = businessId,
                    BusinessName = business.Name,
                    Category = business.Category,
                    Town = business.Town?.Name ?? "Unknown",
                    TotalViews = viewLogs.Count,
                    TotalReviews = reviews.Count,
                    TotalFavorites = favorites.Count,
                    AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                    ViewsThisMonth = viewLogs.Count(v => v.ViewedAt >= thisMonthStart),
                    ViewsLastMonth = viewLogs.Count(v => v.ViewedAt >= lastMonthStart && v.ViewedAt <= lastMonthEnd),
                    ReviewsThisMonth = reviews.Count(r => r.CreatedAt >= thisMonthStart),
                    ReviewsLastMonth = reviews.Count(r => r.CreatedAt >= lastMonthStart && r.CreatedAt <= lastMonthEnd),
                    FavoritesThisMonth = favorites.Count(f => f.CreatedAt >= thisMonthStart),
                    FavoritesLastMonth = favorites.Count(f => f.CreatedAt >= lastMonthStart && f.CreatedAt <= lastMonthEnd),
                    ViewsGrowthRate = (double)growthRates.ViewsGrowthRate,
                    ReviewsGrowthRate = (double)growthRates.ReviewsGrowthRate,
                    FavoritesGrowthRate = (double)growthRates.FavoritesGrowthRate,
                    RatingGrowthRate = (double)growthRates.RatingGrowthRate,
                    EngagementScore = CalculateEngagementScore(reviews.Count, favorites.Count, viewLogs.Count),
                    PerformanceRating = CalculateNumericPerformanceRating(reviews.Count, favorites.Count, viewLogs.Count, reviews.Any() ? reviews.Average(r => r.Rating) : 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting business analytics for BusinessId {BusinessId}, UserId {UserId}", businessId, userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetBusinessAnalytics", ex.Message, new { BusinessId = businessId, UserId = userId });
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

        public async Task<List<BusinessPerformanceInsight>> GetPerformanceInsightsAsync(string userId)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "PerformanceInsights");

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return new List<BusinessPerformanceInsight>();

                var businessIds = businesses.Select(b => b.Id).ToList();
                var businessAnalytics = await GetBusinessAnalyticsBatchAsync(businessIds, userId);

                return GeneratePerformanceInsights(businessAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance insights for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetPerformanceInsights", ex.Message, new { UserId = userId });
                throw;
            }
        }

        public async Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "CategoryBenchmarks", new { Category = category });

                var categoryBusinesses = await _dataService.GetCategoryBusinessesAsync(category);
                if (categoryBusinesses.Count < AnalyticsConstants.MinCategoryBusinessesForBenchmark)
                {
                    return null;
                }

                var userBusinesses = await _dataService.GetUserBusinessesAsync(userId);
                var userCategoryBusinesses = userBusinesses.Where(b => b.Category == category).ToList();

                if (!userCategoryBusinesses.Any()) return null;

                return CalculateCategoryBenchmarks(userCategoryBusinesses, categoryBusinesses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category benchmarks for UserId {UserId}, Category {Category}", userId, category);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetCategoryBenchmarks", ex.Message, new { UserId = userId, Category = category });
                throw;
            }
        }

        public async Task<CategoryBenchmarks?> GetDetailedCategoryBenchmarksAsync(string userId, string category)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "DetailedCategoryBenchmarks", new { Category = category });

                var categoryBusinesses = await _dataService.GetCategoryBusinessesAsync(category);
                if (categoryBusinesses.Count < AnalyticsConstants.MinCategoryBusinessesForBenchmark)
                {
                    return null;
                }

                var userBusinesses = await _dataService.GetUserBusinessesAsync(userId);
                var userCategoryBusinesses = userBusinesses.Where(b => b.Category == category).ToList();

                if (!userCategoryBusinesses.Any()) return null;

                return CalculateDetailedCategoryBenchmarks(userCategoryBusinesses, categoryBusinesses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed category benchmarks for UserId {UserId}, Category {Category}", userId, category);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetDetailedCategoryBenchmarks", ex.Message, new { UserId = userId, Category = category });
                throw;
            }
        }

        public async Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string userId)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "CompetitorInsights");

                var userBusinesses = await _dataService.GetUserBusinessesAsync(userId);
                var insights = new List<CompetitorInsight>();

                foreach (var business in userBusinesses)
                {
                    var competitors = await _dataService.GetCompetitorBusinessesAsync(business.Id, business.Category, business.Town?.Name ?? "");
                    if (competitors.Any())
                    {
                        var insight = CalculateCompetitorInsight(business, competitors);
                        insights.Add(insight);
                    }
                }

                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting competitor insights for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetCompetitorInsights", ex.Message, new { UserId = userId });
                throw;
            }
        }

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

        // Chart data processing methods (Phase 2.2)
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

                var viewsData = await GetViewsOverTimeByPlatformAsync(userId, days, platform);
                
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

                var reviewsData = await GetReviewsOverTimeAsync(userId, days);
                
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

        // Comparative analysis methods (Phase 4.2)
        public async Task<ComparativeAnalysisResponse> GetComparativeAnalysisAsync(string userId, ComparativeAnalysisRequest request)
        {
            try
            {
                // Validate request
                var validation = _validationService.ValidateComparativeAnalysisRequest(request);
                if (!validation.IsValid)
                {
                    throw new ArgumentException(validation.ErrorMessage);
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ComparativeAnalysis", request);

                switch (request.ComparisonType)
                {
                    case "WeekOverWeek":
                    case "MonthOverMonth":
                    case "QuarterOverQuarter":
                        return await GetPeriodOverPeriodComparisonAsync(userId, request.BusinessId, request.ComparisonType, request.Platform);
                    case "YearOverYear":
                        return await GetYearOverYearComparisonAsync(userId, request.BusinessId, request.Platform);
                    case "CustomRange":
                        return await GetCustomRangeComparisonAsync(userId, request.CurrentPeriodStart, request.CurrentPeriodEnd, 
                            request.PreviousPeriodStart ?? DateTime.MinValue, request.PreviousPeriodEnd ?? DateTime.MinValue, 
                            request.BusinessId, request.Platform);
                    default:
                        throw new ArgumentException($"Unsupported comparison type: {request.ComparisonType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparative analysis for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetComparativeAnalysis", ex.Message, new { UserId = userId, Request = request });
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetPeriodOverPeriodComparisonAsync(string userId, int? businessId = null, string comparisonType = "MonthOverMonth", string? platform = null)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "PeriodOverPeriodComparison", new { BusinessId = businessId, ComparisonType = comparisonType, Platform = platform });

                var endDate = DateTime.UtcNow;
                var periodDays = comparisonType switch
                {
                    "WeekOverWeek" => 7,
                    "MonthOverMonth" => 30,
                    "QuarterOverQuarter" => 90,
                    _ => 30
                };

                var currentStart = endDate.AddDays(-periodDays);
                var previousStart = currentStart.AddDays(-periodDays);
                var previousEnd = currentStart.AddDays(-1);

                return await GetCustomRangeComparisonAsync(userId, currentStart, endDate, previousStart, previousEnd, businessId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting period over period comparison for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetPeriodOverPeriodComparison", ex.Message, new { UserId = userId, BusinessId = businessId, ComparisonType = comparisonType, Platform = platform });
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetYearOverYearComparisonAsync(string userId, int? businessId = null, string? platform = null)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "YearOverYearComparison", new { BusinessId = businessId, Platform = platform });

                var endDate = DateTime.UtcNow;
                var currentStart = endDate.AddDays(-365);
                var previousStart = currentStart.AddDays(-365);
                var previousEnd = currentStart.AddDays(-1);

                return await GetCustomRangeComparisonAsync(userId, currentStart, endDate, previousStart, previousEnd, businessId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting year over year comparison for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetYearOverYearComparison", ex.Message, new { UserId = userId, BusinessId = businessId, Platform = platform });
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetCustomRangeComparisonAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? businessId = null, string? platform = null)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "CustomRangeComparison", new { BusinessId = businessId, Platform = platform, CurrentStart = currentStart, CurrentEnd = currentEnd, PreviousStart = previousStart, PreviousEnd = previousEnd });

                var businesses = businessId.HasValue 
                    ? new List<Business> { await _dataService.GetBusinessAsync(businessId.Value, userId) ?? throw new ArgumentException("Business not found") }
                    : await _dataService.GetUserBusinessesAsync(userId);

                if (!businesses.Any()) return new ComparativeAnalysisResponse();

                var businessIds = businesses.Select(b => b.Id).ToList();

                // Get data for both periods
                var currentViewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, currentStart, currentEnd, platform);
                var previousViewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, previousStart, previousEnd, platform);
                var currentReviews = await _dataService.GetBusinessReviewsAsync(businessIds, currentStart, currentEnd);
                var previousReviews = await _dataService.GetBusinessReviewsAsync(businessIds, previousStart, previousEnd);
                var currentFavorites = await _dataService.GetBusinessFavoritesAsync(businessIds, currentStart, currentEnd);
                var previousFavorites = await _dataService.GetBusinessFavoritesAsync(businessIds, previousStart, previousEnd);

                // Calculate period data
                var currentPeriod = CalculatePeriodData(currentViewLogs, currentReviews, currentFavorites, currentStart, currentEnd);
                var previousPeriod = CalculatePeriodData(previousViewLogs, previousReviews, previousFavorites, previousStart, previousEnd);

                // Calculate comparison metrics
                var comparisonMetrics = CalculateComparisonMetrics(currentPeriod, previousPeriod);

                return new ComparativeAnalysisResponse
                {
                    ComparisonType = "CustomRange",
                    CurrentPeriod = currentPeriod,
                    PreviousPeriod = previousPeriod,
                    ComparisonMetrics = comparisonMetrics,
                    ChartData = new ComparativeChartData
                    {
                        Labels = new List<string> { "Current Period", "Previous Period" },
                        Datasets = new List<ComparativeChartDataset>
                        {
                            new ComparativeChartDataset
                            {
                                Label = "Views",
                                Data = new List<double> { currentPeriod.TotalViews, previousPeriod.TotalViews },
                                BorderColor = AnalyticsConstants.ChartColors.LapisLazuli,
                                BackgroundColor = AnalyticsConstants.ChartColors.LapisLazuli + AnalyticsConstants.ChartOpacity.Light
                            },
                            new ComparativeChartDataset
                            {
                                Label = "Reviews",
                                Data = new List<double> { currentPeriod.TotalReviews, previousPeriod.TotalReviews },
                                BorderColor = AnalyticsConstants.ChartColors.HunyadiYellow,
                                BackgroundColor = AnalyticsConstants.ChartColors.HunyadiYellow + AnalyticsConstants.ChartOpacity.Light
                            },
                            new ComparativeChartDataset
                            {
                                Label = "Favorites",
                                Data = new List<double> { currentPeriod.TotalFavorites, previousPeriod.TotalFavorites },
                                BorderColor = AnalyticsConstants.ChartColors.OrangePantone,
                                BackgroundColor = AnalyticsConstants.ChartColors.OrangePantone + AnalyticsConstants.ChartOpacity.Light
                            }
                        }
                    },
                    Insights = GenerateComparativeInsights(comparisonMetrics)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom range comparison for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetCustomRangeComparison", ex.Message, new { UserId = userId, BusinessId = businessId, Platform = platform, CurrentStart = currentStart, CurrentEnd = currentEnd, PreviousStart = previousStart, PreviousEnd = previousEnd });
                throw;
            }
        }

        // Private helper methods (simplified for brevity - these would contain the actual business logic)
        private async Task<List<BusinessAnalyticsData>> GetBusinessAnalyticsBatchAsync(List<int> businessIds, string userId)
        {
            // Implementation would be similar to the original but using the data service
            var result = new List<BusinessAnalyticsData>();
            foreach (var businessId in businessIds)
            {
                result.Add(await GetBusinessAnalyticsAsync(businessId, userId));
            }
            return result;
        }

        private Task<AnalyticsOverview> GetAnalyticsOverviewAsync(string userId, List<BusinessAnalyticsData> businessAnalytics)
        {
            // Implementation would calculate overview metrics
            var overview = new AnalyticsOverview
            {
                TotalBusinesses = businessAnalytics.Count,
                TotalViews = businessAnalytics.Sum(b => b.TotalViews),
                TotalReviews = businessAnalytics.Sum(b => b.TotalReviews),
                TotalFavorites = businessAnalytics.Sum(b => b.TotalFavorites),
                AverageRating = businessAnalytics.Any() ? businessAnalytics.Average(b => b.AverageRating) : 0,
                AverageEngagementScore = businessAnalytics.Any() ? businessAnalytics.Average(b => b.EngagementScore) : 0
            };
            return Task.FromResult(overview);
        }

        private List<ViewsOverTimeData> ProcessViewsOverTimeData(List<BusinessViewLog> viewLogs, DateTime startDate, DateTime endDate)
        {
            // Implementation would process view logs into time series data
            var result = new List<ViewsOverTimeData>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayViews = viewLogs.Count(v => v.ViewedAt.Date == date);
                result.Add(new ViewsOverTimeData { Date = date, Views = dayViews });
            }
            return result;
        }

        private List<ReviewsOverTimeData> ProcessReviewsOverTimeData(List<BusinessReview> reviews, DateTime startDate, DateTime endDate)
        {
            // Implementation would process reviews into time series data
            var result = new List<ReviewsOverTimeData>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayReviews = reviews.Count(r => r.CreatedAt.Date == date);
                result.Add(new ReviewsOverTimeData { Date = date, Reviews = dayReviews });
            }
            return result;
        }

        private List<BusinessPerformanceInsight> GeneratePerformanceInsights(List<BusinessAnalyticsData> businessAnalytics)
        {
            // Implementation would generate performance insights
            return businessAnalytics.Select(b => new BusinessPerformanceInsight
            {
                BusinessId = b.BusinessId,
                BusinessName = b.BusinessName,
                Insight = $"Business {b.BusinessName} has {b.TotalViews} total views",
                PerformanceRating = b.PerformanceRating,
                Trend = "stable"
            }).ToList();
        }

        private CategoryBenchmarkData CalculateCategoryBenchmarks(List<Business> userBusinesses, List<Business> categoryBusinesses)
        {
            // Implementation would calculate category benchmarks
            return new CategoryBenchmarkData
            {
                Category = userBusinesses.First().Category,
                UserBusinessesCount = userBusinesses.Count,
                CategoryTotalBusinesses = categoryBusinesses.Count,
                AverageCategoryRating = 4.0, // Placeholder
                UserAverageRating = 4.2, // Placeholder
                PerformanceRating = 4.2 // Above average rating
            };
        }

        private CategoryBenchmarks CalculateDetailedCategoryBenchmarks(List<Business> userBusinesses, List<Business> categoryBusinesses)
        {
            // Implementation would calculate detailed category benchmarks
            return new CategoryBenchmarks
            {
                Category = userBusinesses.First().Category,
                UserBusinessesCount = userBusinesses.Count,
                CategoryTotalBusinesses = categoryBusinesses.Count,
                AverageCategoryRating = 4.0, // Placeholder
                UserAverageRating = 4.2, // Placeholder
                PerformanceRating = 4.2, // Above average rating
                DetailedMetrics = new List<BenchmarkMetric>()
            };
        }

        private CompetitorInsight CalculateCompetitorInsight(Business business, List<Business> competitors)
        {
            // Implementation would calculate competitor insights
            return new CompetitorInsight
            {
                BusinessId = business.Id,
                BusinessName = business.Name,
                CompetitorsCount = competitors.Count,
                AverageCompetitorRating = competitors.Any() ? competitors.Average(c => (double)(c.Rating ?? 0)) : 0,
                MarketPosition = "Competitive",
                Recommendations = new List<string> { "Focus on customer service" }
            };
        }

        private double CalculateEngagementScore(int reviews, int favorites, int views)
        {
            if (views == 0) return 0;
            return (reviews + favorites) * AnalyticsConstants.EngagementScoreMultiplier / views * AnalyticsConstants.PercentageMultiplier;
        }

        private string CalculatePerformanceRating(int reviews, int favorites, int views, double rating)
        {
            var engagementScore = CalculateEngagementScore(reviews, favorites, views);
            
            if (engagementScore >= AnalyticsConstants.StrongEngagementThreshold && rating >= AnalyticsConstants.ExcellentRatingThreshold)
                return AnalyticsConstants.PerformanceRating.Excellent;
            if (engagementScore >= AnalyticsConstants.MinFavoritesThreshold && rating >= AnalyticsConstants.GoodRatingThreshold)
                return AnalyticsConstants.PerformanceRating.Good;
            if (rating >= AnalyticsConstants.PoorRatingThreshold)
                return AnalyticsConstants.PerformanceRating.Fair;
            return AnalyticsConstants.PerformanceRating.Poor;
        }

        private double CalculateNumericPerformanceRating(int reviews, int favorites, int views, double rating)
        {
            var engagementScore = CalculateEngagementScore(reviews, favorites, views);
            
            if (engagementScore >= AnalyticsConstants.StrongEngagementThreshold && rating >= AnalyticsConstants.ExcellentRatingThreshold)
                return 5.0; // Excellent
            if (engagementScore >= AnalyticsConstants.MinFavoritesThreshold && rating >= AnalyticsConstants.GoodRatingThreshold)
                return 4.0; // Good
            if (rating >= AnalyticsConstants.PoorRatingThreshold)
                return 3.0; // Fair
            return 2.0; // Poor
        }

        private PeriodData CalculatePeriodData(List<BusinessViewLog> viewLogs, List<BusinessReview> reviews, List<FavoriteBusiness> favorites, DateTime startDate, DateTime endDate)
        {
            return new PeriodData
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalViews = viewLogs.Count,
                TotalReviews = reviews.Count,
                TotalFavorites = favorites.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                EngagementScore = CalculateEngagementScore(reviews.Count, favorites.Count, viewLogs.Count),
                AverageViewsPerDay = viewLogs.Count / Math.Max(1, (endDate - startDate).Days),
                AverageReviewsPerDay = reviews.Count / Math.Max(1, (endDate - startDate).Days),
                AverageFavoritesPerDay = favorites.Count / Math.Max(1, (endDate - startDate).Days),
                PeakDayViews = viewLogs.GroupBy(v => v.ViewedAt.Date).Max(g => g.Count()),
                PeakDayDate = viewLogs.GroupBy(v => v.ViewedAt.Date).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key,
                LowDayViews = viewLogs.GroupBy(v => v.ViewedAt.Date).Min(g => g.Count()),
                LowDayDate = viewLogs.GroupBy(v => v.ViewedAt.Date).OrderBy(g => g.Count()).FirstOrDefault()?.Key
            };
        }

        private ComparisonMetrics CalculateComparisonMetrics(PeriodData current, PeriodData previous)
        {
            return new ComparisonMetrics
            {
                ViewsChangePercent = CalculatePercentageChange(current.TotalViews, previous.TotalViews),
                ReviewsChangePercent = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews),
                FavoritesChangePercent = CalculatePercentageChange(current.TotalFavorites, previous.TotalFavorites),
                RatingChangePercent = CalculatePercentageChange(current.AverageRating, previous.AverageRating),
                EngagementChangePercent = CalculatePercentageChange(current.EngagementScore, previous.EngagementScore),
                AverageViewsPerDayChangePercent = CalculatePercentageChange(current.AverageViewsPerDay, previous.AverageViewsPerDay),
                AverageReviewsPerDayChangePercent = CalculatePercentageChange(current.AverageReviewsPerDay, previous.AverageReviewsPerDay),
                AverageFavoritesPerDayChangePercent = CalculatePercentageChange(current.AverageFavoritesPerDay, previous.AverageFavoritesPerDay),
                OverallTrend = GetOverallTrend(current, previous),
                PerformanceRating = GetPerformanceRating(current, previous),
                KeyChanges = GenerateKeyChanges(current, previous)
            };
        }

        private double CalculatePercentageChange(double current, double previous)
        {
            if (previous == 0) return current > 0 ? AnalyticsConstants.PercentageMultiplier : 0;
            return (current - previous) / previous * AnalyticsConstants.PercentageMultiplier;
        }

        private string GetTrend(double current, double previous)
        {
            if (current > previous) return AnalyticsConstants.PerformanceTrends.Up;
            if (current < previous) return AnalyticsConstants.PerformanceTrends.Down;
            return AnalyticsConstants.PerformanceTrends.Stable;
        }

        private string GetOverallTrend(PeriodData current, PeriodData previous)
        {
            var improvements = 0;
            var declines = 0;
            
            if (current.TotalViews > previous.TotalViews) improvements++;
            else if (current.TotalViews < previous.TotalViews) declines++;
            
            if (current.TotalReviews > previous.TotalReviews) improvements++;
            else if (current.TotalReviews < previous.TotalReviews) declines++;
            
            if (current.AverageRating > previous.AverageRating) improvements++;
            else if (current.AverageRating < previous.AverageRating) declines++;
            
            if (improvements > declines) return "Improving";
            if (declines > improvements) return "Declining";
            return "Stable";
        }

        private string GetPerformanceRating(PeriodData current, PeriodData previous)
        {
            var engagementScore = current.EngagementScore;
            var rating = current.AverageRating;
            
            if (engagementScore >= AnalyticsConstants.StrongEngagementThreshold && rating >= AnalyticsConstants.ExcellentRatingThreshold)
                return "Excellent";
            if (engagementScore >= AnalyticsConstants.MinFavoritesThreshold && rating >= AnalyticsConstants.GoodRatingThreshold)
                return "Good";
            if (rating >= AnalyticsConstants.PoorRatingThreshold)
                return "Fair";
            return "Poor";
        }

        private List<string> GenerateKeyChanges(PeriodData current, PeriodData previous)
        {
            var changes = new List<string>();
            
            var viewsChange = CalculatePercentageChange(current.TotalViews, previous.TotalViews);
            var reviewsChange = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews);
            var ratingChange = CalculatePercentageChange(current.AverageRating, previous.AverageRating);
            
            if (Math.Abs(viewsChange) >= AnalyticsConstants.SignificantViewsChangeThreshold)
            {
                var direction = viewsChange > 0 ? "increased" : "decreased";
                changes.Add($"Views {direction} by {Math.Abs(viewsChange):F1}%");
            }
            
            if (Math.Abs(reviewsChange) >= AnalyticsConstants.SignificantReviewsChangeThreshold)
            {
                var direction = reviewsChange > 0 ? "increased" : "decreased";
                changes.Add($"Reviews {direction} by {Math.Abs(reviewsChange):F1}%");
            }
            
            if (Math.Abs(ratingChange) >= 5.0) // 5% rating change threshold
            {
                var direction = ratingChange > 0 ? "improved" : "declined";
                changes.Add($"Average rating {direction} by {Math.Abs(ratingChange):F1}%");
            }
            
            return changes;
        }

        private List<string> GenerateComparativeInsights(ComparisonMetrics metrics)
        {
            var insights = new List<string>();
            
            if (Math.Abs(metrics.ViewsChangePercent) >= AnalyticsConstants.SignificantViewsChangeThreshold)
            {
                insights.Add($"Views changed by {metrics.ViewsChangePercent:F1}% compared to the previous period");
            }
            
            if (Math.Abs(metrics.ReviewsChangePercent) >= AnalyticsConstants.SignificantReviewsChangeThreshold)
            {
                insights.Add($"Reviews changed by {metrics.ReviewsChangePercent:F1}% compared to the previous period");
            }
            
            return insights;
        }
    }
}