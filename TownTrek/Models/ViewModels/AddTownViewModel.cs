using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    public class AddTownViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Town name is required")]
        [StringLength(100, ErrorMessage = "Town name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        public string? PostalCode { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Population must be a positive number")]
        public int? Population { get; set; }

        [StringLength(1000, ErrorMessage = "Landmarks cannot exceed 1000 characters")]
        public string? Landmarks { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }
    }
}