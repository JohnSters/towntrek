using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Services
{
    public interface IDatabaseErrorLogger
    {
        Task LogErrorAsync(ErrorLogEntry entry);
        Task<ErrorLogStats> GetErrorStatsAsync(TimeSpan period);
        Task<PagedResult<ErrorLogEntry>> GetErrorLogsAsync(ErrorLogFilter filter);
        Task<ErrorLogEntry?> GetErrorByIdAsync(long id);
        Task MarkAsResolvedAsync(long id, string resolvedBy, string? notes = null);
        Task MarkAsUnresolvedAsync(long id);
    }
}