using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services
{
    public interface ISubscriptionTierService
    {
        Task<List<SubscriptionTierViewModel>> GetAllTiersAsync();
        Task<SubscriptionTierViewModel?> GetTierByIdAsync(int id);
        Task<ServiceResult> CreateTierAsync(SubscriptionTierViewModel model, string adminUserId);
        Task<ServiceResult> UpdateTierAsync(SubscriptionTierViewModel model, string adminUserId);
        Task<ServiceResult> UpdateTierPriceAsync(PriceChangeViewModel model, string adminUserId);
        Task<ServiceResult> DeactivateTierAsync(int tierId, string adminUserId);
        Task<List<SubscriptionTier>> GetActiveTiersForRegistrationAsync();
        Task<PriceChangeViewModel> GetPriceChangeModelAsync(int tierId);
        Task<SubscriptionTierListViewModel> GetTierListViewModelAsync();
    }

    public class SubscriptionTierService : ISubscriptionTierService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriptionTierService> _logger;
        private readonly IEmailService _emailService;

        public SubscriptionTierService(
            ApplicationDbContext context,
            ILogger<SubscriptionTierService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<List<SubscriptionTierViewModel>> GetAllTiersAsync()
        {
            var tiers = await _context.SubscriptionTiers
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Subscriptions.Where(s => s.IsActive))
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            return tiers.Select(MapToViewModel).ToList();
        }

        public async Task<SubscriptionTierViewModel?> GetTierByIdAsync(int id)
        {
            var tier = await _context.SubscriptionTiers
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Subscriptions.Where(s => s.IsActive))
                .FirstOrDefaultAsync(t => t.Id == id);

            return tier != null ? MapToViewModel(tier) : null;
        }

        public async Task<ServiceResult> CreateTierAsync(SubscriptionTierViewModel model, string adminUserId)
        {
            try
            {
                // Check if tier name already exists
                var existingTier = await _context.SubscriptionTiers
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == model.Name.ToLower());

                if (existingTier != null)
                    return ServiceResult.Error("A tier with this name already exists");

                var tier = new SubscriptionTier
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    MonthlyPrice = model.MonthlyPrice,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder,
                    UpdatedById = adminUserId
                };

                await _context.SubscriptionTiers.AddAsync(tier);
                await _context.SaveChangesAsync();

                // Add limits
                await AddTierLimitsAsync(tier.Id, model);

                // Add features
                await AddTierFeaturesAsync(tier.Id, model);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription tier '{TierName}' created by admin {AdminId}", model.Name, adminUserId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription tier");
                return ServiceResult.Error("An error occurred while creating the tier");
            }
        }

        public async Task<ServiceResult> UpdateTierAsync(SubscriptionTierViewModel model, string adminUserId)
        {
            try
            {
                var tier = await _context.SubscriptionTiers
                    .Include(t => t.Limits)
                    .Include(t => t.Features)
                    .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (tier == null)
                    return ServiceResult.Error("Tier not found");

                // Check if another tier with same name exists
                var existingTier = await _context.SubscriptionTiers
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == model.Name.ToLower() && t.Id != model.Id);

                if (existingTier != null)
                    return ServiceResult.Error("A tier with this name already exists");

                // Update basic properties
                tier.Name = model.Name;
                tier.DisplayName = model.DisplayName;
                tier.Description = model.Description;
                tier.IsActive = model.IsActive;
                tier.SortOrder = model.SortOrder;
                tier.UpdatedAt = DateTime.UtcNow;
                tier.UpdatedById = adminUserId;

                // Update limits
                await UpdateTierLimitsAsync(tier, model);

                // Update features
                await UpdateTierFeaturesAsync(tier, model);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription tier '{TierName}' updated by admin {AdminId}", model.Name, adminUserId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription tier");
                return ServiceResult.Error("An error occurred while updating the tier");
            }
        }

        public async Task<ServiceResult> UpdateTierPriceAsync(PriceChangeViewModel model, string adminUserId)
        {
            try
            {
                var tier = await _context.SubscriptionTiers
                    .Include(t => t.Subscriptions.Where(s => s.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == model.SubscriptionTierId);

                if (tier == null)
                    return ServiceResult.Error("Tier not found");

                var oldPrice = tier.MonthlyPrice;

                // Record price change history
                var priceChange = new PriceChangeHistory
                {
                    SubscriptionTierId = model.SubscriptionTierId,
                    OldPrice = oldPrice,
                    NewPrice = model.NewPrice,
                    ChangedById = adminUserId,
                    ChangeReason = model.ChangeReason,
                    EffectiveDate = model.EffectiveDate,
                    NotificationSent = false
                };

                await _context.PriceChangeHistory.AddAsync(priceChange);

                // Update tier price
                tier.MonthlyPrice = model.NewPrice;
                tier.UpdatedAt = DateTime.UtcNow;
                tier.UpdatedById = adminUserId;

                await _context.SaveChangesAsync();

                // Send notifications to affected customers if requested
                if (model.SendNotification && tier.Subscriptions.Any())
                {
                    await SendPriceChangeNotificationsAsync(tier, oldPrice, model.NewPrice, model.EffectiveDate);
                    
                    priceChange.NotificationSent = true;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Price changed for tier '{TierName}' from R{OldPrice} to R{NewPrice} by admin {AdminId}", 
                    tier.Name, oldPrice, model.NewPrice, adminUserId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tier price");
                return ServiceResult.Error("An error occurred while updating the price");
            }
        }

        public async Task<ServiceResult> DeactivateTierAsync(int tierId, string adminUserId)
        {
            try
            {
                var tier = await _context.SubscriptionTiers
                    .Include(t => t.Subscriptions.Where(s => s.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == tierId);

                if (tier == null)
                    return ServiceResult.Error("Tier not found");

                if (tier.Subscriptions.Any())
                    return ServiceResult.Error("Cannot deactivate tier with active subscriptions");

                tier.IsActive = false;
                tier.UpdatedAt = DateTime.UtcNow;
                tier.UpdatedById = adminUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription tier '{TierName}' deactivated by admin {AdminId}", tier.Name, adminUserId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating tier");
                return ServiceResult.Error("An error occurred while deactivating the tier");
            }
        }

        public async Task<List<SubscriptionTier>> GetActiveTiersForRegistrationAsync()
        {
            return await _context.SubscriptionTiers
                .Where(t => t.IsActive)
                .Include(t => t.Limits)
                .Include(t => t.Features)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<PriceChangeViewModel> GetPriceChangeModelAsync(int tierId)
        {
            var tier = await _context.SubscriptionTiers
                .Include(t => t.Subscriptions.Where(s => s.IsActive))
                .FirstOrDefaultAsync(t => t.Id == tierId);

            if (tier == null)
                throw new ArgumentException("Tier not found");

            return new PriceChangeViewModel
            {
                SubscriptionTierId = tierId,
                TierName = tier.DisplayName,
                CurrentPrice = tier.MonthlyPrice,
                NewPrice = tier.MonthlyPrice,
                AffectedCustomersCount = tier.Subscriptions.Count
            };
        }

        public async Task<SubscriptionTierListViewModel> GetTierListViewModelAsync()
        {
            var tiers = await GetAllTiersAsync();
            var totalActiveSubscriptions = await _context.Subscriptions.CountAsync(s => s.IsActive);
            var totalRevenue = await _context.Subscriptions
                .Where(s => s.IsActive)
                .SumAsync(s => s.MonthlyPrice);

            var recentPriceChanges = await _context.PriceChangeHistory
                .Include(p => p.SubscriptionTier)
                .Include(p => p.ChangedBy)
                .OrderByDescending(p => p.ChangeDate)
                .Take(10)
                .ToListAsync();

            return new SubscriptionTierListViewModel
            {
                Tiers = tiers,
                TotalActiveSubscriptions = totalActiveSubscriptions,
                TotalMonthlyRevenue = totalRevenue,
                RecentPriceChanges = recentPriceChanges
            };
        }

        private async Task SendPriceChangeNotificationsAsync(SubscriptionTier tier, decimal oldPrice, decimal newPrice, DateTime effectiveDate)
        {
            var affectedUsers = await _context.Subscriptions
                .Where(s => s.SubscriptionTierId == tier.Id && s.IsActive)
                .Include(s => s.User)
                .Select(s => s.User)
                .ToListAsync();

            foreach (var user in affectedUsers)
            {
                await _emailService.SendPriceChangeNotificationAsync(
                    user.Email,
                    user.FirstName,
                    tier.DisplayName,
                    oldPrice,
                    newPrice,
                    effectiveDate
                );
            }

            _logger.LogInformation("Price change notifications sent to {Count} customers for tier '{TierName}'", 
                affectedUsers.Count, tier.Name);
        }

        private async Task AddTierLimitsAsync(int tierId, SubscriptionTierViewModel model)
        {
            var limits = new List<SubscriptionTierLimit>
            {
                new() { SubscriptionTierId = tierId, LimitType = "MaxBusinesses", LimitValue = model.MaxBusinesses },
                new() { SubscriptionTierId = tierId, LimitType = "MaxImages", LimitValue = model.MaxImages },
                new() { SubscriptionTierId = tierId, LimitType = "MaxPDFs", LimitValue = model.MaxPDFs }
            };

            await _context.SubscriptionTierLimits.AddRangeAsync(limits);
        }

        private async Task AddTierFeaturesAsync(int tierId, SubscriptionTierViewModel model)
        {
            var features = new List<SubscriptionTierFeature>
            {
                new() { SubscriptionTierId = tierId, FeatureKey = "BasicSupport", IsEnabled = model.HasBasicSupport, FeatureName = "Basic Support" },
                new() { SubscriptionTierId = tierId, FeatureKey = "PrioritySupport", IsEnabled = model.HasPrioritySupport, FeatureName = "Priority Support" },
                new() { SubscriptionTierId = tierId, FeatureKey = "DedicatedSupport", IsEnabled = model.HasDedicatedSupport, FeatureName = "Dedicated Support" },
                new() { SubscriptionTierId = tierId, FeatureKey = "BasicAnalytics", IsEnabled = model.HasBasicAnalytics, FeatureName = "Basic Analytics" },
                new() { SubscriptionTierId = tierId, FeatureKey = "AdvancedAnalytics", IsEnabled = model.HasAdvancedAnalytics, FeatureName = "Advanced Analytics" },
                new() { SubscriptionTierId = tierId, FeatureKey = "FeaturedPlacement", IsEnabled = model.HasFeaturedPlacement, FeatureName = "Featured Placement" },
                new() { SubscriptionTierId = tierId, FeatureKey = "PDFUploads", IsEnabled = model.HasPDFUploads, FeatureName = "PDF Uploads" }
            };

            await _context.SubscriptionTierFeatures.AddRangeAsync(features);
        }

        private async Task UpdateTierLimitsAsync(SubscriptionTier tier, SubscriptionTierViewModel model)
        {
            // Remove existing limits
            _context.SubscriptionTierLimits.RemoveRange(tier.Limits);

            // Add new limits
            await AddTierLimitsAsync(tier.Id, model);
        }

        private async Task UpdateTierFeaturesAsync(SubscriptionTier tier, SubscriptionTierViewModel model)
        {
            // Remove existing features
            _context.SubscriptionTierFeatures.RemoveRange(tier.Features);

            // Add new features
            await AddTierFeaturesAsync(tier.Id, model);
        }

        private static SubscriptionTierViewModel MapToViewModel(SubscriptionTier tier)
        {
            var viewModel = new SubscriptionTierViewModel
            {
                Id = tier.Id,
                Name = tier.Name,
                DisplayName = tier.DisplayName,
                Description = tier.Description,
                MonthlyPrice = tier.MonthlyPrice,
                IsActive = tier.IsActive,
                SortOrder = tier.SortOrder,
                CreatedAt = tier.CreatedAt,
                UpdatedAt = tier.UpdatedAt,
                UpdatedByName = tier.UpdatedBy?.FirstName + " " + tier.UpdatedBy?.LastName,
                ActiveSubscriptionsCount = tier.Subscriptions.Count
            };

            // Map limits
            var maxBusinesses = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxBusinesses");
            var maxImages = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxImages");
            var maxPDFs = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxPDFs");

            viewModel.MaxBusinesses = maxBusinesses?.LimitValue ?? 1;
            viewModel.MaxImages = maxImages?.LimitValue ?? 5;
            viewModel.MaxPDFs = maxPDFs?.LimitValue ?? 0;

            // Map features
            viewModel.HasBasicSupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "BasicSupport")?.IsEnabled ?? false;
            viewModel.HasPrioritySupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "PrioritySupport")?.IsEnabled ?? false;
            viewModel.HasDedicatedSupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "DedicatedSupport")?.IsEnabled ?? false;
            viewModel.HasBasicAnalytics = tier.Features.FirstOrDefault(f => f.FeatureKey == "BasicAnalytics")?.IsEnabled ?? false;
            viewModel.HasAdvancedAnalytics = tier.Features.FirstOrDefault(f => f.FeatureKey == "AdvancedAnalytics")?.IsEnabled ?? false;
            viewModel.HasFeaturedPlacement = tier.Features.FirstOrDefault(f => f.FeatureKey == "FeaturedPlacement")?.IsEnabled ?? false;
            viewModel.HasPDFUploads = tier.Features.FirstOrDefault(f => f.FeatureKey == "PDFUploads")?.IsEnabled ?? false;

            return viewModel;
        }
    }

    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Data { get; set; }

        public static ServiceResult Success(object? data = null) => new() { IsSuccess = true, Data = data };
        public static ServiceResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}