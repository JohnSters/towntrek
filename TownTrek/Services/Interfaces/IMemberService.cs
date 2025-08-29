using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for member-specific operations and business interactions
    /// </summary>
    public interface IMemberService
    {
        /// <summary>
        /// Gets the member dashboard view model for a user
        /// </summary>
        Task<MemberDashboardViewModel> GetMemberDashboardAsync(string userId);
        
        /// <summary>
        /// Gets a list of businesses in a specific town with optional filtering
        /// </summary>
        Task<TownBusinessListViewModel> GetTownBusinessListAsync(int townId, string? category = null, string? subCategory = null, string? searchTerm = null, int page = 1, int pageSize = 12, string? userId = null);
        
        /// <summary>
        /// Gets detailed information about a specific business
        /// </summary>
        Task<BusinessDetailsViewModel> GetBusinessDetailsAsync(int businessId, string? userId = null);
        
        /// <summary>
        /// Searches for businesses with various filter criteria
        /// </summary>
        Task<BusinessSearchViewModel> SearchBusinessesAsync(string? searchTerm = null, int? townId = null, string? category = null, string? subCategory = null, int page = 1, int pageSize = 12, string? userId = null);
        
        /// <summary>
        /// Gets all available towns
        /// </summary>
        Task<List<Town>> GetAvailableTownsAsync();
        
        /// <summary>
        /// Gets all business categories
        /// </summary>
        Task<List<BusinessCategory>> GetBusinessCategoriesAsync();
        
        /// <summary>
        /// Adds a review for a business
        /// </summary>
        Task<ReviewSubmissionResult> AddReviewAsync(string userId, AddReviewViewModel model);
        
        /// <summary>
        /// Submits a response to a business review
        /// </summary>
        Task<ReviewResponseSubmissionResult> SubmitReviewResponseAsync(AddReviewResponseViewModel model, string userId);
        
        /// <summary>
        /// Toggles the favorite status of a business for a user
        /// </summary>
        Task<bool> ToggleFavoriteAsync(string userId, int businessId);
        
        /// <summary>
        /// Gets all favorite businesses for a user
        /// </summary>
        Task<List<BusinessCardViewModel>> GetUserFavoritesAsync(string userId);
        
        /// <summary>
        /// Gets featured businesses, optionally filtered by town
        /// </summary>
        Task<List<BusinessCardViewModel>> GetFeaturedBusinessesAsync(int? townId = null, int count = 6, string? userId = null);
        
        /// <summary>
        /// Increments the view count for a business
        /// </summary>
        Task IncrementBusinessViewCountAsync(int businessId);
    }
}
