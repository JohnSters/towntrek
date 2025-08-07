using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Attributes;
using System.Security.Claims;

namespace TownTrek.Controllers
{
    [Authorize]
    public class ClientController(
        ApplicationDbContext context,
        IBusinessService businessService,
        ISubscriptionTierService subscriptionService,
        ISubscriptionAuthService subscriptionAuthService,
        UserManager<ApplicationUser> userManager,
        ILogger<ClientController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IBusinessService _businessService = businessService;
        private readonly ISubscriptionTierService _subscriptionService = subscriptionService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<ClientController> _logger = logger;

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

            // Set subscription tier for layout display
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;

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
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
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
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
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
            
            // Set subscription tier for layout display
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            ViewData["UserSubscriptionTier"] = authResult.SubscriptionTier;
            
            return View("EditBusiness", model);
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
            
            // If there are validation errors, reload the business data and preserve the form data
            var business = await _businessService.GetBusinessByIdAsync(id, userId);
            if (business != null)
            {
                var baseModel = await _businessService.GetAddBusinessViewModelAsync(userId);
                
                // Preserve the form data that was submitted
                baseModel.Id = model.Id;
                baseModel.BusinessName = model.BusinessName;
                baseModel.BusinessCategory = model.BusinessCategory;
                baseModel.SubCategory = model.SubCategory;
                baseModel.TownId = model.TownId;
                baseModel.BusinessDescription = model.BusinessDescription;
                baseModel.ShortDescription = model.ShortDescription;
                baseModel.PhoneNumber = model.PhoneNumber;
                baseModel.PhoneNumber2 = model.PhoneNumber2;
                baseModel.EmailAddress = model.EmailAddress;
                baseModel.Website = model.Website;
                baseModel.PhysicalAddress = model.PhysicalAddress;
                baseModel.Latitude = model.Latitude;
                baseModel.Longitude = model.Longitude;
                baseModel.BusinessHours = model.BusinessHours;
                baseModel.Services = model.Services;
                
                // Load existing business images
                baseModel.ExistingBusinessImages = business.BusinessImages.Where(bi => bi.IsActive).ToList();
                
                return View("AddBusiness", baseModel);
            }
            
            return RedirectToAction("ManageBusinesses");
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
        public IActionResult ValidateAddress([FromBody] AddressValidationRequest request)
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
                .Include(u => u.Subscriptions)
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Limits)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var availableTiers = await _subscriptionService.GetActiveTiersForRegistrationAsync();
            
            // Get current subscription (including inactive ones for display)
            var currentSubscription = user?.Subscriptions.FirstOrDefault(s => s.IsActive) 
                ?? user?.Subscriptions.FirstOrDefault(); // Fallback to any subscription for display
            
            var model = new ClientSubscriptionViewModel
            {
                CurrentSubscription = currentSubscription,
                AvailableTiers = availableTiers,
                BusinessCount = await _context.Businesses.CountAsync(b => b.UserId == userId && b.Status != "Deleted")
            };

            // Set subscription tier for layout display
            ViewData["UserSubscriptionTier"] = currentSubscription?.SubscriptionTier;

            // Clear any inappropriate success messages for existing users
            if (user != null && user.HasActiveSubscription)
            {
                TempData.Remove("SuccessMessage");
            }

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
            
            // Set the ID for editing
            model.Id = business.Id;
            
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

            // Convert business hours - ensure all days are represented
            var existingHours = business.BusinessHours.ToDictionary(bh => bh.DayOfWeek, bh => bh);
            model.BusinessHours = new List<BusinessHourViewModel>();
            
            for (int i = 0; i < 7; i++)
            {
                if (existingHours.TryGetValue(i, out var existingHour))
                {
                    model.BusinessHours.Add(new BusinessHourViewModel
                    {
                        DayOfWeek = i,
                        DayName = GetDayName(i),
                        IsOpen = existingHour.IsOpen,
                        OpenTime = existingHour.OpenTime?.ToString(@"hh\:mm"),
                        CloseTime = existingHour.CloseTime?.ToString(@"hh\:mm"),
                        IsSpecialHours = existingHour.IsSpecialHours,
                        SpecialHoursNote = existingHour.SpecialHoursNote
                    });
                }
                else
                {
                    // Add default closed day
                    model.BusinessHours.Add(new BusinessHourViewModel
                    {
                        DayOfWeek = i,
                        DayName = GetDayName(i),
                        IsOpen = false,
                        OpenTime = "09:00",
                        CloseTime = "17:00"
                    });
                }
            }

            // Convert services
            model.Services = business.BusinessServices.Where(s => s.IsActive).Select(s => s.ServiceType).ToList();

            // Load existing business images
            model.ExistingBusinessImages = business.BusinessImages.Where(bi => bi.IsActive).ToList();

            // Load category-specific data
            await LoadCategorySpecificDataAsync(model, business);

            return model;
        }

        private async Task LoadCategorySpecificDataAsync(AddBusinessViewModel model, Business business)
        {
            switch (business.Category.ToLower())
            {
                case "restaurants-food":
                    await LoadRestaurantDetailsAsync(model, business.Id);
                    break;
                case "accommodation":
                    await LoadAccommodationDetailsAsync(model, business.Id);
                    break;
                case "tours-experiences":
                    await LoadTourDetailsAsync(model, business.Id);
                    break;
                case "events":
                    await LoadEventDetailsAsync(model, business.Id);
                    break;
                case "markets-vendors":
                    await LoadMarketDetailsAsync(model, business.Id);
                    break;
            }
        }

        private async Task LoadRestaurantDetailsAsync(AddBusinessViewModel model, int businessId)
        {
            var restaurantDetails = await _context.RestaurantDetails.FirstOrDefaultAsync(rd => rd.BusinessId == businessId);
            if (restaurantDetails != null)
            {
                model.CuisineType = restaurantDetails.CuisineType;
                model.PriceRange = restaurantDetails.PriceRange;
                model.HasDelivery = restaurantDetails.HasDelivery;
                model.HasTakeaway = restaurantDetails.HasTakeaway;
                model.AcceptsReservations = restaurantDetails.AcceptsReservations;
                model.SeatingCapacity = restaurantDetails.SeatingCapacity;
                model.DietaryOptions = restaurantDetails.DietaryOptions;
                model.MenuUrl = restaurantDetails.MenuUrl;
                model.HasKidsMenu = restaurantDetails.HasKidsMenu;
                model.HasOutdoorSeating = restaurantDetails.HasOutdoorSeating;
                model.ServesBreakfast = restaurantDetails.ServesBreakfast;
                model.ServesLunch = restaurantDetails.ServesLunch;
                model.ServesDinner = restaurantDetails.ServesDinner;
                model.ServesAlcohol = restaurantDetails.ServesAlcohol;
            }
        }

        private async Task LoadAccommodationDetailsAsync(AddBusinessViewModel model, int businessId)
        {
            var accommodationDetails = await _context.AccommodationDetails.FirstOrDefaultAsync(ad => ad.BusinessId == businessId);
            if (accommodationDetails != null)
            {
                model.PropertyType = accommodationDetails.PropertyType;
                model.StarRating = accommodationDetails.StarRating;
                model.RoomCount = accommodationDetails.RoomCount;
                model.MaxGuests = accommodationDetails.MaxGuests;
                model.CheckInTime = accommodationDetails.CheckInTime?.ToString(@"hh\:mm");
                model.CheckOutTime = accommodationDetails.CheckOutTime?.ToString(@"hh\:mm");
                model.Amenities = accommodationDetails.Amenities;
                model.HasWiFi = accommodationDetails.HasWiFi;
                model.HasPool = accommodationDetails.HasPool;
                model.HasRestaurant = accommodationDetails.HasRestaurant;
                model.IsPetFriendly = accommodationDetails.IsPetFriendly;
            }
        }

        private async Task LoadTourDetailsAsync(AddBusinessViewModel model, int businessId)
        {
            var tourDetails = await _context.TourDetails.FirstOrDefaultAsync(td => td.BusinessId == businessId);
            if (tourDetails != null)
            {
                model.TourType = tourDetails.TourType;
                model.Duration = tourDetails.Duration;
                model.MaxGroupSize = tourDetails.MaxGroupSize;
                model.MinGroupSize = tourDetails.MinGroupSize;
                model.MinAge = tourDetails.MinAge;
                model.DifficultyLevel = tourDetails.DifficultyLevel;
                model.DepartureLocation = tourDetails.DepartureLocation;
                model.Itinerary = tourDetails.Itinerary;
                model.IncludedItems = tourDetails.IncludedItems;
                model.ExcludedItems = tourDetails.ExcludedItems;
                model.RequiredEquipment = tourDetails.RequiredEquipment;
                model.PricingInfo = tourDetails.PricingInfo;
                model.RequiresBooking = tourDetails.RequiresBooking;
                model.AdvanceBookingDays = tourDetails.AdvanceBookingDays;
                model.IsWeatherDependent = tourDetails.IsWeatherDependent;
                model.IsAccessible = tourDetails.IsAccessible;
            }
        }

        private async Task LoadEventDetailsAsync(AddBusinessViewModel model, int businessId)
        {
            var eventDetails = await _context.EventDetails.FirstOrDefaultAsync(ed => ed.BusinessId == businessId);
            if (eventDetails != null)
            {
                model.EventType = eventDetails.EventType;
                model.EventStartDate = eventDetails.StartDate;
                model.EventEndDate = eventDetails.EndDate;
                model.EventStartTime = eventDetails.StartTime?.ToString(@"hh\:mm");
                model.EventEndTime = eventDetails.EndTime?.ToString(@"hh\:mm");
                model.IsRecurringEvent = eventDetails.IsRecurring;
                model.EventRecurrencePattern = eventDetails.RecurrencePattern;
                model.Venue = eventDetails.Venue;
                model.VenueAddress = eventDetails.VenueAddress;
                model.MaxAttendees = eventDetails.MaxAttendees;
                model.TicketInfo = eventDetails.TicketInfo;
                model.OrganizerContact = eventDetails.OrganizerContact;
                model.RequiresTickets = eventDetails.RequiresTickets;
                model.IsFreeEvent = eventDetails.IsFreeEvent;
                model.EventProgram = eventDetails.EventProgram;
                model.HasParking = eventDetails.HasParking;
                model.IsOutdoorEvent = eventDetails.IsOutdoorEvent;
            }
        }

        private async Task LoadMarketDetailsAsync(AddBusinessViewModel model, int businessId)
        {
            var marketDetails = await _context.MarketDetails.FirstOrDefaultAsync(md => md.BusinessId == businessId);
            if (marketDetails != null)
            {
                model.MarketType = marketDetails.MarketType;
                model.IsRecurringMarket = marketDetails.IsRecurring;
                model.RecurrencePattern = marketDetails.RecurrencePattern;
                model.MarketDays = !string.IsNullOrEmpty(marketDetails.MarketDays) 
                    ? marketDetails.MarketDays.Split(',').ToList() 
                    : new List<string>();
                model.MarketStartTime = marketDetails.MarketStartTime?.ToString(@"hh\:mm");
                model.MarketEndTime = marketDetails.MarketEndTime?.ToString(@"hh\:mm");
                model.EstimatedVendorCount = marketDetails.EstimatedVendorCount;
                model.VendorTypes = marketDetails.VendorTypes;
                model.ParkingInfo = marketDetails.ParkingInfo;
                model.EntryFee = marketDetails.EntryFee;
                model.HasRestrooms = marketDetails.HasRestrooms;
                model.HasFoodVendors = marketDetails.HasFoodVendors;
                model.IsCoveredVenue = marketDetails.IsCoveredVenue;
            }
        }

        private static string GetDayName(int dayOfWeek)
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