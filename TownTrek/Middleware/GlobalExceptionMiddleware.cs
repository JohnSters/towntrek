using System.Net;
using System.Text.Json;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;

namespace TownTrek.Middleware;

/// <summary>
/// Global exception handling middleware for centralized error processing
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IServiceProvider _serviceProvider;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}, User: {User}",
                context.TraceIdentifier,
                context.Request.Path,
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous");

            // Log to database
            await LogErrorToDatabaseAsync(context, ex);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorViewModel
        {
            RequestId = context.TraceIdentifier,
            ShowDetails = _environment.IsDevelopment()
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Title = "Unauthorized";
                errorResponse.Message = "You are not authorized to access this resource.";
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Title = "Not Found";
                errorResponse.Message = "The requested resource was not found.";
                break;

            case ArgumentNullException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Title = "Bad Request";
                errorResponse.Message = "The request contains invalid data.";
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Title = "Invalid Operation";
                errorResponse.Message = "The requested operation is not valid in the current state.";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Title = "Internal Server Error";
                errorResponse.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        // Add detailed error information in development
        if (_environment.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
        }

        // Check if this is an API request
        if (IsApiRequest(context))
        {
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
        else
        {
            // For web requests, redirect to error page
            context.Items["ErrorViewModel"] = errorResponse;
            context.Response.Redirect($"/Error/{errorResponse.StatusCode}");
        }
    }

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api") ||
               context.Request.Headers.Accept.Any(h => h?.Contains("application/json") == true) ||
               context.Request.ContentType?.Contains("application/json") == true;
    }

    private async Task LogErrorToDatabaseAsync(HttpContext context, Exception exception)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var databaseErrorLogger = scope.ServiceProvider.GetService<IDatabaseErrorLogger>();
            
            if (databaseErrorLogger != null)
            {
                var (errorType, severity) = ErrorClassificationRules.ClassifyException(exception);
                
                var errorEntry = new ErrorLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    ErrorType = errorType,
                    Message = exception.Message,
                    StackTrace = exception.ToString(),
                    UserId = context.User?.Identity?.IsAuthenticated == true ? 
                             context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null,
                    RequestPath = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    IpAddress = GetClientIpAddress(context),
                    Severity = severity
                };

                await databaseErrorLogger.LogErrorAsync(errorEntry);
            }
        }
        catch (Exception ex)
        {
            // Don't let database logging errors crash the application
            _logger.LogError(ex, "Failed to log error to database");
        }
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}