using cgbc.Web.Data;
using cgbc.Web.Identity;
using cgbc.Web.Models;
using cgbc.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<ContentService>();
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "cgbc.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ConnectionCardService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(nameof(TurnstileService), client => client.Timeout = TimeSpan.FromSeconds(5));
builder.Services.AddScoped<TurnstileService>();

builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<AdminUserClaimsPrincipalFactory>();

// The site runs behind IIS + ASP.NET Core Module on the same box, which
// forwards the real client IP via X-Forwarded-For. Without this, every
// request's RemoteIpAddress is the loopback IIS proxy address, collapsing
// the per-client rate limiter below into a single shared bucket.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

// Rate limiting, partitioned per client IP so one abusive IP can't exhaust
// a shared bucket and lock out every other client.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => PerIpFixedWindow(httpContext, permitLimit: 5, window: TimeSpan.FromMinutes(5)));
});

// A null RemoteIpAddress shouldn't happen under normal IIS/Kestrel TCP
// hosting, but if it does (e.g. a misconfigured reverse proxy), fall back to
// a single shared "unknown" bucket so all such requests are throttled
// together. A fresh GUID per request would give each one its own unlimited
// bucket — defeating rate limiting entirely and leaking memory unboundedly.
static RateLimitPartition<string> PerIpFixedWindow(HttpContext httpContext, int permitLimit, TimeSpan window)
{
    var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey,
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window
        });
}

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

Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(builder.Configuration["Turnstile:SecretKey"]))
{
    Console.WriteLine("WARNING: Turnstile:SecretKey not configured. The Connect form will run without CAPTCHA protection.");
}

var adminSeedPassword = AdminSeeder.ReadPasswordFromJsonConfig(builder.Environment.ContentRootPath, builder.Environment.EnvironmentName);

AdminSeeder.ValidateStartupConfig(builder.Environment.IsDevelopment(), adminSeedPassword);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminConfig = app.Configuration.GetSection("AdminSeed");
    var username = adminConfig["Username"] ?? "admin";
    var email = adminConfig["Email"] ?? "admin@cedargrovebaptist.church";

    await AdminSeeder.SeedAsync(userManager, roleManager, username, email, adminSeedPassword);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();

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

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<cgbc.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/sitemap.xml", cgbc.Web.Endpoints.SitemapEndpoint.Handle);
cgbc.Web.Endpoints.AuthEndpoints.Map(app);
cgbc.Web.Endpoints.ExportEndpoint.Map(app);

app.Run();

public partial class Program { }
