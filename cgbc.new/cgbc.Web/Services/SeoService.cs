using System.Text.Json;

namespace cgbc.Web.Services;

public static class SeoService
{
    public static string GetChurchJsonLd()
    {
        var data = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Church",
            ["name"] = "Cedar Grove Baptist Church",
            ["description"] = "A Christ-centered Baptist church in Shepherdsville, Kentucky committed to worship, instruction, fellowship, and evangelism.",
            ["url"] = "https://cedargrovebaptist.church",
            ["telephone"] = "+1-502-543-4101",
            ["address"] = new Dictionary<string, object>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = "4900 Cedar Grove Rd",
                ["addressLocality"] = "Shepherdsville",
                ["addressRegion"] = "KY",
                ["postalCode"] = "40165",
                ["addressCountry"] = "US"
            },
            ["geo"] = new Dictionary<string, object>
            {
                ["@type"] = "GeoCoordinates",
                ["latitude"] = 37.9887,
                ["longitude"] = -85.7138
            },
            ["sameAs"] = new[]
            {
                "https://www.facebook.com/cedargrove4900",
                "https://www.youtube.com/channel/UC9klpwpBFYOrF8QXkz8sVfg"
            }
        };

        return JsonSerializer.Serialize(data);
    }
}
