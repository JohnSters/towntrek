using Microsoft.AspNetCore.Identity;

namespace TownTrek.Extensions;

/// <summary>
/// Extension methods for enhanced logging throughout the application
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Log business operation with context
    /// </summary>
    public static void LogBusinessOperation(this ILogger logger, string operation, string? userId = null, object? data = null)
    {
        logger.LogInformation("Business Operation: {Operation} | User: {UserId} | Data: {@Data}", 
            operation, userId ?? "Anonymous", data);
    }

    /// <summary>
    /// Log security event
    /// </summary>
    public static void LogSecurityEvent(this ILogger logger, string eventType, string? userId = null, string? details = null)
    {
        logger.LogWarning("Security Event: {EventType} | User: {UserId} | Details: {Details}", 
            eventType, userId ?? "Anonymous", details);
    }

    /// <summary>
    /// Log authentication event
    /// </summary>
    public static void LogAuthenticationEvent(this ILogger logger, string eventType, string? userId = null, bool success = true)
    {
        if (success)
        {
            logger.LogInformation("Authentication Success: {EventType} | User: {UserId}", eventType, userId ?? "Anonymous");
        }
        else
        {
            logger.LogWarning("Authentication Failed: {EventType} | User: {UserId}", eventType, userId ?? "Anonymous");
        }
    }

    /// <summary>
    /// Log payment operation
    /// </summary>
    public static void LogPaymentOperation(this ILogger logger, string operation, string? userId = null, decimal? amount = null, string? transactionId = null)
    {
        logger.LogInformation("Payment Operation: {Operation} | User: {UserId} | Amount: {Amount} | TransactionId: {TransactionId}", 
            operation, userId ?? "Anonymous", amount, transactionId);
    }

    /// <summary>
    /// Log subscription event
    /// </summary>
    public static void LogSubscriptionEvent(this ILogger logger, string eventType, string? userId = null, string? subscriptionTier = null)
    {
        logger.LogInformation("Subscription Event: {EventType} | User: {UserId} | Tier: {SubscriptionTier}", 
            eventType, userId ?? "Anonymous", subscriptionTier);
    }

    /// <summary>
    /// Log data access operation
    /// </summary>
    public static void LogDataAccess(this ILogger logger, string operation, string entityType, string? entityId = null, string? userId = null)
    {
        logger.LogDebug("Data Access: {Operation} | Entity: {EntityType} | EntityId: {EntityId} | User: {UserId}", 
            operation, entityType, entityId, userId ?? "Anonymous");
    }

    /// <summary>
    /// Log performance metric
    /// </summary>
    public static void LogPerformance(this ILogger logger, string operation, TimeSpan duration, string? additionalInfo = null)
    {
        logger.LogInformation("Performance: {Operation} completed in {Duration}ms | Info: {AdditionalInfo}", 
            operation, duration.TotalMilliseconds, additionalInfo);
    }

    /// <summary>
    /// Log Identity result errors
    /// </summary>
    public static void LogIdentityErrors(this ILogger logger, IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Identity Operation Failed: {Operation} | Errors: {Errors}", operation, errors);
        }
    }

    /// <summary>
    /// Log external service call
    /// </summary>
    public static void LogExternalServiceCall(this ILogger logger, string serviceName, string operation, bool success, TimeSpan? duration = null, string? error = null)
    {
        if (success)
        {
            logger.LogInformation("External Service Call: {ServiceName}.{Operation} succeeded in {Duration}ms", 
                serviceName, operation, duration?.TotalMilliseconds);
        }
        else
        {
            logger.LogError("External Service Call: {ServiceName}.{Operation} failed | Error: {Error}", 
                serviceName, operation, error);
        }
    }
}