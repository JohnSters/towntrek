using Microsoft.Extensions.Options;

using TownTrek.Models.ViewModels;
using TownTrek.Options;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Analytics cache service implementation with specialized caching strategies
    /// </summary>
    public class AnalyticsCacheService : IAnalyticsCacheService
    {
        private readonly ICacheService _cacheService;
        private readonly IAnalyticsService _analyticsService;
        private readonly CacheOptions _cacheOptions;
        private readonly ILogger<AnalyticsCacheService> _logger;
        private readonly AnalyticsCacheStatistics _statistics;

        public AnalyticsCacheService(
            ICacheService cacheService,
            IAnalyticsService analyticsService,
            IOptions<CacheOptions> cacheOptions,
            ILogger<AnalyticsCacheService> logger)
        {
            _cacheService = cacheService;
            _analyticsService = analyticsService;
            _cacheOptions = cacheOptions.Value;
            _logger = logger;
            _statistics = new AnalyticsCacheStatistics();
        }

        public async Task<ClientAnalyticsViewModel?> GetClientAnalyticsAsync(string userId)
        {
            var cacheKey = $"client_analytics:{userId}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.DashboardExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.DashboardCacheMisses++;
                return await _analyticsService.GetClientAnalyticsAsync(userId);
            }, expiration);
        }

        public async Task<BusinessAnalyticsData?> GetBusinessAnalyticsAsync(int businessId, string userId)
        {
            var cacheKey = $"business_analytics:{businessId}:{userId}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.BusinessAnalyticsExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.BusinessAnalyticsCacheMisses++;
                return await _analyticsService.GetBusinessAnalyticsAsync(businessId, userId);
            }, expiration);
        }

        public async Task<ViewsChartDataResponse?> GetViewsChartDataAsync(string userId, int days, string? platform)
        {
            var cacheKey = $"views_chart:{userId}:{days}:{platform ?? "all"}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.ChartDataExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.ChartDataCacheMisses++;
                return await _analyticsService.GetViewsChartDataAsync(userId, days, platform);
            }, expiration);
        }

        public async Task<ReviewsChartDataResponse?> GetReviewsChartDataAsync(string userId, int days)
        {
            var cacheKey = $"reviews_chart:{userId}:{days}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.ChartDataExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.ChartDataCacheMisses++;
                return await _analyticsService.GetReviewsChartDataAsync(userId, days);
            }, expiration);
        }

        public async Task<List<ViewsOverTimeData>?> GetViewsOverTimeAsync(string userId, int days)
        {
            var cacheKey = $"views_over_time:{userId}:{days}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.ChartDataCacheMisses++;
                return await _analyticsService.GetViewsOverTimeAsync(userId, days);
            }, expiration);
        }

        public async Task<List<ReviewsOverTimeData>?> GetReviewsOverTimeAsync(string userId, int days)
        {
            var cacheKey = $"reviews_over_time:{userId}:{days}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.ChartDataCacheMisses++;
                return await _analyticsService.GetReviewsOverTimeAsync(userId, days);
            }, expiration);
        }

        public async Task<List<BusinessPerformanceInsight>?> GetPerformanceInsightsAsync(string userId)
        {
            var cacheKey = $"performance_insights:{userId}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.DashboardCacheMisses++;
                return await _analyticsService.GetPerformanceInsightsAsync(userId);
            }, expiration);
        }

        public async Task<CategoryBenchmarkData?> GetCategoryBenchmarksAsync(string userId, string category)
        {
            var cacheKey = $"category_benchmarks:{userId}:{category}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.DashboardCacheMisses++;
                return await _analyticsService.GetCategoryBenchmarksAsync(userId, category);
            }, expiration);
        }

        public async Task<List<CompetitorInsight>?> GetCompetitorInsightsAsync(string userId)
        {
            var cacheKey = $"competitor_insights:{userId}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes);

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _statistics.DashboardCacheMisses++;
                return await _analyticsService.GetCompetitorInsightsAsync(userId);
            }, expiration);
        }

        public async Task InvalidateUserAnalyticsAsync(string userId)
        {
            try
            {
                var patterns = new[]
                {
                    $"client_analytics:{userId}",
                    $"views_chart:{userId}:*",
                    $"reviews_chart:{userId}:*",
                    $"views_over_time:{userId}:*",
                    $"reviews_over_time:{userId}:*",
                    $"performance_insights:{userId}",
                    $"category_benchmarks:{userId}:*",
                    $"competitor_insights:{userId}"
                };

                foreach (var pattern in patterns)
                {
                    await _cacheService.RemoveByPatternAsync(pattern);
                }

                _logger.LogInformation("Invalidated all analytics cache for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating user analytics cache for user: {UserId}", userId);
            }
        }

        public async Task InvalidateBusinessAnalyticsAsync(int businessId)
        {
            try
            {
                var pattern = $"business_analytics:{businessId}:*";
                await _cacheService.RemoveByPatternAsync(pattern);

                _logger.LogInformation("Invalidated business analytics cache for business: {BusinessId}", businessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating business analytics cache for business: {BusinessId}", businessId);
            }
        }

        public async Task InvalidateChartDataAsync(string userId)
        {
            try
            {
                var patterns = new[]
                {
                    $"views_chart:{userId}:*",
                    $"reviews_chart:{userId}:*",
                    $"views_over_time:{userId}:*",
                    $"reviews_over_time:{userId}:*"
                };

                foreach (var pattern in patterns)
                {
                    await _cacheService.RemoveByPatternAsync(pattern);
                }

                _logger.LogInformation("Invalidated chart data cache for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating chart data cache for user: {UserId}", userId);
            }
        }

        public async Task WarmUpAnalyticsCacheAsync(List<string> userIds)
        {
            try
            {
                var warmUpTasks = new List<Task>();

                foreach (var userId in userIds)
                {
                    // Warm up dashboard data
                    warmUpTasks.Add(GetClientAnalyticsAsync(userId));
                    
                    // Warm up chart data for common time ranges
                    warmUpTasks.Add(GetViewsChartDataAsync(userId, 30, null));
                    warmUpTasks.Add(GetViewsChartDataAsync(userId, 7, null));
                    warmUpTasks.Add(GetReviewsChartDataAsync(userId, 30));
                    warmUpTasks.Add(GetReviewsChartDataAsync(userId, 7));
                    
                    // Warm up performance insights
                    warmUpTasks.Add(GetPerformanceInsightsAsync(userId));
                }

                await Task.WhenAll(warmUpTasks);
                _statistics.LastWarmUp = DateTime.UtcNow;

                _logger.LogInformation("Warmed up analytics cache for {UserCount} users", userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up analytics cache for {UserCount} users", userIds.Count);
            }
        }

        public Task<AnalyticsCacheStatistics> GetAnalyticsCacheStatisticsAsync()
        {
            try
            {
                // Calculate overall hit rate
                var totalHits = _statistics.DashboardCacheHits + _statistics.ChartDataCacheHits + _statistics.BusinessAnalyticsCacheHits;
                var totalMisses = _statistics.DashboardCacheMisses + _statistics.ChartDataCacheMisses + _statistics.BusinessAnalyticsCacheMisses;
                var totalRequests = totalHits + totalMisses;

                _statistics.OverallHitRate = totalRequests > 0 ? (double)totalHits / totalRequests * 100 : 0;

                return Task.FromResult(_statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics cache statistics");
                return Task.FromResult(_statistics);
            }
        }
    }
}
