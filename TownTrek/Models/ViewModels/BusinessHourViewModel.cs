namespace TownTrek.Models.ViewModels
{
    public class BusinessHourViewModel
    {
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.
        public string DayName { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = false;
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public bool IsSpecialHours { get; set; } = false;
        public string? SpecialHoursNote { get; set; }
    }
}