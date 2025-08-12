using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class ImageUploadResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public BusinessImage? Image { get; set; }
        public string? FileName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class ImageValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ImageUploadViewModel
    {
        public int BusinessId { get; set; }
        public string ImageType { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

    public class DeleteImageRequest
    {
        public int ImageId { get; set; }
    }

    public class BusinessImageViewModel
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string ImageType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public DateTime UploadedAt { get; set; }
        public long FileSizeKB => FileSize / 1024;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    public class ImageGalleryViewModel
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public BusinessImage? Logo { get; set; }
        public List<BusinessImage> GalleryImages { get; set; } = new List<BusinessImage>();
        public List<BusinessImage> AllImages { get; set; } = new List<BusinessImage>();
        public bool CanUpload { get; set; } = true;
        public int MaxImagesAllowed { get; set; } = 10;
        public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024; // 2MB
        public string[] AllowedFileTypes { get; set; } = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    }
}