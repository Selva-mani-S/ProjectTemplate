namespace Solution.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class IdentityDataSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using IServiceScope scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        const string adminRole = "Administrator";
        const string adminEmail = "admin@baseline.local";

        if (!await roleManager.RoleExistsAsync(adminRole).ConfigureAwait(false))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = adminRole,
                Description = "System Administrator with full permissions"
            }).ConfigureAwait(false);
        }

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail).ConfigureAwait(false);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Meets strict password complexity: >=12 chars, digit, uppercase, lowercase, non-alphanumeric, 4 unique chars
            IdentityResult result = await userManager.CreateAsync(adminUser, "AdminP@ssw0rd123!").ConfigureAwait(false);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole).ConfigureAwait(false);
            }
        }
    }
}
