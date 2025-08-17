using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class AdminMessageTopic
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string IconClass { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        [MaxLength(50)]
        public string ColorClass { get; set; } = "info"; // success, info, warning, danger
        
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<AdminMessage> Messages { get; set; } = new List<AdminMessage>();
    }
}