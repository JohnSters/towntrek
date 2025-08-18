using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;
using System.Security.Claims;

namespace TownTrek.Services
{
    public class ClientService(
        ApplicationDbContext context,
        IBusinessService businessService,
        ISubscriptionTierService subscriptionService,
        ISubscriptionAuthService subscriptionAuthService,
        UserManager<ApplicationUser> userManager,
        ITrialService trialService,
        IAdminMessageService adminMessageService,
        ILogger<ClientService> logger) : IClientService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IBusinessService _businessService = businessService;
        private readonly ISubscriptionTierService _subscriptionService = subscriptionService;
        private readonly ISubscriptionAuthService _subscriptionAuthService = subscriptionAuthService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ITrialService _trialService = trialService;
        private readonly IAdminMessageService _adminMessageService = adminMessageService;
        private readonly ILogger<ClientService> _logger = logger;

        public async Task<ClientDashboardViewModel> GetDashboardViewModelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);
            var businesses = await _businessService.GetUserBusinessesAsync(userId);

            // Get trial status if user is a trial user
            var trialStatus = await _trialService.GetTrialStatusAsync(userId);

            return new ClientDashboardViewModel
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
                // Everyone except active trial users can access analytics
                HasAnalyticsAccess = !(trialStatus.IsTrialUser && !trialStatus.IsExpired),
                HasAdvancedAnalyticsAccess = await _subscriptionAuthService.CanAccessFeatureAsync(userId, "AdvancedAnalytics"),
                HasPrioritySupport = authResult.Limits?.HasPrioritySupport ?? false,
                HasDedicatedSupport = authResult.Limits?.HasDedicatedSupport ?? false,
                
                // Trial information
                IsTrialUser = trialStatus.IsTrialUser,
                TrialDaysRemaining = trialStatus.DaysRemaining,
                TrialEndDate = trialStatus.TrialEndDate,
                IsTrialExpired = trialStatus.IsExpired,
                TrialStatusMessage = trialStatus.StatusMessage
            };
        }

        public async Task<ClientSubscriptionViewModel> GetSubscriptionViewModelAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Subscriptions)
                .ThenInclude(s => s.SubscriptionTier)
                .ThenInclude(st => st.Limits)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var availableTiers = await _subscriptionService.GetActiveTiersForRegistrationAsync();
            var currentSubscription = user?.Subscriptions.FirstOrDefault(s => s.IsActive) 
                ?? user?.Subscriptions.FirstOrDefault();

            return new ClientSubscriptionViewModel
            {
                CurrentSubscription = currentSubscription,
                AvailableTiers = availableTiers,
                BusinessCount = await _context.Businesses.CountAsync(b => b.UserId == userId && b.Status != "Deleted")
            };
        }

        public async Task<ClientAnalyticsViewModel> GetAnalyticsViewModelAsync(string userId)
        {
            // This method is now deprecated - use IAnalyticsService directly
            var businesses = await _businessService.GetUserBusinessesAsync(userId);
            
            return new ClientAnalyticsViewModel
            {
                Businesses = businesses,
                Overview = new AnalyticsOverview { TotalViews = businesses.Sum(b => b.ViewCount) }
            };
        }

        public async Task<AddBusinessViewModel> PrepareEditBusinessViewModelAsync(int businessId, string userId)
        {
            var business = await _businessService.GetBusinessByIdAsync(businessId, userId);
            if (business == null)
            {
                throw new InvalidOperationException("Business not found or access denied");
            }

            var model = await _businessService.GetAddBusinessViewModelAsync(userId);
            
            // Populate with existing business data
            model.Id = business.Id;
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
            PopulateBusinessHours(model, business);
            
            // Convert services
            model.Services = business.BusinessServices.Where(s => s.IsActive).Select(s => s.ServiceType).ToList();
            
            // Load existing business images
            model.ExistingBusinessImages = business.BusinessImages.Where(bi => bi.IsActive).ToList();
            
            // Load category-specific data
            await LoadCategorySpecificDataAsync(model, business);

            return model;
        }

        private static void PopulateBusinessHours(AddBusinessViewModel model, Business business)
        {
            var existingHours = business.BusinessHours.ToDictionary(bh => bh.DayOfWeek, bh => bh);
            model.BusinessHours = [];

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
        }

        private async Task LoadCategorySpecificDataAsync(AddBusinessViewModel model, Business business)
        {
            switch (business.Category.ToLower())
            {
                case "shops-retail":
                    await LoadShopDetailsAsync(model, business.Id);
                    break;
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

        private async Task LoadShopDetailsAsync(AddBusinessViewModel model, int businessId)
        {
            var shopDetails = await _context.ShopDetails.FirstOrDefaultAsync(sd => sd.BusinessId == businessId);
            if (shopDetails != null)
            {
                model.ShopType = shopDetails.ShopType;
                model.ShopSize = shopDetails.ShopSize;
                model.BrandNames = shopDetails.BrandNames;
                model.PriceRange = shopDetails.PriceRange;
                model.Specialties = shopDetails.Specialties;
                model.HasOnlineStore = shopDetails.HasOnlineStore;
                model.OffersLayaway = shopDetails.OffersLayaway;
                model.HasFittingRoom = shopDetails.HasFittingRoom;
                model.OffersRepairs = shopDetails.OffersRepairs;
                model.HasLoyaltyProgram = shopDetails.HasLoyaltyProgram;
                model.AcceptsReturns = shopDetails.AcceptsReturns;
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

        public async Task<AddBusinessViewModel> PrepareEditBusinessViewModelWithFormDataAsync(int businessId, string userId, AddBusinessViewModel formData)
        {
            var baseModel = await PrepareEditBusinessViewModelAsync(businessId, userId);
            
            // Preserve the submitted form data
            baseModel.BusinessName = formData.BusinessName;
            baseModel.BusinessCategory = formData.BusinessCategory;
            baseModel.SubCategory = formData.SubCategory;
            baseModel.TownId = formData.TownId;
            baseModel.BusinessDescription = formData.BusinessDescription;
            baseModel.ShortDescription = formData.ShortDescription;
            baseModel.PhoneNumber = formData.PhoneNumber;
            baseModel.PhoneNumber2 = formData.PhoneNumber2;
            baseModel.EmailAddress = formData.EmailAddress;
            baseModel.Website = formData.Website;
            baseModel.PhysicalAddress = formData.PhysicalAddress;
            baseModel.Latitude = formData.Latitude;
            baseModel.Longitude = formData.Longitude;
            baseModel.BusinessHours = formData.BusinessHours;
            baseModel.Services = formData.Services;
            
            return baseModel;
        }

        public async Task<ContactAdminViewModel> GetContactAdminViewModelAsync(string userId)
        {
            return await _adminMessageService.GetContactAdminViewModelAsync(userId);
        }

        public async Task<AdminMessage> CreateAdminMessageAsync(string userId, int topicId, string subject, string message)
        {
            return await _adminMessageService.CreateMessageAsync(userId, topicId, subject, message);
        }

        public async Task<AdminMessageTopic?> GetAdminMessageTopicAsync(int topicId)
        {
            return await _adminMessageService.GetTopicByIdAsync(topicId);
        }

        private static string GetDayName(int dayOfWeek)
        {
            var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return days[dayOfWeek];
        }
    }
}