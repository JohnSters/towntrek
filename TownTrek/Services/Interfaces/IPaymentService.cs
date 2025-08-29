using TownTrek.Models;

namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for processing payment notifications and handling payment operations
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Processes a payment notification from the payment gateway
        /// </summary>
        Task<PaymentResult> ProcessPaymentNotificationAsync(string paymentId, string paymentStatus, string token);
        
        /// <summary>
        /// Validates the PayFast signature for payment verification
        /// </summary>
        Task<bool> ValidatePayFastSignatureAsync(IFormCollection formData);
        
        /// <summary>
        /// Handles successful payment processing for a user
        /// </summary>
        Task<PaymentResult> HandleSuccessfulPaymentAsync(string userId);
    }

    /// <summary>
    /// Represents the result of a payment operation
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// Indicates whether the payment operation was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Error message if the payment operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// The user associated with the payment operation
        /// </summary>
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Creates a successful payment result
        /// </summary>
        public static PaymentResult Success(ApplicationUser? user = null) => new() { IsSuccess = true, User = user };
        
        /// <summary>
        /// Creates an error payment result with the specified message
        /// </summary>
        public static PaymentResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}