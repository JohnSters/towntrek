// Add this to your Program.cs or create a separate configuration file

using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Register subscription tier services
            services.AddScoped<ISubscriptionTierService, SubscriptionTierService>();
            services.AddScoped<ISubscriptionAuthService, SubscriptionAuthService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IEmailService, EmailService>();
            
            // Register business management services
            services.AddScoped<IBusinessService, BusinessService>();
            services.AddScoped<IClientService, ClientService>();
            
            // Register image management services
            services.AddScoped<IImageService, ImageService>();
            
            // Register other services
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRoleInitializationService, RoleInitializationService>();
            
            // Register error logging services
            services.AddScoped<IDatabaseErrorLogger, DatabaseErrorLogger>();
            
            // Register analytics services
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IViewTrackingService, ViewTrackingService>();
        }
    }
}