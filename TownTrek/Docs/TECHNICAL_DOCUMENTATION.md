# TownTrek Technical Documentation

## Application Overview

**TownTrek** is a .NET 8 ASP.NET Core MVC web application designed as a local business directory and management platform for South African towns. The application allows business owners to register, manage their business listings, and provides subscription-based access to advanced features.

---

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core 8.0.8
- **Authentication**: ASP.NET Core Identity with custom user management
- **Payment Processing**: PayFast integration (South African payment gateway)
- **Image Processing**: SixLabors.ImageSharp 3.1.11
- **Dependency Injection**: Built-in .NET DI container

### Frontend
- **UI Framework**: Bootstrap 5 with custom CSS
- **JavaScript**: Vanilla JS with modular architecture
- **Icons**: Font Awesome
- **Design System**: Custom flat design with Material Design principles
- **Responsive**: Mobile-first approach

### Development & Deployment
- **IDE**: Visual Studio 2022 / VS Code
- **Version Control**: Git
- **Containerization**: Docker support
- **Configuration**: JSON-based with environment-specific settings

---

## Project Structure

```
TownTrek/
├── Controllers/           # MVC Controllers organized by area
│   ├── Admin/            # Admin panel controllers
│   ├── Client/           # Client-facing controllers
│   ├── Auth/             # Authentication controllers
│   ├── Api/              # API endpoints
│   ├── Analytics/        # Analytics and reporting
│   └── Home/             # Public pages
├── Models/               # Entity models and ViewModels
│   └── ViewModels/       # Data transfer objects (18 ViewModels)
├── Services/             # Business logic layer
│   └── Interfaces/       # Service contracts
├── Data/                 # Data access layer
├── Views/                # Razor views organized by area
│   ├── Admin/           # Admin panel views
│   ├── Client/          # Client views
│   ├── Auth/            # Authentication views
│   └── Shared/          # Shared layouts and components
├── wwwroot/             # Static assets
│   ├── css/             # Stylesheets (organized by feature)
│   ├── js/              # JavaScript modules
│   └── uploads/         # User-uploaded content
├── Middleware/          # Custom middleware components
├── Extensions/          # Custom extensions and helpers
├── Attributes/          # Custom authorization attributes
└── Migrations/          # Entity Framework migrations
```

---

## Database Schema

### Core Entities

#### Users & Authentication
```sql
-- ASP.NET Core Identity tables (extended)
AspNetUsers (ApplicationUser)
├── Id (PK)
├── UserName, Email, PhoneNumber
├── SubscriptionId (FK to Subscriptions)
└── Custom fields for business management

AspNetRoles, AspNetUserRoles (Role-based authorization)
```

#### Business Management
```sql
Towns
├── Id (PK)
├── Name, Province, PostalCode
├── Description, Landmarks
└── Population

Businesses
├── Id (PK)
├── UserId (FK to AspNetUsers)
├── TownId (FK to Towns)
├── Name, Category, SubCategory
├── Description, PhoneNumber, EmailAddress
├── PhysicalAddress, Website
├── Latitude, Longitude (GPS coordinates)
├── Status (Pending/Active/Inactive/Suspended)
├── IsFeatured, IsVerified
├── Rating, TotalReviews, ViewCount
└── Timestamps (CreatedAt, UpdatedAt, ApprovedAt)

BusinessHours
├── Id (PK)
├── BusinessId (FK to Businesses)
├── DayOfWeek (0-6)
├── OpenTime, CloseTime
└── IsOpen, IsSpecialHours

BusinessImages
├── Id (PK)
├── BusinessId (FK to Businesses)
├── ImageType (Logo/Cover/Gallery/Menu)
├── FileName, OriginalFileName
├── FileSize, ContentType
├── ImageUrl, ThumbnailUrl
├── AltText, DisplayOrder
└── Approval status and timestamps
```

#### Subscription System
```sql
SubscriptionTiers
├── Id (PK)
├── Name (Basic/Standard/Premium)
├── DisplayName, Description
├── MonthlyPrice, IsActive
└── SortOrder

SubscriptionTierLimits
├── Id (PK)
├── SubscriptionTierId (FK)
├── LimitType (MaxBusinesses/MaxImages/MaxPDFs)
└── LimitValue (-1 for unlimited)

SubscriptionTierFeatures
├── Id (PK)
├── SubscriptionTierId (FK)
├── FeatureKey, FeatureName
└── IsEnabled

Subscriptions
├── Id (PK)
├── UserId (FK to AspNetUsers)
├── SubscriptionTierId (FK)
├── MonthlyPrice (historical)
├── StartDate, EndDate, IsActive
├── PayFastToken, PayFastPaymentId
├── LastPaymentDate, NextBillingDate
└── PaymentStatus
```

#### Category-Specific Details
```sql
BusinessCategories
├── Id (PK)
├── Key (shops-retail/restaurants-food/etc.)
├── Name, Description, IconClass
├── FormType (enum)
└── IsActive

BusinessSubCategories
├── Id (PK)
├── CategoryId (FK to BusinessCategories)
├── Key, Name
└── IsActive

-- Category-specific detail tables
MarketDetails, TourDetails, EventDetails
RestaurantDetails, AccommodationDetails, ShopDetails
```

---

## Key Features & Architecture

### 1. Multi-Tenant Architecture
- **Role-based access**: Admin, Client, Public
- **Subscription tiers**: Basic, Standard, Premium
- **Feature gating**: Based on subscription limits
- **Middleware**: SubscriptionRedirectMiddleware for access control

### 2. Business Management System
- **Multi-category support**: 6 main categories with subcategories
- **Dynamic forms**: Category-specific business details
- **Image management**: Multi-image upload with thumbnails
- **Operating hours**: Flexible scheduling with special hours
- **Location services**: GPS coordinates for mapping

### 3. Subscription & Payment System
- **Tier-based pricing**: 3 subscription levels
- **PayFast integration**: South African payment processing
- **Usage limits**: Configurable limits per tier
- **Feature flags**: Granular feature control
- **Billing management**: Automatic payment tracking

### 4. Image Processing Pipeline
- **SixLabors.ImageSharp**: Server-side image processing
- **Multiple formats**: Support for common image types
- **Thumbnail generation**: Automatic resizing
- **Storage management**: Organized file structure
- **Approval workflow**: Admin approval for business images

### 5. Security & Authentication
- **ASP.NET Core Identity**: User management
- **Custom attributes**: RequireActiveSubscription, RequireAdmin
- **Session management**: Configurable timeouts
- **Role-based authorization**: Granular permissions
- **Data validation**: Server-side and client-side validation

---

## Service Layer Architecture

### Core Services
```csharp
// Business Logic Services
IBusinessService          // Business CRUD operations
IClientService           // Client dashboard operations
ISubscriptionTierService // Subscription management
ISubscriptionAuthService // Subscription validation
IImageService           // Image processing and storage
IPaymentService         // PayFast integration
IEmailService           // Email notifications
INotificationService    // In-app notifications
IRegistrationService    // User registration workflow
IRoleInitializationService // Role setup
```

### Service Patterns
- **Dependency Injection**: All services registered in DI container
- **Interface-based design**: Loose coupling and testability
- **Result patterns**: ServiceResult<T> for consistent error handling
- **Async operations**: Full async/await support
- **Validation**: Business rule enforcement

---

## Frontend Architecture

### CSS Organization
```
wwwroot/css/
├── foundation/          # Base styles and variables
├── components/          # Reusable UI components
├── features/           # Feature-specific styles
├── layouts/            # Layout-specific styles
└── entrypoints/        # Main CSS bundles
```

### JavaScript Architecture
```
wwwroot/js/
├── core/               # Core utilities and helpers
├── components/         # Reusable JS components
├── modules/           # Feature-specific modules
└── shared/            # Shared functionality
```

### Design System
- **Flat design**: No shadows or 3D effects
- **Color palette**: 5 primary colors with semantic meaning
- **Typography**: Inter font family with consistent scale
- **Spacing**: 8px grid system
- **Components**: Consistent button, card, form patterns
- **Accessibility**: WCAG 2.1 AA compliance

---

## Configuration & Environment

### App Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection string"
  },
  "PayFast": {
    "MerchantId": "10040964",
    "MerchantKey": "mieu9ydfgtqo4",
    "PassPhrase": "MyTestPassPhrase123",
    "PaymentUrl": "https://sandbox.payfast.co.za/eng/process",
    "UseSandbox": true
  },
  "BaseUrl": "https://localhost:7154"
}
```

### Environment-Specific Settings
- **Development**: Local SQL Server, sandbox payments
- **Production**: Production database, live payment processing
- **Docker**: Containerized deployment support

---

## Security Considerations

### Data Protection
- **Connection strings**: Stored in user secrets (development)
- **Payment data**: Minimal storage, PayFast tokenization
- **File uploads**: Validation and sanitization
- **SQL injection**: Entity Framework parameterized queries
- **XSS protection**: Razor view encoding

### Authentication & Authorization
- **Password requirements**: 6+ chars, uppercase, lowercase, digit
- **Session management**: 8-hour default, 7-day "remember me"
- **Role-based access**: Admin, Client, Public roles
- **Subscription validation**: Middleware-based access control

---

## Pre-Production Hardening Checklist

- Authentication and Roles
  - Disable legacy subscription flag fallback in production. Add an app setting (e.g., `AllowLegacyTierFallback = false` in Production, true in Development) and gate the fallback in `SubscriptionAuthService`.
  - Normalize client roles on login: map tiers to `AppRoles.ClientBasic`, `ClientStandard`, `ClientPremium`; remove any stale `Client-*` roles that don’t match the current tier; remove `Client-Trial` if the user isn’t a trial anymore.
  - Enforce optional MFA and review password policy before go-live.

- Diagnostics and Test Utilities
  - Remove (or restrict to Admin only) the diagnostic endpoints in `TestAnalyticsController` and any temporary debug navigation links.
  - Add an `EnableDiagnostics` flag (true in Development, false in Production).

- Subscription Features and Seeding
  - Standardize analytics features to two keys only: `BasicAnalytics` and `AdvancedAnalytics`. Ensure no `StandardAnalytics`/`PremiumAnalytics` keys remain in seeders or code.
  - Verify `SubscriptionTiers` seed contains correct features: Basic and Standard include `BasicAnalytics`; Premium includes `AdvancedAnalytics` (plus `BasicAnalytics`).
  - Route all subscription mutations through `SubscriptionManagementService`; avoid direct writes to legacy flags. Use `SyncUserSubscriptionFlagsAsync` to reconcile when needed.

- Payments Integration
  - Align on a single provider and naming. Current docs reference PayFast; if PayPal is the provider for production, update service names, configuration, and webhook/IPN handling accordingly.
  - Validate webhook signatures/IPN, idempotently process payment events, and map provider statuses to internal statuses (e.g., Completed/Active/Paid vs Pending/Failed/Rejected).

- Configuration and Environments
  - Introduce environment-based toggles: `AllowLegacyTierFallback`, `EnableDiagnostics`, and payment `UseSandbox`.
  - Verify Production appsettings exclude secrets; use environment variables or a secure secret store.

- Access Control and Middleware
  - Ensure `[RequireActiveSubscription(requiredFeature: "BasicAnalytics"|"AdvancedAnalytics")]` is used for all analytics features.
  - Keep `SubscriptionRedirectMiddleware` active to protect `/Client` routes for non-client users.

- Auditing and Observability
  - Add audit logs for subscription tier changes, role changes, and payment state transitions (who/when/before/after).
  - Ensure structured logging around authentication, authorization failures, and payment exceptions; configure alerting.

- QA Coverage
  - Test with: Member, Client-Basic, Client-Standard, Client-Premium, Client-Trial.
  - Validate analytics access: Basic vs Advanced paths; charts and insights visibility.
  - Exercise payment flows: Pending, Completed, Failed/Rejected; verify redirects and warnings.

---

## Performance & Scalability

### Database Optimization
- **Indexing**: Strategic indexes on frequently queried columns
- **Relationships**: Proper foreign key constraints
- **Query optimization**: Entity Framework best practices
- **Connection pooling**: SQL Server connection management

### Frontend Performance
- **CSS bundling**: Organized by feature and layout
- **JavaScript modules**: Modular loading and caching
- **Image optimization**: Automatic resizing and compression
- **CDN ready**: Static asset organization for CDN deployment

### Caching Strategy
- **Static files**: Browser caching for CSS/JS
- **Database queries**: Entity Framework query optimization
- **Session state**: In-memory session storage
- **Image caching**: Thumbnail generation and storage

---

## Deployment & DevOps

### Container Support
- **Docker**: Multi-stage builds for production
- **Environment variables**: Configuration management
- **Health checks**: Built-in .NET health monitoring
- **Logging**: Structured logging with Serilog

### Database Migrations
- **Entity Framework**: Code-first migrations
- **Version control**: Migration history tracking
- **Seeding**: Initial data population
- **Rollback**: Migration reversal capabilities

---

## Monitoring & Analytics

### Application Monitoring
- **Health endpoints**: Built-in health checks
- **Error tracking**: Exception handling and logging
- **Performance metrics**: Request timing and resource usage
- **User analytics**: Business view counts and engagement

### Business Intelligence
- **Subscription analytics**: Tier distribution and revenue
- **Business metrics**: Category popularity and growth
- **User behavior**: Registration and engagement patterns
- **Geographic data**: Town and business distribution

---

## Future Considerations

### Scalability Improvements
- **Microservices**: Service decomposition for large scale
- **Caching layer**: Redis for session and data caching
- **CDN integration**: Global content delivery
- **Database sharding**: Multi-tenant database separation

### Feature Enhancements
- **Mobile app**: React Native or Flutter
- **API expansion**: RESTful API for third-party integration
- **Advanced analytics**: Business intelligence dashboard
- **Multi-language**: Internationalization support

---

**Document Version**: 1.0  
**Last Updated**: January 2025  
**Next Review**: Before major architectural changes
