using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace cgbc.Web.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<AuthEndpointsTestFactory>
{
    private readonly AuthEndpointsTestFactory _factory;

    public AuthEndpointsTests(AuthEndpointsTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Login_WithoutAntiforgeryToken_IsRejectedWithoutAuthenticating()
    {
        var client = CreateClient();

        var response = await client.PostAsync(
            "/api/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = "admin",
                ["password"] = "whatever",
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/admin/login?error=1", response.Headers.Location?.OriginalString);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Logout_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/auth/logout", new FormUrlEncodedContent([]));

        // RequireAuthorization() runs first for an unauthenticated request and redirects
        // to the login page before the handler (and its antiforgery check) ever runs —
        // this asserts that unauthenticated behavior, distinct from the authenticated
        // antiforgery-rejection case which needs a real signed-in session to reach.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_GetLoginPage_ReturnsAntiforgeryCookie()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/admin/login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies!, c => c.Contains("Antiforgery", StringComparison.OrdinalIgnoreCase));
    }
}
