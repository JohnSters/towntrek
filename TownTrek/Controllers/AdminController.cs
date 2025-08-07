using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Dashboard - Main admin overview
        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalTowns = await _context.Towns.CountAsync(),
                TotalBusinesses = await _context.Businesses.CountAsync(),
                ActiveBusinesses = await _context.Businesses.CountAsync(b => b.Status == "Active"),
                PendingApprovals = await _context.Businesses.CountAsync(b => b.Status == "Pending"),
                TotalPopulation = await _context.Towns.Where(t => t.Population.HasValue).SumAsync(t => t.Population!.Value),
                TownsWithLandmarks = await _context.Towns.CountAsync(t => !string.IsNullOrEmpty(t.Landmarks))
            };

            return View(stats);
        }

        // Towns Management
        public async Task<IActionResult> Towns()
        {
            var towns = await _context.Towns
                .Include(t => t.Businesses)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(towns);
        }

        public IActionResult AddTown()
        {
            var model = new AddTownViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTown(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if town already exists
                var existingTown = await _context.Towns
                    .FirstOrDefaultAsync(t => t.Name == model.Name && t.Province == model.Province);

                if (existingTown != null)
                {
                    ModelState.AddModelError("", "A town with this name already exists in the selected province.");
                    return View(model);
                }

                var town = new Town
                {
                    Name = model.Name,
                    Province = model.Province,
                    PostalCode = model.PostalCode,
                    Description = model.Description,
                    Population = model.Population,
                    Landmarks = model.Landmarks,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Towns.Add(town);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Town '{model.Name}' has been added successfully!";
                return RedirectToAction(nameof(Towns));
            }

            return View(model);
        }

        public async Task<IActionResult> EditTown(int id)
        {
            var town = await _context.Towns.FindAsync(id);
            if (town == null)
            {
                return NotFound();
            }

            var model = new AddTownViewModel
            {
                Id = town.Id,
                Name = town.Name,
                Province = town.Province,
                PostalCode = town.PostalCode,
                Description = town.Description,
                Population = town.Population,
                Landmarks = town.Landmarks,
                Latitude = town.Latitude,
                Longitude = town.Longitude
            };

            return View("AddTown", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTown(AddTownViewModel model)
        {
            if (ModelState.IsValid)
            {
                var town = await _context.Towns.FindAsync(model.Id);
                if (town == null)
                {
                    return NotFound();
                }

                // Check if another town with same name/province exists
                var existingTown = await _context.Towns
                    .FirstOrDefaultAsync(t => t.Name == model.Name && t.Province == model.Province && t.Id != model.Id);

                if (existingTown != null)
                {
                    ModelState.AddModelError("", "A town with this name already exists in the selected province.");
                    return View("AddTown", model);
                }

                town.Name = model.Name;
                town.Province = model.Province;
                town.PostalCode = model.PostalCode;
                town.Description = model.Description;
                town.Population = model.Population;
                town.Landmarks = model.Landmarks;
                town.Latitude = model.Latitude;
                town.Longitude = model.Longitude;
                town.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Town '{model.Name}' has been updated successfully!";
                return RedirectToAction(nameof(Towns));
            }

            return View("AddTown", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTown(int id)
        {
            var town = await _context.Towns
                .Include(t => t.Businesses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (town == null)
            {
                return NotFound();
            }

            if (town.Businesses.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{town.Name}' because it has associated businesses.";
                return RedirectToAction(nameof(Towns));
            }

            _context.Towns.Remove(town);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Town '{town.Name}' has been deleted successfully!";
            return RedirectToAction(nameof(Towns));
        }

        // Business Management
        public async Task<IActionResult> Businesses()
        {
            var businesses = await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(businesses);
        }

        public async Task<IActionResult> EditBusiness(int id)
        {
            var business = await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .Include(b => b.BusinessImages)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (business == null)
            {
                return NotFound();
            }

            // Convert to view model
            var model = new AddBusinessViewModel
            {
                Id = business.Id,
                BusinessName = business.Name,
                BusinessCategory = business.Category,
                SubCategory = business.SubCategory,
                BusinessDescription = business.Description,
                ShortDescription = business.ShortDescription,
                PhoneNumber = business.PhoneNumber,
                PhoneNumber2 = business.PhoneNumber2,
                EmailAddress = business.EmailAddress,
                Website = business.Website,
                PhysicalAddress = business.PhysicalAddress,
                Latitude = business.Latitude,
                Longitude = business.Longitude,
                TownId = business.TownId,
                
                // Populate business hours
                BusinessHours = business.BusinessHours?.Select(bh => new BusinessHourViewModel
                {
                    DayOfWeek = bh.DayOfWeek,
                    DayName = GetDayName(bh.DayOfWeek),
                    IsOpen = bh.IsOpen,
                    OpenTime = bh.OpenTime?.ToString(@"hh\:mm"),
                    CloseTime = bh.CloseTime?.ToString(@"hh\:mm"),
                    IsSpecialHours = bh.IsSpecialHours,
                    SpecialHoursNote = bh.SpecialHoursNote
                }).ToList() ?? new List<BusinessHourViewModel>(),
                
                // Populate existing business images
                ExistingBusinessImages = business.BusinessImages?.Where(bi => bi.IsActive).OrderBy(bi => bi.DisplayOrder).ToList(),
                
                // Populate available towns
                AvailableTowns = await _context.Towns.OrderBy(t => t.Name).ToListAsync(),
                
                // Populate available categories
                AvailableCategories = GetBusinessCategories(),
                AvailableSubCategories = GetBusinessSubCategories(business.Category)
            };

            return View("EditBusiness", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBusiness(AddBusinessViewModel model)
        {
            if (ModelState.IsValid)
            {
                var business = await _context.Businesses
                    .Include(b => b.BusinessHours)
                    .Include(b => b.BusinessImages)
                    .FirstOrDefaultAsync(b => b.Id == model.Id);
                
                if (business == null)
                {
                    return NotFound();
                }

                // Update basic information
                business.Name = model.BusinessName;
                business.Category = model.BusinessCategory;
                business.SubCategory = model.SubCategory;
                business.Description = model.BusinessDescription;
                business.ShortDescription = model.ShortDescription;
                business.PhoneNumber = model.PhoneNumber;
                business.PhoneNumber2 = model.PhoneNumber2;
                business.EmailAddress = model.EmailAddress;
                business.Website = model.Website;
                business.PhysicalAddress = model.PhysicalAddress;
                business.Latitude = model.Latitude;
                business.Longitude = model.Longitude;
                business.TownId = model.TownId;
                business.UpdatedAt = DateTime.UtcNow;

                // Handle image removals
                var imagesToRemove = Request.Form["ImagesToRemove"].ToList();
                if (imagesToRemove.Any())
                {
                    var imagesToDelete = business.BusinessImages.Where(bi => imagesToRemove.Contains(bi.Id.ToString())).ToList();
                    foreach (var image in imagesToDelete)
                    {
                        image.IsActive = false;
                        image.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Handle new image uploads
                if (model.BusinessLogo != null)
                {
                    await SaveBusinessImage(business.Id, model.BusinessLogo, "Logo");
                }

                if (model.BusinessImages != null && model.BusinessImages.Any())
                {
                    foreach (var imageFile in model.BusinessImages)
                    {
                        await SaveBusinessImage(business.Id, imageFile, "Gallery");
                    }
                }

                // Update business hours
                if (model.BusinessHours != null)
                {
                    // Remove existing business hours
                    _context.BusinessHours.RemoveRange(business.BusinessHours);
                    
                    // Add new business hours
                    foreach (var hour in model.BusinessHours.Where(h => h.IsOpen))
                    {
                        TimeSpan? openTime = null;
                        TimeSpan? closeTime = null;
                        
                        if (TimeSpan.TryParse(hour.OpenTime, out var open))
                            openTime = open;
                        if (TimeSpan.TryParse(hour.CloseTime, out var close))
                            closeTime = close;
                        
                        business.BusinessHours.Add(new BusinessHour
                        {
                            BusinessId = business.Id,
                            DayOfWeek = hour.DayOfWeek,
                            IsOpen = hour.IsOpen,
                            OpenTime = openTime,
                            CloseTime = closeTime,
                            IsSpecialHours = hour.IsSpecialHours,
                            SpecialHoursNote = hour.SpecialHoursNote
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Business '{model.BusinessName}' has been updated successfully!";
                return RedirectToAction(nameof(Businesses));
            }

            // If validation fails, repopulate the model and return to view
            model.AvailableTowns = await _context.Towns.OrderBy(t => t.Name).ToListAsync();
            model.AvailableCategories = GetBusinessCategories();
            model.AvailableSubCategories = GetBusinessSubCategories(model.BusinessCategory);
            
            return View("EditBusiness", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Active";
            business.ApprovedAt = DateTime.UtcNow;
            business.ApprovedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been approved and is now live!";
            return RedirectToAction(nameof(Businesses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Inactive";
            business.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been rejected.";
            return RedirectToAction(nameof(Businesses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Suspended";
            business.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been suspended.";
            return RedirectToAction(nameof(Businesses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = "Deleted";
            business.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Business '{business.Name}' has been deleted.";
            return RedirectToAction(nameof(Businesses));
        }

        // User Management (placeholder)
        public IActionResult Users()
        {
            return View();
        }

        // Settings
        public IActionResult Settings()
        {
            return View();
        }

        // Helper methods for business categories
        private List<BusinessCategoryOption> GetBusinessCategories()
        {
            return new List<BusinessCategoryOption>
            {
                new BusinessCategoryOption { Value = "shops-retail", Text = "Shops & Retail", Description = "Retail stores and shops" },
                new BusinessCategoryOption { Value = "restaurants-food", Text = "Restaurants & Food Services", Description = "Food and dining establishments" },
                new BusinessCategoryOption { Value = "markets-vendors", Text = "Markets & Vendors", Description = "Markets and vendor stalls" },
                new BusinessCategoryOption { Value = "accommodation", Text = "Accommodation", Description = "Hotels, guesthouses, and lodging" },
                new BusinessCategoryOption { Value = "tours-experiences", Text = "Tours & Experiences", Description = "Tourism and experience services" },
                new BusinessCategoryOption { Value = "events", Text = "Events", Description = "Events and entertainment" }
            };
        }

        private List<BusinessCategoryOption> GetBusinessSubCategories(string category)
        {
            var subCategories = new Dictionary<string, List<BusinessCategoryOption>>
            {
                ["shops-retail"] = new List<BusinessCategoryOption>
                {
                    new BusinessCategoryOption { Value = "clothing-fashion", Text = "Clothing & Fashion" },
                    new BusinessCategoryOption { Value = "electronics", Text = "Electronics" },
                    new BusinessCategoryOption { Value = "home-garden", Text = "Home & Garden" },
                    new BusinessCategoryOption { Value = "books-stationery", Text = "Books & Stationery" },
                    new BusinessCategoryOption { Value = "sports-outdoor", Text = "Sports & Outdoor" },
                    new BusinessCategoryOption { Value = "jewelry-accessories", Text = "Jewelry & Accessories" },
                    new BusinessCategoryOption { Value = "pharmacy-health", Text = "Pharmacy & Health" },
                    new BusinessCategoryOption { Value = "other-retail", Text = "Other Retail" }
                },
                ["restaurants-food"] = new List<BusinessCategoryOption>
                {
                    new BusinessCategoryOption { Value = "fine-dining", Text = "Fine Dining" },
                    new BusinessCategoryOption { Value = "casual-dining", Text = "Casual Dining" },
                    new BusinessCategoryOption { Value = "fast-food", Text = "Fast Food" },
                    new BusinessCategoryOption { Value = "cafe-coffee", Text = "Café & Coffee" },
                    new BusinessCategoryOption { Value = "bakery-pastry", Text = "Bakery & Pastry" },
                    new BusinessCategoryOption { Value = "pizza-italian", Text = "Pizza & Italian" },
                    new BusinessCategoryOption { Value = "asian-cuisine", Text = "Asian Cuisine" },
                    new BusinessCategoryOption { Value = "african-cuisine", Text = "African Cuisine" },
                    new BusinessCategoryOption { Value = "other-food", Text = "Other Food Services" }
                },
                ["markets-vendors"] = new List<BusinessCategoryOption>
                {
                    new BusinessCategoryOption { Value = "farmers-market", Text = "Farmers Market" },
                    new BusinessCategoryOption { Value = "craft-market", Text = "Craft Market" },
                    new BusinessCategoryOption { Value = "flea-market", Text = "Flea Market" },
                    new BusinessCategoryOption { Value = "food-market", Text = "Food Market" },
                    new BusinessCategoryOption { Value = "artisan-market", Text = "Artisan Market" },
                    new BusinessCategoryOption { Value = "other-market", Text = "Other Market" }
                },
                ["accommodation"] = new List<BusinessCategoryOption>
                {
                    new BusinessCategoryOption { Value = "hotel", Text = "Hotel" },
                    new BusinessCategoryOption { Value = "guesthouse", Text = "Guesthouse" },
                    new BusinessCategoryOption { Value = "bed-breakfast", Text = "Bed & Breakfast" },
                    new BusinessCategoryOption { Value = "self-catering", Text = "Self-Catering" },
                    new BusinessCategoryOption { Value = "camping", Text = "Camping" },
                    new BusinessCategoryOption { Value = "other-accommodation", Text = "Other Accommodation" }
                },
                ["tours-experiences"] = new List<BusinessCategoryOption>
                {
                    new BusinessCategoryOption { Value = "cultural-tours", Text = "Cultural Tours" },
                    new BusinessCategoryOption { Value = "adventure-tours", Text = "Adventure Tours" },
                    new BusinessCategoryOption { Value = "food-tours", Text = "Food Tours" },
                    new BusinessCategoryOption { Value = "nature-tours", Text = "Nature Tours" },
                    new BusinessCategoryOption { Value = "city-tours", Text = "City Tours" },
                    new BusinessCategoryOption { Value = "other-tours", Text = "Other Tours" }
                },
                ["events"] = new List<BusinessCategoryOption>
                {
                    new BusinessCategoryOption { Value = "festivals", Text = "Festivals" },
                    new BusinessCategoryOption { Value = "concerts", Text = "Concerts" },
                    new BusinessCategoryOption { Value = "exhibitions", Text = "Exhibitions" },
                    new BusinessCategoryOption { Value = "workshops", Text = "Workshops" },
                    new BusinessCategoryOption { Value = "sports-events", Text = "Sports Events" },
                    new BusinessCategoryOption { Value = "other-events", Text = "Other Events" }
                }
            };

            return subCategories.ContainsKey(category) ? subCategories[category] : new List<BusinessCategoryOption>();
        }

        private string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Sunday",
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                _ => "Unknown"
            };
        }

        private async Task SaveBusinessImage(int businessId, IFormFile imageFile, string imageType)
        {
            if (imageFile == null || imageFile.Length == 0)
                return;

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                return;

            // Validate file size (2MB limit)
            if (imageFile.Length > 2 * 1024 * 1024)
                return;

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "businesses");
            
            // Ensure directory exists
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Create business image record
            var businessImage = new BusinessImage
            {
                BusinessId = businessId,
                ImageType = imageType,
                FileName = fileName,
                OriginalFileName = imageFile.FileName,
                FileSize = imageFile.Length,
                ContentType = imageFile.ContentType,
                ImageUrl = $"/uploads/businesses/{fileName}",
                AltText = $"{imageType} for business",
                DisplayOrder = 0,
                IsActive = true,
                IsApproved = true,
                UploadedAt = DateTime.UtcNow
            };

            _context.BusinessImages.Add(businessImage);
        }
    }
}