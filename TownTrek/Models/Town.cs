using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class Town
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? Population { get; set; }

        [StringLength(1000)]
        public string? Landmarks { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();
    }
}