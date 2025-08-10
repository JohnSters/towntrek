using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;

namespace TownTrek.Services.Interfaces
{
    public interface IBusinessService
    {
        Task<AddBusinessViewModel> GetAddBusinessViewModelAsync(string userId);
        Task<ServiceResult> CreateBusinessAsync(AddBusinessViewModel model, string userId);
        Task<ServiceResult> UpdateBusinessAsync(int businessId, AddBusinessViewModel model, string userId);
        Task<List<Business>> GetUserBusinessesAsync(string userId);
        Task<Business?> GetBusinessByIdAsync(int id, string userId);
        Task<Business?> GetBusinessByIdAsync(int id);
        Task<ServiceResult> DeleteBusinessAsync(int businessId, string userId);
        Task<bool> CanUserAddBusinessAsync(string userId);
        Task<List<BusinessCategoryOption>> GetBusinessCategoriesAsync();
        Task<List<BusinessCategoryOption>> GetSubCategoriesAsync(string category);
    }
}