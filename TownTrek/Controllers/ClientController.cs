using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Attributes;
using System.Security.Claims;

namespace TownTrek.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBusinessService _businessService;
        private readonly ISubscriptionTierService _subscriptionService;
        private readonly ISubscriptionAuthService _subscriptionAuthService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClientController> _logger;

        public ClientController(
            ApplicationDbContext context,
            IBusinessService businessService,
            ISubscriptionTierService subscriptionService,
            ISubscriptionAuthService subscriptionAuthService,
            UserManager<ApplicationUser> userManager,
            ILogger<ClientController> logger)
        {
            _context = context;
            _businessService = businessService;
            _subscriptionService = subscriptionService;
            _subscriptionAuthService = subscriptionAuthService;
            _userManager = userManager;
            _logger = logger;
        }

        // Dashboard - Main overview page
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.GetUserAsync(User);
            
            // Get subscription validation result
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            
            // If payment is pending or rejected, show appropriate message
            if (authResult.HasActiveSubscription && !authResult.IsPaymentValid)
            {
                TempData["WarningMessage"] = $"Your payment status is {authResult.PaymentStatus}. Please complete your payment to access all features.";
                if (!string.IsNullOrEmpty(authResult.RedirectUrl))
                {
                    ViewBag.PaymentUrl = authResult.RedirectUrl;
                }
            }

            var businesses = await _businessService.GetUserBusinessesAsync(userId);
            
            var dashboardModel = new ClientDashboardViewModel
            {
                User = user,
                TotalBusinesses = businesses.Count,
                ActiveBusinesses = businesses.Count(b => b.Status == "Active"),
                PendingBusinesses = businesses.Count(b => b.Status == "Pending"),
                RecentBusinesses = businesses.Take(5).ToList(),
                TotalViews = businesses.Sum(b => b.ViewCount),
                SubscriptionTier = authResult.SubscriptionTier,
                UserLimits = authResult.Limits,
                PaymentStatus = authResult.PaymentStatus,
                CanAddBusiness = authResult.Limits?.MaxBusinesses == -1 || (authResult.Limits?.CurrentBusinessCount < authResult.Limits?.MaxBusinesses),
                HasAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "BasicAnalytics"),
                HasPrioritySupport = authResult.Limits?.HasPrioritySupport ?? false,
                HasDedicatedSupport = authResult.Limits?.HasDedicatedSupport ?? false
            };

            return View(dashboardModel);
        }

        // Business Management
        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> ManageBusinesses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var businesses = await _businessService.GetUserBusinessesAsync(userId);
            
            // Get user limits for display
            var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
            ViewBag.UserLimits = limits;
            
            return View(businesses);
        }

        [RequireActiveSubscription(allowFreeTier: true)]
        public async Task<IActionResult> AddBusiness()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Check if user can add more businesses
            if (!await _businessService.CanUserAddBusinessAsync(userId))
            {
                var limits = await _subscriptionAuthService.GetUserLimitsAsync(userId);
                TempData["ErrorMessage"] = $"You have reached your subscription limit for businesses ({limits.CurrentBusinessCount}/{limits.MaxBusinesses}). Please upgrade your plan.";
                return RedirectToAction("Subscription");
            }

            var model = await _businessService.GetAddBusinessViewModelAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBusiness(AddBusinessViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (ModelState.IsValid)
            {
                var result = await _businessService.CreateBusinessAsync(model, userId);
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Business listing created successfully! It will be reviewed and activated within 24 hours.";
                    return RedirectToAction("ManageBusinesses");
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
            }
            
            // Reload the view model data if there are validation errors
            model = await _businessService.GetAddBusinessViewModelAsync(userId);
            return View(model);
        }

        public async Task<IActionResult> EditBusiness(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var business = await _businessService.GetBusinessByIdAsync(id, userId);
            
            if (business == null)
            {
                TempData["ErrorMessage"] = "Business not found or you don't have permission to edit it.";
                return RedirectToAction("ManageBusinesses");
            }

            // Convert business to view model
            var model = await ConvertBusinessToViewModel(business);
            return View("AddBusiness", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBusiness(int id, AddBusinessViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (ModelState.IsValid)
            {
                var result = await _businessService.UpdateBusinessAsync(id, model, userId);
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Business listing updated successfully!";
                    return RedirectToAction("ManageBusinesses");
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
            }
            
            // Reload the view model data if there are validation errors
            model = await _businessService.GetAddBusinessViewModelAsync(userId);
            return View("AddBusiness", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _businessService.DeleteBusinessAsync(id, userId);
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Business listing deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction("ManageBusinesses");
        }

        // AJAX endpoints for dynamic form behavior
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(string category)
        {
            var subCategories = await _businessService.GetSubCategoriesAsync(category);
            return Json(subCategories);
        }

        [HttpPost]
        public async Task<IActionResult> ValidateAddress([FromBody] AddressValidationRequest request)
        {
            // This would integrate with Google Maps Geocoding API
            // For now, return a mock response
            return Json(new { 
                isValid = true, 
                latitude = -26.2041, 
                longitude = 28.0473,
                formattedAddress = request.Address
            });
        }

        // Profile Management
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // Subscription & Billing
        public async Task<IActionResult> Subscription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _context.Users
                .Include(u => u.Subscriptions.Where(s => s.IsActive))
                .ThenInclude(s => s.SubscriptionTier)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var availableTiers = await _subscriptionService.GetActiveTiersForRegistrationAsync();
            
            var model = new ClientSubscriptionViewModel
            {
                CurrentSubscription = user?.Subscriptions.FirstOrDefault(s => s.IsActive),
                AvailableTiers = availableTiers,
                BusinessCount = await _context.Businesses.CountAsync(b => b.UserId == userId && b.Status != "Deleted")
            };

            return View(model);
        }

        public IActionResult Billing()
        {
            return View();
        }

        // Analytics & Reports
        [RequireActiveSubscription(requiredFeature: "BasicAnalytics")]
        public async Task<IActionResult> Analytics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var businesses = await _businessService.GetUserBusinessesAsync(userId);
            
            var analyticsModel = new ClientAnalyticsViewModel
            {
                Businesses = businesses,
                TotalViews = businesses.Sum(b => b.ViewCount),
                // Add more analytics data as needed
            };

            return View(analyticsModel);
        }

        // Support & Help
        public IActionResult Support()
        {
            return View();
        }

        public IActionResult Documentation()
        {
            return View();
        }

        // Settings
        public IActionResult Settings()
        {
            return View();
        }

        private async Task<AddBusinessViewModel> ConvertBusinessToViewModel(Business business)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await _businessService.GetAddBusinessViewModelAsync(userId);
            
            // Populate with existing business data
            model.BusinessName = business.Name;
            model.BusinessCategory = business.Category;
            model.SubCategory = business.SubCategory;
            model.TownId = business.TownId;
            model.BusinessDescription = business.Description;
            model.ShortDescription = business.ShortDescription;
            model.PhoneNumber = business.PhoneNumber;
            model.PhoneNumber2 = business.PhoneNumber2;
            model.EmailAddress = business.EmailAddress;
            model.Website = business.Website;
            model.PhysicalAddress = business.PhysicalAddress;
            model.Latitude = business.Latitude;
            model.Longitude = business.Longitude;

            // Convert business hours
            model.BusinessHours = business.BusinessHours.Select(bh => new BusinessHourViewModel
            {
                DayOfWeek = bh.DayOfWeek,
                DayName = GetDayName(bh.DayOfWeek),
                IsOpen = bh.IsOpen,
                OpenTime = bh.OpenTime?.ToString(@"hh\:mm"),
                CloseTime = bh.CloseTime?.ToString(@"hh\:mm"),
                IsSpecialHours = bh.IsSpecialHours,
                SpecialHoursNote = bh.SpecialHoursNote
            }).ToList();

            // Convert services
            model.Services = business.BusinessServices.Where(s => s.IsActive).Select(s => s.ServiceType).ToList();

            return model;
        }

        private string GetDayName(int dayOfWeek)
        {
            var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return days[dayOfWeek];
        }
    }

    // Supporting view models
    public class ClientDashboardViewModel
    {
        public ApplicationUser? User { get; set; }
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int PendingBusinesses { get; set; }
        public List<Business> RecentBusinesses { get; set; } = new();
        public int TotalViews { get; set; }
        public SubscriptionTier? SubscriptionTier { get; set; }
        public SubscriptionLimits? UserLimits { get; set; }
        public string? PaymentStatus { get; set; }
        public bool CanAddBusiness { get; set; }
        public bool HasAnalyticsAccess { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasDedicatedSupport { get; set; }
    }

    public class ClientSubscriptionViewModel
    {
        public Subscription? CurrentSubscription { get; set; }
        public List<SubscriptionTier> AvailableTiers { get; set; } = new();
        public int BusinessCount { get; set; }
    }

    public class ClientAnalyticsViewModel
    {
        public List<Business> Businesses { get; set; } = new();
        public int TotalViews { get; set; }
    }

    public class AddressValidationRequest
    {
        public string Address { get; set; } = string.Empty;
    }
}