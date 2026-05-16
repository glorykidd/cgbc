using cgbc.Web.Data;
using cgbc.Web.Endpoints;
using cgbc.Web.Models;
using cgbc.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace cgbc.Web.Tests.Endpoints;

public class ExportEndpointTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ConnectionCardService _service;

    public ExportEndpointTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        var config = new ConfigurationBuilder().Build();
        var email = new EmailService(config, NullLogger<EmailService>.Instance);
        _service = new ConnectionCardService(_db, email);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    // --- Escape helper ---

    [Fact]
    public void Escape_ReturnsEmptyString_ForNull()
    {
        Assert.Equal("", ExportEndpoint.Escape(null));
    }

    [Fact]
    public void Escape_ReturnsEmptyString_ForEmptyString()
    {
        Assert.Equal("", ExportEndpoint.Escape(""));
    }

    [Fact]
    public void Escape_ReturnsValue_WhenNoSpecialChars()
    {
        Assert.Equal("John Doe", ExportEndpoint.Escape("John Doe"));
    }

    [Fact]
    public void Escape_QuotesValue_WhenContainsComma()
    {
        Assert.Equal("\"Smith, John\"", ExportEndpoint.Escape("Smith, John"));
    }

    [Fact]
    public void Escape_QuotesValue_WhenContainsNewline()
    {
        Assert.Equal("\"line1\nline2\"", ExportEndpoint.Escape("line1\nline2"));
    }

    [Fact]
    public void Escape_DoublesInternalQuotes()
    {
        Assert.Equal("\"say \"\"hello\"\"\"", ExportEndpoint.Escape("say \"hello\""));
    }

    [Fact]
    public void Escape_QuotesAndDoublesQuotes_WhenBothPresent()
    {
        var result = ExportEndpoint.Escape("he said, \"hi\"");
        Assert.StartsWith("\"", result);
        Assert.EndsWith("\"", result);
        Assert.Contains("\"\"", result);
    }
}
