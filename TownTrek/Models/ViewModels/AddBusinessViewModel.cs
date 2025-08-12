using System.ComponentModel.DataAnnotations;
using TownTrek.Services;

namespace TownTrek.Models.ViewModels
{
    public class AddBusinessViewModel
    {
        // ID for editing existing businesses
        public int? Id { get; set; }

        // Basic Information
        [Required(ErrorMessage = "Business name is required")]
        [StringLength(200, ErrorMessage = "Business name cannot exceed 200 characters")]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business category is required")]
        [Display(Name = "Business Category")]
        public string BusinessCategory { get; set; } = string.Empty;

        [Display(Name = "Sub Category")]
        public string? SubCategory { get; set; }

        [Required(ErrorMessage = "Town selection is required")]
        [Display(Name = "Town/Location")]
        public int TownId { get; set; }

        [Required(ErrorMessage = "Business description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [Display(Name = "Business Description")]
        public string BusinessDescription { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Short description cannot exceed 500 characters")]
        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        // Contact Information
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Primary Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Secondary Phone Number")]
        public string? PhoneNumber2 { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        // Location Information
        [Required(ErrorMessage = "Physical address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Physical Address")]
        public string PhysicalAddress { get; set; } = string.Empty;

        // Google Maps coordinates (populated via JavaScript)
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Operating Hours
        public List<BusinessHourViewModel> BusinessHours { get; set; } = new List<BusinessHourViewModel>();

        // File uploads
        [Display(Name = "Business Logo")]
        public IFormFile? BusinessLogo { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "Business Photos")]
        public List<IFormFile>? BusinessImages { get; set; }

        // Existing business images (for editing)
        public List<TownTrek.Models.BusinessImage>? ExistingBusinessImages { get; set; }

        // Services
        public List<string> Services { get; set; } = new List<string>();

        // Additional Information
        [StringLength(500, ErrorMessage = "Special offers cannot exceed 500 characters")]
        [Display(Name = "Special Offers")]
        public string? SpecialOffers { get; set; }

        [StringLength(500, ErrorMessage = "Additional notes cannot exceed 500 characters")]
        [Display(Name = "Additional Notes")]
        public string? AdditionalNotes { get; set; }

        // Category-specific fields (populated dynamically based on category)
        public Dictionary<string, object> CategorySpecificData { get; set; } = new Dictionary<string, object>();

        // Market-specific fields
        public string? MarketType { get; set; }
        public bool IsRecurringMarket { get; set; } = true;
        public string? RecurrencePattern { get; set; }
        public List<string> MarketDays { get; set; } = new List<string>();
        public string? MarketStartTime { get; set; }
        public string? MarketEndTime { get; set; }
        public int? EstimatedVendorCount { get; set; }
        public string? VendorTypes { get; set; }
        public string? ParkingInfo { get; set; }
        public decimal? EntryFee { get; set; }
        public bool HasRestrooms { get; set; } = false;
        public bool HasFoodVendors { get; set; } = false;
        public bool IsCoveredVenue { get; set; } = false;

        // Tour-specific fields
        public string? TourType { get; set; }
        public string? Duration { get; set; }
        public int? MaxGroupSize { get; set; }
        public int? MinGroupSize { get; set; }
        public int? MinAge { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? DepartureLocation { get; set; }
        public string? Itinerary { get; set; }
        public string? IncludedItems { get; set; }
        public string? ExcludedItems { get; set; }
        public string? RequiredEquipment { get; set; }
        public string? PricingInfo { get; set; }
        public bool RequiresBooking { get; set; } = true;
        public int? AdvanceBookingDays { get; set; }
        public bool IsWeatherDependent { get; set; } = false;
        public bool IsAccessible { get; set; } = false;
        public bool HasTransport { get; set; } = false;

        // Event-specific fields
        public string? EventType { get; set; }
        public DateTime? EventStartDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string? EventStartTime { get; set; }
        public string? EventEndTime { get; set; }
        public bool IsRecurringEvent { get; set; } = false;
        public string? EventRecurrencePattern { get; set; }
        public string? Venue { get; set; }
        public string? VenueAddress { get; set; }
        public int? MaxAttendees { get; set; }
        public string? TicketInfo { get; set; }
        public string? OrganizerContact { get; set; }
        public bool RequiresTickets { get; set; } = false;
        public bool IsFreeEvent { get; set; } = true;
        public string? EventProgram { get; set; }
        public bool HasParking { get; set; } = false;
        public bool IsOutdoorEvent { get; set; } = false;

        // Restaurant-specific fields
        public string? CuisineType { get; set; }
        public string? PriceRange { get; set; }
        public bool HasDelivery { get; set; } = false;
        public bool HasTakeaway { get; set; } = false;
        public bool AcceptsReservations { get; set; } = false;
        public int? SeatingCapacity { get; set; }
        public string? DietaryOptions { get; set; }
        public string? MenuUrl { get; set; }
        public bool HasKidsMenu { get; set; } = false;
        public bool HasOutdoorSeating { get; set; } = false;
        public bool ServesBreakfast { get; set; } = false;
        public bool ServesLunch { get; set; } = false;
        public bool ServesDinner { get; set; } = false;
        public bool ServesAlcohol { get; set; } = false;

        // Accommodation-specific fields
        public string? PropertyType { get; set; }
        public int? StarRating { get; set; }
        public int? RoomCount { get; set; }
        public int? MaxGuests { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public string? RoomTypes { get; set; }
        public string? Amenities { get; set; }
        public bool HasWiFi { get; set; } = false;
        public bool HasPool { get; set; } = false;
        public bool HasRestaurant { get; set; } = false;
        public bool IsPetFriendly { get; set; } = false;
        public bool HasBreakfast { get; set; } = false;
        public bool HasAirConditioning { get; set; } = false;
        public bool HasLaundry { get; set; } = false;
        public bool HasConferenceRoom { get; set; } = false;

        // Shop-specific fields
        public string? ShopType { get; set; }
        public string? ShopSize { get; set; }
        public string? BrandNames { get; set; }
        public string? Specialties { get; set; }
        public bool HasOnlineStore { get; set; } = false;
        public bool OffersLayaway { get; set; } = false;
        public bool HasFittingRoom { get; set; } = false;
        public bool OffersRepairs { get; set; } = false;
        public bool HasLoyaltyProgram { get; set; } = false;
        public bool AcceptsReturns { get; set; } = false;

        // Available options for dropdowns (populated by controller)
        public List<TownTrek.Models.Town> AvailableTowns { get; set; } = new List<TownTrek.Models.Town>();
        public List<BusinessCategoryOption> AvailableCategories { get; set; } = new List<BusinessCategoryOption>();
        public List<BusinessCategoryOption> AvailableSubCategories { get; set; } = new List<BusinessCategoryOption>();

        // User's subscription limits
        public SubscriptionLimits UserLimits { get; set; } = new SubscriptionLimits();
        public int CurrentBusinessCount { get; set; }
    }

    public class BusinessCategoryOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public List<BusinessCategoryOption> SubCategories { get; set; } = new List<BusinessCategoryOption>();
    }
}