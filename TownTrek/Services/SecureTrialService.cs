using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class SecureTrialService : ITrialService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SecureTrialService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int TRIAL_DAYS = 30;
        private const int MAX_DAILY_CHECKS = 100; // Prevent abuse
        private const string TRIAL_SECRET_KEY = "TownTrek_Trial_Security_2024"; // Should be in config

        public SecureTrialService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SecureTrialService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> IsTrialValidAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsTrialUser) return false;

            // Security: Rate limiting - prevent excessive checks
            if (await IsRateLimitedAsync(user))
            {
                _logger.LogWarning("Rate limit exceeded for trial validation: {UserId}", userId);
                return false;
            }

            // Security: Validate trial integrity
            if (!ValidateTrialIntegrity(user))
            {
                _logger.LogError("Trial integrity check failed for user: {UserId}", userId);
                await ExpireTrialAsync(userId);
                return false;
            }

            // Use multiple time sources for validation
            var currentTime = await GetSecureCurrentTimeAsync();
            var trialEndTime = new DateTime(user.TrialEndTicks, DateTimeKind.Utc);

            // Update check tracking
            await UpdateTrialCheckAsync(user, currentTime);

            if (currentTime >= trialEndTime)
            {
                _logger.LogInformation("Trial expired for user {UserId} at {CurrentTime}, trial ended {TrialEnd}", 
                    userId, currentTime, trialEndTime);
                await ExpireTrialAsync(userId);
                return false;
            }

            return true;
        }

        public async Task<int> GetTrialDaysRemainingAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsTrialUser || user.TrialEndTicks == 0)
                return 0;

            if (!ValidateTrialIntegrity(user))
            {
                _logger.LogError("Trial integrity check failed during days remaining calculation: {UserId}", userId);
                return 0;
            }

            var currentTime = await GetSecureCurrentTimeAsync();
            var trialEndTime = new DateTime(user.TrialEndTicks, DateTimeKind.Utc);
            
            var daysRemaining = (trialEndTime - currentTime).Days;
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

                // Security: Log the expiration with details
                _logger.LogInformation("Trial expired for user {UserId}. Start: {Start}, End: {End}, Checks: {Checks}", 
                    userId, 
                    user.TrialStartTicks > 0 ? new DateTime(user.TrialStartTicks, DateTimeKind.Utc) : null,
                    user.TrialEndTicks > 0 ? new DateTime(user.TrialEndTicks, DateTimeKind.Utc) : null,
                    user.TrialCheckCount);

                // Remove from Client-Trial role
                await _userManager.RemoveFromRoleAsync(user, AppRoles.ClientTrial);

                await _context.SaveChangesAsync();
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
                var currentTime = await GetSecureCurrentTimeAsync();
                var trialEndTime = currentTime.AddDays(TRIAL_DAYS);

                user.IsTrialUser = true;
                user.TrialStartDate = currentTime;
                user.TrialEndDate = trialEndTime;
                user.TrialStartTicks = currentTime.Ticks;
                user.TrialEndTicks = trialEndTime.Ticks;
                user.TrialExpired = false;
                user.CurrentSubscriptionTier = "Trial";
                user.TrialCheckCount = 0;
                user.LastTrialCheck = currentTime;

                // Security: Generate integrity hash
                user.TrialSecurityHash = GenerateTrialSecurityHash(user.Id, user.TrialStartTicks, user.TrialEndTicks);

                // Add to Client-Trial role
                await _userManager.AddToRoleAsync(user, AppRoles.ClientTrial);

                // Create audit log
                await CreateAuditLogAsync(user.Id, "Started", $"Trial started, expires: {trialEndTime:yyyy-MM-dd HH:mm:ss} UTC", 
                    user.TrialStartTicks, user.TrialEndTicks, TRIAL_DAYS);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Secure trial started for user {UserId}, expires on {ExpiryDate} (Ticks: {EndTicks})", 
                    user.Id, trialEndTime, user.TrialEndTicks);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting secure trial for user {UserId}", user.Id);
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

                // Security: Log the conversion with trial details
                _logger.LogInformation("Converting trial to subscription for user {UserId}. Trial used {Days} days, {Checks} checks", 
                    userId, 
                    user.TrialStartTicks > 0 ? (DateTime.UtcNow - new DateTime(user.TrialStartTicks, DateTimeKind.Utc)).Days : 0,
                    user.TrialCheckCount);

                // End trial period
                user.IsTrialUser = false;
                user.TrialExpired = false;
                user.HasActiveSubscription = true;
                user.CurrentSubscriptionTier = subscriptionTier.Name;
                user.SubscriptionStartDate = DateTime.UtcNow;
                user.IsActive = true;

                // Clear trial security data
                user.TrialSecurityHash = null;
                user.TrialCheckCount = 0;

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

            // Security: Validate integrity before returning status
            if (!ValidateTrialIntegrity(user))
            {
                _logger.LogError("Trial integrity check failed during status check: {UserId}", userId);
                return new TrialStatus
                {
                    IsTrialUser = true,
                    IsExpired = true,
                    DaysRemaining = 0,
                    StatusMessage = "Trial period has been compromised and expired"
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

        // Security Methods

        private Task<DateTime> GetSecureCurrentTimeAsync()
        {
            // Primary: Use server UTC time
            var serverTime = DateTime.UtcNow;

            // TODO: In production, consider adding external time validation
            // For example, call a time API service occasionally to validate server time
            // This would help detect if server time has been manipulated

            return Task.FromResult(serverTime);
        }

        private bool ValidateTrialIntegrity(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.TrialSecurityHash) || 
                user.TrialStartTicks == 0 || 
                user.TrialEndTicks == 0)
            {
                return false;
            }

            var expectedHash = GenerateTrialSecurityHash(user.Id, user.TrialStartTicks, user.TrialEndTicks);
            return user.TrialSecurityHash == expectedHash;
        }

        private string GenerateTrialSecurityHash(string userId, long startTicks, long endTicks)
        {
            var secretKey = _configuration["TrialSecretKey"] ?? TRIAL_SECRET_KEY;
            var data = $"{userId}:{startTicks}:{endTicks}:{secretKey}";
            
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }

        private async Task<bool> IsRateLimitedAsync(ApplicationUser user)
        {
            var today = DateTime.UtcNow.Date;
            var lastCheckDate = user.LastTrialCheck?.Date;

            // Reset daily counter if it's a new day
            if (lastCheckDate != today)
            {
                user.TrialCheckCount = 0;
                user.LastTrialCheck = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return false;
            }

            // Check if user has exceeded daily limit
            return user.TrialCheckCount >= MAX_DAILY_CHECKS;
        }

        private async Task UpdateTrialCheckAsync(ApplicationUser user, DateTime currentTime)
        {
            user.TrialCheckCount++;
            user.LastTrialCheck = currentTime;
            await _context.SaveChangesAsync();
        }

        private async Task CreateAuditLogAsync(string userId, string action, string details, 
            long? startTicks = null, long? endTicks = null, int? daysRemaining = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var auditLog = new TrialAuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                TrialStartTicks = startTicks,
                TrialEndTicks = endTicks,
                DaysRemaining = daysRemaining
            };

            _context.TrialAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}