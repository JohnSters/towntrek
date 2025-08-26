using TownTrek.Models.Exceptions;

namespace TownTrek.Services.Interfaces;

/// <summary>
/// Interface for standardized analytics error handling
/// </summary>
public interface IAnalyticsErrorHandler
{
    /// <summary>
    /// Handles analytics exceptions with standardized logging and tracking
    /// </summary>
    Task HandleAnalyticsExceptionAsync(AnalyticsException exception, string userId, string operation, Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Handles data access exceptions
    /// </summary>
    Task HandleDataExceptionAsync(Exception exception, string userId, string queryName, string? tableName = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Handles validation exceptions
    /// </summary>
    Task HandleValidationExceptionAsync(string message, string userId, string validationField, string validationRule, Dictionary<string, object>? context = null);

    /// <summary>
    /// Handles cache exceptions
    /// </summary>
    Task HandleCacheExceptionAsync(Exception exception, string userId, string cacheOperation, string? cacheKey = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Handles chart rendering exceptions
    /// </summary>
    Task HandleChartExceptionAsync(Exception exception, string userId, string chartType, string? chartId = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Handles export exceptions
    /// </summary>
    Task HandleExportExceptionAsync(Exception exception, string userId, string exportType, string? exportFormat = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Handles permission exceptions
    /// </summary>
    Task HandlePermissionExceptionAsync(string message, string userId, string requiredPermission, Dictionary<string, object>? context = null);

    /// <summary>
    /// Handles general exceptions with analytics context
    /// </summary>
    Task HandleGeneralExceptionAsync(Exception exception, string userId, string operation, Dictionary<string, object>? context = null);

    /// <summary>
    /// Creates a safe operation wrapper that handles exceptions automatically
    /// </summary>
    Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, string userId, string operationName, Dictionary<string, object>? context = null);

    /// <summary>
    /// Creates a safe operation wrapper for void operations
    /// </summary>
    Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string userId, string operationName, Dictionary<string, object>? context = null);
}
