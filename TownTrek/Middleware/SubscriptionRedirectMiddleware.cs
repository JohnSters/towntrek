using System.Security.Claims;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Middleware
{
    public class SubscriptionRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SubscriptionRedirectMiddleware> _logger;

        public SubscriptionRedirectMiddleware(RequestDelegate next, ILogger<SubscriptionRedirectMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ISubscriptionAuthService subscriptionAuthService)
        {
            // Only apply to authenticated users accessing client routes
            if (context.User.Identity?.IsAuthenticated == true && 
                context.Request.Path.StartsWithSegments("/Client") &&
                !context.Request.Path.StartsWithSegments("/Client/Subscription") &&
                !context.Request.Path.StartsWithSegments("/Client/Profile"))
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var authResult = await subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                        
                        // If user has no subscription and is trying to access restricted areas
                        if (!authResult.HasActiveSubscription && 
                            (context.Request.Path.StartsWithSegments("/Client/Dashboard") ||
                             context.Request.Path.StartsWithSegments("/Client/ManageBusinesses") ||
                             context.Request.Path.StartsWithSegments("/Client/AddBusiness")))
                        {
                            // Allow access to dashboard with limited functionality
                            // Other routes will be handled by the RequireActiveSubscription attribute
                        }
                        
                        // If payment is required, redirect to payment (but not during AJAX requests or API calls)
                        if (authResult.HasActiveSubscription && 
                            !authResult.IsPaymentValid && 
                            !string.IsNullOrEmpty(authResult.RedirectUrl) &&
                            !context.Request.Path.StartsWithSegments("/Payment") &&
                            !context.Request.Headers.ContainsKey("X-Requested-With") &&
                            context.Request.Method == "GET")
                        {
                            _logger.LogInformation("Redirecting user {UserId} to payment due to status: {PaymentStatus}", 
                                userId, authResult.PaymentStatus);
                            
                            context.Response.Redirect(authResult.RedirectUrl);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in subscription redirect middleware for user {UserId}", userId);
                        // Continue processing - don't break the request flow
                    }
                }
            }

            await _next(context);
        }
    }

    public static class SubscriptionRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseSubscriptionRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SubscriptionRedirectMiddleware>();
        }
    }
}