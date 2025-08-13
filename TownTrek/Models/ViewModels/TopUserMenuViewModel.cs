using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class TopUserMenuViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string Initials { get; set; } = "U";
        public string DisplayName { get; set; } = "User";
        public string DisplayRole { get; set; } = "Member";
        public SubscriptionTier? SubscriptionTier { get; set; }
        public bool IsMember { get; set; } // Members are authenticated users without subscriptions
        public bool IsBusinessOwner { get; set; } // Business owners have active subscriptions or trial users
        public bool IsTrialUser { get; set; } // Trial users have limited-time access
    }
}