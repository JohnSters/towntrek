namespace TownTrek.Models
{
    public class SubscriptionLimits
    {
        public int MaxBusinesses { get; set; }
        public int MaxImages { get; set; }
        public int MaxPDFs { get; set; }
        public bool HasBasicSupport { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasDedicatedSupport { get; set; }
        public bool HasBasicAnalytics { get; set; }
        public bool HasAdvancedAnalytics { get; set; }
        public bool HasFeaturedPlacement { get; set; }
        public bool HasPDFUploads { get; set; }
        public int CurrentBusinessCount { get; set; }
        public int CurrentImageCount { get; set; }
        public int CurrentPDFCount { get; set; }
    }
}