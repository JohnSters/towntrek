using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Analytics
{
    [Authorize]
    [Route("Client/TestAnalytics/[action]")]
    public class TestAnalyticsController(
        ISubscriptionAuthService subscriptionAuthService,
        ILogger<TestAnalyticsController> logger) : Controller
    {
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly ILogger<TestAnalyticsController> _logger = logger;

        // Simple test action to check subscription status
        public async Task<IActionResult> Status()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
                var tier = await _subscriptionAuthService.GetUserSubscriptionTierAsync(userId);
                var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
                var canAccessBasic = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                var canAccessAdvanced = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics");

                var debugInfo = new
                {
                    UserId = userId,
                    UserEmail = User.Identity?.Name,
                    IsAuthenticated = authResult.IsAuthenticated,
                    HasActiveSubscription = authResult.HasActiveSubscription,
                    IsPaymentValid = authResult.IsPaymentValid,
                    PaymentStatus = authResult.PaymentStatus,
                    ErrorMessage = authResult.ErrorMessage,
                    TierName = tier?.Name,
                    TierDisplayName = tier?.DisplayName,
                    CanAccessBasicAnalytics = canAccessBasic,
                    CanAccessAdvancedAnalytics = canAccessAdvanced,
                    Limits = new
                    {
                        limits.HasBasicAnalytics,
                        limits.HasAdvancedAnalytics,
                        limits.MaxBusinesses,
                        limits.CurrentBusinessCount
                    }
                };

                return Json(debugInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subscription status");
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Simple analytics page without complex subscription checks
        public async Task<IActionResult> Simple()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Just check if user can access basic analytics
                var canAccessBasic = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics");
                
                ViewBag.CanAccessBasicAnalytics = canAccessBasic;
                ViewBag.UserId = userId;
                ViewBag.UserEmail = User.Identity?.Name;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading simple analytics");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // Check raw user data from database
        public async Task<IActionResult> UserData()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                // Get user data directly from context
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TownTrek.Data.ApplicationDbContext>();
                
                var user = await context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.HasActiveSubscription,
                        u.CurrentSubscriptionTier,
                        u.SubscriptionStartDate,
                        u.SubscriptionEndDate,
                        u.IsTrialUser,
                        u.TrialExpired
                    })
                    .FirstOrDefaultAsync();

                var subscriptions = await context.Subscriptions
                    .Where(s => s.UserId == userId)
                    .Select(s => new {
                        s.Id,
                        s.IsActive,
                        s.PaymentStatus,
                        s.StartDate,
                        s.EndDate,
                        TierName = s.SubscriptionTier.Name,
                        TierDisplayName = s.SubscriptionTier.DisplayName
                    })
                    .ToListAsync();

                var userRoles = await context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                // Also check available subscription tiers
                var availableTiers = await context.SubscriptionTiers
                    .Select(st => new {
                        st.Id,
                        st.Name,
                        st.DisplayName,
                        st.IsActive,
                        Features = st.Features.Select(f => new { f.FeatureKey, f.IsEnabled }).ToList(),
                        Limits = st.Limits.Select(l => new { l.LimitType, l.LimitValue }).ToList()
                    })
                    .ToListAsync();

                return Json(new { 
                    User = user, 
                    Subscriptions = subscriptions,
                    Roles = userRoles,
                    AvailableTiers = availableTiers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user data");
                return Json(new { error = ex.Message });
            }
        }

        // Seed subscription tiers if they don't exist (for development)
        public async Task<IActionResult> SeedTiers()
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TownTrek.Data.ApplicationDbContext>();
                
                // Check if tiers already exist
                var existingTiers = await context.SubscriptionTiers.CountAsync();
                if (existingTiers > 0)
                {
                    return Json(new { message = "Subscription tiers already exist", count = existingTiers });
                }

                // Create Basic tier
                var basicTier = new TownTrek.Models.SubscriptionTier
                {
                    Name = "BASIC",
                    DisplayName = "Basic Plan",
                    Description = "Basic features for small businesses",
                    MonthlyPrice = 99.00m,
                    IsActive = true,
                    SortOrder = 1
                };
                context.SubscriptionTiers.Add(basicTier);
                await context.SaveChangesAsync();

                // Add Basic tier features and limits
                context.SubscriptionTierFeatures.AddRange(new[]
                {
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = basicTier.Id, FeatureKey = "BasicSupport", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = basicTier.Id, FeatureKey = "BasicAnalytics", IsEnabled = true }
                });

                context.SubscriptionTierLimits.AddRange(new[]
                {
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = basicTier.Id, LimitType = "MaxBusinesses", LimitValue = 1 },
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = basicTier.Id, LimitType = "MaxImages", LimitValue = 5 },
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = basicTier.Id, LimitType = "MaxPDFs", LimitValue = 0 }
                });

                // Create Standard tier
                var standardTier = new TownTrek.Models.SubscriptionTier
                {
                    Name = "STANDARD",
                    DisplayName = "Standard Plan",
                    Description = "Enhanced features with analytics",
                    MonthlyPrice = 199.00m,
                    IsActive = true,
                    SortOrder = 2
                };
                context.SubscriptionTiers.Add(standardTier);
                await context.SaveChangesAsync();

                // Add Standard tier features and limits
                context.SubscriptionTierFeatures.AddRange(new[]
                {
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = standardTier.Id, FeatureKey = "BasicSupport", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = standardTier.Id, FeatureKey = "PrioritySupport", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = standardTier.Id, FeatureKey = "BasicAnalytics", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = standardTier.Id, FeatureKey = "StandardAnalytics", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = standardTier.Id, FeatureKey = "PDFUploads", IsEnabled = true }
                });

                context.SubscriptionTierLimits.AddRange(new[]
                {
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = standardTier.Id, LimitType = "MaxBusinesses", LimitValue = 3 },
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = standardTier.Id, LimitType = "MaxImages", LimitValue = 15 },
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = standardTier.Id, LimitType = "MaxPDFs", LimitValue = 5 }
                });

                // Create Premium tier
                var premiumTier = new TownTrek.Models.SubscriptionTier
                {
                    Name = "PREMIUM",
                    DisplayName = "Premium Plan",
                    Description = "All features with advanced analytics",
                    MonthlyPrice = 299.00m,
                    IsActive = true,
                    SortOrder = 3
                };
                context.SubscriptionTiers.Add(premiumTier);
                await context.SaveChangesAsync();

                // Add Premium tier features and limits
                context.SubscriptionTierFeatures.AddRange(new[]
                {
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "BasicSupport", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "PrioritySupport", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "DedicatedSupport", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "BasicAnalytics", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "StandardAnalytics", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "PremiumAnalytics", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "FeaturedPlacement", IsEnabled = true },
                    new TownTrek.Models.SubscriptionTierFeature { SubscriptionTierId = premiumTier.Id, FeatureKey = "PDFUploads", IsEnabled = true }
                });

                context.SubscriptionTierLimits.AddRange(new[]
                {
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = premiumTier.Id, LimitType = "MaxBusinesses", LimitValue = -1 }, // Unlimited
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = premiumTier.Id, LimitType = "MaxImages", LimitValue = -1 }, // Unlimited
                    new TownTrek.Models.SubscriptionTierLimit { SubscriptionTierId = premiumTier.Id, LimitType = "MaxPDFs", LimitValue = -1 } // Unlimited
                });

                await context.SaveChangesAsync();

                return Json(new { message = "Subscription tiers created successfully", tiersCreated = 3 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding subscription tiers");
                return Json(new { error = ex.Message });
            }
        }

        // Fix existing Premium tier missing BasicAnalytics
        public async Task<IActionResult> FixPremiumTier()
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TownTrek.Data.ApplicationDbContext>();
                
                var premiumTier = await context.SubscriptionTiers
                    .Include(st => st.Features)
                    .FirstOrDefaultAsync(st => st.Name.ToUpper() == "PREMIUM");

                if (premiumTier == null)
                {
                    return Json(new { error = "Premium tier not found" });
                }

                // Check if BasicAnalytics feature exists
                var hasBasicAnalytics = premiumTier.Features.Any(f => f.FeatureKey == "BasicAnalytics");
                
                if (!hasBasicAnalytics)
                {
                    // Add missing BasicAnalytics feature
                    context.SubscriptionTierFeatures.Add(new TownTrek.Models.SubscriptionTierFeature 
                    { 
                        SubscriptionTierId = premiumTier.Id, 
                        FeatureKey = "BasicAnalytics", 
                        IsEnabled = true 
                    });
                    
                    await context.SaveChangesAsync();
                    return Json(new { message = "Added BasicAnalytics feature to Premium tier" });
                }

                return Json(new { message = "Premium tier already has BasicAnalytics feature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing Premium tier");
                return Json(new { error = ex.Message });
            }
        }

        // Sync user subscription data to fix mismatches
        public async Task<IActionResult> SyncUserData()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TownTrek.Data.ApplicationDbContext>();
                
                var user = await context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { error = "User not found" });
                }

                var activeSubscription = await context.Subscriptions
                    .Include(s => s.SubscriptionTier)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                if (activeSubscription != null)
                {
                    // Sync user flags with subscription record
                    var oldTier = user.CurrentSubscriptionTier;
                    user.CurrentSubscriptionTier = activeSubscription.SubscriptionTier.Name;
                    user.HasActiveSubscription = true;
                    user.SubscriptionStartDate = activeSubscription.StartDate;
                    user.SubscriptionEndDate = activeSubscription.EndDate;

                    await context.SaveChangesAsync();

                    return Json(new { 
                        message = "User data synced successfully", 
                        oldTier = oldTier,
                        newTier = user.CurrentSubscriptionTier,
                        subscriptionTier = activeSubscription.SubscriptionTier.Name
                    });
                }

                return Json(new { message = "No active subscription found to sync" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing user data");
                return Json(new { error = ex.Message });
            }
        }

        // Fix existing Basic tier to include BasicAnalytics
        public async Task<IActionResult> FixBasicTier()
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TownTrek.Data.ApplicationDbContext>();
                
                var basicTier = await context.SubscriptionTiers
                    .Include(st => st.Features)
                    .FirstOrDefaultAsync(st => st.Name.ToUpper() == "BASIC");

                if (basicTier == null)
                {
                    return Json(new { error = "Basic tier not found" });
                }

                // Check if BasicAnalytics feature exists
                var hasBasicAnalytics = basicTier.Features.Any(f => f.FeatureKey == "BasicAnalytics");
                
                if (!hasBasicAnalytics)
                {
                    // Add missing BasicAnalytics feature
                    context.SubscriptionTierFeatures.Add(new TownTrek.Models.SubscriptionTierFeature 
                    { 
                        SubscriptionTierId = basicTier.Id, 
                        FeatureKey = "BasicAnalytics", 
                        IsEnabled = true 
                    });
                    
                    await context.SaveChangesAsync();
                    return Json(new { message = "Added BasicAnalytics feature to Basic tier" });
                }

                return Json(new { message = "Basic tier already has BasicAnalytics feature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing Basic tier");
                return Json(new { error = ex.Message });
            }
        }

        // Fix existing Standard tier to include StandardAnalytics
        public async Task<IActionResult> FixStandardTier()
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TownTrek.Data.ApplicationDbContext>();
                
                var standardTier = await context.SubscriptionTiers
                    .Include(st => st.Features)
                    .FirstOrDefaultAsync(st => st.Name.ToUpper() == "STANDARD");

                if (standardTier == null)
                {
                    return Json(new { error = "Standard tier not found" });
                }

                // Check if StandardAnalytics feature exists
                var hasStandardAnalytics = standardTier.Features.Any(f => f.FeatureKey == "StandardAnalytics");
                
                if (!hasStandardAnalytics)
                {
                    // Add missing StandardAnalytics feature
                    context.SubscriptionTierFeatures.Add(new TownTrek.Models.SubscriptionTierFeature 
                    { 
                        SubscriptionTierId = standardTier.Id, 
                        FeatureKey = "StandardAnalytics", 
                        IsEnabled = true 
                    });
                    
                    await context.SaveChangesAsync();
                    return Json(new { message = "Added StandardAnalytics feature to Standard tier" });
                }

                return Json(new { message = "Standard tier already has StandardAnalytics feature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing Standard tier");
                return Json(new { error = ex.Message });
            }
        }
    }
}