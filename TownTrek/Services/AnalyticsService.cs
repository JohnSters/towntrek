using Microsoft.EntityFrameworkCore;
using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class AnalyticsService(
        ApplicationDbContext context,
        ISubscriptionAuthService subscriptionAuthService,
        IViewTrackingService viewTrackingService,
        IAnalyticsSnapshotService analyticsSnapshotService,
        ILogger<AnalyticsService> logger) : IAnalyticsService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly IViewTrackingService _viewTrackingService = viewTrackingService;
        private readonly IAnalyticsSnapshotService _analyticsSnapshotService = analyticsSnapshotService;
        private readonly ILogger<AnalyticsService> _logger = logger;

        public async Task<ClientAnalyticsViewModel> GetClientAnalyticsAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new ArgumentException("User not found", nameof(userId));

            // All non-trial users get the same analytics experience.
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            var hasBasicAnalytics = true;
            var hasAdvancedAnalytics = true;

            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .Include(b => b.Town)
                .ToListAsync();

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

        /// <summary>
        /// Optimized batch method to get analytics for multiple businesses in a single query
        /// </summary>
        private async Task<List<BusinessAnalyticsData>> GetBusinessAnalyticsBatchAsync(List<int> businessIds, string userId)
        {
            if (!businessIds.Any()) return new List<BusinessAnalyticsData>();

            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            // OPTIMIZED: Single query for all businesses
            var businesses = await _context.Businesses
                .Where(b => businessIds.Contains(b.Id) && b.UserId == userId)
                .ToListAsync();

            // OPTIMIZED: Single query for all reviews
            var allReviews = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.IsActive)
                .ToListAsync();

            // OPTIMIZED: Single query for all favorites
            var allFavorites = await _context.FavoriteBusinesses
                .Where(f => businessIds.Contains(f.BusinessId))
                .ToListAsync();

            // OPTIMIZED: Batch growth rate calculations
            var growthRatesTasks = businessIds.Select(id => _analyticsSnapshotService.CalculateGrowthRatesAsync(id, AnalyticsConstants.DefaultGrowthRateDays, AnalyticsConstants.DefaultGrowthRateDays));
            var growthRatesResults = await Task.WhenAll(growthRatesTasks);
            var growthRatesDict = businessIds.Zip(growthRatesResults, (id, rates) => new { Id = id, Rates = rates })
                .ToDictionary(x => x.Id, x => x.Rates);

            var result = new List<BusinessAnalyticsData>();

            foreach (var business in businesses)
            {
                var businessReviews = allReviews.Where(r => r.BusinessId == business.Id).ToList();
                var businessFavorites = allFavorites.Where(f => f.BusinessId == business.Id).ToList();
                var growthRates = growthRatesDict[business.Id];

                var reviewsThisMonth = businessReviews.Count(r => r.CreatedAt >= thisMonthStart);
                var reviewsLastMonth = businessReviews.Count(r => r.CreatedAt >= lastMonthStart && r.CreatedAt <= lastMonthEnd);
                var favoritesThisMonth = businessFavorites.Count(f => f.CreatedAt >= thisMonthStart);

                var engagementScore = CalculateEngagementScore(business.ViewCount, businessReviews.Count, businessFavorites.Count);

                var analytics = new BusinessAnalyticsData
                {
                    BusinessId = business.Id,
                    BusinessName = business.Name,
                    Category = business.Category,
                    Status = business.Status,
                    TotalViews = business.ViewCount,
                    ViewsThisMonth = growthRates.CurrentPeriodViews,
                    ViewsLastMonth = growthRates.PreviousPeriodViews,
                    ViewsGrowthRate = (double)growthRates.ViewsGrowthRate,
                    TotalReviews = businessReviews.Count,
                    AverageRating = businessReviews.Any() ? businessReviews.Average(r => r.Rating) : 0,
                    ReviewsThisMonth = reviewsThisMonth,
                    ReviewsLastMonth = reviewsLastMonth,
                    TotalFavorites = businessFavorites.Count,
                    FavoritesThisMonth = favoritesThisMonth,
                    EngagementScore = engagementScore,
                    PerformanceTrend = DeterminePerformanceTrend((double)growthRates.ViewsGrowthRate, reviewsThisMonth, reviewsLastMonth),
                    Recommendations = GenerateRecommendations(business, businessReviews, businessFavorites)
                };

                result.Add(analytics);
            }

            return result;
        }

        /// <summary>
        /// Optimized batch method to get views over time for multiple businesses
        /// </summary>
        private async Task<List<ViewsOverTimeData>> GetViewsOverTimeBatchAsync(List<int> businessIds, int days = AnalyticsConstants.DefaultAnalyticsDays)
        {
            if (!businessIds.Any()) return new List<ViewsOverTimeData>();

            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days + 1);

            // OPTIMIZED: Single query for all view logs
            var allViewLogs = await _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId) && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
                .ToListAsync();

            // OPTIMIZED: Single query for business names
            var businessNames = await _context.Businesses
                .Where(b => businessIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Name })
                .ToDictionaryAsync(b => b.Id, b => b.Name);

            var data = new List<ViewsOverTimeData>();

            // Group by business and date
            var groupedViews = allViewLogs
                .GroupBy(v => new { v.BusinessId, Date = v.ViewedAt.Date })
                .Select(g => new
                {
                    g.Key.BusinessId,
                    g.Key.Date,
                    TotalViews = g.Count()
                })
                .ToList();

            // Create complete dataset with zero values for missing dates
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var businessId in businessIds)
                {
                    var dayViews = groupedViews.FirstOrDefault(v => v.BusinessId == businessId && v.Date == date);
                    data.Add(new ViewsOverTimeData
                    {
                        Date = date,
                        Views = dayViews?.TotalViews ?? 0,
                        BusinessId = businessId,
                        BusinessName = businessNames[businessId]
                    });
                }
            }

            return data.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// Optimized batch method to get performance insights for multiple businesses
        /// </summary>
        private async Task<List<BusinessPerformanceInsight>> GetPerformanceInsightsBatchAsync(List<int> businessIds, string userId)
        {
            if (!businessIds.Any()) return new List<BusinessPerformanceInsight>();

            // OPTIMIZED: Single query for all businesses with towns
            var businesses = await _context.Businesses
                .Where(b => businessIds.Contains(b.Id) && b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .Include(b => b.Town)
                .ToListAsync();

            // OPTIMIZED: Single query for all reviews
            var allReviews = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.IsActive)
                .ToListAsync();

            // OPTIMIZED: Single query for all favorites
            var allFavorites = await _context.FavoriteBusinesses
                .Where(f => businessIds.Contains(f.BusinessId))
                .ToListAsync();

            var insights = new List<BusinessPerformanceInsight>();

            foreach (var business in businesses)
            {
                var businessReviews = allReviews.Where(r => r.BusinessId == business.Id).ToList();
                var favoritesCount = allFavorites.Count(f => f.BusinessId == business.Id);

                // Generate insights based on performance
                if (businessReviews.Count == 0)
                {
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        InsightType = "opportunity",
                        Title = "No Reviews Yet",
                        Description = $"{business.Name} hasn't received any reviews yet.",
                        ActionRecommendation = "Encourage satisfied customers to leave reviews to build credibility.",
                        Priority = 4
                    });
                }
                else if (businessReviews.Average(r => r.Rating) < AnalyticsConstants.LowRatingThreshold)
                {
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        InsightType = "warning",
                        Title = "Low Rating",
                        Description = $"{business.Name} has an average rating of {businessReviews.Average(r => r.Rating):F1} stars.",
                        ActionRecommendation = "Review recent feedback and address customer concerns to improve ratings.",
                        Priority = AnalyticsConstants.MinFavoritesThreshold
                    });
                }
                else if (businessReviews.Average(r => r.Rating) >= AnalyticsConstants.ExcellentRatingThreshold)
                {
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        InsightType = "success",
                        Title = "Excellent Performance",
                        Description = $"{business.Name} maintains an excellent {businessReviews.Average(r => r.Rating):F1} star rating.",
                        ActionRecommendation = "Keep up the great work! Consider highlighting positive reviews in marketing.",
                        Priority = 2
                    });
                }

                if (business.ViewCount < AnalyticsConstants.LowVisibilityThreshold)
                {
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        InsightType = "opportunity",
                        Title = "Low Visibility",
                        Description = $"{business.Name} has only {business.ViewCount} views.",
                        ActionRecommendation = "Optimize your business description and add more photos to increase visibility.",
                        Priority = 3
                    });
                }
            }

            return insights.OrderByDescending(i => i.Priority).ToList();
        }

        public async Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(int businessId, string userId)
        {
            var business = await _context.Businesses
                .Where(b => b.Id == businessId && b.UserId == userId)
                .FirstOrDefaultAsync();

            if (business == null) throw new ArgumentException("Business not found", nameof(businessId));

            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            // Get review metrics
            var reviews = await _context.BusinessReviews
                .Where(r => r.BusinessId == businessId && r.IsActive)
                .ToListAsync();

            var reviewsThisMonth = reviews.Count(r => r.CreatedAt >= thisMonthStart);
            var reviewsLastMonth = reviews.Count(r => r.CreatedAt >= lastMonthStart && r.CreatedAt <= lastMonthEnd);

            // Get favorites metrics
            var favorites = await _context.FavoriteBusinesses
                .Where(f => f.BusinessId == businessId)
                .ToListAsync();

            var favoritesThisMonth = favorites.Count(f => f.CreatedAt >= thisMonthStart);

            // Calculate growth rates using historical data
            var growthRates = await _analyticsSnapshotService.CalculateGrowthRatesAsync(businessId, AnalyticsConstants.DefaultGrowthRateDays, AnalyticsConstants.DefaultGrowthRateDays);
            var engagementScore = CalculateEngagementScore(business.ViewCount, reviews.Count, favorites.Count);

            var analytics = new BusinessAnalyticsData
            {
                BusinessId = business.Id,
                BusinessName = business.Name,
                Category = business.Category,
                Status = business.Status,
                TotalViews = business.ViewCount,
                ViewsThisMonth = growthRates.CurrentPeriodViews,
                ViewsLastMonth = growthRates.PreviousPeriodViews,
                ViewsGrowthRate = (double)growthRates.ViewsGrowthRate,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                ReviewsThisMonth = reviewsThisMonth,
                ReviewsLastMonth = reviewsLastMonth,
                TotalFavorites = favorites.Count,
                FavoritesThisMonth = favoritesThisMonth,
                EngagementScore = engagementScore,
                PerformanceTrend = DeterminePerformanceTrend((double)growthRates.ViewsGrowthRate, reviewsThisMonth, reviewsLastMonth),
                Recommendations = GenerateRecommendations(business, reviews, favorites)
            };

            return analytics;
        }

        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = AnalyticsConstants.DefaultAnalyticsDays)
        {
            // OPTIMIZED: Get businesses and use batch query
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .ToListAsync();

            var businessIds = businesses.Select(b => b.Id).ToList();
            return await GetViewsOverTimeBatchAsync(businessIds, days);
        }

        public async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = AnalyticsConstants.DefaultAnalyticsDays)
        {
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .ToListAsync();

            var businessIds = businesses.Select(b => b.Id).ToList();
            var startDate = DateTime.UtcNow.AddDays(-days);

            var reviews = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.CreatedAt >= startDate && r.IsActive)
                .Include(r => r.Business)
                .ToListAsync();

            var data = reviews
                .GroupBy(r => new { Date = r.CreatedAt.Date, r.BusinessId, r.Business.Name })
                .Select(g => new ReviewsOverTimeData
                {
                    Date = g.Key.Date,
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => r.Rating),
                    BusinessId = g.Key.BusinessId,
                    BusinessName = g.Key.Name
                })
                .OrderBy(d => d.Date)
                .ToList();

            return data;
        }

        public async Task<List<BusinessPerformanceInsight>> GetPerformanceInsightsAsync(string userId)
        {
            // OPTIMIZED: Get businesses and use batch query
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .ToListAsync();

            var businessIds = businesses.Select(b => b.Id).ToList();
            return await GetPerformanceInsightsBatchAsync(businessIds, userId);
        }

        public async Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category)
        {
            var userBusinesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Category == category && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .ToListAsync();

            if (!userBusinesses.Any()) return null;

            var categoryBusinesses = await _context.Businesses
                .Where(b => b.Category == category && b.Status == AnalyticsConstants.BusinessStatus.Active)
                .ToListAsync();

            if (categoryBusinesses.Count < AnalyticsConstants.MinCategoryBusinessesForBenchmark) return null; // Need enough data for meaningful benchmarks

            var categoryReviews = await _context.BusinessReviews
                .Where(r => categoryBusinesses.Select(b => b.Id).Contains(r.BusinessId) && r.IsActive)
                .ToListAsync();

            var avgViews = categoryBusinesses.Average(b => b.ViewCount);
            var avgRating = categoryReviews.Any() ? categoryReviews.Average(r => r.Rating) : 0;
            var avgReviewCount = categoryBusinesses.Average(b => categoryReviews.Count(r => r.BusinessId == b.Id));

            var userAvgViews = userBusinesses.Average(b => b.ViewCount);
            var userReviews = await _context.BusinessReviews
                .Where(r => userBusinesses.Select(b => b.Id).Contains(r.BusinessId) && r.IsActive)
                .ToListAsync();
            var userAvgRating = userReviews.Any() ? userReviews.Average(r => r.Rating) : 0;
            var userAvgReviewCount = userBusinesses.Average(b => userReviews.Count(r => r.BusinessId == b.Id));

            var performance = DetermineOverallPerformance(userAvgViews, avgViews, userAvgRating, avgRating);

            return new CategoryBenchmarkData
            {
                Category = category,
                AverageViewsInCategory = avgViews,
                AverageRatingInCategory = avgRating,
                AverageReviewsInCategory = avgReviewCount,
                YourPerformanceVsAverage = performance,
                Metrics = new List<BenchmarkMetric>
                {
                    new BenchmarkMetric
                    {
                        MetricName = "Views",
                        YourValue = userAvgViews,
                        CategoryAverage = avgViews,
                        PercentilRank = CalculatePercentile(userAvgViews, categoryBusinesses.Select(b => (double)b.ViewCount)),
                        Performance = DetermineMetricPerformance(userAvgViews, avgViews)
                    },
                    new BenchmarkMetric
                    {
                        MetricName = "Rating",
                        YourValue = userAvgRating,
                        CategoryAverage = avgRating,
                        PercentilRank = CalculatePercentile(userAvgRating, categoryReviews.GroupBy(r => r.BusinessId).Select(g => g.Average(r => (double)r.Rating))),
                        Performance = DetermineMetricPerformance(userAvgRating, avgRating)
                    }
                }
            };
        }

        public async Task<List<CompetitorInsight>> GetCompetitorInsightsAsync(string userId)
        {
            var userBusinesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != "Deleted")
                .Include(b => b.Town)
                .ToListAsync();

            var insights = new List<CompetitorInsight>();

            foreach (var businessGroup in userBusinesses.GroupBy(b => new { b.Category, b.TownId }))
            {
                var competitors = await _context.Businesses
                    .Where(b => b.Category == businessGroup.Key.Category && 
                               b.TownId == businessGroup.Key.TownId && 
                               b.UserId != userId && 
                               b.Status == "Active")
                    .ToListAsync();

                if (competitors.Any())
                {
                    var competitorReviews = await _context.BusinessReviews
                        .Where(r => competitors.Select(c => c.Id).Contains(r.BusinessId) && r.IsActive)
                        .ToListAsync();

                    var avgCompetitorRating = competitorReviews.Any() ? competitorReviews.Average(r => r.Rating) : 0;
                    var userBusinessRating = await GetUserBusinessAverageRating(businessGroup.Select(b => b.Id).ToList());

                    var town = await _context.Towns.FindAsync(businessGroup.Key.TownId);
                    
                    insights.Add(new CompetitorInsight
                    {
                        Category = businessGroup.Key.Category,
                        Town = town?.Name ?? "Unknown",
                        CompetitorCount = competitors.Count,
                        AverageCompetitorRating = avgCompetitorRating,
                        MarketPosition = DetermineMarketPosition(userBusinessRating, avgCompetitorRating, competitors.Count),
                        OpportunityAreas = GenerateOpportunityAreas(businessGroup.Key.Category, userBusinessRating, avgCompetitorRating)
                    });
                }
            }

            return insights;
        }

        public async Task<CategoryBenchmarks?> GetDetailedCategoryBenchmarksAsync(string userId, string category)
        {
            var basicBenchmarks = await GetCategoryBenchmarksAsync(userId, category);
            if (basicBenchmarks == null) return null;

            var userBusinesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Category == category && b.Status != "Deleted")
                .ToListAsync();

            var userReviews = await _context.BusinessReviews
                .Where(r => userBusinesses.Select(b => b.Id).Contains(r.BusinessId) && r.IsActive)
                .ToListAsync();

            var insights = new List<string>();
            
            // Generate insights based on performance
            if (basicBenchmarks.YourPerformanceVsAverage == "above")
            {
                insights.Add("Your businesses are performing above category average - great work!");
                insights.Add("Consider sharing your success strategies with other businesses in your network.");
            }
            else if (basicBenchmarks.YourPerformanceVsAverage == "below")
            {
                insights.Add("There's room for improvement compared to category averages.");
                insights.Add("Focus on improving customer engagement and service quality.");
                insights.Add("Consider updating your business information and photos regularly.");
            }
            else
            {
                insights.Add("Your performance is on par with category averages.");
                insights.Add("Small improvements in customer service could help you stand out.");
            }

            return new CategoryBenchmarks
            {
                Category = category,
                YourPerformanceVsAverage = basicBenchmarks.YourPerformanceVsAverage,
                YourAverageViews = (int)userBusinesses.Average(b => b.ViewCount),
                YourAverageReviews = (int)userBusinesses.Average(b => userReviews.Count(r => r.BusinessId == b.Id)),
                YourAverageRating = userReviews.Any() ? userReviews.Average(r => r.Rating) : 0,
                CategoryAverageViews = (int)basicBenchmarks.AverageViewsInCategory,
                CategoryAverageReviews = (int)basicBenchmarks.AverageReviewsInCategory,
                CategoryAverageRating = basicBenchmarks.AverageRatingInCategory,
                Insights = insights
            };
        }

        public async Task RecordBusinessViewAsync(int businessId)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business != null)
            {
                business.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        // Platform-specific analytics methods
        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = AnalyticsConstants.DefaultAnalyticsDays, string? platform = null)
        {
            // OPTIMIZED: Get businesses and use batch query with platform filter
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .ToListAsync();

            var businessIds = businesses.Select(b => b.Id).ToList();
            
            if (string.IsNullOrEmpty(platform))
            {
                return await GetViewsOverTimeBatchAsync(businessIds, days);
            }
            
            // For platform-specific queries, we need to filter the view logs
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days + 1);

            // OPTIMIZED: Single query for all view logs with platform filter
            var allViewLogs = await _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId) && v.ViewedAt >= startDate && v.ViewedAt <= endDate && v.Platform == platform)
                .ToListAsync();

            // OPTIMIZED: Single query for business names
            var businessNames = await _context.Businesses
                .Where(b => businessIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Name })
                .ToDictionaryAsync(b => b.Id, b => b.Name);

            var data = new List<ViewsOverTimeData>();

            // Group by business and date
            var groupedViews = allViewLogs
                .GroupBy(v => new { v.BusinessId, Date = v.ViewedAt.Date })
                .Select(g => new
                {
                    g.Key.BusinessId,
                    g.Key.Date,
                    TotalViews = g.Count()
                })
                .ToList();

            // Create complete dataset with zero values for missing dates
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var businessId in businessIds)
                {
                    var dayViews = groupedViews.FirstOrDefault(v => v.BusinessId == businessId && v.Date == date);
                    data.Add(new ViewsOverTimeData
                    {
                        Date = date,
                        Views = dayViews?.TotalViews ?? 0,
                        BusinessId = businessId,
                        BusinessName = businessNames[businessId]
                    });
                }
            }

            return data.OrderBy(d => d.Date).ToList();
        }

        public async Task<ViewStatistics> GetBusinessViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            return await _viewTrackingService.GetViewStatisticsAsync(businessId, startDate, endDate, platform);
        }

        // Helper methods
        private Task<AnalyticsOverview> GetAnalyticsOverviewAsync(string userId, List<BusinessAnalyticsData> businessAnalytics)
        {
            var totalViews = businessAnalytics.Sum(b => b.TotalViews);
            var totalReviews = businessAnalytics.Sum(b => b.TotalReviews);
            var totalFavorites = businessAnalytics.Sum(b => b.TotalFavorites);
            var overallRating = businessAnalytics.Where(b => b.TotalReviews > 0).Any() 
                ? businessAnalytics.Where(b => b.TotalReviews > 0).Average(b => b.AverageRating) 
                : 0;

            var viewsThisMonth = businessAnalytics.Sum(b => b.ViewsThisMonth);
            var reviewsThisMonth = businessAnalytics.Sum(b => b.ReviewsThisMonth);
            var favoritesThisMonth = businessAnalytics.Sum(b => b.FavoritesThisMonth);

            var topPerforming = businessAnalytics.OrderByDescending(b => b.TotalViews).FirstOrDefault();
            var mostReviewed = businessAnalytics.OrderByDescending(b => b.TotalReviews).FirstOrDefault();
            var mostFavorited = businessAnalytics.OrderByDescending(b => b.TotalFavorites).FirstOrDefault();

            return Task.FromResult(new AnalyticsOverview
            {
                TotalViews = totalViews,
                TotalReviews = totalReviews,
                TotalFavorites = totalFavorites,
                OverallRating = overallRating,
                ViewsThisMonth = viewsThisMonth,
                ReviewsThisMonth = reviewsThisMonth,
                FavoritesThisMonth = favoritesThisMonth,
                ViewsGrowthRate = 0, // Would need historical data
                ReviewsGrowthRate = 0, // Would need historical data
                FavoritesGrowthRate = 0, // Would need historical data
                TopPerformingBusiness = topPerforming?.BusinessName ?? "",
                MostReviewedBusiness = mostReviewed?.BusinessName ?? "",
                MostFavoritedBusiness = mostFavorited?.BusinessName ?? "",
                KeyInsights = GenerateKeyInsights(businessAnalytics),
                ActionableRecommendations = GenerateActionableRecommendations(businessAnalytics)
            });
        }

        private static double CalculateGrowthRate(int current, int previous)
        {
            if (previous == 0) return current > 0 ? AnalyticsConstants.PercentageMultiplier : 0;
            return ((double)(current - previous) / previous) * AnalyticsConstants.PercentageMultiplier;
        }

        private static double CalculateEngagementScore(int views, int reviews, int favorites)
        {
            if (views == 0) return 0;
            return ((double)(reviews * AnalyticsConstants.EngagementScoreMultiplier + favorites) / views) * AnalyticsConstants.PercentageMultiplier;
        }

        private static string DeterminePerformanceTrend(double viewsGrowth, int reviewsThisMonth, int reviewsLastMonth)
        {
            if (viewsGrowth > AnalyticsConstants.SignificantViewsChangeThreshold || reviewsThisMonth > reviewsLastMonth) return AnalyticsConstants.PerformanceTrends.Up;
            if (viewsGrowth < -AnalyticsConstants.SignificantViewsChangeThreshold || reviewsThisMonth < reviewsLastMonth) return AnalyticsConstants.PerformanceTrends.Down;
            return AnalyticsConstants.PerformanceTrends.Stable;
        }

        private static List<string> GenerateRecommendations(Business business, List<BusinessReview> reviews, List<FavoriteBusiness> favorites)
        {
            var recommendations = new List<string>();

            if (reviews.Count == 0)
                recommendations.Add("Encourage customers to leave reviews");
            
            if (business.ViewCount < AnalyticsConstants.GoodVisibilityThreshold)
                recommendations.Add("Add more photos and improve business description");
            
            if (reviews.Any() && reviews.Average(r => r.Rating) < AnalyticsConstants.GoodRatingThreshold)
                recommendations.Add("Address customer feedback to improve ratings");
            
            if (favorites.Count < AnalyticsConstants.MinFavoritesThreshold)
                recommendations.Add("Engage with customers to increase favorites");

            return recommendations;
        }

        private static List<string> GenerateKeyInsights(List<BusinessAnalyticsData> analytics)
        {
            var insights = new List<string>();
            
            if (analytics.Any(a => a.PerformanceTrend == "up"))
                insights.Add("Some businesses are showing positive growth trends");
            
            if (analytics.Any(a => a.AverageRating >= AnalyticsConstants.ExcellentRatingThreshold))
                insights.Add("You have businesses with excellent customer ratings");
            
            if (analytics.Sum(a => a.TotalReviews) > AnalyticsConstants.StrongEngagementThreshold)
                insights.Add("Strong customer engagement across your businesses");

            return insights;
        }

        private static List<string> GenerateActionableRecommendations(List<BusinessAnalyticsData> analytics)
        {
            var recommendations = new List<string>();
            
            if (analytics.Any(a => a.TotalReviews == 0))
                recommendations.Add("Focus on getting first reviews for businesses without any");
            
            if (analytics.Any(a => a.AverageRating < AnalyticsConstants.PoorRatingThreshold))
                recommendations.Add("Address quality issues for low-rated businesses");
            
            if (analytics.Average(a => a.TotalViews) < AnalyticsConstants.GoodVisibilityThreshold)
                recommendations.Add("Improve business visibility with better photos and descriptions");

            return recommendations;
        }

        private async Task<double> GetUserBusinessAverageRating(List<int> businessIds)
        {
            var reviews = await _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.IsActive)
                .ToListAsync();
            
            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        private static string DetermineOverallPerformance(double userViews, double avgViews, double userRating, double avgRating)
        {
            var viewsRatio = avgViews > 0 ? userViews / avgViews : 1;
            var ratingRatio = avgRating > 0 ? userRating / avgRating : 1;
            var overallRatio = (viewsRatio + ratingRatio) / 2;

            return overallRatio switch
            {
                > AnalyticsConstants.GoodPerformanceRatio => AnalyticsConstants.OverallPerformance.Above,
                < AnalyticsConstants.AveragePerformanceRatio => AnalyticsConstants.OverallPerformance.Below,
                _ => AnalyticsConstants.OverallPerformance.Average
            };
        }

        private static double CalculatePercentile(double value, IEnumerable<double> dataset)
        {
            var sortedData = dataset.OrderBy(x => x).ToList();
            if (!sortedData.Any()) return AnalyticsConstants.DefaultPercentile;

            var rank = sortedData.Count(x => x <= value);
            return (double)rank / sortedData.Count * AnalyticsConstants.PercentageMultiplier;
        }

        private static string DetermineMetricPerformance(double userValue, double avgValue)
        {
            if (avgValue == 0) return AnalyticsConstants.MetricPerformance.Average;
            
            var ratio = userValue / avgValue;
            return ratio switch
            {
                > AnalyticsConstants.ExcellentPerformanceRatio => AnalyticsConstants.MetricPerformance.Excellent,
                > AnalyticsConstants.GoodPerformanceRatio => AnalyticsConstants.MetricPerformance.Good,
                > AnalyticsConstants.AveragePerformanceRatio => AnalyticsConstants.MetricPerformance.Average,
                > AnalyticsConstants.BelowAveragePerformanceRatio => AnalyticsConstants.MetricPerformance.BelowAverage,
                _ => AnalyticsConstants.MetricPerformance.Poor
            };
        }

        private static string DetermineMarketPosition(double userRating, double competitorRating, int competitorCount)
        {
            if (competitorCount == 0) return AnalyticsConstants.MarketPosition.Leader;
            if (userRating > competitorRating + AnalyticsConstants.LeaderRatingDifference) return AnalyticsConstants.MarketPosition.Leader;
            if (userRating > competitorRating) return AnalyticsConstants.MarketPosition.Competitive;
            if (userRating > competitorRating - AnalyticsConstants.ChallengerRatingDifference) return AnalyticsConstants.MarketPosition.Challenger;
            return AnalyticsConstants.MarketPosition.Niche;
        }

        private static List<string> GenerateOpportunityAreas(string category, double userRating, double competitorRating)
        {
            var opportunities = new List<string>();
            
            if (userRating < competitorRating)
                opportunities.Add("Improve service quality to match competitor standards");
            
            opportunities.Add("Enhance online presence and customer engagement");
            opportunities.Add("Collect more customer reviews and feedback");
            
            return opportunities;
        }

        // ===== CHART DATA PROCESSING METHODS (Phase 2.2) =====

        /// <summary>
        /// Get pre-formatted views chart data for Chart.js
        /// </summary>
        public async Task<ViewsChartDataResponse> GetViewsChartDataAsync(string userId, int days = AnalyticsConstants.DefaultAnalyticsDays, string? platform = null)
        {
            try
            {
                var rawData = await GetViewsOverTimeByPlatformAsync(userId, days, platform);
                
                if (!rawData.Any())
                {
                    return CreateEmptyViewsChartData(days);
                }

                // Generate date labels
                var labels = GenerateDateLabels(days);
                
                // Group data by business
                var businessGroups = rawData
                    .GroupBy(d => d.BusinessName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Create datasets for each business
                var datasets = new List<ChartDataset>();
                var colors = GetChartColors();
                var colorIndex = 0;

                foreach (var businessGroup in businessGroups)
                {
                    var businessName = businessGroup.Key;
                    var businessData = businessGroup.Value;
                    var color = colors[colorIndex % colors.Length];
                    colorIndex++;

                    // Fill in missing dates with 0 views
                    var dataPoints = labels.Select(label =>
                    {
                        var date = ParseDateLabel(label, days);
                        var dataPoint = businessData.FirstOrDefault(d => d.Date.Date == date.Date);
                        return dataPoint?.Views ?? 0.0;
                    }).ToList();

                    datasets.Add(new ChartDataset
                    {
                        Label = businessName,
                        Data = dataPoints,
                        BorderColor = color,
                        BackgroundColor = color + AnalyticsConstants.ChartOpacity.Light,
                        Fill = false,
                        Tension = 0.4
                    });
                }

                var totalViews = rawData.Sum(d => d.Views);
                var averageViewsPerDay = days > 0 ? (double)totalViews / days : 0;

                return new ViewsChartDataResponse
                {
                    Labels = labels,
                    Datasets = datasets,
                    ChartType = "line",
                    TimeRange = $"{days} days",
                    TotalViews = totalViews,
                    AverageViewsPerDay = averageViewsPerDay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating views chart data for user {UserId}", userId);
                return CreateEmptyViewsChartData(days);
            }
        }

        /// <summary>
        /// Get pre-formatted reviews chart data for Chart.js
        /// </summary>
        public async Task<ReviewsChartDataResponse> GetReviewsChartDataAsync(string userId, int days = AnalyticsConstants.DefaultAnalyticsDays)
        {
            try
            {
                var rawData = await GetReviewsOverTimeAsync(userId, days);
                
                if (!rawData.Any())
                {
                    return CreateEmptyReviewsChartData(days);
                }

                // Generate date labels
                var labels = GenerateDateLabels(days);
                
                // Group data by business
                var businessGroups = rawData
                    .GroupBy(d => d.BusinessName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Create datasets for each business
                var datasets = new List<ChartDataset>();
                var colors = GetChartColors();
                var colorIndex = 0;

                foreach (var businessGroup in businessGroups)
                {
                    var businessName = businessGroup.Key;
                    var businessData = businessGroup.Value;
                    var color = colors[colorIndex % colors.Length];
                    colorIndex++;

                    // Fill in missing dates with 0 reviews
                    var dataPoints = labels.Select(label =>
                    {
                        var date = ParseDateLabel(label, days);
                        var dataPoint = businessData.FirstOrDefault(d => d.Date.Date == date.Date);
                        return dataPoint?.ReviewCount ?? 0.0;
                    }).ToList();

                    datasets.Add(new ChartDataset
                    {
                        Label = businessName,
                        Data = dataPoints,
                        BorderColor = color,
                        BackgroundColor = color + AnalyticsConstants.ChartOpacity.Heavy,
                        Fill = false,
                        BorderWidth = 1
                    });
                }

                var totalReviews = rawData.Sum(d => d.ReviewCount);
                var averageRating = rawData.Where(d => d.AverageRating > 0).Average(d => d.AverageRating);
                var averageReviewsPerDay = days > 0 ? (double)totalReviews / days : 0;

                return new ReviewsChartDataResponse
                {
                    Labels = labels,
                    Datasets = datasets,
                    ChartType = "bar",
                    TimeRange = $"{days} days",
                    TotalReviews = totalReviews,
                    AverageRating = averageRating,
                    AverageReviewsPerDay = averageReviewsPerDay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reviews chart data for user {UserId}", userId);
                return CreateEmptyReviewsChartData(days);
            }
        }

        // Helper methods for chart data processing
        private static List<string> GenerateDateLabels(int days)
        {
            var labels = new List<string>();
            var today = DateTime.UtcNow.Date;
            
            for (var i = days - 1; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                
                if (days <= 7)
                {
                    labels.Add(date.ToString(AnalyticsConstants.DateFormats.LongDate));
                }
                else if (days <= AnalyticsConstants.DefaultAnalyticsDays)
                {
                    labels.Add(date.ToString(AnalyticsConstants.DateFormats.ShortDate));
                }
                else
                {
                    labels.Add(date.ToString(AnalyticsConstants.DateFormats.ShortDate));
                }
            }
            
            return labels;
        }

        private static DateTime ParseDateLabel(string label, int days)
        {
            var today = DateTime.UtcNow.Date;
            
            // Find the date that corresponds to this label
            for (var i = days - 1; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var expectedLabel = days <= 7 ? date.ToString("ddd, MMM dd") : date.ToString("MMM dd");
                
                if (label == expectedLabel)
                {
                    return date;
                }
            }
            
            return today; // Fallback
        }

        private static string[] GetChartColors()
        {
            return new[]
            {
                AnalyticsConstants.ChartColors.LapisLazuli,
                AnalyticsConstants.ChartColors.CarolinaBlue,
                AnalyticsConstants.ChartColors.HunyadiYellow,
                AnalyticsConstants.ChartColors.OrangePantone,
                AnalyticsConstants.ChartColors.Charcoal,
                AnalyticsConstants.ChartColors.DarkGray,
            };
        }

        private static ViewsChartDataResponse CreateEmptyViewsChartData(int days)
        {
            var labels = GenerateDateLabels(days);
            return new ViewsChartDataResponse
            {
                Labels = labels,
                Datasets = new List<ChartDataset>
                {
                    new()
                    {
                        Label = "No Views Data",
                        Data = new List<double>(new double[labels.Count]),
                        BorderColor = AnalyticsConstants.ChartColors.LightGray,
                        BackgroundColor = AnalyticsConstants.ChartColors.LightGray + AnalyticsConstants.ChartOpacity.Light,
                        Fill = false
                    }
                },
                ChartType = "line",
                TimeRange = $"{days} days",
                TotalViews = 0,
                AverageViewsPerDay = 0
            };
        }

        private static ReviewsChartDataResponse CreateEmptyReviewsChartData(int days)
        {
            var labels = GenerateDateLabels(days);
            return new ReviewsChartDataResponse
            {
                Labels = labels,
                Datasets = new List<ChartDataset>
                {
                    new()
                    {
                        Label = "No Reviews Data",
                        Data = new List<double>(new double[labels.Count]),
                        BorderColor = AnalyticsConstants.ChartColors.LightGray,
                        BackgroundColor = AnalyticsConstants.ChartColors.LightGray + AnalyticsConstants.ChartOpacity.Heavy,
                        Fill = false,
                        BorderWidth = 1
                    }
                },
                ChartType = "bar",
                TimeRange = $"{days} days",
                TotalReviews = 0,
                AverageRating = 0,
                AverageReviewsPerDay = 0
            };
        }

        // ===== COMPARATIVE ANALYSIS METHODS (Phase 4.2) =====

        /// <summary>
        /// Main comparative analysis method that handles all comparison types
        /// </summary>
        public async Task<ComparativeAnalysisResponse> GetComparativeAnalysisAsync(string userId, ComparativeAnalysisRequest request)
        {
            try
            {
                // Validate user owns the business if specified
                if (request.BusinessId.HasValue)
                {
                    var business = await _context.Businesses
                        .FirstOrDefaultAsync(b => b.Id == request.BusinessId.Value && b.UserId == userId);
                    if (business == null)
                    {
                        throw new ArgumentException("Business not found or access denied");
                    }
                }

                // Calculate periods based on comparison type
                var (currentStart, currentEnd, previousStart, previousEnd) = CalculateComparisonPeriods(request);

                // Get data for both periods
                var currentPeriodData = await GetPeriodDataAsync(userId, currentStart, currentEnd, request.BusinessId, request.Platform);
                var previousPeriodData = await GetPeriodDataAsync(userId, previousStart, previousEnd, request.BusinessId, request.Platform);

                // Calculate comparison metrics
                var comparisonMetrics = CalculateComparisonMetrics(currentPeriodData, previousPeriodData);

                // Generate chart data
                var chartData = await GenerateComparativeChartDataAsync(userId, currentStart, currentEnd, previousStart, previousEnd, request.BusinessId, request.Platform);

                // Generate insights
                var insights = GenerateComparativeInsights(currentPeriodData, previousPeriodData, comparisonMetrics);

                // Get business-specific data if applicable
                BusinessComparisonData? businessData = null;
                if (request.BusinessId.HasValue)
                {
                    businessData = await GetBusinessComparisonDataAsync(userId, request.BusinessId.Value, currentPeriodData, previousPeriodData);
                }

                return new ComparativeAnalysisResponse
                {
                    ComparisonType = request.ComparisonType,
                    CurrentPeriod = currentPeriodData,
                    PreviousPeriod = previousPeriodData,
                    ComparisonMetrics = comparisonMetrics,
                    ChartData = chartData,
                    Insights = insights,
                    BusinessData = businessData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing comparative analysis for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get period-over-period comparison (week-over-week, month-over-month, etc.)
        /// </summary>
        public async Task<ComparativeAnalysisResponse> GetPeriodOverPeriodComparisonAsync(string userId, int? businessId = null, string comparisonType = "MonthOverMonth", string? platform = null)
        {
            var request = new ComparativeAnalysisRequest
            {
                ComparisonType = comparisonType,
                BusinessId = businessId,
                Platform = platform
            };

            return await GetComparativeAnalysisAsync(userId, request);
        }

        /// <summary>
        /// Get year-over-year comparison
        /// </summary>
        public async Task<ComparativeAnalysisResponse> GetYearOverYearComparisonAsync(string userId, int? businessId = null, string? platform = null)
        {
            var request = new ComparativeAnalysisRequest
            {
                ComparisonType = "YearOverYear",
                BusinessId = businessId,
                Platform = platform
            };

            return await GetComparativeAnalysisAsync(userId, request);
        }

        /// <summary>
        /// Get custom range comparison
        /// </summary>
        public async Task<ComparativeAnalysisResponse> GetCustomRangeComparisonAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? businessId = null, string? platform = null)
        {
            var request = new ComparativeAnalysisRequest
            {
                ComparisonType = "CustomRange",
                BusinessId = businessId,
                CurrentPeriodStart = currentStart,
                CurrentPeriodEnd = currentEnd,
                PreviousPeriodStart = previousStart,
                PreviousPeriodEnd = previousEnd,
                Platform = platform
            };

            return await GetComparativeAnalysisAsync(userId, request);
        }

        /// <summary>
        /// Calculate comparison periods based on request type
        /// </summary>
        private (DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd) CalculateComparisonPeriods(ComparativeAnalysisRequest request)
        {
            var now = DateTime.UtcNow.Date;

            switch (request.ComparisonType)
            {
                case "CustomRange":
                    return (
                        request.CurrentPeriodStart.Date,
                        request.CurrentPeriodEnd.Date,
                        request.PreviousPeriodStart!.Value.Date,
                        request.PreviousPeriodEnd!.Value.Date
                    );

                case "WeekOverWeek":
                    var weekStart = GetWeekStart(now);
                    return (
                        weekStart.AddDays(-7),
                        weekStart.AddDays(-1),
                        weekStart.AddDays(-14),
                        weekStart.AddDays(-8)
                    );

                case "MonthOverMonth":
                    var monthStart = new DateTime(now.Year, now.Month, 1);
                    return (
                        monthStart.AddMonths(-1),
                        monthStart.AddDays(-1),
                        monthStart.AddMonths(-2),
                        monthStart.AddMonths(-1).AddDays(-1)
                    );

                case "QuarterOverQuarter":
                    var quarterStart = GetQuarterStart(now);
                    return (
                        quarterStart.AddMonths(-3),
                        quarterStart.AddDays(-1),
                        quarterStart.AddMonths(-6),
                        quarterStart.AddMonths(-3).AddDays(-1)
                    );

                case "YearOverYear":
                    var yearStart = new DateTime(now.Year, 1, 1);
                    return (
                        yearStart.AddYears(-1),
                        now.AddDays(-1),
                        yearStart.AddYears(-2),
                        yearStart.AddYears(-1).AddDays(-1)
                    );

                default:
                    // Default to month-over-month
                    var defaultMonthStart = new DateTime(now.Year, now.Month, 1);
                    return (
                        defaultMonthStart.AddMonths(-1),
                        defaultMonthStart.AddDays(-1),
                        defaultMonthStart.AddMonths(-2),
                        defaultMonthStart.AddMonths(-1).AddDays(-1)
                    );
            }
        }

        /// <summary>
        /// Get data for a specific time period
        /// </summary>
        private async Task<PeriodData> GetPeriodDataAsync(string userId, DateTime startDate, DateTime endDate, int? businessId, string? platform)
        {
            var businessIds = businessId.HasValue 
                ? new List<int> { businessId.Value }
                : await _context.Businesses
                    .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                    .Select(b => b.Id)
                    .ToListAsync();

            if (!businessIds.Any())
            {
                return CreateEmptyPeriodData(startDate, endDate);
            }

            // Get views data
            var viewsQuery = _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId) && v.ViewedAt.Date >= startDate && v.ViewedAt.Date <= endDate);

            if (!string.IsNullOrEmpty(platform))
            {
                viewsQuery = viewsQuery.Where(v => v.Platform == platform);
            }

            var viewsData = await viewsQuery
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new { Date = g.Key, Views = g.Count() })
                .ToListAsync();

            // Get reviews data
            var reviewsQuery = _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.CreatedAt.Date >= startDate && r.CreatedAt.Date <= endDate && r.IsActive);

            var reviewsData = await reviewsQuery
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Reviews = g.Count(), Rating = g.Average(r => r.Rating) })
                .ToListAsync();

            // Get favorites data
            var favoritesQuery = _context.FavoriteBusinesses
                .Where(f => businessIds.Contains(f.BusinessId) && f.CreatedAt.Date >= startDate && f.CreatedAt.Date <= endDate);

            var favoritesData = await favoritesQuery
                .GroupBy(f => f.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Favorites = g.Count() })
                .ToListAsync();

            // Calculate totals and averages
            var totalViews = viewsData.Sum(v => v.Views);
            var totalReviews = reviewsData.Sum(r => r.Reviews);
            var totalFavorites = favoritesData.Sum(f => f.Favorites);
            var averageRating = reviewsData.Any() ? reviewsData.Average(r => r.Rating) : 0;
            var daysInPeriod = (endDate - startDate).Days + 1;

            // Find peak and low days
            var peakDay = viewsData.OrderByDescending(v => v.Views).FirstOrDefault();
            var lowDay = viewsData.OrderBy(v => v.Views).FirstOrDefault();

            return new PeriodData
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalViews = totalViews,
                TotalReviews = totalReviews,
                TotalFavorites = totalFavorites,
                AverageRating = averageRating,
                EngagementScore = CalculateEngagementScore(totalViews, totalReviews, totalFavorites),
                AverageViewsPerDay = daysInPeriod > 0 ? (double)totalViews / daysInPeriod : 0,
                AverageReviewsPerDay = daysInPeriod > 0 ? (double)totalReviews / daysInPeriod : 0,
                AverageFavoritesPerDay = daysInPeriod > 0 ? (double)totalFavorites / daysInPeriod : 0,
                PeakDayViews = peakDay?.Views ?? 0,
                PeakDayDate = peakDay?.Date,
                LowDayViews = lowDay?.Views ?? 0,
                LowDayDate = lowDay?.Date
            };
        }

        /// <summary>
        /// Calculate comparison metrics between two periods
        /// </summary>
        private static ComparisonMetrics CalculateComparisonMetrics(PeriodData current, PeriodData previous)
        {
            var viewsChange = CalculatePercentageChange(current.TotalViews, previous.TotalViews);
            var reviewsChange = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews);
            var favoritesChange = CalculatePercentageChange(current.TotalFavorites, previous.TotalFavorites);
            var ratingChange = CalculatePercentageChange(current.AverageRating, previous.AverageRating);
            var engagementChange = CalculatePercentageChange(current.EngagementScore, previous.EngagementScore);
            var avgViewsChange = CalculatePercentageChange(current.AverageViewsPerDay, previous.AverageViewsPerDay);
            var avgReviewsChange = CalculatePercentageChange(current.AverageReviewsPerDay, previous.AverageReviewsPerDay);
            var avgFavoritesChange = CalculatePercentageChange(current.AverageFavoritesPerDay, previous.AverageFavoritesPerDay);

            // Determine overall trend
            var positiveChanges = new[] { viewsChange, reviewsChange, favoritesChange, ratingChange, engagementChange }
                .Count(c => c > 0);
            var negativeChanges = new[] { viewsChange, reviewsChange, favoritesChange, ratingChange, engagementChange }
                .Count(c => c < 0);

            var overallTrend = positiveChanges > negativeChanges ? AnalyticsConstants.OverallTrend.Improving :
                              negativeChanges > positiveChanges ? AnalyticsConstants.OverallTrend.Declining : AnalyticsConstants.OverallTrend.Stable;

            // Determine performance rating
            var avgChange = (viewsChange + reviewsChange + favoritesChange + ratingChange + engagementChange) / AnalyticsConstants.MetricsCountForAveraging;
            var performanceRating = avgChange >= AnalyticsConstants.ExcellentChangeThreshold ? AnalyticsConstants.PerformanceRating.Excellent :
                                   avgChange >= AnalyticsConstants.GoodChangeThreshold ? AnalyticsConstants.PerformanceRating.Good :
                                   avgChange >= AnalyticsConstants.FairChangeThreshold ? AnalyticsConstants.PerformanceRating.Fair : AnalyticsConstants.PerformanceRating.Poor;

            // Generate key changes
            var keyChanges = new List<string>();
            if (Math.Abs(viewsChange) >= AnalyticsConstants.SignificantViewsChangeThreshold) keyChanges.Add($"Views: {(viewsChange > 0 ? "+" : "")}{viewsChange:F1}%");
            if (Math.Abs(reviewsChange) >= AnalyticsConstants.SignificantViewsChangeThreshold) keyChanges.Add($"Reviews: {(reviewsChange > 0 ? "+" : "")}{reviewsChange:F1}%");
            if (Math.Abs(favoritesChange) >= AnalyticsConstants.SignificantViewsChangeThreshold) keyChanges.Add($"Favorites: {(favoritesChange > 0 ? "+" : "")}{favoritesChange:F1}%");
            if (Math.Abs(ratingChange) >= AnalyticsConstants.SignificantRatingChangeThresholdForKeyChanges) keyChanges.Add($"Rating: {(ratingChange > 0 ? "+" : "")}{ratingChange:F1}%");

            return new ComparisonMetrics
            {
                ViewsChangePercent = viewsChange,
                ReviewsChangePercent = reviewsChange,
                FavoritesChangePercent = favoritesChange,
                RatingChangePercent = ratingChange,
                EngagementChangePercent = engagementChange,
                AverageViewsPerDayChangePercent = avgViewsChange,
                AverageReviewsPerDayChangePercent = avgReviewsChange,
                AverageFavoritesPerDayChangePercent = avgFavoritesChange,
                OverallTrend = overallTrend,
                PerformanceRating = performanceRating,
                KeyChanges = keyChanges
            };
        }

        /// <summary>
        /// Generate comparative chart data
        /// </summary>
        private async Task<ComparativeChartData> GenerateComparativeChartDataAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? businessId, string? platform)
        {
            var businessIds = businessId.HasValue 
                ? new List<int> { businessId.Value }
                : await _context.Businesses
                    .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                    .Select(b => b.Id)
                    .ToListAsync();

            if (!businessIds.Any())
            {
                return CreateEmptyComparativeChartData();
            }

            // Get daily data for both periods
            var currentData = await GetDailyDataForPeriodAsync(businessIds, currentStart, currentEnd, platform);
            var previousData = await GetDailyDataForPeriodAsync(businessIds, previousStart, previousEnd, platform);

            // Generate labels (use current period dates)
            var labels = GenerateDateLabelsForPeriod(currentStart, currentEnd);

            // Create datasets
            var datasets = new List<ComparativeChartDataset>
            {
                new()
                {
                    Label = "Current Period",
                    Data = currentData.Select(d => (double)d.Views).ToList(),
                    BorderColor = AnalyticsConstants.ChartColors.LapisLazuli,
                    BackgroundColor = AnalyticsConstants.ChartColors.LapisLazuli + AnalyticsConstants.ChartOpacity.Medium,
                    Fill = false,
                    BorderWidth = 2
                },
                new()
                {
                    Label = "Previous Period",
                    Data = previousData.Select(d => (double)d.Views).ToList(),
                    BorderColor = AnalyticsConstants.ChartColors.CarolinaBlue,
                    BackgroundColor = AnalyticsConstants.ChartColors.CarolinaBlue + AnalyticsConstants.ChartOpacity.Medium,
                    Fill = false,
                    BorderWidth = 2
                }
            };

            return new ComparativeChartData
            {
                Labels = labels,
                Datasets = datasets,
                ChartType = "line",
                TimeRange = $"{(currentEnd - currentStart).Days + 1} days"
            };
        }

        /// <summary>
        /// Get daily data for a specific period
        /// </summary>
        private async Task<List<DailyDataPoint>> GetDailyDataForPeriodAsync(List<int> businessIds, DateTime startDate, DateTime endDate, string? platform)
        {
            var viewsQuery = _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId) && v.ViewedAt.Date >= startDate && v.ViewedAt.Date <= endDate);

            if (!string.IsNullOrEmpty(platform))
            {
                viewsQuery = viewsQuery.Where(v => v.Platform == platform);
            }

            var data = await viewsQuery
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new DailyDataPoint { Date = g.Key, Views = g.Count() })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Fill in missing dates with 0 views
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .ToList();

            var result = new List<DailyDataPoint>();
            foreach (var date in allDates)
            {
                var existingData = data.FirstOrDefault(d => d.Date == date);
                result.Add(existingData ?? new DailyDataPoint { Date = date, Views = 0 });
            }

            return result;
        }

        /// <summary>
        /// Generate business comparison data
        /// </summary>
        private async Task<BusinessComparisonData> GetBusinessComparisonDataAsync(string userId, int businessId, PeriodData currentPeriod, PeriodData previousPeriod)
        {
            var business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);

            if (business == null)
            {
                throw new ArgumentException("Business not found");
            }

            // Get category comparison data
            var categoryComparison = await GetCategoryComparisonAsync(business.Category, currentPeriod);

            // Get portfolio comparison data
            var portfolioComparison = await GetPortfolioComparisonAsync(userId, businessId, currentPeriod);

            return new BusinessComparisonData
            {
                BusinessId = businessId,
                BusinessName = business.Name,
                Category = business.Category,
                CategoryComparison = categoryComparison,
                PortfolioComparison = portfolioComparison
            };
        }

        /// <summary>
        /// Get category comparison data
        /// </summary>
        private async Task<CategoryComparison> GetCategoryComparisonAsync(string category, PeriodData businessData)
        {
            // Get average data for the category (simplified - in real implementation, you'd have category benchmarks)
            var categoryBusinesses = await _context.Businesses
                .Where(b => b.Category == category && b.Status == "Active")
                .Select(b => b.Id)
                .ToListAsync();

            if (!categoryBusinesses.Any())
            {
                return new CategoryComparison();
            }

            // Calculate category averages (simplified - you'd use actual historical data)
            var categoryAverageViews = businessData.TotalViews * AnalyticsConstants.PlaceholderMultipliers.CategoryViewsMultiplier; // Placeholder
            var categoryAverageReviews = businessData.TotalReviews * AnalyticsConstants.PlaceholderMultipliers.CategoryReviewsMultiplier; // Placeholder
            var categoryAverageRating = businessData.AverageRating * AnalyticsConstants.PlaceholderMultipliers.CategoryRatingMultiplier; // Placeholder

            return new CategoryComparison
            {
                CategoryAverageViews = categoryAverageViews,
                CategoryAverageReviews = categoryAverageReviews,
                CategoryAverageRating = categoryAverageRating,
                ViewsVsCategory = CalculatePercentageDifference(businessData.TotalViews, categoryAverageViews),
                ReviewsVsCategory = CalculatePercentageDifference(businessData.TotalReviews, categoryAverageReviews),
                RatingVsCategory = CalculatePercentageDifference(businessData.AverageRating, categoryAverageRating),
                CategoryPerformance = businessData.TotalViews > categoryAverageViews ? AnalyticsConstants.CategoryPerformance.AboveAverage : AnalyticsConstants.CategoryPerformance.BelowAverage
            };
        }

        /// <summary>
        /// Get portfolio comparison data
        /// </summary>
        private async Task<PortfolioComparison> GetPortfolioComparisonAsync(string userId, int businessId, PeriodData businessData)
        {
            var userBusinesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .ToListAsync();

            if (userBusinesses.Count <= 1)
            {
                return new PortfolioComparison
                {
                    TotalPortfolioBusinesses = userBusinesses.Count
                };
            }

            // Calculate portfolio averages (simplified - you'd use actual historical data)
            var portfolioAverageViews = businessData.TotalViews * AnalyticsConstants.PlaceholderMultipliers.PortfolioViewsMultiplier; // Placeholder
            var portfolioAverageReviews = businessData.TotalReviews * AnalyticsConstants.PlaceholderMultipliers.PortfolioReviewsMultiplier; // Placeholder
            var portfolioAverageRating = businessData.AverageRating * AnalyticsConstants.PlaceholderMultipliers.PortfolioRatingMultiplier; // Placeholder

            // Calculate rank (simplified)
            var portfolioRank = 1; // Placeholder

            return new PortfolioComparison
            {
                PortfolioAverageViews = portfolioAverageViews,
                PortfolioAverageReviews = portfolioAverageReviews,
                PortfolioAverageRating = portfolioAverageRating,
                ViewsVsPortfolio = CalculatePercentageDifference(businessData.TotalViews, portfolioAverageViews),
                ReviewsVsPortfolio = CalculatePercentageDifference(businessData.TotalReviews, portfolioAverageReviews),
                RatingVsPortfolio = CalculatePercentageDifference(businessData.AverageRating, portfolioAverageRating),
                PortfolioRank = portfolioRank,
                TotalPortfolioBusinesses = userBusinesses.Count
            };
        }

        /// <summary>
        /// Generate insights based on comparison
        /// </summary>
        private static List<string> GenerateComparativeInsights(PeriodData current, PeriodData previous, ComparisonMetrics metrics)
        {
            var insights = new List<string>();

            if (metrics.ViewsChangePercent > AnalyticsConstants.SignificantViewsChangeThreshold)
                insights.Add($"Strong growth in views: +{metrics.ViewsChangePercent:F1}% increase");
            else if (metrics.ViewsChangePercent < -AnalyticsConstants.SignificantViewsChangeThreshold)
                insights.Add($"Significant decline in views: {metrics.ViewsChangePercent:F1}% decrease");

            if (metrics.ReviewsChangePercent > AnalyticsConstants.SignificantReviewsChangeThreshold)
                insights.Add($"Excellent review growth: +{metrics.ReviewsChangePercent:F1}% increase");
            else if (metrics.ReviewsChangePercent < -AnalyticsConstants.SignificantReviewsChangeThreshold)
                insights.Add($"Review activity declining: {metrics.ReviewsChangePercent:F1}% decrease");

            if (metrics.RatingChangePercent > AnalyticsConstants.SignificantRatingChangeThreshold)
                insights.Add($"Improved customer satisfaction: +{metrics.RatingChangePercent:F1}% rating increase");
            else if (metrics.RatingChangePercent < -AnalyticsConstants.SignificantRatingChangeThreshold)
                insights.Add($"Customer satisfaction declining: {metrics.RatingChangePercent:F1}% rating decrease");

            if (metrics.OverallTrend == AnalyticsConstants.OverallTrend.Improving)
                insights.Add("Overall performance is trending upward");
            else if (metrics.OverallTrend == AnalyticsConstants.OverallTrend.Declining)
                insights.Add("Performance is declining - consider reviewing strategies");

            if (current.PeakDayViews > previous.PeakDayViews)
                insights.Add($"Peak day performance improved: {current.PeakDayViews} vs {previous.PeakDayViews} views");

            return insights;
        }

        // Helper methods
        private static double CalculatePercentageChange(double current, double previous)
        {
            if (previous == 0) return current > 0 ? AnalyticsConstants.PercentageMultiplier : 0;
            return ((current - previous) / previous) * AnalyticsConstants.PercentageMultiplier;
        }

        private static double CalculatePercentageDifference(double value, double average)
        {
            if (average == 0) return 0;
            return ((value - average) / average) * AnalyticsConstants.PercentageMultiplier;
        }

        private static DateTime GetQuarterStart(DateTime date)
        {
            var quarter = (date.Month - 1) / 3;
            return new DateTime(date.Year, quarter * 3 + 1, 1);
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var daysSinceMonday = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7; // Handle Sunday
            return date.AddDays(-daysSinceMonday).Date;
        }

        private static PeriodData CreateEmptyPeriodData(DateTime startDate, DateTime endDate)
        {
            return new PeriodData
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }

        private static ComparativeChartData CreateEmptyComparativeChartData()
        {
            return new ComparativeChartData
            {
                Labels = new List<string>(),
                Datasets = new List<ComparativeChartDataset>(),
                ChartType = "line",
                TimeRange = "No data"
            };
        }

        private static List<string> GenerateDateLabelsForPeriod(DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var current = startDate;
            
            while (current <= endDate)
            {
                labels.Add(current.ToString(AnalyticsConstants.DateFormats.ShortDate));
                current = current.AddDays(1);
            }
            
            return labels;
        }

        private class DailyDataPoint
        {
            public DateTime Date { get; set; }
            public int Views { get; set; }
        }
    }
}