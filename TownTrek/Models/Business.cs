using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class Business
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int TownId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EmailAddress { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [Required]
        [StringLength(500)]
        public string PhysicalAddress { get; set; } = string.Empty;

        // Operating Hours (stored as JSON or separate table)
        public string? OperatingHours { get; set; }

        // Services (stored as JSON)
        public string? Services { get; set; }

        [StringLength(500)]
        public string? SpecialOffers { get; set; }

        [StringLength(500)]
        public string? AdditionalNotes { get; set; }

        public string? LogoPath { get; set; }
        public string? ImagesPath { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Town Town { get; set; } = null!;
        public string UserId { get; set; } = string.Empty; // Owner of the business
    }
}