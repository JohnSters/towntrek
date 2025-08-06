using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models
{
    public class SubscriptionTier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Basic, Standard, Premium

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty; // "Basic Plan", "Standard Plan"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999.99)]
        public decimal MonthlyPrice { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } // For display ordering
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedById { get; set; } // Admin who made changes

        // Navigation properties
        public virtual ICollection<SubscriptionTierLimit> Limits { get; set; } = new List<SubscriptionTierLimit>();
        public virtual ICollection<SubscriptionTierFeature> Features { get; set; } = new List<SubscriptionTierFeature>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ApplicationUser? UpdatedBy { get; set; }
    }

    public class SubscriptionTierLimit
    {
        public int Id { get; set; }
        public int SubscriptionTierId { get; set; }

        [Required]
        [StringLength(50)]
        public string LimitType { get; set; } = string.Empty; // "MaxBusinesses", "MaxImages", "MaxPDFs"

        public int LimitValue { get; set; } // -1 for unlimited
        public string? Description { get; set; }

        public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
    }

    public class SubscriptionTierFeature
    {
        public int Id { get; set; }
        public int SubscriptionTierId { get; set; }

        [Required]
        [StringLength(50)]
        public string FeatureKey { get; set; } = string.Empty; // "Analytics", "PrioritySupport", "FeaturedPlacement"

        public bool IsEnabled { get; set; } = true;
        public string? FeatureName { get; set; }
        public string? Description { get; set; }

        public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
    }

    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int SubscriptionTierId { get; set; } // Reference to SubscriptionTier
        public decimal MonthlyPrice { get; set; } // Price at time of subscription (for historical tracking)
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // PayFast Integration - SAFE DATA ONLY
        public string? PayFastToken { get; set; }
        public string? PayFastPaymentId { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string PaymentStatus { get; set; } = "Pending";

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
    }

    public class PriceChangeHistory
    {
        public int Id { get; set; }
        public int SubscriptionTierId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string ChangedById { get; set; } = string.Empty;

        public string? ChangeReason { get; set; }
        public DateTime? EffectiveDate { get; set; } // When the change takes effect
        public bool NotificationSent { get; set; } = false;

        public virtual SubscriptionTier SubscriptionTier { get; set; } = null!;
        public virtual ApplicationUser ChangedBy { get; set; } = null!;
    }
}