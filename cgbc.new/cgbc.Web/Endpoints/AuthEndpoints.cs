using cgbc.Web.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace cgbc.Web.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            HttpContext context,
            SignInManager<AdminUser> signInManager,
            IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Redirect("/admin/login?error=1");
            }

            var form = context.Request.Form;
            var username = form["username"].ToString();
            var password = form["password"].ToString();

            var result = await signInManager.PasswordSignInAsync(
                username, password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return Results.Redirect("/admin");
            }

            if (result.IsLockedOut)
            {
                return Results.Redirect("/admin/login?error=lockedout");
            }

            return Results.Redirect("/admin/login?error=1");
        }).RequireRateLimiting("login");

        app.MapPost("/api/auth/logout", async (
            HttpContext context,
            SignInManager<AdminUser> signInManager,
            IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest();
            }

            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        }).RequireAuthorization();
    }
}
