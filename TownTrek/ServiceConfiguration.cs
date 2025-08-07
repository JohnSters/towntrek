// Add this to your Program.cs or create a separate configuration file

using TownTrek.Services;

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
        }
    }
}