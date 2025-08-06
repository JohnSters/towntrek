using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Controllers
{
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
        public async Task<IActionResult> Success()
        {
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

            TempData["SuccessMessage"] = "Payment completed successfully!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            TempData["ErrorMessage"] = "Payment was cancelled. You can try again later from your account settings.";
            return RedirectToAction("Register", "Auth");
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
    }
}