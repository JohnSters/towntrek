using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class AdminMessage
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        public int TopicId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed
        
        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        
        [MaxLength(450)]
        public string? ResolvedBy { get; set; }
        
        [MaxLength(2000)]
        public string? AdminResponse { get; set; }
        
        public DateTime? ResponseAt { get; set; }
        
        [MaxLength(450)]
        public string? ResponseBy { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual AdminMessageTopic Topic { get; set; } = null!;
        public virtual ApplicationUser? ResolvedByUser { get; set; }
        public virtual ApplicationUser? ResponseByUser { get; set; }
    }
}