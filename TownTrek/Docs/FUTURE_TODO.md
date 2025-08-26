# Future TODOs

Centralized list of non-blocking tasks and production hardening items to tackle later.

## Analytics & Charts - CRITICAL ISSUES IDENTIFIED ✅ **VERIFIED**

### Immediate Critical Fixes Required
- **Service Registration Duplication**: ✅ **VERIFIED** - Remove duplicate registrations in `Program.cs` and `ServiceConfiguration.cs`
  - **Impact**: DI container conflicts, memory leaks, inconsistent service lifetimes
  - **Solution**: Remove duplicates from `ServiceConfiguration.cs`, keep only in `Program.cs`
- **N+1 Query Problems**: ✅ **PARTIALLY ADDRESSED** - Fix remaining multiple database queries in loops in `AnalyticsService.cs`
  - **Status**: Some methods already use batch queries, but `GetCompetitorInsightsAsync` and `GetCategoryBenchmarksAsync` still need optimization
- **Missing Database Indexes**: ✅ **VERIFIED** - Add indexes for analytics queries in `ApplicationDbContext.cs`
  - **Critical Missing Indexes**:
    - `BusinessViewLog`: Composite indexes for `(BusinessId, ViewedAt, Platform)`, `(UserId, ViewedAt)`
    - `AnalyticsEvents`: Indexes for `(EventType, OccurredAt)`, `(UserId, EventType, OccurredAt)`
    - `CustomMetric`: Indexes for `(UserId, IsActive)`, `(Category, IsActive)`
- **Massive Controller**: ✅ **VERIFIED** - Split `AnalyticsController.cs` (1,168 lines) into smaller controllers
  - **Proposed Structure**: `ClientAnalyticsController`, `BusinessAnalyticsController`, `ChartDataController`, `ExportController`

### Performance Issues ✅ **VERIFIED**
- Implement real per-day view tracking:
  - ✅ **VERIFIED** - `BusinessViewLog` table exists and is being used
  - ✅ **VERIFIED** - `AnalyticsSnapshot` table exists for daily aggregation
  - **Issue**: Missing proper indexes for efficient querying
- Optional interim: distribute `Business.ViewCount` over N days with noise for better demo visuals.

### Architecture Issues ✅ **VERIFIED**
- **Service Layer**: ✅ **VERIFIED** - Split `AnalyticsService.cs` (946 lines) into focused services
  - **Proposed Services**: `ClientAnalyticsService`, `BusinessMetricsService`, `ChartDataService`, `ComparativeAnalysisService`
- **Real-Time Analytics**: ✅ **VERIFIED** - Optimize SignalR implementation and background services
  - **Current**: 5 large JS files (2,767 lines total, 106.6KB)
  - **Strategy**: Consolidate into 2-3 optimized modules
- **JavaScript Complexity**: ✅ **VERIFIED** - Consolidate 5 large JS files (2,767 lines total)
  - `analytics.js` (29KB, 758 lines)
  - `real-time-analytics.js` (18KB, 481 lines)
  - `advanced-analytics.js` (21KB, 528 lines)
  - `comparative-analytics.js` (23KB, 629 lines)
  - `analytics-export.js` (15KB, 371 lines)
- **Data Models**: ✅ **VERIFIED** - Remove circular dependencies and standardize naming

### New Issues Identified ✅ **NEW**
- **Missing Database Configurations**: Advanced analytics tables missing proper Entity Framework configurations
  - **Tables**: `CustomMetric`, `AnomalyDetection`, `PredictiveForecast`, `SeasonalPattern`
  - **Impact**: Missing constraints and indexes
- **JavaScript Bundle Size**: 106.6KB total affects page load performance
  - **Solution**: Implement code splitting and lazy loading

### See: `Docs/ANALYTICS_SYSTEM_ANALYSIS.md` for complete analysis ✅ **UPDATED**

## Subscription & Access Control
- Add app setting to disable legacy subscription flag fallback in production (gate in `SubscriptionAuthService`).
- Normalize and clean stale roles on login: map `CurrentSubscriptionTier` to `AppRoles` and remove mismatched `Client-*` or `Client-Trial` when not applicable.
- ✅ Removed `TestAnalyticsController` diagnostics.

## Payments
- Align payment provider naming and services (PayFast vs PayPal) across code and docs.
- Implement secure webhook/IPN handling with signature validation and idempotent processing.
- Map provider payment states to internal statuses consistently.

## Security & Auth
- Optional MFA for production; review password policy before go-live.
- Add audit logs for subscription/role changes and payment transitions.

## Configuration
- Introduce environment toggles: `AllowLegacyTierFallback`, `EnableDiagnostics`, `UseSandbox`.
- Ensure secrets are not in production appsettings; use env vars/secret store.

## Misc
- Implement Forgot Password flow (`AuthController` placeholder).
- Consider external time validation for trial security (`SecureTrialService` TODO comment).

## Database Optimization ✅ **ENHANCED**
- ✅ **VERIFIED** - Add missing indexes for analytics queries (see above)
- ✅ **VERIFIED** - Optimize BusinessViewLog queries for performance
- ✅ **VERIFIED** - Implement proper caching strategy for analytics data
- ✅ **VERIFIED** - Review and optimize slow queries identified in AnalyticsPerformanceMonitor
- **New**: Add proper Entity Framework configurations for advanced analytics tables

## Code Quality ✅ **ENHANCED**
- ✅ **VERIFIED** - Reduce cyclomatic complexity in analytics services (946-line AnalyticsService)
- ✅ **VERIFIED** - Implement comprehensive unit tests for analytics services
- ✅ **VERIFIED** - Standardize error handling patterns across all services
- ✅ **VERIFIED** - Remove code duplication in analytics modules
- **New**: Fix service registration duplication (critical issue)

## Frontend Optimization ✅ **ENHANCED**
- ✅ **VERIFIED** - Optimize JavaScript bundle sizes (106.6KB total)
- ✅ **VERIFIED** - Implement proper error boundaries
- ✅ **VERIFIED** - Add loading states for analytics charts
- ✅ **VERIFIED** - Improve mobile responsiveness for analytics dashboard
- **New**: Implement code splitting and lazy loading for analytics modules

## Implementation Priority Matrix ✅ **UPDATED**

| Issue | Impact | Effort | Priority | Timeline | Status |
|-------|--------|--------|----------|----------|---------|
| Service Registration Duplication | High | Low | CRITICAL | Week 1 | ✅ VERIFIED |
| Missing Database Indexes | High | Low | HIGH | Week 1 | ✅ VERIFIED |
| N+1 Query Problems | High | Medium | HIGH | Week 1-2 | ✅ PARTIALLY ADDRESSED |
| Massive Controller | High | High | HIGH | Week 5-6 | ✅ VERIFIED |
| Service Architecture | Medium | High | MEDIUM | Week 3-4 | ✅ VERIFIED |
| Real-Time Complexity | Medium | Medium | MEDIUM | Week 9-10 | ✅ VERIFIED |
| Data Model Issues | Medium | Medium | MEDIUM | Week 7-8 | ✅ VERIFIED |
| JavaScript Optimization | Low | Medium | LOW | Week 9-10 | ✅ VERIFIED |

## Success Metrics ✅ **ADDED**

### Performance Improvements
- **Database Query Count**: Reduce by 70%
- **Page Load Time**: Improve by 50%
- **Memory Usage**: Reduce by 30%
- **Error Rate**: Reduce by 80%

### Code Quality Improvements
- **Cyclomatic Complexity**: Reduce by 60%
- **Code Duplication**: Eliminate 90%
- **Test Coverage**: Achieve 80% coverage
- **Maintainability Index**: Improve by 40%

### User Experience Improvements
- **Analytics Dashboard Load Time**: < 2 seconds
- **Real-Time Update Latency**: < 500ms
- **Error Recovery**: 95% success rate
- **Mobile Performance**: Parity with desktop

---

*Last Updated: [Current Date]*  
*Priority: Analytics issues marked as CRITICAL require immediate attention*  
*Status: ✅ Verified and Enhanced*
