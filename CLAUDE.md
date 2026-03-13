# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Cedar Grove Baptist Church (CGBC) website is a .NET 10.0 Blazor Static SSR application with server-side rendering for SEO. Interactive components (carousels, tabs, modals) use `@rendermode InteractiveServer` via SignalR. Content is managed through markdown files with YAML frontmatter and JSON files in the `Content/` directory. The site is deployed automatically to a self-hosted Windows server (IIS + ASP.NET Core Module) when changes are pushed to the `main` branch.

## Solution Structure

```
cgbc.new/
├── cgbc.Web/         # Blazor Static SSR host (ACTIVE - main application)
├── cgbcWeb/          # Blazor WebAssembly frontend (RETIRED - kept for reference)
├── cgbc.api/         # ASP.NET Core Minimal API (proof of concept, not actively used)
└── cgbc.sln          # Visual Studio solution file
```

The Blazor SSR project (`cgbc.Web`) contains:
- **Components/Pages/**: 8 Razor components for different routes (Home, About, Ministries, Sermons, Calendar, etc.)
- **Components/Layout/**: Shared layout components (MainLayout, NavMenu, Footer)
- **Components/Shared/**: Reusable interactive components (SeoHead, HomeCarousel, MinistryTabs, ImageCarousel, VideoModal, MinistryCarousel)
- **Services/**: ContentService (reads markdown + JSON content), SeoService (JSON-LD structured data)
- **Models/**: StaffMember, SliderContent, MinistrySliderContent, ImageSlide, SeoMetadata
- **Content/**: Markdown and JSON content files (staff/, ministries/, slider/, pages/)
- **Endpoints/**: SitemapEndpoint (dynamic sitemap.xml)
- **wwwroot/**: Static assets (img/, css/, robots.txt, favicon.png)

## Development Commands

All commands should be run from the `cgbc.new/cgbc.Web/` directory unless otherwise specified.

### Build
```bash
dotnet build
```

### Run Development Server
```bash
dotnet run
```
- HTTP: http://localhost:5287
- HTTPS: https://localhost:7263

### Publish for Production
```bash
dotnet publish -c Release -o C:/www-root/cedargrovebaptist.church
```

## Architecture & Data Flow

This is a **server-rendered application** with interactive islands:

1. User requests a page - ASP.NET Core renders full HTML on the server
2. Page components use `ContentService` (injected singleton) to load data from `Content/` directory
3. Static pages render as pure SSR HTML (zero JS) - Calendar, Sermons, Churchindialogue, About
4. Interactive components use `@rendermode InteractiveServer` (SignalR) - carousels, tabs, modals
5. `blazor.web.js` enables enhanced navigation (SPA-like partial page swaps without full reloads)
6. SEO metadata (meta tags, Open Graph, JSON-LD) rendered server-side via `SeoHead` component

### Content Files
- `Content/staff/*.md` - Staff directory (YAML frontmatter: name, role, image, order)
- `Content/slider/*.md` - Homepage carousel items (YAML frontmatter)
- `Content/ministries/_slider.json` - Ministry organization carousel
- `Content/ministries/mens.json`, `womens.json` - Image URL lists for photo carousels
- `Content/pages/*.md` - Per-page SEO metadata (title, description, keywords, ogImage)

### SEO Features
- Full HTML in view-source (no empty div)
- `<SeoHead>` component on every page with meta/OG/Twitter Card/JSON-LD
- Dynamic `/sitemap.xml` endpoint
- `robots.txt` in wwwroot
- Schema.org Church structured data

## Technology Stack

- **.NET 10.0** with C# (nullable reference types enabled, implicit usings enabled)
- **Blazor Static SSR** with InteractiveServer components (SignalR)
- **Blazor Bootstrap** (v3.5.0) - Bootstrap component library
- **Markdig** - Markdown processing with YAML frontmatter
- **Bootstrap 5.3.7** - CSS framework (CDN)

## Deployment

Deployment is automated via GitHub Actions (`.github/workflows/cgbc.yml`):

- **Trigger**: Push to `main` branch with changes in `cgbc.new/**`
- **Target**: Self-hosted Windows runner with IIS + ASP.NET Core Module (ANCM)
- **Deployment path**: `C:/www-root/cedargrovebaptist.church`
- **Backup**: Creates timestamped backup in `C:/backups/` before each deployment
- **Process**:
  1. Checkout code
  2. Generate timestamped build number
  3. Create deployment manifest
  4. Backup current production
  5. Stop IIS site (releases DLL locks)
  6. Build and publish Blazor SSR project
  7. Start IIS site

**Server requirements**: .NET 10 Hosting Bundle, IIS Application Pool (No Managed Code), WebSocket support enabled.

## Working with Content

To update church content (staff, ministries, slider images, etc.), edit files in `cgbc.new/cgbc.Web/Content/`. Staff and slider content use markdown with YAML frontmatter. Ministry slider and image lists use JSON.

When adding new pages:
1. Create a new `.razor` file in `Components/Pages/`
2. Add the route with `@page "/route"` directive
3. Add `<SeoHead Page="pagename" PagePath="/route" />` component
4. Create `Content/pages/pagename.md` with SEO metadata
5. Update `NavMenu.razor` to include navigation link
6. Add the route to `Endpoints/SitemapEndpoint.cs`

## Branch Strategy

- **main**: Production branch (auto-deploys on push)
- **develop**: Development branch for testing changes before merging to main
- Always create feature/fix branches from `develop`
- PRs must target `develop` first — never merge directly to `main`
- After changes are validated on `develop`, merge `develop` into `main` to deploy to production
