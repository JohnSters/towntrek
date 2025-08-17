using Microsoft.EntityFrameworkCore;
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
                .Where(b => b.UserId == userId && b.Status != "Deleted")
                .Include(b => b.Town)
                .ToListAsync();

            var businessAnalytics = new List<BusinessAnalyticsData>();
            foreach (var business in businesses)
            {
                var analytics = await GetBusinessAnalyticsAsync(business.Id, userId);
                businessAnalytics.Add(analytics);
            }

            var overview = await GetAnalyticsOverviewAsync(userId, businessAnalytics);
            var viewsOverTime = await GetViewsOverTimeAsync(userId, 30);
            var reviewsOverTime = await GetReviewsOverTimeAsync(userId, 30);
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
            var growthRates = await _analyticsSnapshotService.CalculateGrowthRatesAsync(businessId, 30, 30);
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

        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeAsync(string userId, int days = 30)
        {
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != "Deleted")
                .ToListAsync();

            var data = new List<ViewsOverTimeData>();

            foreach (var business in businesses)
            {
                // Get real view data from ViewTrackingService
                var dailyViews = await _viewTrackingService.GetViewsOverTimeAsync(business.Id, days);
                
                foreach (var dailyView in dailyViews)
                {
                    data.Add(new ViewsOverTimeData
                    {
                        Date = dailyView.Date,
                        Views = dailyView.TotalViews,
                        BusinessId = business.Id,
                        BusinessName = business.Name
                    });
                }
            }

            return data.OrderBy(d => d.Date).ToList();
        }

        public async Task<List<ReviewsOverTimeData>> GetReviewsOverTimeAsync(string userId, int days = 30)
        {
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != "Deleted")
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
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != "Deleted")
                .Include(b => b.Town)
                .ToListAsync();

            var insights = new List<BusinessPerformanceInsight>();

            foreach (var business in businesses)
            {
                var reviews = await _context.BusinessReviews
                    .Where(r => r.BusinessId == business.Id && r.IsActive)
                    .ToListAsync();

                var favorites = await _context.FavoriteBusinesses
                    .Where(f => f.BusinessId == business.Id)
                    .CountAsync();

                // Generate insights based on performance
                if (reviews.Count == 0)
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
                else if (reviews.Average(r => r.Rating) < 3.0)
                {
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        InsightType = "warning",
                        Title = "Low Rating Alert",
                        Description = $"{business.Name} has an average rating of {reviews.Average(r => r.Rating):F1} stars.",
                        ActionRecommendation = "Review recent feedback and address customer concerns to improve ratings.",
                        Priority = 5
                    });
                }
                else if (reviews.Average(r => r.Rating) >= 4.5)
                {
                    insights.Add(new BusinessPerformanceInsight
                    {
                        BusinessId = business.Id,
                        BusinessName = business.Name,
                        InsightType = "success",
                        Title = "Excellent Performance",
                        Description = $"{business.Name} maintains an excellent {reviews.Average(r => r.Rating):F1} star rating.",
                        ActionRecommendation = "Keep up the great work! Consider highlighting positive reviews in marketing.",
                        Priority = 2
                    });
                }

                if (business.ViewCount < 50)
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

        public async Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category)
        {
            var userBusinesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Category == category && b.Status != "Deleted")
                .ToListAsync();

            if (!userBusinesses.Any()) return null;

            var categoryBusinesses = await _context.Businesses
                .Where(b => b.Category == category && b.Status == "Active")
                .ToListAsync();

            if (categoryBusinesses.Count < 5) return null; // Need enough data for meaningful benchmarks

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
        public async Task<List<ViewsOverTimeData>> GetViewsOverTimeByPlatformAsync(string userId, int days = 30, string? platform = null)
        {
            var businesses = await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != "Deleted")
                .ToListAsync();

            var data = new List<ViewsOverTimeData>();

            foreach (var business in businesses)
            {
                // Get platform-specific view data from ViewTrackingService
                var dailyViews = await _viewTrackingService.GetViewsOverTimeAsync(business.Id, days, platform);
                
                foreach (var dailyView in dailyViews)
                {
                    data.Add(new ViewsOverTimeData
                    {
                        Date = dailyView.Date,
                        Views = dailyView.TotalViews,
                        BusinessId = business.Id,
                        BusinessName = business.Name
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
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((double)(current - previous) / previous) * 100;
        }

        private static double CalculateEngagementScore(int views, int reviews, int favorites)
        {
            if (views == 0) return 0;
            return ((double)(reviews * 2 + favorites) / views) * 100;
        }

        private static string DeterminePerformanceTrend(double viewsGrowth, int reviewsThisMonth, int reviewsLastMonth)
        {
            if (viewsGrowth > 10 || reviewsThisMonth > reviewsLastMonth) return "up";
            if (viewsGrowth < -10 || reviewsThisMonth < reviewsLastMonth) return "down";
            return "stable";
        }

        private static List<string> GenerateRecommendations(Business business, List<BusinessReview> reviews, List<FavoriteBusiness> favorites)
        {
            var recommendations = new List<string>();

            if (reviews.Count == 0)
                recommendations.Add("Encourage customers to leave reviews");
            
            if (business.ViewCount < 100)
                recommendations.Add("Add more photos and improve business description");
            
            if (reviews.Any() && reviews.Average(r => r.Rating) < 4.0)
                recommendations.Add("Address customer feedback to improve ratings");
            
            if (favorites.Count < 5)
                recommendations.Add("Engage with customers to increase favorites");

            return recommendations;
        }

        private static List<string> GenerateKeyInsights(List<BusinessAnalyticsData> analytics)
        {
            var insights = new List<string>();
            
            if (analytics.Any(a => a.PerformanceTrend == "up"))
                insights.Add("Some businesses are showing positive growth trends");
            
            if (analytics.Any(a => a.AverageRating >= 4.5))
                insights.Add("You have businesses with excellent customer ratings");
            
            if (analytics.Sum(a => a.TotalReviews) > 50)
                insights.Add("Strong customer engagement across your businesses");

            return insights;
        }

        private static List<string> GenerateActionableRecommendations(List<BusinessAnalyticsData> analytics)
        {
            var recommendations = new List<string>();
            
            if (analytics.Any(a => a.TotalReviews == 0))
                recommendations.Add("Focus on getting first reviews for businesses without any");
            
            if (analytics.Any(a => a.AverageRating < 3.5))
                recommendations.Add("Address quality issues for low-rated businesses");
            
            if (analytics.Average(a => a.TotalViews) < 100)
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
                > 1.2 => "above",
                < 0.8 => "below",
                _ => "average"
            };
        }

        private static double CalculatePercentile(double value, IEnumerable<double> dataset)
        {
            var sortedData = dataset.OrderBy(x => x).ToList();
            if (!sortedData.Any()) return 50;

            var rank = sortedData.Count(x => x <= value);
            return (double)rank / sortedData.Count * 100;
        }

        private static string DetermineMetricPerformance(double userValue, double avgValue)
        {
            if (avgValue == 0) return "average";
            
            var ratio = userValue / avgValue;
            return ratio switch
            {
                > 1.5 => "excellent",
                > 1.2 => "good",
                > 0.8 => "average",
                > 0.5 => "below_average",
                _ => "poor"
            };
        }

        private static string DetermineMarketPosition(double userRating, double competitorRating, int competitorCount)
        {
            if (competitorCount == 0) return "leader";
            if (userRating > competitorRating + 0.5) return "leader";
            if (userRating > competitorRating) return "competitive";
            if (userRating > competitorRating - 0.5) return "challenger";
            return "niche";
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
    }
}