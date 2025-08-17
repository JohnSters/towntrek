using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Api
{
    [Route("Api/[controller]/[action]")]
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPaymentService _paymentService;

        public PaymentController(
            ILogger<PaymentController> logger,
            SignInManager<ApplicationUser> signInManager,
            IPaymentService paymentService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Success(string? paymentId = null)
        {
            // Prefer TempData (normal redirect flow), fallback to paymentId via return_url
            var userId = TempData["PendingUserId"]?.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await _paymentService.HandleSuccessfulPaymentAsync(userId);
                if (result.IsSuccess && result.User != null)
                {
                    await _signInManager.SignInAsync(result.User, isPersistent: false);
                    TempData["SuccessMessage"] = "Payment successful! Your subscription is now active.";
                    return RedirectToAction("Dashboard", "Client");
                }
            }

            // If PayFast returned with a paymentId but TempData is not available, try to sign the user in
            if (!string.IsNullOrWhiteSpace(paymentId))
            {
                // The IPN will activate the subscription; here we can only redirect with a generic success.
                TempData["SuccessMessage"] = "Payment completed successfully! Your subscription will be activated shortly.";
                return RedirectToAction("Login", "Auth");
            }

            TempData["SuccessMessage"] = "Payment completed successfully!";
            return RedirectToAction("Index", "Home");
        }

        // Helper endpoint to display exactly what we are sending to PayFast for diagnostics (DEV only)
        [HttpGet]
        public async Task<IActionResult> DebugFields(string userId)
        {
            var reg = HttpContext.RequestServices.GetRequiredService<IRegistrationService>();
            var fields = await reg.BuildPayFastFormFieldsForUserAsync(userId);
            if (fields == null) return NotFound("No pending subscription found");
            return Json(fields);
        }

        [HttpGet]
        public async Task<IActionResult> DebugFieldsByPayment(int paymentId)
        {
            var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var sub = await db.Subscriptions
                .Include(s => s.SubscriptionTier)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == paymentId);
            if (sub?.SubscriptionTier == null || sub.User == null) return NotFound("Subscription not found");

            var reg = HttpContext.RequestServices.GetRequiredService<IRegistrationService>();
            var fields = await reg.BuildPayFastFormFieldsAsync(sub.SubscriptionTier, sub.User, sub.Id);
            return Json(fields);
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            TempData["ErrorMessage"] = "Payment was cancelled. You can try again later from your account settings.";
            return RedirectToAction("Register", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> Process(int subscriptionId)
        {
            // Build a server-side POST to PayFast to avoid manual URL issues
            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var subscription = await db.Subscriptions
                    .Include(s => s.SubscriptionTier)
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == subscriptionId);

                if (subscription?.SubscriptionTier != null && subscription.User != null)
                {
                    var registrationService = HttpContext.RequestServices.GetRequiredService<IRegistrationService>();
                    var fields = await registrationService.BuildPayFastFormFieldsAsync(subscription.SubscriptionTier, subscription.User, subscription.Id);
                    var payFastUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                        ["PayFast:PaymentUrl"] ?? "https://sandbox.payfast.co.za/eng/process";

                    return View("AutoPost", new TownTrek.Models.ViewModels.PayFast.PayFastPostViewModel
                    {
                        ActionUrl = payFastUrl,
                        Fields = fields
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment retry for subscription {SubscriptionId}", subscriptionId);
            }

            TempData["ErrorMessage"] = "Unable to process payment. Please contact support.";
            return RedirectToAction(actionName: "Index", controllerName: "Subscription");
        }

        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            try
            {
                var formData = await Request.ReadFormAsync();
                
                // Validate PayFast signature for security
                if (!await _paymentService.ValidatePayFastSignatureAsync(formData))
                {
                    _logger.LogWarning("Invalid PayFast signature received");
                    return BadRequest("Invalid signature");
                }

                var paymentId = formData["m_payment_id"].ToString();
                var paymentStatus = formData["payment_status"].ToString();
                var token = formData["token"].ToString();

                _logger.LogInformation("PayFast notification received: PaymentId={PaymentId}, Status={Status}", 
                    paymentId, paymentStatus);

                var result = await _paymentService.ProcessPaymentNotificationAsync(paymentId, paymentStatus, token);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Payment processing failed: {Error}", result.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayFast notification");
                return Ok(); // Still return OK to prevent PayFast retries
            }
        }

        // DEV: trigger a fake successful IPN to verify activation/email flow end-to-end
        [HttpGet]
        public async Task<IActionResult> NotifyDev(int paymentId)
        {
            var result = await _paymentService.ProcessPaymentNotificationAsync(paymentId.ToString(), "COMPLETE", "DEV-TOKEN");
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Payment (DEV) marked complete and subscription activated.";
                return RedirectToAction("Login", "Auth");
            }
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to process DEV notification.";
            return RedirectToAction("Login", "Auth");
        }
    }
}