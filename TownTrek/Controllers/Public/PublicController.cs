using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;
using TownTrek.Data;
using Microsoft.EntityFrameworkCore;
using TownTrek.Services.Interfaces.ClientAnalytics;

namespace TownTrek.Controllers.Public
{
    public class PublicController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAnalyticsExportService _analyticsExportService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PublicController> _logger;

        public PublicController(
            IMemberService memberService,
            UserManager<ApplicationUser> userManager,
            IAnalyticsExportService analyticsExportService,
            ApplicationDbContext context,
            ILogger<PublicController> logger)
        {
            _memberService = memberService;
            _userManager = userManager;
            _analyticsExportService = analyticsExportService;
            _context = context;
            _logger = logger;
        }

        // GET: /Public
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search = null, int? townId = null, string? category = null, string? subCategory = null, int page = 1)
        {
            try
            {
                // Get current user ID if authenticated
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                
                // Use the search view model for public landing/search page
                var viewModel = await _memberService.SearchBusinessesAsync(search, townId, category, subCategory, page, userId: userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public index/search page");
                TempData["ErrorMessage"] = "An error occurred while loading businesses.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Public/Town/{townId}
        [AllowAnonymous]
        [Route("Public/Town/{townId:int}")]
        public async Task<IActionResult> TownBusinesses(int townId, string? category = null, string? subCategory = null, string? search = null, int page = 1)
        {
            try
            {
                // Get current user ID if authenticated
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                
                var viewModel = await _memberService.GetTownBusinessListAsync(townId, category, subCategory, search, page, userId: userId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BusinessListPartial", viewModel);
                }
                
                return View(viewModel);
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = "Town not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading town businesses for town {TownId}", townId);
                TempData["ErrorMessage"] = "An error occurred while loading businesses.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Public/Business/{businessId}
        [AllowAnonymous]
        [Route("Public/Business/{businessId:int}")]
        public async Task<IActionResult> BusinessDetails(int businessId)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                var viewModel = await _memberService.GetBusinessDetailsAsync(businessId, userId);
                return View(viewModel);
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = "Business not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading business details for business {BusinessId}", businessId);
                TempData["ErrorMessage"] = "An error occurred while loading business details.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Public/Search
        [AllowAnonymous]
        public async Task<IActionResult> Search(string? search = null, int? townId = null, string? category = null, string? subCategory = null, int page = 1)
        {
            try
            {
                // Get current user ID if authenticated
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                
                var viewModel = await _memberService.SearchBusinessesAsync(search, townId, category, subCategory, page, userId: userId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_SearchResultsPartial", viewModel);
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing business search");
                TempData["ErrorMessage"] = "An error occurred while searching businesses.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Public/Favorites (members only)
        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var favorites = await _memberService.GetUserFavoritesAsync(userId!);
                return View(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user favorites");
                TempData["ErrorMessage"] = "An error occurred while loading your favorites.";
                return RedirectToAction("Index");
            }
        }

        // POST: /Public/AddReview (members only)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid review data." });
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _memberService.AddReviewAsync(userId!, model);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = "Review submitted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for business {BusinessId}", model.BusinessId);
                return Json(new { success = false, message = "An error occurred while submitting your review." });
            }
        }

        // POST: /Public/AddReviewResponse (business owners only)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReviewResponse(AddReviewResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid response data." });
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _memberService.SubmitReviewResponseAsync(model, userId!);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = "Response posted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review response for review {ReviewId}", model.ReviewId);
                return Json(new { success = false, message = "An error occurred while posting your response." });
            }
        }

        // POST: /Public/ToggleFavorite (members only)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int businessId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isNowFavorite = await _memberService.ToggleFavoriteAsync(userId!, businessId);
                
                return Json(new { 
                    success = true, 
                    isFavorite = isNowFavorite,
                    message = isNowFavorite ? "Added to favorites" : "Removed from favorites"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for business {BusinessId}", businessId);
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        // API: /Public/Api/Towns
        [AllowAnonymous]
        [Route("Public/Api/Towns")]
        public async Task<IActionResult> GetTowns()
        {
            try
            {
                var towns = await _memberService.GetAvailableTownsAsync();
                return Json(towns.Select(t => new { id = t.Id, name = t.Name, province = t.Province }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading towns API");
                return Json(new { error = "Failed to load towns" });
            }
        }

        // GET: /Public/Plans - Subscription plans page
        [AllowAnonymous]
        public IActionResult Plans()
        {
            return View();
        }

        // API: /Public/Api/Categories
        [AllowAnonymous]
        [Route("Public/Api/Categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _memberService.GetBusinessCategoriesAsync();
                return Json(categories.Select(c => new { 
                    key = c.Key, 
                    name = c.Name, 
                    subCategories = c.SubCategories.Select(sc => new { key = sc.Key, name = sc.Name })
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories API");
                return Json(new { error = "Failed to load categories" });
            }
        }

        // GET: Public/SharedDashboard
        public async Task<IActionResult> SharedDashboard(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return View("SharedDashboardError", new { message = "Invalid or missing shareable link token." });
                }

                // Validate the shareable link
                var isValid = await _analyticsExportService.ValidateShareableLinkAsync(token);
                if (!isValid)
                {
                    return View("SharedDashboardError", new { message = "This shareable link has expired or is no longer valid." });
                }

                // Get the shareable link details
                var shareableLink = await _context.AnalyticsShareableLinks
                    .Include(l => l.User)
                    .Include(l => l.Business)
                    .FirstOrDefaultAsync(l => l.LinkToken == token && l.IsActive);

                if (shareableLink == null)
                {
                    return View("SharedDashboardError", new { message = "Shareable link not found." });
                }

                // Get analytics data
                var analyticsData = await _analyticsExportService.GetShareableLinkDataAsync(token);
                if (analyticsData == null)
                {
                    return View("SharedDashboardError", new { message = "Unable to load analytics data." });
                }

                var viewModel = new
                {
                    ShareableLink = shareableLink,
                    AnalyticsData = analyticsData,
                    DashboardType = shareableLink.DashboardType,
                    BusinessName = shareableLink.Business?.Name,
                    UserName = shareableLink.User?.UserName,
                    CreatedAt = shareableLink.CreatedAt,
                    ExpiresAt = shareableLink.ExpiresAt,
                    AccessCount = shareableLink.AccessCount
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shared dashboard for token {Token}", token);
                return View("SharedDashboardError", new { message = "An error occurred while loading the shared dashboard." });
            }
        }

        // GET: Public/SharedDashboardError
        public IActionResult SharedDashboardError()
        {
            return View();
        }
    }
}
