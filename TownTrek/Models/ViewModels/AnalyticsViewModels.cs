using System.ComponentModel.DataAnnotations;

namespace TownTrek.Models.ViewModels
{
    public class UsageTrackingRequest
    {
        public string FeatureName { get; set; } = string.Empty;
        public string InteractionType { get; set; } = string.Empty;
        public double Duration { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
