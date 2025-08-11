using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class ImageService : IImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageService> _logger;
        
        // Configuration constants
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImageService(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ImageService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<ImageUploadResult> UploadBusinessImageAsync(int businessId, IFormFile imageFile, string imageType, int displayOrder = 0)
        {
            try
            {
                // Validate the image file
                var validationResult = ValidateImageFile(imageFile);
                if (!validationResult.IsValid)
                {
                    return new ImageUploadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = string.Join(", ", validationResult.Errors)
                    };
                }

                // Verify business exists
                var businessExists = await _context.Businesses.AnyAsync(b => b.Id == businessId);
                if (!businessExists)
                {
                    return new ImageUploadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Business not found"
                    };
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{businessId}_{imageType}_{Guid.NewGuid()}{fileExtension}";
                
                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "businesses");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                // Save the original image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Generate thumbnail
                var thumbnailUrl = await GenerateThumbnailAsync(filePath);

                // Create database record
                var businessImage = new BusinessImage
                {
                    BusinessId = businessId,
                    ImageType = imageType,
                    FileName = fileName,
                    OriginalFileName = imageFile.FileName,
                    FileSize = imageFile.Length,
                    ContentType = imageFile.ContentType,
                    ImageUrl = $"/uploads/businesses/{fileName}",
                    ThumbnailUrl = thumbnailUrl,
                    AltText = $"{imageType} for business",
                    DisplayOrder = displayOrder,
                    IsActive = true,
                    IsApproved = true, // Auto-approve for now, can be changed based on business rules
                    UploadedAt = DateTime.UtcNow
                };

                _context.BusinessImages.Add(businessImage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully uploaded image {FileName} for business {BusinessId}", fileName, businessId);

                return new ImageUploadResult
                {
                    IsSuccess = true,
                    Image = businessImage,
                    FileName = fileName,
                    ImageUrl = businessImage.ImageUrl,
                    ThumbnailUrl = thumbnailUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for business {BusinessId}", businessId);
                return new ImageUploadResult
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while uploading the image"
                };
            }
        }

        public async Task<List<ImageUploadResult>> UploadBusinessImagesAsync(int businessId, IEnumerable<IFormFile> imageFiles, string imageType)
        {
            var results = new List<ImageUploadResult>();
            var displayOrder = 0;

            foreach (var imageFile in imageFiles)
            {
                var result = await UploadBusinessImageAsync(businessId, imageFile, imageType, displayOrder++);
                results.Add(result);
            }

            return results;
        }

        public async Task<List<BusinessImage>> GetBusinessImagesAsync(int businessId)
        {
            return await _context.BusinessImages
                .Where(bi => bi.BusinessId == businessId && bi.IsActive)
                .OrderBy(bi => bi.DisplayOrder)
                .ThenBy(bi => bi.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<BusinessImage>> GetBusinessImagesByTypeAsync(int businessId, string imageType)
        {
            return await _context.BusinessImages
                .Where(bi => bi.BusinessId == businessId && bi.ImageType == imageType && bi.IsActive)
                .OrderBy(bi => bi.DisplayOrder)
                .ThenBy(bi => bi.UploadedAt)
                .ToListAsync();
        }

        public async Task<BusinessImage?> GetBusinessLogoAsync(int businessId)
        {
            return await _context.BusinessImages
                .Where(bi => bi.BusinessId == businessId && bi.ImageType == "Logo" && bi.IsActive)
                .OrderBy(bi => bi.DisplayOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<BusinessImage?> GetImageByIdAsync(int imageId)
        {
            return await _context.BusinessImages.FirstOrDefaultAsync(i => i.Id == imageId && i.IsActive);
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            try
            {
                var image = await _context.BusinessImages.FindAsync(imageId);
                if (image == null)
                    return false;

                image.IsActive = false;
                image.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Soft deleted image {ImageId}", imageId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId}", imageId);
                return false;
            }
        }

        public async Task<bool> UpdateImageAsync(int imageId, string? altText = null, int? displayOrder = null)
        {
            try
            {
                var image = await _context.BusinessImages.FindAsync(imageId);
                if (image == null)
                    return false;

                if (altText != null)
                    image.AltText = altText;

                if (displayOrder.HasValue)
                    image.DisplayOrder = displayOrder.Value;

                image.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image {ImageId}", imageId);
                return false;
            }
        }

        public async Task<bool> ApproveImageAsync(int imageId, string approvedBy)
        {
            try
            {
                var image = await _context.BusinessImages.FindAsync(imageId);
                if (image == null)
                    return false;

                image.IsApproved = true;
                image.ApprovedAt = DateTime.UtcNow;
                image.ApprovedBy = approvedBy;
                image.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving image {ImageId}", imageId);
                return false;
            }
        }

        public ImageValidationResult ValidateImageFile(IFormFile imageFile)
        {
            var result = new ImageValidationResult { IsValid = true };

            if (imageFile == null || imageFile.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("No file was uploaded");
                return result;
            }

            // Check file size
            if (imageFile.Length > MaxFileSizeBytes)
            {
                result.IsValid = false;
                result.Errors.Add($"File size must be less than {MaxFileSizeBytes / (1024 * 1024)}MB");
            }

            // Check content type
            if (!AllowedContentTypes.Contains(imageFile.ContentType.ToLower()))
            {
                result.IsValid = false;
                result.Errors.Add($"File type '{imageFile.ContentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");
            }

            // Check file extension
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.Errors.Add($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", AllowedExtensions)}");
            }

            // Additional validation: try to load the image to ensure it's valid
            try
            {
                using var stream = imageFile.OpenReadStream();
                using var image = Image.Load(stream);
                // If we get here, the image is valid
            }
            catch
            {
                result.IsValid = false;
                result.Errors.Add("The uploaded file is not a valid image");
            }

            return result;
        }

        public async Task<string?> GenerateThumbnailAsync(string imagePath, int width = ThumbnailWidth, int height = ThumbnailHeight)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(imagePath);
                var extension = Path.GetExtension(imagePath);
                var directory = Path.GetDirectoryName(imagePath);
                
                var thumbnailFileName = $"{fileName}_thumb{extension}";
                var thumbnailPath = Path.Combine(directory!, thumbnailFileName);

                using var image = await Image.LoadAsync(imagePath);
                
                // Calculate aspect ratio preserving resize
                var aspectRatio = (float)image.Width / image.Height;
                int newWidth, newHeight;
                
                if (aspectRatio > 1) // Landscape
                {
                    newWidth = width;
                    newHeight = (int)(width / aspectRatio);
                }
                else // Portrait or square
                {
                    newHeight = height;
                    newWidth = (int)(height * aspectRatio);
                }

                image.Mutate(x => x.Resize(newWidth, newHeight));
                await image.SaveAsync(thumbnailPath);

                return $"/uploads/businesses/{thumbnailFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail for {ImagePath}", imagePath);
                return null;
            }
        }

        public string GetImagePath(string fileName)
        {
            return Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "businesses", fileName);
        }

        public string GetImageUrl(string fileName)
        {
            return $"/uploads/businesses/{fileName}";
        }

        public Task<bool> DeleteImageFileAsync(string fileName)
        {
            try
            {
                var filePath = GetImagePath(fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Also delete thumbnail if it exists
                var thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + "_thumb" + Path.GetExtension(fileName);
                var thumbnailPath = GetImagePath(thumbnailFileName);
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image file {FileName}", fileName);
                return Task.FromResult(false);
            }
        }
    }
}