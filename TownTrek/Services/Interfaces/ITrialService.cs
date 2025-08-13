using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    public interface ITrialService
    {
        Task<bool> IsTrialValidAsync(string userId);
        Task<int> GetTrialDaysRemainingAsync(string userId);
        Task<bool> ExpireTrialAsync(string userId);
        Task<ApplicationUser?> StartTrialAsync(ApplicationUser user);
        Task<bool> ConvertTrialToSubscriptionAsync(string userId, int subscriptionTierId);
        Task<TrialStatus> GetTrialStatusAsync(string userId);
    }

    public class TrialStatus
    {
        public bool IsTrialUser { get; set; }
        public bool IsExpired { get; set; }
        public int DaysRemaining { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }
}