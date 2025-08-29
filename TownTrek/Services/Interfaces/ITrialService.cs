using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for managing trial user accounts and trial-related operations
    /// </summary>
    public interface ITrialService
    {
        /// <summary>
        /// Checks if a user's trial is still valid
        /// </summary>
        Task<bool> IsTrialValidAsync(string userId);
        
        /// <summary>
        /// Gets the number of trial days remaining for a user
        /// </summary>
        Task<int> GetTrialDaysRemainingAsync(string userId);
        
        /// <summary>
        /// Expires a user's trial
        /// </summary>
        Task<bool> ExpireTrialAsync(string userId);
        
        /// <summary>
        /// Starts a trial for a new user
        /// </summary>
        Task<ApplicationUser?> StartTrialAsync(ApplicationUser user);
        
        /// <summary>
        /// Converts a trial user to a paid subscription
        /// </summary>
        Task<bool> ConvertTrialToSubscriptionAsync(string userId, int subscriptionTierId);
        
        /// <summary>
        /// Gets the complete trial status for a user
        /// </summary>
        Task<TrialStatus> GetTrialStatusAsync(string userId);
    }

    /// <summary>
    /// Represents the trial status of a user
    /// </summary>
    public class TrialStatus
    {
        /// <summary>
        /// Indicates whether the user is a trial user
        /// </summary>
        public bool IsTrialUser { get; set; }
        
        /// <summary>
        /// Indicates whether the trial has expired
        /// </summary>
        public bool IsExpired { get; set; }
        
        /// <summary>
        /// Number of days remaining in the trial
        /// </summary>
        public int DaysRemaining { get; set; }
        
        /// <summary>
        /// The date when the trial ends
        /// </summary>
        public DateTime? TrialEndDate { get; set; }
        
        /// <summary>
        /// Status message describing the trial state
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;
    }
}