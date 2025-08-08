using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Business
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly IImageService _imageService;
        private readonly IBusinessService _businessService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(
            IImageService imageService,
            IBusinessService businessService,
            ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _businessService = businessService;
            _logger = logger;
        }

        /// <summary>
        /// Display image gallery for a business
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Gallery(int businessId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Verify user owns this business or is admin
            if (!await CanUserAccessBusiness(businessId, userId))
            {
                return Forbid();
            }

            var images = await _imageService.GetBusinessImagesAsync(businessId);
            var logo = await _imageService.GetBusinessLogoAsync(businessId);
            var galleryImages = await _imageService.GetBusinessImagesByTypeAsync(businessId, "Gallery");

            var model = new ImageGalleryViewModel
            {
                BusinessId = businessId,
                Logo = logo,
                GalleryImages = galleryImages,
                AllImages = images,
                CanUpload = true,
                MaxImagesAllowed = 10,
                MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
                AllowedFileTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" }
            };

            return View(model);
        }

        /// <summary>
        /// Upload single image via AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] ImageUploadViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Verify user owns this business or is admin
            if (!await CanUserAccessBusiness(model.BusinessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            if (model.ImageFile == null)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            var result = await _imageService.UploadBusinessImageAsync(
                model.BusinessId, 
                model.ImageFile, 
                model.ImageType, 
                model.DisplayOrder);

            if (result.IsSuccess)
            {
                return Json(new
                {
                    success = true,
                    message = "Image uploaded successfully",
                    data = new
                    {
                        id = result.Image!.Id,
                        fileName = result.FileName,
                        imageUrl = result.ImageUrl,
                        thumbnailUrl = result.ThumbnailUrl,
                        originalFileName = result.Image.OriginalFileName
                    }
                });
            }

            return Json(new { success = false, message = result.ErrorMessage });
        }

        /// <summary>
        /// Upload multiple images via AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadMultiple([FromForm] int businessId, [FromForm] string imageType, [FromForm] List<IFormFile> files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Verify user owns this business or is admin
            if (!await CanUserAccessBusiness(businessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            if (files == null || !files.Any())
            {
                return Json(new { success = false, message = "No files uploaded" });
            }

            var results = await _imageService.UploadBusinessImagesAsync(businessId, files, imageType);
            var successfulUploads = results.Where(r => r.IsSuccess).ToList();
            var failedUploads = results.Where(r => !r.IsSuccess).ToList();

            return Json(new
            {
                success = successfulUploads.Any(),
                message = $"{successfulUploads.Count} images uploaded successfully" + 
                         (failedUploads.Any() ? $", {failedUploads.Count} failed" : ""),
                data = successfulUploads.Select(r => new
                {
                    id = r.Image!.Id,
                    fileName = r.FileName,
                    imageUrl = r.ImageUrl,
                    thumbnailUrl = r.ThumbnailUrl,
                    originalFileName = r.Image.OriginalFileName
                }),
                errors = failedUploads.Select(r => r.ErrorMessage)
            });
        }

        /// <summary>
        /// Delete image via AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int imageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Get the image to verify ownership
            var images = await _imageService.GetBusinessImagesAsync(0); // This needs to be improved
            var image = images.FirstOrDefault(i => i.Id == imageId);
            
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found" });
            }

            // Verify user owns this business or is admin
            if (!await CanUserAccessBusiness(image.BusinessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            var success = await _imageService.DeleteImageAsync(imageId);
            
            if (success)
            {
                return Json(new { success = true, message = "Image deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete image" });
        }

        /// <summary>
        /// Update image metadata via AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateMetadata(int imageId, string? altText, int? displayOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            // Similar ownership verification as Delete method
            var images = await _imageService.GetBusinessImagesAsync(0);
            var image = images.FirstOrDefault(i => i.Id == imageId);
            
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found" });
            }

            if (!await CanUserAccessBusiness(image.BusinessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            var success = await _imageService.UpdateImageAsync(imageId, altText, displayOrder);
            
            if (success)
            {
                return Json(new { success = true, message = "Image updated successfully" });
            }

            return Json(new { success = false, message = "Failed to update image" });
        }

        /// <summary>
        /// Get images for a business via AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetImages(int businessId, string? imageType = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            if (!await CanUserAccessBusiness(businessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            List<Models.BusinessImage> images;
            
            if (string.IsNullOrEmpty(imageType))
            {
                images = await _imageService.GetBusinessImagesAsync(businessId);
            }
            else
            {
                images = await _imageService.GetBusinessImagesByTypeAsync(businessId, imageType);
            }

            var imageData = images.Select(img => new
            {
                id = img.Id,
                imageType = img.ImageType,
                fileName = img.FileName,
                originalFileName = img.OriginalFileName,
                imageUrl = img.ImageUrl,
                thumbnailUrl = img.ThumbnailUrl,
                altText = img.AltText,
                displayOrder = img.DisplayOrder,
                isActive = img.IsActive,
                isApproved = img.IsApproved,
                uploadedAt = img.UploadedAt,
                fileSize = img.FileSize
            });

            return Json(new { success = true, data = imageData });
        }

        /// <summary>
        /// Validate image file before upload
        /// </summary>
        [HttpPost]
        public IActionResult ValidateFile([FromForm] IFormFile file)
        {
            var validationResult = _imageService.ValidateImageFile(file);
            
            return Json(new
            {
                isValid = validationResult.IsValid,
                errors = validationResult.Errors
            });
        }

        /// <summary>
        /// Admin-only: Approve image
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int imageId)
        {
            var approvedBy = User.Identity?.Name ?? "System";
            var success = await _imageService.ApproveImageAsync(imageId, approvedBy);
            
            if (success)
            {
                return Json(new { success = true, message = "Image approved successfully" });
            }

            return Json(new { success = false, message = "Failed to approve image" });
        }

        /// <summary>
        /// Helper method to check if user can access business
        /// </summary>
        private async Task<bool> CanUserAccessBusiness(int businessId, string userId)
        {
            // Check if user is admin
            if (User.IsInRole("Admin"))
                return true;

            // Check if user owns the business
            var business = await _businessService.GetBusinessByIdAsync(businessId);
            return business?.UserId == userId;
        }
    }
}