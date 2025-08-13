using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // External Authentication
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string AuthenticationMethod { get; set; } = "Email"; // Email, Google, Facebook

        // Subscription Information (NO PAYMENT DATA)
        public string? CurrentSubscriptionTier { get; set; }
        public bool HasActiveSubscription { get; set; } = false;
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        // Trial Period Information
        public bool IsTrialUser { get; set; } = false;
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool TrialExpired { get; set; } = false;
        
        // Security: Additional trial tracking
        public long TrialStartTicks { get; set; } = 0; // UTC ticks for precision
        public long TrialEndTicks { get; set; } = 0; // UTC ticks for precision
        public string? TrialSecurityHash { get; set; } // Hash to prevent tampering
        public int TrialCheckCount { get; set; } = 0; // Track validation attempts
        public DateTime? LastTrialCheck { get; set; } // Last validation timestamp

        // Navigation properties
        public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}