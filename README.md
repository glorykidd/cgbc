# Cedar Grove Baptist Church Website

A modern church website built with .NET 10.0 Blazor Static SSR, featuring server-side rendering for SEO, interactive components via SignalR, and automated deployment to IIS.

## Overview

The Cedar Grove Baptist Church (CGBC) website uses Blazor Static SSR to deliver fully-rendered HTML pages for optimal SEO and performance. Interactive components (carousels, tabs, modals) use `@rendermode InteractiveServer` via SignalR, eliminating the need to ship a .NET runtime to the browser. Content is managed through markdown files with YAML frontmatter.

## Tech Stack

### Frontend
- **.NET 10.0** - Latest .NET framework
- **Blazor Static SSR** - Server-side rendered pages with interactive islands
- **Blazor Bootstrap 3.5.0** - Bootstrap component library for Blazor
- **Bootstrap 5.3.7** - Responsive CSS framework
- **Bootstrap Icons 1.13.1** - Icon library
- **Google Analytics** - Website analytics (G-2SYKHRVV88)

### Backend
- **ASP.NET Core** (.NET 10.0) - SSR host with Kestrel
- **Markdig** - Markdown processing with YAML frontmatter
- **Response Compression** - Gzip/Brotli compression middleware
- **Static File Caching** - 7-day cache headers for images/CSS

### Testing
- **xUnit 2.9** - Unit test framework
- **Coverlet** - Code coverage collection

### Infrastructure
- **Self-hosted Windows Server** - Production hosting via IIS + ASP.NET Core Module (ANCM)
- **GitHub Actions** - CI/CD pipeline with IIS stop/start for zero-downtime deploys
- **PowerShell** - Deployment automation scripts

## Project Structure

```
cgbc/
├── cgbc.new/
│   ├── cgbc.sln                          # Visual Studio solution file
│   ├── cgbc.Web/                         # Main Blazor SSR application
│   │   ├── Components/
│   │   │   ├── App.razor                 # Root HTML shell (replaces index.html)
│   │   │   ├── Routes.razor              # Router component
│   │   │   ├── _Imports.razor            # Global usings
│   │   │   ├── Layout/
│   │   │   │   ├── MainLayout.razor      # Flexbox layout with sticky footer
│   │   │   │   ├── NavMenu.razor         # Navigation (Bootstrap native toggle)
│   │   │   │   └── Footer.razor          # Shared footer
│   │   │   ├── Pages/                    # 9 Razor page components (Static SSR)
│   │   │   │   ├── Home.razor
│   │   │   │   ├── About.razor
│   │   │   │   ├── Livestream.razor
│   │   │   │   ├── Ministries.razor
│   │   │   │   ├── Sermons.razor
│   │   │   │   ├── Calendar.razor
│   │   │   │   ├── Churchindialogue.razor
│   │   │   │   ├── Menonmission.razor
│   │   │   │   └── Womenonmission.razor
│   │   │   └── Shared/                   # Interactive components (SignalR)
│   │   │       ├── SeoHead.razor         # Meta tags, OG, JSON-LD
│   │   │       ├── HomeCarousel.razor    # @rendermode InteractiveServer
│   │   │       ├── MinistryCarousel.razor
│   │   │       ├── MinistryTabs.razor
│   │   │       ├── ImageCarousel.razor
│   │   │       └── VideoModal.razor
│   │   ├── Services/
│   │   │   ├── ContentService.cs         # Reads markdown + YAML frontmatter
│   │   │   └── SeoService.cs             # Schema.org JSON-LD generation
│   │   ├── Models/
│   │   │   ├── StaffMember.cs
│   │   │   ├── SliderContent.cs
│   │   │   ├── MinistrySliderContent.cs
│   │   │   ├── ImageSlide.cs
│   │   │   └── SeoMetadata.cs
│   │   ├── Content/                      # Markdown + JSON content files
│   │   │   ├── staff/                    # One .md per staff member
│   │   │   ├── ministries/               # Ministry slider + image data
│   │   │   ├── slider/                   # Homepage carousel slides
│   │   │   └── pages/                    # Per-page SEO metadata
│   │   ├── Endpoints/
│   │   │   └── SitemapEndpoint.cs        # Dynamic sitemap.xml
│   │   ├── wwwroot/
│   │   │   ├── img/                      # Static images
│   │   │   ├── css/app.css               # Custom styles
│   │   │   └── robots.txt
│   │   ├── Program.cs                    # ASP.NET Core SSR host
│   │   └── cgbc.Web.csproj
│   ├── cgbc.Web.Tests/                   # xUnit test project
│   │   ├── Services/                     # ContentService + SeoService tests
│   │   ├── Endpoints/                    # SitemapEndpoint tests
│   │   └── Models/                       # Model tests
│   └── cgbc.api/                         # ASP.NET Core API (unused)
├── .github/
│   └── workflows/
│       ├── cgbc.yml                      # Production deployment (main branch)
│       └── cgbc-develop.yml              # Build + test validation (develop branch)
├── CLAUDE.md                             # Claude Code instructions
└── README.md                             # This file
```

## Architecture

### Static SSR with Interactive Islands

Pages render as full HTML on the server (great for SEO). Interactive components opt-in to SignalR via `@rendermode InteractiveServer`:

- **Static SSR pages**: All 9 pages render complete HTML — search engines see full content
- **Interactive islands**: Carousels, tabs, and modals use SignalR for client interactivity
- **No WASM runtime**: No ~5MB .NET runtime download — pages load instantly as HTML

### Content System

Content is managed through markdown files with YAML frontmatter, processed by `ContentService` using Markdig:

| Content Directory | Purpose |
|-------------------|---------|
| `Content/staff/` | Staff member profiles (name, role, image, order) |
| `Content/slider/` | Homepage carousel slides |
| `Content/ministries/` | Ministry listings and image data |
| `Content/pages/` | Per-page SEO metadata (title, description, keywords) |

### SEO Features

- **Full HTML in view-source** — no empty `<div id="app">`
- **SeoHead component** on every page: `<title>`, meta description, keywords
- **Open Graph tags** for social sharing (Facebook, Twitter)
- **JSON-LD structured data** (Schema.org Church type)
- **Dynamic sitemap.xml** at `/sitemap.xml`
- **robots.txt** for search engine crawling
- **Canonical URLs** on all pages

## Development

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/glorykidd/cgbc.git
   cd cgbc/cgbc.new/cgbc.Web
   ```

2. **Build the project**
   ```bash
   dotnet build
   ```

3. **Run the development server**
   ```bash
   dotnet run
   ```
   - HTTP: http://localhost:5287
   - HTTPS: https://localhost:7263

### Running Tests

```bash
cd cgbc.new
dotnet test
```

Tests cover ContentService, SeoService, SitemapEndpoint, and model validation using xUnit.

### Development Workflow

- **Main branch**: Production branch (auto-deploys to live server)
- **Develop branch**: Development branch (builds and runs tests on push)
- Always create feature branches from `develop`
- Merge to `develop` for testing, then to `main` for production

### Publishing

To publish the application manually:

```bash
cd cgbc.new/cgbc.Web
dotnet publish -c Release -o [output-path]
```

## Deployment

### CI/CD Pipelines

Two GitHub Actions workflows run on a self-hosted Windows runner:

**Production (`cgbc.yml`)** — triggered on push to `main` with changes in `cgbc.new/**`:
1. Run xUnit test suite
2. Generate timestamped build number and deployment manifest
3. Backup current production files to `C:/backups/`
4. Stop IIS application pool (ASP.NET Core locks DLLs)
5. `dotnet publish` to production directory
6. Start IIS application pool
7. Email deployment notification

**Develop (`cgbc-develop.yml`)** — triggered on push to `develop` with changes in `cgbc.new/**`:
1. Run xUnit test suite
2. Build solution in Release configuration
3. Email build notification

### Server Requirements

- Windows Server with IIS
- **.NET 10.0 Hosting Bundle** (includes ASP.NET Core Module for IIS)
- IIS Application Pool configured for **No Managed Code**
- WebSocket protocol enabled in IIS (for SignalR)

### Rollback

In case of deployment issues, backups are stored at:
```
C:/backups/cgbc-[YYYY.MM.DD-HH.mm].zip
```

## Content Management

### Updating Content

To update church content:

1. Edit the appropriate markdown file in `cgbc.new/cgbc.Web/Content/`
2. Commit changes to `develop` branch for testing
3. Merge to `main` to deploy automatically

### Adding New Pages

1. Create a new `.razor` file in `cgbc.new/cgbc.Web/Components/Pages/`
2. Add the `@page "/route"` directive at the top
3. Add `<SeoHead>` component with page-specific metadata
4. Create a matching SEO metadata file in `Content/pages/`
5. Update `NavMenu.razor` to include the navigation link
6. Add the page to `SitemapEndpoint.cs`

### Adding Images

Place images in the appropriate directory under `wwwroot/img/`:
- Staff photos: `img/staff/`
- Ministry images: `img/ministry/`
- Detail images: `img/detail/`

## Contributing

1. Create a feature branch from `develop`
2. Make your changes
3. Test locally
4. Push to `develop` for integration testing
5. Create a pull request to `main` when ready to deploy

## License

Copyright Cedar Grove Baptist Church. All rights reserved.

## Support

For issues or questions about the website, please contact the church office or open an issue in this repository.
