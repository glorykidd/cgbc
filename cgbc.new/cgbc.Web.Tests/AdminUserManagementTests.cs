using cgbc.Web.Data;
using cgbc.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace cgbc.Web.Tests;

public class AdminUserManagementTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _db;

    public AdminUserManagementTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        services.AddIdentity<AdminUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        _db = _serviceProvider.GetRequiredService<AppDbContext>();
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
        _serviceProvider.Dispose();
    }

    private UserManager<AdminUser> GetUserManager() =>
        _serviceProvider.GetRequiredService<UserManager<AdminUser>>();

    private async Task<AdminUser> CreateTestUserAsync(
        string username = "testuser",
        string email = "test@test.com",
        string password = "Pass@123",
        string? displayName = null)
    {
        var userManager = GetUserManager();
        var user = new AdminUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName
        };
        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
        return user;
    }

    // --- User Creation ---

    [Fact]
    public async Task CreateUser_WithDisplayName_PersistsDisplayName()
    {
        var user = await CreateTestUserAsync(displayName: "Test Admin");
        var userManager = GetUserManager();
        var found = await userManager.FindByNameAsync("testuser");

        Assert.NotNull(found);
        Assert.Equal("Test Admin", found.DisplayName);
    }

    [Fact]
    public async Task CreateUser_WithoutDisplayName_DisplayNameIsNull()
    {
        var user = await CreateTestUserAsync();
        var userManager = GetUserManager();
        var found = await userManager.FindByNameAsync("testuser");

        Assert.NotNull(found);
        Assert.Null(found.DisplayName);
    }

    [Fact]
    public async Task CreateUser_WithValidPassword_Succeeds()
    {
        var userManager = GetUserManager();
        var user = new AdminUser
        {
            UserName = "newadmin",
            Email = "new@test.com",
            EmailConfirmed = true,
            DisplayName = "New Admin"
        };

        var result = await userManager.CreateAsync(user, "Valid@1x");
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("Sh@1", "Too short")]
    [InlineData("longpass@1", "No uppercase")]
    [InlineData("LONGPASS@1", "No lowercase")]
    [InlineData("LongPass@x", "No digit")]
    [InlineData("LongPass12", "No special character")]
    public async Task CreateUser_WithWeakPassword_Fails(string password, string _)
    {
        var userManager = GetUserManager();
        var user = new AdminUser
        {
            UserName = "newadmin",
            Email = "new@test.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateUser_DuplicateUsername_Fails()
    {
        await CreateTestUserAsync(username: "admin1");

        var userManager = GetUserManager();
        var duplicate = new AdminUser
        {
            UserName = "admin1",
            Email = "other@test.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(duplicate, "Pass@123");
        Assert.False(result.Succeeded);
    }

    // --- User Update ---

    [Fact]
    public async Task UpdateUser_DisplayName_Succeeds()
    {
        var user = await CreateTestUserAsync(displayName: "Original");
        var userManager = GetUserManager();

        user.DisplayName = "Updated";
        var result = await userManager.UpdateAsync(user);

        Assert.True(result.Succeeded);

        var found = await userManager.FindByNameAsync("testuser");
        Assert.Equal("Updated", found!.DisplayName);
    }

    [Fact]
    public async Task UpdateUser_Email_Succeeds()
    {
        var user = await CreateTestUserAsync();
        var userManager = GetUserManager();

        user.Email = "newemail@test.com";
        var result = await userManager.UpdateAsync(user);

        Assert.True(result.Succeeded);
        var found = await userManager.FindByNameAsync("testuser");
        Assert.Equal("newemail@test.com", found!.Email);
    }

    // --- Password Change (self-service) ---

    [Fact]
    public async Task ChangePassword_WithCorrectCurrentPassword_Succeeds()
    {
        var user = await CreateTestUserAsync(password: "Old@Pass1");
        var userManager = GetUserManager();

        var result = await userManager.ChangePasswordAsync(user, "Old@Pass1", "New@Pass1");
        Assert.True(result.Succeeded);

        // Verify new password works
        Assert.True(await userManager.CheckPasswordAsync(user, "New@Pass1"));
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Fails()
    {
        var user = await CreateTestUserAsync(password: "Old@Pass1");
        var userManager = GetUserManager();

        var result = await userManager.ChangePasswordAsync(user, "Wrong@Pass1", "New@Pass1");
        Assert.False(result.Succeeded);

        // Verify old password still works
        Assert.True(await userManager.CheckPasswordAsync(user, "Old@Pass1"));
    }

    // --- Admin Password Reset ---

    [Fact]
    public async Task AdminResetPassword_ViaToken_Succeeds()
    {
        var user = await CreateTestUserAsync(password: "Old@Pass1");
        var userManager = GetUserManager();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, "Admin@Set1");
        Assert.True(result.Succeeded);

        Assert.True(await userManager.CheckPasswordAsync(user, "Admin@Set1"));
        Assert.False(await userManager.CheckPasswordAsync(user, "Old@Pass1"));
    }

    [Fact]
    public async Task AdminResetPassword_ViaToken_InvalidPassword_KeepsOldPassword()
    {
        var user = await CreateTestUserAsync(password: "Old@Pass1");
        var userManager = GetUserManager();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, "weak");
        Assert.False(result.Succeeded);

        // Old password still works — user is not left without a password
        Assert.True(await userManager.CheckPasswordAsync(user, "Old@Pass1"));
    }

    // --- User Deletion ---

    [Fact]
    public async Task DeleteUser_RemovesFromDatabase()
    {
        var user = await CreateTestUserAsync();
        var userManager = GetUserManager();

        var result = await userManager.DeleteAsync(user);
        Assert.True(result.Succeeded);

        var found = await userManager.FindByNameAsync("testuser");
        Assert.Null(found);
    }

    // --- User Listing ---

    [Fact]
    public async Task Users_ReturnsAllCreatedUsers()
    {
        await CreateTestUserAsync(username: "admin1", email: "a1@test.com");
        await CreateTestUserAsync(username: "admin2", email: "a2@test.com");
        await CreateTestUserAsync(username: "admin3", email: "a3@test.com");

        var userManager = GetUserManager();
        var users = await userManager.Users.ToListAsync();

        Assert.Equal(3, users.Count);
    }

    [Fact]
    public async Task FindByIdAsync_ReturnsCorrectUser()
    {
        var user = await CreateTestUserAsync(displayName: "Find Me");
        var userManager = GetUserManager();

        var found = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(found);
        Assert.Equal("Find Me", found.DisplayName);
        Assert.Equal("testuser", found.UserName);
    }

    // --- Seed Logic Simulation ---

    [Fact]
    public async Task SeedLogic_CreatesUserWithDisplayName()
    {
        var userManager = GetUserManager();

        // Simulate the seed logic from Program.cs
        var username = "admin";
        var existingUser = await userManager.FindByNameAsync(username);
        Assert.Null(existingUser);

        var adminUser = new AdminUser
        {
            UserName = username,
            Email = "admin@test.com",
            EmailConfirmed = true,
            DisplayName = "Administrator"
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@CGBC2026!");
        Assert.True(result.Succeeded);

        var seeded = await userManager.FindByNameAsync(username);
        Assert.NotNull(seeded);
        Assert.Equal("Administrator", seeded.DisplayName);
    }

    [Fact]
    public async Task SeedLogic_ExistingUserWithoutDisplayName_GetsUpdated()
    {
        // Create user without DisplayName (simulating pre-migration state)
        var user = await CreateTestUserAsync(username: "admin", displayName: null);
        var userManager = GetUserManager();

        // Simulate the seed update logic
        var existingUser = await userManager.FindByNameAsync("admin");
        Assert.NotNull(existingUser);

        if (string.IsNullOrEmpty(existingUser.DisplayName))
        {
            existingUser.DisplayName = "Administrator";
            await userManager.UpdateAsync(existingUser);
        }

        var updated = await userManager.FindByNameAsync("admin");
        Assert.Equal("Administrator", updated!.DisplayName);
    }

    [Fact]
    public async Task SeedLogic_ExistingUserWithDisplayName_NotOverwritten()
    {
        var user = await CreateTestUserAsync(username: "admin", displayName: "Custom Name");
        var userManager = GetUserManager();

        var existingUser = await userManager.FindByNameAsync("admin");
        Assert.NotNull(existingUser);

        // Simulate the seed update logic — should NOT overwrite
        if (string.IsNullOrEmpty(existingUser.DisplayName))
        {
            existingUser.DisplayName = "Administrator";
            await userManager.UpdateAsync(existingUser);
        }

        var found = await userManager.FindByNameAsync("admin");
        Assert.Equal("Custom Name", found!.DisplayName);
    }

    // --- Startup Guard (AdminSeed:Password) ---

    private static void AssertStartupGuard(bool isDevelopment, string? adminSeedPassword)
    {
        // Simulate the startup guard from Program.cs
        if (!isDevelopment && string.IsNullOrEmpty(adminSeedPassword))
        {
            throw new InvalidOperationException(
                "AdminSeed:Password must be set in appsettings.Production.json (or appsettings.{Environment}.json) before starting outside the Development environment.");
        }
    }

    [Fact]
    public void StartupGuard_NonDevelopment_MissingPassword_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AssertStartupGuard(isDevelopment: false, adminSeedPassword: null));
    }

    [Fact]
    public void StartupGuard_NonDevelopment_EmptyPassword_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AssertStartupGuard(isDevelopment: false, adminSeedPassword: ""));
    }

    [Fact]
    public void StartupGuard_NonDevelopment_WithPassword_DoesNotThrow()
    {
        AssertStartupGuard(isDevelopment: false, adminSeedPassword: "Some@Strong1");
    }

    [Fact]
    public void StartupGuard_Development_MissingPassword_DoesNotThrow()
    {
        AssertStartupGuard(isDevelopment: true, adminSeedPassword: null);
    }

    [Fact]
    public async Task SeedLogic_NewUser_MissingPassword_UserNotCreated()
    {
        var userManager = GetUserManager();

        // Simulate the seed logic from Program.cs when AdminSeed:Password is unset
        string? password = null;
        var existingUser = await userManager.FindByNameAsync("admin");
        Assert.Null(existingUser);

        if (existingUser == null && !string.IsNullOrEmpty(password))
        {
            var adminUser = new AdminUser
            {
                UserName = "admin",
                Email = "admin@test.com",
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };
            await userManager.CreateAsync(adminUser, password);
        }

        var found = await userManager.FindByNameAsync("admin");
        Assert.Null(found);
    }
}
