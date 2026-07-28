using cgbc.Web.Data;
using cgbc.Web.Models;
using cgbc.Web.Services;
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

    // --- AdminSeeder (Program.cs seed logic, exercised directly) ---

    [Fact]
    public async Task AdminSeeder_SeedAsync_NewUser_CreatesUserWithDisplayName()
    {
        var userManager = GetUserManager();

        await AdminSeeder.SeedAsync(userManager, "admin", "admin@test.com", "Admin@Set1");

        var seeded = await userManager.FindByNameAsync("admin");
        Assert.NotNull(seeded);
        Assert.Equal("Administrator", seeded.DisplayName);
    }

    [Fact]
    public async Task AdminSeeder_SeedAsync_ExistingUserWithoutDisplayName_GetsUpdated()
    {
        // Create user without DisplayName (simulating pre-migration state)
        await CreateTestUserAsync(username: "admin", displayName: null);
        var userManager = GetUserManager();

        await AdminSeeder.SeedAsync(userManager, "admin", "admin@test.com", "Admin@Set1");

        var updated = await userManager.FindByNameAsync("admin");
        Assert.Equal("Administrator", updated!.DisplayName);
    }

    [Fact]
    public async Task AdminSeeder_SeedAsync_ExistingUserWithDisplayName_NotOverwritten()
    {
        await CreateTestUserAsync(username: "admin", displayName: "Custom Name");
        var userManager = GetUserManager();

        await AdminSeeder.SeedAsync(userManager, "admin", "admin@test.com", "Admin@Set1");

        var found = await userManager.FindByNameAsync("admin");
        Assert.Equal("Custom Name", found!.DisplayName);
    }

    [Fact]
    public async Task AdminSeeder_SeedAsync_NewUser_MissingPassword_UserNotCreated()
    {
        var userManager = GetUserManager();

        await AdminSeeder.SeedAsync(userManager, "admin", "admin@test.com", null);

        var found = await userManager.FindByNameAsync("admin");
        Assert.Null(found);
    }

    [Fact]
    public async Task AdminSeeder_SeedAsync_NewUser_EmptyPassword_UserNotCreated()
    {
        var userManager = GetUserManager();

        await AdminSeeder.SeedAsync(userManager, "admin", "admin@test.com", "");

        var found = await userManager.FindByNameAsync("admin");
        Assert.Null(found);
    }

    // --- AdminSeeder.ValidateStartupConfig (Program.cs startup guard, exercised directly) ---

    [Fact]
    public void ValidateStartupConfig_NonDevelopment_MissingPassword_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AdminSeeder.ValidateStartupConfig(isDevelopment: false, adminSeedPassword: null));
    }

    [Fact]
    public void ValidateStartupConfig_NonDevelopment_EmptyPassword_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AdminSeeder.ValidateStartupConfig(isDevelopment: false, adminSeedPassword: ""));
    }

    [Fact]
    public void ValidateStartupConfig_NonDevelopment_WithPassword_DoesNotThrow()
    {
        AdminSeeder.ValidateStartupConfig(isDevelopment: false, adminSeedPassword: "Some@Strong1");
    }

    [Fact]
    public void ValidateStartupConfig_Development_MissingPassword_DoesNotThrow()
    {
        AdminSeeder.ValidateStartupConfig(isDevelopment: true, adminSeedPassword: null);
    }

    // --- AdminSeeder.ReadPasswordFromJsonConfig (must ignore environment variable overrides) ---

    [Fact]
    public void ReadPasswordFromJsonConfig_EnvironmentVariableOnly_ReturnsNull()
    {
        Environment.SetEnvironmentVariable("AdminSeed__Password", "FromEnvVar@123");
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var password = AdminSeeder.ReadPasswordFromJsonConfig(tempDir, "Production");
                Assert.Null(password);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("AdminSeed__Password", null);
        }
    }

    [Fact]
    public void ReadPasswordFromJsonConfig_SetInJsonFile_ReturnsValue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(
                Path.Combine(tempDir, "appsettings.Production.json"),
                """{ "AdminSeed": { "Password": "FromJsonFile@123" } }""");

            var password = AdminSeeder.ReadPasswordFromJsonConfig(tempDir, "Production");
            Assert.Equal("FromJsonFile@123", password);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
