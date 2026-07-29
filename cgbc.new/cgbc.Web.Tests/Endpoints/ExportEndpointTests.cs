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
        var turnstile = new TurnstileService(new NullHttpClientFactory(), config, NullLogger<TurnstileService>.Instance);
        _service = new ConnectionCardService(_db, email, turnstile);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    private class NullHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
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
    public void Escape_QuotesValue_WhenContainsCarriageReturn()
    {
        Assert.Equal("\"line1\rline2\"", ExportEndpoint.Escape("line1\rline2"));
    }

    [Fact]
    public void Escape_QuotesValue_WhenContainsTab()
    {
        Assert.Equal("\"line1\tline2\"", ExportEndpoint.Escape("line1\tline2"));
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

    // --- Formula/DDE injection (CWE-1236) ---

    [Theory]
    [InlineData("=cmd|'/c calc'!A1")]
    [InlineData("+1+1")]
    [InlineData("-1+1")]
    [InlineData("@SUM(A1:A2)")]
    public void Escape_PrefixesLeadingApostrophe_WhenValueStartsWithFormulaTriggerChar(string value)
    {
        var result = ExportEndpoint.Escape(value);
        Assert.StartsWith("'" + value, result);
    }

    [Fact]
    public void Escape_PrefixesLeadingApostrophe_WhenValueStartsWithTab()
    {
        // \t must also trigger CSV quoting, not just the formula-prefix — an
        // unquoted bare \t in a field can be misread as a column delimiter.
        var result = ExportEndpoint.Escape("\tHYPERLINK(evil)");
        Assert.Equal("\"'\tHYPERLINK(evil)\"", result);
    }

    [Fact]
    public void Escape_PrefixesLeadingApostrophe_WhenValueStartsWithCarriageReturn()
    {
        // \r must also trigger CSV quoting, not just the formula-prefix — an
        // unquoted bare \r in a field can be misread as a row terminator.
        var result = ExportEndpoint.Escape("\r=1+1");
        Assert.Equal("\"'\r=1+1\"", result);
    }

    [Fact]
    public void Escape_FormulaValueContainingComma_IsBothPrefixedAndQuoted()
    {
        var result = ExportEndpoint.Escape("=HYPERLINK(\"http://evil\"),\"click\"");
        Assert.Equal("\"'=HYPERLINK(\"\"http://evil\"\"),\"\"click\"\"\"", result);
    }

    [Fact]
    public void Escape_DoesNotPrefix_WhenFormulaTriggerCharIsNotFirst()
    {
        Assert.Equal("Total = 5", ExportEndpoint.Escape("Total = 5"));
    }

    [Fact]
    public void Escape_DoesNotPrefix_ForOrdinaryValue()
    {
        Assert.Equal("John Doe", ExportEndpoint.Escape("John Doe"));
    }
}
