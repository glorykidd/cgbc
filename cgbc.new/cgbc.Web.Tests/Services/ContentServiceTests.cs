using cgbc.Web.Services;
using cgbc.Web.Models;

namespace cgbc.Web.Tests.Services;

public class ContentServiceTests : IClassFixture<TestContentFixture>
{
    private readonly TestContentFixture _fixture;

    public ContentServiceTests(TestContentFixture fixture)
    {
        _fixture = fixture;
    }

    // --- GetStaff ---

    [Fact]
    public void GetStaff_ReturnsAllStaffMembers()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var staff = service.GetStaff();
        Assert.Equal(3, staff.Count);
    }

    [Fact]
    public void GetStaff_ParsesNameAndRole()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var staff = service.GetStaff();
        var pastor = staff.First(s => s.Name == "John Smith");
        Assert.Equal("Senior Pastor", pastor.Role);
    }

    [Fact]
    public void GetStaff_ParsesImageUrl()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var staff = service.GetStaff();
        var pastor = staff.First(s => s.Name == "John Smith");
        Assert.Equal("/img/staff/john.jpg", pastor.ImageUrl);
    }

    [Fact]
    public void GetStaff_OrdersByOrderField()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var staff = service.GetStaff();
        Assert.Equal("John Smith", staff[0].Name);
        Assert.Equal("Jane Doe", staff[1].Name);
    }

    [Fact]
    public void GetStaff_MissingOrderDefaultsTo99()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var staff = service.GetStaff();
        var bob = staff.First(s => s.Name == "Bob Wilson");
        Assert.Equal(99, bob.Order);
    }

    [Fact]
    public void GetStaff_CachesResults()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var first = service.GetStaff();
        var second = service.GetStaff();
        Assert.Same(first, second);
    }

    [Fact]
    public void GetStaff_EmptyDirectory_ReturnsEmptyList()
    {
        var service = new ContentService(_fixture.CreateEmptyEnvironment());
        var staff = service.GetStaff();
        Assert.Empty(staff);
    }

    // --- GetSliderContent ---

    [Fact]
    public void GetSliderContent_ReturnsAllSlides()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var slides = service.GetSliderContent();
        Assert.Equal(2, slides.Count);
    }

    [Fact]
    public void GetSliderContent_ParsesFrontmatter()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var slides = service.GetSliderContent();
        var welcome = slides.First(s => s.Title == "Welcome to CGBC");
        Assert.Equal("Join us for worship", welcome.Summary);
        Assert.Equal("Every Sunday", welcome.Footer);
        Assert.Equal("/about", welcome.Url);
        Assert.Equal("Learn More", welcome.ButtonText);
    }

    [Fact]
    public void GetSliderContent_OrdersByFilename()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var slides = service.GetSliderContent();
        Assert.Equal("Welcome to CGBC", slides[0].Title);
        Assert.Equal("Upcoming Events", slides[1].Title);
    }

    [Fact]
    public void GetSliderContent_CachesResults()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var first = service.GetSliderContent();
        var second = service.GetSliderContent();
        Assert.Same(first, second);
    }

    [Fact]
    public void GetSliderContent_EmptyDirectory_ReturnsEmptyList()
    {
        var service = new ContentService(_fixture.CreateEmptyEnvironment());
        var slides = service.GetSliderContent();
        Assert.Empty(slides);
    }

    // --- GetMinistrySliderContent ---

    [Fact]
    public void GetMinistrySliderContent_ReturnsAllMinistries()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var ministries = service.GetMinistrySliderContent();
        Assert.Equal(2, ministries.Count);
    }

    [Fact]
    public void GetMinistrySliderContent_ParsesJson()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var ministries = service.GetMinistrySliderContent();
        Assert.Equal("Men on Mission", ministries[0].Title);
        Assert.Equal("Men's ministry", ministries[0].Summary);
        Assert.Equal("/img/men.jpg", ministries[0].ImageUrl);
    }

    [Fact]
    public void GetMinistrySliderContent_CachesResults()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var first = service.GetMinistrySliderContent();
        var second = service.GetMinistrySliderContent();
        Assert.Same(first, second);
    }

    [Fact]
    public void GetMinistrySliderContent_MissingFile_ReturnsEmptyList()
    {
        var service = new ContentService(_fixture.CreateEmptyEnvironment());
        var ministries = service.GetMinistrySliderContent();
        Assert.Empty(ministries);
    }

    // --- GetMensImages ---

    [Fact]
    public void GetMensImages_ReturnsAllImages()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var images = service.GetMensImages();
        Assert.Equal(2, images.Count);
    }

    [Fact]
    public void GetMensImages_ParsesImageUrls()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var images = service.GetMensImages();
        Assert.Equal("/img/mens/1.jpg", images[0].ImageUrl);
        Assert.Equal("/img/mens/2.jpg", images[1].ImageUrl);
    }

    [Fact]
    public void GetMensImages_CachesResults()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var first = service.GetMensImages();
        var second = service.GetMensImages();
        Assert.Same(first, second);
    }

    [Fact]
    public void GetMensImages_MissingFile_ReturnsEmptyList()
    {
        var service = new ContentService(_fixture.CreateEmptyEnvironment());
        var images = service.GetMensImages();
        Assert.Empty(images);
    }

    // --- GetWomensImages ---

    [Fact]
    public void GetWomensImages_ReturnsAllImages()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var images = service.GetWomensImages();
        Assert.Single(images);
    }

    [Fact]
    public void GetWomensImages_ParsesImageUrl()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var images = service.GetWomensImages();
        Assert.Equal("/img/womens/1.jpg", images[0].ImageUrl);
    }

    [Fact]
    public void GetWomensImages_CachesResults()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var first = service.GetWomensImages();
        var second = service.GetWomensImages();
        Assert.Same(first, second);
    }

    [Fact]
    public void GetWomensImages_MissingFile_ReturnsEmptyList()
    {
        var service = new ContentService(_fixture.CreateEmptyEnvironment());
        var images = service.GetWomensImages();
        Assert.Empty(images);
    }

    // --- GetSeoMetadata ---

    [Fact]
    public void GetSeoMetadata_ReturnsMetadataForKnownPage()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var meta = service.GetSeoMetadata("home");
        Assert.Equal("Cedar Grove Baptist Church - Home", meta.Title);
        Assert.Equal("Welcome to Cedar Grove Baptist Church", meta.Description);
        Assert.Equal("church, baptist, home", meta.Keywords);
    }

    [Fact]
    public void GetSeoMetadata_ParsesOgImage()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var meta = service.GetSeoMetadata("home");
        Assert.Equal("/img/home-og.jpg", meta.OgImage);
    }

    [Fact]
    public void GetSeoMetadata_MissingOgImage_UsesDefault()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var meta = service.GetSeoMetadata("about");
        Assert.Equal("/img/church-background.jpeg", meta.OgImage);
    }

    [Fact]
    public void GetSeoMetadata_ParsesCanonicalUrl()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var meta = service.GetSeoMetadata("home");
        Assert.Equal("https://cedargrovebaptist.church/", meta.CanonicalUrl);
    }

    [Fact]
    public void GetSeoMetadata_CaseInsensitiveLookup()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var meta = service.GetSeoMetadata("HOME");
        Assert.Equal("Cedar Grove Baptist Church - Home", meta.Title);
    }

    [Fact]
    public void GetSeoMetadata_UnknownPage_ReturnsDefaults()
    {
        var service = new ContentService(_fixture.CreateMockEnvironment());
        var meta = service.GetSeoMetadata("nonexistent");
        Assert.Equal("Cedar Grove Baptist Church", meta.Title);
        Assert.Contains("Shepherdsville", meta.Description);
    }

    [Fact]
    public void GetSeoMetadata_EmptyDirectory_ReturnsDefaults()
    {
        var service = new ContentService(_fixture.CreateEmptyEnvironment());
        var meta = service.GetSeoMetadata("home");
        Assert.Equal("Cedar Grove Baptist Church", meta.Title);
    }
}
