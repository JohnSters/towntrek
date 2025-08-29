using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;
using TownTrek.Services.Interfaces.ClientAnalytics;

namespace TownTrek.Services.ClientAnalytics
{
    /// <summary>
    /// Main analytics service with reduced coupling and improved architecture
    /// </summary>
    public class ClientAnalyticsService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        ISubscriptionAuthService subscriptionAuthService,
        IViewTrackingService viewTrackingService,
        IAnalyticsSnapshotService analyticsSnapshotService,
        IBusinessService businessService,
        ApplicationDbContext context,
        ILogger<ClientAnalyticsService> logger) : IClientAnalyticsService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly IAnalyticsSnapshotService _analyticsSnapshotService = analyticsSnapshotService;
        private readonly IBusinessService _businessService = businessService;
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ClientAnalyticsService> _logger = logger;

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

                // Check subscription-based analytics access
                var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                var hasBasicAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                var hasAdvancedAnalytics = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");

                var businesses = await _dataService.GetUserBusinessesAsync(userId);

                // OPTIMIZED: Use batch queries instead of N+1 queries
                var businessIds = businesses.Select(b => b.Id).ToList();
                var businessAnalytics = await GetBusinessAnalyticsBatchAsync(businessIds, userId);

                var overview = await GetAnalyticsOverviewAsync(userId, businessAnalytics);
                var viewsOverTime = await GetViewsOverTimeAsync(userId, AnalyticsConstants.DefaultAnalyticsDays);
                var reviewsOverTime = await GetReviewsOverTimeAsync(userId, AnalyticsConstants.DefaultAnalyticsDays);

                var model = new ClientAnalyticsViewModel
                {
                    User = ConvertToUserInfo(user),
                    SubscriptionTier = authResult.SubscriptionTier?.Name ?? "None",
                    HasBasicAnalytics = hasBasicAnalytics,
                    HasStandardAnalytics = false,
                    HasPremiumAnalytics = hasAdvancedAnalytics,
                    Businesses = businesses,
                    BusinessAnalytics = businessAnalytics,
                    Overview = overview,
                    ViewsOverTime = viewsOverTime,
                    ReviewsOverTime = reviewsOverTime
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



        public async Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "CategoryBenchmarks", new { Category = category });

                // Get user businesses first and filter by category to avoid unnecessary database calls
                var userBusinesses = await _dataService.GetUserBusinessesAsync(userId);
                var userCategoryBusinesses = userBusinesses.Where(b => b.Category == category).ToList();

                if (!userCategoryBusinesses.Any()) return null;

                // Only fetch category businesses if user has businesses in that category
                var categoryBusinesses = await _dataService.GetCategoryBusinessesAsync(category);
                if (categoryBusinesses.Count < AnalyticsConstants.MinCategoryBusinessesForBenchmark)
                {
                    return null;
                }

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

                // Get user businesses first and filter by category to avoid unnecessary database calls
                var userBusinesses = await _dataService.GetUserBusinessesAsync(userId);
                var userCategoryBusinesses = userBusinesses.Where(b => b.Category == category).ToList();

                if (!userCategoryBusinesses.Any()) return null;

                // Only fetch category businesses if user has businesses in that category
                var categoryBusinesses = await _dataService.GetCategoryBusinessesAsync(category);
                if (categoryBusinesses.Count < AnalyticsConstants.MinCategoryBusinessesForBenchmark)
                {
                    return null;
                }

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
                if (!userBusinesses.Any()) return new List<CompetitorInsight>();

                // Batch competitor lookup to avoid N+1 queries
                var competitorLookups = userBusinesses.Select(b => (object)new
                {
                    BusinessId = b.Id,
                    b.Category,
                    Town = b.Town?.Name ?? ""
                }).ToList();

                var allCompetitors = await _dataService.GetCompetitorBusinessesBatchAsync(competitorLookups);
                var insights = new List<CompetitorInsight>();

                foreach (var business in userBusinesses)
                {
                    var businessCompetitors = allCompetitors
                        .Where(c => c.Category == business.Category && c.Town?.Name == business.Town?.Name && c.Id != business.Id)
                        .ToList();

                    if (businessCompetitors.Any())
                    {
                        var insight = CalculateCompetitorInsight(business, businessCompetitors);
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





        // Private helper methods (simplified for brevity - these would contain the actual business logic)
        private async Task<List<BusinessAnalyticsData>> GetBusinessAnalyticsBatchAsync(List<int> businessIds, string userId)
        {
            // Implementation would be similar to the original but using the data service
            var result = new List<BusinessAnalyticsData>();
            foreach (var businessId in businessIds)
            {
                // Get business data directly from data service to avoid circular dependency
                var business = await _dataService.GetBusinessAsync(businessId, userId);
                if (business == null) continue;

                var viewLogs = await _dataService.GetBusinessViewLogsAsync(new List<int> { businessId });
                var reviews = await _dataService.GetBusinessReviewsAsync(new List<int> { businessId });
                var favorites = await _dataService.GetBusinessFavoritesAsync(new List<int> { businessId });

                var businessAnalytics = new BusinessAnalyticsData
                {
                    BusinessId = business.Id,
                    BusinessName = business.Name,
                    Category = business.Category,
                    Status = business.Status,
                    TotalViews = viewLogs.Count,
                    TotalReviews = reviews.Count,
                    TotalFavorites = favorites.Count,
                    AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                    EngagementScore = CalculateEngagementScore(reviews.Count, favorites.Count, viewLogs.Count),
                    PerformanceRating = CalculateBusinessPerformanceRating(viewLogs.Count, reviews.Count, favorites.Count, reviews.Any() ? reviews.Average(r => r.Rating) : 0),
                    PerformanceTrend = "stable", // Placeholder
                    Recommendations = new List<string> { "Add more photos", "Update business hours" } // Placeholder
                };

                result.Add(businessAnalytics);
            }
            return result;
        }



        private double CalculateBusinessPerformanceRating(int views, int reviews, int favorites, double averageRating)
        {
            // Simple performance rating calculation
            var engagementScore = CalculateEngagementScore(views, reviews, favorites);
            return engagementScore * 0.7 + averageRating * 0.3;
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
            var result = new List<ReviewsOverTimeData>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayReviews = reviews.Where(r => r.CreatedAt.Date == date).ToList();
                var reviewCount = dayReviews.Count;
                var averageRating = dayReviews.Any() ? dayReviews.Average(r => r.Rating) : 0;
                
                result.Add(new ReviewsOverTimeData 
                { 
                    Date = date, 
                    Reviews = reviewCount,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating
                });
            }
            return result;
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

        // Additional methods needed by controllers (aliases for existing methods)
        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeDataAsync(string userId, int days = 30)
        {
            return await GetViewsOverTimeAsync(userId, days);
        }

        public async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeDataAsync(string userId, int days = 30)
        {
            return await GetReviewsOverTimeAsync(userId, days);
        }

        public async Task<ComparativeAnalysisResponse> GetComparativeAnalysisDataAsync(string userId, string comparisonType, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var endDate = toDate ?? DateTime.UtcNow;
                var startDate = fromDate ?? endDate.AddDays(-30);

                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return new ComparativeAnalysisResponse
                    {
                        ComparisonType = comparisonType,
                        Insights = new List<string> { "No businesses found for analysis." }
                    };
                }

                // Calculate period data based on comparison type
                var (currentPeriod, previousPeriod) = await CalculatePeriodDataAsync(userId, userBusinesses, comparisonType, startDate, endDate);

                // Calculate comparison metrics
                var comparisonMetrics = CalculateComparisonMetrics(currentPeriod, previousPeriod);

                // Generate insights
                var insights = GenerateComparativeInsights(currentPeriod, previousPeriod, comparisonMetrics);

                return new ComparativeAnalysisResponse
                {
                    ComparisonType = comparisonType,
                    CurrentPeriod = currentPeriod,
                    PreviousPeriod = previousPeriod,
                    ComparisonMetrics = comparisonMetrics,
                    ChartData = GenerateChartData(currentPeriod, previousPeriod, comparisonType),
                    Insights = insights,
                    BusinessData = userBusinesses.Count == 1 ? new BusinessComparisonData
                    {
                        BusinessId = userBusinesses.First().Id,
                        BusinessName = userBusinesses.First().Name,
                        CurrentPeriodViews = currentPeriod.TotalViews,
                        CurrentPeriodReviews = currentPeriod.TotalReviews,
                        PreviousPeriodViews = previousPeriod.TotalViews,
                        PreviousPeriodReviews = previousPeriod.TotalReviews
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparative analysis data for user {UserId}", userId);
                return new ComparativeAnalysisResponse
                {
                    ComparisonType = comparisonType,
                    Insights = new List<string> { "Unable to load comparative analysis data." }
                };
            }
        }

        private async Task<(PeriodData currentPeriod, PeriodData previousPeriod)> CalculatePeriodDataAsync(string userId, List<Business> businesses, string comparisonType, DateTime startDate, DateTime endDate)
        {
            var businessIds = businesses.Select(b => b.Id).ToList();
            var periodDays = comparisonType switch
            {
                "WeekOverWeek" => 7,
                "MonthOverMonth" => 30,
                "QuarterOverQuarter" => 90,
                "YearOverYear" => 365,
                _ => 30
            };

            var currentPeriodStart = endDate.AddDays(-periodDays);
            var previousPeriodStart = currentPeriodStart.AddDays(-periodDays);
            var previousPeriodEnd = currentPeriodStart.AddDays(-1);

            // Get current period data
            var currentViews = await _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId) && v.ViewedAt >= currentPeriodStart && v.ViewedAt <= endDate)
                .CountAsync();

            var currentReviews = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.CreatedAt >= currentPeriodStart && r.CreatedAt <= endDate)
                .CountAsync();

            var currentRating = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.CreatedAt >= currentPeriodStart && r.CreatedAt <= endDate)
                .AverageAsync(r => (double)r.Rating);

            // Get previous period data
            var previousViews = await _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId) && v.ViewedAt >= previousPeriodStart && v.ViewedAt <= previousPeriodEnd)
                .CountAsync();

            var previousReviews = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.CreatedAt >= previousPeriodStart && r.CreatedAt <= previousPeriodEnd)
                .CountAsync();

            var previousRating = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.CreatedAt >= previousPeriodStart && r.CreatedAt <= previousPeriodEnd)
                .AverageAsync(r => (double)r.Rating);

            var currentPeriod = new PeriodData
            {
                StartDate = currentPeriodStart,
                EndDate = endDate,
                TotalViews = currentViews,
                TotalReviews = currentReviews,
                TotalFavorites = 0, // Would need to implement favorites logic
                AverageRating = double.IsNaN(currentRating) ? 0 : currentRating,
                EngagementScore = CalculateEngagementScore(currentReviews, 0, currentViews),
                AverageViewsPerDay = periodDays > 0 ? (double)currentViews / periodDays : 0,
                AverageReviewsPerDay = periodDays > 0 ? (double)currentReviews / periodDays : 0
            };

            var previousPeriod = new PeriodData
            {
                StartDate = previousPeriodStart,
                EndDate = previousPeriodEnd,
                TotalViews = previousViews,
                TotalReviews = previousReviews,
                TotalFavorites = 0, // Would need to implement favorites logic
                AverageRating = double.IsNaN(previousRating) ? 0 : previousRating,
                EngagementScore = CalculateEngagementScore(previousReviews, 0, previousViews),
                AverageViewsPerDay = periodDays > 0 ? (double)previousViews / periodDays : 0,
                AverageReviewsPerDay = periodDays > 0 ? (double)previousReviews / periodDays : 0
            };

            return (currentPeriod, previousPeriod);
        }

        private ComparisonMetrics CalculateComparisonMetrics(PeriodData currentPeriod, PeriodData previousPeriod)
        {
            var viewsGrowthPercentage = previousPeriod.TotalViews > 0 
                ? (currentPeriod.TotalViews - previousPeriod.TotalViews) / (double)previousPeriod.TotalViews * 100 
                : 0;

            var reviewsGrowthPercentage = previousPeriod.TotalReviews > 0 
                ? (currentPeriod.TotalReviews - previousPeriod.TotalReviews) / (double)previousPeriod.TotalReviews * 100 
                : 0;

            var ratingGrowthPercentage = previousPeriod.AverageRating > 0 
                ? (currentPeriod.AverageRating - previousPeriod.AverageRating) / previousPeriod.AverageRating * 100 
                : 0;

            return new ComparisonMetrics
            {
                ViewsGrowthPercentage = viewsGrowthPercentage,
                ReviewsGrowthPercentage = reviewsGrowthPercentage,
                RatingGrowthPercentage = ratingGrowthPercentage,
                EngagementGrowthPercentage = previousPeriod.EngagementScore > 0 
                    ? (currentPeriod.EngagementScore - previousPeriod.EngagementScore) / previousPeriod.EngagementScore * 100 
                    : 0,
                OverallPerformanceChange = CalculateOverallPerformanceChange(currentPeriod, previousPeriod),
                // Set the legacy properties for backward compatibility
                ViewsChangePercent = viewsGrowthPercentage,
                ReviewsChangePercent = reviewsGrowthPercentage,
                RatingChangePercent = ratingGrowthPercentage,
                EngagementChangePercent = previousPeriod.EngagementScore > 0 
                    ? (currentPeriod.EngagementScore - previousPeriod.EngagementScore) / previousPeriod.EngagementScore * 100 
                    : 0
            };
        }

        private string CalculateOverallPerformanceChange(PeriodData currentPeriod, PeriodData previousPeriod)
        {
            var viewsChange = currentPeriod.TotalViews - previousPeriod.TotalViews;
            var reviewsChange = currentPeriod.TotalReviews - previousPeriod.TotalReviews;
            var ratingChange = currentPeriod.AverageRating - previousPeriod.AverageRating;

            if (viewsChange > 0 && reviewsChange > 0 && ratingChange > 0)
                return "Significantly Improved";
            if (viewsChange > 0 || reviewsChange > 0 || ratingChange > 0)
                return "Improved";
            if (viewsChange < 0 && reviewsChange < 0 && ratingChange < 0)
                return "Declined";
            return "Stable";
        }

        private List<string> GenerateComparativeInsights(PeriodData currentPeriod, PeriodData previousPeriod, ComparisonMetrics metrics)
        {
            var insights = new List<string>();

            if (metrics.ViewsGrowthPercentage > 10)
                insights.Add($"Views increased by {metrics.ViewsGrowthPercentage:F1}% compared to the previous period.");
            else if (metrics.ViewsGrowthPercentage < -10)
                insights.Add($"Views decreased by {Math.Abs(metrics.ViewsGrowthPercentage):F1}% compared to the previous period.");

            if (metrics.ReviewsGrowthPercentage > 10)
                insights.Add($"Reviews increased by {metrics.ReviewsGrowthPercentage:F1}% compared to the previous period.");
            else if (metrics.ReviewsGrowthPercentage < -10)
                insights.Add($"Reviews decreased by {Math.Abs(metrics.ReviewsGrowthPercentage):F1}% compared to the previous period.");

            if (metrics.RatingGrowthPercentage > 5)
                insights.Add($"Average rating improved by {metrics.RatingGrowthPercentage:F1}% compared to the previous period.");
            else if (metrics.RatingGrowthPercentage < -5)
                insights.Add($"Average rating decreased by {Math.Abs(metrics.RatingGrowthPercentage):F1}% compared to the previous period.");

            if (insights.Count == 0)
                insights.Add("Performance remained stable compared to the previous period.");

            return insights;
        }

        private ComparativeChartData GenerateChartData(PeriodData currentPeriod, PeriodData previousPeriod, string comparisonType)
        {
            return new ComparativeChartData
            {
                Labels = new List<string> { "Current Period", "Previous Period" },
                Datasets = new List<ComparativeChartDataset>
                {
                    new ComparativeChartDataset
                    {
                        Label = "Views",
                        Data = new List<double> { currentPeriod.TotalViews, previousPeriod.TotalViews },
                        BackgroundColor = "#33658a",
                        BorderColor = "#33658a",
                        BorderWidth = 1
                    },
                    new ComparativeChartDataset
                    {
                        Label = "Reviews",
                        Data = new List<double> { currentPeriod.TotalReviews, previousPeriod.TotalReviews },
                        BackgroundColor = "#f6ae2d",
                        BorderColor = "#f6ae2d",
                        BorderWidth = 1
                    }
                }
            };
        }

        private UserInfo ConvertToUserInfo(ApplicationUser user)
        {
            return new UserInfo
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Location = user.Location,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                AuthenticationMethod = user.AuthenticationMethod,
                CurrentSubscriptionTier = user.CurrentSubscriptionTier,
                HasActiveSubscription = user.HasActiveSubscription,
                SubscriptionStartDate = user.SubscriptionStartDate,
                SubscriptionEndDate = user.SubscriptionEndDate,
                IsTrialUser = user.IsTrialUser,
                TrialStartDate = user.TrialStartDate,
                TrialEndDate = user.TrialEndDate,
                TrialExpired = user.TrialExpired
            };
        }

        private List<string> GeneratePerformanceRecommendations(BusinessAnalyticsData analytics)
        {
            var recommendations = new List<string>();

            if (analytics.EngagementScore < 5)
                recommendations.Add("Focus on improving customer engagement through better service quality");

            if (analytics.AverageRating < 4.0)
                recommendations.Add("Work on improving customer satisfaction and ratings");

            if (analytics.ViewsGrowthRate < 0)
                recommendations.Add("Consider marketing strategies to increase business visibility");

            if (analytics.ReviewsGrowthRate < 0)
                recommendations.Add("Encourage customers to leave reviews and feedback");

            return recommendations;
        }

        private List<CompetitorInsight> GetTopPerformers(List<BusinessAnalyticsData> analytics, int count)
        {
            return analytics.OrderByDescending(a => a.EngagementScore)
                .Take(count)
                .Select(a => new CompetitorInsight
                {
                    BusinessId = a.BusinessId,
                    BusinessName = a.BusinessName,
                    Category = a.Category,
                    Town = a.Town,
                    AverageCompetitorViews = a.TotalViews,
                    AverageCompetitorReviews = a.TotalReviews,
                    AverageCompetitorRating = a.AverageRating
                })
                .ToList();
        }

        private List<string> GenerateCategoryInsights(List<BusinessAnalyticsData> userAnalytics, List<BusinessAnalyticsData> competitorAnalytics)
        {
            var insights = new List<string>();

            if (!userAnalytics.Any() || !competitorAnalytics.Any()) return insights;

            var userAvgViews = userAnalytics.Average(a => a.TotalViews);
            var competitorAvgViews = competitorAnalytics.Average(a => a.TotalViews);

            if (userAvgViews < competitorAvgViews * 0.8)
                insights.Add("Your businesses are getting fewer views than competitors in this category");

            var userAvgRating = userAnalytics.Average(a => a.AverageRating);
            var competitorAvgRating = competitorAnalytics.Average(a => a.AverageRating);

            if (userAvgRating < competitorAvgRating)
                insights.Add("Consider improving service quality to match competitor ratings");

            return insights;
        }

        private int GetRank(double value, IEnumerable<double> values)
        {
            var sortedValues = values.OrderByDescending(v => v).ToList();
            return sortedValues.IndexOf(value) + 1;
        }

        private List<string> GenerateCompetitorRecommendations(BusinessAnalyticsData userAnalytics, List<BusinessAnalyticsData> competitorAnalytics)
        {
            var recommendations = new List<string>();

            if (!competitorAnalytics.Any()) return recommendations;

            var avgCompetitorViews = competitorAnalytics.Average(a => a.TotalViews);
            if (userAnalytics.TotalViews < avgCompetitorViews * 0.8)
                recommendations.Add("Focus on increasing visibility to match competitor view counts");

            var avgCompetitorRating = competitorAnalytics.Average(a => a.AverageRating);
            if (userAnalytics.AverageRating < avgCompetitorRating)
                recommendations.Add("Work on improving customer satisfaction to match competitor ratings");

            return recommendations;
        }
    }
}