using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class MemberService : IMemberService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MemberService> _logger;

        public MemberService(ApplicationDbContext context, ILogger<MemberService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MemberDashboardViewModel> GetMemberDashboardAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var viewModel = new MemberDashboardViewModel
            {
                UserName = user?.UserName ?? "Member"
            };

            // Get available towns
            viewModel.AvailableTowns = await GetAvailableTownsAsync();

            // Get featured businesses
            viewModel.FeaturedBusinesses = await GetFeaturedBusinessesAsync(count: 6, userId: userId);

            // Get recent businesses
            viewModel.RecentBusinesses = await GetRecentBusinessesAsync(6, userId);

            // Get total business count
            viewModel.TotalBusinessCount = await _context.Businesses
                .Where(b => b.Status == "Active")
                .CountAsync();

            // Get user favorites
            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.UserFavorites = await _context.Set<FavoriteBusiness>()
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Business)
                    .ToListAsync();
            }

            return viewModel;
        }

        public async Task<TownBusinessListViewModel> GetTownBusinessListAsync(int townId, string? category = null, string? subCategory = null, string? searchTerm = null, int page = 1, int pageSize = 12, string? userId = null)
        {
            var town = await _context.Towns.FindAsync(townId);
            if (town == null)
                throw new ArgumentException("Town not found", nameof(townId));

            var query = _context.Businesses
                .Where(b => b.TownId == townId && b.Status == "Active")
                .Include(b => b.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category == category);
            }

            if (!string.IsNullOrEmpty(subCategory))
            {
                query = query.Where(b => b.SubCategory == subCategory);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm) || 
                                        b.Description.Contains(searchTerm) ||
                                        b.PhysicalAddress.Contains(searchTerm));
            }

            // Get total count
            var totalResults = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalResults / (double)pageSize);

            // Apply pagination
            var businesses = await query
                .OrderByDescending(b => b.IsFeatured)
                .ThenByDescending(b => b.Rating)
                .ThenBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var businessCards = new List<BusinessCardViewModel>();
            foreach (var business in businesses)
            {
                businessCards.Add(await MapToBusinessCardViewModelAsync(business, userId));
            }

            return new TownBusinessListViewModel
            {
                Town = town,
                Categories = await GetBusinessCategoriesAsync(),
                Businesses = businessCards,
                SelectedCategory = category,
                SelectedSubCategory = subCategory,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalResults = totalResults
            };
        }

        public async Task<BusinessDetailsViewModel> GetBusinessDetailsAsync(int businessId, string? userId = null)
        {
            var business = await _context.Businesses
                .Include(b => b.Town)
                .Include(b => b.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .FirstOrDefaultAsync(b => b.Id == businessId && b.Status == "Active");

            if (business == null)
                throw new ArgumentException("Business not found", nameof(businessId));

            // Increment view count
            await IncrementBusinessViewCountAsync(businessId);

            var viewModel = new BusinessDetailsViewModel
            {
                Business = MapToBusinessCardViewModel(business)
            };

            // Get reviews with responses
            var reviews = await _context.Set<BusinessReview>()
                .Where(r => r.BusinessId == businessId && r.IsActive && r.IsApproved)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewResponses = await _context.Set<BusinessReviewResponse>()
                .Where(rr => reviews.Select(r => r.Id).Contains(rr.BusinessReviewId) && rr.IsActive)
                .Include(rr => rr.User)
                .ToListAsync();

            viewModel.Reviews = reviews.Select(review => new ReviewWithResponseViewModel
            {
                Review = review,
                Response = reviewResponses.FirstOrDefault(rr => rr.BusinessReviewId == review.Id),
                CanUserRespond = !string.IsNullOrEmpty(userId) && business.UserId == userId && 
                                !reviewResponses.Any(rr => rr.BusinessReviewId == review.Id)
            }).ToList();

            // Check if user can review and get existing review
            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.UserReview = await _context.Set<BusinessReview>()
                    .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.UserId == userId);
                
                viewModel.CanUserReview = viewModel.UserReview == null;
                viewModel.Business.IsUserFavorite = await _context.Set<FavoriteBusiness>()
                    .AnyAsync(f => f.BusinessId == businessId && f.UserId == userId);
            }

            // Get related businesses (same category in same town)
            viewModel.RelatedBusinesses = await GetRelatedBusinessesAsync(business.TownId, business.Category, businessId, 4, userId);

            viewModel.NewReview.BusinessId = businessId;

            return viewModel;
        }

        public async Task<BusinessSearchViewModel> SearchBusinessesAsync(string? searchTerm = null, int? townId = null, string? category = null, string? subCategory = null, int page = 1, int pageSize = 12, string? userId = null)
        {
            var query = _context.Businesses
                .Where(b => b.Status == "Active")
                .Include(b => b.Town)
                .Include(b => b.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .AsQueryable();

            // Apply filters
            if (townId.HasValue)
            {
                query = query.Where(b => b.TownId == townId.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category == category);
            }

            if (!string.IsNullOrEmpty(subCategory))
            {
                query = query.Where(b => b.SubCategory == subCategory);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm) || 
                                        b.Description.Contains(searchTerm) ||
                                        b.PhysicalAddress.Contains(searchTerm) ||
                                        b.Town.Name.Contains(searchTerm));
            }

            var totalResults = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalResults / (double)pageSize);

            var businesses = await query
                .OrderByDescending(b => b.IsFeatured)
                .ThenByDescending(b => b.Rating)
                .ThenBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var businessCards = new List<BusinessCardViewModel>();
            foreach (var business in businesses)
            {
                businessCards.Add(await MapToBusinessCardViewModelAsync(business, userId));
            }

            return new BusinessSearchViewModel
            {
                SearchTerm = searchTerm,
                TownId = townId,
                Category = category,
                SubCategory = subCategory,
                AvailableTowns = await GetAvailableTownsAsync(),
                AvailableCategories = await GetBusinessCategoriesAsync(),
                Results = businessCards,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalResults = totalResults
            };
        }

        public async Task<List<Town>> GetAvailableTownsAsync()
        {
            return await _context.Towns
                .Where(t => t.IsActive)
                .OrderBy(t => t.Province)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<BusinessCategory>> GetBusinessCategoriesAsync()
        {
            return await _context.BusinessCategories
                .Where(c => c.IsActive)
                .Include(c => c.SubCategories.Where(sc => sc.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ReviewSubmissionResult> AddReviewAsync(string userId, AddReviewViewModel model)
        {
            try
            {
                // Check if user already has a review for this business
                var existingReview = await _context.Set<BusinessReview>()
                    .FirstOrDefaultAsync(r => r.BusinessId == model.BusinessId && r.UserId == userId);

                if (existingReview != null)
                {
                    return new ReviewSubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "You have already reviewed this business."
                    };
                }

                var review = new BusinessReview
                {
                    BusinessId = model.BusinessId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = true,
                    IsActive = true
                };

                _context.Set<BusinessReview>().Add(review);
                await _context.SaveChangesAsync();

                // Update business rating
                await UpdateBusinessRatingAsync(model.BusinessId);

                return new ReviewSubmissionResult
                {
                    IsSuccess = true,
                    Review = review
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for business {BusinessId} by user {UserId}", model.BusinessId, userId);
                return new ReviewSubmissionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while submitting your review. Please try again."
                };
            }
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, int businessId)
        {
            try
            {
                var favorite = await _context.Set<FavoriteBusiness>()
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.BusinessId == businessId);

                if (favorite != null)
                {
                    // Remove favorite
                    _context.Set<FavoriteBusiness>().Remove(favorite);
                    await _context.SaveChangesAsync();
                    return false;
                }
                else
                {
                    // Add favorite
                    var newFavorite = new FavoriteBusiness
                    {
                        UserId = userId,
                        BusinessId = businessId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Set<FavoriteBusiness>().Add(newFavorite);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for business {BusinessId} by user {UserId}", businessId, userId);
                return false;
            }
        }

        public async Task<List<BusinessCardViewModel>> GetUserFavoritesAsync(string userId)
        {
            var favorites = await _context.Set<FavoriteBusiness>()
                .Where(f => f.UserId == userId)
                .Include(f => f.Business)
                .ThenInclude(b => b.Town)
                .Include(f => f.Business.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(f => f.Business.BusinessHours)
                .Include(f => f.Business.BusinessServices)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return favorites.Select(f => MapToBusinessCardViewModel(f.Business, true)).ToList();
        }

        public async Task<List<BusinessCardViewModel>> GetFeaturedBusinessesAsync(int? townId = null, int count = 6, string? userId = null)
        {
            var query = _context.Businesses
                .Where(b => b.Status == "Active" && b.IsFeatured)
                .Include(b => b.Town)
                .Include(b => b.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .AsQueryable();

            if (townId.HasValue)
            {
                query = query.Where(b => b.TownId == townId.Value);
            }

            var businesses = await query
                .OrderByDescending(b => b.Rating)
                .ThenBy(b => b.Name)
                .Take(count)
                .ToListAsync();

            var businessCards = new List<BusinessCardViewModel>();
            foreach (var business in businesses)
            {
                businessCards.Add(await MapToBusinessCardViewModelAsync(business, userId));
            }

            return businessCards;
        }

        public async Task IncrementBusinessViewCountAsync(int businessId)
        {
            try
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business != null)
                {
                    business.ViewCount++;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing view count for business {BusinessId}", businessId);
            }
        }

        private async Task<List<BusinessCardViewModel>> GetRecentBusinessesAsync(int count, string? userId = null)
        {
            var businesses = await _context.Businesses
                .Where(b => b.Status == "Active")
                .Include(b => b.Town)
                .Include(b => b.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();

            var businessCards = new List<BusinessCardViewModel>();
            foreach (var business in businesses)
            {
                businessCards.Add(await MapToBusinessCardViewModelAsync(business, userId));
            }

            return businessCards;
        }

        private async Task<List<BusinessCardViewModel>> GetRelatedBusinessesAsync(int townId, string category, int excludeBusinessId, int count, string? userId = null)
        {
            var businesses = await _context.Businesses
                .Where(b => b.TownId == townId && 
                           b.Category == category && 
                           b.Id != excludeBusinessId && 
                           b.Status == "Active")
                .Include(b => b.Town)
                .Include(b => b.BusinessImages.Where(i => i.IsActive && i.IsApproved))
                .Include(b => b.BusinessHours)
                .Include(b => b.BusinessServices)
                .OrderByDescending(b => b.Rating)
                .Take(count)
                .ToListAsync();

            var businessCards = new List<BusinessCardViewModel>();
            foreach (var business in businesses)
            {
                businessCards.Add(await MapToBusinessCardViewModelAsync(business, userId));
            }

            return businessCards;
        }

        private async Task UpdateBusinessRatingAsync(int businessId)
        {
            var reviews = await _context.Set<BusinessReview>()
                .Where(r => r.BusinessId == businessId && r.IsActive && r.IsApproved)
                .ToListAsync();

            var business = await _context.Businesses.FindAsync(businessId);
            if (business != null)
            {
                business.TotalReviews = reviews.Count;
                business.Rating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : null;
                await _context.SaveChangesAsync();
            }
        }

        private BusinessCardViewModel MapToBusinessCardViewModel(Business business, bool isUserFavorite = false)
        {
            return new BusinessCardViewModel
            {
                Id = business.Id,
                Name = business.Name,
                Category = business.Category,
                SubCategory = business.SubCategory,
                ShortDescription = business.ShortDescription,
                PhoneNumber = business.PhoneNumber,
                EmailAddress = business.EmailAddress,
                Website = business.Website,
                PhysicalAddress = business.PhysicalAddress,
                LogoUrl = business.LogoUrl,
                CoverImageUrl = business.CoverImageUrl,
                IsFeatured = business.IsFeatured,
                IsVerified = business.IsVerified,
                Rating = business.Rating,
                TotalReviews = business.TotalReviews,
                IsUserFavorite = isUserFavorite,
                TownName = business.Town?.Name ?? string.Empty,
                GalleryImages = business.BusinessImages.Where(i => i.ImageType == "Gallery").OrderBy(i => i.DisplayOrder).ToList(),
                BusinessHours = business.BusinessHours.OrderBy(h => h.DayOfWeek).ToList(),
                Services = business.BusinessServices.Where(s => s.IsActive).ToList()
            };
        }

        private async Task<BusinessCardViewModel> MapToBusinessCardViewModelAsync(Business business, string? userId = null)
        {
            bool isUserFavorite = false;
            if (!string.IsNullOrEmpty(userId))
            {
                isUserFavorite = await _context.Set<FavoriteBusiness>()
                    .AnyAsync(f => f.BusinessId == business.Id && f.UserId == userId);
            }

            return MapToBusinessCardViewModel(business, isUserFavorite);
        }

        public async Task<ReviewResponseSubmissionResult> SubmitReviewResponseAsync(AddReviewResponseViewModel model, string userId)
        {
            try
            {
                // Get the review and verify the user owns the business
                var review = await _context.Set<BusinessReview>()
                    .Include(r => r.Business)
                    .FirstOrDefaultAsync(r => r.Id == model.ReviewId);

                if (review == null)
                {
                    return new ReviewResponseSubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Review not found."
                    };
                }

                if (review.Business.UserId != userId)
                {
                    return new ReviewResponseSubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "You can only respond to reviews for your own business."
                    };
                }

                // Check if response already exists
                var existingResponse = await _context.Set<BusinessReviewResponse>()
                    .FirstOrDefaultAsync(rr => rr.BusinessReviewId == model.ReviewId);

                if (existingResponse != null)
                {
                    return new ReviewResponseSubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "You have already responded to this review."
                    };
                }

                var response = new BusinessReviewResponse
                {
                    BusinessReviewId = model.ReviewId,
                    UserId = userId,
                    Response = model.Response.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Set<BusinessReviewResponse>().Add(response);
                await _context.SaveChangesAsync();

                // Load the response with user data for return
                response = await _context.Set<BusinessReviewResponse>()
                    .Include(rr => rr.User)
                    .FirstOrDefaultAsync(rr => rr.Id == response.Id);

                return new ReviewResponseSubmissionResult
                {
                    IsSuccess = true,
                    Response = response
                };
            }
            catch (Exception)
            {
                return new ReviewResponseSubmissionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while submitting your response. Please try again."
                };
            }
        }
    }
}
