using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using cgbc.Web.Endpoints;

namespace cgbc.Web.Tests.Endpoints;

public class SitemapEndpointTests
{
    private static async Task<string> GetSitemapXmlAsync()
    {
        var result = SitemapEndpoint.Handle();
        var services = new ServiceCollection();
        services.AddLogging();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        httpContext.Response.Body = new MemoryStream();
        await result.ExecuteAsync(httpContext);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private static string GetSitemapXml() => GetSitemapXmlAsync().GetAwaiter().GetResult();

    [Fact]
    public void Handle_ReturnsValidXml()
    {
        var xml = GetSitemapXml();
        var doc = XDocument.Parse(xml);
        Assert.NotNull(doc);
    }

    [Fact]
    public void Handle_HasCorrectXmlDeclaration()
    {
        var xml = GetSitemapXml();
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", xml);
    }

    [Fact]
    public void Handle_ContainsSitemapNamespace()
    {
        var xml = GetSitemapXml();
        Assert.Contains("http://www.sitemaps.org/schemas/sitemap/0.9", xml);
    }

    [Fact]
    public void Handle_ContainsEightUrls()
    {
        var xml = GetSitemapXml();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var doc = XDocument.Parse(xml);
        var urls = doc.Descendants(ns + "url").ToList();
        Assert.Equal(9, urls.Count);
    }

    [Fact]
    public void Handle_HomepageHasHighestPriority()
    {
        var xml = GetSitemapXml();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var doc = XDocument.Parse(xml);
        var homeUrl = doc.Descendants(ns + "url")
            .First(u => u.Element(ns + "loc")!.Value.EndsWith(".church/"));
        Assert.Equal("1.0", homeUrl.Element(ns + "priority")!.Value);
    }

    [Fact]
    public void Handle_AllUrlsHaveBaseUrl()
    {
        var xml = GetSitemapXml();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var doc = XDocument.Parse(xml);
        var locs = doc.Descendants(ns + "loc").Select(e => e.Value).ToList();
        Assert.All(locs, loc => Assert.StartsWith("https://cedargrovebaptist.church", loc));
    }

    [Fact]
    public void Handle_ContainsExpectedPages()
    {
        var xml = GetSitemapXml();
        var expectedPages = new[]
        {
            "https://cedargrovebaptist.church/",
            "https://cedargrovebaptist.church/about",
            "https://cedargrovebaptist.church/livestream",
            "https://cedargrovebaptist.church/sermons",
            "https://cedargrovebaptist.church/ministries",
            "https://cedargrovebaptist.church/calendar",
            "https://cedargrovebaptist.church/churchindialogue",
            "https://cedargrovebaptist.church/menonmission",
            "https://cedargrovebaptist.church/womenonmission"
        };

        foreach (var page in expectedPages)
        {
            Assert.Contains(page, xml);
        }
    }

    [Fact]
    public void Handle_AllUrlsHaveWeeklyChangefreq()
    {
        var xml = GetSitemapXml();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var doc = XDocument.Parse(xml);
        var changefreqs = doc.Descendants(ns + "changefreq").Select(e => e.Value).ToList();
        Assert.All(changefreqs, cf => Assert.Equal("weekly", cf));
    }

    [Fact]
    public void Handle_AllUrlsHaveLastmod()
    {
        var xml = GetSitemapXml();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var doc = XDocument.Parse(xml);
        var lastmods = doc.Descendants(ns + "lastmod").Select(e => e.Value).ToList();
        Assert.Equal(9, lastmods.Count);

        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        Assert.All(lastmods, lm => Assert.Equal(today, lm));
    }
}
