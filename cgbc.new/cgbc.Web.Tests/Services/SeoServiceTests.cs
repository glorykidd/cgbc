using System.Text.Json;
using cgbc.Web.Services;

namespace cgbc.Web.Tests.Services;

public class SeoServiceTests
{
    [Fact]
    public void GetChurchJsonLd_ReturnsValidJson()
    {
        var json = SeoService.GetChurchJsonLd();
        var doc = JsonDocument.Parse(json);
        Assert.NotNull(doc);
    }

    [Fact]
    public void GetChurchJsonLd_ContainsSchemaContext()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("https://schema.org", doc.RootElement.GetProperty("@context").GetString());
    }

    [Fact]
    public void GetChurchJsonLd_TypeIsChurch()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Church", doc.RootElement.GetProperty("@type").GetString());
    }

    [Fact]
    public void GetChurchJsonLd_ContainsChurchName()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Cedar Grove Baptist Church", doc.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public void GetChurchJsonLd_ContainsTelephone()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("+1-502-543-4101", doc.RootElement.GetProperty("telephone").GetString());
    }

    [Fact]
    public void GetChurchJsonLd_ContainsAddress()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        var address = doc.RootElement.GetProperty("address");
        Assert.Equal("PostalAddress", address.GetProperty("@type").GetString());
        Assert.Equal("4900 Cedar Grove Rd", address.GetProperty("streetAddress").GetString());
        Assert.Equal("Shepherdsville", address.GetProperty("addressLocality").GetString());
        Assert.Equal("KY", address.GetProperty("addressRegion").GetString());
        Assert.Equal("40165", address.GetProperty("postalCode").GetString());
        Assert.Equal("US", address.GetProperty("addressCountry").GetString());
    }

    [Fact]
    public void GetChurchJsonLd_ContainsGeoCoordinates()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        var geo = doc.RootElement.GetProperty("geo");
        Assert.Equal("GeoCoordinates", geo.GetProperty("@type").GetString());
        Assert.Equal(37.9887, geo.GetProperty("latitude").GetDouble(), 4);
        Assert.Equal(-85.7138, geo.GetProperty("longitude").GetDouble(), 4);
    }

    [Fact]
    public void GetChurchJsonLd_ContainsSocialMediaLinks()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        var sameAs = doc.RootElement.GetProperty("sameAs");
        Assert.Equal(2, sameAs.GetArrayLength());
    }

    [Fact]
    public void GetChurchJsonLd_ContainsWebsiteUrl()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("https://cedargrovebaptist.church", doc.RootElement.GetProperty("url").GetString());
    }

    [Fact]
    public void GetChurchJsonLd_ContainsDescription()
    {
        var json = SeoService.GetChurchJsonLd();
        using var doc = JsonDocument.Parse(json);
        var description = doc.RootElement.GetProperty("description").GetString();
        Assert.NotNull(description);
        Assert.Contains("Christ-centered", description);
        Assert.Contains("Shepherdsville", description);
    }
}
