using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult> CreateBusinessAlertAsync(int businessId, string alertType, string title, string message, DateTime? expiresAt = null, bool isPushNotification = false)
        {
            try
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    return ServiceResult.Error("Business not found");
                }

                var alert = new BusinessAlert
                {
                    BusinessId = businessId,
                    AlertType = alertType,
                    Title = title,
                    Message = message,
                    ExpiresAt = expiresAt,
                    IsPushNotification = isPushNotification,
                    Priority = DeterminePriority(alertType),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.BusinessAlerts.AddAsync(alert);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Business alert created for business {BusinessId}: {Title}", businessId, title);

                // Here you would integrate with push notification service (Firebase, Azure Notification Hubs, etc.)
                if (isPushNotification)
                {
                    await SendPushNotificationAsync(alert);
                }

                return ServiceResult.Success(alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating business alert for business {BusinessId}", businessId);
                return ServiceResult.Error("Failed to create alert");
            }
        }

        public async Task<List<BusinessAlert>> GetActiveAlertsForBusinessAsync(int businessId)
        {
            return await _context.BusinessAlerts
                .Where(a => a.BusinessId == businessId && 
                           a.IsActive && 
                           (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BusinessAlert>> GetActiveAlertsForTownAsync(int townId)
        {
            return await _context.BusinessAlerts
                .Include(a => a.Business)
                .Where(a => a.Business.TownId == townId && 
                           a.IsActive && 
                           (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<ServiceResult> SendMarketDayReminderAsync(int marketBusinessId)
        {
            var marketBusiness = await _context.Businesses
                .Include(b => b.Town)
                .FirstOrDefaultAsync(b => b.Id == marketBusinessId && b.Category == "markets-vendors");

            if (marketBusiness == null)
            {
                return ServiceResult.Error("Market business not found");
            }

            var marketDetails = await _context.MarketDetails
                .FirstOrDefaultAsync(md => md.BusinessId == marketBusinessId);

            if (marketDetails == null)
            {
                return ServiceResult.Error("Market details not found");
            }

            var title = $"{marketBusiness.Name} - Market Day Tomorrow!";
            var message = $"Don't forget! {marketBusiness.Name} market is tomorrow from {marketDetails.MarketStartTime} to {marketDetails.MarketEndTime}. {marketDetails.EstimatedVendorCount} vendors expected.";

            return await CreateBusinessAlertAsync(marketBusinessId, "MarketReminder", title, message, DateTime.UtcNow.AddDays(2), true);
        }

        public async Task<ServiceResult> SendEventUpdateAsync(int eventBusinessId, string updateType, string message)
        {
            var eventBusiness = await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == eventBusinessId && b.Category == "events");

            if (eventBusiness == null)
            {
                return ServiceResult.Error("Event business not found");
            }

            var title = updateType switch
            {
                "Cancelled" => $"Event Cancelled: {eventBusiness.Name}",
                "Postponed" => $"Event Postponed: {eventBusiness.Name}",
                "TimeChange" => $"Time Change: {eventBusiness.Name}",
                "VenueChange" => $"Venue Change: {eventBusiness.Name}",
                _ => $"Event Update: {eventBusiness.Name}"
            };

            var priority = updateType == "Cancelled" ? "High" : "Normal";

            var alert = new BusinessAlert
            {
                BusinessId = eventBusinessId,
                AlertType = "EventUpdate",
                Title = title,
                Message = message,
                IsPushNotification = true,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.BusinessAlerts.AddAsync(alert);
            await _context.SaveChangesAsync();

            await SendPushNotificationAsync(alert);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> SendWeatherAlertAsync(int businessId, string weatherCondition)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null)
            {
                return ServiceResult.Error("Business not found");
            }

            // Check if business is weather-dependent (tours, outdoor events, markets)
            var isWeatherDependent = business.Category switch
            {
                "tours-experiences" => true,
                "events" => await _context.EventDetails.AnyAsync(ed => ed.BusinessId == businessId && ed.IsOutdoorEvent),
                "markets-vendors" => true,
                _ => false
            };

            if (!isWeatherDependent)
            {
                return ServiceResult.Success(); // No need to send weather alerts
            }

            var title = $"Weather Alert: {business.Name}";
            var message = weatherCondition switch
            {
                "Rain" => "Due to expected rain, please check with us before visiting. We may need to reschedule or move indoors.",
                "Storm" => "Due to severe weather conditions, today's activities may be cancelled. Please contact us for updates.",
                "Heat" => "Extreme heat expected today. Please bring sun protection and stay hydrated.",
                _ => $"Weather conditions may affect today's schedule: {weatherCondition}"
            };

            return await CreateBusinessAlertAsync(businessId, "WeatherAlert", title, message, DateTime.UtcNow.AddHours(24), true);
        }

        private string DeterminePriority(string alertType)
        {
            return alertType switch
            {
                "Emergency" => "Urgent",
                "EventUpdate" => "High",
                "WeatherAlert" => "High",
                "StatusChange" => "Normal",
                "SpecialOffer" => "Low",
                _ => "Normal"
            };
        }

        private async Task SendPushNotificationAsync(BusinessAlert alert)
        {
            // This would integrate with your push notification service
            // For example: Firebase Cloud Messaging, Azure Notification Hubs, etc.
            
            _logger.LogInformation("Push notification would be sent: {Title} - {Message}", alert.Title, alert.Message);
            
            // Placeholder for actual push notification implementation
            await Task.CompletedTask;
        }
    }
}
