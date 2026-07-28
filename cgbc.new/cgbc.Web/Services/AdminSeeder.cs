using cgbc.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace cgbc.Web.Services;

public static class AdminSeeder
{
    public static void ValidateStartupConfig(bool isDevelopment, string? adminSeedPassword)
    {
        if (!isDevelopment && string.IsNullOrEmpty(adminSeedPassword))
        {
            throw new InvalidOperationException(
                "AdminSeed:Password must be set (via appsettings.Production.json or the AdminSeed__Password environment variable) before starting outside the Development environment.");
        }
    }

    public static async Task SeedAsync(
        UserManager<AdminUser> userManager,
        string username,
        string email,
        string? password)
    {
        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser == null)
        {
            if (string.IsNullOrEmpty(password))
                return;

            var adminUser = new AdminUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };
            await userManager.CreateAsync(adminUser, password);
        }
        else if (string.IsNullOrEmpty(existingUser.DisplayName))
        {
            existingUser.DisplayName = "Administrator";
            await userManager.UpdateAsync(existingUser);
        }
    }
}
