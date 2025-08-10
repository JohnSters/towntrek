using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class TopUserMenuViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string Initials { get; set; } = "U";
        public string DisplayName { get; set; } = "User";
        public string DisplayRole { get; set; } = "Free Tier";
        public SubscriptionTier? SubscriptionTier { get; set; }
    }
}


