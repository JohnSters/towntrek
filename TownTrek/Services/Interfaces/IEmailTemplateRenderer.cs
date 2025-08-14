namespace TownTrek.Services.Interfaces
{
	public interface IEmailTemplateRenderer
	{
		Task<string> RenderAsync(string viewPath, object model);
	}
}


