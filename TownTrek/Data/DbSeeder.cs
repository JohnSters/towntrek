using Microsoft.AspNetCore.Identity;
using TownTrek.Models;

namespace TownTrek.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            await CreateRoleIfNotExists(roleManager, "Admin");
            await CreateRoleIfNotExists(roleManager, "Client-Basic");
            await CreateRoleIfNotExists(roleManager, "Client-Standard");
            await CreateRoleIfNotExists(roleManager, "Client-Premium");
            await CreateRoleIfNotExists(roleManager, "Member");

            // Create admin user
            await CreateAdminUserIfNotExists(userManager);
        }

        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task CreateAdminUserIfNotExists(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@towntrek.co.za";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    AuthenticationMethod = "Email"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}