using System.Text;

namespace cgbc.Web.Endpoints;

public static class SitemapEndpoint
{
    private static readonly (string Loc, string Priority)[] Pages =
    [
        ("/", "1.0"),
        ("/about", "0.8"),
        ("/livestream", "0.8"),
        ("/sermons", "0.8"),
        ("/ministries", "0.8"),
        ("/calendar", "0.7"),
        ("/churchindialogue", "0.6"),
        ("/menonmission", "0.6"),
        ("/womenonmission", "0.6"),
        ("/connect", "0.8"),
    ];

    public static IResult Handle()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        var lastmod = DateTime.UtcNow.ToString("yyyy-MM-dd");
        foreach (var (loc, priority) in Pages)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>https://cedargrovebaptist.church{loc}</loc>");
            sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
            sb.AppendLine($"    <changefreq>weekly</changefreq>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");
        return Results.Content(sb.ToString(), "application/xml");
    }
}
