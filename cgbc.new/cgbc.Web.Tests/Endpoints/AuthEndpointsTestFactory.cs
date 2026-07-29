using cgbc.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace cgbc.Web.Tests.Endpoints;

/// <summary>
/// Boots the real Program.cs pipeline (routing, antiforgery, rate limiting,
/// AuthEndpoints) against an in-memory SQLite database. Runs in Development
/// so AdminSeeder.ValidateStartupConfig doesn't require AdminSeed:Password —
/// these tests only exercise the antiforgery guard itself, not the seeded
/// admin account, so no real credentials are needed.
/// </summary>
public class AuthEndpointsTestFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnectionHolder _connectionHolder = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connectionHolder.Connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connectionHolder.Dispose();
    }

    private sealed class SqliteConnectionHolder : IDisposable
    {
        public Microsoft.Data.Sqlite.SqliteConnection Connection { get; } = new("DataSource=:memory:");

        public SqliteConnectionHolder()
        {
            Connection.Open();
        }

        public void Dispose() => Connection.Dispose();
    }
}
