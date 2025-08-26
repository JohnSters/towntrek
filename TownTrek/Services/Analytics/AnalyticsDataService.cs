using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Service responsible for data access operations for analytics
    /// </summary>
    public class AnalyticsDataService(
        ApplicationDbContext context,
        ILogger<AnalyticsDataService> logger) : IAnalyticsDataService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<AnalyticsDataService> _logger = logger;

        public async Task<List<Business>> GetUserBusinessesAsync(string userId)
        {
            return await _context.Businesses
                .Where(b => b.UserId == userId && b.Status != AnalyticsConstants.BusinessStatus.Deleted)
                .Include(b => b.Town)
                .ToListAsync();
        }

        public async Task<List<BusinessReview>> GetBusinessReviewsAsync(List<int> businessIds, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!businessIds.Any()) return new List<BusinessReview>();

            var query = _context.BusinessReviews
                .Where(r => businessIds.Contains(r.BusinessId) && r.IsActive);

            if (startDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= endDate.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<FavoriteBusiness>> GetBusinessFavoritesAsync(List<int> businessIds, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!businessIds.Any()) return new List<FavoriteBusiness>();

            var query = _context.FavoriteBusinesses
                .Where(f => businessIds.Contains(f.BusinessId));

            if (startDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt <= endDate.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<BusinessViewLog>> GetBusinessViewLogsAsync(List<int> businessIds, DateTime? startDate = null, DateTime? endDate = null, string? platform = null)
        {
            if (!businessIds.Any()) return new List<BusinessViewLog>();

            var query = _context.BusinessViewLogs
                .Where(v => businessIds.Contains(v.BusinessId));

            if (startDate.HasValue)
            {
                query = query.Where(v => v.ViewedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(v => v.ViewedAt <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(platform) && platform != AnalyticsConstants.Platforms.All)
            {
                query = query.Where(v => v.Platform == platform);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Business>> GetCategoryBusinessesAsync(string category, int? excludeBusinessId = null)
        {
            var query = _context.Businesses
                .Where(b => b.Category == category && b.Status == AnalyticsConstants.BusinessStatus.Active);

            if (excludeBusinessId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBusinessId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Business>> GetCompetitorBusinessesAsync(int businessId, string category, string town)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null) return new List<Business>();

            return await _context.Businesses
                .Where(b => b.Id != businessId && 
                           b.Category == category && 
                           b.Town.Name == town && 
                           b.Status == AnalyticsConstants.BusinessStatus.Active)
                .ToListAsync();
        }

        public async Task<List<AnalyticsSnapshot>> GetAnalyticsSnapshotsAsync(int businessId, DateTime startDate, DateTime endDate)
        {
            return await _context.AnalyticsSnapshots
                .Where(s => s.BusinessId == businessId && 
                           s.SnapshotDate >= startDate.Date && 
                           s.SnapshotDate <= endDate.Date)
                .OrderBy(s => s.SnapshotDate)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<Business?> GetBusinessAsync(int businessId, string userId)
        {
            return await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        }
    }
}
