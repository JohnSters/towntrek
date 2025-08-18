# Analytics System Guide

## Overview

The TownTrek Analytics system is a comprehensive monitoring and analytics platform that tracks application performance, user behavior, and system health. While the infrastructure is complete, it requires integration points to collect real data.

## System Architecture

### 1. **Monitoring Services**
- **`AnalyticsPerformanceMonitor`** - Tracks page load times, database queries, chart rendering
- **`AnalyticsErrorTracker`** - Tracks errors with severity levels and resolution
- **`AnalyticsUsageTracker`** - Tracks feature usage and user engagement
- **`AnalyticsHealthCheck`** - Monitors overall system health

### 2. **Background Services**
- **`AnalyticsSnapshotBackgroundService`** - Creates daily snapshots (runs at 2 AM UTC)
- **`RealTimeAnalyticsBackgroundService`** - Handles real-time updates
- **`AnalyticsAuditCleanupBackgroundService`** - Cleans up old data

### 3. **Database Tables**
- **`AnalyticsPerformanceLogs`** - Performance metrics
- **`AnalyticsErrorLogs`** - Error tracking with severity
- **`AnalyticsUsageLogs`** - User interaction tracking
- **`AnalyticsAuditLogs`** - Security audit logs
- **`BusinessViewLogs`** - Real view tracking
- **`AnalyticsSnapshots`** - Historical data snapshots

## Current Status

### ✅ **What's Working**
- All database tables exist and are properly configured
- All services are registered in DI container
- Background services are running
- Admin monitoring dashboard is accessible
- Health checks are functional

### ❌ **What's Not Working**
- **No automatic data collection** - Services only log when explicitly called
- **Empty monitoring dashboards** - No real data being collected
- **Missing integration points** - Not connected to actual application flow

## How to Activate the System

### **Option 1: Quick Test (Recommended)**

1. **Access the Admin Monitoring Dashboard**
   - Navigate to `/Admin/AnalyticsMonitoring/Dashboard`
   - You'll see empty metrics (0.0 ms, 0.00%, etc.)

2. **Generate Test Data**
   - Click the "Generate Test Data" button
   - This will create sample performance, usage, and error data
   - Refresh the dashboard to see real metrics

3. **View Different Sections**
   - **Performance** - Page load times, query performance
   - **Errors** - Error tracking and resolution
   - **Usage** - Feature usage and user engagement

### **Option 2: Full Integration**

To make the system collect real data automatically, you need to integrate the monitoring services into your application:

#### **Performance Tracking**
```csharp
// In your controllers
public async Task<IActionResult> SomeAction()
{
    var startTime = DateTime.UtcNow;
    var isSuccess = false;
    
    try
    {
        // Your action logic here
        isSuccess = true;
        return View();
    }
    finally
    {
        var loadTime = DateTime.UtcNow - startTime;
        await _performanceMonitor.TrackAnalyticsPageLoadAsync(userId, loadTime, isSuccess);
    }
}
```

#### **Error Tracking**
```csharp
// Already integrated in GlobalExceptionMiddleware
// Automatically tracks errors for analytics-related requests
```

#### **Usage Tracking**
```csharp
// Track user interactions
await _usageTracker.TrackFeatureUsageAsync(userId, "FeatureName", duration);
```

## Monitoring Dashboard Features

### **Main Dashboard (`/Admin/AnalyticsMonitoring/Dashboard`)**
- **Performance Stats** - Average page load time, query time, chart render time
- **Error Rate** - Percentage of failed requests
- **Active Users** - Number of users using analytics features
- **System Health** - Overall system status (Healthy/Degraded/Unhealthy)

### **Performance Monitoring (`/Admin/AnalyticsMonitoring/Performance`)**
- Detailed performance metrics
- Slow query analysis
- User engagement statistics
- Performance trends over time

### **Error Tracking (`/Admin/AnalyticsMonitoring/Errors`)**
- Recent errors with severity levels
- Error trends and breakdown
- Critical errors requiring attention
- Error resolution tracking

### **Usage Analytics (`/Admin/AnalyticsMonitoring/Usage`)**
- Most used features
- User engagement metrics
- Session analytics
- Feature adoption rates

## Background Services

### **Analytics Snapshots**
- **Runs**: Daily at 2 AM UTC
- **Purpose**: Creates historical snapshots for trend analysis
- **Data**: Business views, reviews, favorites, engagement scores

### **Real-Time Updates**
- **Runs**: Every 30 seconds
- **Purpose**: Sends real-time updates to connected users
- **Features**: Live data updates, notifications for significant changes

### **Data Cleanup**
- **Runs**: Weekly
- **Purpose**: Removes old audit logs (365-day retention)
- **Tables**: AnalyticsAuditLogs, AnalyticsPerformanceLogs, etc.

## Health Checks

### **System Health Endpoint**
- **URL**: `/health`
- **Checks**: Database connectivity, cache health, performance metrics, error rates
- **Status**: Healthy, Degraded, or Unhealthy

### **Analytics Health Check**
- **Component**: `AnalyticsHealthCheck`
- **Thresholds**:
  - Page load time > 3 seconds = Degraded
  - Query time > 1 second = Degraded
  - Error rate > 5% = Unhealthy
  - Success rate < 95% = Degraded

## Data Collection Points

### **Automatic Collection**
- **View Tracking**: Middleware automatically tracks business page views
- **Error Tracking**: Global exception middleware tracks analytics errors
- **Audit Logging**: All analytics access is automatically logged

### **Manual Collection**
- **Performance Tracking**: Must be added to controllers
- **Usage Tracking**: Must be added to user interactions
- **Custom Metrics**: Can be added for specific business needs

## Configuration

### **Cache Settings** (`appsettings.json`)
```json
{
  "Cache": {
    "UseRedis": false,
    "RedisConnectionString": "",
    "DefaultExpirationMinutes": 30
  }
}
```

### **Rate Limiting**
- **Global**: 1000 requests/minute per user/IP
- **Analytics**: 100 requests/minute per user
- **Chart Data**: 50 requests/minute per user

## Troubleshooting

### **Empty Dashboard**
- **Cause**: No data being collected
- **Solution**: Use "Generate Test Data" button or integrate monitoring services

### **High Error Rates**
- **Check**: Error tracking logs
- **Action**: Review recent errors in monitoring dashboard
- **Resolution**: Fix underlying issues and mark errors as resolved

### **Performance Issues**
- **Check**: Performance monitoring dashboard
- **Action**: Identify slow queries or page loads
- **Resolution**: Optimize database queries or page rendering

### **Background Service Issues**
- **Check**: Application logs for background service errors
- **Action**: Verify database connectivity and permissions
- **Resolution**: Restart application if services are stuck

## Best Practices

### **Performance Monitoring**
1. Add performance tracking to all analytics endpoints
2. Monitor slow queries and optimize them
3. Set appropriate thresholds for alerts

### **Error Tracking**
1. Categorize errors by severity
2. Set up alerts for critical errors
3. Regularly review and resolve errors

### **Usage Tracking**
1. Track meaningful user interactions
2. Monitor feature adoption rates
3. Use data to improve user experience

### **Data Management**
1. Regularly review data retention policies
2. Monitor database size and performance
3. Clean up old data periodically

## Next Steps

1. **Test the System**: Use the "Generate Test Data" button to see it in action
2. **Integrate Monitoring**: Add performance and usage tracking to your controllers
3. **Set Up Alerts**: Configure notifications for critical issues
4. **Customize Metrics**: Add business-specific monitoring as needed
5. **Scale Up**: Consider Redis caching for high-traffic scenarios

## Support

For issues or questions about the analytics system:
1. Check the application logs for detailed error information
2. Review the monitoring dashboard for system health
3. Consult the technical documentation for implementation details
4. Contact the development team for complex issues
