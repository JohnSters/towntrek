using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.Exceptions;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TownTrek.Services.ClientAnalytics
{
    public class ClientAnalyticsExportService(
        ApplicationDbContext context,
        IClientAnalyticsService analyticsService,
        IAnalyticsCacheService analyticsCacheService,
        IEmailService emailService,
        IAnalyticsErrorHandler errorHandler,
        ILogger<ClientAnalyticsExportService> logger) : IAnalyticsExportService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IClientAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsCacheService _analyticsCacheService = analyticsCacheService;
        private readonly IEmailService _emailService = emailService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<ClientAnalyticsExportService> _logger = logger;

        public async Task<byte[]> GenerateBusinessAnalyticsPdfAsync(int businessId, string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Verify user owns this business
                var business = await _context.Businesses
                    .Include(b => b.Town)
                    .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);

                if (business == null)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        "Business not found or access denied",
                        userId,
                        "BusinessAccess",
                        "AccessDenied",
                        new Dictionary<string, object> { ["BusinessId"] = businessId }
                    );
                    throw new AnalyticsValidationException("Business not found or access denied", "BusinessAccess", "AccessDenied");
                }

                // Get analytics data
                var analytics = await _analyticsCacheService.GetBusinessAnalyticsAsync(businessId, userId);
                if (analytics == null)
                {
                    throw new AnalyticsValidationException("Analytics data not found", "Analytics", "NotFound");
                }
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                using var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Add header
                var header = new Paragraph($"Analytics Report - {business.Name}")
                    .SetFontSize(24)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(header);

                // Add report info
                var reportInfo = new Paragraph($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(reportInfo);

                var dateRange = new Paragraph($"Date Range: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(dateRange);

                document.Add(new Paragraph(" ")); // Spacing

                // Add business overview
                var overviewHeader = new Paragraph("Business Overview")
                    .SetFontSize(16)
                    .SetBold();
                document.Add(overviewHeader);

                var overviewTable = new Table(2);
                overviewTable.AddCell(new Cell().Add(new Paragraph("Business Name")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(business.Name)));
                overviewTable.AddCell(new Cell().Add(new Paragraph("Category")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(business.Category)));
                overviewTable.AddCell(new Cell().Add(new Paragraph("Town")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(business.Town?.Name ?? "N/A")));
                overviewTable.AddCell(new Cell().Add(new Paragraph("Phone")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(business.PhoneNumber)));
                document.Add(overviewTable);

                document.Add(new Paragraph(" ")); // Spacing

                // Add analytics metrics
                var metricsHeader = new Paragraph("Analytics Metrics")
                    .SetFontSize(16)
                    .SetBold();
                document.Add(metricsHeader);

                var metricsTable = new Table(2);
                metricsTable.AddCell(new Cell().Add(new Paragraph("Total Views")).SetBold());
                metricsTable.AddCell(new Cell().Add(new Paragraph(analytics.TotalViews.ToString("N0"))));
                metricsTable.AddCell(new Cell().Add(new Paragraph("Total Reviews")).SetBold());
                metricsTable.AddCell(new Cell().Add(new Paragraph(analytics.TotalReviews.ToString("N0"))));
                metricsTable.AddCell(new Cell().Add(new Paragraph("Average Rating")).SetBold());
                metricsTable.AddCell(new Cell().Add(new Paragraph(analytics.AverageRating.ToString("F1"))));
                metricsTable.AddCell(new Cell().Add(new Paragraph("Total Favorites")).SetBold());
                metricsTable.AddCell(new Cell().Add(new Paragraph(analytics.TotalFavorites.ToString("N0"))));
                metricsTable.AddCell(new Cell().Add(new Paragraph("Engagement Score")).SetBold());
                metricsTable.AddCell(new Cell().Add(new Paragraph(analytics.EngagementScore.ToString("F2"))));
                document.Add(metricsTable);

                document.Add(new Paragraph(" ")); // Spacing

                // Add recommendations
                if (analytics.Recommendations.Any())
                {
                    var recommendationsHeader = new Paragraph("Recommendations")
                        .SetFontSize(16)
                        .SetBold();
                    document.Add(recommendationsHeader);

                    foreach (var recommendation in analytics.Recommendations.Take(5))
                    {
                        var recommendationText = new Paragraph($"• {recommendation}")
                            .SetFontSize(10);
                        document.Add(recommendationText);
                    }
                }

                document.Close();

                return memoryStream.ToArray();
            }, userId, "GenerateBusinessAnalyticsPdf", new Dictionary<string, object> { ["BusinessId"] = businessId, ["FromDate"] = fromDate!, ["ToDate"] = toDate! });
        }

        public async Task<byte[]> GenerateClientAnalyticsPdfAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Get analytics data
                var analytics = await _analyticsCacheService.GetClientAnalyticsAsync(userId);
                if (analytics == null)
                {
                    throw new AnalyticsValidationException("Analytics data not found", "Analytics", "NotFound");
                }
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                using var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Add header
                var header = new Paragraph("Analytics Overview Report")
                    .SetFontSize(24)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(header);

                // Add report info
                var reportInfo = new Paragraph($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(reportInfo);

                var dateRange = new Paragraph($"Date Range: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(dateRange);

                document.Add(new Paragraph(" ")); // Spacing

                // Add overview metrics
                var overviewHeader = new Paragraph("Overview Metrics")
                    .SetFontSize(16)
                    .SetBold();
                document.Add(overviewHeader);

                var overviewTable = new Table(2);
                overviewTable.AddCell(new Cell().Add(new Paragraph("Total Businesses")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(analytics.Businesses.Count.ToString())));
                overviewTable.AddCell(new Cell().Add(new Paragraph("Total Views")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(analytics.Overview.TotalViews.ToString("N0"))));
                overviewTable.AddCell(new Cell().Add(new Paragraph("Total Reviews")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(analytics.Overview.TotalReviews.ToString("N0"))));
                overviewTable.AddCell(new Cell().Add(new Paragraph("Overall Rating")).SetBold());
                overviewTable.AddCell(new Cell().Add(new Paragraph(analytics.Overview.OverallRating.ToString("F1"))));
                document.Add(overviewTable);

                document.Add(new Paragraph(" ")); // Spacing

                // Add business performance
                if (analytics.BusinessAnalytics.Any())
                {
                    var businessHeader = new Paragraph("Business Performance")
                        .SetFontSize(16)
                        .SetBold();
                    document.Add(businessHeader);

                    var businessTable = new Table(4);
                    businessTable.AddHeaderCell(new Cell().Add(new Paragraph("Business")).SetBold());
                    businessTable.AddHeaderCell(new Cell().Add(new Paragraph("Views")).SetBold());
                    businessTable.AddHeaderCell(new Cell().Add(new Paragraph("Reviews")).SetBold());
                    businessTable.AddHeaderCell(new Cell().Add(new Paragraph("Rating")).SetBold());

                    foreach (var business in analytics.BusinessAnalytics.Take(10))
                    {
                        businessTable.AddCell(new Cell().Add(new Paragraph(business.BusinessName)));
                        businessTable.AddCell(new Cell().Add(new Paragraph(business.TotalViews.ToString("N0"))));
                        businessTable.AddCell(new Cell().Add(new Paragraph(business.TotalReviews.ToString("N0"))));
                        businessTable.AddCell(new Cell().Add(new Paragraph(business.AverageRating.ToString("F1"))));
                    }
                    document.Add(businessTable);
                }

                document.Close();

                return memoryStream.ToArray();
            }, userId, "PDFGeneration", new Dictionary<string, object>
            {
                ["FromDate"] = fromDate!,
                ["ToDate"] = toDate!
            });
        }

        public async Task<byte[]> ExportAnalyticsCsvAsync(string userId, string dataType, DateTime? fromDate = null, DateTime? toDate = null, int? businessId = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                switch (dataType.ToLower())
                {
                    case "views":
                        await ExportViewsDataAsync(csv, userId, fromDate.Value, toDate.Value, businessId);
                        break;
                    case "reviews":
                        await ExportReviewsDataAsync(csv, userId, fromDate.Value, toDate.Value, businessId);
                        break;
                    case "performance":
                        await ExportPerformanceDataAsync(csv, userId, fromDate.Value, toDate.Value, businessId);
                        break;
                    default:
                        await _errorHandler.HandleValidationExceptionAsync(
                            $"Unsupported data type: {dataType}",
                            userId,
                            "DataExport",
                            "UnsupportedDataType",
                            new Dictionary<string, object> { ["DataType"] = dataType! }
                        );
                        throw new AnalyticsValidationException($"Unsupported data type: {dataType}", "DataExport", "UnsupportedDataType");
                }

                writer.Flush();
                return memoryStream.ToArray();
            }, userId, "ExportAnalyticsCsv", new Dictionary<string, object>
            {
                ["DataType"] = dataType!,
                ["FromDate"] = fromDate!,
                ["ToDate"] = toDate!,
                ["BusinessId"] = businessId!
            });
        }

        public async Task<string> GenerateShareableLinkAsync(string userId, string dashboardType, int? businessId = null, DateTime? expiresAt = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Verify user owns the business if specified
                if (businessId.HasValue)
                {
                    var business = await _context.Businesses
                        .FirstOrDefaultAsync(b => b.Id == businessId.Value && b.UserId == userId);
                    if (business == null)
                    {
                        await _errorHandler.HandleValidationExceptionAsync(
                            "Business not found or access denied",
                            userId,
                            "BusinessAccess",
                            "AccessDenied",
                            new Dictionary<string, object> { ["BusinessId"] = businessId.Value }
                        );
                        throw new AnalyticsValidationException("Business not found or access denied", "BusinessAccess", "AccessDenied");
                    }
                }

                // Generate unique token
                var token = GenerateSecureToken();

                // Set default expiration (30 days)
                expiresAt ??= DateTime.UtcNow.AddDays(30);

                var shareableLink = new AnalyticsShareableLink
                {
                    UserId = userId,
                    LinkToken = token,
                    DashboardType = dashboardType,
                    BusinessId = businessId,
                    ExpiresAt = expiresAt,
                    IsActive = true
                };

                _context.AnalyticsShareableLinks.Add(shareableLink);
                await _context.SaveChangesAsync();

                return token;
            }, userId, "GenerateShareableLink", new Dictionary<string, object>
            {
                ["DashboardType"] = dashboardType!,
                ["BusinessId"] = businessId!,
                ["ExpiresAt"] = expiresAt!
            });
        }

        public async Task<bool> ValidateShareableLinkAsync(string linkToken)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(linkToken))
                {
                    throw new AnalyticsValidationException("Link token cannot be null or empty", "LinkToken", "Required");
                }

                var link = await _context.AnalyticsShareableLinks
                    .FirstOrDefaultAsync(l => l.LinkToken == linkToken && l.IsActive);

                if (link == null)
                    return false;

                // Check if expired
                if (link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow)
                {
                    link.IsActive = false;
                    await _context.SaveChangesAsync();
                    return false;
                }

                // Update access count and last accessed
                link.AccessCount++;
                link.LastAccessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }, string.Empty, "ValidateShareableLink", new Dictionary<string, object>
            {
                ["LinkToken"] = linkToken!
            });
        }

        public async Task<object?> GetShareableLinkDataAsync(string linkToken)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync<object?>(async () =>
            {
                if (string.IsNullOrWhiteSpace(linkToken))
                {
                    throw new AnalyticsValidationException("Link token cannot be null or empty", "LinkToken", "Required");
                }

                var link = await _context.AnalyticsShareableLinks
                    .Include(l => l.User)
                    .Include(l => l.Business)
                    .FirstOrDefaultAsync(l => l.LinkToken == linkToken && l.IsActive);

                if (link == null || link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow)
                    return null;

                // Get analytics data based on dashboard type
                switch (link.DashboardType.ToLower())
                {
                    case "overview":
                        return await _analyticsCacheService.GetClientAnalyticsAsync(link.UserId);
                    case "business":
                        if (link.BusinessId.HasValue)
                            return await _analyticsCacheService.GetBusinessAnalyticsAsync(link.BusinessId.Value, link.UserId);
                        break;
                    case "benchmarks":
                        // Return basic benchmark data (without sensitive information)
                        return new { message = "Benchmark data available" };
                    case "competitors":
                        return await _analyticsCacheService.GetCompetitorInsightsAsync(link.UserId);
                }

                return null;
            }, string.Empty, "GetShareableLinkData", new Dictionary<string, object>
            {
                ["LinkToken"] = linkToken!
            });
        }

        public async Task<bool> ScheduleEmailReportAsync(string userId, string reportType, string frequency, int? businessId = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync<bool>(async () =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AnalyticsValidationException("User ID cannot be null or empty", "UserId", "Required");
                }

                if (string.IsNullOrWhiteSpace(reportType))
                {
                    throw new AnalyticsValidationException("Report type cannot be null or empty", "ReportType", "Required");
                }

                if (string.IsNullOrWhiteSpace(frequency))
                {
                    throw new AnalyticsValidationException("Frequency cannot be null or empty", "Frequency", "Required");
                }

                // Verify user owns the business if specified
                if (businessId.HasValue)
                {
                    var business = await _context.Businesses
                        .FirstOrDefaultAsync(b => b.Id == businessId.Value && b.UserId == userId);
                    if (business == null)
                    {
                        throw new AnalyticsValidationException("Business not found or access denied", "BusinessAccess", "AccessDenied");
                    }
                }

                // Calculate next scheduled time
                var nextScheduledAt = frequency.ToLower() switch
                {
                    "daily" => DateTime.UtcNow.AddDays(1).Date.AddHours(9), // 9 AM UTC
                    "weekly" => DateTime.UtcNow.AddDays(7).Date.AddHours(9),
                    "monthly" => DateTime.UtcNow.AddMonths(1).Date.AddHours(9),
                    "once" => DateTime.UtcNow.AddHours(1), // Send in 1 hour
                                            _ => throw new AnalyticsValidationException($"Unsupported frequency: {frequency}", "Frequency", "Unsupported")
                };

                var emailReport = new AnalyticsEmailReport
                {
                    UserId = userId,
                    ReportType = reportType,
                    Frequency = frequency,
                    BusinessId = businessId,
                    NextScheduledAt = nextScheduledAt,
                    IsActive = true
                };

                _context.AnalyticsEmailReports.Add(emailReport);
                await _context.SaveChangesAsync();

                return true;
            }, userId, "ScheduleEmailReport", new Dictionary<string, object>
            {
                ["ReportType"] = reportType!,
                ["Frequency"] = frequency!,
                ["BusinessId"] = businessId!
            });
        }

        public async Task<bool> SendEmailReportAsync(string userId, string reportType, int? businessId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync<bool>(async () =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AnalyticsValidationException("User ID cannot be null or empty", "UserId", "Required");
                }

                if (string.IsNullOrWhiteSpace(reportType))
                {
                    throw new AnalyticsValidationException("Report type cannot be null or empty", "ReportType", "Required");
                }

                // Get user email
                var user = await _context.Users.FindAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    throw new AnalyticsValidationException("User not found or email not available", "UserEmail", "NotFound");
                }

                byte[] reportData;
                string subject;
                string fileName;

                switch (reportType.ToLower())
                {
                    case "business":
                        if (!businessId.HasValue)
                        {
                            throw new AnalyticsValidationException("Business ID is required for business reports", "BusinessId", "Required");
                        }
                        reportData = await GenerateBusinessAnalyticsPdfAsync(businessId.Value, userId, fromDate, toDate);
                        subject = "Business Analytics Report";
                        fileName = $"business-analytics-{DateTime.UtcNow:yyyy-MM-dd}.pdf";
                        break;
                    case "overview":
                        reportData = await GenerateClientAnalyticsPdfAsync(userId, fromDate, toDate);
                        subject = "Analytics Overview Report";
                        fileName = $"analytics-overview-{DateTime.UtcNow:yyyy-MM-dd}.pdf";
                        break;
                    default:
                        throw new AnalyticsValidationException($"Unsupported report type: {reportType}", "ReportType", "Unsupported");
                }

                // Send email with PDF attachment
                var emailSent = await _emailService.SendEmailWithAttachmentAsync(
                    user.Email!,
                    subject,
                    "Please find your analytics report attached.",
                    reportData,
                    fileName,
                    "application/pdf"
                );

                if (emailSent)
                {
                    // Update email report record
                    var emailReport = await _context.AnalyticsEmailReports
                        .FirstOrDefaultAsync(r => r.UserId == userId && r.ReportType == reportType && r.IsActive);

                    if (emailReport != null)
                    {
                        emailReport.LastSentAt = DateTime.UtcNow;
                        emailReport.SendCount++;
                        await _context.SaveChangesAsync();
                    }
                }

                return emailSent;
            }, userId, "SendEmailReport", new Dictionary<string, object>
            {
                ["ReportType"] = reportType!,
                ["BusinessId"] = businessId!,
                ["FromDate"] = fromDate!,
                ["ToDate"] = toDate!
            });
        }

        private async Task ExportViewsDataAsync(CsvWriter csv, string userId, DateTime fromDate, DateTime toDate, int? businessId)
        {
            var query = _context.BusinessViewLogs
                .Include(v => v.Business)
                .Where(v => v.Business.UserId == userId && v.ViewedAt >= fromDate && v.ViewedAt <= toDate);

            if (businessId.HasValue)
                query = query.Where(v => v.BusinessId == businessId.Value);

            var views = await query
                .OrderBy(v => v.ViewedAt)
                .Select(v => new
                {
                    v.ViewedAt.Date,
                    BusinessName = v.Business.Name,
                    v.Platform,
                    v.UserAgent,
                    v.IpAddress
                })
                .ToListAsync();

            csv.WriteRecords(views);
        }

        private async Task ExportReviewsDataAsync(CsvWriter csv, string userId, DateTime fromDate, DateTime toDate, int? businessId)
        {
            var query = _context.BusinessReviews
                .Include(r => r.Business)
                .Where(r => r.Business.UserId == userId && r.CreatedAt >= fromDate && r.CreatedAt <= toDate && r.IsActive);

            if (businessId.HasValue)
                query = query.Where(r => r.BusinessId == businessId.Value);

            var reviews = await query
                .Include(r => r.User)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    r.CreatedAt.Date,
                    BusinessName = r.Business.Name,
                    r.Rating,
                    r.Comment,
                    ReviewerName = r.User.UserName ?? "Unknown"
                })
                .ToListAsync();

            csv.WriteRecords(reviews);
        }

        private async Task ExportPerformanceDataAsync(CsvWriter csv, string userId, DateTime fromDate, DateTime toDate, int? businessId)
        {
            var query = _context.AnalyticsSnapshots
                .Include(s => s.Business)
                .Where(s => s.Business.UserId == userId && s.SnapshotDate >= fromDate.Date && s.SnapshotDate <= toDate.Date);

            if (businessId.HasValue)
                query = query.Where(s => s.BusinessId == businessId.Value);

            var snapshots = await query
                .OrderBy(s => s.SnapshotDate)
                .Select(s => new
                {
                    Date = s.SnapshotDate,
                    BusinessName = s.Business.Name,
                    s.TotalViews,
                    s.TotalReviews,
                    s.TotalFavorites,
                    s.AverageRating,
                    s.EngagementScore
                })
                .ToListAsync();

            csv.WriteRecords(snapshots);
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 32);
        }

        // Additional methods needed by controllers (aliases for existing methods)
        public async Task<byte[]> ExportBusinessAnalyticsToPdfAsync(int businessId, string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync<byte[]>(async () =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AnalyticsValidationException("User ID cannot be null or empty", "UserId", "Required");
                }

                if (businessId <= 0)
                {
                    throw new AnalyticsValidationException("Business ID must be greater than zero", "BusinessId", "Invalid");
                }

                return await GenerateBusinessAnalyticsPdfAsync(businessId, userId, fromDate, toDate);
            }, userId, "ExportBusinessAnalyticsToPdf", new Dictionary<string, object>
            {
                ["BusinessId"] = businessId,
                ["FromDate"] = fromDate!,
                ["ToDate"] = toDate!
            });
        }

        public async Task<byte[]> ExportOverviewAnalyticsToPdfAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync<byte[]>(async () =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AnalyticsValidationException("User ID cannot be null or empty", "UserId", "Required");
                }

                return await GenerateClientAnalyticsPdfAsync(userId, fromDate, toDate);
            }, userId, "ExportOverviewAnalyticsToPdf", new Dictionary<string, object>
            {
                ["FromDate"] = fromDate!,
                ["ToDate"] = toDate!
            });
        }

        public async Task<byte[]> ExportDataToCsvAsync(string userId, string dataType, DateTime? fromDate = null, DateTime? toDate = null, int? businessId = null)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync<byte[]>(async () =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new AnalyticsValidationException("User ID cannot be null or empty", "UserId", "Required");
                }

                if (string.IsNullOrWhiteSpace(dataType))
                {
                    throw new AnalyticsValidationException("Data type cannot be null or empty", "DataType", "Required");
                }

                return await ExportAnalyticsCsvAsync(userId, dataType, fromDate, toDate, businessId);
            }, userId, "ExportDataToCsv", new Dictionary<string, object>
            {
                ["DataType"] = dataType!,
                ["FromDate"] = fromDate!,
                ["ToDate"] = toDate!,
                ["BusinessId"] = businessId!
            });
        }
    }
}
