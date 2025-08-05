using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    public class SubscriptionTierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tier name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display name is required")]
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Monthly price is required")]
        [Range(0, 999999.99, ErrorMessage = "Price must be between R0 and R999,999.99")]
        [Display(Name = "Monthly Price (R)")]
        public decimal MonthlyPrice { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Range(1, 100, ErrorMessage = "Sort order must be between 1 and 100")]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        // Limits
        [Range(1, 100, ErrorMessage = "Max businesses must be between 1 and 100, or -1 for unlimited")]
        [Display(Name = "Max Businesses (-1 for unlimited)")]
        public int MaxBusinesses { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "Max images must be between 1 and 1000, or -1 for unlimited")]
        [Display(Name = "Max Images per Business (-1 for unlimited)")]
        public int MaxImages { get; set; } = 5;

        [Range(0, 100, ErrorMessage = "Max PDFs must be between 0 and 100, or -1 for unlimited")]
        [Display(Name = "Max PDFs per Business (-1 for unlimited)")]
        public int MaxPDFs { get; set; } = 0;

        // Features
        [Display(Name = "Basic Support")]
        public bool HasBasicSupport { get; set; } = true;

        [Display(Name = "Priority Support")]
        public bool HasPrioritySupport { get; set; } = false;

        [Display(Name = "Dedicated Support")]
        public bool HasDedicatedSupport { get; set; } = false;

        [Display(Name = "Basic Analytics")]
        public bool HasBasicAnalytics { get; set; } = false;

        [Display(Name = "Advanced Analytics")]
        public bool HasAdvancedAnalytics { get; set; } = false;

        [Display(Name = "Featured Placement")]
        public bool HasFeaturedPlacement { get; set; } = false;

        [Display(Name = "PDF Uploads")]
        public bool HasPDFUploads { get; set; } = false;

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByName { get; set; }
        public int ActiveSubscriptionsCount { get; set; }
    }

    public class PriceChangeViewModel
    {
        public int SubscriptionTierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }

        [Required(ErrorMessage = "New price is required")]
        [Range(0, 999999.99, ErrorMessage = "Price must be between R0 and R999,999.99")]
        [Display(Name = "New Monthly Price (R)")]
        public decimal NewPrice { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Reason for Change")]
        public string? ChangeReason { get; set; }

        [Display(Name = "Effective Date")]
        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Display(Name = "Send Notification to Customers")]
        public bool SendNotification { get; set; } = true;

        public int AffectedCustomersCount { get; set; }
    }

    public class SubscriptionTierListViewModel
    {
        public List<SubscriptionTierViewModel> Tiers { get; set; } = new();
        public int TotalActiveSubscriptions { get; set; }
        public decimal TotalMonthlyRevenue { get; set; }
        public List<PriceChangeHistory> RecentPriceChanges { get; set; } = new();
    }
}