using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.SharedAnalytics
{
    /// <summary>
    /// Service responsible for tracking analytics events for event sourcing
    /// </summary>
    public class AnalyticsEventService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AnalyticsEventService> logger) : IAnalyticsEventService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<AnalyticsEventService> _logger = logger;

        public async Task RecordAnalyticsEventAsync(string eventType, string userId, int? businessId = null, object? eventData = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var analyticsEvent = new AnalyticsEvent
                {
                    EventType = eventType,
                    UserId = userId,
                    BusinessId = businessId,
                    Platform = GetPlatformFromContext(httpContext),
                    IpAddress = GetIpAddressFromContext(httpContext),
                    UserAgent = GetUserAgentFromContext(httpContext),
                    SessionId = GetSessionIdFromContext(httpContext),
                    EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                    OccurredAt = DateTime.UtcNow,
                    RecordedAt = DateTime.UtcNow
                };

                _context.AnalyticsEvents.Add(analyticsEvent);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Analytics event recorded: {EventType} for UserId {UserId}, BusinessId {BusinessId}", 
                    eventType, userId, businessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record analytics event: {EventType} for UserId {UserId}", eventType, userId);
                // Don't throw - event recording should not break the main functionality
            }
        }

        public async Task RecordBusinessViewEventAsync(int businessId, string? userId = null, string? platform = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var analyticsEvent = new AnalyticsEvent
                {
                    EventType = "BusinessView",
                    UserId = userId,
                    BusinessId = businessId,
                    Platform = platform ?? GetPlatformFromContext(httpContext),
                    IpAddress = GetIpAddressFromContext(httpContext),
                    UserAgent = GetUserAgentFromContext(httpContext),
                    SessionId = GetSessionIdFromContext(httpContext),
                    OccurredAt = DateTime.UtcNow,
                    RecordedAt = DateTime.UtcNow
                };

                _context.AnalyticsEvents.Add(analyticsEvent);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Business view event recorded: BusinessId {BusinessId}, UserId {UserId}, Platform {Platform}", 
                    businessId, userId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record business view event: BusinessId {BusinessId}", businessId);
                // Don't throw - event recording should not break the main functionality
            }
        }

        public async Task RecordAnalyticsAccessEventAsync(string userId, string analyticsType, object? parameters = null)
        {
            var eventData = new
            {
                AnalyticsType = analyticsType,
                Parameters = parameters
            };

            await RecordAnalyticsEventAsync("AnalyticsAccess", userId, eventData: eventData);
        }

        public async Task RecordAnalyticsExportEventAsync(string userId, string exportType, object? parameters = null)
        {
            var eventData = new
            {
                ExportType = exportType,
                Parameters = parameters
            };

            await RecordAnalyticsEventAsync("AnalyticsExport", userId, eventData: eventData);
        }

        public async Task RecordAnalyticsErrorEventAsync(string userId, string errorType, string errorMessage, object? context = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var analyticsEvent = new AnalyticsEvent
                {
                    EventType = "AnalyticsError",
                    UserId = userId,
                    Platform = GetPlatformFromContext(httpContext),
                    IpAddress = GetIpAddressFromContext(httpContext),
                    UserAgent = GetUserAgentFromContext(httpContext),
                    SessionId = GetSessionIdFromContext(httpContext),
                    EventData = context != null ? JsonSerializer.Serialize(context) : null,
                    ErrorMessage = errorMessage,
                    Severity = "Error",
                    OccurredAt = DateTime.UtcNow,
                    RecordedAt = DateTime.UtcNow
                };

                _context.AnalyticsEvents.Add(analyticsEvent);
                await _context.SaveChangesAsync();

                _logger.LogError("Analytics error event recorded: {ErrorType} for UserId {UserId}: {ErrorMessage}", 
                    errorType, userId, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record analytics error event: {ErrorType} for UserId {UserId}", errorType, userId);
                // Don't throw - event recording should not break the main functionality
            }
        }

        public async Task<List<AnalyticsEvent>> GetAnalyticsEventsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.AnalyticsEvents
                .Where(e => e.UserId == userId && e.OccurredAt >= startDate && e.OccurredAt <= endDate)
                .OrderByDescending(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<List<AnalyticsEvent>> GetBusinessAnalyticsEventsAsync(int businessId, DateTime startDate, DateTime endDate)
        {
            return await _context.AnalyticsEvents
                .Where(e => e.BusinessId == businessId && e.OccurredAt >= startDate && e.OccurredAt <= endDate)
                .OrderByDescending(e => e.OccurredAt)
                .ToListAsync();
        }

        private string? GetPlatformFromContext(HttpContext? httpContext)
        {
            if (httpContext?.Request == null) return AnalyticsConstants.Platforms.Web;

            var userAgent = httpContext.Request.Headers.UserAgent.ToString().ToLower();
            
            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            {
                return AnalyticsConstants.Platforms.Mobile;
            }

            // Check if it's an API request
            if (httpContext.Request.Path.StartsWithSegments("/api"))
            {
                return AnalyticsConstants.Platforms.Api;
            }

            return AnalyticsConstants.Platforms.Web;
        }

        private string? GetIpAddressFromContext(HttpContext? httpContext)
        {
            if (httpContext?.Connection?.RemoteIpAddress == null) return null;

            // Handle IPv6 mapped to IPv4
            var ip = httpContext.Connection.RemoteIpAddress;
            if (ip?.IsIPv4MappedToIPv6 == true)
            {
                ip = ip.MapToIPv4();
            }

            return ip?.ToString();
        }

        private string? GetUserAgentFromContext(HttpContext? httpContext)
        {
            return httpContext?.Request.Headers.UserAgent.ToString();
        }

        private string? GetSessionIdFromContext(HttpContext? httpContext)
        {
            return httpContext?.Session?.Id;
        }
    }
}
