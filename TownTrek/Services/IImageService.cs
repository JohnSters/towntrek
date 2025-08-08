using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Uploads and saves a business image
        /// </summary>
        Task<ImageUploadResult> UploadBusinessImageAsync(int businessId, IFormFile imageFile, string imageType, int displayOrder = 0);

        /// <summary>
        /// Uploads multiple business images
        /// </summary>
        Task<List<ImageUploadResult>> UploadBusinessImagesAsync(int businessId, IEnumerable<IFormFile> imageFiles, string imageType);

        /// <summary>
        /// Gets all active images for a business
        /// </summary>
        Task<List<BusinessImage>> GetBusinessImagesAsync(int businessId);

        /// <summary>
        /// Gets images by type for a business
        /// </summary>
        Task<List<BusinessImage>> GetBusinessImagesByTypeAsync(int businessId, string imageType);

        /// <summary>
        /// Gets the primary logo for a business
        /// </summary>
        Task<BusinessImage?> GetBusinessLogoAsync(int businessId);

        /// <summary>
        /// Soft deletes an image (sets IsActive to false)
        /// </summary>
        Task<bool> DeleteImageAsync(int imageId);

        /// <summary>
        /// Updates image metadata
        /// </summary>
        Task<bool> UpdateImageAsync(int imageId, string? altText = null, int? displayOrder = null);

        /// <summary>
        /// Approves an image (admin function)
        /// </summary>
        Task<bool> ApproveImageAsync(int imageId, string approvedBy);

        /// <summary>
        /// Validates image file before upload
        /// </summary>
        ImageValidationResult ValidateImageFile(IFormFile imageFile);

        /// <summary>
        /// Generates thumbnail for an image
        /// </summary>
        Task<string?> GenerateThumbnailAsync(string imagePath, int width = 300, int height = 300);

        /// <summary>
        /// Gets the full file path for an image
        /// </summary>
        string GetImagePath(string fileName);

        /// <summary>
        /// Gets the public URL for an image
        /// </summary>
        string GetImageUrl(string fileName);

        /// <summary>
        /// Physically deletes image files from disk
        /// </summary>
        Task<bool> DeleteImageFileAsync(string fileName);
    }
}