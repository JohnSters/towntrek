using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Profile Picture URL")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? ProfilePictureUrl { get; set; }

        // Read-only properties for display
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string AuthenticationMethod { get; set; } = string.Empty;
    }
}