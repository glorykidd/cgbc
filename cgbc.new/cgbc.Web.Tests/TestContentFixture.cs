using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace cgbc.Web.Tests;

/// <summary>
/// Creates a temporary Content directory structure with test data files
/// for use by ContentService tests.
/// </summary>
public class TestContentFixture : IDisposable
{
    public string ContentRoot { get; }

    public TestContentFixture()
    {
        ContentRoot = Path.Combine(Path.GetTempPath(), $"cgbc-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ContentRoot);
        CreateTestContent();
    }

    private void CreateTestContent()
    {
        // Staff markdown files
        var staffDir = Path.Combine(ContentRoot, "Content", "staff");
        Directory.CreateDirectory(staffDir);

        File.WriteAllText(Path.Combine(staffDir, "01-pastor.md"), """
            ---
            name: John Smith
            role: Senior Pastor
            image: /img/staff/john.jpg
            order: 1
            ---
            Bio content here.
            """);

        File.WriteAllText(Path.Combine(staffDir, "02-deacon.md"), """
            ---
            name: Jane Doe
            role: Deacon
            image: /img/staff/jane.jpg
            order: 2
            ---
            Another bio.
            """);

        File.WriteAllText(Path.Combine(staffDir, "03-no-order.md"), """
            ---
            name: Bob Wilson
            role: Youth Leader
            image: /img/staff/bob.jpg
            ---
            No order specified.
            """);

        // Slider markdown files
        var sliderDir = Path.Combine(ContentRoot, "Content", "slider");
        Directory.CreateDirectory(sliderDir);

        File.WriteAllText(Path.Combine(sliderDir, "01-welcome.md"), """
            ---
            title: Welcome to CGBC
            summary: Join us for worship
            footer: Every Sunday
            url: /about
            buttonText: Learn More
            ---
            """);

        File.WriteAllText(Path.Combine(sliderDir, "02-events.md"), """
            ---
            title: Upcoming Events
            summary: Check our calendar
            footer: ""
            url: /calendar
            buttonText: View Calendar
            ---
            """);

        // Ministry slider JSON
        var ministriesDir = Path.Combine(ContentRoot, "Content", "ministries");
        Directory.CreateDirectory(ministriesDir);

        File.WriteAllText(Path.Combine(ministriesDir, "_slider.json"), """
            [
                { "title": "Men on Mission", "summary": "Men's ministry", "imageUrl": "/img/men.jpg" },
                { "title": "Women on Mission", "summary": "Women's ministry", "imageUrl": "/img/women.jpg" }
            ]
            """);

        File.WriteAllText(Path.Combine(ministriesDir, "mens.json"), """
            [
                { "imageUrl": "/img/mens/1.jpg" },
                { "imageUrl": "/img/mens/2.jpg" }
            ]
            """);

        File.WriteAllText(Path.Combine(ministriesDir, "womens.json"), """
            [
                { "imageUrl": "/img/womens/1.jpg" }
            ]
            """);

        // Pages SEO markdown
        var pagesDir = Path.Combine(ContentRoot, "Content", "pages");
        Directory.CreateDirectory(pagesDir);

        File.WriteAllText(Path.Combine(pagesDir, "home.md"), """
            ---
            title: Cedar Grove Baptist Church - Home
            description: Welcome to Cedar Grove Baptist Church
            keywords: church, baptist, home
            ogImage: /img/home-og.jpg
            canonicalUrl: https://cedargrovebaptist.church/
            ---
            """);

        File.WriteAllText(Path.Combine(pagesDir, "about.md"), """
            ---
            title: About Us
            description: Learn about our church
            keywords: about, staff, history
            canonicalUrl: https://cedargrovebaptist.church/about
            ---
            """);
    }

    public IWebHostEnvironment CreateMockEnvironment()
    {
        return new FakeWebHostEnvironment(ContentRoot);
    }

    public IWebHostEnvironment CreateEmptyEnvironment()
    {
        var emptyRoot = Path.Combine(Path.GetTempPath(), $"cgbc-empty-{Guid.NewGuid():N}");
        Directory.CreateDirectory(emptyRoot);
        Directory.CreateDirectory(Path.Combine(emptyRoot, "Content"));
        return new FakeWebHostEnvironment(emptyRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(ContentRoot))
            Directory.Delete(ContentRoot, true);
    }

    private class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
            EnvironmentName = "Testing";
            ApplicationName = "cgbc.Web.Tests";
            ContentRootFileProvider = new NullFileProvider();
            WebRootFileProvider = new NullFileProvider();
        }

        public string ContentRootPath { get; set; }
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
    }
}
