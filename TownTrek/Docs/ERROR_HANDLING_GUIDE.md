# TownTrek Error Handling System

## Overview

This document describes the centralized error handling system implemented for the TownTrek ASP.NET 8 application.

## Architecture

### Server-Side Error Handling (Primary)

The error handling system follows ASP.NET Core best practices with these components:

1. **GlobalExceptionMiddleware** - Catches all unhandled exceptions
2. **ErrorController** - Handles error pages and API error responses
3. **Serilog Integration** - Structured logging to files
4. **Custom Error Views** - User-friendly error pages

### Client-Side Error Handling (Minimal)

Simple JavaScript error handler focused on user experience:

- `ClientErrorHandler.handleApiError()` - For AJAX call errors
- `ClientErrorHandler.showError()` - For user notifications
- `ClientErrorHandler.showSuccess()` - For success messages

## Configuration

### Serilog Configuration

Logs are written to the `Logs/` directory with:
- Daily rolling files
- 30-day retention (production)
- 7-day retention (development)
- 10MB file size limit with rollover

### Error Pages

- `/Error` - General error page
- `/Error/404` - Not found page
- `/Error/401` - Unauthorized page
- `/Error/403` - Forbidden page

## Usage Examples

### In Controllers/Services

```csharp
public class BusinessController : Controller
{
    private readonly IApplicationLogger _appLogger;

    public BusinessController(IApplicationLogger appLogger)
    {
        _appLogger = appLogger;
    }

    public async Task<IActionResult> CreateBusiness(BusinessViewModel model)
    {
        try
        {
            // Business logic
            _appLogger.LogBusinessOperation("CreateBusiness", User.Identity?.Name, model);
            return View(result);
        }
        catch (ArgumentException ex)
        {
            _appLogger.LogError(ex, "CreateBusiness validation failed", User.Identity?.Name);
            // This will be caught by GlobalExceptionMiddleware
            throw;
        }
    }
}
```

### In JavaScript

```javascript
// For AJAX calls
try {
    const response = await fetch('/api/businesses', {
        method: 'POST',
        body: JSON.stringify(data),
        headers: { 'Content-Type': 'application/json' }
    });
    
    if (!response.ok) {
        await ClientErrorHandler.handleApiError(response, 'Creating business');
        return;
    }
    
    ClientErrorHandler.showSuccess('Business created successfully!');
} catch (error) {
    await ClientErrorHandler.handleApiError(error, 'Creating business');
}
```

### Application Logger Usage

```csharp
// Inject IApplicationLogger instead of ILogger
private readonly IApplicationLogger _appLogger;

// Business operations
_appLogger.LogBusinessOperation("UpdateBusiness", userId, businessData);

// Security events
_appLogger.LogSecurityEvent("UnauthorizedAccess", userId, "Attempted to access admin area");

// Authentication
_appLogger.LogAuthenticationEvent("Login", userId, success: true);

// Payment operations
_appLogger.LogPaymentOperation("ProcessPayment", userId, amount, transactionId);

// Errors with context
_appLogger.LogError(exception, "Payment processing failed", userId);

// Warnings and info
_appLogger.LogWarning("Subscription expiring soon", userId);
_appLogger.LogInformation("User profile updated", userId);
```

## Error Types Handled

### Server-Side Exceptions

- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found
- `ArgumentException`/`ArgumentNullException` → 400 Bad Request
- `InvalidOperationException` → 400 Bad Request
- All other exceptions → 500 Internal Server Error

### Client-Side Errors

- Network errors → User-friendly message
- HTTP status codes → Appropriate user messages
- Validation errors → Field-specific error display

## Testing

Use the test endpoints (remove in production):

- `/test-error/exception` - Test general exception
- `/test-error/not-found` - Test 404 error
- `/test-error/unauthorized` - Test 401 error
- `/test-error/argument` - Test argument error
- `/test-error/api/exception` - Test API exception (returns JSON)

## Log Files

Logs are stored in:
- `Logs/towntrek-YYYY-MM-DD.log` (production)
- `Logs/towntrek-dev-YYYY-MM-DD.log` (development)

### Log File Management
- **File Size**: Limited to 5MB per file (10MB in production)
- **Retention**: 7 days (development), 30 days (production), 90 days (production with high traffic)
- **Rotation**: Daily rotation prevents files from growing too large
- **Auto-cleanup**: Old files automatically deleted

### What Gets Logged

**Production (Minimal)**:
- Application errors and exceptions
- Security events (failed logins, unauthorized access)
- Business operations (payments, subscriptions)
- Critical system events

**Development (More Verbose)**:
- All production logs plus
- Debug information for TownTrek namespace
- Some Entity Framework warnings
- MVC action execution (warnings only)

**Filtered Out**:
- Static file requests
- Routine HTTP requests
- Entity Framework SQL queries (unless errors)
- ASP.NET Core infrastructure noise

## Security Considerations

- Detailed error information only shown in development
- No sensitive data exposed in client-side errors
- All errors logged with context (user, request ID, etc.)
- Request IDs for error tracking and support

## Maintenance

- Log files automatically rotate daily
- Old logs automatically deleted based on retention policy
- No manual cleanup required
- Monitor log file sizes and disk space

## Migration from Old System

The old complex JavaScript error handler has been replaced with:
- Server-side error handling (primary)
- Simple client-side error handling (UX only)
- Proper separation of concerns
- Industry-standard patterns

Remove the `TestErrorController` before deploying to production.

## Frequently Asked Questions

### Q1: Will log files grow exponentially?
**A: No.** The system is configured with:
- **File size limits**: 5MB per file (development), 10MB (production)
- **Daily rotation**: New file created each day
- **Automatic cleanup**: Old files deleted after retention period
- **Filtered logging**: Only relevant events are logged

### Q2: What does the log output help with vs console?
**A: Logs provide persistent, structured data that console doesn't:**
- **Persistence**: Logs survive application restarts
- **Production debugging**: Console isn't available in production
- **Audit trail**: Track user actions and system events over time
- **Error correlation**: Link errors to specific users/requests
- **Compliance**: Required for many business applications
- **Performance analysis**: Identify patterns and bottlenecks

### Q3: How can logging be further improved?
**A: Several enhancements are possible:**
- **Structured logging**: JSON format for better parsing
- **External services**: Send logs to Azure Application Insights, Seq, or ELK stack
- **Metrics**: Add performance counters and business metrics
- **Alerting**: Automatic notifications for critical errors
- **Log aggregation**: Centralized logging for multiple instances

### Q4: Does it catch all controller and service errors?
**A: Yes, the GlobalExceptionMiddleware catches:**
- ✅ All unhandled exceptions in controllers
- ✅ All unhandled exceptions in services
- ✅ All unhandled exceptions in middleware
- ✅ Database errors
- ✅ Network errors
- ✅ Authentication/authorization errors

**However, it does NOT catch:**
- ❌ Exceptions that are caught and handled in try-catch blocks
- ❌ Background service errors (unless they bubble up)
- ❌ Client-side JavaScript errors (handled separately)

### Q5: What's the difference between ILogger and IApplicationLogger?
**A: IApplicationLogger is focused and business-relevant:**
- **ILogger**: Generic, catches everything (noisy)
- **IApplicationLogger**: Business-focused, structured, meaningful
- **Better signal-to-noise ratio**: Only logs what matters
- **Consistent format**: All business events follow same pattern
- **Easier to search**: Prefixed categories (BUSINESS:, SECURITY:, etc.)

## Sample Log Output (After Optimization)

**Before (Noisy):**
```
2025-08-14 22:52:44.407 [INF] Microsoft.AspNetCore.Hosting.Diagnostics Request starting HTTP/2 GET https://localhost:44316/css/components/navigation.css
2025-08-14 22:52:44.415 [INF] Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware The file /css/components/navigation.css was not modified
```

**After (Clean):**
```
2025-08-14 22:52:44 [INF] BUSINESS: CreateBusiness | User: john@example.com | Data: {"Name":"New Business","Location":"Cape Town"}
2025-08-14 22:52:45 [WRN] SECURITY: UnauthorizedAccess | User: Anonymous | Details: Attempted to access admin area
2025-08-14 22:52:46 [ERR] ERROR: Payment processing failed | User: jane@example.com
```