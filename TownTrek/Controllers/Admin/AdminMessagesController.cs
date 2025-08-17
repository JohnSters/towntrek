using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Messages/[action]")]
    public class AdminMessagesController : Controller
    {
        private readonly IAdminMessageService _adminMessageService;
        private readonly ILogger<AdminMessagesController> _logger;

        public AdminMessagesController(
            IAdminMessageService adminMessageService,
            ILogger<AdminMessagesController> logger)
        {
            _adminMessageService = adminMessageService;
            _logger = logger;
        }

        // GET: Admin/Messages/Index
        [HttpGet]
        public async Task<IActionResult> Index(AdminMessageFilters? filters)
        {
            try
            {
                var model = await _adminMessageService.GetAdminMessagesViewModelAsync(filters);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin messages");
                TempData["ErrorMessage"] = "Error loading messages. Please try again.";
                return View(new AdminMessagesViewModel());
            }
        }

        // GET: Admin/Messages/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _adminMessageService.GetMessageDetailsAsync(id);
                return View(model);
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = "Message not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading message details for ID {MessageId}", id);
                TempData["ErrorMessage"] = "Error loading message details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Messages/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int messageId, string status)
        {
            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _adminMessageService.UpdateMessageStatusAsync(messageId, status, adminUserId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Message status updated to {status}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update message status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message status for ID {MessageId}", messageId);
                TempData["ErrorMessage"] = "Error updating message status. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = messageId });
        }

        // POST: Admin/Messages/Respond
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int messageId, string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                TempData["ErrorMessage"] = "Response text is required.";
                return RedirectToAction(nameof(Details), new { id = messageId });
            }

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var success = await _adminMessageService.RespondToMessageAsync(messageId, responseText, adminUserId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Response sent successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to send response.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to message ID {MessageId}", messageId);
                TempData["ErrorMessage"] = "Error sending response. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = messageId });
        }

        // POST: Admin/Messages/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int messageId)
        {
            try
            {
                var success = await _adminMessageService.DeleteMessageAsync(messageId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Message deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete message.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message ID {MessageId}", messageId);
                TempData["ErrorMessage"] = "Error deleting message. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get message stats for dashboard
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _adminMessageService.GetMessageStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message stats");
                return Json(new { error = "Failed to load stats" });
            }
        }

        // AJAX: Bulk actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(string action, int[] messageIds)
        {
            if (messageIds == null || messageIds.Length == 0)
            {
                return Json(new { success = false, message = "No messages selected" });
            }

            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var successCount = 0;

                foreach (var messageId in messageIds)
                {
                    bool success = action.ToLower() switch
                    {
                        "resolve" => await _adminMessageService.UpdateMessageStatusAsync(messageId, "Resolved", adminUserId),
                        "close" => await _adminMessageService.UpdateMessageStatusAsync(messageId, "Closed", adminUserId),
                        "delete" => await _adminMessageService.DeleteMessageAsync(messageId),
                        _ => false
                    };

                    if (success) successCount++;
                }

                return Json(new 
                { 
                    success = true, 
                    message = $"{successCount} of {messageIds.Length} messages processed successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk action {Action} on messages", action);
                return Json(new { success = false, message = "Error performing bulk action" });
            }
        }
    }
}