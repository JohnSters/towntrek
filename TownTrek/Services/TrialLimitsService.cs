using TownTrek.Models;

namespace TownTrek.Services
{
    public static class TrialLimitsService
    {
        public static SubscriptionLimits GetTrialLimits()
        {
            return new SubscriptionLimits
            {
                MaxBusinesses = 1,
                MaxImages = 5,
                MaxPDFs = 0, // No PDF uploads for trial
                HasBasicSupport = true,
                HasPrioritySupport = false,
                HasDedicatedSupport = false,
                HasBasicAnalytics = false, // No analytics for trial
                HasAdvancedAnalytics = false,
                HasFeaturedPlacement = false,
                HasPDFUploads = false
            };
        }

        public static bool IsFeatureAllowed(string feature, string userRole)
        {
            if (userRole != "Client-Trial") return true;

            return feature.ToLower() switch
            {
                "analytics" => false,
                "advanced_analytics" => false,
                "featured_placement" => false,
                "pdf_uploads" => false,
                "priority_support" => false,
                "dedicated_support" => false,
                _ => true
            };
        }
    }
}