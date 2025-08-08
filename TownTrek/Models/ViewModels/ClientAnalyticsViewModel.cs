using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class ClientAnalyticsViewModel
    {
        public List<Business> Businesses { get; set; } = new();
        public int TotalViews { get; set; }
    }
}