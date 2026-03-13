using cgbc.Web.Models;

namespace cgbc.Web.Tests.Models;

public class StaffMemberTests
{
    [Fact]
    public void DefaultValues_AreEmptyStrings()
    {
        var member = new StaffMember();
        Assert.Equal("", member.Name);
        Assert.Equal("", member.Role);
        Assert.Equal("", member.ImageUrl);
    }

    [Fact]
    public void DefaultOrder_IsZero()
    {
        var member = new StaffMember();
        Assert.Equal(0, member.Order);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var member = new StaffMember
        {
            Name = "Test",
            Role = "Pastor",
            ImageUrl = "/img/test.jpg",
            Order = 5
        };
        Assert.Equal("Test", member.Name);
        Assert.Equal("Pastor", member.Role);
        Assert.Equal("/img/test.jpg", member.ImageUrl);
        Assert.Equal(5, member.Order);
    }
}

public class SeoMetadataTests
{
    [Fact]
    public void DefaultTitle_IsChurchName()
    {
        var meta = new SeoMetadata();
        Assert.Equal("Cedar Grove Baptist Church", meta.Title);
    }

    [Fact]
    public void DefaultDescription_ContainsShepherdsville()
    {
        var meta = new SeoMetadata();
        Assert.Contains("Shepherdsville", meta.Description);
    }

    [Fact]
    public void DefaultKeywords_ContainsChurch()
    {
        var meta = new SeoMetadata();
        Assert.Contains("church", meta.Keywords);
    }

    [Fact]
    public void DefaultOgImage_IsChurchBackground()
    {
        var meta = new SeoMetadata();
        Assert.Equal("/img/church-background.jpeg", meta.OgImage);
    }

    [Fact]
    public void DefaultOgType_IsWebsite()
    {
        var meta = new SeoMetadata();
        Assert.Equal("website", meta.OgType);
    }

    [Fact]
    public void DefaultCanonicalUrl_IsEmpty()
    {
        var meta = new SeoMetadata();
        Assert.Equal("", meta.CanonicalUrl);
    }
}

public class SliderContentTests
{
    [Fact]
    public void DefaultValues_AreEmptyStrings()
    {
        var slide = new SliderContent();
        Assert.Equal("", slide.Title);
        Assert.Equal("", slide.Summary);
        Assert.Equal("", slide.Footer);
        Assert.Equal("", slide.Url);
        Assert.Equal("", slide.ButtonText);
    }
}

public class MinistrySliderContentTests
{
    [Fact]
    public void DefaultValues_AreEmptyStrings()
    {
        var ministry = new MinistrySliderContent();
        Assert.Equal("", ministry.Title);
        Assert.Equal("", ministry.Summary);
        Assert.Equal("", ministry.ImageUrl);
    }
}

public class ImageSlideTests
{
    [Fact]
    public void DefaultImageUrl_IsEmptyString()
    {
        var slide = new ImageSlide();
        Assert.Equal("", slide.ImageUrl);
    }
}
