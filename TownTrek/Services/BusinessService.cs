using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Services
{
    public interface IBusinessService
    {
        Task<AddBusinessViewModel> GetAddBusinessViewModelAsync(string userId);
        Task<ServiceResult> CreateBusinessAsync(AddBusinessViewModel model, string userId);
        Task<ServiceResult> UpdateBusinessAsync(int businessId, AddBusinessViewModel model, string userId);
        Task<List<Business>> GetUserBusinessesAsync(string userId);
        Task<Business?> GetBusinessByIdAsync(int id, string userId);
        Task<ServiceResult> DeleteBusinessAsync(int businessId, string userId);
        Task<bool> CanUserAddBusinessAsync(string userId);
        Task<List<BusinessCategoryOption>> GetBusinessCategoriesAsync();
        Task<List<BusinessCategoryOption>> GetSubCategoriesAsync(string category);
    }

    public class BusinessService : IBusinessService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionTierService _subscriptionService;
        private readonly ILogger<BusinessService> _logger;

        public BusinessService(
            ApplicationDbContext context,
            ISubscriptionTierService subscriptionService,
            ILogger<BusinessService> logger)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public async Task<AddBusinessViewModel> GetAddBusinessViewModelAsync(string userId)
        {
            var viewModel = new AddBusinessViewModel();

            // Load available towns
            viewModel.AvailableTowns = await _context.Towns
                .Where(t => t.IsActive)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .ToListAsync();

            // Load business categories
            viewModel.AvailableCategories = await GetBusinessCategoriesAsync();

            // Get user's subscription limits
            var user = await _context.Users
                .Include(u => u.Subscriptions.Where(s => s.IsActive))
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Limits)
                .Include(u => u.Subscriptions.Where(s => s.IsActive))
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Features)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var activeSubscription = user?.Subscriptions.FirstOrDefault(s => s.IsActive);
            
            if (activeSubscription != null)
            {
                var tier = activeSubscription.SubscriptionTier;

                viewModel.UserLimits = new SubscriptionLimits
                {
                    MaxBusinesses = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxBusinesses")?.LimitValue ?? 1,
                    MaxImages = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxImages")?.LimitValue ?? 5,
                    MaxPDFs = tier.Limits.FirstOrDefault(l => l.LimitType == "MaxPDFs")?.LimitValue ?? 0,
                    HasBasicSupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "BasicSupport")?.IsEnabled ?? false,
                    HasPrioritySupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "PrioritySupport")?.IsEnabled ?? false,
                    HasDedicatedSupport = tier.Features.FirstOrDefault(f => f.FeatureKey == "DedicatedSupport")?.IsEnabled ?? false,
                    HasBasicAnalytics = tier.Features.FirstOrDefault(f => f.FeatureKey == "BasicAnalytics")?.IsEnabled ?? false,
                    HasAdvancedAnalytics = tier.Features.FirstOrDefault(f => f.FeatureKey == "AdvancedAnalytics")?.IsEnabled ?? false,
                    HasFeaturedPlacement = tier.Features.FirstOrDefault(f => f.FeatureKey == "FeaturedPlacement")?.IsEnabled ?? false,
                    HasPDFUploads = tier.Features.FirstOrDefault(f => f.FeatureKey == "PDFUploads")?.IsEnabled ?? false
                };
            }
            else
            {
                // Default limits for users without subscription (free tier)
                viewModel.UserLimits = new SubscriptionLimits
                {
                    MaxBusinesses = 1,
                    MaxImages = 3,
                    MaxPDFs = 0,
                    HasBasicSupport = false,
                    HasPrioritySupport = false,
                    HasDedicatedSupport = false,
                    HasBasicAnalytics = false,
                    HasAdvancedAnalytics = false,
                    HasFeaturedPlacement = false,
                    HasPDFUploads = false
                };
            }

            // Get current business count
            viewModel.CurrentBusinessCount = await _context.Businesses
                .CountAsync(b => b.UserId == userId && b.Status != "Deleted");

            // Initialize business hours
            viewModel.BusinessHours = GetDefaultBusinessHours();

            return viewModel;
        }

        public async Task<ServiceResult> CreateBusinessAsync(AddBusinessViewModel model, string userId)
        {
            try
            {
                // Check if user can add more businesses
                if (!await CanUserAddBusinessAsync(userId))
                {
                    return ServiceResult.Error("You have reached your subscription limit for businesses. Please upgrade your plan.");
                }

                // Validate town exists
                var town = await _context.Towns.FindAsync(model.TownId);
                if (town == null)
                {
                    return ServiceResult.Error("Selected town not found.");
                }

                // Create business entity
                var business = new Business
                {
                    UserId = userId,
                    TownId = model.TownId,
                    Name = model.BusinessName,
                    Category = model.BusinessCategory,
                    SubCategory = model.SubCategory,
                    Description = model.BusinessDescription,
                    ShortDescription = model.ShortDescription,
                    PhoneNumber = model.PhoneNumber,
                    PhoneNumber2 = model.PhoneNumber2,
                    EmailAddress = model.EmailAddress,
                    Website = model.Website,
                    PhysicalAddress = model.PhysicalAddress,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Businesses.AddAsync(business);
                await _context.SaveChangesAsync();

                // Add business hours
                await AddBusinessHoursAsync(business.Id, model.BusinessHours);

                // Add business services
                await AddBusinessServicesAsync(business.Id, model.Services);

                // Handle file uploads (logo, cover image, gallery images)
                await HandleFileUploadsAsync(business.Id, model);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' created by user {UserId}", model.BusinessName, userId);
                return ServiceResult.Success(business.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating business for user {UserId}", userId);
                return ServiceResult.Error("An error occurred while creating your business listing.");
            }
        }

        public async Task<ServiceResult> UpdateBusinessAsync(int businessId, AddBusinessViewModel model, string userId)
        {
            try
            {
                var business = await _context.Businesses
                    .Include(b => b.BusinessHours)
                    .Include(b => b.BusinessServices)
                    .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);

                if (business == null)
                {
                    return ServiceResult.Error("Business not found or you don't have permission to edit it.");
                }

                // Update business properties
                business.Name = model.BusinessName;
                business.Category = model.BusinessCategory;
                business.SubCategory = model.SubCategory;
                business.Description = model.BusinessDescription;
                business.ShortDescription = model.ShortDescription;
                business.PhoneNumber = model.PhoneNumber;
                business.PhoneNumber2 = model.PhoneNumber2;
                business.EmailAddress = model.EmailAddress;
                business.Website = model.Website;
                business.PhysicalAddress = model.PhysicalAddress;
                business.Latitude = model.Latitude;
                business.Longitude = model.Longitude;
                business.UpdatedAt = DateTime.UtcNow;

                // Update business hours
                _context.BusinessHours.RemoveRange(business.BusinessHours);
                await AddBusinessHoursAsync(business.Id, model.BusinessHours);

                // Update business services
                _context.BusinessServices.RemoveRange(business.BusinessServices);
                await AddBusinessServicesAsync(business.Id, model.Services);

                // Handle file uploads
                await HandleFileUploadsAsync(business.Id, model);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' updated by user {UserId}", model.BusinessName, userId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business {BusinessId} for user {UserId}", businessId, userId);
                return ServiceResult.Error("An error occurred while updating your business listing.");
            }
        }

        public async Task<List<Business>> GetUserBusinessesAsync(string userId)
        {
            return await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.BusinessImages.Where(i => i.ImageType == "Logo"))
                .Where(b => b.UserId == userId && b.Status != "Deleted")
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Business?> GetBusinessByIdAsync(int id, string userId)
        {
            return await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessImages)
                .Include(b => b.BusinessServices)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        }

        public async Task<ServiceResult> DeleteBusinessAsync(int businessId, string userId)
        {
            try
            {
                var business = await _context.Businesses
                    .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);

                if (business == null)
                {
                    return ServiceResult.Error("Business not found or you don't have permission to delete it.");
                }

                business.Status = "Deleted";
                business.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' deleted by user {UserId}", business.Name, userId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting business {BusinessId} for user {UserId}", businessId, userId);
                return ServiceResult.Error("An error occurred while deleting your business listing.");
            }
        }

        public async Task<bool> CanUserAddBusinessAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Subscriptions.Where(s => s.IsActive))
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Limits)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false; // User not found
            }

            // For development: If no active subscription, allow 1 free business
            var activeSubscription = user.Subscriptions.FirstOrDefault(s => s.IsActive);
            int maxBusinesses = 1; // Default free limit

            if (activeSubscription != null)
            {
                maxBusinesses = activeSubscription.SubscriptionTier.Limits
                    .FirstOrDefault(l => l.LimitType == "MaxBusinesses")?.LimitValue ?? 1;
            }

            if (maxBusinesses == -1) // Unlimited
            {
                return true;
            }

            var currentCount = await _context.Businesses
                .CountAsync(b => b.UserId == userId && b.Status != "Deleted");

            return currentCount < maxBusinesses;
        }

        public async Task<List<BusinessCategoryOption>> GetBusinessCategoriesAsync()
        {
            // This would typically come from a database table, but for now we'll use the predefined categories
            return new List<BusinessCategoryOption>
            {
                new() { Value = "shops-retail", Text = "Shops & Retail", Description = "Local shops and retail businesses", IconClass = "fas fa-shopping-bag" },
                new() { Value = "restaurants-food", Text = "Restaurants & Food Services", Description = "Restaurants, cafes, and food services", IconClass = "fas fa-utensils" },
                new() { Value = "markets-vendors", Text = "Markets & Vendors", Description = "Local markets and vendor stalls", IconClass = "fas fa-store" },
                new() { Value = "accommodation", Text = "Accommodation", Description = "Hotels, guesthouses, and lodging", IconClass = "fas fa-bed" },
                new() { Value = "tours-experiences", Text = "Tours & Experiences", Description = "Tour guides and experience providers", IconClass = "fas fa-map-marked-alt" },
                new() { Value = "events", Text = "Events", Description = "Local events and entertainment", IconClass = "fas fa-calendar-alt" }
            };
        }

        public async Task<List<BusinessCategoryOption>> GetSubCategoriesAsync(string category)
        {
            // Return subcategories based on main category
            return category switch
            {
                "shops-retail" => new List<BusinessCategoryOption>
                {
                    new() { Value = "clothing", Text = "Clothing & Fashion" },
                    new() { Value = "electronics", Text = "Electronics" },
                    new() { Value = "books", Text = "Books & Stationery" },
                    new() { Value = "gifts", Text = "Gifts & Souvenirs" },
                    new() { Value = "hardware", Text = "Hardware & Tools" },
                    new() { Value = "pharmacy", Text = "Pharmacy & Health" }
                },
                "restaurants-food" => new List<BusinessCategoryOption>
                {
                    new() { Value = "restaurant", Text = "Restaurant" },
                    new() { Value = "cafe", Text = "Cafe & Coffee Shop" },
                    new() { Value = "fast-food", Text = "Fast Food" },
                    new() { Value = "bakery", Text = "Bakery" },
                    new() { Value = "bar", Text = "Bar & Pub" },
                    new() { Value = "takeaway", Text = "Takeaway" }
                },
                "accommodation" => new List<BusinessCategoryOption>
                {
                    new() { Value = "hotel", Text = "Hotel" },
                    new() { Value = "guesthouse", Text = "Guesthouse" },
                    new() { Value = "bnb", Text = "Bed & Breakfast" },
                    new() { Value = "self-catering", Text = "Self-catering" },
                    new() { Value = "backpackers", Text = "Backpackers" },
                    new() { Value = "camping", Text = "Camping & Caravan" }
                },
                _ => new List<BusinessCategoryOption>()
            };
        }

        private List<BusinessHourViewModel> GetDefaultBusinessHours()
        {
            var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            var businessHours = new List<BusinessHourViewModel>();

            for (int i = 0; i < days.Length; i++)
            {
                businessHours.Add(new BusinessHourViewModel
                {
                    DayOfWeek = i,
                    DayName = days[i],
                    IsOpen = i >= 1 && i <= 5, // Monday to Friday open by default
                    OpenTime = "09:00",
                    CloseTime = "17:00"
                });
            }

            return businessHours;
        }

        private async Task AddBusinessHoursAsync(int businessId, List<BusinessHourViewModel> hours)
        {
            var businessHours = hours.Where(h => h.IsOpen).Select(h => new BusinessHour
            {
                BusinessId = businessId,
                DayOfWeek = h.DayOfWeek,
                OpenTime = TimeSpan.TryParse(h.OpenTime, out var openTime) ? openTime : null,
                CloseTime = TimeSpan.TryParse(h.CloseTime, out var closeTime) ? closeTime : null,
                IsOpen = h.IsOpen,
                IsSpecialHours = h.IsSpecialHours,
                SpecialHoursNote = h.SpecialHoursNote
            }).ToList();

            await _context.BusinessHours.AddRangeAsync(businessHours);
        }

        private async Task AddBusinessServicesAsync(int businessId, List<string> services)
        {
            var serviceDefinitions = new Dictionary<string, string>
            {
                { "delivery", "Delivery Available" },
                { "takeaway", "Takeaway/Collection" },
                { "wheelchair", "Wheelchair Accessible" },
                { "parking", "Parking Available" },
                { "wifi", "Free WiFi" },
                { "cards", "Card Payments Accepted" }
            };

            var businessServices = services.Where(s => serviceDefinitions.ContainsKey(s))
                .Select(s => new Models.BusinessService
                {
                    BusinessId = businessId,
                    ServiceType = s,
                    ServiceName = serviceDefinitions[s],
                    IsActive = true
                }).ToList();

            await _context.BusinessServices.AddRangeAsync(businessServices);
        }

        private async Task HandleFileUploadsAsync(int businessId, AddBusinessViewModel model)
        {
            // This is a placeholder for file upload handling
            // In a real implementation, you would:
            // 1. Validate file types and sizes
            // 2. Generate unique file names
            // 3. Save files to storage (local, Azure Blob, AWS S3, etc.)
            // 4. Create BusinessImage records
            // 5. Update Business entity with file URLs

            // For now, we'll just log that files were received
            if (model.BusinessLogo != null)
            {
                _logger.LogInformation("Logo file received for business {BusinessId}: {FileName}", businessId, model.BusinessLogo.FileName);
            }

            if (model.CoverImage != null)
            {
                _logger.LogInformation("Cover image received for business {BusinessId}: {FileName}", businessId, model.CoverImage.FileName);
            }

            if (model.BusinessImages?.Any() == true)
            {
                _logger.LogInformation("Gallery images received for business {BusinessId}: {Count} files", businessId, model.BusinessImages.Count);
            }

            await Task.CompletedTask;
        }
    }
}