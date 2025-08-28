using Microsoft.EntityFrameworkCore;

using TownTrek.Constants;
using TownTrek.Data;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.ClientAnalytics
{
    /// <summary>
    /// Service responsible for validating analytics parameters and requests
    /// </summary>
    public class ClientAnalyticsValidationService(
        ApplicationDbContext context,
        ILogger<ClientAnalyticsValidationService> logger) : IAnalyticsValidationService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ClientAnalyticsValidationService> _logger = logger;

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "User ID is required");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Analytics validation failed: User not found for ID {UserId}", userId);
                return (false, "User not found");
            }

            return (true, null);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateBusinessOwnershipAsync(int businessId, string userId)
        {
            if (businessId <= 0)
            {
                return (false, "Invalid business ID");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "User ID is required");
            }

            var business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);

            if (business == null)
            {
                _logger.LogWarning("Analytics validation failed: Business ownership validation failed for BusinessId {BusinessId}, UserId {UserId}", businessId, userId);
                return (false, "Business not found or access denied");
            }

            if (business.Status == AnalyticsConstants.BusinessStatus.Deleted)
            {
                return (false, "Cannot access analytics for deleted business");
            }

            return (true, null);
        }

        public (bool IsValid, string? ErrorMessage) ValidateAnalyticsDays(int days)
        {
            if (days < AnalyticsConstants.MinAnalyticsDays)
            {
                return (false, $"Analytics days must be at least {AnalyticsConstants.MinAnalyticsDays}");
            }

            if (days > AnalyticsConstants.MaxAnalyticsDays)
            {
                return (false, $"Analytics days cannot exceed {AnalyticsConstants.MaxAnalyticsDays}");
            }

            return (true, null);
        }

        public (bool IsValid, string? ErrorMessage) ValidatePlatform(string? platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
            {
                return (true, null); // Platform is optional
            }

            var validPlatforms = new[] { AnalyticsConstants.Platforms.Web, AnalyticsConstants.Platforms.Mobile, AnalyticsConstants.Platforms.Api, AnalyticsConstants.Platforms.All };
            
            if (!validPlatforms.Contains(platform))
            {
                return (false, $"Invalid platform. Must be one of: {string.Join(", ", validPlatforms)}");
            }

            return (true, null);
        }

        public (bool IsValid, string? ErrorMessage) ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                return (false, "Start date must be before end date");
            }

            var maxDateRange = TimeSpan.FromDays(AnalyticsConstants.MaxAnalyticsDays);
            if (endDate - startDate > maxDateRange)
            {
                return (false, $"Date range cannot exceed {AnalyticsConstants.MaxAnalyticsDays} days");
            }

            if (startDate < DateTime.UtcNow.AddYears(-1))
            {
                return (false, "Start date cannot be more than 1 year ago");
            }

            if (endDate > DateTime.UtcNow.AddDays(1))
            {
                return (false, "End date cannot be in the future");
            }

            return (true, null);
        }

        public (bool IsValid, string? ErrorMessage) ValidateComparativeAnalysisRequest(ComparativeAnalysisRequest request)
        {
            if (request == null)
            {
                return (false, "Request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(request.ComparisonType))
            {
                return (false, "Comparison type is required");
            }

            var validComparisonTypes = new[] { "WeekOverWeek", "MonthOverMonth", "QuarterOverQuarter", "YearOverYear", "CustomRange" };
            if (!validComparisonTypes.Contains(request.ComparisonType))
            {
                return (false, $"Invalid comparison type. Must be one of: {string.Join(", ", validComparisonTypes)}");
            }

            if (request.ComparisonType == "CustomRange")
            {
                var dateRangeValidation = ValidateDateRange(request.CurrentPeriodStart, request.CurrentPeriodEnd);
                if (!dateRangeValidation.IsValid)
                {
                    return dateRangeValidation;
                }

                dateRangeValidation = ValidateDateRange(request.PreviousPeriodStart ?? DateTime.MinValue, request.PreviousPeriodEnd ?? DateTime.MinValue);
                if (!dateRangeValidation.IsValid)
                {
                    return dateRangeValidation;
                }
            }

            var platformValidation = ValidatePlatform(request.Platform);
            if (!platformValidation.IsValid)
            {
                return platformValidation;
            }

            return (true, null);
        }

        public (bool IsValid, string? ErrorMessage) ValidateChartDataRequest(string userId, int days, string? platform = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "User ID is required");
            }

            var daysValidation = ValidateAnalyticsDays(days);
            if (!daysValidation.IsValid)
            {
                return daysValidation;
            }

            var platformValidation = ValidatePlatform(platform);
            if (!platformValidation.IsValid)
            {
                return platformValidation;
            }

            return (true, null);
        }
    }
}
