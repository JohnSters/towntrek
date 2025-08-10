using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class MediaGalleryOverviewViewModel
    {
        public List<Business> Businesses { get; set; } = new();
        public List<BusinessImage> AllImages { get; set; } = new();
        public Dictionary<int, int> BusinessImageCounts { get; set; } = new();
        
        // Statistics
        public int TotalImages { get; set; }
        public int TotalLogos { get; set; }
        public int TotalGalleryImages { get; set; }
        
        // Upload settings
        public bool CanUpload { get; set; }
        public int MaxFileSizeBytes { get; set; }
        public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
        
        // Filtering
        public int? SelectedBusinessId { get; set; }
        public string? SelectedImageType { get; set; }
    }
}