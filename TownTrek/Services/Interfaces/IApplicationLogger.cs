namespace TownTrek.Services.Interfaces;

/// <summary>
/// Application-specific logging interface for business operations
/// </summary>
public interface IApplicationLogger
{
    /// <summary>
    /// Log business operation
    /// </summary>
    void LogBusinessOperation(string operation, string? userId = null, object? data = null);

    /// <summary>
    /// Log security event
    /// </summary>
    void LogSecurityEvent(string eventType, string? userId = null, string? details = null);

    /// <summary>
    /// Log authentication event
    /// </summary>
    void LogAuthenticationEvent(string eventType, string? userId = null, bool success = true);

    /// <summary>
    /// Log payment operation
    /// </summary>
    void LogPaymentOperation(string operation, string? userId = null, decimal? amount = null, string? transactionId = null);

    /// <summary>
    /// Log subscription event
    /// </summary>
    void LogSubscriptionEvent(string eventType, string? userId = null, string? subscriptionTier = null);

    /// <summary>
    /// Log error with context
    /// </summary>
    void LogError(Exception exception, string context, string? userId = null);

    /// <summary>
    /// Log warning
    /// </summary>
    void LogWarning(string message, string? userId = null, object? data = null);

    /// <summary>
    /// Log information
    /// </summary>
    void LogInformation(string message, string? userId = null, object? data = null);
}