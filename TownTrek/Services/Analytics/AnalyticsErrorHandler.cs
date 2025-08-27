using TownTrek.Models.Exceptions;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics;

/// <summary>
/// Standardized error handling service for all analytics operations
/// </summary>
public class AnalyticsErrorHandler : IAnalyticsErrorHandler
{
    private readonly IAnalyticsErrorTracker _errorTracker;
    private readonly IAnalyticsEventService _eventService;
    private readonly ILogger<AnalyticsErrorHandler> _logger;

    public AnalyticsErrorHandler(
        IAnalyticsErrorTracker errorTracker,
        IAnalyticsEventService eventService,
        ILogger<AnalyticsErrorHandler> logger)
    {
        _errorTracker = errorTracker;
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Handles analytics exceptions with standardized logging and tracking
    /// </summary>
    public async Task HandleAnalyticsExceptionAsync(AnalyticsException exception, string userId, string operation, Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            // Merge contexts
            var context = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["UserId"] = userId,
                ["ErrorCode"] = exception.ErrorCode,
                ["ErrorCategory"] = exception.ErrorCategory
            };

            if (exception.Context != null)
            {
                foreach (var kvp in exception.Context)
                {
                    context[kvp.Key] = kvp.Value;
                }
            }

            if (additionalContext != null)
            {
                foreach (var kvp in additionalContext)
                {
                    context[kvp.Key] = kvp.Value;
                }
            }

            // Log the exception
            LogAnalyticsException(exception, userId, operation, context);

            // Track the error
            await _errorTracker.TrackErrorAsync(
                userId,
                exception.ErrorCategory,
                exception.Message,
                exception.StackTrace,
                context
            );

            // Record analytics error event
            await _eventService.RecordAnalyticsErrorEventAsync(userId, exception.ErrorCode, exception.Message, context);
        }
        catch (Exception trackingEx)
        {
            _logger.LogError(trackingEx, "Failed to handle analytics exception for UserId {UserId}, Operation {Operation}", userId, operation);
        }
    }

    /// <summary>
    /// Handles data access exceptions
    /// </summary>
    public async Task HandleDataExceptionAsync(Exception exception, string userId, string queryName, string? tableName = null, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsDataException(
            $"Data access error in {queryName}: {exception.Message}",
            queryName,
            tableName,
            context,
            exception
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, "DataAccess", context);
    }

    /// <summary>
    /// Handles validation exceptions
    /// </summary>
    public async Task HandleValidationExceptionAsync(string message, string userId, string validationField, string validationRule, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsValidationException(
            message,
            validationField,
            validationRule,
            context
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, "Validation", context);
    }

    /// <summary>
    /// Handles cache exceptions
    /// </summary>
    public async Task HandleCacheExceptionAsync(Exception exception, string userId, string cacheOperation, string? cacheKey = null, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsCacheException(
            $"Cache error in {cacheOperation}: {exception.Message}",
            cacheOperation,
            cacheKey,
            context,
            exception
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, "Cache", context);
    }

    /// <summary>
    /// Handles chart rendering exceptions
    /// </summary>
    public async Task HandleChartExceptionAsync(Exception exception, string userId, string chartType, string? chartId = null, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsChartException(
            $"Chart rendering error for {chartType}: {exception.Message}",
            chartType,
            chartId,
            context,
            exception
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, "Chart", context);
    }

    /// <summary>
    /// Handles export exceptions
    /// </summary>
    public async Task HandleExportExceptionAsync(Exception exception, string userId, string exportType, string? exportFormat = null, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsExportException(
            $"Export error for {exportType}: {exception.Message}",
            exportType,
            exportFormat,
            context,
            exception
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, "Export", context);
    }

    /// <summary>
    /// Handles permission exceptions
    /// </summary>
    public async Task HandlePermissionExceptionAsync(string message, string userId, string requiredPermission, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsPermissionException(
            message,
            requiredPermission,
            userId,
            context
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, "Permission", context);
    }

    /// <summary>
    /// Handles general exceptions with analytics context
    /// </summary>
    public async Task HandleGeneralExceptionAsync(Exception exception, string userId, string operation, Dictionary<string, object>? context = null)
    {
        var analyticsException = new AnalyticsException(
            $"General analytics error in {operation}: {exception.Message}",
            "ANALYTICS_GENERAL_ERROR",
            "General",
            context,
            exception
        );

        await HandleAnalyticsExceptionAsync(analyticsException, userId, operation, context);
    }

    /// <summary>
    /// Creates a safe operation wrapper that handles exceptions automatically
    /// </summary>
    public async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, string userId, string operationName, Dictionary<string, object>? context = null)
    {
        try
        {
            return await operation();
        }
        catch (AnalyticsException ex)
        {
            await HandleAnalyticsExceptionAsync(ex, userId, operationName, context);
            throw;
        }
        catch (Exception ex)
        {
            await HandleGeneralExceptionAsync(ex, userId, operationName, context);
            throw new AnalyticsException($"Operation {operationName} failed: {ex.Message}", "ANALYTICS_OPERATION_ERROR", "General", context, ex);
        }
    }

    /// <summary>
    /// Creates a safe operation wrapper for void operations
    /// </summary>
    public async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string userId, string operationName, Dictionary<string, object>? context = null)
    {
        try
        {
            await operation();
        }
        catch (AnalyticsException ex)
        {
            await HandleAnalyticsExceptionAsync(ex, userId, operationName, context);
            throw;
        }
        catch (Exception ex)
        {
            await HandleGeneralExceptionAsync(ex, userId, operationName, context);
            throw new AnalyticsException($"Operation {operationName} failed: {ex.Message}", "ANALYTICS_OPERATION_ERROR", "General", context, ex);
        }
    }

    private void LogAnalyticsException(AnalyticsException exception, string userId, string operation, Dictionary<string, object> context)
    {
        var logLevel = GetLogLevelForException(exception);
        
        _logger.Log(logLevel, exception, 
            "Analytics error: {ErrorCode} in {Operation} for UserId {UserId}. Category: {Category}. Context: {@Context}",
            exception.ErrorCode,
            operation,
            userId,
            exception.ErrorCategory,
            context
        );
    }

    private static LogLevel GetLogLevelForException(AnalyticsException exception)
    {
        return exception.ErrorCategory switch
        {
            "Permission" => LogLevel.Warning,
            "Validation" => LogLevel.Information,
            "Cache" => LogLevel.Warning,
            "DataAccess" => LogLevel.Error,
            "Chart" => LogLevel.Warning,
            "Export" => LogLevel.Warning,
            _ => LogLevel.Error
        };
    }
}
