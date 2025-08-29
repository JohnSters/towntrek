using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing admin-specific business operations
    /// </summary>
    public interface IAdminBusinessService
    {
        /// <summary>
        /// Gets all businesses for admin review with related data
        /// </summary>
        Task<List<Business>> GetAllBusinessesForAdminAsync();
        
        /// <summary>
        /// Approves a business listing
        /// </summary>
        Task<ServiceResult> ApproveBusinessAsync(int businessId, string approvedBy);
        
        /// <summary>
        /// Rejects a business listing
        /// </summary>
        Task<ServiceResult> RejectBusinessAsync(int businessId);
        
        /// <summary>
        /// Suspends a business listing
        /// </summary>
        Task<ServiceResult> SuspendBusinessAsync(int businessId);
        
        /// <summary>
        /// Deletes a business listing (soft delete)
        /// </summary>
        Task<ServiceResult> DeleteBusinessAsync(int businessId);
        
        /// <summary>
        /// Gets a business by ID for admin operations
        /// </summary>
        Task<Business?> GetBusinessByIdForAdminAsync(int businessId);
    }
}
