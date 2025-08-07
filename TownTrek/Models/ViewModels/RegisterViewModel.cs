using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; } = "business"; // "business" or "community"

        [Required]
        [StringLength(50)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Business Owner specific fields
        [Display(Name = "Selected Plan")]
        public int? SelectedPlan { get; set; }

        [Required]
        [Display(Name = "Accept Terms")]
        public bool AcceptTerms { get; set; }

        [Display(Name = "Marketing Emails")]
        public bool MarketingEmails { get; set; }

        // Helper properties
        public string FirstName => FullName.Split(' ').FirstOrDefault() ?? "";
        public string LastName => FullName.Contains(' ') ? FullName.Substring(FullName.IndexOf(' ') + 1) : "";
        public bool IsBusinessOwner => AccountType == "business";
    }
}