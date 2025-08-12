using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services.Interfaces
{
    public interface IMemberService
    {
        Task<MemberDashboardViewModel> GetMemberDashboardAsync(string userId);
        Task<TownBusinessListViewModel> GetTownBusinessListAsync(int townId, string? category = null, string? subCategory = null, string? searchTerm = null, int page = 1, int pageSize = 12);
        Task<BusinessDetailsViewModel> GetBusinessDetailsAsync(int businessId, string? userId = null);
        Task<BusinessSearchViewModel> SearchBusinessesAsync(string? searchTerm = null, int? townId = null, string? category = null, string? subCategory = null, int page = 1, int pageSize = 12);
        Task<List<Town>> GetAvailableTownsAsync();
        Task<List<BusinessCategory>> GetBusinessCategoriesAsync();
        Task<ReviewSubmissionResult> AddReviewAsync(string userId, AddReviewViewModel model);
        Task<bool> ToggleFavoriteAsync(string userId, int businessId);
        Task<List<BusinessCardViewModel>> GetUserFavoritesAsync(string userId);
        Task<List<BusinessCardViewModel>> GetFeaturedBusinessesAsync(int? townId = null, int count = 6);
        Task IncrementBusinessViewCountAsync(int businessId);
    }
}
