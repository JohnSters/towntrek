# Database Error Logging System Design

## Overview

The database error logging system extends TownTrek's existing error handling infrastructure by adding persistent storage for critical errors in the database. This design maintains the current file-based logging while providing administrators with quick access to error data through the admin dashboard. The system focuses on capturing actionable error information and providing efficient querying and management capabilities.

## Architecture

### System Integration

The database error logging integrates with the existing error handling components:

```
┌─────────────────┐    ┌──────────────────────┐    ┌─────────────────┐
│   Application   │───▶│ GlobalException      │───▶│   File Logs     │
│   Exception     │    │ Middleware           │    │   (Existing)    │
└─────────────────┘    └──────────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌──────────────────────┐    ┌─────────────────┐
                       │ DatabaseErrorLogger  │───▶│   Database      │
                       │ Service              │    │   ErrorLogs     │
                       └──────────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌──────────────────────┐    ┌─────────────────┐
                       │ Admin Dashboard      │───▶│ Error Management│
                       │ Integration          │    │ Views           │
                       └──────────────────────┘    └─────────────────┘
```

### Database Schema

```sql
CREATE TABLE ErrorLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ErrorType NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    StackTrace NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NULL,
    RequestPath NVARCHAR(500) NULL,
    UserAgent NVARCHAR(1000) NULL,
    IpAddress NVARCHAR(45) NULL,
    Severity NVARCHAR(20) NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    ResolvedBy NVARCHAR(450) NULL,
    ResolvedAt DATETIME2 NULL,
    Notes NVARCHAR(MAX) NULL,
    
    INDEX IX_ErrorLogs_Timestamp (Timestamp DESC),
    INDEX IX_ErrorLogs_ErrorType (ErrorType),
    INDEX IX_ErrorLogs_Severity (Severity),
    INDEX IX_ErrorLogs_IsResolved (IsResolved),
    INDEX IX_ErrorLogs_UserId (UserId)
);
```

## Components and Interfaces

### 1. DatabaseErrorLogger Service

**Interface:**
```csharp
public interface IDatabaseErrorLogger
{
    Task LogErrorAsync(ErrorLogEntry entry);
    Task<ErrorLogStats> GetErrorStatsAsync(TimeSpan period);
    Task<PagedResult<ErrorLogEntry>> GetErrorLogsAsync(ErrorLogFilter filter);
    Task<ErrorLogEntry> GetErrorByIdAsync(long id);
    Task MarkAsResolvedAsync(long id, string resolvedBy, string notes = null);
    Task MarkAsUnresolvedAsync(long id);
}
```

**Implementation:**
- Handles database operations for error logging
- Provides querying and filtering capabilities
- Manages error resolution status
- Implements performance optimizations for large datasets

### 2. ErrorLogEntry Model

```csharp
public class ErrorLogEntry
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public ErrorType ErrorType { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string UserId { get; set; }
    public string RequestPath { get; set; }
    public string UserAgent { get; set; }
    public string IpAddress { get; set; }
    public ErrorSeverity Severity { get; set; }
    public bool IsResolved { get; set; }
    public string ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Notes { get; set; }
}

public enum ErrorType
{
    Exception,
    NotFound,
    Unauthorized,
    Argument,
    Api
}

public enum ErrorSeverity
{
    Warning,
    Error,
    Critical
}
```

### 3. Admin Dashboard Integration

**AdminDashboardViewModel Enhancement:**
```csharp
public class AdminDashboardViewModel
{
    // Existing properties...
    
    public int CriticalErrorsLast24Hours { get; set; }
    public int UnresolvedErrorsTotal { get; set; }
    public List<RecentErrorActivity> RecentErrors { get; set; }
    public ErrorTrendData ErrorTrends { get; set; }
}
```

### 4. Error Management Controller

```csharp
[Authorize(Roles = "Admin")]
public class AdminErrorsController : Controller
{
    public async Task<IActionResult> Index(ErrorLogFilter filter)
    public async Task<IActionResult> Details(long id)
    public async Task<IActionResult> Resolve(long id, string notes)
    public async Task<IActionResult> Unresolve(long id)
    public async Task<IActionResult> Export(ErrorLogFilter filter)
}
```

## Data Models

### ErrorLogFilter
```csharp
public class ErrorLogFilter
{
    public ErrorType? ErrorType { get; set; }
    public ErrorSeverity? Severity { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsResolved { get; set; }
    public string SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "Timestamp";
    public bool SortDescending { get; set; } = true;
}
```

### ErrorLogStats
```csharp
public class ErrorLogStats
{
    public int TotalErrors { get; set; }
    public int CriticalErrors { get; set; }
    public int UnresolvedErrors { get; set; }
    public Dictionary<ErrorType, int> ErrorsByType { get; set; }
    public Dictionary<ErrorSeverity, int> ErrorsBySeverity { get; set; }
    public List<DailyErrorCount> DailyTrends { get; set; }
}
```

## Error Handling

### Database Logging Failure Strategy

1. **Primary Strategy:** Continue application operation if database logging fails
2. **Fallback:** Log database errors to file system
3. **Recovery:** Retry database logging on next error occurrence
4. **Monitoring:** Track database logging health through existing file logs

### Error Classification Rules

```csharp
public static class ErrorClassificationRules
{
    public static (ErrorType Type, ErrorSeverity Severity) ClassifyException(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => (ErrorType.Unauthorized, ErrorSeverity.Error),
            KeyNotFoundException => (ErrorType.NotFound, ErrorSeverity.Warning),
            ArgumentException => (ErrorType.Argument, ErrorSeverity.Error),
            ArgumentNullException => (ErrorType.Argument, ErrorSeverity.Error),
            InvalidOperationException => (ErrorType.Exception, ErrorSeverity.Error),
            _ => (ErrorType.Exception, ErrorSeverity.Critical)
        };
    }
}
```

## Testing Strategy

### Unit Tests
- DatabaseErrorLogger service methods
- Error classification logic
- Model validation and mapping
- Filter and query logic

### Integration Tests
- Database operations with real database
- Error logging pipeline end-to-end
- Admin dashboard data integration
- Performance with large datasets

### UI Tests
- Admin dashboard error statistics display
- Error management view functionality
- Filtering and search operations
- Error resolution workflow

### Performance Tests
- Database logging performance impact
- Query performance with large error datasets
- Dashboard loading times with error statistics
- Concurrent error logging scenarios

## Security Considerations

### Data Protection
- Sanitize sensitive information from error messages
- Limit stack trace exposure in UI
- Secure admin-only access to error logs
- Audit trail for error resolution actions

### Access Control
- Role-based access (Admin only)
- Request validation for error management actions
- CSRF protection on resolution forms
- Rate limiting on error log queries

### Data Retention
- Automatic cleanup of old error logs (configurable retention period)
- Archive strategy for long-term error analysis
- GDPR compliance for user-related error data

## Performance Optimizations

### Database Performance
- Indexed queries for common filter scenarios
- Pagination for large result sets
- Async operations for all database calls
- Connection pooling optimization

### Caching Strategy
- Cache error statistics for dashboard (5-minute TTL)
- Cache error type/severity counts
- Invalidate cache on new critical errors

### Query Optimization
- Efficient filtering with proper indexes
- Limit result set sizes
- Use projection for list views
- Batch operations where applicable

## Monitoring and Alerting

### Health Checks
- Database connectivity for error logging
- Error logging service availability
- Performance metrics tracking

### Metrics Collection
- Error logging success/failure rates
- Database query performance
- Dashboard load times
- Error resolution rates

### Alerting Rules
- High error rates (>50 errors/hour)
- Database logging failures
- Unresolved critical errors (>24 hours old)
- Performance degradation alerts