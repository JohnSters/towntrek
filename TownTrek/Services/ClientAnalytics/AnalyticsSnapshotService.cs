using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Service for managing analytics snapshots and historical data
    /// </summary>
    public class AnalyticsSnapshotService : IAnalyticsSnapshotService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsSnapshotService> _logger;

        public AnalyticsSnapshotService(
            ApplicationDbContext context,
            ILogger<AnalyticsSnapshotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CreateDailySnapshotsAsync(DateTime? snapshotDate = null)
        {
            var date = snapshotDate ?? DateTime.UtcNow.Date.AddDays(-1); // Default to yesterday
            var snapshotsCreated = 0;

            try
            {
                _logger.LogInformation("Starting daily snapshot creation for {Date}", date);

                // Get all active businesses with their current metrics in a single query
                var businessMetrics = await GetBusinessMetricsForDateAsync(date);
                
                // Batch create snapshots for better performance
                var snapshotsToAdd = new List<AnalyticsSnapshot>();
                
                foreach (var metric in businessMetrics)
                {
                    // Check if snapshot already exists
                    var existingSnapshot = await _context.AnalyticsSnapshots
                        .FirstOrDefaultAsync(s => s.BusinessId == metric.BusinessId && s.SnapshotDate == date);

                    if (existingSnapshot == null)
                    {
                        var snapshot = new AnalyticsSnapshot
                        {
                            BusinessId = metric.BusinessId,
                            SnapshotDate = date,
                            TotalViews = metric.DailyViews,
                            TotalReviews = metric.DailyReviews,
                            TotalFavorites = metric.DailyFavorites,
                            AverageRating = metric.AverageRating,
                            EngagementScore = CalculateEngagementScore(metric.DailyViews, metric.DailyReviews, metric.DailyFavorites, metric.AverageRating),
                            CreatedAt = DateTime.UtcNow
                        };

                        snapshotsToAdd.Add(snapshot);
                        snapshotsCreated++;
                    }
                }

                // Batch insert all snapshots
                if (snapshotsToAdd.Any())
                {
                    _context.AnalyticsSnapshots.AddRange(snapshotsToAdd);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Created {Count} daily snapshots for {Date}", snapshotsCreated, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily snapshots for {Date}", date);
            }

            return snapshotsCreated;
        }

        public async Task<AnalyticsSnapshot?> CreateBusinessSnapshotAsync(int businessId, DateTime snapshotDate)
        {
            try
            {
                // Check if snapshot already exists for this date
                var existingSnapshot = await _context.AnalyticsSnapshots
                    .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.SnapshotDate == snapshotDate.Date);

                if (existingSnapshot != null)
                {
                    _logger.LogWarning("Snapshot already exists for business {BusinessId} on {Date}", businessId, snapshotDate);
                    return existingSnapshot;
                }

                // Get business data
                var business = await _context.Businesses
                    .FirstOrDefaultAsync(b => b.Id == businessId);

                if (business == null)
                {
                    _logger.LogWarning("Business {BusinessId} not found for snapshot", businessId);
                    return null;
                }

                // Calculate daily views from BusinessViewLog
                var dailyViews = await _context.BusinessViewLogs
                    .Where(v => v.BusinessId == businessId && v.ViewedAt.Date == snapshotDate.Date)
                    .CountAsync();

                // Calculate daily reviews
                var dailyReviews = await _context.BusinessReviews
                    .Where(r => r.BusinessId == businessId && r.CreatedAt.Date == snapshotDate.Date && r.IsActive)
                    .CountAsync();

                // Calculate daily favorites
                var dailyFavorites = await _context.FavoriteBusinesses
                    .Where(f => f.BusinessId == businessId && f.CreatedAt.Date == snapshotDate.Date)
                    .CountAsync();

                // Calculate average rating for the day
                var dailyReviewsForRating = await _context.BusinessReviews
                    .Where(r => r.BusinessId == businessId && r.CreatedAt.Date == snapshotDate.Date && r.IsActive)
                    .ToListAsync();

                decimal? averageRating = null;
                if (dailyReviewsForRating.Any())
                {
                    averageRating = (decimal)dailyReviewsForRating.Average(r => r.Rating);
                }

                // Calculate engagement score (0-100)
                var engagementScore = CalculateEngagementScore(dailyViews, dailyReviews, dailyFavorites, averageRating);

                var snapshot = new AnalyticsSnapshot
                {
                    BusinessId = businessId,
                    SnapshotDate = snapshotDate.Date,
                    TotalViews = dailyViews,
                    TotalReviews = dailyReviews,
                    TotalFavorites = dailyFavorites,
                    AverageRating = averageRating,
                    EngagementScore = engagementScore,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AnalyticsSnapshots.Add(snapshot);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created snapshot for business {BusinessId} on {Date}: Views={Views}, Reviews={Reviews}, Favorites={Favorites}, Rating={Rating}, Engagement={Engagement}",
                    businessId, snapshotDate, dailyViews, dailyReviews, dailyFavorites, averageRating, engagementScore);

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating snapshot for business {BusinessId} on {Date}", businessId, snapshotDate);
                return null;
            }
        }

        public async Task<List<AnalyticsSnapshot>> GetBusinessSnapshotsAsync(int businessId, DateTime startDate, DateTime endDate)
        {
            return await _context.AnalyticsSnapshots
                .Where(s => s.BusinessId == businessId && s.SnapshotDate >= startDate.Date && s.SnapshotDate <= endDate.Date)
                .OrderBy(s => s.SnapshotDate)
                .ToListAsync();
        }

        public async Task<GrowthRateData> CalculateGrowthRatesAsync(int businessId, int currentPeriodDays = 30, int previousPeriodDays = 30)
        {
            var endDate = DateTime.UtcNow.Date;
            var currentPeriodStart = endDate.AddDays(-currentPeriodDays + 1);
            var previousPeriodStart = currentPeriodStart.AddDays(-previousPeriodDays);
            var previousPeriodEnd = currentPeriodStart.AddDays(-1);

            var currentPeriodSnapshots = await GetBusinessSnapshotsAsync(businessId, currentPeriodStart, endDate);
            var previousPeriodSnapshots = await GetBusinessSnapshotsAsync(businessId, previousPeriodStart, previousPeriodEnd);

            var currentPeriodViews = currentPeriodSnapshots.Sum(s => s.TotalViews);
            var previousPeriodViews = previousPeriodSnapshots.Sum(s => s.TotalViews);
            var currentPeriodReviews = currentPeriodSnapshots.Sum(s => s.TotalReviews);
            var previousPeriodReviews = previousPeriodSnapshots.Sum(s => s.TotalReviews);
            var currentPeriodFavorites = currentPeriodSnapshots.Sum(s => s.TotalFavorites);
            var previousPeriodFavorites = previousPeriodSnapshots.Sum(s => s.TotalFavorites);

            var currentPeriodRating = currentPeriodSnapshots.Any() ? currentPeriodSnapshots.Average(s => s.AverageRating ?? 0) : 0;
            var previousPeriodRating = previousPeriodSnapshots.Any() ? previousPeriodSnapshots.Average(s => s.AverageRating ?? 0) : 0;
            var currentPeriodEngagement = currentPeriodSnapshots.Any() ? currentPeriodSnapshots.Average(s => s.EngagementScore ?? 0) : 0;
            var previousPeriodEngagement = previousPeriodSnapshots.Any() ? previousPeriodSnapshots.Average(s => s.EngagementScore ?? 0) : 0;

            return new GrowthRateData
            {
                ViewsGrowthRate = CalculateGrowthRate(currentPeriodViews, previousPeriodViews),
                ReviewsGrowthRate = CalculateGrowthRate(currentPeriodReviews, previousPeriodReviews),
                FavoritesGrowthRate = CalculateGrowthRate(currentPeriodFavorites, previousPeriodFavorites),
                RatingGrowthRate = CalculateGrowthRate(currentPeriodRating, previousPeriodRating),
                EngagementGrowthRate = CalculateGrowthRate(currentPeriodEngagement, previousPeriodEngagement),
                CurrentPeriodViews = currentPeriodViews,
                PreviousPeriodViews = previousPeriodViews,
                CurrentPeriodReviews = currentPeriodReviews,
                PreviousPeriodReviews = previousPeriodReviews,
                CurrentPeriodFavorites = currentPeriodFavorites,
                PreviousPeriodFavorites = previousPeriodFavorites,
                CurrentPeriodRating = currentPeriodRating,
                PreviousPeriodRating = previousPeriodRating,
                CurrentPeriodEngagement = currentPeriodEngagement,
                PreviousPeriodEngagement = previousPeriodEngagement
            };
        }

        public async Task<int> CleanupOldSnapshotsAsync(int retentionDays = 730)
        {
            var cutoffDate = DateTime.UtcNow.Date.AddDays(-retentionDays);
            
            var oldSnapshots = await _context.AnalyticsSnapshots
                .Where(s => s.SnapshotDate < cutoffDate)
                .ToListAsync();

            if (oldSnapshots.Any())
            {
                _context.AnalyticsSnapshots.RemoveRange(oldSnapshots);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Cleaned up {Count} old snapshots older than {CutoffDate}", oldSnapshots.Count, cutoffDate);
                return oldSnapshots.Count;
            }

            return 0;
        }

        public async Task<List<AggregatedAnalyticsData>> GetAggregatedTrendsAsync(int businessId, string aggregationType, int months = 12)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddMonths(-months);

            var snapshots = await GetBusinessSnapshotsAsync(businessId, startDate, endDate);
            var aggregatedData = new List<AggregatedAnalyticsData>();

            if (aggregationType.ToLower() == "weekly")
            {
                // Group by weeks
                var weeklyGroups = snapshots
                    .GroupBy(s => GetWeekStart(s.SnapshotDate))
                    .OrderBy(g => g.Key);

                foreach (var group in weeklyGroups)
                {
                    aggregatedData.Add(new AggregatedAnalyticsData
                    {
                        PeriodStart = group.Key,
                        PeriodEnd = group.Key.AddDays(6),
                        PeriodLabel = $"Week of {group.Key:MMM dd}",
                        TotalViews = group.Sum(s => s.TotalViews),
                        TotalReviews = group.Sum(s => s.TotalReviews),
                        TotalFavorites = group.Sum(s => s.TotalFavorites),
                        AverageRating = group.Any(s => s.AverageRating.HasValue) ? group.Where(s => s.AverageRating.HasValue).Average(s => s.AverageRating!.Value) : 0,
                        AverageEngagement = group.Any(s => s.EngagementScore.HasValue) ? group.Where(s => s.EngagementScore.HasValue).Average(s => s.EngagementScore!.Value) : 0
                    });
                }
            }
            else if (aggregationType.ToLower() == "monthly")
            {
                // Group by months
                var monthlyGroups = snapshots
                    .GroupBy(s => new DateTime(s.SnapshotDate.Year, s.SnapshotDate.Month, 1))
                    .OrderBy(g => g.Key);

                foreach (var group in monthlyGroups)
                {
                    aggregatedData.Add(new AggregatedAnalyticsData
                    {
                        PeriodStart = group.Key,
                        PeriodEnd = group.Key.AddMonths(1).AddDays(-1),
                        PeriodLabel = group.Key.ToString("MMM yyyy"),
                        TotalViews = group.Sum(s => s.TotalViews),
                        TotalReviews = group.Sum(s => s.TotalReviews),
                        TotalFavorites = group.Sum(s => s.TotalFavorites),
                        AverageRating = group.Any(s => s.AverageRating.HasValue) ? group.Where(s => s.AverageRating.HasValue).Average(s => s.AverageRating!.Value) : 0,
                        AverageEngagement = group.Any(s => s.EngagementScore.HasValue) ? group.Where(s => s.EngagementScore.HasValue).Average(s => s.EngagementScore!.Value) : 0
                    });
                }
            }

            return aggregatedData;
        }

        private static decimal CalculateEngagementScore(int views, int reviews, int favorites, decimal? rating)
        {
            // Simple engagement score calculation (0-100)
            // Views: 40%, Reviews: 30%, Favorites: 20%, Rating: 10%
            var viewScore = Math.Min(views * 2, 40); // Max 40 points for views
            var reviewScore = Math.Min(reviews * 3, 30); // Max 30 points for reviews
            var favoriteScore = Math.Min(favorites * 4, 20); // Max 20 points for favorites
            var ratingScore = rating.HasValue ? Math.Min(rating.Value * 2, 10) : 0; // Max 10 points for rating

            return Math.Min(viewScore + reviewScore + favoriteScore + ratingScore, 100);
        }

        private static decimal CalculateGrowthRate(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0; // 100% growth if going from 0 to something, 0% if staying at 0
            }

            return (current - previous) / previous * 100;
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var daysSinceMonday = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7;
            return date.AddDays(-daysSinceMonday);
        }

        /// <summary>
        /// Optimized method to get all business metrics for a specific date in a single query
        /// </summary>
        private async Task<List<BusinessDailyMetrics>> GetBusinessMetricsForDateAsync(DateTime date)
        {
            var businessMetrics = new List<BusinessDailyMetrics>();

            try
            {
                // Get all active businesses with their daily metrics in optimized queries
                var activeBusinesses = await _context.Businesses
                    .Where(b => b.Status == "Active")
                    .Select(b => b.Id)
                    .ToListAsync();

                if (!activeBusinesses.Any())
                    return businessMetrics;

                // Get daily views for all businesses in one query
                var dailyViews = await _context.BusinessViewLogs
                    .Where(v => activeBusinesses.Contains(v.BusinessId) && v.ViewedAt.Date == date)
                    .GroupBy(v => v.BusinessId)
                    .Select(g => new { BusinessId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Get daily reviews for all businesses in one query
                var dailyReviews = await _context.BusinessReviews
                    .Where(r => activeBusinesses.Contains(r.BusinessId) && r.CreatedAt.Date == date && r.IsActive)
                    .GroupBy(r => r.BusinessId)
                    .Select(g => new { BusinessId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Get daily favorites for all businesses in one query
                var dailyFavorites = await _context.FavoriteBusinesses
                    .Where(r => activeBusinesses.Contains(r.BusinessId) && r.CreatedAt.Date == date)
                    .GroupBy(r => r.BusinessId)
                    .Select(g => new { BusinessId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Get average ratings for all businesses in one query
                var averageRatings = await _context.BusinessReviews
                    .Where(r => activeBusinesses.Contains(r.BusinessId) && r.CreatedAt.Date == date && r.IsActive)
                    .GroupBy(r => r.BusinessId)
                    .Select(g => new { BusinessId = g.Key, AverageRating = g.Average(r => r.Rating) })
                    .ToListAsync();

                // Combine all metrics
                foreach (var businessId in activeBusinesses)
                {
                    var views = dailyViews.FirstOrDefault(v => v.BusinessId == businessId)?.Count ?? 0;
                    var reviews = dailyReviews.FirstOrDefault(r => r.BusinessId == businessId)?.Count ?? 0;
                    var favorites = dailyFavorites.FirstOrDefault(f => f.BusinessId == businessId)?.Count ?? 0;
                    var rating = averageRatings.FirstOrDefault(r => r.BusinessId == businessId)?.AverageRating;

                    businessMetrics.Add(new BusinessDailyMetrics
                    {
                        BusinessId = businessId,
                        DailyViews = views,
                        DailyReviews = reviews,
                        DailyFavorites = favorites,
                        AverageRating = rating.HasValue ? (decimal?)rating.Value : null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting business metrics for date {Date}", date);
            }

            return businessMetrics;
        }

        /// <summary>
        /// Data transfer object for business daily metrics
        /// </summary>
        private class BusinessDailyMetrics
        {
            public int BusinessId { get; set; }
            public int DailyViews { get; set; }
            public int DailyReviews { get; set; }
            public int DailyFavorites { get; set; }
            public decimal? AverageRating { get; set; }
        }
    }
}
