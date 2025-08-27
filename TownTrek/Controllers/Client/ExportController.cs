using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Attributes;
using TownTrek.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers.Client
{
    [Authorize(Policy = "PaidClientAccess")] // Only allow paid clients (Basic, Standard, Premium) + Admin
    [Route("Client/[controller]/[action]")]
    public class ExportController(
        IAnalyticsService analyticsService,
        IAnalyticsExportService analyticsExportService,
        IBusinessService businessService,
        IAnalyticsUsageTracker usageTracker,
        ILogger<ExportController> logger) : Controller
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IAnalyticsExportService _analyticsExportService = analyticsExportService;
        private readonly IBusinessService _businessService = businessService;
        private readonly IAnalyticsUsageTracker _usageTracker = usageTracker;
        private readonly ILogger<ExportController> _logger = logger;

        // Export business analytics to PDF
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ExportBusinessPdf(int businessId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                // Verify user owns this business
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any(b => b.Id == businessId))
                {
                    TempData["ErrorMessage"] = "Business not found or access denied.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                var pdfBytes = await _analyticsExportService.ExportBusinessAnalyticsToPdfAsync(businessId, userId, fromDate, toDate);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ExportBusinessPdf", TimeSpan.FromMilliseconds(200));
                
                return File(pdfBytes, "application/pdf", $"business-analytics-{businessId}-{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting business PDF for business {BusinessId} and user {UserId}", businessId, userId);
                TempData["ErrorMessage"] = "Unable to export PDF. Please try again later.";
                return RedirectToAction("Index", "ClientAnalytics");
            }
        }

        // Export overview analytics to PDF
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ExportOverviewPdf(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    TempData["ErrorMessage"] = "No businesses found.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                var pdfBytes = await _analyticsExportService.ExportOverviewAnalyticsToPdfAsync(userId, fromDate, toDate);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ExportOverviewPdf", TimeSpan.FromMilliseconds(200));
                
                return File(pdfBytes, "application/pdf", $"analytics-overview-{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting overview PDF for user {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to export PDF. Please try again later.";
                return RedirectToAction("Index", "ClientAnalytics");
            }
        }

        // Export data to CSV
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ExportCsv(string dataType, DateTime? fromDate = null, DateTime? toDate = null, int? businessId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    TempData["ErrorMessage"] = "No businesses found.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                // If businessId is specified, verify user owns it
                if (businessId.HasValue && !userBusinesses.Any(b => b.Id == businessId.Value))
                {
                    TempData["ErrorMessage"] = "Business not found or access denied.";
                    return RedirectToAction("Index", "ClientAnalytics");
                }

                var csvBytes = await _analyticsExportService.ExportDataToCsvAsync(dataType, userId, fromDate, toDate, businessId);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ExportCsv", TimeSpan.FromMilliseconds(100));
                
                return File(csvBytes, "text/csv", $"{dataType}-{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV for data type {DataType} and user {UserId}", dataType, userId);
                TempData["ErrorMessage"] = "Unable to export CSV. Please try again later.";
                return RedirectToAction("Index", "ClientAnalytics");
            }
        }

        // Generate shareable link
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> GenerateShareableLink(string dashboardType, int? businessId = null, DateTime? expiresAt = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                // If businessId is specified, verify user owns it
                if (businessId.HasValue && !userBusinesses.Any(b => b.Id == businessId.Value))
                {
                    return Json(new { success = false, message = "Business not found or access denied." });
                }

                var shareableLink = await _analyticsExportService.GenerateShareableLinkAsync(dashboardType, userId, businessId, expiresAt);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "GenerateShareableLink", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, link = shareableLink });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shareable link for dashboard type {DashboardType} and user {UserId}", dashboardType, userId);
                return Json(new { success = false, message = "Unable to generate shareable link." });
            }
        }

        // Schedule email report
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> ScheduleEmailReport(string reportType, string frequency, int? businessId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                // If businessId is specified, verify user owns it
                if (businessId.HasValue && !userBusinesses.Any(b => b.Id == businessId.Value))
                {
                    return Json(new { success = false, message = "Business not found or access denied." });
                }

                var scheduled = await _analyticsExportService.ScheduleEmailReportAsync(reportType, frequency, userId, businessId);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "ScheduleEmailReport", TimeSpan.FromMilliseconds(50));
                
                return Json(new { success = true, scheduled = scheduled });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling email report for report type {ReportType} and user {UserId}", reportType, userId);
                return Json(new { success = false, message = "Unable to schedule email report." });
            }
        }

        // Send email report
        [EnableRateLimiting("AnalyticsRateLimit")]
        public async Task<IActionResult> SendEmailReport(string reportType, int? businessId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            try
            {
                var userBusinesses = await _businessService.GetUserBusinessesAsync(userId);
                if (!userBusinesses.Any())
                {
                    return Json(new { success = false, message = "No businesses found." });
                }

                // If businessId is specified, verify user owns it
                if (businessId.HasValue && !userBusinesses.Any(b => b.Id == businessId.Value))
                {
                    return Json(new { success = false, message = "Business not found or access denied." });
                }

                var sent = await _analyticsExportService.SendEmailReportAsync(reportType, userId, businessId, fromDate, toDate);
                
                // Track usage
                await _usageTracker.TrackFeatureUsageAsync(userId, "SendEmailReport", TimeSpan.FromMilliseconds(100));
                
                return Json(new { success = true, sent = sent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email report for report type {ReportType} and user {UserId}", reportType, userId);
                return Json(new { success = false, message = "Unable to send email report." });
            }
        }
    }
}
