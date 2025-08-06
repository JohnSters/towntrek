# MyTown Database Design
## SQL Server Database Schema & Structure

### Document Overview
This document outlines the complete database design for the MyTown application, including table structures, relationships, indexes, and administrative capabilities. The design follows Entity Framework Core code-first approach with SQL Server as the database engine.

---

## Database Architecture Overview

### High-Level Schema
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Identity      │    │   Business      │    │   Content       │
│   Tables        │    │   Management    │    │   Management    │
│                 │    │                 │    │                 │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ AspNetUsers     │    │ Towns           │    │ BusinessImages  │
│ AspNetRoles     │    │ Businesses      │    │ BusinessDocs    │
│ AspNetUserRoles │    │ BusinessHours   │    │ ContentMod      │
│ AspNetUserClaims│    │ BusinessContacts│    │                 │
│ AspNetUserLogins│    │ BusinessLocations│   │                 │
│ AspNetUserTokens│    │ BusinessCategories│  │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Analytics     │    │   Payments      │    │   System        │
│   & Reporting   │    │   & Billing     │    │   Configuration │
│                 │    │                 │    │                 │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ PageViews       │    │ Subscriptions   │    │ SystemSettings  │
│ SearchLogs      │    │ PaymentHistory  │    │ EmailTemplates  │
│ ContactActions  │    │ PaymentGateways │    │ AuditLogs       │
│ UserSessions    │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

---

## Core Table Structures

### 1. Identity Tables (ASP.NET Core Identity)

#### AspNetUsers (Extended)
```sql
-- Extends the default IdentityUser table
CREATE TABLE [dbo].[AspNetUsers] (
    [Id] NVARCHAR(450) NOT NULL,
    [UserName] NVARCHAR(256) NULL,
    [NormalizedUserName] NVARCHAR(256) NULL,
    [Email] NVARCHAR(256) NULL,
    [NormalizedEmail] NVARCHAR(256) NULL,
    [EmailConfirmed] BIT NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed] BIT NOT NULL,
    [TwoFactorEnabled] BIT NOT NULL,
    [LockoutEnd] DATETIMEOFFSET(7) NULL,
    [LockoutEnabled] BIT NOT NULL,
    [AccessFailedCount] INT NOT NULL,
    
    -- Custom fields for MyTown
    [FirstName] NVARCHAR(100) NULL,
    [LastName] NVARCHAR(100) NULL,
    [CompanyName] NVARCHAR(200) NULL,
    [RegistrationDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [LastLoginDate] DATETIME2(7) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [ProfileImageUrl] NVARCHAR(500) NULL,
    [PhoneNumber2] NVARCHAR(50) NULL,
    [Address] NVARCHAR(500) NULL,
    [City] NVARCHAR(100) NULL,
    [Province] NVARCHAR(100) NULL,
    [PostalCode] NVARCHAR(10) NULL,
    [Country] NVARCHAR(100) NULL DEFAULT 'South Africa',
    
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
```

#### UserProfiles (Additional User Information)
```sql
CREATE TABLE [dbo].[UserProfiles] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [BusinessType] NVARCHAR(100) NULL,
    [BusinessRegistrationNumber] NVARCHAR(50) NULL,
    [VATNumber] NVARCHAR(50) NULL,
    [PreferredContactMethod] NVARCHAR(20) NULL, -- Email, Phone, SMS
    [MarketingConsent] BIT NOT NULL DEFAULT 0,
    [TermsAccepted] BIT NOT NULL DEFAULT 0,
    [TermsAcceptedDate] DATETIME2(7) NULL,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserProfiles_AspNetUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
```

### 2. Geographic & Location Tables

#### Towns
```sql
CREATE TABLE [dbo].[Towns] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Province] NVARCHAR(100) NOT NULL,
    [Latitude] DECIMAL(10, 8) NULL,
    [Longitude] DECIMAL(11, 8) NULL,
    [Description] NVARCHAR(1000) NULL,
    [Population] INT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    [CreatedBy] NVARCHAR(450) NULL,
    [ModifiedBy] NVARCHAR(450) NULL,
    
    CONSTRAINT [PK_Towns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Towns_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Towns_AspNetUsers_ModifiedBy] FOREIGN KEY ([ModifiedBy]) 
        REFERENCES [dbo].[AspNetUsers] ([Id])
);
```

#### BusinessLocations
```sql
CREATE TABLE [dbo].[BusinessLocations] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [Province] NVARCHAR(100) NOT NULL,
    [PostalCode] NVARCHAR(10) NULL,
    [Latitude] DECIMAL(10, 8) NULL,
    [Longitude] DECIMAL(11, 8) NULL,
    [IsPrimary] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_BusinessLocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessLocations_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

### 3. Business Management Tables

#### BusinessCategories
```sql
CREATE TABLE [dbo].[BusinessCategories] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IconClass] NVARCHAR(100) NULL, -- For UI icons
    [ColorCode] NVARCHAR(7) NULL, -- Hex color code
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_BusinessCategories] PRIMARY KEY ([Id])
);
```

#### BusinessSubCategories
```sql
CREATE TABLE [dbo].[BusinessSubCategories] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [CategoryId] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_BusinessSubCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessSubCategories_BusinessCategories] FOREIGN KEY ([CategoryId]) 
        REFERENCES [dbo].[BusinessCategories] ([Id])
);
```

#### Businesses
```sql
CREATE TABLE [dbo].[Businesses] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [TownId] INT NOT NULL,
    [CategoryId] INT NOT NULL,
    [SubCategoryId] INT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [ShortDescription] NVARCHAR(500) NULL,
    [Website] NVARCHAR(500) NULL,
    [Email] NVARCHAR(256) NULL,
    [PhoneNumber] NVARCHAR(50) NULL,
    [PhoneNumber2] NVARCHAR(50) NULL,
    [LogoUrl] NVARCHAR(500) NULL,
    [CoverImageUrl] NVARCHAR(500) NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Active, Inactive, Suspended
    [IsFeatured] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [Rating] DECIMAL(3,2) NULL,
    [TotalReviews] INT NOT NULL DEFAULT 0,
    [ViewCount] INT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    [ApprovedDate] DATETIME2(7) NULL,
    [ApprovedBy] NVARCHAR(450) NULL,
    
    CONSTRAINT [PK_Businesses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Businesses_AspNetUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Businesses_Towns] FOREIGN KEY ([TownId]) 
        REFERENCES [dbo].[Towns] ([Id]),
    CONSTRAINT [FK_Businesses_BusinessCategories] FOREIGN KEY ([CategoryId]) 
        REFERENCES [dbo].[BusinessCategories] ([Id]),
    CONSTRAINT [FK_Businesses_BusinessSubCategories] FOREIGN KEY ([SubCategoryId]) 
        REFERENCES [dbo].[BusinessSubCategories] ([Id]),
    CONSTRAINT [FK_Businesses_AspNetUsers_ApprovedBy] FOREIGN KEY ([ApprovedBy]) 
        REFERENCES [dbo].[AspNetUsers] ([Id])
);
```

#### BusinessHours
```sql
CREATE TABLE [dbo].[BusinessHours] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [DayOfWeek] INT NOT NULL, -- 0=Sunday, 1=Monday, etc.
    [OpenTime] TIME(7) NULL,
    [CloseTime] TIME(7) NULL,
    [IsOpen] BIT NOT NULL DEFAULT 1,
    [IsSpecialHours] BIT NOT NULL DEFAULT 0,
    [SpecialHoursNote] NVARCHAR(200) NULL,
    
    CONSTRAINT [PK_BusinessHours] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessHours_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

#### BusinessContacts
```sql
CREATE TABLE [dbo].[BusinessContacts] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [ContactType] NVARCHAR(50) NOT NULL, -- Phone, Email, WhatsApp, Facebook, Instagram
    [ContactValue] NVARCHAR(500) NOT NULL,
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_BusinessContacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessContacts_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

### 4. Category-Specific Business Tables

#### RestaurantDetails
```sql
CREATE TABLE [dbo].[RestaurantDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [CuisineType] NVARCHAR(100) NULL,
    [PriceRange] NVARCHAR(20) NULL, -- Budget, Moderate, Expensive
    [HasDelivery] BIT NOT NULL DEFAULT 0,
    [HasTakeaway] BIT NOT NULL DEFAULT 0,
    [HasReservations] BIT NOT NULL DEFAULT 0,
    [MaxGroupSize] INT NULL,
    [DietaryOptions] NVARCHAR(500) NULL, -- JSON or comma-separated
    [MenuUrl] NVARCHAR(500) NULL,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_RestaurantDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RestaurantDetails_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

#### AccommodationDetails
```sql
CREATE TABLE [dbo].[AccommodationDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [PropertyType] NVARCHAR(100) NULL, -- Hotel, Guesthouse, B&B, Self-catering
    [StarRating] INT NULL,
    [RoomCount] INT NULL,
    [CheckInTime] TIME(7) NULL,
    [CheckOutTime] TIME(7) NULL,
    [Amenities] NVARCHAR(1000) NULL, -- JSON or comma-separated
    [PricingInfo] NVARCHAR(500) NULL,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_AccommodationDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AccommodationDetails_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

#### TourDetails
```sql
CREATE TABLE [dbo].[TourDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [TourType] NVARCHAR(100) NULL, -- Cultural, Adventure, Wildlife, etc.
    [Duration] NVARCHAR(100) NULL, -- e.g., "2 hours", "Full day"
    [MaxGroupSize] INT NULL,
    [MinAge] INT NULL,
    [PricingInfo] NVARCHAR(500) NULL,
    [DepartureLocation] NVARCHAR(500) NULL,
    [Itinerary] NVARCHAR(2000) NULL,
    [IncludedItems] NVARCHAR(1000) NULL,
    [ExcludedItems] NVARCHAR(1000) NULL,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_TourDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TourDetails_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

#### EventDetails
```sql
CREATE TABLE [dbo].[EventDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [EventType] NVARCHAR(100) NULL, -- Festival, Market, Concert, etc.
    [StartDate] DATETIME2(7) NOT NULL,
    [EndDate] DATETIME2(7) NULL,
    [IsRecurring] BIT NOT NULL DEFAULT 0,
    [RecurrencePattern] NVARCHAR(100) NULL, -- Weekly, Monthly, etc.
    [TicketInfo] NVARCHAR(500) NULL,
    [OrganizerContact] NVARCHAR(500) NULL,
    [ExpectedAttendance] INT NULL,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_EventDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventDetails_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE
);
```

### 5. Content Management Tables

#### BusinessImages
```sql
CREATE TABLE [dbo].[BusinessImages] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [ImageType] NVARCHAR(50) NOT NULL, -- Logo, Cover, Gallery, Menu
    [FileName] NVARCHAR(255) NOT NULL,
    [OriginalFileName] NVARCHAR(255) NOT NULL,
    [FileSize] BIGINT NOT NULL,
    [ContentType] NVARCHAR(100) NOT NULL,
    [ImageUrl] NVARCHAR(500) NOT NULL,
    [ThumbnailUrl] NVARCHAR(500) NULL,
    [AltText] NVARCHAR(200) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsApproved] BIT NOT NULL DEFAULT 0,
    [UploadedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ApprovedDate] DATETIME2(7) NULL,
    [ApprovedBy] NVARCHAR(450) NULL,
    
    CONSTRAINT [PK_BusinessImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessImages_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BusinessImages_AspNetUsers_ApprovedBy] FOREIGN KEY ([ApprovedBy]) 
        REFERENCES [dbo].[AspNetUsers] ([Id])
);
```

#### BusinessDocuments
```sql
CREATE TABLE [dbo].[BusinessDocuments] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [DocumentType] NVARCHAR(50) NOT NULL, -- Menu, Brochure, PriceList, etc.
    [FileName] NVARCHAR(255) NOT NULL,
    [OriginalFileName] NVARCHAR(255) NOT NULL,
    [FileSize] BIGINT NOT NULL,
    [ContentType] NVARCHAR(100) NOT NULL,
    [DocumentUrl] NVARCHAR(500) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsApproved] BIT NOT NULL DEFAULT 0,
    [UploadedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ApprovedDate] DATETIME2(7) NULL,
    [ApprovedBy] NVARCHAR(450) NULL,
    
    CONSTRAINT [PK_BusinessDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessDocuments_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BusinessDocuments_AspNetUsers_ApprovedBy] FOREIGN KEY ([ApprovedBy]) 
        REFERENCES [dbo].[AspNetUsers] ([Id])
);
```

### 6. Subscription & Payment Tables

#### SubscriptionPlans
```sql
CREATE TABLE [dbo].[SubscriptionPlans] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [MonthlyPrice] DECIMAL(10,2) NOT NULL,
    [YearlyPrice] DECIMAL(10,2) NULL,
    [MaxBusinesses] INT NOT NULL,
    [MaxImages] INT NOT NULL,
    [MaxDocuments] INT NOT NULL,
    [Features] NVARCHAR(1000) NULL, -- JSON array of features
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id])
);
```

#### UserSubscriptions
```sql
CREATE TABLE [dbo].[UserSubscriptions] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [PlanId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL, -- Active, Inactive, Suspended, Cancelled
    [StartDate] DATETIME2(7) NOT NULL,
    [EndDate] DATETIME2(7) NULL,
    [AutoRenew] BIT NOT NULL DEFAULT 1,
    [PaymentMethod] NVARCHAR(50) NULL,
    [LastPaymentDate] DATETIME2(7) NULL,
    [NextPaymentDate] DATETIME2(7) NULL,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_UserSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserSubscriptions_AspNetUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_UserSubscriptions_SubscriptionPlans] FOREIGN KEY ([PlanId]) 
        REFERENCES [dbo].[SubscriptionPlans] ([Id])
);
```

#### PaymentHistory
```sql
CREATE TABLE [dbo].[PaymentHistory] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [SubscriptionId] INT NOT NULL,
    [PaymentGateway] NVARCHAR(50) NOT NULL, -- PayGate, PayFast, etc.
    [TransactionId] NVARCHAR(100) NOT NULL,
    [Amount] DECIMAL(10,2) NOT NULL,
    [Currency] NVARCHAR(3) NOT NULL DEFAULT 'ZAR',
    [Status] NVARCHAR(20) NOT NULL, -- Pending, Completed, Failed, Refunded
    [PaymentMethod] NVARCHAR(50) NULL,
    [GatewayResponse] NVARCHAR(MAX) NULL, -- JSON response from gateway
    [PaymentDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ProcessedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_PaymentHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentHistory_UserSubscriptions] FOREIGN KEY ([SubscriptionId]) 
        REFERENCES [dbo].[UserSubscriptions] ([Id])
);
```

### 7. Analytics & Reporting Tables

#### PageViews
```sql
CREATE TABLE [dbo].[PageViews] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [SessionId] NVARCHAR(100) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [Referrer] NVARCHAR(500) NULL,
    [PageType] NVARCHAR(50) NULL, -- Business, Category, Search
    [ViewDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_PageViews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PageViews_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id])
);
```

#### SearchLogs
```sql
CREATE TABLE [dbo].[SearchLogs] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [SearchTerm] NVARCHAR(200) NOT NULL,
    [CategoryId] INT NULL,
    [TownId] INT NULL,
    [Latitude] DECIMAL(10, 8) NULL,
    [Longitude] DECIMAL(11, 8) NULL,
    [Radius] INT NULL, -- Search radius in km
    [ResultCount] INT NOT NULL,
    [SessionId] NVARCHAR(100) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [SearchDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_SearchLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SearchLogs_BusinessCategories] FOREIGN KEY ([CategoryId]) 
        REFERENCES [dbo].[BusinessCategories] ([Id]),
    CONSTRAINT [FK_SearchLogs_Towns] FOREIGN KEY ([TownId]) 
        REFERENCES [dbo].[Towns] ([Id])
);
```

#### ContactActions
```sql
CREATE TABLE [dbo].[ContactActions] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [BusinessId] INT NOT NULL,
    [ActionType] NVARCHAR(50) NOT NULL, -- Phone, Email, Website, Directions
    [SessionId] NVARCHAR(100) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [ActionDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_ContactActions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactActions_Businesses] FOREIGN KEY ([BusinessId]) 
        REFERENCES [dbo].[Businesses] ([Id])
);
```

### 8. System Configuration Tables

#### SystemSettings
```sql
CREATE TABLE [dbo].[SystemSettings] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [SettingKey] NVARCHAR(100) NOT NULL,
    [SettingValue] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME2(7) NULL,
    
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_SystemSettings_SettingKey] UNIQUE ([SettingKey])
);
```

#### AuditLogs
```sql
CREATE TABLE [dbo].[AuditLogs] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NULL,
    [Action] NVARCHAR(100) NOT NULL,
    [EntityType] NVARCHAR(100) NOT NULL,
    [EntityId] NVARCHAR(100) NULL,
    [OldValues] NVARCHAR(MAX) NULL, -- JSON
    [NewValues] NVARCHAR(MAX) NULL, -- JSON
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [ActionDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_AspNetUsers] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id])
);
```

---

## Indexes for Performance

### Primary Indexes
```sql
-- Business search optimization
CREATE NONCLUSTERED INDEX [IX_Businesses_TownId_CategoryId_Status] 
ON [dbo].[Businesses] ([TownId], [CategoryId], [Status])
INCLUDE ([Name], [Description], [Latitude], [Longitude]);

-- Location-based search
CREATE NONCLUSTERED INDEX [IX_BusinessLocations_Latitude_Longitude] 
ON [dbo].[BusinessLocations] ([Latitude], [Longitude])
INCLUDE ([BusinessId], [Address]);

-- User business management
CREATE NONCLUSTERED INDEX [IX_Businesses_UserId_Status] 
ON [dbo].[Businesses] ([UserId], [Status])
INCLUDE ([Name], [CreatedDate]);

-- Analytics optimization
CREATE NONCLUSTERED INDEX [IX_PageViews_BusinessId_ViewDate] 
ON [dbo].[PageViews] ([BusinessId], [ViewDate]);

CREATE NONCLUSTERED INDEX [IX_SearchLogs_SearchDate] 
ON [dbo].[SearchLogs] ([SearchDate])
INCLUDE ([SearchTerm], [CategoryId], [TownId]);

-- Payment tracking
CREATE NONCLUSTERED INDEX [IX_PaymentHistory_Status_PaymentDate] 
ON [dbo].[PaymentHistory] ([Status], [PaymentDate])
INCLUDE ([SubscriptionId], [Amount]);
```

---

## Administrative Capabilities

### 1. User Management
- **User Registration**: Track new user registrations
- **User Status**: Active, inactive, suspended users
- **User Verification**: Email verification status
- **User Analytics**: Registration trends, login patterns

### 2. Business Management
- **Business Approval**: Pending business approvals
- **Business Status**: Active, inactive, suspended businesses
- **Business Verification**: Manual verification process
- **Featured Businesses**: Manage featured business listings

### 3. Content Moderation
- **Image Approval**: Review and approve uploaded images
- **Document Approval**: Review and approve uploaded documents
- **Content Guidelines**: Enforce content standards
- **Reported Content**: Handle user-reported inappropriate content

### 4. Geographic Management
- **Town Management**: Add, edit, and manage towns
- **Province Management**: Organize towns by provinces
- **Location Validation**: Verify business locations
- **Coverage Analysis**: Geographic coverage reports

### 5. Analytics & Reporting
- **Business Performance**: View counts, contact actions
- **User Engagement**: Search patterns, popular categories
- **Revenue Analytics**: Subscription and payment reports
- **Geographic Insights**: Popular towns and areas

### 6. System Configuration
- **Subscription Plans**: Manage pricing and features
- **Payment Gateways**: Configure payment processors
- **Email Templates**: Manage system emails
- **System Settings**: Application configuration

---

## Data Seeding

### Initial Data Requirements

#### Business Categories
```sql
INSERT INTO [dbo].[BusinessCategories] ([Name], [Description], [IconClass], [ColorCode], [DisplayOrder]) VALUES
('Shops & Retail', 'Local shops and retail businesses', 'fas fa-shopping-bag', '#007bff', 1),
('Restaurants & Food', 'Restaurants, cafes, and food services', 'fas fa-utensils', '#28a745', 2),
('Markets & Vendors', 'Local markets and vendor stalls', 'fas fa-store', '#ffc107', 3),
('Accommodation', 'Hotels, guesthouses, and lodging', 'fas fa-bed', '#17a2b8', 4),
('Tours & Experiences', 'Tour guides and experience providers', 'fas fa-map-marked-alt', '#6f42c1', 5),
('Events', 'Local events and entertainment', 'fas fa-calendar-alt', '#dc3545', 6);
```

#### Subscription Plans
```sql
INSERT INTO [dbo].[SubscriptionPlans] ([Name], [Description], [MonthlyPrice], [MaxBusinesses], [MaxImages], [MaxDocuments], [Features]) VALUES
('Basic', 'Perfect for small businesses', 99.00, 1, 5, 0, '["Basic listing", "Contact information", "Operating hours"]'),
('Standard', 'Great for growing businesses', 199.00, 3, 15, 5, '["Multiple listings", "Document uploads", "Basic analytics", "Priority support"]'),
('Premium', 'For established businesses', 399.00, 10, -1, -1, '["Unlimited listings", "Unlimited uploads", "Advanced analytics", "Featured placement", "Dedicated support"]');
```

#### System Settings
```sql
INSERT INTO [dbo].[SystemSettings] ([SettingKey], [SettingValue], [Description]) VALUES
('SiteName', 'MyTown', 'Application name'),
('SiteDescription', 'South African Small Business Information Gateway', 'Application description'),
('ContactEmail', 'admin@mytown.co.za', 'Primary contact email'),
('SupportPhone', '+27 11 123 4567', 'Support phone number'),
('MaxImageSize', '10485760', 'Maximum image file size in bytes (10MB)'),
('MaxDocumentSize', '26214400', 'Maximum document file size in bytes (25MB)'),
('ImageFormats', 'jpg,jpeg,png,webp', 'Allowed image formats'),
('DocumentFormats', 'pdf', 'Allowed document formats');
```

---

## Security Considerations

### 1. Data Protection
- **Encryption**: Sensitive data encrypted at rest
- **Access Control**: Role-based permissions
- **Audit Trail**: Complete audit logging
- **Data Retention**: Automated data cleanup policies

### 2. Payment Security
- **PCI Compliance**: Secure payment processing
- **Tokenization**: Payment method tokenization
- **Fraud Detection**: Payment fraud monitoring
- **Secure Storage**: Encrypted payment data

### 3. User Privacy
- **POPIA Compliance**: South African data protection
- **Consent Management**: User consent tracking
- **Data Portability**: User data export capabilities
- **Right to Deletion**: User data deletion requests

---

## Maintenance & Optimization

### 1. Database Maintenance
- **Index Rebuilding**: Monthly index maintenance
- **Statistics Updates**: Regular statistics updates
- **Data Archiving**: Archive old analytics data
- **Backup Strategy**: Daily backups with point-in-time recovery

### 2. Performance Monitoring
- **Query Performance**: Monitor slow queries
- **Index Usage**: Track index effectiveness
- **Connection Pooling**: Optimize database connections
- **Caching Strategy**: Implement application-level caching

---

*This database design provides a solid foundation for the MyTown application with comprehensive business management, content handling, analytics, and administrative capabilities.*

**Document Version**: 1.0  
**Last Updated**: January 2025  
**Next Review**: Before implementation 