using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class ClientSubscriptionViewModel
    {
        public Subscription? CurrentSubscription { get; set; }
        public List<SubscriptionTier> AvailableTiers { get; set; } = new();
        public int BusinessCount { get; set; }
    }
}