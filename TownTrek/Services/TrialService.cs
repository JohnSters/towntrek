using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class TrialService : ITrialService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TrialService> _logger;
        private const int TRIAL_DAYS = 30;

        public TrialService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TrialService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> IsTrialValidAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsTrialUser) return false;

            if (user.TrialEndDate.HasValue && user.TrialEndDate.Value < DateTime.UtcNow)
            {
                await ExpireTrialAsync(userId);
                return false;
            }

            return true;
        }

        public async Task<int> GetTrialDaysRemainingAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsTrialUser || !user.TrialEndDate.HasValue)
                return 0;

            var daysRemaining = (user.TrialEndDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, daysRemaining);
        }

        public async Task<bool> ExpireTrialAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.TrialExpired = true;
                user.IsActive = false; // Disable account access

                // Remove from Client-Trial role
                await _userManager.RemoveFromRoleAsync(user, AppRoles.ClientTrial);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Trial expired for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring trial for user {UserId}", userId);
                return false;
            }
        }

        public async Task<ApplicationUser?> StartTrialAsync(ApplicationUser user)
        {
            try
            {
                user.IsTrialUser = true;
                user.TrialStartDate = DateTime.UtcNow;
                user.TrialEndDate = DateTime.UtcNow.AddDays(TRIAL_DAYS);
                user.TrialExpired = false;
                user.CurrentSubscriptionTier = "Trial";

                // Add to Client-Trial role
                await _userManager.AddToRoleAsync(user, AppRoles.ClientTrial);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Trial started for user {UserId}, expires on {ExpiryDate}", 
                    user.Id, user.TrialEndDate);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trial for user {UserId}", user.Id);
                return null;
            }
        }

        public async Task<bool> ConvertTrialToSubscriptionAsync(string userId, int subscriptionTierId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var subscriptionTier = await _context.SubscriptionTiers.FindAsync(subscriptionTierId);
                if (subscriptionTier == null) return false;

                // End trial period
                user.IsTrialUser = false;
                user.TrialExpired = false;
                user.HasActiveSubscription = true;
                user.CurrentSubscriptionTier = subscriptionTier.Name;
                user.SubscriptionStartDate = DateTime.UtcNow;
                user.IsActive = true;

                // Remove from trial role
                await _userManager.RemoveFromRoleAsync(user, AppRoles.ClientTrial);

                // Add to appropriate subscription role
                var newRole = subscriptionTier.Name.ToLower() switch
                {
                    "basic" => AppRoles.ClientBasic,
                    "standard" => AppRoles.ClientStandard,
                    "premium" => AppRoles.ClientPremium,
                    _ => AppRoles.ClientBasic
                };

                await _userManager.AddToRoleAsync(user, newRole);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Trial converted to subscription for user {UserId}, tier {TierName}", 
                    userId, subscriptionTier.Name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting trial to subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<TrialStatus> GetTrialStatusAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsTrialUser)
            {
                return new TrialStatus
                {
                    IsTrialUser = false,
                    IsExpired = false,
                    DaysRemaining = 0,
                    StatusMessage = "Not a trial user"
                };
            }

            var daysRemaining = await GetTrialDaysRemainingAsync(userId);
            var isExpired = daysRemaining <= 0;

            return new TrialStatus
            {
                IsTrialUser = true,
                IsExpired = isExpired,
                DaysRemaining = daysRemaining,
                TrialEndDate = user.TrialEndDate,
                StatusMessage = isExpired 
                    ? "Trial period has expired" 
                    : $"Trial expires in {daysRemaining} day{(daysRemaining != 1 ? "s" : "")}"
            };
        }
    }
}