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
    private readonly ILogger<BusinessController> _logger;

    public async Task<IActionResult> CreateBusiness(BusinessViewModel model)
    {
        try
        {
            // Business logic
            _logger.LogBusinessOperation("CreateBusiness", User.Identity.Name, model);
            return View(result);
        }
        catch (ArgumentException ex)
        {
            // This will be caught by GlobalExceptionMiddleware
            // and handled appropriately
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

### Logging Extensions

```csharp
// Business operations
_logger.LogBusinessOperation("UpdateBusiness", userId, businessData);

// Security events
_logger.LogSecurityEvent("UnauthorizedAccess", userId, "Attempted to access admin area");

// Authentication
_logger.LogAuthenticationEvent("Login", userId, success: true);

// Payment operations
_logger.LogPaymentOperation("ProcessPayment", userId, amount, transactionId);

// Performance tracking
_logger.LogPerformance("DatabaseQuery", duration, "GetBusinessList");
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