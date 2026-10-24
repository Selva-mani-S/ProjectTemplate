namespace Solution.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class IdentityDataSeeder
{
    public const string AdminRole = "Administrator";
    public const string MemberRole = "Member";

    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using IServiceScope scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        const string adminEmail = "admin@baseline.local";

        if (!await roleManager.RoleExistsAsync(AdminRole).ConfigureAwait(false))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = AdminRole,
                Description = "System Administrator with full permissions"
            }).ConfigureAwait(false);
        }

        if (!await roleManager.RoleExistsAsync(MemberRole).ConfigureAwait(false))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = MemberRole,
                Description = "IAF Batch 1986 Registered Member"
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
                Department = "Headquarters",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Meets strict password complexity: >=12 chars, digit, uppercase, lowercase, non-alphanumeric, 4 unique chars
            IdentityResult result = await userManager.CreateAsync(adminUser, "AdminP@ssw0rd123!").ConfigureAwait(false);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole).ConfigureAwait(false);
            }
        }

        const string memberEmail = "veteran@iaf1986.local";
        ApplicationUser? memberUser = await userManager.FindByEmailAsync(memberEmail).ConfigureAwait(false);
        if (memberUser == null)
        {
            memberUser = new ApplicationUser
            {
                UserName = memberEmail,
                Email = memberEmail,
                FirstName = "Rajesh",
                LastName = "Kumar",
                Department = "IAF 1986 Batch",
                ServiceNo = "661234",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult result = await userManager.CreateAsync(memberUser, "VeteranP@ssw0rd123!").ConfigureAwait(false);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(memberUser, MemberRole).ConfigureAwait(false);
            }
        }
    }
}
