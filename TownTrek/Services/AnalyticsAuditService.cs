using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using TownTrek.Models;

namespace TownTrek.Services
{
    public class AnalyticsAuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AnalyticsAuditService> logger) : IAnalyticsAuditService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<AnalyticsAuditService> _logger = logger;

        public async Task LogAnalyticsAccessAsync(string userId, string action, string? businessId = null, string? platform = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;

                var auditLog = new AnalyticsAuditLog
                {
                    UserId = userId,
                    Action = action,
                    BusinessId = businessId,
                    Platform = platform,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    IsSuspicious = false
                };

                _context.AnalyticsAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Analytics access logged: User {UserId}, Action {Action}, Business {BusinessId}", 
                    userId, action, businessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log analytics access for user {UserId}", userId);
            }
        }

        public async Task LogDataExportAsync(string userId, string exportType, string? businessId = null, string? format = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;

                var auditLog = new AnalyticsAuditLog
                {
                    UserId = userId,
                    Action = "DataExport",
                    BusinessId = businessId,
                    ExportType = exportType,
                    Format = format,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    IsSuspicious = false
                };

                _context.AnalyticsAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Data export logged: User {UserId}, Type {ExportType}, Format {Format}, Business {BusinessId}", 
                    userId, exportType, format, businessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log data export for user {UserId}", userId);
            }
        }

        public async Task LogSuspiciousActivityAsync(string userId, string activity, string details, string ipAddress)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;

                var auditLog = new AnalyticsAuditLog
                {
                    UserId = userId,
                    Action = activity,
                    Details = details,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    IsSuspicious = true
                };

                _context.AnalyticsAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Suspicious activity logged: User {UserId}, Activity {Activity}, Details {Details}, IP {IpAddress}", 
                    userId, activity, details, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log suspicious activity for user {UserId}", userId);
            }
        }

        public async Task<List<AnalyticsAuditLog>> GetUserAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AnalyticsAuditLogs.Where(log => log.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate.Value);

            return await query.OrderByDescending(log => log.Timestamp).ToListAsync();
        }

        public async Task<List<AnalyticsAuditLog>> GetAllAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null)
        {
            var query = _context.AnalyticsAuditLogs.AsQueryable();

            if (userId != null)
                query = query.Where(log => log.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate.Value);

            return await query.OrderByDescending(log => log.Timestamp).ToListAsync();
        }

        public async Task<List<AnalyticsAuditLog>> GetSuspiciousActivityLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AnalyticsAuditLogs.Where(log => log.IsSuspicious);

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate.Value);

            return await query.OrderByDescending(log => log.Timestamp).ToListAsync();
        }

        public async Task<int> CleanupOldAuditLogsAsync(int retentionDays = 365)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var oldLogs = _context.AnalyticsAuditLogs.Where(log => log.Timestamp < cutoffDate);
            
            var count = await oldLogs.CountAsync();
            _context.AnalyticsAuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} old analytics audit logs older than {RetentionDays} days", 
                count, retentionDays);

            return count;
        }

        private static string GetClientIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null) return "unknown";

            // Check for forwarded headers (for proxy/load balancer scenarios)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Take the first IP in the chain
                return forwardedFor.Split(',')[0].Trim();
            }

            var forwarded = httpContext.Request.Headers["X-Forwarded"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }

            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
