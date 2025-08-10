using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TownTrek.Data;
using TownTrek.Services;
using TownTrek.Services.Interfaces;
using TownTrek.Models;

namespace TownTrek;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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

        // Add these service registrations
        builder.Services.AddScoped<ISubscriptionTierService, SubscriptionTierService>();
        builder.Services.AddScoped<ISubscriptionAuthService, SubscriptionAuthService>();
        builder.Services.AddScoped<IRegistrationService, RegistrationService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();
        
        // Register image service before business services that depend on it
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<IBusinessService, Services.BusinessService>();
        builder.Services.AddScoped<IClientService, ClientService>();

        builder.Services.AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                // Allow view discovery to look under Views/Client/**/* as well
                options.ViewLocationExpanders.Add(new TownTrek.Extensions.ClientViewLocationExpander());
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
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
        // Add subscription redirect middleware
        app.UseMiddleware<TownTrek.Middleware.SubscriptionRedirectMiddleware>();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
