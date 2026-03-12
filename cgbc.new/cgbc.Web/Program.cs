using cgbc.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<ContentService>();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

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

app.UseAntiforgery();

app.MapRazorComponents<cgbc.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/sitemap.xml", cgbc.Web.Endpoints.SitemapEndpoint.Handle);

app.Run();
