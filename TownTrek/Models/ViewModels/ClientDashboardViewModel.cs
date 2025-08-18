using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Models.ViewModels
{
    public class ClientDashboardViewModel
    {
        public ApplicationUser? User { get; set; }
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int PendingBusinesses { get; set; }
        public List<Business> RecentBusinesses { get; set; } = new();
        public int TotalViews { get; set; }
        public SubscriptionTier? SubscriptionTier { get; set; }
        public SubscriptionLimits? UserLimits { get; set; }
        public string? PaymentStatus { get; set; }
        public bool CanAddBusiness { get; set; }
        public bool HasAnalyticsAccess { get; set; }
        public bool HasAdvancedAnalyticsAccess { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasDedicatedSupport { get; set; }
        
        // Trial-specific properties
        public bool IsTrialUser { get; set; }
        public int TrialDaysRemaining { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsTrialExpired { get; set; }
        public string? TrialStatusMessage { get; set; }
    }
}