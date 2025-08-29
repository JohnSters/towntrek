using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces.ClientAnalytics;

namespace TownTrek.Services
{
    public class ViewTrackingService : IViewTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ViewTrackingService> _logger;

        public ViewTrackingService(ApplicationDbContext context, ILogger<ViewTrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogBusinessViewAsync(int businessId, string? userId, string platform, string? ipAddress, string? userAgent, string? referrer, string? sessionId)
        {
            try
            {
                // Validate platform
                if (!IsValidPlatform(platform))
                {
                    platform = "Web"; // Default to Web if invalid
                }

                var viewLog = new BusinessViewLog
                {
                    BusinessId = businessId,
                    UserId = userId,
                    Platform = platform,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Referrer = referrer,
                    SessionId = sessionId,
                    ViewedAt = DateTime.UtcNow
                };

                // Use batch processing for better performance
                _context.BusinessViewLogs.Add(viewLog);
                
                // Only save immediately for critical views, otherwise let batch processing handle it
                if (ShouldSaveImmediately(userId, platform))
                {
                    await _context.SaveChangesAsync();
                    _logger.LogDebug("Business view logged immediately: BusinessId={BusinessId}, Platform={Platform}, UserId={UserId}", 
                        businessId, platform, userId ?? "anonymous");
                }
                else
                {
                    // For non-critical views, let the batch processor handle it
                    _logger.LogDebug("Business view queued for batch processing: BusinessId={BusinessId}, Platform={Platform}", 
                        businessId, platform);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging business view: BusinessId={BusinessId}, Platform={Platform}", 
                    businessId, platform);
                // Don't throw - view tracking should not break the user experience
            }
        }

        /// <summary>
        /// Determines if a view should be saved immediately or batched
        /// </summary>
        private static bool ShouldSaveImmediately(string? userId, string platform)
        {
            // Save immediately for:
            // - Authenticated users (important for user analytics)
            // - API views (critical for system integration)
            // - Mobile views (important for mobile analytics)
            return !string.IsNullOrEmpty(userId) || platform == "API" || platform == "Mobile";
        }

        public async Task<ViewStatistics> GetViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            try
            {
                // Optimized query to get all statistics in a single database call
                var query = _context.BusinessViewLogs
                    .Where(v => v.BusinessId == businessId && v.ViewedAt >= startDate && v.ViewedAt <= endDate);

                // Apply platform filter if specified
                if (!string.IsNullOrEmpty(platform) && IsValidPlatform(platform))
                {
                    query = query.Where(v => v.Platform == platform);
                }

                // Get all data in a single optimized query
                var viewData = await query
                    .Select(v => new
                    {
                        v.Platform,
                        v.UserId,
                        v.IpAddress,
                        v.ViewedAt
                    })
                    .ToListAsync();

                // Process data in memory for better performance
                var platformGroups = viewData.GroupBy(v => v.Platform).ToList();
                
                var stats = platformGroups.Select(g => new
                {
                    Platform = g.Key,
                    Count = g.Count(),
                    UniqueVisitors = g.Select(v => v.UserId ?? v.IpAddress).Distinct().Count(),
                    LastViewed = g.Max(v => v.ViewedAt)
                }).ToList();

                var totalViews = stats.Sum(s => s.Count);
                var totalDays = (endDate - startDate).Days + 1;

                return new ViewStatistics
                {
                    TotalViews = totalViews,
                    UniqueVisitors = stats.Sum(s => s.UniqueVisitors),
                    WebViews = stats.FirstOrDefault(s => s.Platform == "Web")?.Count ?? 0,
                    MobileViews = stats.FirstOrDefault(s => s.Platform == "Mobile")?.Count ?? 0,
                    ApiViews = stats.FirstOrDefault(s => s.Platform == "API")?.Count ?? 0,
                    AverageViewsPerDay = totalDays > 0 ? (double)totalViews / totalDays : 0,
                    LastViewed = stats.Max(s => s.LastViewed)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting view statistics: BusinessId={BusinessId}", businessId);
                return new ViewStatistics();
            }
        }

        public async Task<List<DailyViews>> GetViewsOverTimeAsync(int businessId, int days = 30, string? platform = null)
        {
            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days + 1);

                var query = _context.BusinessViewLogs
                    .Where(v => v.BusinessId == businessId && v.ViewedAt >= startDate && v.ViewedAt <= endDate);

                // Apply platform filter if specified
                if (!string.IsNullOrEmpty(platform) && IsValidPlatform(platform))
                {
                    query = query.Where(v => v.Platform == platform);
                }

                // Optimized query to get all daily stats in a single call
                var dailyStats = await query
                    .GroupBy(v => new { Date = v.ViewedAt.Date, Platform = v.Platform })
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        Platform = g.Key.Platform,
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Create a complete list of dates with zero values for missing dates
                // Use dictionary for faster lookups
                var statsLookup = dailyStats.ToLookup(s => s.Date);
                var result = new List<DailyViews>();
                
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayStats = statsLookup[date].ToList();
                    
                    result.Add(new DailyViews
                    {
                        Date = date,
                        TotalViews = dayStats.Sum(s => s.Count),
                        WebViews = dayStats.FirstOrDefault(s => s.Platform == "Web")?.Count ?? 0,
                        MobileViews = dayStats.FirstOrDefault(s => s.Platform == "Mobile")?.Count ?? 0,
                        ApiViews = dayStats.FirstOrDefault(s => s.Platform == "API")?.Count ?? 0
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views over time: BusinessId={BusinessId}, Days={Days}", businessId, days);
                return new List<DailyViews>();
            }
        }

        private static bool IsValidPlatform(string platform)
        {
            return platform switch
            {
                "Web" or "Mobile" or "API" => true,
                _ => false
            };
        }
    }
}
