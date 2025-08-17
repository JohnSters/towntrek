using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using TownTrek.Models;
using TownTrek.Options;
using TownTrek.Services.Interfaces;
using TownTrek.Models.ViewModels.Emails;

namespace TownTrek.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailOptions _options;
        private readonly IEmailTemplateRenderer _templateRenderer;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailOptions> options, IEmailTemplateRenderer templateRenderer)
        {
            _logger = logger;
            _options = options.Value;
            _templateRenderer = templateRenderer;
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "Welcome to TownTrek";
            var html = await _templateRenderer.RenderAsync("/Views/Emails/WelcomeEmail.cshtml", new WelcomeEmailViewModel { FirstName = firstName });
            await SendAsync(email, subject, html, isHtml: true);
        }

        public async Task SendEmailConfirmationAsync(string email, string firstName, string confirmationUrl)
        {
            var subject = "Confirm your email";
            var html = await _templateRenderer.RenderAsync("/Views/Emails/ConfirmEmail.cshtml", new ConfirmEmailViewModel { FirstName = firstName, ConfirmationUrl = confirmationUrl });
            await SendAsync(email, subject, html, isHtml: true);
        }

        public async Task SendPriceChangeNotificationAsync(string email, string firstName, string tierName, decimal oldPrice, decimal newPrice, DateTime effectiveDate)
        {
            var subject = $"Price change for {tierName}";
            var body = $"Hi {firstName},\n\nThe price for your {tierName} plan will change from R{oldPrice:F2} to R{newPrice:F2} effective {effectiveDate:yyyy-MM-dd}.\n\nIf you have questions, reply to this email.\n\n— {_options.FromName}";
            await SendAsync(email, subject, body);
        }

        public async Task SendSubscriptionExpiredEmailAsync(string email, string firstName)
        {
            var subject = "Your subscription has expired";
            var html = "<p>Your subscription has expired. Please renew to continue enjoying TownTrek features.</p>"; // Simple placeholder
            await SendAsync(email, subject, html, isHtml: true);
        }

        public async Task SendPaymentFailedReminderAsync(Subscription subscription)
        {
            var subject = "We couldn't process your last payment";
            var recipient = subscription.User.Email;
            if (string.IsNullOrWhiteSpace(recipient))
            {
                _logger.LogWarning("Cannot send payment failed email for subscription {SubscriptionId}: user email is empty", subscription.Id);
                return;
            }

            var firstName = subscription.User.FirstName ?? subscription.User.UserName ?? "there";
            var html = await _templateRenderer.RenderAsync("/Views/Emails/PaymentFailed.cshtml", new PaymentEmailViewModel { FirstName = firstName, Status = "Failed", TierName = subscription.SubscriptionTier?.DisplayName ?? subscription.SubscriptionTier?.Name });
            await SendAsync(recipient, subject, html, isHtml: true);
        }

        public async Task SendPaymentSuccessEmailAsync(Subscription subscription)
        {
            var recipient = subscription.User.Email;
            if (string.IsNullOrWhiteSpace(recipient)) return;
            var subject = "Payment successful";
            var html = await _templateRenderer.RenderAsync("/Views/Emails/PaymentSuccess.cshtml", new PaymentEmailViewModel { FirstName = subscription.User.FirstName, Status = "Success", TierName = subscription.SubscriptionTier?.DisplayName ?? subscription.SubscriptionTier?.Name });
            await SendAsync(recipient, subject, html, isHtml: true);
        }

        public async Task SendPaymentFailedEmailAsync(Subscription subscription)
        {
            var recipient = subscription.User.Email;
            if (string.IsNullOrWhiteSpace(recipient)) return;
            var subject = "Payment failed";
            var body = $"Hi {subscription.User.FirstName},\n\nUnfortunately, we could not process your payment. Please update your billing details and try again.\n\n— {_options.FromName}";
            await SendAsync(recipient, subject, body);
        }

        public async Task<bool> SendEmailWithAttachmentAsync(string email, string subject, string body, byte[] attachmentData, string fileName, string contentType)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body
                };

                // Add attachment
                bodyBuilder.Attachments.Add(fileName, attachmentData, MimeKit.ContentType.Parse(contentType));

                message.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                if (_options.SkipCertificateValidation)
                {
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                }

                var secure = _options.GetSecureSocketOptions();
                await smtp.ConnectAsync(_options.Host, _options.Port, secure);

                if (!string.IsNullOrEmpty(_options.Username))
                {
                    await smtp.AuthenticateAsync(_options.Username, _options.Password);
                }

                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email with attachment sent to {Email} with subject '{Subject}'", email, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email with attachment to {Email}", email);
                return false;
            }
        }

        private async Task SendAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = isHtml ? null : body,
                HtmlBody = isHtml ? body : null
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            if (_options.SkipCertificateValidation)
            {
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            }

            var secure = _options.GetSecureSocketOptions();
            await smtp.ConnectAsync(_options.Host, _options.Port, secure);

            if (!string.IsNullOrEmpty(_options.Username))
            {
                await smtp.AuthenticateAsync(_options.Username, _options.Password);
            }

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} with subject '{Subject}'", toEmail, subject);
        }
    }
}