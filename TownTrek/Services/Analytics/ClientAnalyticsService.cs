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
    /// Service for client-specific analytics functionality
    /// </summary>
    public class ClientAnalyticsService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        ISubscriptionAuthService subscriptionAuthService,
        IViewTrackingService viewTrackingService,
        IAnalyticsSnapshotService analyticsSnapshotService,
        IAnalyticsErrorHandler errorHandler,
        ILogger<ClientAnalyticsService> logger) : IClientAnalyticsService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly IAnalyticsSnapshotService _analyticsSnapshotService = analyticsSnapshotService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<ClientAnalyticsService> _logger = logger;

        public async Task<ClientAnalyticsViewModel> GetClientAnalyticsAsync(string userId)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate user ID
                var userValidation = await _validationService.ValidateUserIdAsync(userId);
                if (!userValidation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        userValidation.ErrorMessage ?? "Invalid user validation",
                        userId,
                        "UserId",
                        "UserValidation",
                        new Dictionary<string, object> { ["UserId"] = userId }
                    );
                    throw new AnalyticsValidationException(userValidation.ErrorMessage ?? "Invalid user validation", "UserId", "UserValidation");
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ClientAnalytics");

                var user = await _dataService.GetUserAsync(userId);
                if (user == null)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        "User not found",
                        userId,
                        "UserId",
                        "UserExists",
                        new Dictionary<string, object> { ["UserId"] = userId }
                    );
                    throw new AnalyticsValidationException("User not found", "UserId", "UserExists");
                }

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
                    User = ConvertToUserInfo(user),
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
            }, userId, "GetClientAnalytics", new Dictionary<string, object> { ["UserId"] = userId });
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

        public async Task<List<BusinessPerformanceInsight>> GetPerformanceInsightsAsync(string userId)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "PerformanceInsights");

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return new List<BusinessPerformanceInsight>();

                var businessIds = businesses.Select(b => b.Id).ToList();
                var insights = new List<BusinessPerformanceInsight>();

                foreach (var business in businesses)
                {
                    var businessAnalytics = await GetBusinessAnalyticsAsync(business.Id, userId);
                    
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        Category = business.Category,
                        PerformanceRating = businessAnalytics.PerformanceRating,
                        EngagementScore = businessAnalytics.EngagementScore,
                        ViewsGrowthRate = businessAnalytics.ViewsGrowthRate,
                        ReviewsGrowthRate = businessAnalytics.ReviewsGrowthRate,
                        FavoritesGrowthRate = businessAnalytics.FavoritesGrowthRate,
                        RatingGrowthRate = businessAnalytics.RatingGrowthRate,
                        Recommendations = GeneratePerformanceRecommendations(businessAnalytics)
                    });
                }

                return insights.OrderByDescending(i => i.EngagementScore).ToList();
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

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return null;

                // Get user's businesses in this category
                var userBusinessesInCategory = businesses.Where(b => b.Category == category).ToList();
                if (!userBusinessesInCategory.Any()) return null;

                var userBusinessIds = userBusinessesInCategory.Select(b => b.Id).ToList();
                
                // Get all businesses in this category for comparison
                var allBusinessesInCategory = await _dataService.GetBusinessesByCategoryAsync(category);
                var competitorBusinessIds = allBusinessesInCategory.Where(b => !userBusinessIds.Contains(b.Id)).Select(b => b.Id).ToList();

                if (!competitorBusinessIds.Any()) return null;

                // Get analytics data for comparison
                var userAnalytics = await GetBusinessAnalyticsBatchAsync(userBusinessIds, userId);
                var competitorAnalytics = await GetBusinessAnalyticsBatchAsync(competitorBusinessIds, userId);

                return new CategoryBenchmarkData
                {
                    Category = category,
                    UserBusinessCount = userBusinessesInCategory.Count,
                    TotalBusinessCount = allBusinessesInCategory.Count,
                    AverageViews = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.TotalViews) : 0,
                    AverageReviews = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.TotalReviews) : 0,
                    AverageRating = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.AverageRating) : 0,
                    UserAverageViews = userAnalytics.Any() ? userAnalytics.Average(a => a.TotalViews) : 0,
                    UserAverageReviews = userAnalytics.Any() ? userAnalytics.Average(a => a.TotalReviews) : 0,
                    UserAverageRating = userAnalytics.Any() ? userAnalytics.Average(a => a.AverageRating) : 0
                };
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

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return null;

                // Get user's businesses in this category
                var userBusinessesInCategory = businesses.Where(b => b.Category == category).ToList();
                if (!userBusinessesInCategory.Any()) return null;

                var userBusinessIds = userBusinessesInCategory.Select(b => b.Id).ToList();
                
                // Get all businesses in this category for comparison
                var allBusinessesInCategory = await _dataService.GetBusinessesByCategoryAsync(category);
                var competitorBusinessIds = allBusinessesInCategory.Where(b => !userBusinessIds.Contains(b.Id)).Select(b => b.Id).ToList();

                if (!competitorBusinessIds.Any()) return null;

                // Get analytics data for comparison
                var userAnalytics = await GetBusinessAnalyticsBatchAsync(userBusinessIds, userId);
                var competitorAnalytics = await GetBusinessAnalyticsBatchAsync(competitorBusinessIds, userId);

                return new CategoryBenchmarks
                {
                    Category = category,
                    UserBusinessCount = userBusinessesInCategory.Count,
                    TotalBusinessCount = allBusinessesInCategory.Count,
                    AverageViews = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.TotalViews) : 0,
                    AverageReviews = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.TotalReviews) : 0,
                    AverageRating = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.AverageRating) : 0,
                    UserAverageViews = userAnalytics.Any() ? userAnalytics.Average(a => a.TotalViews) : 0,
                    UserAverageReviews = userAnalytics.Any() ? userAnalytics.Average(a => a.TotalReviews) : 0,
                    UserAverageRating = userAnalytics.Any() ? userAnalytics.Average(a => a.AverageRating) : 0,
                    TopPerformers = GetTopPerformers(competitorAnalytics, 5),
                    PerformanceInsights = GenerateCategoryInsights(userAnalytics, competitorAnalytics)
                };
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

                var businesses = await _dataService.GetUserBusinessesAsync(userId);
                if (!businesses.Any()) return new List<CompetitorInsight>();

                var insights = new List<CompetitorInsight>();

                foreach (var business in businesses)
                {
                    // Get competitors in the same category and town
                    var competitors = await _dataService.GetCompetitorBusinessesAsync(business.Id, business.Category, business.Town?.Name ?? "Unknown");
                    if (!competitors.Any()) continue;

                    var competitorIds = competitors.Select(c => c.Id).ToList();
                    var competitorAnalytics = await GetBusinessAnalyticsBatchAsync(competitorIds, userId);
                    var businessAnalytics = await GetBusinessAnalyticsAsync(business.Id, userId);

                    var insight = new CompetitorInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        Category = business.Category,
                        Town = business.Town?.Name ?? "Unknown",
                        CompetitorCount = competitors.Count,
                        AverageCompetitorViews = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.TotalViews) : 0,
                        AverageCompetitorReviews = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.TotalReviews) : 0,
                        AverageCompetitorRating = competitorAnalytics.Any() ? competitorAnalytics.Average(a => a.AverageRating) : 0,
                        UserViews = businessAnalytics.TotalViews,
                        UserReviews = businessAnalytics.TotalReviews,
                        UserRating = businessAnalytics.AverageRating,
                        ViewsRank = GetRank((double)businessAnalytics.TotalViews, competitorAnalytics.Select(a => (double)a.TotalViews)),
                        ReviewsRank = GetRank((double)businessAnalytics.TotalReviews, competitorAnalytics.Select(a => (double)a.TotalReviews)),
                        RatingRank = GetRank(businessAnalytics.AverageRating, competitorAnalytics.Select(a => a.AverageRating)),
                        Recommendations = GenerateCompetitorRecommendations(businessAnalytics, competitorAnalytics)
                    };

                    insights.Add(insight);
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

        // Private helper methods
        private async Task<List<BusinessAnalyticsData>> GetBusinessAnalyticsBatchAsync(List<int> businessIds, string userId)
        {
            if (!businessIds.Any()) return new List<BusinessAnalyticsData>();

            var analytics = new List<BusinessAnalyticsData>();
            foreach (var businessId in businessIds)
            {
                analytics.Add(await GetBusinessAnalyticsAsync(businessId, userId));
            }
            return analytics;
        }

        private Task<AnalyticsOverview> GetAnalyticsOverviewAsync(string userId, List<BusinessAnalyticsData> businessAnalytics)
        {
            if (!businessAnalytics.Any()) return Task.FromResult(new AnalyticsOverview());

            return Task.FromResult(new AnalyticsOverview
            {
                TotalBusinesses = businessAnalytics.Count,
                TotalViews = businessAnalytics.Sum(a => a.TotalViews),
                TotalReviews = businessAnalytics.Sum(a => a.TotalReviews),
                TotalFavorites = businessAnalytics.Sum(a => a.TotalFavorites),
                AverageRating = businessAnalytics.Average(a => a.AverageRating),
                AverageEngagementScore = businessAnalytics.Average(a => a.EngagementScore),
                TopPerformingBusiness = businessAnalytics.OrderByDescending(a => a.EngagementScore).First()?.BusinessName ?? "None"
            });
        }

        private async Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days)
        {
            var businesses = await _dataService.GetUserBusinessesAsync(userId);
            if (!businesses.Any()) return new List<ViewsOverTimeData>();

            var businessIds = businesses.Select(b => b.Id).ToList();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var viewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, startDate, endDate);
            return ProcessViewsOverTimeData(viewLogs, startDate, endDate);
        }

        private async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days)
        {
            var businesses = await _dataService.GetUserBusinessesAsync(userId);
            if (!businesses.Any()) return new List<ReviewsOverTimeData>();

            var businessIds = businesses.Select(b => b.Id).ToList();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var reviews = await _dataService.GetBusinessReviewsAsync(businessIds, startDate, endDate);
            return ProcessReviewsOverTimeData(reviews, startDate, endDate);
        }

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

        private double CalculateEngagementScore(int reviews, int favorites, int views)
        {
            if (views == 0) return 0;
            return (reviews * 2 + favorites * 1.5) / views * 100;
        }

        private double CalculateNumericPerformanceRating(int reviews, int favorites, int views, double rating)
        {
            var engagementScore = CalculateEngagementScore(reviews, favorites, views);
            return (engagementScore * 0.6) + (rating * 8); // Weighted score
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
    }
}
