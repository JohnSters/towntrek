using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class DatabaseErrorLogger : IDatabaseErrorLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationLogger _appLogger;

        public DatabaseErrorLogger(ApplicationDbContext context, IApplicationLogger appLogger)
        {
            _context = context;
            _appLogger = appLogger;
        }

        public async Task LogErrorAsync(ErrorLogEntry entry)
        {
            try
            {
                _context.ErrorLogs.Add(entry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Fallback to file logging if database logging fails
                _appLogger.LogError(ex, "Failed to log error to database", entry.UserId);
            }
        }

        public async Task<ErrorLogStats> GetErrorStatsAsync(TimeSpan period)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(period);
                var last24Hours = DateTime.UtcNow.AddDays(-1);

                var errors = await _context.ErrorLogs
                    .Where(e => e.Timestamp >= cutoffDate)
                    .ToListAsync();

                var stats = new ErrorLogStats
                {
                    TotalErrors = errors.Count,
                    CriticalErrors = errors.Count(e => e.Severity == ErrorSeverity.Critical.ToString()),
                    UnresolvedErrors = errors.Count(e => !e.IsResolved),
                    CriticalErrorsLast24Hours = errors.Count(e => e.Timestamp >= last24Hours && e.Severity == ErrorSeverity.Critical.ToString()),
                    ErrorsByType = errors.GroupBy(e => e.ErrorType).ToDictionary(g => g.Key, g => g.Count()),
                    ErrorsBySeverity = errors.GroupBy(e => e.Severity).ToDictionary(g => g.Key, g => g.Count())
                };

                // Calculate daily trends for the last 7 days
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                var dailyErrors = await _context.ErrorLogs
                    .Where(e => e.Timestamp >= sevenDaysAgo)
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyErrorCount
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        CriticalCount = g.Count(e => e.Severity == ErrorSeverity.Critical.ToString())
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                stats.DailyTrends = dailyErrors;

                // Get recent errors for activity feed
                var recentErrors = await _context.ErrorLogs
                    .Include(e => e.User)
                    .Where(e => e.Timestamp >= last24Hours)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(10)
                    .Select(e => new RecentErrorActivity
                    {
                        Id = e.Id,
                        Timestamp = e.Timestamp,
                        ErrorType = e.ErrorType,
                        Message = e.Message.Length > 100 ? e.Message.Substring(0, 100) + "..." : e.Message,
                        Severity = e.Severity,
                        UserId = e.UserId,
                        UserName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : null,
                        IsResolved = e.IsResolved
                    })
                    .ToListAsync();

                stats.RecentErrors = recentErrors;

                return stats;
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, "Failed to get error statistics", null);
                return new ErrorLogStats(); // Return empty stats on failure
            }
        }

        public async Task<PagedResult<ErrorLogEntry>> GetErrorLogsAsync(ErrorLogFilter filter)
        {
            try
            {
                var query = _context.ErrorLogs.Include(e => e.User).Include(e => e.ResolvedByUser).AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.ErrorType))
                {
                    query = query.Where(e => e.ErrorType == filter.ErrorType);
                }

                if (!string.IsNullOrEmpty(filter.Severity))
                {
                    query = query.Where(e => e.Severity == filter.Severity);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(e => e.Timestamp >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(e => e.Timestamp <= filter.ToDate.Value);
                }

                if (filter.IsResolved.HasValue)
                {
                    query = query.Where(e => e.IsResolved == filter.IsResolved.Value);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(e => e.Message.Contains(filter.SearchTerm) ||
                                           (e.User != null && (e.User.FirstName + " " + e.User.LastName).Contains(filter.SearchTerm)));
                }

                // Apply sorting
                query = filter.SortBy.ToLower() switch
                {
                    "errortype" => filter.SortDescending ? query.OrderByDescending(e => e.ErrorType) : query.OrderBy(e => e.ErrorType),
                    "severity" => filter.SortDescending ? query.OrderByDescending(e => e.Severity) : query.OrderBy(e => e.Severity),
                    "isresolved" => filter.SortDescending ? query.OrderByDescending(e => e.IsResolved) : query.OrderBy(e => e.IsResolved),
                    _ => filter.SortDescending ? query.OrderByDescending(e => e.Timestamp) : query.OrderBy(e => e.Timestamp)
                };

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return new PagedResult<ErrorLogEntry>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, "Failed to get error logs", null);
                return new PagedResult<ErrorLogEntry>(); // Return empty result on failure
            }
        }

        public async Task<ErrorLogEntry?> GetErrorByIdAsync(long id)
        {
            try
            {
                return await _context.ErrorLogs
                    .Include(e => e.User)
                    .Include(e => e.ResolvedByUser)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, $"Failed to get error by ID: {id}", null);
                return null;
            }
        }

        public async Task MarkAsResolvedAsync(long id, string resolvedBy, string? notes = null)
        {
            try
            {
                var error = await _context.ErrorLogs.FindAsync(id);
                if (error != null)
                {
                    error.IsResolved = true;
                    error.ResolvedBy = resolvedBy;
                    error.ResolvedAt = DateTime.UtcNow;
                    error.Notes = notes;

                    await _context.SaveChangesAsync();
                    _appLogger.LogInformation($"Error {id} marked as resolved by {resolvedBy}", resolvedBy);
                }
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, $"Failed to mark error {id} as resolved", resolvedBy);
            }
        }

        public async Task MarkAsUnresolvedAsync(long id)
        {
            try
            {
                var error = await _context.ErrorLogs.FindAsync(id);
                if (error != null)
                {
                    error.IsResolved = false;
                    error.ResolvedBy = null;
                    error.ResolvedAt = null;
                    error.Notes = null;

                    await _context.SaveChangesAsync();
                    _appLogger.LogInformation($"Error {id} marked as unresolved", null);
                }
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, $"Failed to mark error {id} as unresolved", null);
            }
        }
    }
}