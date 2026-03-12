using System.Text.Json;
using cgbc.Web.Models;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace cgbc.Web.Services;

public class ContentService
{
    private readonly string _contentRoot;
    private readonly MarkdownPipeline _pipeline;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private List<StaffMember>? _staffCache;
    private List<SliderContent>? _sliderCache;
    private List<MinistrySliderContent>? _ministrySliderCache;
    private List<ImageSlide>? _mensCache;
    private List<ImageSlide>? _womensCache;
    private Dictionary<string, SeoMetadata>? _seoCache;

    public ContentService(IWebHostEnvironment env)
    {
        _contentRoot = Path.Combine(env.ContentRootPath, "Content");
        _pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();
    }

    public List<StaffMember> GetStaff()
    {
        if (_staffCache != null) return _staffCache;

        var staffDir = Path.Combine(_contentRoot, "staff");
        var staff = new List<StaffMember>();

        if (!Directory.Exists(staffDir)) return staff;

        foreach (var file in Directory.GetFiles(staffDir, "*.md"))
        {
            var content = File.ReadAllText(file);
            var frontmatter = ExtractFrontmatter(content);
            staff.Add(new StaffMember
            {
                Name = GetValue(frontmatter, "name"),
                Role = GetValue(frontmatter, "role"),
                ImageUrl = GetValue(frontmatter, "image"),
                Order = int.TryParse(GetValue(frontmatter, "order"), out var o) ? o : 99
            });
        }

        _staffCache = staff.OrderBy(s => s.Order).ToList();
        return _staffCache;
    }

    public List<SliderContent> GetSliderContent()
    {
        if (_sliderCache != null) return _sliderCache;

        var sliderDir = Path.Combine(_contentRoot, "slider");
        var slides = new List<SliderContent>();

        if (!Directory.Exists(sliderDir)) return slides;

        foreach (var file in Directory.GetFiles(sliderDir, "*.md").OrderBy(f => f))
        {
            var content = File.ReadAllText(file);
            var frontmatter = ExtractFrontmatter(content);
            slides.Add(new SliderContent
            {
                Title = GetValue(frontmatter, "title"),
                Summary = GetValue(frontmatter, "summary"),
                Footer = GetValue(frontmatter, "footer"),
                Url = GetValue(frontmatter, "url"),
                ButtonText = GetValue(frontmatter, "buttonText")
            });
        }

        _sliderCache = slides;
        return _sliderCache;
    }

    public List<MinistrySliderContent> GetMinistrySliderContent()
    {
        if (_ministrySliderCache != null) return _ministrySliderCache;

        var file = Path.Combine(_contentRoot, "ministries", "_slider.json");
        if (!File.Exists(file)) return [];

        var json = File.ReadAllText(file);
        _ministrySliderCache = JsonSerializer.Deserialize<List<MinistrySliderContent>>(json, _jsonOptions) ?? [];
        return _ministrySliderCache;
    }

    public List<ImageSlide> GetMensImages()
    {
        if (_mensCache != null) return _mensCache;

        var file = Path.Combine(_contentRoot, "ministries", "mens.json");
        if (!File.Exists(file)) return [];

        var json = File.ReadAllText(file);
        _mensCache = JsonSerializer.Deserialize<List<ImageSlide>>(json, _jsonOptions) ?? [];
        return _mensCache;
    }

    public List<ImageSlide> GetWomensImages()
    {
        if (_womensCache != null) return _womensCache;

        var file = Path.Combine(_contentRoot, "ministries", "womens.json");
        if (!File.Exists(file)) return [];

        var json = File.ReadAllText(file);
        _womensCache = JsonSerializer.Deserialize<List<ImageSlide>>(json, _jsonOptions) ?? [];
        return _womensCache;
    }

    public SeoMetadata GetSeoMetadata(string page)
    {
        if (_seoCache == null)
        {
            _seoCache = new Dictionary<string, SeoMetadata>(StringComparer.OrdinalIgnoreCase);
            var pagesDir = Path.Combine(_contentRoot, "pages");
            if (Directory.Exists(pagesDir))
            {
                foreach (var file in Directory.GetFiles(pagesDir, "*.md"))
                {
                    var content = File.ReadAllText(file);
                    var frontmatter = ExtractFrontmatter(content);
                    var slug = Path.GetFileNameWithoutExtension(file);
                    _seoCache[slug] = new SeoMetadata
                    {
                        Title = GetValue(frontmatter, "title"),
                        Description = GetValue(frontmatter, "description"),
                        Keywords = GetValue(frontmatter, "keywords"),
                        OgImage = GetValueOrDefault(frontmatter, "ogImage", "/img/church-background.jpeg"),
                        CanonicalUrl = GetValue(frontmatter, "canonicalUrl")
                    };
                }
            }
        }

        return _seoCache.TryGetValue(page, out var meta) ? meta : new SeoMetadata();
    }

    private Dictionary<string, string> ExtractFrontmatter(string markdown)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var doc = Markdown.Parse(markdown, _pipeline);
        var yamlBlock = doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (yamlBlock == null) return result;

        var yaml = markdown.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);
        foreach (var line in yaml.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed == "---") continue;
            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0) continue;
            var key = trimmed[..colonIndex].Trim();
            var value = trimmed[(colonIndex + 1)..].Trim().Trim('"').Trim('\'');
            result[key] = value;
        }

        return result;
    }

    private static string GetValue(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out var val) ? val : "";

    private static string GetValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue)
        => dict.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val) ? val : defaultValue;
}
