using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public enum BusinessFormType
    {
        None = 0,
        Restaurant = 1,
        Market = 2,
        Tour = 3,
        Event = 4,
        Accommodation = 5,
        Shop = 6
    }

    public class BusinessCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty; // e.g., restaurants-food

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Display name

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? IconClass { get; set; }

        public bool IsActive { get; set; } = true;

        public BusinessFormType FormType { get; set; } = BusinessFormType.None;

        public virtual ICollection<BusinessSubCategory> SubCategories { get; set; } = new List<BusinessSubCategory>();
    }

    public class BusinessSubCategory
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty; // e.g., cafe, bakery

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Display name

        public bool IsActive { get; set; } = true;

        public virtual BusinessCategory Category { get; set; } = null!;
    }

    public class ServiceDefinition
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty; // e.g., wifi

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., Free WiFi

        public bool IsActive { get; set; } = true;
    }
}


