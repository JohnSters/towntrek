using TownTrek.Models;

namespace TownTrek.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentNotificationAsync(string paymentId, string paymentStatus, string token);
        Task<bool> ValidatePayFastSignatureAsync(IFormCollection formData);
        Task<PaymentResult> HandleSuccessfulPaymentAsync(string userId);
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public ApplicationUser? User { get; set; }

        public static PaymentResult Success(ApplicationUser? user = null) => new() { IsSuccess = true, User = user };
        public static PaymentResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}