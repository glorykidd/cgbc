using cgbc.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace cgbc.Web.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            HttpContext context,
            SignInManager<AdminUser> signInManager) =>
        {
            var form = context.Request.Form;
            var username = form["username"].ToString();
            var password = form["password"].ToString();

            var result = await signInManager.PasswordSignInAsync(
                username, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Results.Redirect("/admin");
            }

            return Results.Redirect("/admin/login?error=1");
        });

        app.MapPost("/api/auth/logout", async (SignInManager<AdminUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/admin/login");
        }).RequireAuthorization();
    }
}
