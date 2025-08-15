using TownTrek.Services.Interfaces;

namespace TownTrek.Services;

/// <summary>
/// Application-specific logger that focuses on business-relevant events
/// </summary>
public class ApplicationLogger : IApplicationLogger
{
    private readonly ILogger<ApplicationLogger> _logger;

    public ApplicationLogger(ILogger<ApplicationLogger> logger)
    {
        _logger = logger;
    }

    public void LogBusinessOperation(string operation, string? userId = null, object? data = null)
    {
        _logger.LogInformation("BUSINESS: {Operation} | User: {UserId} | Data: {@Data}", 
            operation, userId ?? "Anonymous", data);
    }

    public void LogSecurityEvent(string eventType, string? userId = null, string? details = null)
    {
        _logger.LogWarning("SECURITY: {EventType} | User: {UserId} | Details: {Details}", 
            eventType, userId ?? "Anonymous", details);
    }

    public void LogAuthenticationEvent(string eventType, string? userId = null, bool success = true)
    {
        if (success)
        {
            _logger.LogInformation("AUTH_SUCCESS: {EventType} | User: {UserId}", eventType, userId ?? "Anonymous");
        }
        else
        {
            _logger.LogWarning("AUTH_FAILED: {EventType} | User: {UserId}", eventType, userId ?? "Anonymous");
        }
    }

    public void LogPaymentOperation(string operation, string? userId = null, decimal? amount = null, string? transactionId = null)
    {
        _logger.LogInformation("PAYMENT: {Operation} | User: {UserId} | Amount: {Amount} | TransactionId: {TransactionId}", 
            operation, userId ?? "Anonymous", amount, transactionId);
    }

    public void LogSubscriptionEvent(string eventType, string? userId = null, string? subscriptionTier = null)
    {
        _logger.LogInformation("SUBSCRIPTION: {EventType} | User: {UserId} | Tier: {SubscriptionTier}", 
            eventType, userId ?? "Anonymous", subscriptionTier);
    }

    public void LogError(Exception exception, string context, string? userId = null)
    {
        _logger.LogError(exception, "ERROR: {Context} | User: {UserId}", context, userId ?? "Anonymous");
    }

    public void LogWarning(string message, string? userId = null, object? data = null)
    {
        _logger.LogWarning("WARNING: {Message} | User: {UserId} | Data: {@Data}", 
            message, userId ?? "Anonymous", data);
    }

    public void LogInformation(string message, string? userId = null, object? data = null)
    {
        _logger.LogInformation("INFO: {Message} | User: {UserId} | Data: {@Data}", 
            message, userId ?? "Anonymous", data);
    }
}