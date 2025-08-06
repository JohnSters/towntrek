using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class AddBusinessViewModel
    {
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

        // Available options for dropdowns (populated by controller)
        public List<Town> AvailableTowns { get; set; } = new List<Town>();
        public List<BusinessCategoryOption> AvailableCategories { get; set; } = new List<BusinessCategoryOption>();
        public List<BusinessCategoryOption> AvailableSubCategories { get; set; } = new List<BusinessCategoryOption>();

        // User's subscription limits
        public SubscriptionLimits UserLimits { get; set; } = new SubscriptionLimits();
        public int CurrentBusinessCount { get; set; }
    }

    public class BusinessHourViewModel
    {
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.
        public string DayName { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = false;
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public bool IsSpecialHours { get; set; } = false;
        public string? SpecialHoursNote { get; set; }
    }

    public class BusinessCategoryOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public List<BusinessCategoryOption> SubCategories { get; set; } = new List<BusinessCategoryOption>();
    }

    public class SubscriptionLimits
    {
        public int MaxBusinesses { get; set; }
        public int MaxImages { get; set; }
        public int MaxPDFs { get; set; }
        public bool HasBasicSupport { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasDedicatedSupport { get; set; }
        public bool HasBasicAnalytics { get; set; }
        public bool HasAdvancedAnalytics { get; set; }
        public bool HasFeaturedPlacement { get; set; }
        public bool HasPDFUploads { get; set; }
    }
}