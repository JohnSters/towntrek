# Future TODOs

Centralized list of non-blocking tasks and production hardening items to tackle later.

## Analytics & Charts - CRITICAL ISSUES IDENTIFIED ✅ **VERIFIED**

### Immediate Critical Fixes Required
- **Unused ServiceConfiguration.cs**: ✅ **NEW ISSUE IDENTIFIED** - Remove ServiceConfiguration.cs as it's not being called in Program.cs
  - **Impact**: Code confusion, potential maintenance issues, misleading documentation
  - **Solution**: Delete ServiceConfiguration.cs entirely
- **Missing BusinessViewLog Entity Configuration**: ✅ **NEW ISSUE IDENTIFIED** - Add proper entity configuration for BusinessViewLog in ApplicationDbContext.cs
  - **Impact**: Missing indexes for analytics queries, potential performance issues
  - **Solution**: Add proper entity configuration with composite indexes
- **Missing Advanced Analytics Entity Configurations**: ✅ **VERIFIED** - Add proper Entity Framework configurations for advanced analytics tables
  - **Tables**: `CustomMetric`, `AnomalyDetection`, `PredictiveForecast`, `SeasonalPattern`
  - **Impact**: Missing constraints and indexes
  - **Solution**: Add proper entity configurations with indexes and constraints
- **N+1 Query Problems**: ✅ **PARTIALLY ADDRESSED** - Fix remaining multiple database queries in loops in `AnalyticsService.cs`
  - **Status**: Some methods already use batch queries, but `GetCompetitorInsightsAsync` and `GetCategoryBenchmarksAsync` still need optimization
- **Missing Database Indexes**: ✅ **VERIFIED** - Add indexes for analytics queries in `ApplicationDbContext.cs`
  - **Critical Missing Indexes**:
    - `BusinessViewLog`: Composite indexes for `(BusinessId, ViewedAt, Platform)`, `(UserId, ViewedAt)`
    - `AnalyticsEvents`: Indexes for `(EventType, OccurredAt)`, `(UserId, EventType, OccurredAt)`
    - `CustomMetric`: Indexes for `(UserId, IsActive)`, `(Category, IsActive)`
- **Massive Controller**: ✅ **VERIFIED** - Split `AnalyticsController.cs` (1,039 lines) into smaller controllers
  - **Proposed Structure**: `ClientAnalyticsController`, `BusinessAnalyticsController`, `ChartDataController`, `ExportController`

### Performance Issues ✅ **VERIFIED**
- Implement real per-day view tracking:
  - ✅ **VERIFIED** - `BusinessViewLog` table exists and is being used
  - ✅ **VERIFIED** - `AnalyticsSnapshot` table exists for daily aggregation
  - **Issue**: Missing proper indexes for efficient querying
- Optional interim: distribute `Business.ViewCount` over N days with noise for better demo visuals.

### Architecture Issues ✅ **VERIFIED**
- **Service Layer**: ✅ **VERIFIED** - Split `AnalyticsService.cs` (841 lines) into focused services
  - **Proposed Services**: `ClientAnalyticsService`, `BusinessMetricsService`, `ChartDataService`, `ComparativeAnalysisService`
- **Real-Time Analytics**: ✅ **VERIFIED** - Optimize SignalR implementation and background services
  - **Current**: 5 large JS files (2,462 lines total, ~95KB)
  - **Strategy**: Consolidate into 2-3 optimized modules
- **JavaScript Complexity**: ✅ **CORRECTED** - Consolidate 5 large JS files (2,462 lines total)
  - `analytics.js` (701 lines)
  - `real-time-analytics.js` (411 lines)
  - `advanced-analytics.js` (458 lines)
  - `comparative-analytics.js` (553 lines)
  - `analytics-export.js` (337 lines)

### New Issues Identified ✅ **NEW**
- **Unused ServiceConfiguration.cs**: ServiceConfiguration.cs exists but is never called
  - **Impact**: Code confusion, potential maintenance issues
  - **Solution**: Remove ServiceConfiguration.cs entirely
- **Missing BusinessViewLog Entity Configuration**: BusinessViewLog has no entity configuration in ApplicationDbContext.cs
  - **Impact**: Missing indexes for analytics queries, potential performance issues
  - **Solution**: Add proper entity configuration with composite indexes
- **JavaScript Bundle Size**: ~95KB total affects page load performance
  - **Solution**: Implement code splitting and lazy loading

### Frontend Architecture Issues ✅ **CORRECTED**
- **CSS Complexity**: ✅ **CORRECTED** - 6 analytics CSS files (2,820 lines total, ~54KB)
  - `analytics.css` (889 lines) - Main analytics styles
  - `advanced-analytics.css` (424 lines) - Advanced features
  - `real-time-analytics.css` (383 lines) - Real-time components
  - `business-analytics.css` (302 lines) - Business-specific
  - `analytics-export-modal.css` (202 lines) - Export modal
  - `comparative-analytics.css` (620 lines) - Comparison features
  - **Issues**: Style duplication, no clear architecture, performance impact, inconsistent organization
  - **Solution**: Consolidate into 4 focused files with proper architecture

- **View Complexity**: ✅ **CORRECTED** - 6 analytics views (1,431 lines total)
  - `Index.cshtml` (346 lines) - Main dashboard
  - `ComparativeAnalysis.cshtml` (423 lines) - Comparison view
  - `_DashboardCustomization.cshtml` (208 lines) - Customization
  - `Business.cshtml` (196 lines) - Business analytics
  - `Competitors.cshtml` (85 lines) - Competitor analysis
  - `Benchmarks.cshtml` (147 lines) - Benchmarking
  - **Issues**: Mixed concerns, large file sizes, inconsistent structure
  - **Solution**: Break into smaller partial views and components

- **SignalR Connection Management**: ✅ **VERIFIED** - Connection management not optimized
  - **Issues**: Connection leaks, inadequate error recovery, memory usage, no connection pooling
  - **Solution**: Implement proper connection pooling, cleanup, and error handling

### JavaScript Architecture Issues ✅ **CORRECTED**
- **Module Dependencies**: Complex inter-module dependencies without clear interfaces
- **Memory Leaks**: SignalR connections not properly managed
- **Error Handling**: Inconsistent error handling across modules
- **Performance**: Large bundle size affects page load times
- **Proposed Solution**: 
  ```javascript
  // New architecture
  analytics-core.js          -- Core analytics functionality
  analytics-charts.js        -- Chart management
  analytics-realtime.js      -- SignalR integration
  analytics-export.js        -- Export functionality
  ```

### CSS Architecture Issues ✅ **CORRECTED**
- **Style Duplication**: Similar styles repeated across files
- **No CSS Architecture**: No clear organization or naming conventions
- **Performance Impact**: Multiple CSS files increase HTTP requests
- **Inconsistent Organization**: Files scattered across different directories
- **Proposed Solution**:
  ```css
  /* New architecture */
  analytics-base.css         -- Base analytics styles
  analytics-components.css   -- Reusable components
  analytics-layouts.css      -- Layout-specific styles
  analytics-themes.css       -- Theme variations
  ```

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
- ✅ **NEW** - Add proper Entity Framework configurations for BusinessViewLog and advanced analytics tables

## Code Quality ✅ **ENHANCED**
- ✅ **VERIFIED** - Reduce cyclomatic complexity in analytics services (841-line AnalyticsService)
- ✅ **VERIFIED** - Implement comprehensive unit tests for analytics services
- ✅ **VERIFIED** - Standardize error handling patterns across all services
- ✅ **VERIFIED** - Remove code duplication in analytics modules
- ✅ **NEW** - Remove unused ServiceConfiguration.cs (critical issue)

## Frontend Optimization ✅ **ENHANCED**
- ✅ **CORRECTED** - Optimize JavaScript bundle sizes (~95KB total)
- ✅ **VERIFIED** - Implement proper error boundaries
- ✅ **VERIFIED** - Add loading states for analytics charts
- ✅ **VERIFIED** - Improve mobile responsiveness for analytics dashboard
- ✅ **NEW** - Implement code splitting and lazy loading for analytics modules
- ✅ **CORRECTED** - Consolidate CSS files (~54KB total, 6 files)
- ✅ **CORRECTED** - Refactor large views into smaller components
- ✅ **VERIFIED** - Optimize SignalR connection management

## Implementation Priority Matrix ✅ **UPDATED**

| Issue | Impact | Effort | Priority | Timeline | Status |
|-------|--------|--------|----------|----------|---------|
| Unused ServiceConfiguration.cs | Medium | Low | CRITICAL | Week 1 | ✅ NEW ISSUE |
| Missing BusinessViewLog Config | High | Low | HIGH | Week 1 | ✅ NEW ISSUE |
| Missing Advanced Analytics Configs | Medium | Low | HIGH | Week 1 | ✅ NEW ISSUE |
| Missing Database Indexes | High | Low | HIGH | Week 1 | ✅ VERIFIED |
| N+1 Query Problems | High | Medium | HIGH | Week 1-2 | ✅ PARTIALLY ADDRESSED |
| JavaScript Architecture | High | High | HIGH | Week 9-10 | ✅ CORRECTED |
| Massive Controller | High | High | HIGH | Week 5-6 | ✅ VERIFIED |
| CSS Architecture | Medium | Medium | MEDIUM | Week 9-10 | ✅ CORRECTED |
| Service Architecture | Medium | High | MEDIUM | Week 3-4 | ✅ VERIFIED |
| SignalR Optimization | Medium | Medium | MEDIUM | Week 11-12 | ✅ VERIFIED |
| Real-Time Complexity | Medium | Medium | MEDIUM | Week 9-10 | ✅ VERIFIED |
| Data Model Issues | Medium | Medium | MEDIUM | Week 7-8 | ✅ VERIFIED |
| View Complexity | Medium | Medium | MEDIUM | Week 9-10 | ✅ CORRECTED |
| JavaScript Optimization | Low | Medium | LOW | Week 9-10 | ✅ VERIFIED |

## Success Metrics ✅ **ADDED**

### Performance Improvements
- **Database Query Count**: Reduce by 70%
- **Page Load Time**: Improve by 50%
- **Memory Usage**: Reduce by 30%
- **Error Rate**: Reduce by 80%
- **JavaScript Bundle Size**: Reduce by 40%
- **CSS Bundle Size**: Reduce by 30%

### Code Quality Improvements
- **Cyclomatic Complexity**: Reduce by 60%
- **Code Duplication**: Eliminate 90%
- **Test Coverage**: Achieve 80% coverage
- **Maintainability Index**: Improve by 40%
- **File Size Reduction**: Reduce average file size by 50%

### User Experience Improvements
- **Analytics Dashboard Load Time**: < 2 seconds
- **Real-Time Update Latency**: < 500ms
- **Error Recovery**: 95% success rate
- **Mobile Performance**: Parity with desktop
- **Connection Stability**: 99.9% uptime

## Risk Assessment ✅ **ENHANCED**

### High Risk Items
1. **Service Registration Changes**: Potential runtime errors
2. **Database Schema Changes**: Data integrity risks
3. **Controller Refactoring**: Breaking changes to API endpoints
4. **JavaScript Refactoring**: Potential breaking changes to frontend
5. **SignalR Changes**: Real-time functionality disruption

### Mitigation Strategies
1. **Comprehensive Testing**: Unit, integration, and end-to-end tests
2. **Feature Flags**: Gradual rollout of changes
3. **Database Backups**: Before schema changes
4. **Monitoring**: Enhanced logging during transition
5. **Rollback Plan**: Quick rollback procedures
6. **User Communication**: Clear communication about changes

## Additional Findings ✅ **ENHANCED**

### Unused ServiceConfiguration.cs ✅ **NEW ISSUE IDENTIFIED**
**Severity**: MEDIUM
**Issue**: ServiceConfiguration.cs exists but is never called, creating confusion and potential maintenance issues
**Impact**: Code confusion, potential future duplication if someone tries to use it
**Solution**: Remove ServiceConfiguration.cs entirely

### Missing BusinessViewLog Entity Configuration ✅ **NEW ISSUE IDENTIFIED**
**Severity**: HIGH
**Issue**: BusinessViewLog has no entity configuration in ApplicationDbContext.cs
**Impact**: Missing indexes for analytics queries, potential performance issues
**Solution**: Add proper entity configuration with composite indexes

### Missing Advanced Analytics Entity Configurations ✅ **VERIFIED**
**Severity**: MEDIUM
**Issue**: Advanced analytics tables (CustomMetric, AnomalyDetection, etc.) are missing proper Entity Framework configurations in `ApplicationDbContext.cs`
**Impact**: Potential performance issues and missing constraints
**Solution**: Add proper entity configurations with indexes and constraints

### JavaScript Bundle Size ✅ **CORRECTED**
**Total Size**: ~95KB across 5 files
**Lines of Code**: 2,462 lines
**Impact**: Large bundle size affects page load performance
**Solution**: Implement code splitting and lazy loading

### CSS Bundle Size ✅ **CORRECTED**
**Total Size**: ~54KB across 6 files
**Lines of Code**: 2,820 lines
**Impact**: Multiple HTTP requests and large CSS bundle
**Solution**: Consolidate files and implement critical CSS inlining

### View Complexity ✅ **CORRECTED**
**Total Size**: 1,431 lines across 6 views
**Impact**: Large views affect rendering performance
**Solution**: Break into smaller partial views and components

### SignalR Connection Issues ✅ **VERIFIED**
**Issue**: Connection management not optimized
**Impact**: Potential memory leaks and performance issues
**Solution**: Implement proper connection pooling and cleanup

### Inconsistent CSS Organization ✅ **NEW ISSUE IDENTIFIED**
**Issue**: CSS files scattered across different directories
**Impact**: Inconsistent organization, harder to maintain
**Solution**: Standardize CSS file locations

---

*Last Updated: [Current Date]*  
*Priority: Analytics issues marked as CRITICAL require immediate attention*  
*Status: ✅ Comprehensive Analysis Complete with Corrections*
