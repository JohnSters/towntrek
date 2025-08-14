using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class ErrorLogStats
    {
        public int TotalErrors { get; set; }
        public int CriticalErrors { get; set; }
        public int UnresolvedErrors { get; set; }
        public int CriticalErrorsLast24Hours { get; set; }
        public Dictionary<string, int> ErrorsByType { get; set; } = new();
        public Dictionary<string, int> ErrorsBySeverity { get; set; } = new();
        public List<DailyErrorCount> DailyTrends { get; set; } = new();
        public List<RecentErrorActivity> RecentErrors { get; set; } = new();
    }

    public class ErrorLogFilter
    {
        public string? ErrorType { get; set; }
        public string? Severity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsResolved { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;
    }

    public class RecentErrorActivity
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsResolved { get; set; }
    }

    public class DailyErrorCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int CriticalCount { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public static class ErrorClassificationRules
    {
        public static (string Type, string Severity) ClassifyException(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => (ErrorType.Unauthorized.ToString(), ErrorSeverity.Error.ToString()),
                KeyNotFoundException => (ErrorType.NotFound.ToString(), ErrorSeverity.Warning.ToString()),
                ArgumentNullException => (ErrorType.Argument.ToString(), ErrorSeverity.Error.ToString()),
                ArgumentException => (ErrorType.Argument.ToString(), ErrorSeverity.Error.ToString()),
                InvalidOperationException => (ErrorType.Exception.ToString(), ErrorSeverity.Error.ToString()),
                _ => (ErrorType.Exception.ToString(), ErrorSeverity.Critical.ToString())
            };
        }

        public static (string Type, string Severity) ClassifyHttpError(int statusCode)
        {
            return statusCode switch
            {
                401 => (ErrorType.Unauthorized.ToString(), ErrorSeverity.Error.ToString()),
                403 => (ErrorType.Unauthorized.ToString(), ErrorSeverity.Error.ToString()),
                404 => (ErrorType.NotFound.ToString(), ErrorSeverity.Warning.ToString()),
                400 => (ErrorType.Argument.ToString(), ErrorSeverity.Error.ToString()),
                _ => (ErrorType.Api.ToString(), ErrorSeverity.Critical.ToString())
            };
        }
    }
}