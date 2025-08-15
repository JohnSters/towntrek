using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "ClientAccess")]
    public class ImageController(
        IImageService imageService,
        IBusinessService businessService,
        ILogger<ImageController> logger) : Controller
    {
        private readonly IImageService _imageService = imageService;
        private readonly IBusinessService _businessService = businessService;
        private readonly ILogger<ImageController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Gallery(int businessId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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
                MaxFileSizeBytes = 5 * 1024 * 1024,
                AllowedFileTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"]
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MediaGallery()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var businesses = await _businessService.GetUserBusinessesAsync(userId);

            var allImages = new List<Models.BusinessImage>();
            var businessImageCounts = new Dictionary<int, int>();
            foreach (var business in businesses)
            {
                var businessImages = await _imageService.GetBusinessImagesAsync(business.Id);
                allImages.AddRange(businessImages);
                businessImageCounts[business.Id] = businessImages.Count;
            }

            var model = new MediaGalleryOverviewViewModel
            {
                Businesses = businesses,
                AllImages = allImages.OrderByDescending(i => i.UploadedAt).ToList(),
                BusinessImageCounts = businessImageCounts,
                TotalImages = allImages.Count,
                TotalLogos = allImages.Count(i => i.ImageType == "Logo"),
                TotalGalleryImages = allImages.Count(i => i.ImageType == "Gallery"),
                CanUpload = true,
                MaxFileSizeBytes = 5 * 1024 * 1024,
                AllowedFileTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"]
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] ImageUploadViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!await CanUserAccessBusiness(model.BusinessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            if (model.ImageFile == null)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            var result = await _imageService.UploadBusinessImageAsync(model.BusinessId, model.ImageFile, model.ImageType, model.DisplayOrder);
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

        [HttpPost]
        public async Task<IActionResult> UploadMultiple([FromForm] int businessId, [FromForm] string imageType, [FromForm] List<IFormFile> files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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
                success = successfulUploads.Count != 0,
                message = $"{successfulUploads.Count} images uploaded successfully" + (failedUploads.Count != 0 ? $", {failedUploads.Count} failed" : ""),
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

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteImageRequest request)
        {
            if (request == null || request.ImageId <= 0) return Json(new { success = false, message = "Invalid request" });
            var imageId = request.ImageId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var image = await _imageService.GetImageByIdAsync(imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found" });
            }
            if (!await CanUserAccessBusiness(image.BusinessId, userId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            var success = await _imageService.DeleteImageAsync(imageId);
            return Json(new { success, message = success ? "Image deleted successfully" : "Failed to delete image" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMetadata(int imageId, string? altText, int? displayOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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
            return Json(new { success, message = success ? "Image updated successfully" : "Failed to update image" });
        }

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

        [HttpPost]
        public IActionResult ValidateFile([FromForm] IFormFile file)
        {
            var validationResult = _imageService.ValidateImageFile(file);
            return Json(new { isValid = validationResult.IsValid, errors = validationResult.Errors });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int imageId)
        {
            var approvedBy = User.Identity?.Name ?? "System";
            var success = await _imageService.ApproveImageAsync(imageId, approvedBy);
            return Json(new { success, message = success ? "Image approved successfully" : "Failed to approve image" });
        }

        private async Task<bool> CanUserAccessBusiness(int businessId, string userId)
        {
            if (User.IsInRole("Admin")) return true;
            var business = await _businessService.GetBusinessByIdAsync(businessId);
            return business?.UserId == userId;
        }
    }
}


