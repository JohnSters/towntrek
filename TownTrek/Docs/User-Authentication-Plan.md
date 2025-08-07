# TownTrek User Authentication & Authorization Plan
## ASP.NET Core Identity with External Providers & Secure Payment Integration

### Overview

TownTrek requires a robust, secure user authentication and authorization system supporting multiple authentication methods and safe payment processing. This document outlines the implementation strategy using ASP.NET Core Identity with external authentication providers (Google, Facebook), PayFast payment gateway integration, and industry-standard security practices.

### Security-First Approach
This plan prioritizes:
- **Zero payment data storage** - No credit card or banking information stored locally
- **External authentication integration** - Leverage trusted OAuth providers
- **Minimal sensitive data retention** - Store only essential business information
- **PCI DSS compliance avoidance** - By not handling payment data directly
- **POPIA compliance** - South African data protection requirements

---

## Current Identity Infrastructure

### ASP.NET Core Identity Tables
The application currently uses `IdentityDbContext` which automatically creates the following tables:

- **AspNetUsers**: Core user information (Id, UserName, Email, PasswordHash, etc.)
- **AspNetRoles**: Role definitions (Id, Name, NormalizedName)
- **AspNetUserRoles**: Many-to-many relationship between users and roles
- **AspNetUserClaims**: Additional user claims/permissions
- **AspNetRoleClaims**: Role-based claims
- **AspNetUserLogins**: External login providers (Google, Facebook, etc.)
- **AspNetUserTokens**: Security tokens for password resets, email confirmation

### Current Database Context
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    // Towns and Businesses entities
    public DbSet<Town> Towns { get; set; }
    public DbSet<Business> Businesses { get; set; }
}
```

---

## User Types & Roles

### 1. Members (Community Users)
**Role**: `Member`
**Access Level**: Basic consumer access
**Capabilities**:
- Browse business listings
- Leave reviews and ratings
- Save favorite businesses
- View business contact information
- No subscription required (Free)

**Registration Requirements**:
- Email address
- Password
- Basic profile information
- Email verification

### 2. Clients/Business Owners
**Roles**: `Client-Basic`, `Client-Standard`, `Client-Premium`
**Access Level**: Business management portal
**Capabilities by Tier**:

#### Basic Tier (R199/month)
- 1 business listing
- Basic information management
- 5 image uploads
- Standard support

#### Standard Tier (R399/month)
- 3 business listings
- Advanced information management
- 15 image uploads
- PDF document uploads
- Priority support
- Basic analytics

#### Premium Tier (R599/month)
- 10 business listings
- Full feature access
- Unlimited image uploads
- Advanced analytics
- Featured placement
- Dedicated support

**Registration Requirements**:
- Full name
- Email address
- Phone number
- Physical location
- Business information
- Subscription plan selection
- Payment processing

### 3. Administrators
**Role**: `Admin`
**Access Level**: Full system administration
**Capabilities**:
- Manage all towns and businesses
- Approve/reject business listings
- Manage user accounts
- System configuration
- Analytics and reporting
- Content moderation

**Access Method**:
- Pre-created accounts (no public registration)
- Direct database seeding or admin creation

---

## Extended User Model

### Custom User Properties
We need to extend the default `IdentityUser` to include additional properties:

```csharp
public class ApplicationUser : IdentityUser
{
    // Common Properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Client-Specific Properties
    public string? SubscriptionTier { get; set; } // Basic, Standard, Premium
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public bool IsSubscriptionActive { get; set; } = false;
    public string? PaymentCustomerId { get; set; } // For payment gateway
    
    // Business Relationship
    public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();
    
    // Member-Specific Properties
    public virtual ICollection<BusinessReview> Reviews { get; set; } = new List<BusinessReview>();
    public virtual ICollection<FavoriteBusiness> FavoriteBusinesses { get; set; } = new List<FavoriteBusiness>();
}
```

### Business Ownership Model
```csharp
public class Business
{
    // ... existing properties
    
    // Owner relationship
    public string OwnerId { get; set; } = string.Empty;
    public virtual ApplicationUser Owner { get; set; } = null!;
    
    // Subscription tier validation
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedById { get; set; }
    public virtual ApplicationUser? ApprovedBy { get; set; }
}
```

---

## Role-Based Authorization Strategy

### Role Definitions
```csharp
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string ClientBasic = "Client-Basic";
    public const string ClientStandard = "Client-Standard";
    public const string ClientPremium = "Client-Premium";
    public const string Member = "Member";
    
    public static readonly string[] ClientRoles = { ClientBasic, ClientStandard, ClientPremium };
    public static readonly string[] AllRoles = { Admin, ClientBasic, ClientStandard, ClientPremium, Member };
}
```

### Authorization Policies
```csharp
public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireClient = "RequireClient";
    public const string RequireMember = "RequireMember";
    public const string RequireActiveSubscription = "RequireActiveSubscription";
    public const string RequireBusinessOwner = "RequireBusinessOwner";
}
```

### Controller Authorization Examples
```csharp
[Authorize(Roles = UserRoles.Admin)]
public class AdminController : Controller { }

[Authorize(Policy = AuthorizationPolicies.RequireClient)]
public class ClientController : Controller { }

[Authorize(Policy = AuthorizationPolicies.RequireMember)]
public class ReviewController : Controller { }
```

---

## Business Logic Constraints

### Subscription Tier Limitations
1. **Business Count Validation**:
   - Basic: Maximum 1 business
   - Standard: Maximum 3 businesses
   - Premium: Maximum 10 businesses

2. **Image Upload Limits**:
   - Basic: 5 images per business
   - Standard: 15 images per business
   - Premium: Unlimited images

3. **Feature Access**:
   - PDF uploads: Standard and Premium only
   - Analytics: Standard and Premium only
   - Featured placement: Premium only

### Business Ownership Rules (Updated for Dynamic Limits)
1. Only active subscription holders can create businesses
2. Business creation validated against current tier limits
3. Business listings are automatically deactivated if subscription expires
4. Business ownership cannot be transferred without admin approval
5. Each business must be associated with exactly one owner
6. Tier downgrades validated against current business count

### Dynamic Business Validation Service
```csharp
public class BusinessValidationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task<ValidationResult> ValidateBusinessCreationAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ValidationResult.Error("User not found");

        if (!user.HasActiveSubscription)
            return ValidationResult.Error("Active subscription required to create businesses");

        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionTier)
            .ThenInclude(t => t.Limits)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (subscription == null)
            return ValidationResult.Error("No active subscription found");

        // Check business count limit
        var maxBusinesses = GetTierLimit(subscription.SubscriptionTier, "MaxBusinesses");
        if (maxBusinesses != -1) // -1 means unlimited
        {
            var currentBusinessCount = await _context.Businesses
                .CountAsync(b => b.OwnerId == userId && b.IsActive);

            if (currentBusinessCount >= maxBusinesses)
            {
                return ValidationResult.Error($"Business limit reached. Your {subscription.SubscriptionTier.DisplayName} allows {maxBusinesses} business(es). Upgrade your plan to add more businesses.");
            }
        }

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateImageUploadAsync(string userId, int businessId, int additionalImageCount)
    {
        var subscription = await GetUserActiveSubscriptionAsync(userId);
        if (subscription == null)
            return ValidationResult.Error("Active subscription required");

        var maxImages = GetTierLimit(subscription.SubscriptionTier, "MaxImages");
        if (maxImages == -1) // Unlimited
            return ValidationResult.Success();

        var currentImageCount = await _context.BusinessImages
            .CountAsync(i => i.BusinessId == businessId);

        if (currentImageCount + additionalImageCount > maxImages)
        {
            return ValidationResult.Error($"Image limit exceeded. Your {subscription.SubscriptionTier.DisplayName} allows {maxImages} images per business.");
        }

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidatePDFUploadAsync(string userId, int businessId)
    {
        var subscription = await GetUserActiveSubscriptionAsync(userId);
        if (subscription == null)
            return ValidationResult.Error("Active subscription required");

        var maxPDFs = GetTierLimit(subscription.SubscriptionTier, "MaxPDFs");
        if (maxPDFs == 0)
        {
            return ValidationResult.Error($"PDF uploads not available in your {subscription.SubscriptionTier.DisplayName}. Upgrade to Standard or Premium plan.");
        }

        if (maxPDFs == -1) // Unlimited
            return ValidationResult.Success();

        var currentPDFCount = await _context.BusinessDocuments
            .CountAsync(d => d.BusinessId == businessId && d.DocumentType == "PDF");

        if (currentPDFCount >= maxPDFs)
        {
            return ValidationResult.Error($"PDF limit reached. Your {subscription.SubscriptionTier.DisplayName} allows {maxPDFs} PDF(s) per business.");
        }

        return ValidationResult.Success();
    }

    public async Task<bool> HasFeatureAccessAsync(string userId, string featureKey)
    {
        var subscription = await GetUserActiveSubscriptionAsync(userId);
        if (subscription == null)
            return false;

        var feature = subscription.SubscriptionTier.Features
            .FirstOrDefault(f => f.FeatureKey == featureKey);

        return feature?.IsEnabled ?? false;
    }

    private async Task<Subscription?> GetUserActiveSubscriptionAsync(string userId)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionTier)
            .ThenInclude(t => t.Limits)
            .Include(s => s.SubscriptionTier)
            .ThenInclude(t => t.Features)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
    }

    private int GetTierLimit(SubscriptionTier tier, string limitType)
    {
        var limit = tier.Limits.FirstOrDefault(l => l.LimitType == limitType);
        return limit?.LimitValue ?? 0;
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Error(string message) => new() { IsValid = false, ErrorMessage = message };
}
```

### Updated Client Controller with Dynamic Validation
```csharp
[Authorize(Policy = AuthorizationPolicies.RequireClient)]
public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BusinessValidationService _businessValidation;

    [HttpGet]
    public async Task<IActionResult> AddBusiness()
    {
        var userId = _userManager.GetUserId(User);
        var validationResult = await _businessValidation.ValidateBusinessCreationAsync(userId);

        if (!validationResult.IsValid)
        {
            TempData["ErrorMessage"] = validationResult.ErrorMessage;
            return RedirectToAction("Dashboard");
        }

        // Load towns for dropdown
        ViewBag.Towns = await _context.Towns
            .Where(t => t.IsActive)
            .OrderBy(t => t.Province)
            .ThenBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, t.Province })
            .ToListAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddBusiness(AddBusinessViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        
        // Validate business creation limits
        var validationResult = await _businessValidation.ValidateBusinessCreationAsync(userId);
        if (!validationResult.IsValid)
        {
            ModelState.AddModelError("", validationResult.ErrorMessage);
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var business = new Business
            {
                Name = model.BusinessName,
                Category = model.BusinessCategory,
                Description = model.BusinessDescription,
                PhoneNumber = model.PhoneNumber,
                EmailAddress = model.EmailAddress,
                Website = model.Website,
                PhysicalAddress = model.PhysicalAddress,
                TownId = model.TownId,
                OwnerId = userId,
                SpecialOffers = model.SpecialOffers,
                AdditionalNotes = model.AdditionalNotes
            };

            await _context.Businesses.AddAsync(business);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Business added successfully!";
            return RedirectToAction("Dashboard");
        }

        return View(model);
    }
}
```

---

## Authentication Methods

### 1. Email/Password Registration
**Traditional registration with email verification**
- User provides email, password, and basic information
- Email verification required before account activation
- Strong password requirements enforced
- Account lockout protection

### 2. Google OAuth Authentication
**Integration with Google Identity Platform**
- OAuth 2.0 flow implementation
- Automatic account creation on first login
- Profile information retrieval (name, email, profile picture)
- No password storage required

### 3. Facebook Login
**Facebook Login for Web integration**
- Facebook SDK implementation
- OAuth 2.0 flow with Facebook Graph API
- Profile data retrieval with user consent
- Automatic account linking

### External Authentication Configuration
```csharp
// Startup.cs / Program.cs
services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
    })
    .AddFacebook(options =>
    {
        options.AppId = configuration["Authentication:Facebook:AppId"];
        options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
        options.CallbackPath = "/signin-facebook";
    });
```

---

## PayFast Payment Integration Strategy

### Security-First Payment Approach
**We DO NOT store any payment information locally**

### What We Store (Safe Data Only):
```csharp
public class Subscription
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty; // Basic, Standard, Premium
    public decimal MonthlyPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // PayFast Integration - SAFE DATA ONLY
    public string? PayFastToken { get; set; } // PayFast subscription token
    public string? PayFastPaymentId { get; set; } // PayFast payment reference
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Active, Cancelled, Failed
    
    // NO CREDIT CARD DATA - NEVER STORED
    public virtual ApplicationUser User { get; set; } = null!;
}
```

### What We NEVER Store:
- ❌ Credit card numbers
- ❌ CVV codes
- ❌ Banking details
- ❌ Payment method information
- ❌ Personal financial data

### PayFast Integration Flow

#### 1. Subscription Creation Process
```csharp
public class PayFastSubscriptionService
{
    public async Task<PayFastRedirectResult> CreateSubscriptionAsync(string userId, string tier)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var subscription = new Subscription
        {
            UserId = userId,
            Tier = tier,
            MonthlyPrice = GetTierPrice(tier),
            StartDate = DateTime.UtcNow,
            PaymentStatus = "Pending"
        };
        
        // Save subscription with pending status
        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
        
        // Create PayFast payment request
        var paymentRequest = new PayFastPaymentRequest
        {
            merchant_id = _payFastSettings.MerchantId,
            merchant_key = _payFastSettings.MerchantKey,
            return_url = $"{_baseUrl}/payment/success",
            cancel_url = $"{_baseUrl}/payment/cancel",
            notify_url = $"{_baseUrl}/payment/notify",
            
            // Subscription details
            subscription_type = "1", // Recurring
            billing_date = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd"),
            recurring_amount = subscription.MonthlyPrice.ToString("F2"),
            frequency = "3", // Monthly
            cycles = "0", // Indefinite
            
            // Order details
            m_payment_id = subscription.Id.ToString(),
            amount = subscription.MonthlyPrice.ToString("F2"),
            item_name = $"TownTrek {tier} Subscription",
            item_description = $"Monthly subscription for {tier} plan"
        };
        
        // Generate signature for security
        paymentRequest.signature = GeneratePayFastSignature(paymentRequest);
        
        return new PayFastRedirectResult
        {
            PaymentUrl = _payFastSettings.PaymentUrl,
            PaymentData = paymentRequest
        };
    }
}
```

#### 2. Payment Notification Handling (ITN)
```csharp
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> PaymentNotification()
{
    // Verify PayFast ITN (Instant Transaction Notification)
    var formData = await Request.ReadFormAsync();
    
    // Security validation
    if (!ValidatePayFastNotification(formData))
    {
        return BadRequest("Invalid notification");
    }
    
    var paymentId = formData["m_payment_id"];
    var paymentStatus = formData["payment_status"];
    var payFastToken = formData["token"];
    
    var subscription = await _context.Subscriptions
        .FirstOrDefaultAsync(s => s.Id.ToString() == paymentId);
    
    if (subscription != null)
    {
        subscription.PayFastToken = payFastToken;
        subscription.PaymentStatus = paymentStatus == "COMPLETE" ? "Active" : "Failed";
        subscription.LastPaymentDate = DateTime.UtcNow;
        subscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
        subscription.IsActive = paymentStatus == "COMPLETE";
        
        // Update user role based on subscription
        if (subscription.IsActive)
        {
            await UpdateUserSubscriptionRole(subscription.UserId, subscription.Tier);
        }
        
        await _context.SaveChangesAsync();
    }
    
    return Ok();
}
```

### PayFast Security Measures

#### 1. Signature Validation
```csharp
private bool ValidatePayFastNotification(IFormCollection formData)
{
    // Remove signature from data
    var dataToValidate = formData.Where(x => x.Key != "signature")
        .OrderBy(x => x.Key)
        .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}")
        .Aggregate((x, y) => $"{x}&{y}");
    
    // Add passphrase if configured
    if (!string.IsNullOrEmpty(_payFastSettings.Passphrase))
    {
        dataToValidate += $"&passphrase={Uri.EscapeDataString(_payFastSettings.Passphrase)}";
    }
    
    // Generate MD5 hash
    var hash = GenerateMD5Hash(dataToValidate);
    var receivedSignature = formData["signature"];
    
    return hash.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);
}
```

#### 2. IP Address Validation
```csharp
private bool IsValidPayFastIP(string ipAddress)
{
    var validIPs = new[]
    {
        "197.97.145.144", "197.97.145.145", "197.97.145.146", "197.97.145.147",
        "197.97.145.148", "197.97.145.149", "197.97.145.150", "197.97.145.151",
        "197.97.145.152", "197.97.145.153", "197.97.145.154", "197.97.145.155"
    };
    
    return validIPs.Contains(ipAddress);
}
```

---

## Registration & Authentication Flows

### Member Registration Flow

#### Option 1: Email Registration
1. User selects "Sign up with Email"
2. User fills basic information form
3. Email verification sent
4. User clicks verification link
5. Account activated with `Member` role
6. Redirect to member dashboard

#### Option 2: Google Registration
1. User clicks "Continue with Google"
2. OAuth redirect to Google
3. User authorizes application
4. Google returns user profile data
5. Account created automatically with `Member` role
6. Redirect to member dashboard

#### Option 3: Facebook Registration
1. User clicks "Continue with Facebook"
2. OAuth redirect to Facebook
3. User authorizes application
4. Facebook returns user profile data
5. Account created automatically with `Member` role
6. Redirect to member dashboard

### Client Registration Flow

#### Step 1: Account Creation
1. User selects "Business Owner" account type
2. User chooses authentication method:
   - Email/Password
   - Google OAuth
   - Facebook Login
3. Account created with basic information
4. Email verification (if email registration)

#### Step 2: Business Information
1. User completes business owner profile
2. User selects subscription tier
3. Business information collected
4. Account marked as "Pending Payment"

#### Step 3: Payment Processing
1. User redirected to PayFast payment gateway
2. User completes payment on PayFast (secure, external)
3. PayFast processes payment
4. PayFast sends ITN (Instant Transaction Notification)
5. Our system receives payment confirmation
6. Subscription activated
7. User role updated to appropriate `Client-*` role
8. User redirected to client dashboard

### Login Process
1. User selects authentication method
2. Authentication completed (local or external)
3. Role-based redirection:
   - Admin → `/Admin/Dashboard`
   - Client → `/Client/Dashboard`
   - Member → `/Home/Index`

---

## Extended User Model (Updated)

```csharp
public class ApplicationUser : IdentityUser
{
    // Common Properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // External Authentication
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string AuthenticationMethod { get; set; } = "Email"; // Email, Google, Facebook
    
    // Subscription Information (NO PAYMENT DATA)
    public string? CurrentSubscriptionTier { get; set; }
    public bool HasActiveSubscription { get; set; } = false;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    
    // Relationships
    public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();
    public virtual ICollection<BusinessReview> Reviews { get; set; } = new List<BusinessReview>();
    public virtual ICollection<FavoriteBusiness> FavoriteBusinesses { get; set; } = new List<FavoriteBusiness>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
```

---

## Database Schema Updates

### ApplicationDbContext Updates
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Town> Towns { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessReview> BusinessReviews { get; set; }
    public DbSet<FavoriteBusiness> FavoriteBusinesses { get; set; }
    
    // Dynamic Subscription System
    public DbSet<SubscriptionTier> SubscriptionTiers { get; set; }
    public DbSet<SubscriptionTierLimit> SubscriptionTierLimits { get; set; }
    public DbSet<SubscriptionTierFeature> SubscriptionTierFeatures { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PriceChangeHistory> PriceChangeHistory { get; set; }
    
    // System Configuration
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CurrentSubscriptionTier).HasMaxLength(20);
        });

        // Configure Business-User relationship
        builder.Entity<Business>(entity =>
        {
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.Businesses)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SubscriptionTier
        builder.Entity<SubscriptionTier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(10,2)");
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.SortOrder);
        });

        // Configure SubscriptionTierLimit
        builder.Entity<SubscriptionTierLimit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LimitType).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.SubscriptionTier)
                  .WithMany(t => t.Limits)
                  .HasForeignKey(e => e.SubscriptionTierId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.SubscriptionTierId, e.LimitType }).IsUnique();
        });

        // Configure SubscriptionTierFeature
        builder.Entity<SubscriptionTierFeature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FeatureKey).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FeatureName).HasMaxLength(100);
            
            entity.HasOne(e => e.SubscriptionTier)
                  .WithMany(t => t.Features)
                  .HasForeignKey(e => e.SubscriptionTierId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.SubscriptionTierId, e.FeatureKey }).IsUnique();
        });

        // Configure Subscription
        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(10,2)");
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Subscriptions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.SubscriptionTier)
                  .WithMany(t => t.Subscriptions)
                  .HasForeignKey(e => e.SubscriptionTierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SystemConfiguration
        builder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfigKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConfigValue).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(20);
            
            entity.HasIndex(e => e.ConfigKey).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // Configure PriceChangeHistory
        builder.Entity<PriceChangeHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.NewPrice).HasColumnType("decimal(10,2)");
            
            entity.HasOne(e => e.SubscriptionTier)
                  .WithMany()
                  .HasForeignKey(e => e.SubscriptionTierId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.ChangedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ChangedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed default data
        SeedRoles(builder);
        SeedAdminUser(builder);
        SeedDefaultSubscriptionTiers(builder);
        SeedSystemConfiguration(builder);
    }

    private void SeedDefaultSubscriptionTiers(ModelBuilder builder)
    {
        // Seed default subscription tiers
        builder.Entity<SubscriptionTier>().HasData(
            new SubscriptionTier
            {
                Id = 1,
                Name = "Basic",
                DisplayName = "Basic Plan",
                Description = "Perfect for small businesses getting started",
                MonthlyPrice = 199.00m,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SubscriptionTier
            {
                Id = 2,
                Name = "Standard",
                DisplayName = "Standard Plan",
                Description = "Great for growing businesses with multiple locations",
                MonthlyPrice = 399.00m,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SubscriptionTier
            {
                Id = 3,
                Name = "Premium",
                DisplayName = "Premium Plan",
                Description = "Full-featured plan for established businesses",
                MonthlyPrice = 599.00m,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed tier limits
        builder.Entity<SubscriptionTierLimit>().HasData(
            // Basic tier limits
            new SubscriptionTierLimit { Id = 1, SubscriptionTierId = 1, LimitType = "MaxBusinesses", LimitValue = 1 },
            new SubscriptionTierLimit { Id = 2, SubscriptionTierId = 1, LimitType = "MaxImages", LimitValue = 5 },
            new SubscriptionTierLimit { Id = 3, SubscriptionTierId = 1, LimitType = "MaxPDFs", LimitValue = 0 },
            
            // Standard tier limits
            new SubscriptionTierLimit { Id = 4, SubscriptionTierId = 2, LimitType = "MaxBusinesses", LimitValue = 3 },
            new SubscriptionTierLimit { Id = 5, SubscriptionTierId = 2, LimitType = "MaxImages", LimitValue = 15 },
            new SubscriptionTierLimit { Id = 6, SubscriptionTierId = 2, LimitType = "MaxPDFs", LimitValue = 5 },
            
            // Premium tier limits
            new SubscriptionTierLimit { Id = 7, SubscriptionTierId = 3, LimitType = "MaxBusinesses", LimitValue = 10 },
            new SubscriptionTierLimit { Id = 8, SubscriptionTierId = 3, LimitType = "MaxImages", LimitValue = -1 }, // Unlimited
            new SubscriptionTierLimit { Id = 9, SubscriptionTierId = 3, LimitType = "MaxPDFs", LimitValue = -1 }  // Unlimited
        );

        // Seed tier features
        builder.Entity<SubscriptionTierFeature>().HasData(
            // Basic tier features
            new SubscriptionTierFeature { Id = 1, SubscriptionTierId = 1, FeatureKey = "BasicSupport", IsEnabled = true, FeatureName = "Standard Support" },
            
            // Standard tier features
            new SubscriptionTierFeature { Id = 2, SubscriptionTierId = 2, FeatureKey = "BasicSupport", IsEnabled = true, FeatureName = "Standard Support" },
            new SubscriptionTierFeature { Id = 3, SubscriptionTierId = 2, FeatureKey = "PrioritySupport", IsEnabled = true, FeatureName = "Priority Support" },
            new SubscriptionTierFeature { Id = 4, SubscriptionTierId = 2, FeatureKey = "BasicAnalytics", IsEnabled = true, FeatureName = "Basic Analytics" },
            new SubscriptionTierFeature { Id = 5, SubscriptionTierId = 2, FeatureKey = "PDFUploads", IsEnabled = true, FeatureName = "PDF Document Uploads" },
            
            // Premium tier features
            new SubscriptionTierFeature { Id = 6, SubscriptionTierId = 3, FeatureKey = "BasicSupport", IsEnabled = true, FeatureName = "Standard Support" },
            new SubscriptionTierFeature { Id = 7, SubscriptionTierId = 3, FeatureKey = "PrioritySupport", IsEnabled = true, FeatureName = "Priority Support" },
            new SubscriptionTierFeature { Id = 8, SubscriptionTierId = 3, FeatureKey = "DedicatedSupport", IsEnabled = true, FeatureName = "Dedicated Support" },
            new SubscriptionTierFeature { Id = 9, SubscriptionTierId = 3, FeatureKey = "AdvancedAnalytics", IsEnabled = true, FeatureName = "Advanced Analytics" },
            new SubscriptionTierFeature { Id = 10, SubscriptionTierId = 3, FeatureKey = "FeaturedPlacement", IsEnabled = true, FeatureName = "Featured Placement" },
            new SubscriptionTierFeature { Id = 11, SubscriptionTierId = 3, FeatureKey = "PDFUploads", IsEnabled = true, FeatureName = "PDF Document Uploads" }
        );
    }

    private void SeedSystemConfiguration(ModelBuilder builder)
    {
        builder.Entity<SystemConfiguration>().HasData(
            new SystemConfiguration { Id = 1, ConfigKey = "GracePeriodDays", ConfigValue = "7", DataType = "Integer", Description = "Days before subscription deactivation", Category = "Payment" },
            new SystemConfiguration { Id = 2, ConfigKey = "MaxFileSize", ConfigValue = "2097152", DataType = "Integer", Description = "Maximum file size in bytes (2MB)", Category = "Business" },
            new SystemConfiguration { Id = 3, ConfigKey = "EmailVerificationRequired", ConfigValue = "true", DataType = "Boolean", Description = "Require email verification for new accounts", Category = "Security" },
            new SystemConfiguration { Id = 4, ConfigKey = "AllowSelfRegistration", ConfigValue = "true", DataType = "Boolean", Description = "Allow public user registration", Category = "Security" }
        );
    }
}
```

### Additional Models

#### Core Business Models
```csharp
public class BusinessReview
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 stars
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Business Business { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}

public class FavoriteBusiness
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Business Business { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}
```

#### Dynamic Subscription Tier System
```csharp
public class SubscriptionTier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Basic, Standard, Premium
    public string DisplayName { get; set; } = string.Empty; // "Basic Plan", "Standard Plan"
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } // For display ordering
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedById { get; set; } // Admin who made changes
    
    // Navigation properties
    public virtual ICollection<SubscriptionTierLimit> Limits { get; set; } = new List<SubscriptionTierLimit>();
    public virtual ICollection<SubscriptionTierFeature> Features { get; set; } = new List<SubscriptionTierFeature>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ApplicationUser? UpdatedBy { get; set; }
}

public class SubscriptionTierLimit
{
    public int Id { get; set; }
    public int SubscriptionTierId { get; set; }
    public string LimitType { get; set; } = string.Empty; // "MaxBusinesses", "MaxImages", "MaxPDFs"
    public int LimitValue { get; set; } // -1 for unlimited
    public string? Description { get; set; }
    
    public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
}

public class SubscriptionTierFeature
{
    public int Id { get; set; }
    public int SubscriptionTierId { get; set; }
    public string FeatureKey { get; set; } = string.Empty; // "Analytics", "PrioritySupport", "FeaturedPlacement"
    public bool IsEnabled { get; set; } = true;
    public string? FeatureName { get; set; }
    public string? Description { get; set; }
    
    public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
}

public class Subscription
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SubscriptionTierId { get; set; } // Reference to SubscriptionTier
    public decimal MonthlyPrice { get; set; } // Price at time of subscription (for historical tracking)
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // PayFast Integration - SAFE DATA ONLY
    public string? PayFastToken { get; set; }
    public string? PayFastPaymentId { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
}
```

#### Admin Configuration System
```csharp
public class SystemConfiguration
{
    public int Id { get; set; }
    public string ConfigKey { get; set; } = string.Empty; // "GracePeriodDays", "MaxFileSize", etc.
    public string ConfigValue { get; set; } = string.Empty;
    public string DataType { get; set; } = "String"; // String, Integer, Decimal, Boolean
    public string? Description { get; set; }
    public string? Category { get; set; } // "Payment", "Business", "Security"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedById { get; set; }
    
    public virtual ApplicationUser? UpdatedBy { get; set; }
}

public class PriceChangeHistory
{
    public int Id { get; set; }
    public int SubscriptionTierId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    public string ChangedById { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public DateTime? EffectiveDate { get; set; } // When the change takes effect
    
    public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
    public virtual ApplicationUser ChangedBy { get; set; } = null!;
}
```

---

## Security Considerations & Best Practices

### Authentication Security

#### Password Requirements (Email Registration)
```csharp
services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});
```

#### External Authentication Security
- **OAuth 2.0 compliance** for Google and Facebook
- **State parameter validation** to prevent CSRF attacks
- **Secure token handling** with proper expiration
- **Profile data validation** from external providers

### Payment Security (PayFast Integration)

#### What Makes This Secure:
1. **No Payment Data Storage**: We never store credit card or banking information
2. **External Payment Processing**: All payment data handled by PayFast (PCI DSS compliant)
3. **Signature Validation**: All PayFast communications cryptographically verified
4. **IP Whitelisting**: Only accept notifications from verified PayFast IP addresses
5. **HTTPS Only**: All payment-related communications over encrypted connections

#### PayFast Security Configuration
```csharp
public class PayFastSettings
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty; // Additional security
    public string PaymentUrl { get; set; } = "https://www.payfast.co.za/eng/process";
    public string SandboxUrl { get; set; } = "https://sandbox.payfast.co.za/eng/process";
    public bool UseSandbox { get; set; } = false;
    
    // Security settings
    public bool ValidateIPAddress { get; set; } = true;
    public bool ValidateSignature { get; set; } = true;
    public int NotificationTimeoutSeconds { get; set; } = 30;
}
```

### Data Protection & Privacy

#### POPIA Compliance (South African Data Protection)
```csharp
public class DataProtectionService
{
    // Data minimization - only collect necessary data
    public class UserDataCollection
    {
        // Required for service
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        
        // Optional with explicit consent
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public bool MarketingConsent { get; set; } = false;
        
        // Audit trail
        public DateTime ConsentDate { get; set; }
        public string ConsentVersion { get; set; } = "1.0";
    }
    
    // Data retention policies
    public async Task ApplyDataRetentionPolicies()
    {
        // Delete inactive accounts after 2 years
        var cutoffDate = DateTime.UtcNow.AddYears(-2);
        var inactiveUsers = await _context.Users
            .Where(u => u.LastLoginAt < cutoffDate && !u.HasActiveSubscription)
            .ToListAsync();
            
        // Anonymize rather than delete to maintain business relationships
        foreach (var user in inactiveUsers)
        {
            await AnonymizeUserData(user);
        }
    }
}
```

#### Encryption & Data Security
```csharp
public class DataEncryptionService
{
    // Encrypt sensitive data at rest
    public string EncryptSensitiveData(string data)
    {
        // Use ASP.NET Core Data Protection API
        return _dataProtector.Protect(data);
    }
    
    // Decrypt when needed
    public string DecryptSensitiveData(string encryptedData)
    {
        return _dataProtector.Unprotect(encryptedData);
    }
}

// Configure in Startup
services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys/"))
    .SetApplicationName("TownTrek")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
```

### Security Headers & HTTPS
```csharp
// Configure security headers
app.UseHsts(); // HTTP Strict Transport Security
app.UseHttpsRedirection();

// Content Security Policy
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://apis.google.com https://connect.facebook.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://www.payfast.co.za;");
    
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});
```

### Audit Logging
```csharp
public class SecurityAuditService
{
    public async Task LogSecurityEvent(string userId, string eventType, string details, string ipAddress)
    {
        var auditLog = new SecurityAuditLog
        {
            UserId = userId,
            EventType = eventType, // Login, Registration, PaymentSuccess, PaymentFailed, etc.
            Details = details,
            IpAddress = ipAddress,
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"],
            Timestamp = DateTime.UtcNow
        };
        
        await _context.SecurityAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}

public class SecurityAuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

## Subscription Management & Business Logic

### Subscription Lifecycle Management

#### 1. Subscription Creation
```csharp
public class SubscriptionManagementService
{
    public async Task<SubscriptionResult> CreateSubscriptionAsync(string userId, string tier)
    {
        // Validate user eligibility
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return SubscriptionResult.UserNotFound();
        
        // Check for existing active subscription
        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
            
        if (existingSubscription != null)
        {
            return SubscriptionResult.AlreadyHasActiveSubscription();
        }
        
        // Create subscription record
        var subscription = new Subscription
        {
            UserId = userId,
            Tier = tier,
            MonthlyPrice = GetTierPrice(tier),
            StartDate = DateTime.UtcNow,
            PaymentStatus = "Pending",
            IsActive = false
        };
        
        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
        
        // Generate PayFast payment URL
        var paymentUrl = await _payFastService.CreatePaymentUrlAsync(subscription);
        
        return SubscriptionResult.Success(paymentUrl);
    }
}
```

#### 2. Automatic Renewal Handling
```csharp
public class SubscriptionRenewalService
{
    // Called by PayFast ITN for recurring payments
    public async Task HandleRecurringPaymentAsync(string payFastToken, string paymentStatus)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.PayFastToken == payFastToken);
            
        if (subscription == null) return;
        
        if (paymentStatus == "COMPLETE")
        {
            // Successful renewal
            subscription.LastPaymentDate = DateTime.UtcNow;
            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscription.PaymentStatus = "Active";
            subscription.IsActive = true;
            
            // Extend subscription period
            subscription.EndDate = DateTime.UtcNow.AddMonths(1);
            
            await _auditService.LogSecurityEvent(subscription.UserId, 
                "SubscriptionRenewed", $"Tier: {subscription.Tier}", "PayFast");
        }
        else
        {
            // Failed renewal - enter grace period
            subscription.PaymentStatus = "Failed";
            subscription.IsActive = false;
            
            await HandleFailedPaymentAsync(subscription);
        }
        
        await _context.SaveChangesAsync();
    }
}
```

#### 3. Failed Payment & Grace Period
```csharp
public async Task HandleFailedPaymentAsync(Subscription subscription)
{
    // 7-day grace period before deactivation
    var gracePeriodEnd = subscription.NextBillingDate?.AddDays(7) ?? DateTime.UtcNow.AddDays(7);
    
    if (DateTime.UtcNow > gracePeriodEnd)
    {
        // Grace period expired - deactivate businesses
        await DeactivateUserBusinessesAsync(subscription.UserId);
        
        // Downgrade user role to Member
        var user = await _userManager.FindByIdAsync(subscription.UserId);
        if (user != null)
        {
            await _userManager.RemoveFromRolesAsync(user, UserRoles.ClientRoles);
            await _userManager.AddToRoleAsync(user, UserRoles.Member);
            
            user.HasActiveSubscription = false;
            user.CurrentSubscriptionTier = null;
            await _userManager.UpdateAsync(user);
        }
        
        // Send notification email
        await _emailService.SendSubscriptionExpiredEmailAsync(user.Email, user.FirstName);
    }
    else
    {
        // Still in grace period - send reminder
        await _emailService.SendPaymentFailedReminderAsync(subscription);
    }
}
```

### Business Deactivation Logic
```csharp
public async Task DeactivateUserBusinessesAsync(string userId)
{
    var businesses = await _context.Businesses
        .Where(b => b.OwnerId == userId && b.IsActive)
        .ToListAsync();
        
    foreach (var business in businesses)
    {
        business.IsActive = false;
        business.DeactivatedAt = DateTime.UtcNow;
        business.DeactivationReason = "Subscription Expired";
    }
    
    await _context.SaveChangesAsync();
    
    // Log deactivation
    await _auditService.LogSecurityEvent(userId, "BusinessesDeactivated", 
        $"Count: {businesses.Count}", "System");
}
```

### Subscription Upgrade/Downgrade (Updated for Dynamic Tiers)
```csharp
public async Task<SubscriptionChangeResult> ChangeSubscriptionTierAsync(string userId, int newTierId)
{
    var currentSubscription = await _context.Subscriptions
        .Include(s => s.SubscriptionTier)
        .ThenInclude(t => t.Limits)
        .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        
    if (currentSubscription == null)
        return SubscriptionChangeResult.NoActiveSubscription();

    var newTier = await _context.SubscriptionTiers
        .Include(t => t.Limits)
        .FirstOrDefaultAsync(t => t.Id == newTierId && t.IsActive);
        
    if (newTier == null)
        return SubscriptionChangeResult.InvalidTier();
    
    // Check if downgrade is possible
    var currentMaxBusinesses = GetTierLimit(currentSubscription.SubscriptionTier, "MaxBusinesses");
    var newMaxBusinesses = GetTierLimit(newTier, "MaxBusinesses");
    
    if (newMaxBusinesses != -1 && newMaxBusinesses < currentMaxBusinesses)
    {
        var businessCount = await _context.Businesses
            .CountAsync(b => b.OwnerId == userId && b.IsActive);
            
        if (businessCount > newMaxBusinesses)
        {
            return SubscriptionChangeResult.TooManyBusinessesForDowngrade(businessCount, newMaxBusinesses);
        }
    }
    
    // Calculate prorated amount
    var proratedAmount = CalculateProratedAmount(currentSubscription, newTier);
    
    if (proratedAmount > 0)
    {
        // Upgrade - requires additional payment
        var paymentUrl = await _payFastService.CreateUpgradePaymentAsync(currentSubscription, newTier, proratedAmount);
        return SubscriptionChangeResult.RequiresPayment(paymentUrl);
    }
    else
    {
        // Downgrade - effective immediately
        currentSubscription.SubscriptionTierId = newTierId;
        currentSubscription.MonthlyPrice = newTier.MonthlyPrice;
        
        // Update user role
        var user = await _userManager.FindByIdAsync(userId);
        await _userManager.RemoveFromRolesAsync(user, UserRoles.ClientRoles);
        await _userManager.AddToRoleAsync(user, GetClientRole(newTier.Name));
        
        user.CurrentSubscriptionTier = newTier.Name;
        await _userManager.UpdateAsync(user);
        
        await _context.SaveChangesAsync();
        return SubscriptionChangeResult.Success();
    }
}

private int GetTierLimit(SubscriptionTier tier, string limitType)
{
    var limit = tier.Limits.FirstOrDefault(l => l.LimitType == limitType);
    return limit?.LimitValue ?? 0;
}
```

---

## Admin Management System

### Subscription Tier Management Service
```csharp
public class SubscriptionTierManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubscriptionTierManagementService> _logger;
    private readonly SecurityAuditService _auditService;

    public async Task<List<SubscriptionTier>> GetAllTiersAsync()
    {
        return await _context.SubscriptionTiers
            .Include(t => t.Limits)
            .Include(t => t.Features)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<SubscriptionTier?> GetTierByIdAsync(int id)
    {
        return await _context.SubscriptionTiers
            .Include(t => t.Limits)
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<AdminResult> UpdateTierPricingAsync(int tierId, decimal newPrice, string adminUserId, string? reason = null)
    {
        var tier = await _context.SubscriptionTiers.FindAsync(tierId);
        if (tier == null)
            return AdminResult.NotFound("Subscription tier not found");

        var oldPrice = tier.MonthlyPrice;
        
        // Record price change history
        var priceChange = new PriceChangeHistory
        {
            SubscriptionTierId = tierId,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            ChangedById = adminUserId,
            ChangeReason = reason,
            EffectiveDate = DateTime.UtcNow.AddDays(30) // 30-day notice for existing customers
        };

        await _context.PriceChangeHistory.AddAsync(priceChange);

        // Update tier price
        tier.MonthlyPrice = newPrice;
        tier.UpdatedAt = DateTime.UtcNow;
        tier.UpdatedById = adminUserId;

        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogSecurityEvent(adminUserId, "TierPriceChanged", 
            $"Tier: {tier.Name}, Old: R{oldPrice}, New: R{newPrice}", "Admin");

        // Notify existing customers (implement notification service)
        await NotifyCustomersOfPriceChangeAsync(tierId, oldPrice, newPrice);

        return AdminResult.Success();
    }

    public async Task<AdminResult> UpdateTierLimitAsync(int tierId, string limitType, int newLimit, string adminUserId)
    {
        var tier = await _context.SubscriptionTiers
            .Include(t => t.Limits)
            .FirstOrDefaultAsync(t => t.Id == tierId);
            
        if (tier == null)
            return AdminResult.NotFound("Subscription tier not found");

        var existingLimit = tier.Limits.FirstOrDefault(l => l.LimitType == limitType);
        
        if (existingLimit != null)
        {
            var oldValue = existingLimit.LimitValue;
            existingLimit.LimitValue = newLimit;
            
            await _auditService.LogSecurityEvent(adminUserId, "TierLimitChanged", 
                $"Tier: {tier.Name}, Limit: {limitType}, Old: {oldValue}, New: {newLimit}", "Admin");
        }
        else
        {
            // Create new limit
            var newLimitRecord = new SubscriptionTierLimit
            {
                SubscriptionTierId = tierId,
                LimitType = limitType,
                LimitValue = newLimit,
                Description = GetLimitDescription(limitType)
            };
            
            await _context.SubscriptionTierLimits.AddAsync(newLimitRecord);
            
            await _auditService.LogSecurityEvent(adminUserId, "TierLimitAdded", 
                $"Tier: {tier.Name}, Limit: {limitType}, Value: {newLimit}", "Admin");
        }

        tier.UpdatedAt = DateTime.UtcNow;
        tier.UpdatedById = adminUserId;

        await _context.SaveChangesAsync();

        // Check if any existing customers exceed new limits
        await ValidateExistingCustomersAgainstNewLimitsAsync(tierId, limitType, newLimit);

        return AdminResult.Success();
    }

    public async Task<AdminResult> CreateNewTierAsync(CreateTierRequest request, string adminUserId)
    {
        // Check if tier name already exists
        var existingTier = await _context.SubscriptionTiers
            .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower());
            
        if (existingTier != null)
            return AdminResult.Error("A tier with this name already exists");

        var newTier = new SubscriptionTier
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            MonthlyPrice = request.MonthlyPrice,
            IsActive = true,
            SortOrder = request.SortOrder,
            UpdatedById = adminUserId
        };

        await _context.SubscriptionTiers.AddAsync(newTier);
        await _context.SaveChangesAsync();

        // Add default limits
        foreach (var limit in request.Limits)
        {
            var tierLimit = new SubscriptionTierLimit
            {
                SubscriptionTierId = newTier.Id,
                LimitType = limit.LimitType,
                LimitValue = limit.LimitValue,
                Description = limit.Description
            };
            
            await _context.SubscriptionTierLimits.AddAsync(tierLimit);
        }

        // Add features
        foreach (var feature in request.Features)
        {
            var tierFeature = new SubscriptionTierFeature
            {
                SubscriptionTierId = newTier.Id,
                FeatureKey = feature.FeatureKey,
                IsEnabled = feature.IsEnabled,
                FeatureName = feature.FeatureName,
                Description = feature.Description
            };
            
            await _context.SubscriptionTierFeatures.AddAsync(tierFeature);
        }

        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEvent(adminUserId, "TierCreated", 
            $"Name: {newTier.Name}, Price: R{newTier.MonthlyPrice}", "Admin");

        return AdminResult.Success(newTier.Id);
    }

    private async Task ValidateExistingCustomersAgainstNewLimitsAsync(int tierId, string limitType, int newLimit)
    {
        if (limitType == "MaxBusinesses" && newLimit != -1)
        {
            var affectedUsers = await _context.Subscriptions
                .Where(s => s.SubscriptionTierId == tierId && s.IsActive)
                .Include(s => s.User)
                .ThenInclude(u => u.Businesses.Where(b => b.IsActive))
                .Where(s => s.User.Businesses.Count > newLimit)
                .ToListAsync();

            foreach (var subscription in affectedUsers)
            {
                // Send notification to user about limit change
                await _emailService.SendLimitChangeNotificationAsync(
                    subscription.User.Email, 
                    subscription.User.FirstName,
                    limitType, 
                    newLimit, 
                    subscription.User.Businesses.Count
                );
            }
        }
    }

    private string GetLimitDescription(string limitType)
    {
        return limitType switch
        {
            "MaxBusinesses" => "Maximum number of business listings",
            "MaxImages" => "Maximum number of images per business",
            "MaxPDFs" => "Maximum number of PDF documents per business",
            _ => $"Limit for {limitType}"
        };
    }
}

public class CreateTierRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int SortOrder { get; set; }
    public List<CreateTierLimitRequest> Limits { get; set; } = new();
    public List<CreateTierFeatureRequest> Features { get; set; } = new();
}

public class CreateTierLimitRequest
{
    public string LimitType { get; set; } = string.Empty;
    public int LimitValue { get; set; }
    public string? Description { get; set; }
}

public class CreateTierFeatureRequest
{
    public string FeatureKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? FeatureName { get; set; }
    public string? Description { get; set; }
}

public class AdminResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }

    public static AdminResult Success(object? data = null) => new() { IsSuccess = true, Data = data };
    public static AdminResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    public static AdminResult NotFound(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
```

### Admin Controller for Tier Management
```csharp
[Authorize(Roles = UserRoles.Admin)]
public class AdminSubscriptionController : Controller
{
    private readonly SubscriptionTierManagementService _tierService;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task<IActionResult> SubscriptionTiers()
    {
        var tiers = await _tierService.GetAllTiersAsync();
        return View(tiers);
    }

    public async Task<IActionResult> EditTier(int id)
    {
        var tier = await _tierService.GetTierByIdAsync(id);
        if (tier == null)
            return NotFound();

        return View(tier);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTierPrice(int tierId, decimal newPrice, string? reason)
    {
        var adminUserId = _userManager.GetUserId(User);
        var result = await _tierService.UpdateTierPricingAsync(tierId, newPrice, adminUserId, reason);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Tier pricing updated successfully";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(EditTier), new { id = tierId });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTierLimit(int tierId, string limitType, int limitValue)
    {
        var adminUserId = _userManager.GetUserId(User);
        var result = await _tierService.UpdateTierLimitAsync(tierId, limitType, limitValue, adminUserId);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Tier limit updated successfully";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(EditTier), new { id = tierId });
    }

    public IActionResult CreateTier()
    {
        return View(new CreateTierRequest());
    }

    [HttpPost]
    public async Task<IActionResult> CreateTier(CreateTierRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var adminUserId = _userManager.GetUserId(User);
        var result = await _tierService.CreateNewTierAsync(request, adminUserId);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "New subscription tier created successfully";
            return RedirectToAction(nameof(SubscriptionTiers));
        }
        else
        {
            ModelState.AddModelError("", result.ErrorMessage);
            return View(request);
        }
    }
}
```

---

## Implementation Phases

### Phase 1: Core Identity & External Authentication Setup
**Duration: 2-3 weeks**

1. **Extend Identity System**
   - Create `ApplicationUser` extending `IdentityUser`
   - Update `ApplicationDbContext` with new models
   - Create and run EF migrations
   - Implement role seeding

2. **External Authentication Integration**
   - Configure Google OAuth 2.0
   - Configure Facebook Login
   - Implement external login callbacks
   - Handle account linking/creation

3. **Security Foundation**
   - Configure HTTPS and security headers
   - Implement data protection services
   - Set up audit logging
   - Configure password policies

### Phase 2: Registration & Authentication Flows
**Duration: 2-3 weeks**

1. **Update Registration Forms**
   - Multi-method registration UI (Email/Google/Facebook)
   - Account type selection (Member/Client)
   - Business information collection
   - Terms and privacy consent

2. **Authentication Logic**
   - Role-based registration processing
   - Email verification workflows
   - External provider data handling
   - Account activation processes

3. **Authorization Policies**
   - Create custom authorization policies
   - Implement role-based access control
   - Update controller authorization
   - Test access restrictions

### Phase 3: PayFast Payment Integration
**Duration: 3-4 weeks**

1. **PayFast Integration**
   - Configure PayFast merchant settings
   - Implement payment request generation
   - Create secure signature validation
   - Set up ITN (notification) handling

2. **Subscription Management**
   - Subscription creation and activation
   - Recurring payment processing
   - Failed payment handling
   - Grace period management

3. **Security Implementation**
   - IP address validation
   - Signature verification
   - Secure data storage (no payment data)
   - Audit trail for all payment events

### Phase 4: Business Logic & Tier Management
**Duration: 2-3 weeks**

1. **Tier-Based Restrictions**
   - Business count validation
   - Image upload limits
   - Feature access control
   - Subscription tier enforcement

2. **Subscription Lifecycle**
   - Automatic renewal handling
   - Upgrade/downgrade processing
   - Business deactivation logic
   - Data retention policies

3. **Admin Management Tools**
   - Subscription monitoring
   - Payment status tracking
   - User role management
   - Business approval workflows

### Phase 5: Testing & Security Hardening
**Duration: 2-3 weeks**

1. **Comprehensive Testing**
   - Unit tests for authentication flows
   - Integration tests for payment processing
   - Security penetration testing
   - User acceptance testing

2. **Security Hardening**
   - Security audit and review
   - POPIA compliance verification
   - Performance optimization
   - Monitoring and alerting setup

3. **Documentation & Training**
   - User documentation
   - Admin training materials
   - Security incident response procedures
   - Maintenance and support procedures

---

## Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class AuthenticationServiceTests
{
    [TestMethod]
    public async Task CreateSubscription_ValidUser_ReturnsPaymentUrl()
    {
        // Test subscription creation logic
    }
    
    [TestMethod]
    public async Task ValidatePayFastNotification_ValidSignature_ReturnsTrue()
    {
        // Test PayFast signature validation
    }
    
    [TestMethod]
    public async Task HandleFailedPayment_GracePeriodExpired_DeactivatesBusinesses()
    {
        // Test business deactivation logic
    }
}
```

### Integration Tests
```csharp
[TestClass]
public class PaymentIntegrationTests
{
    [TestMethod]
    public async Task PayFastNotification_ValidPayment_ActivatesSubscription()
    {
        // Test end-to-end payment processing
    }
    
    [TestMethod]
    public async Task ExternalLogin_GoogleOAuth_CreatesUserAccount()
    {
        // Test Google OAuth integration
    }
}
```

### Security Tests
- **Penetration Testing**: External security audit
- **OWASP Top 10 Compliance**: Vulnerability scanning
- **Payment Security**: PayFast integration security review
- **Data Protection**: POPIA compliance verification

### User Acceptance Tests
- **Registration Journeys**: All authentication methods
- **Payment Flows**: Subscription creation and management
- **Role-Based Access**: Feature restrictions by tier
- **Business Management**: Creation, editing, deactivation

---

## Configuration & Environment Setup

### Development Environment
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret"
    }
  },
  "PayFast": {
    "MerchantId": "10000100",
    "MerchantKey": "46f0cd694581a",
    "Passphrase": "your-secure-passphrase",
    "UseSandbox": true,
    "ValidateIPAddress": false
  }
}
```

### Production Environment
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "production-google-client-id",
      "ClientSecret": "production-google-client-secret"
    },
    "Facebook": {
      "AppId": "production-facebook-app-id",
      "AppSecret": "production-facebook-app-secret"
    }
  },
  "PayFast": {
    "MerchantId": "your-live-merchant-id",
    "MerchantKey": "your-live-merchant-key",
    "Passphrase": "your-production-passphrase",
    "UseSandbox": false,
    "ValidateIPAddress": true
  }
}
```

---

## Risk Assessment & Mitigation

### High-Risk Areas
1. **Payment Processing**
   - **Risk**: Payment fraud, failed transactions
   - **Mitigation**: PayFast handles all payment data, signature validation, IP whitelisting

2. **External Authentication**
   - **Risk**: OAuth vulnerabilities, account takeover
   - **Mitigation**: State parameter validation, secure token handling, profile verification

3. **Data Protection**
   - **Risk**: Personal data breaches, POPIA violations
   - **Mitigation**: Data minimization, encryption at rest, audit logging, retention policies

4. **Subscription Management**
   - **Risk**: Unauthorized access, billing disputes
   - **Mitigation**: Role-based access control, comprehensive audit trails, clear terms of service

### Security Monitoring
```csharp
public class SecurityMonitoringService
{
    public async Task MonitorSuspiciousActivity()
    {
        // Monitor for:
        // - Multiple failed login attempts
        // - Unusual payment patterns
        // - Rapid account creation from same IP
        // - External authentication failures
        
        var suspiciousEvents = await _context.SecurityAuditLogs
            .Where(log => log.Timestamp > DateTime.UtcNow.AddHours(-1))
            .GroupBy(log => log.IpAddress)
            .Where(group => group.Count() > 10)
            .ToListAsync();
            
        foreach (var suspiciousGroup in suspiciousEvents)
        {
            await _alertService.SendSecurityAlertAsync(
                $"Suspicious activity from IP: {suspiciousGroup.Key}",
                $"Event count: {suspiciousGroup.Count()}"
            );
        }
    }
}
```

---

## Compliance & Legal Considerations

### POPIA (Protection of Personal Information Act) Compliance
- **Data Minimization**: Only collect necessary information
- **Consent Management**: Clear consent for data processing
- **Right to Access**: Users can request their data
- **Right to Deletion**: Users can request account deletion
- **Data Portability**: Export user data in standard format
- **Breach Notification**: 72-hour breach notification procedures

### Terms of Service & Privacy Policy
- Clear explanation of data collection and usage
- Payment terms and subscription policies
- External authentication provider disclosures
- Cookie and tracking policies
- Dispute resolution procedures

---

*This comprehensive authentication plan provides a secure, scalable foundation for TownTrek's multi-tier user system with external authentication and safe payment processing through PayFast integration.*

**Document Version**: 2.0  
**Last Updated**: February 2025  
**Security Review**: Required before production deployment  
**Next Review**: After Phase 3 implementation