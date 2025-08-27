using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TownTrek.Data;
using TownTrek.Services;
using TownTrek.Services.Interfaces;
using TownTrek.Models;
using TownTrek.Options;
using TownTrek.Middleware;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TownTrek.Services.Analytics;
using TownTrek.Services.ClientAnalytics;
using TownTrek.Services.AdminAnalytics;
using TownTrek.Services.SharedAnalytics;
using TownTrek.Services.ClientAnalytics.RealTime;

namespace TownTrek;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Serilog
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
        

        
        // Add Entity Framework
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add Identity services
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            
            // User settings
            options.User.RequireUniqueEmail = true;
            // Require confirmed email before sign-in
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Add session services for analytics tracking
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Configure application cookie settings
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8); // Default session: 8 hours
            options.SlidingExpiration = true; // Extends session on activity
            
            // Configure different expiration for "Remember Me"
            options.Events.OnSigningIn = context =>
            {
                if (context.Properties?.IsPersistent == true)
                {
                    // If "Remember Me" is checked, extend to 7 days
                    context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
                }
                return Task.CompletedTask;
            };
        });

        // Add rate limiting services
        builder.Services.AddRateLimiter(options =>
        {
            // Global rate limiting policy
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.IsAuthenticated == true 
                        ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 1000, // 1000 requests per window
                        Window = TimeSpan.FromMinutes(1) // 1 minute window
                    }));

            // Analytics-specific rate limiting
            options.AddPolicy("AnalyticsRateLimit", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.IsAuthenticated == true 
                        ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100, // 100 analytics requests per window
                        Window = TimeSpan.FromMinutes(1) // 1 minute window
                    }));

            // Chart data rate limiting (more restrictive)
            options.AddPolicy("ChartDataRateLimit", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.IsAuthenticated == true 
                        ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50, // 50 chart data requests per window
                        Window = TimeSpan.FromMinutes(1) // 1 minute window
                    }));

            // On rejection, return 429 Too Many Requests
            options.RejectionStatusCode = 429;
        });

        // Add authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ClientAccess", policy =>
                policy.RequireRole(
                    TownTrek.Constants.AppRoles.ClientBasic,
                    TownTrek.Constants.AppRoles.ClientStandard,
                    TownTrek.Constants.AppRoles.ClientPremium,
                    TownTrek.Constants.AppRoles.ClientTrial,
                    TownTrek.Constants.AppRoles.Admin));

            options.AddPolicy("PaidClientAccess", policy =>
                policy.RequireRole(
                    TownTrek.Constants.AppRoles.ClientBasic,
                    TownTrek.Constants.AppRoles.ClientStandard,
                    TownTrek.Constants.AppRoles.ClientPremium,
                    TownTrek.Constants.AppRoles.Admin));

            options.AddPolicy("PremiumOrAdmin", policy =>
                policy.RequireRole(
                    TownTrek.Constants.AppRoles.ClientPremium,
                    TownTrek.Constants.AppRoles.Admin));

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(TownTrek.Constants.AppRoles.Admin));
        });

        // Configure options
        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
        builder.Services.Configure<PayFastOptions>(builder.Configuration.GetSection("PayFast"));
        builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));

        // Add these service registrations
        builder.Services.AddScoped<ISubscriptionTierService, SubscriptionTierService>();
        builder.Services.AddScoped<ISubscriptionAuthService, SubscriptionAuthService>();
        builder.Services.AddScoped<IRegistrationService, RegistrationService>();
        builder.Services.AddScoped<ITrialService, SecureTrialService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();
        
        // Register image service before business services that depend on it
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<IBusinessService, Services.BusinessService>();
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IMemberService, MemberService>();
        // Add analytics architecture services
        builder.Services.AddScoped<IAnalyticsDataService, ClientAnalyticsDataService>();
        builder.Services.AddScoped<IAnalyticsValidationService, ClientAnalyticsValidationService>();
        builder.Services.AddScoped<IAnalyticsEventService, AnalyticsEventService>();
        builder.Services.AddScoped<IClientAnalyticsService, ClientAnalyticsService>();
        builder.Services.AddScoped<IClientAnalyticsService, ClientAnalyticsService>();
        builder.Services.AddScoped<IBusinessMetricsService, BusinessMetricsService>();
        builder.Services.AddScoped<IChartDataService, ChartDataService>();
        builder.Services.AddScoped<IComparativeAnalysisService, ComparativeAnalysisService>();
        builder.Services.AddScoped<IViewTrackingService, ViewTrackingService>();
        builder.Services.AddScoped<IAnalyticsSnapshotService, ClientAnalyticsSnapshotService>();
        builder.Services.AddHostedService<AnalyticsSnapshotBackgroundService>();
        builder.Services.AddHostedService<AdminAuditCleanupBackgroundService>();
        builder.Services.AddHostedService<AdminConnectionCleanupBackgroundService>();
        
        // Add cache services
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddScoped<IAnalyticsCacheService, ClientAnalyticsCacheService>();
        builder.Services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();
        builder.Services.AddScoped<IApplicationLogger, ApplicationLogger>();
        builder.Services.AddScoped<IDatabaseErrorLogger, DatabaseErrorLogger>();
        builder.Services.AddScoped<IAdminMessageService, AdminMessageService>();

        // Add analytics audit service
        builder.Services.AddScoped<IAnalyticsAuditService, AdminAnalyticsAuditService>();

        // Add analytics monitoring and observability services
        builder.Services.AddScoped<IAnalyticsPerformanceMonitor, AdminPerformanceMonitorService>();
        builder.Services.AddScoped<IAnalyticsErrorTracker, AdminErrorTrackerService>();
        builder.Services.AddScoped<IAnalyticsUsageTracker, AdminUsageTrackerService>();
        
        // Add analytics error handling service
        builder.Services.AddScoped<IAnalyticsErrorHandler, AnalyticsErrorHandler>();

        // Add analytics export and sharing services
        builder.Services.AddScoped<IAnalyticsExportService, ClientAnalyticsExportService>();

        // Add real-time analytics services
        builder.Services.AddScoped<IRealTimeAnalyticsService, ClientRealTimeAnalyticsService>();
        builder.Services.AddHostedService<ClientRealTimeBackgroundService>();

        // Add missing service registrations
        builder.Services.AddScoped<IAdvancedAnalyticsService, AdvancedAnalyticsService>();
        builder.Services.AddScoped<IDashboardCustomizationService, DashboardCustomizationService>();

        // Add HTTP context accessor for security services
        builder.Services.AddHttpContextAccessor();

        // Add health checks
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database")
            .AddCheck<AdminAnalyticsHealthCheckService>("analytics");

        // Add caching services
        builder.Services.AddMemoryCache();
        
        // Configure Redis or in-memory cache based on configuration
        var cacheOptions = builder.Configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>();
        if (cacheOptions?.UseRedis == true && !string.IsNullOrEmpty(cacheOptions.RedisConnectionString))
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = "TownTrek_";
            });
        }
        else
        {
            builder.Services.AddDistributedMemoryCache();
        }

        // Add SignalR
        builder.Services.AddSignalR();

        builder.Services.AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                // Allow view discovery to look under Views/Client/**/* and Views/Admin/**/*
                options.ViewLocationExpanders.Add(new TownTrek.Extensions.ClientViewLocationExpander());
                options.ViewLocationExpanders.Add(new TownTrek.Extensions.AdminViewLocationExpander());
            });

        var app = builder.Build();



        // Apply migrations and seed the database
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
            
            await DbSeeder.SeedAsync(scope.ServiceProvider);
            
            // Initialize roles
            var roleInitService = scope.ServiceProvider.GetRequiredService<IRoleInitializationService>();
            await roleInitService.InitializeRolesAsync();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // Add global exception handling middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();
        
        // Configure status code pages for common HTTP errors
        app.UseStatusCodePagesWithReExecute("/Error/{0}");

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Add session middleware for analytics tracking
        app.UseSession();

        // Add rate limiting middleware
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();
        
        // Add subscription redirect middleware
        app.UseMiddleware<TownTrek.Middleware.SubscriptionRedirectMiddleware>();
        
        // Add trial validation middleware
        app.UseMiddleware<TownTrek.Middleware.TrialValidationMiddleware>();
        
        // Add view tracking middleware
        app.UseViewTracking();

        // Map health checks
        app.MapHealthChecks("/health");

        // Map SignalR hub
        app.MapHub<TownTrek.Hubs.AnalyticsHub>("/analyticsHub");

        app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
