using Microsoft.AspNetCore.Mvc;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Api
{
	[Route("Api/[controller]/[action]")]
	public class EmailController : Controller
	{
		private readonly IEmailService _emailService;

		public EmailController(IEmailService emailService)
		{
			_emailService = emailService;
		}

		[HttpGet]
		public async Task<IActionResult> TestWelcome(string to, string firstName = "Friend")
		{
			await _emailService.SendWelcomeEmailAsync(to, firstName);
			return Ok(new { sent = true });
		}

		[HttpGet]
		public async Task<IActionResult> TestConfirm(string to, string url)
		{
			await _emailService.SendEmailConfirmationAsync(to, to, url);
			return Ok(new { sent = true });
		}

		[HttpGet]
		public async Task<IActionResult> TestPaymentSuccess(string to)
		{
			// minimal fake subscription for template
			var sub = new TownTrek.Models.Subscription
			{
				User = new TownTrek.Models.ApplicationUser { Email = to, FirstName = to }
			};
			await _emailService.SendPaymentSuccessEmailAsync(sub);
			return Ok(new { sent = true });
		}
	}
}


