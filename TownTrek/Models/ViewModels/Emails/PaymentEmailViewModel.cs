namespace TownTrek.Models.ViewModels.Emails
{
	public class PaymentEmailViewModel
	{
		public string FirstName { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty; // "Success" or "Failed"
		public string? TierName { get; set; }
	}
}


