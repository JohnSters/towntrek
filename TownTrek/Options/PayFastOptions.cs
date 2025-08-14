namespace TownTrek.Options;

public class PayFastOptions
{
	public string MerchantId { get; set; } = string.Empty;
	public string MerchantKey { get; set; } = string.Empty;
	public string? PassPhrase { get; set; }
	public string PaymentUrl { get; set; } = string.Empty;
	public bool IsLive { get; set; } = false;
	public string Environment { get; set; } = "sandbox"; // sandbox | live
    public bool UseEncodedSignature { get; set; } = false; // Toggle if gateway expects encoded values in signature
}


