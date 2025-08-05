using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class AddBusinessViewModel
    {
        [Required(ErrorMessage = "Business name is required")]
        [StringLength(100, ErrorMessage = "Business name cannot exceed 100 characters")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business category is required")]
        public string BusinessCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Town selection is required")]
        public int TownId { get; set; }

        [Required(ErrorMessage = "Business description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string BusinessDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? EmailAddress { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? Website { get; set; }

        [Required(ErrorMessage = "Physical address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string PhysicalAddress { get; set; } = string.Empty;

        // Operating Hours
        public List<string> OperatingDays { get; set; } = new List<string>();
        public string? MondayOpen { get; set; }
        public string? MondayClose { get; set; }
        public string? TuesdayOpen { get; set; }
        public string? TuesdayClose { get; set; }
        public string? WednesdayOpen { get; set; }
        public string? WednesdayClose { get; set; }
        public string? ThursdayOpen { get; set; }
        public string? ThursdayClose { get; set; }
        public string? FridayOpen { get; set; }
        public string? FridayClose { get; set; }
        public string? SaturdayOpen { get; set; }
        public string? SaturdayClose { get; set; }
        public string? SundayOpen { get; set; }
        public string? SundayClose { get; set; }

        // File uploads (these would be handled separately in a real application)
        public IFormFile? BusinessLogo { get; set; }
        public List<IFormFile>? BusinessImages { get; set; }

        // Additional services
        public List<string> Services { get; set; } = new List<string>();

        [StringLength(500, ErrorMessage = "Special offers cannot exceed 500 characters")]
        public string? SpecialOffers { get; set; }

        [StringLength(500, ErrorMessage = "Additional notes cannot exceed 500 characters")]
        public string? AdditionalNotes { get; set; }
    }
}