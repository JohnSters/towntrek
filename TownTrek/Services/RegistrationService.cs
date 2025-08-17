using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;
using TownTrek.Constants;
using TownTrek.Options;
using Microsoft.Extensions.Options;

namespace TownTrek.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITrialService _trialService;
        private readonly IEmailService _emailService;
        private readonly PayFastOptions _payFastOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegistrationService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<RegistrationService> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITrialService trialService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PayFastOptions> payFastOptions)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _trialService = trialService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _payFastOptions = payFastOptions.Value;
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
                await _userManager.AddToRoleAsync(user, AppRoles.Member);

                // Send email confirmation
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = Uri.EscapeDataString(token);
                    var baseUrl = _configuration["BaseUrl"] ?? "";
                    var confirmationUrl = string.IsNullOrWhiteSpace(baseUrl)
                        ? $"/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}"
                        : $"{baseUrl}/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}";
                    await _emailService.SendEmailConfirmationAsync(user.Email!, user.FirstName, confirmationUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", model.Email);
                }

                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", model.Email);
                }

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
                    // If the user exists but has no active subscription, try to generate a payment link to complete purchase
                    if (!existingUser.HasActiveSubscription)
                    {
                        var pendingSub = await _context.Subscriptions
                            .Include(s => s.SubscriptionTier)
                            .Where(s => s.UserId == existingUser.Id && (s.PaymentStatus == "Pending" || s.PaymentStatus == "Failed"))
                            .OrderByDescending(s => s.StartDate)
                            .FirstOrDefaultAsync();

                        // If user selected a specific plan, ensure the subscription matches or create a fresh pending one
                        SubscriptionTier? tierForPayment = null;
                        if (model.SelectedPlan.HasValue)
                        {
                            tierForPayment = await GetSubscriptionTierByIdAsync(model.SelectedPlan.Value);
                        }

                        if (pendingSub == null && tierForPayment != null)
                        {
                            var newSub = new Subscription
                            {
                                UserId = existingUser.Id,
                                SubscriptionTierId = tierForPayment.Id,
                                MonthlyPrice = tierForPayment.MonthlyPrice,
                                StartDate = DateTime.UtcNow,
                                IsActive = false,
                                PaymentStatus = "Pending"
                            };
                            await _context.Subscriptions.AddAsync(newSub);
                            await _context.SaveChangesAsync();
                            pendingSub = newSub;
                        }

                        var tier = pendingSub?.SubscriptionTier ?? tierForPayment;
                        if (tier != null)
                        {
                            var paymentUrlExisting = await GeneratePayFastPaymentDataAsync(tier, existingUser);
                            _logger.LogInformation("Existing user without active subscription; providing payment URL. {Email}", model.Email);
                            return RegistrationResult.SuccessWithPayment(existingUser, paymentUrlExisting);
                        }
                    }

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

                // Send email confirmation
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = Uri.EscapeDataString(token);
                    var baseUrl = _configuration["BaseUrl"] ?? "";
                    var confirmationUrl = string.IsNullOrWhiteSpace(baseUrl)
                        ? $"/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}"
                        : $"{baseUrl}/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}";
                    await _emailService.SendEmailConfirmationAsync(user.Email!, user.FirstName, confirmationUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", model.Email);
                }

                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", model.Email);
                }

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
            var merchantId = _payFastOptions.MerchantId;
            var merchantKey = _payFastOptions.MerchantKey;
            var passPhrase = _payFastOptions.PassPhrase ?? "";
            var baseUrl = _configuration["BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                // Derive from current request if not configured
                var req = _httpContextAccessor.HttpContext?.Request;
                if (req != null)
                {
                    baseUrl = $"{req.Scheme}://{req.Host}";
                }
            }
            var payFastUrl = string.IsNullOrWhiteSpace(_payFastOptions.PaymentUrl)
                ? "https://sandbox.payfast.co.za/eng/process"
                : _payFastOptions.PaymentUrl;

            var testAmount = Math.Max(tier.MonthlyPrice, 5.00m);

            // Start with the WORKING minimal version and add fields carefully
            var paymentData = new Dictionary<string, string>
            {
                ["merchant_id"] = merchantId,
                ["merchant_key"] = merchantKey,
                ["amount"] = testAmount.ToString("0.00", CultureInfo.InvariantCulture),
                ["item_name"] = "TownTrek-Subscription", // No spaces, no special chars
                ["m_payment_id"] = paymentId,
                // Add essential URLs for redirect and IPN
                ["return_url"] = $"{baseUrl}/Api/Payment/Success?paymentId={paymentId}",
                ["cancel_url"] = $"{baseUrl}/Api/Payment/Cancel",
                ["notify_url"] = $"{baseUrl}/Api/Payment/Notify",
                // Recipient email for PayFast records
                ["email_address"] = user.Email ?? "test@test.com"
            };

            _logger.LogInformation("Generating PayFast payment for user {UserId}, tier {TierName}, amount R{Amount}", 
                user.Id, tier.Name, testAmount);
            
            // Log payment data (excluding sensitive information)
            _logger.LogInformation("PayFast payment data - Amount: R{Amount}, Item: {ItemName}, PaymentId: {PaymentId}", 
                paymentData["amount"], paymentData["item_name"], paymentId);

            // Generate signature using the simple method
            var signature = GeneratePayFastSignatureSimple(paymentData, passPhrase);
            paymentData["signature"] = signature;

            // Build PayFast URL: URL-encode values for the request itself
            string EncodeValue(string value)
            {
                return Uri.EscapeDataString(value).Replace("%20", "+");
            }

            var orderedForQuery = paymentData
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .OrderBy(k => k.Key, StringComparer.Ordinal);
            var queryString = string.Join("&", orderedForQuery.Select(kvp => $"{kvp.Key}={EncodeValue(kvp.Value)}"));
            var paymentUrl = $"{payFastUrl}?{queryString}";

            _logger.LogInformation("PayFast payment URL generated for user {UserId}", user.Id);

            return paymentUrl;
        }

        public async Task<Dictionary<string, string>> BuildPayFastFormFieldsAsync(SubscriptionTier tier, ApplicationUser user, int paymentNumericId)
        {
            await Task.CompletedTask;

            var merchantId = _payFastOptions.MerchantId;
            var merchantKey = _payFastOptions.MerchantKey;
            var passPhrase = _payFastOptions.PassPhrase ?? "";
            var baseUrl = _configuration["BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var req = _httpContextAccessor.HttpContext?.Request;
                if (req != null)
                {
                    baseUrl = $"{req.Scheme}://{req.Host}";
                }
            }
            var payFastUrl = string.IsNullOrWhiteSpace(_payFastOptions.PaymentUrl)
                ? "https://sandbox.payfast.co.za/eng/process"
                : _payFastOptions.PaymentUrl;

            var amount = Math.Max(tier.MonthlyPrice, 5.00m)
                .ToString("0.00", CultureInfo.InvariantCulture);
            var paymentId = paymentNumericId.ToString();

            var fields = new Dictionary<string, string>
            {
                ["merchant_id"] = merchantId,
                ["merchant_key"] = merchantKey,
                ["amount"] = amount,
                ["item_name"] = "TownTrek-Subscription",
                ["m_payment_id"] = paymentId,
                ["return_url"] = $"{baseUrl}/Api/Payment/Success?paymentId={paymentId}",
                ["cancel_url"] = $"{baseUrl}/Api/Payment/Cancel",
                ["notify_url"] = $"{baseUrl}/Api/Payment/Notify",
                ["email_address"] = user.Email ?? "test@test.com"
            };

            var signature = GeneratePayFastSignatureSimple(fields, passPhrase);
            fields["signature"] = signature;
            return fields;
        }

        public async Task<Dictionary<string, string>?> BuildPayFastFormFieldsForUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var sub = await _context.Subscriptions
                .Include(s => s.SubscriptionTier)
                .Where(s => s.UserId == userId && s.PaymentStatus == "Pending")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (sub?.SubscriptionTier == null) return null;
            return await BuildPayFastFormFieldsAsync(sub.SubscriptionTier, user, sub.Id);
        }

        private string GeneratePayFastSignatureSimple(Dictionary<string, string> data, string? passPhrase = null)
        {
            // PayFast signature rules
            // - Exclude 'signature' key and empty values
            // - Sort keys alphabetically (Ordinal)
            // - Build key=value pairs joined by '&'
            // - Some accounts require RAW values; some require URL-encoded values. Toggle via PayFastOptions.UseEncodedSignature.
            // - Append '&passphrase=...' LAST (RAW, not encoded) if configured
            // - MD5 and lowercase

            var ordered = data
                .Where(kvp => kvp.Key != "signature" && !string.IsNullOrEmpty(kvp.Value))
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal);

            var sb = new System.Text.StringBuilder();
            foreach (var kvp in ordered)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(kvp.Key);
                sb.Append('=');
                if (_payFastOptions.UseEncodedSignature)
                {
                    // encodeURIComponent and replace %20 with +
                    var encoded = Uri.EscapeDataString(kvp.Value).Replace("%20", "+");
                    sb.Append(encoded);
                }
                else
                {
                    sb.Append(kvp.Value);
                }
            }

            if (!string.IsNullOrEmpty(passPhrase))
            {
                // IMPORTANT: PayFast expects the passphrase appended LAST and NOT URL-encoded
                sb.Append("&passphrase=");
                sb.Append(passPhrase.Trim());
            }

            var raw = sb.ToString();
            var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
            var signature = Convert.ToHexString(hash).ToLower();
            return signature;
        }

        public async Task<RegistrationResult> RegisterTrialUserAsync(RegisterViewModel model)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return RegistrationResult.Error("A user with this email address already exists.");
                }

                // Create new trial user
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
                    HasActiveSubscription = false, // Trial users don't have paid subscriptions
                    IsTrialUser = true // Mark as trial user
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RegistrationResult.Error($"Failed to create user: {errors}");
                }

                // Start trial period - this will add the Client-Trial role and set trial dates
                var trialUser = await _trialService.StartTrialAsync(user);
                if (trialUser == null)
                {
                    await _userManager.DeleteAsync(user);
                    return RegistrationResult.Error("Failed to start trial period. Please try again.");
                }

                // Send email confirmation
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(trialUser);
                    var encodedToken = Uri.EscapeDataString(token);
                    var baseUrl = _configuration["BaseUrl"] ?? "";
                    var confirmationUrl = string.IsNullOrWhiteSpace(baseUrl)
                        ? $"/Auth/ConfirmEmail?userId={trialUser.Id}&token={encodedToken}"
                        : $"{baseUrl}/Auth/ConfirmEmail?userId={trialUser.Id}&token={encodedToken}";
                    await _emailService.SendEmailConfirmationAsync(trialUser.Email!, trialUser.FirstName, confirmationUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", model.Email);
                }

                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", model.Email);
                }

                _logger.LogInformation("Trial user created successfully: {Email}, Trial expires: {ExpiryDate}", 
                    model.Email, trialUser.TrialEndDate);
                return RegistrationResult.Success(trialUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trial user: {Email}", model.Email);
                return RegistrationResult.Error("An error occurred during registration. Please try again.");
            }
        }
    }
}
