using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TownTrek.Constants;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Middleware
{
    public class TrialValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TrialValidationMiddleware> _logger;

        public TrialValidationMiddleware(RequestDelegate next, ILogger<TrialValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITrialService trialService, UserManager<ApplicationUser> userManager)
        {
            // Only check for authenticated users
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Check if user is in trial role
                    if (context.User.IsInRole(AppRoles.ClientTrial))
                    {
                        // Skip validation for certain paths to prevent infinite loops
                        var path = context.Request.Path.Value?.ToLower();
                        var skipPaths = new[] { "/auth/logout", "/client/subscription", "/api/", "/health" };
                        
                        if (!skipPaths.Any(skip => path?.StartsWith(skip) == true))
                        {
                            try
                            {
                                var isValid = await trialService.IsTrialValidAsync(userId);
                                if (!isValid)
                                {
                                    _logger.LogInformation("Trial expired for user {UserId}, redirecting to subscription page", userId);
                                    
                                    // For AJAX requests, return JSON response
                                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                        context.Request.Headers["Accept"].ToString().Contains("application/json"))
                                    {
                                        context.Response.StatusCode = 402; // Payment Required
                                        context.Response.ContentType = "application/json";
                                        await context.Response.WriteAsync("{\"error\":\"Trial expired\",\"redirect\":\"/Client/Subscription\"}");
                                        return;
                                    }
                                    
                                    // For regular requests, redirect to subscription page
                                    context.Response.Redirect("/Client/Subscription?expired=true");
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error validating trial for user {UserId}", userId);
                                // Continue with request on error to avoid breaking the app
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}