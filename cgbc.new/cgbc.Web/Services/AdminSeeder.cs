using cgbc.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace cgbc.Web.Services;

public static class AdminSeeder
{
    // AdminSeed:Password must come from appsettings.json/appsettings.{Environment}.json only —
    // not from environment variables — so it can only be set by editing config on the server.
    public static string? ReadPasswordFromJsonConfig(string contentRootPath, string environmentName)
    {
        var jsonConfig = new ConfigurationBuilder()
            .SetBasePath(contentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .Build();
        return jsonConfig["AdminSeed:Password"];
    }

    public static void ValidateStartupConfig(bool isDevelopment, string? adminSeedPassword)
    {
        if (!isDevelopment && string.IsNullOrWhiteSpace(adminSeedPassword))
        {
            throw new InvalidOperationException(
                "AdminSeed:Password must be set in appsettings.Production.json before starting outside the Development environment.");
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
            if (string.IsNullOrWhiteSpace(password))
                return;

            var adminUser = new AdminUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };
            var result = await userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed admin user '{username}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }
        else if (string.IsNullOrEmpty(existingUser.DisplayName))
        {
            existingUser.DisplayName = "Administrator";
            await userManager.UpdateAsync(existingUser);
        }
    }
}
