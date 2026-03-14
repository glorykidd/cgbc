using cgbc.Web.Data;
using cgbc.Web.Models;
using cgbc.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<ContentService>();
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "cgbc.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<ConnectionCardService>();

builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
    var adminConfig = app.Configuration.GetSection("AdminSeed");
    var username = adminConfig["Username"] ?? "admin";
    var email = adminConfig["Email"] ?? "admin@cedargrovebaptist.church";
    var password = adminConfig["Password"] ?? "Admin@CGBC2026!";

    var existingUser = await userManager.FindByNameAsync(username);
    if (existingUser == null)
    {
        var adminUser = new AdminUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, password);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name;
        if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") ||
            path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".svg") ||
            path.EndsWith(".woff2"))
        {
            ctx.Context.Response.Headers.CacheControl = "public, max-age=604800";
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<cgbc.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/sitemap.xml", cgbc.Web.Endpoints.SitemapEndpoint.Handle);
cgbc.Web.Endpoints.AuthEndpoints.Map(app);
cgbc.Web.Endpoints.ExportEndpoint.Map(app);

app.Run();
