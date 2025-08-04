using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models;

public class Town
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
    
    public int Population { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}