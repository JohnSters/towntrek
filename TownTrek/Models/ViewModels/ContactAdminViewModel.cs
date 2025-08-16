using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    public class ContactAdminViewModel
    {
        [Required(ErrorMessage = "Please select a topic")]
        public int TopicId { get; set; }
        
        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Message is required")]
        [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
        public string Message { get; set; } = string.Empty;
        
        // Available topics for dropdown
        public List<AdminMessageTopic> AvailableTopics { get; set; } = new();
        
        // User's existing messages for reference
        public List<AdminMessage> UserMessages { get; set; } = new();
        
        // Selected topic details (populated via AJAX)
        public AdminMessageTopic? SelectedTopic { get; set; }
    }
}