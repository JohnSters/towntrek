using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    public class AdminBusinessService : IAdminBusinessService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminBusinessService> _logger;

        public AdminBusinessService(
            ApplicationDbContext context,
            ILogger<AdminBusinessService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Business>> GetAllBusinessesForAdminAsync()
        {
            try
            {
                var businesses = await _context.Businesses
                    .Include(b => b.Town)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} businesses for admin review", businesses.Count);
                return businesses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving businesses for admin review");
                throw;
            }
        }

        public async Task<ServiceResult> ApproveBusinessAsync(int businessId, string approvedBy)
        {
            try
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    _logger.LogWarning("Attempted to approve non-existent business with ID: {BusinessId}", businessId);
                    return ServiceResult.Error("Business not found.");
                }

                business.Status = "Active";
                business.ApprovedAt = DateTime.UtcNow;
                business.ApprovedBy = approvedBy;
                business.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' (ID: {BusinessId}) approved by {ApprovedBy}", 
                    business.Name, businessId, approvedBy);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving business {BusinessId}", businessId);
                return ServiceResult.Error("An error occurred while approving the business.");
            }
        }

        public async Task<ServiceResult> RejectBusinessAsync(int businessId)
        {
            try
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    _logger.LogWarning("Attempted to reject non-existent business with ID: {BusinessId}", businessId);
                    return ServiceResult.Error("Business not found.");
                }

                business.Status = "Inactive";
                business.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' (ID: {BusinessId}) rejected", 
                    business.Name, businessId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting business {BusinessId}", businessId);
                return ServiceResult.Error("An error occurred while rejecting the business.");
            }
        }

        public async Task<ServiceResult> SuspendBusinessAsync(int businessId)
        {
            try
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    _logger.LogWarning("Attempted to suspend non-existent business with ID: {BusinessId}", businessId);
                    return ServiceResult.Error("Business not found.");
                }

                business.Status = "Suspended";
                business.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' (ID: {BusinessId}) suspended", 
                    business.Name, businessId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending business {BusinessId}", businessId);
                return ServiceResult.Error("An error occurred while suspending the business.");
            }
        }

        public async Task<ServiceResult> DeleteBusinessAsync(int businessId)
        {
            try
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent business with ID: {BusinessId}", businessId);
                    return ServiceResult.Error("Business not found.");
                }

                business.Status = "Deleted";
                business.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Business '{BusinessName}' (ID: {BusinessId}) deleted", 
                    business.Name, businessId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting business {BusinessId}", businessId);
                return ServiceResult.Error("An error occurred while deleting the business.");
            }
        }

        public async Task<Business?> GetBusinessByIdForAdminAsync(int businessId)
        {
            try
            {
                var business = await _context.Businesses
                    .Include(b => b.Town)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == businessId);

                if (business == null)
                {
                    _logger.LogWarning("Business with ID {BusinessId} not found for admin operation", businessId);
                }

                return business;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business {BusinessId} for admin operation", businessId);
                throw;
            }
        }
    }
}
