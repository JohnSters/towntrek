using MailKit.Security;

namespace TownTrek.Options;

public class EmailOptions
{
	public string Host { get; set; } = string.Empty;
	public int Port { get; set; } = 587;
	public string Username { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string FromName { get; set; } = "TownTrek";
	public string FromAddress { get; set; } = "no-reply@towntrek.local";
	public bool UseStartTls { get; set; } = true;
	public bool UseSsl { get; set; } = false;
	public bool SkipCertificateValidation { get; set; } = false; // never enable in production

	public SecureSocketOptions GetSecureSocketOptions()
	{
		if (UseSsl) return SecureSocketOptions.SslOnConnect;
		if (UseStartTls) return SecureSocketOptions.StartTlsWhenAvailable;
		return SecureSocketOptions.Auto;
	}
}


