using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class Business
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Owner of the business

        [Required]
        public int TownId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SubCategory { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PhoneNumber2 { get; set; }

        [StringLength(256)]
        public string? EmailAddress { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [Required]
        [StringLength(500)]
        public string PhysicalAddress { get; set; } = string.Empty;

        // Location coordinates for Google Maps integration
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // File paths
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }

        // Status and verification
        public string Status { get; set; } = "Pending"; // Pending, Active, Inactive, Suspended
        public bool IsFeatured { get; set; } = false;
        public bool IsVerified { get; set; } = false;

        // Analytics
        public decimal? Rating { get; set; }
        public int TotalReviews { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Town Town { get; set; } = null!;
        public virtual ApplicationUser? ApprovedByUser { get; set; }
        public virtual ICollection<BusinessHour> BusinessHours { get; set; } = new List<BusinessHour>();
        public virtual ICollection<BusinessImage> BusinessImages { get; set; } = new List<BusinessImage>();
        public virtual ICollection<BusinessContact> BusinessContacts { get; set; } = new List<BusinessContact>();
        public virtual ICollection<BusinessService> BusinessServices { get; set; } = new List<BusinessService>();
    }

    public class BusinessHour
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsOpen { get; set; } = true;
        public bool IsSpecialHours { get; set; } = false;
        public string? SpecialHoursNote { get; set; }

        public virtual Business Business { get; set; } = null!;
    }

    public class BusinessImage
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string ImageType { get; set; } = string.Empty; // Logo, Cover, Gallery, Menu
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public virtual Business Business { get; set; } = null!;
        public virtual ApplicationUser? ApprovedByUser { get; set; }
    }

    public class BusinessContact
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string ContactType { get; set; } = string.Empty; // Phone, Email, WhatsApp, Facebook, Instagram
        public string ContactValue { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Business Business { get; set; } = null!;
    }

    public class BusinessService
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string ServiceType { get; set; } = string.Empty; // delivery, takeaway, wheelchair, parking, wifi, cards
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Business Business { get; set; } = null!;
    }
}