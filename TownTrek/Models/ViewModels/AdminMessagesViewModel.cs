namespace TownTrek.Models.ViewModels
{
    public class AdminMessagesViewModel
    {
        public List<AdminMessage> Messages { get; set; } = new();
        public AdminMessageStats Stats { get; set; } = new();
        public AdminMessageFilters Filters { get; set; } = new();
        public List<AdminMessageTopic> Topics { get; set; } = new();
    }
    
    public class AdminMessageStats
    {
        public int TotalMessages { get; set; }
        public int OpenMessages { get; set; }
        public int InProgressMessages { get; set; }
        public int ResolvedMessages { get; set; }
        public int CriticalMessages { get; set; }
        public int HighPriorityMessages { get; set; }
        public int MediumPriorityMessages { get; set; }
        public int LowPriorityMessages { get; set; }
        public int MessagesToday { get; set; }
        public int MessagesThisWeek { get; set; }
    }
    
    public class AdminMessageFilters
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? TopicId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }
    
    public class AdminMessageDetailsViewModel
    {
        public AdminMessage Message { get; set; } = null!;
        public string? ResponseText { get; set; }
        public List<AdminMessage> RelatedMessages { get; set; } = new();
    }
}