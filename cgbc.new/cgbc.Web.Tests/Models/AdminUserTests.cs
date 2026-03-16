using cgbc.Web.Models;

namespace cgbc.Web.Tests.Models;

public class AdminUserTests
{
    [Fact]
    public void DisplayName_DefaultsToNull()
    {
        var user = new AdminUser();
        Assert.Null(user.DisplayName);
    }

    [Fact]
    public void DisplayName_CanBeSet()
    {
        var user = new AdminUser { DisplayName = "Test Admin" };
        Assert.Equal("Test Admin", user.DisplayName);
    }

    [Fact]
    public void InheritsIdentityUserProperties()
    {
        var user = new AdminUser
        {
            UserName = "admin",
            Email = "admin@test.com",
            DisplayName = "Administrator"
        };

        Assert.Equal("admin", user.UserName);
        Assert.Equal("admin@test.com", user.Email);
        Assert.Equal("Administrator", user.DisplayName);
    }
}
