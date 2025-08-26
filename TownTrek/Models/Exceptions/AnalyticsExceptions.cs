using System.Runtime.Serialization;

namespace TownTrek.Models.Exceptions;

/// <summary>
/// Base exception for all analytics-related errors
/// </summary>
[Serializable]
public class AnalyticsException : Exception
{
    public string ErrorCode { get; }
    public string ErrorCategory { get; }
    public Dictionary<string, object>? Context { get; }

    public AnalyticsException(string message, string errorCode = "ANALYTICS_ERROR", string errorCategory = "General", Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        ErrorCategory = errorCategory;
        Context = context;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
        info.AddValue(nameof(ErrorCategory), ErrorCategory);
    }
}

/// <summary>
/// Exception for analytics data access errors
/// </summary>
[Serializable]
public class AnalyticsDataException : AnalyticsException
{
    public string QueryName { get; }
    public string? TableName { get; }

    public AnalyticsDataException(string message, string queryName, string? tableName = null, Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, "ANALYTICS_DATA_ERROR", "DataAccess", context, innerException)
    {
        QueryName = queryName;
        TableName = tableName;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(QueryName), QueryName);
        info.AddValue(nameof(TableName), TableName);
    }
}

/// <summary>
/// Exception for analytics validation errors
/// </summary>
[Serializable]
public class AnalyticsValidationException : AnalyticsException
{
    public string ValidationField { get; }
    public string ValidationRule { get; }

    public AnalyticsValidationException(string message, string validationField, string validationRule, Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, "ANALYTICS_VALIDATION_ERROR", "Validation", context, innerException)
    {
        ValidationField = validationField;
        ValidationRule = validationRule;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ValidationField), ValidationField);
        info.AddValue(nameof(ValidationRule), ValidationRule);
    }
}

/// <summary>
/// Exception for analytics cache errors
/// </summary>
[Serializable]
public class AnalyticsCacheException : AnalyticsException
{
    public string CacheOperation { get; }
    public string? CacheKey { get; }

    public AnalyticsCacheException(string message, string cacheOperation, string? cacheKey = null, Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, "ANALYTICS_CACHE_ERROR", "Cache", context, innerException)
    {
        CacheOperation = cacheOperation;
        CacheKey = cacheKey;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(CacheOperation), CacheOperation);
        info.AddValue(nameof(CacheKey), CacheKey);
    }
}

/// <summary>
/// Exception for analytics chart rendering errors
/// </summary>
[Serializable]
public class AnalyticsChartException : AnalyticsException
{
    public string ChartType { get; }
    public string? ChartId { get; }

    public AnalyticsChartException(string message, string chartType, string? chartId = null, Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, "ANALYTICS_CHART_ERROR", "Chart", context, innerException)
    {
        ChartType = chartType;
        ChartId = chartId;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ChartType), ChartType);
        info.AddValue(nameof(ChartId), ChartId);
    }
}

/// <summary>
/// Exception for analytics export errors
/// </summary>
[Serializable]
public class AnalyticsExportException : AnalyticsException
{
    public string ExportType { get; }
    public string? ExportFormat { get; }

    public AnalyticsExportException(string message, string exportType, string? exportFormat = null, Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, "ANALYTICS_EXPORT_ERROR", "Export", context, innerException)
    {
        ExportType = exportType;
        ExportFormat = exportFormat;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ExportType), ExportType);
        info.AddValue(nameof(ExportFormat), ExportFormat);
    }
}

/// <summary>
/// Exception for analytics permission/authorization errors
/// </summary>
[Serializable]
public class AnalyticsPermissionException : AnalyticsException
{
    public string RequiredPermission { get; }
    public string UserId { get; }

    public AnalyticsPermissionException(string message, string requiredPermission, string userId, Dictionary<string, object>? context = null, Exception? innerException = null)
        : base(message, "ANALYTICS_PERMISSION_ERROR", "Permission", context, innerException)
    {
        RequiredPermission = requiredPermission;
        UserId = userId;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(RequiredPermission), RequiredPermission);
        info.AddValue(nameof(UserId), UserId);
    }
}
