using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AssignmentFinder.Infrastructure.Identity;

public class AdminSeedingException(string action, IEnumerable<IdentityError> errors)
    : Exception($"AdminSeeder failed to {action}: {string.Join("; ", errors.Select(e => e.Description))}");

/// <summary>
/// Ensures the single bootstrap admin account (configured via Admin:Email / Admin:Password)
/// exists, is approved, and holds the Admin role. Runs idempotently on every startup so the
/// admin's password/role can be fixed by restarting the app after changing config, and so a
/// fresh database always ends up with exactly one ready-to-use admin.
/// </summary>
public class AdminSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration)
{
    public async Task SeedAsync()
    {
        if (!await roleManager.RoleExistsAsync(ApplicationUser.AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(ApplicationUser.AdminRole));
        }

        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsApproved = true,
            };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new AdminSeedingException("create the bootstrap admin", createResult.Errors);
            }
        }
        else
        {
            if (!admin.IsApproved)
            {
                admin.IsApproved = true;
                await userManager.UpdateAsync(admin);
            }

            if (!await userManager.CheckPasswordAsync(admin, adminPassword))
            {
                var removeResult = await userManager.RemovePasswordAsync(admin);
                if (!removeResult.Succeeded)
                {
                    throw new AdminSeedingException("remove the admin's old password", removeResult.Errors);
                }

                var addResult = await userManager.AddPasswordAsync(admin, adminPassword);
                if (!addResult.Succeeded)
                {
                    throw new AdminSeedingException("set the admin's new password", addResult.Errors);
                }
            }
        }

        if (!await userManager.IsInRoleAsync(admin, ApplicationUser.AdminRole))
        {
            await userManager.AddToRoleAsync(admin, ApplicationUser.AdminRole);
        }
    }
}
