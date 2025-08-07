using Microsoft.AspNetCore.Identity;

namespace TownTrek.Services
{
    public interface IRoleInitializationService
    {
        Task InitializeRolesAsync();
    }

    public class RoleInitializationService : IRoleInitializationService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleInitializationService> _logger;

        public RoleInitializationService(
            RoleManager<IdentityRole> roleManager,
            ILogger<RoleInitializationService> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeRolesAsync()
        {
            var roles = new[]
            {
                "Admin",
                "Member",
                "Client-Basic",
                "Client-Standard", 
                "Client-Premium"
            };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    var result = await _roleManager.CreateAsync(role);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' created successfully", roleName);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}