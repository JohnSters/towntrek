namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for rendering email templates with data models
    /// </summary>
    public interface IEmailTemplateRenderer
    {
        /// <summary>
        /// Renders an email template with the provided model data
        /// </summary>
        Task<string> RenderAsync(string viewPath, object model);
    }
}


