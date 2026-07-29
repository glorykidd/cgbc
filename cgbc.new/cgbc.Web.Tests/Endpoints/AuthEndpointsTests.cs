using System.Net;
using System.Text.RegularExpressions;
using cgbc.Web.Data;
using cgbc.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

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

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(html, """name="__RequestVerificationToken"[^>]*value="([^"]+)""");
        Assert.True(match.Success, "Antiforgery token not found in rendered login page.");
        return match.Groups[1].Value;
    }

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
        Assert.False(
            response.Headers.TryGetValues("Set-Cookie", out var cookies) &&
            cookies.Any(c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal)),
            "No auth cookie should be set when the antiforgery check rejects the request.");
    }

    [Fact]
    public async Task Logout_Unauthenticated_RedirectsToLoginWithoutReachingAntiforgeryCheck()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/auth/logout", new FormUrlEncodedContent([]));

        // RequireAuthorization() runs first for an unauthenticated request and redirects
        // to the login page before the handler (and its antiforgery check) ever runs — this
        // is distinct from the authenticated antiforgery-rejection case (see the next test),
        // which needs a real signed-in session to actually reach the handler's ValidateRequestAsync call.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_AuthenticatedWithoutAntiforgeryToken_ReturnsBadRequest()
    {
        const string username = "logouttestadmin";
        const string password = "Test@Admin1";

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
            var user = new AdminUser { UserName = username, Email = "logouttest@test.com", EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var client = CreateClient();

        var loginPage = await client.GetAsync("/admin/login");
        var token = ExtractAntiforgeryToken(await loginPage.Content.ReadAsStringAsync());

        var loginResponse = await client.PostAsync(
            "/api/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = username,
                ["password"] = password,
                ["__RequestVerificationToken"] = token,
            }));
        Assert.Equal("/admin", loginResponse.Headers.Location?.OriginalString);

        // Authenticated now, but posting logout without a token should be rejected by
        // ValidateRequestAsync — this is the actual antiforgery-failure path on logout,
        // as opposed to the unauthenticated-redirect path covered by the previous test.
        var logoutResponse = await client.PostAsync("/api/auth/logout", new FormUrlEncodedContent([]));

        Assert.Equal(HttpStatusCode.BadRequest, logoutResponse.StatusCode);
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
