namespace TownTrek.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalTowns { get; set; }
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalUsers { get; set; }
        public int NewBusinessesThisMonth { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalPopulation { get; set; }
        public int TownsWithLandmarks { get; set; }
        
        // Error logging statistics
        public int CriticalErrorsLast24Hours { get; set; }
        public int UnresolvedErrorsTotal { get; set; }
        public List<RecentErrorActivity> RecentErrors { get; set; } = new();
        public ErrorLogStats? ErrorStats { get; set; }
    }
}