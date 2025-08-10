using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    // Base class for category-specific business details
    public abstract class BusinessCategoryDetails
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Business Business { get; set; } = null!;
    }

    // Market-specific details
    public class MarketDetails : BusinessCategoryDetails
    {
        [StringLength(100)]
        public string? MarketType { get; set; } // Farmers Market, Craft Market, Flea Market

        public bool IsRecurring { get; set; } = true;
        
        [StringLength(100)]
        public string? RecurrencePattern { get; set; } // Weekly, Monthly, Seasonal

        // Market-specific operating days (different from regular business hours)
        public string? MarketDays { get; set; } // JSON: ["Saturday", "Sunday"]
        
        public TimeSpan? MarketStartTime { get; set; }
        public TimeSpan? MarketEndTime { get; set; }

        public int? EstimatedVendorCount { get; set; }
        
        [StringLength(500)]
        public string? VendorTypes { get; set; } // JSON array of vendor categories

        [StringLength(500)]
        public string? ParkingInfo { get; set; }
        
        [StringLength(500)]
        public string? SpecialEvents { get; set; } // Seasonal events, special occasions

        public decimal? EntryFee { get; set; }
        public bool HasRestrooms { get; set; } = false;
        public bool HasFoodVendors { get; set; } = false;
        public bool IsCoveredVenue { get; set; } = false;
    }

    // Tour-specific details
    public class TourDetails : BusinessCategoryDetails
    {
        [StringLength(100)]
        public string? TourType { get; set; } // Cultural, Adventure, Wildlife, Historical

        [StringLength(100)]
        public string? Duration { get; set; } // "2 hours", "Half day", "Full day", "Multi-day"

        public int? MaxGroupSize { get; set; }
        public int? MinGroupSize { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        [StringLength(100)]
        public string? DifficultyLevel { get; set; } // Easy, Moderate, Challenging

        [StringLength(500)]
        public string? DepartureLocation { get; set; }
        
        [StringLength(2000)]
        public string? Itinerary { get; set; }

        [StringLength(1000)]
        public string? IncludedItems { get; set; } // What's included in the tour

        [StringLength(1000)]
        public string? ExcludedItems { get; set; } // What participants need to bring

        [StringLength(500)]
        public string? RequiredEquipment { get; set; }

        [StringLength(500)]
        public string? PricingInfo { get; set; } // Since we don't handle bookings, just display info

        public bool RequiresBooking { get; set; } = true;
        public int? AdvanceBookingDays { get; set; } // How many days in advance to book

        // Tour availability
        public string? AvailableDays { get; set; } // JSON array of available days
        public string? AvailableSeasons { get; set; } // JSON array of seasons

        public bool IsWeatherDependent { get; set; } = false;
        public bool HasInsurance { get; set; } = false;
        public bool IsAccessible { get; set; } = false; // Wheelchair accessible
    }

    // Event-specific details
    public class EventDetails : BusinessCategoryDetails
    {
        [StringLength(100)]
        public string? EventType { get; set; } // Festival, Concert, Workshop, Conference

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public bool IsRecurring { get; set; } = false;
        
        [StringLength(100)]
        public string? RecurrencePattern { get; set; } // Weekly, Monthly, Annually

        public DateTime? RecurrenceEndDate { get; set; }

        [StringLength(500)]
        public string? Venue { get; set; }
        
        [StringLength(500)]
        public string? VenueAddress { get; set; }

        public int? MaxAttendees { get; set; }
        public int? ExpectedAttendance { get; set; }

        [StringLength(500)]
        public string? TicketInfo { get; set; } // Pricing and where to buy

        [StringLength(500)]
        public string? OrganizerContact { get; set; }

        public bool RequiresTickets { get; set; } = false;
        public bool IsFreeEvent { get; set; } = true;

        [StringLength(1000)]
        public string? EventProgram { get; set; } // Schedule of activities

        [StringLength(500)]
        public string? AgeRestrictions { get; set; }

        public bool HasParking { get; set; } = false;
        public bool HasRefreshments { get; set; } = false;
        public bool IsOutdoorEvent { get; set; } = false;
        public bool HasWeatherBackup { get; set; } = false;

        // Event status
        public string EventStatus { get; set; } = "Scheduled"; // Scheduled, Cancelled, Postponed, Completed
        
        [StringLength(500)]
        public string? StatusNotes { get; set; } // Reason for cancellation, new date, etc.
    }

    // Restaurant-specific details (enhanced)
    public class RestaurantDetails : BusinessCategoryDetails
    {
        [StringLength(100)]
        public string? CuisineType { get; set; }

        [StringLength(20)]
        public string? PriceRange { get; set; } // Budget, Moderate, Expensive, Fine Dining

        public bool HasDelivery { get; set; } = false;
        public bool HasTakeaway { get; set; } = false;
        public bool AcceptsReservations { get; set; } = false;

        public int? MaxGroupSize { get; set; }
        public int? SeatingCapacity { get; set; }

        [StringLength(500)]
        public string? DietaryOptions { get; set; } // JSON: Vegetarian, Vegan, Halal, etc.

        [StringLength(500)]
        public string? MenuUrl { get; set; } // Link to online menu or PDF

        public bool HasKidsMenu { get; set; } = false;
        public bool HasOutdoorSeating { get; set; } = false;
        public bool HasPrivateDining { get; set; } = false;
        public bool HasLiveMusic { get; set; } = false;

        [StringLength(500)]
        public string? SpecialFeatures { get; set; } // Wine cellar, chef's table, etc.

        // Operating specifics
        public bool ServesBreakfast { get; set; } = false;
        public bool ServesLunch { get; set; } = false;
        public bool ServesDinner { get; set; } = false;
        public bool ServesAlcohol { get; set; } = false;

        public TimeSpan? BreakfastStart { get; set; }
        public TimeSpan? BreakfastEnd { get; set; }
        public TimeSpan? LunchStart { get; set; }
        public TimeSpan? LunchEnd { get; set; }
        public TimeSpan? DinnerStart { get; set; }
        public TimeSpan? DinnerEnd { get; set; }
    }

    // Accommodation-specific details
    public class AccommodationDetails : BusinessCategoryDetails
    {
        [StringLength(100)]
        public string? PropertyType { get; set; } // Hotel, Guesthouse, B&B, Self-catering

        public int? StarRating { get; set; }
        public int? RoomCount { get; set; }
        public int? MaxGuests { get; set; }

        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }

        [StringLength(1000)]
        public string? Amenities { get; set; } // JSON array

        [StringLength(500)]
        public string? PricingInfo { get; set; }

        public bool HasWiFi { get; set; } = false;
        public bool HasParking { get; set; } = false;
        public bool HasPool { get; set; } = false;
        public bool HasRestaurant { get; set; } = false;
        public bool HasGym { get; set; } = false;
        public bool HasSpa { get; set; } = false;
        public bool IsPetFriendly { get; set; } = false;
        public bool HasAirConditioning { get; set; } = false;
        public bool HasBreakfast { get; set; } = false;
        public bool HasLaundry { get; set; } = false;
        public bool HasConferenceRoom { get; set; } = false;

        [StringLength(500)]
        public string? RoomTypes { get; set; } // JSON array of room types

        public bool RequiresDeposit { get; set; } = false;
        
        [StringLength(500)]
        public string? CancellationPolicy { get; set; }
    }

    // Business notification/alert system
    public class BusinessAlert
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        
        [StringLength(100)]
        public string AlertType { get; set; } = string.Empty; // StatusChange, SpecialOffer, Event, Emergency

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsPushNotification { get; set; } = false; // For mobile app notifications

        [StringLength(50)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        public virtual Business Business { get; set; } = null!;
    }

    // Special operating hours for events, markets, etc.
    public class SpecialOperatingHours
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public bool IsClosed { get; set; } = false;

        [StringLength(200)]
        public string? Reason { get; set; } // Holiday, Special Event, Maintenance

        [StringLength(500)]
        public string? Notes { get; set; }

        public virtual Business Business { get; set; } = null!;
    }
}