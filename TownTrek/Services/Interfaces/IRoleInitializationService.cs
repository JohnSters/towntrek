namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Service for initializing application roles and permissions
    /// </summary>
    public interface IRoleInitializationService
    {
        /// <summary>
        /// Initializes all application roles in the system
        /// </summary>
        Task InitializeRolesAsync();
    }
}