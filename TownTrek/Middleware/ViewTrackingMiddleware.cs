using TownTrek.Services.Interfaces;

namespace TownTrek.Middleware
{
    public class ViewTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ViewTrackingMiddleware> _logger;

        public ViewTrackingMiddleware(RequestDelegate next, ILogger<ViewTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IViewTrackingService viewTrackingService)
        {
            // Process the request first
            await _next(context);

            // Only track views for successful GET requests to business detail pages
            if (context.Response.StatusCode == 200 && 
                context.Request.Method == "GET" && 
                IsBusinessDetailPage(context.Request.Path))
            {
                try
                {
                    await TrackBusinessViewAsync(context, viewTrackingService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error tracking business view in middleware");
                    // Don't let view tracking errors affect the user experience
                }
            }
        }

        private static bool IsBusinessDetailPage(PathString path)
        {
            // Check if this is a business detail page
            // Examples: /Public/Business/123, /Client/Businesses/Details/123
            return path.StartsWithSegments("/Public/Business") || 
                   path.StartsWithSegments("/Client/Businesses/Details");
        }

        private async Task TrackBusinessViewAsync(HttpContext context, IViewTrackingService viewTrackingService)
        {
            // Extract business ID from the URL
            var businessId = ExtractBusinessIdFromPath(context.Request.Path);
            if (businessId == null)
            {
                return;
            }

            // Determine platform based on user agent
            var platform = DeterminePlatform(context.Request.Headers.UserAgent.ToString());

            // Get user information
            var userId = context.User?.Identity?.IsAuthenticated == true 
                ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                : null;

            // Get session ID
            var sessionId = context.Session?.Id;

            // Get IP address
            var ipAddress = GetClientIpAddress(context);

            // Get referrer
            var referrer = context.Request.Headers.Referer.ToString();

            // Log the view
            await viewTrackingService.LogBusinessViewAsync(
                businessId.Value,
                userId,
                platform,
                ipAddress,
                context.Request.Headers.UserAgent.ToString(),
                referrer,
                sessionId
            );
        }

        private static int? ExtractBusinessIdFromPath(PathString path)
        {
            try
            {
                var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments == null || segments.Length < 3)
                    return null;

                // Handle different URL patterns:
                // /Public/Business/123
                // /Client/Businesses/Details/123
                if (segments[0] == "Public" && segments[1] == "Business" && segments.Length >= 3)
                {
                    return int.TryParse(segments[2], out var id) ? id : null;
                }
                
                if (segments[0] == "Client" && segments[1] == "Businesses" && segments[2] == "Details" && segments.Length >= 4)
                {
                    return int.TryParse(segments[3], out var id) ? id : null;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string DeterminePlatform(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Web";

            var ua = userAgent.ToLowerInvariant();

            // Check for mobile indicators
            if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone") || 
                ua.Contains("ipad") || ua.Contains("windows phone"))
            {
                return "Mobile";
            }

            // Check for API calls (custom user agents)
            if (ua.Contains("towntrek-api") || ua.Contains("ionic") || ua.Contains("angular"))
            {
                return "API";
            }

            return "Web";
        }

        private static string? GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers (when behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    // Extension method for easy registration
    public static class ViewTrackingMiddlewareExtensions
    {
        public static IApplicationBuilder UseViewTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ViewTrackingMiddleware>();
        }
    }
}
