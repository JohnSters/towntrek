using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing business operations and data
    /// </summary>
    public interface IBusinessService
    {
        /// <summary>
        /// Gets the add business view model for a user
        /// </summary>
        Task<AddBusinessViewModel> GetAddBusinessViewModelAsync(string userId);
        
        /// <summary>
        /// Creates a new business for a user
        /// </summary>
        Task<ServiceResult> CreateBusinessAsync(AddBusinessViewModel model, string userId);
        
        /// <summary>
        /// Updates an existing business
        /// </summary>
        Task<ServiceResult> UpdateBusinessAsync(int businessId, AddBusinessViewModel model, string userId);
        
        /// <summary>
        /// Gets all businesses for a specific user
        /// </summary>
        Task<List<Business>> GetUserBusinessesAsync(string userId);
        
        /// <summary>
        /// Gets a business by ID for a specific user
        /// </summary>
        Task<Business?> GetBusinessByIdAsync(int id, string userId);
        
        /// <summary>
        /// Gets a business by ID without user validation
        /// </summary>
        Task<Business?> GetBusinessByIdAsync(int id);
        
        /// <summary>
        /// Deletes a business for a user
        /// </summary>
        Task<ServiceResult> DeleteBusinessAsync(int businessId, string userId);
        
        /// <summary>
        /// Checks if a user can add a new business
        /// </summary>
        Task<bool> CanUserAddBusinessAsync(string userId);
        
        /// <summary>
        /// Gets all available business categories
        /// </summary>
        Task<List<BusinessCategoryOption>> GetBusinessCategoriesAsync();
        
        /// <summary>
        /// Gets sub-categories for a specific category
        /// </summary>
        Task<List<BusinessCategoryOption>> GetSubCategoriesAsync(string category);
    }
}