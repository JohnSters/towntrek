using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TownTrek.Data;
using TownTrek.Services;
using TownTrek.Services.Interfaces;
using TownTrek.Models;
using TownTrek.Options;
using TownTrek.Middleware;
using Serilog;

namespace TownTrek;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Serilog
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
        
        builder.AddServiceDefaults();
        
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
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        builder.Services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();
        builder.Services.AddScoped<IApplicationLogger, ApplicationLogger>();

        // Add HTTP context accessor for security services
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                // Allow view discovery to look under Views/Client/**/* and Views/Admin/**/*
                options.ViewLocationExpanders.Add(new TownTrek.Extensions.ClientViewLocationExpander());
                options.ViewLocationExpanders.Add(new TownTrek.Extensions.AdminViewLocationExpander());
            });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Seed the database and initialize roles
        using (var scope = app.Services.CreateScope())
        {
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

        app.UseAuthentication();
        app.UseAuthorization();
        
        // Add subscription redirect middleware
        app.UseMiddleware<TownTrek.Middleware.SubscriptionRedirectMiddleware>();
        
        // Add trial validation middleware
        app.UseMiddleware<TownTrek.Middleware.TrialValidationMiddleware>();

        app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
