using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services
{
    public interface IRegistrationService
    {
        Task<List<SubscriptionTier>> GetAvailableSubscriptionTiersAsync();
        Task<SubscriptionTier?> GetSubscriptionTierByIdAsync(int tierId);
        Task<RegistrationResult> RegisterMemberAsync(RegisterViewModel model);
        Task<RegistrationResult> RegisterBusinessOwnerAsync(RegisterViewModel model);
        Task<string> GeneratePayFastPaymentDataAsync(SubscriptionTier tier, ApplicationUser user);
    }

    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public ApplicationUser? User { get; set; }
        public string? PaymentUrl { get; set; }
        public bool RequiresPayment { get; set; }

        public static RegistrationResult Success(ApplicationUser user) => new() { IsSuccess = true, User = user };
        public static RegistrationResult SuccessWithPayment(ApplicationUser user, string paymentUrl) => new() 
        { 
            IsSuccess = true, 
            User = user, 
            PaymentUrl = paymentUrl, 
            RequiresPayment = true 
        };
        public static RegistrationResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegistrationService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<RegistrationService> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<SubscriptionTier>> GetAvailableSubscriptionTiersAsync()
        {
            return await _context.SubscriptionTiers
                .Where(t => t.IsActive)
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<SubscriptionTier?> GetSubscriptionTierByIdAsync(int tierId)
        {
            return await _context.SubscriptionTiers
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .FirstOrDefaultAsync(t => t.Id == tierId && t.IsActive);
        }

        public async Task<RegistrationResult> RegisterMemberAsync(RegisterViewModel model)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return RegistrationResult.Error("A user with this email address already exists.");
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.Phone,
                    Location = model.Location,
                    AuthenticationMethod = "Email",
                    IsActive = true,
                    HasActiveSubscription = false // Members don't need subscriptions
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RegistrationResult.Error($"Failed to create user: {errors}");
                }

                // Add to Member role
                await _userManager.AddToRoleAsync(user, "Member");

                _logger.LogInformation("Member user created successfully: {Email}", model.Email);
                return RegistrationResult.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member user: {Email}", model.Email);
                return RegistrationResult.Error("An error occurred during registration. Please try again.");
            }
        }

        public async Task<RegistrationResult> RegisterBusinessOwnerAsync(RegisterViewModel model)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return RegistrationResult.Error("A user with this email address already exists.");
                }

                // Validate selected subscription tier
                if (!model.SelectedPlan.HasValue)
                {
                    return RegistrationResult.Error("Please select a subscription plan.");
                }

                var subscriptionTier = await GetSubscriptionTierByIdAsync(model.SelectedPlan.Value);
                if (subscriptionTier == null)
                {
                    return RegistrationResult.Error("Invalid subscription plan selected.");
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.Phone,
                    Location = model.Location,
                    AuthenticationMethod = "Email",
                    IsActive = true,
                    HasActiveSubscription = false, // Will be activated after payment
                    CurrentSubscriptionTier = subscriptionTier.Name
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RegistrationResult.Error($"Failed to create user: {errors}");
                }

                // Create pending subscription
                var subscription = new Subscription
                {
                    UserId = user.Id,
                    SubscriptionTierId = subscriptionTier.Id,
                    MonthlyPrice = subscriptionTier.MonthlyPrice,
                    StartDate = DateTime.UtcNow,
                    IsActive = false,
                    PaymentStatus = "Pending"
                };

                await _context.Subscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();

                // Generate PayFast payment URL
                var paymentData = await GeneratePayFastPaymentDataAsync(subscriptionTier, user);

                _logger.LogInformation("Business owner user created successfully: {Email}, Tier: {Tier}", 
                    model.Email, subscriptionTier.Name);

                return RegistrationResult.SuccessWithPayment(user, paymentData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating business owner user: {Email}", model.Email);
                return RegistrationResult.Error("An error occurred during registration. Please try again.");
            }
        }

        public async Task<string> GeneratePayFastPaymentDataAsync(SubscriptionTier tier, ApplicationUser user)
        {
            await Task.CompletedTask;

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.SubscriptionTierId == tier.Id && s.PaymentStatus == "Pending");

            var paymentId = subscription?.Id.ToString() ?? Guid.NewGuid().ToString();

            // PayFast configuration - get from appsettings or use sandbox defaults
            var merchantId = _configuration["PayFast:MerchantId"] ?? "10040964";
            var merchantKey = _configuration["PayFast:MerchantKey"] ?? "mieu9ydfgtqo4";
            var passPhrase = _configuration["PayFast:PassPhrase"] ?? "MyTestPassPhrase123";
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7154";
            var payFastUrl = _configuration["PayFast:PaymentUrl"] ?? "https://sandbox.payfast.co.za/eng/process";

            var testAmount = Math.Max(tier.MonthlyPrice, 5.00m);

            // Start with the WORKING minimal version and add fields carefully
            var paymentData = new Dictionary<string, string>
            {
                ["merchant_id"] = merchantId,
                ["merchant_key"] = merchantKey,
                ["amount"] = testAmount.ToString("0.00", CultureInfo.InvariantCulture),
                ["item_name"] = "TownTrek-Subscription", // No spaces, no special chars
                ["m_payment_id"] = paymentId,
                
                // Add ONLY essential fields with simple values
                ["email_address"] = "test@test.com" // Simple email that worked before
            };

            _logger.LogInformation("Generating PayFast payment for user {UserId}, tier {TierName}, amount R{Amount}", 
                user.Id, tier.Name, testAmount);
            
            // Log payment data (excluding sensitive information)
            _logger.LogInformation("PayFast payment data - Amount: R{Amount}, Item: {ItemName}, PaymentId: {PaymentId}", 
                paymentData["amount"], paymentData["item_name"], paymentId);

            // Generate signature using the simple method
            var signature = GeneratePayFastSignatureSimple(paymentData, passPhrase);
            paymentData["signature"] = signature;

            // Build PayFast URL
            var queryString = string.Join("&", paymentData.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var paymentUrl = $"{payFastUrl}?{queryString}";

            _logger.LogInformation("PayFast payment URL generated for user {UserId}", user.Id);

            return paymentUrl;
        }

        private string GeneratePayFastSignatureSimple(Dictionary<string, string> data, string? passPhrase = null)
        {
            // Exactly match the Node.js implementation
            // Sort all keys alphabetically
            var orderedData = data
                .Where(kvp => kvp.Key != "signature" && !string.IsNullOrEmpty(kvp.Value))
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Create the get string exactly like Node.js
            var getString = "";
            foreach (var kvp in orderedData)
            {
                // encodeURIComponent equivalent, then replace %20 with +
                var encodedValue = Uri.EscapeDataString(kvp.Value).Replace("%20", "+");
                getString += $"{kvp.Key}={encodedValue}&";
            }

            // Remove the last '&'
            getString = getString.TrimEnd('&');

            // Add passphrase if provided
            if (!string.IsNullOrEmpty(passPhrase))
            {
                var encodedPassPhrase = Uri.EscapeDataString(passPhrase.Trim()).Replace("%20", "+");
                getString += $"&passphrase={encodedPassPhrase}";
            }

            // Generate MD5 hash
            var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(getString));
            var signature = Convert.ToHexString(hash).ToLower();

            _logger.LogInformation("PayFast signature generated successfully");

            return signature;
        }
    }
}