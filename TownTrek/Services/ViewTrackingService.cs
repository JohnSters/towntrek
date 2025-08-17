using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

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

                _context.BusinessViewLogs.Add(viewLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Business view logged: BusinessId={BusinessId}, Platform={Platform}, UserId={UserId}", 
                    businessId, platform, userId ?? "anonymous");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging business view: BusinessId={BusinessId}, Platform={Platform}", 
                    businessId, platform);
                // Don't throw - view tracking should not break the user experience
            }
        }

        public async Task<ViewStatistics> GetViewStatisticsAsync(int businessId, DateTime startDate, DateTime endDate, string? platform = null)
        {
            try
            {
                var query = _context.BusinessViewLogs
                    .Where(v => v.BusinessId == businessId && v.ViewedAt >= startDate && v.ViewedAt <= endDate);

                // Apply platform filter if specified
                if (!string.IsNullOrEmpty(platform) && IsValidPlatform(platform))
                {
                    query = query.Where(v => v.Platform == platform);
                }

                var stats = await query
                    .GroupBy(v => v.Platform)
                    .Select(g => new
                    {
                        Platform = g.Key,
                        Count = g.Count(),
                        UniqueVisitors = g.Select(v => v.UserId ?? v.IpAddress).Distinct().Count(),
                        LastViewed = g.Max(v => v.ViewedAt)
                    })
                    .ToListAsync();

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
                var result = new List<DailyViews>();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayStats = dailyStats.Where(s => s.Date == date).ToList();
                    
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
