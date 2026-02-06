# Cedar Grove Baptist Church Website

A modern church website built with .NET 10.0 Blazor WebAssembly, featuring automated deployment and static content management.

## Overview

The Cedar Grove Baptist Church (CGBC) website is a single-page application (SPA) that provides information about the church's ministries, staff, events, and sermons. The application uses Blazor WebAssembly for client-side rendering and loads all content from static JSON files, eliminating the need for a backend database.

## Tech Stack

### Frontend
- **.NET 10.0** - Latest .NET framework
- **Blazor WebAssembly** - Client-side SPA framework
- **Blazor Bootstrap 3.5.0** - Bootstrap component library for Blazor
- **Bootstrap 5.3.7** - Responsive CSS framework
- **Bootstrap Icons 1.13.1** - Icon library
- **Chart.js 4.0.1** - Data visualization library
- **Google Analytics** - Website analytics

### Backend
- **ASP.NET Core Minimal API** (.NET 10.0) - Proof of concept, not actively used
- **Microsoft.AspNetCore.OpenApi** - API documentation support

### Infrastructure
- **Self-hosted Windows Server** - Production hosting
- **GitHub Actions** - CI/CD pipeline
- **PowerShell** - Deployment automation scripts

## Project Structure

```
cgbc/
├── cgbc.new/
│   ├── cgbc.sln                      # Visual Studio solution file
│   ├── cgbcWeb/                      # Main Blazor WebAssembly application
│   │   ├── Pages/                    # Razor page components (8 pages)
│   │   │   ├── Home.razor            # Homepage with carousel
│   │   │   ├── About.razor           # About the church
│   │   │   ├── Ministries.razor      # Ministry listings
│   │   │   ├── Sermons.razor         # Sermon archive
│   │   │   ├── Calendar.razor        # Events calendar
│   │   │   ├── Churchindialogue.razor
│   │   │   ├── Menonmission.razor    # Men's ministry
│   │   │   └── Womennonmission.razor # Women's ministry
│   │   ├── Layout/                   # Shared layout components
│   │   │   ├── MainLayout.razor
│   │   │   ├── NavMenu.razor
│   │   │   └── Footer.razor
│   │   ├── wwwroot/
│   │   │   ├── data/                 # JSON content files
│   │   │   │   ├── slider.json       # Homepage carousel
│   │   │   │   ├── staff.json        # Staff directory
│   │   │   │   ├── ministry.json     # Ministry listings
│   │   │   │   ├── mens.json         # Men's ministry
│   │   │   │   └── womens.json       # Women's ministry
│   │   │   ├── img/                  # Static images
│   │   │   │   ├── staff/
│   │   │   │   ├── ministry/
│   │   │   │   └── detail/
│   │   │   ├── css/                  # Custom stylesheets
│   │   │   └── index.html            # SPA entry point
│   │   ├── Program.cs                # Application entry point
│   │   ├── App.razor                 # Root component
│   │   ├── _Imports.razor            # Global using statements
│   │   └── cgbcWeb.csproj            # Project file
│   └── cgbc.api/                     # ASP.NET Core API (unused)
│       └── cgbc.api.csproj
├── .github/
│   └── workflows/
│       └── cgbc.yml                  # Deployment workflow
├── CLAUDE.md                         # Claude Code instructions
└── README.md                         # This file
```

## Data Architecture

The application uses a **static content model**:

1. All church data is stored in JSON files in `wwwroot/data/`
2. Razor components use `HttpClient.GetFromJsonAsync<T>()` to load data
3. Data is type-safe through C# model classes
4. No database or API calls required for production

### Key Data Files

| File | Purpose |
|------|---------|
| `slider.json` | Homepage carousel slides |
| `staff.json` | Staff directory with photos |
| `ministry.json` | Ministry organization listings |
| `mens.json` | Men's ministry content |
| `womens.json` | Women's ministry content |

## Development

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/glorykidd/cgbc.git
   cd cgbc/cgbc.new/cgbcWeb
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

### Development Workflow

- **Main branch**: Production branch (auto-deploys to live server)
- **Develop branch**: Development branch for testing changes
- Always create feature branches from `develop`
- Merge to `develop` for testing, then to `main` for production

### Publishing

To publish the application manually:

```bash
cd cgbc.new/cgbcWeb
dotnet publish -c Release -o [output-path]
```

## Deployment

### Automated Deployment

The application deploys automatically via GitHub Actions when changes are pushed to the `main` branch:

1. **Trigger**: Push to `main` with changes in `cgbc.new/**`
2. **Runner**: Self-hosted Windows server
3. **Process**:
   - Checkout code
   - Generate timestamped build number
   - Create deployment manifest with commit details
   - Backup current production
   - Build and publish to live website

### Deployment Configuration

The deployment workflow (`.github/workflows/cgbc.yml`) includes:
- Automatic backup creation before each deployment
- Timestamped deployment manifests
- Changed file tracking
- Compression of backups for storage efficiency

### Rollback

In case of deployment issues, backups are stored at:
```
C:/backups/cgbc-[YYYY.MM.DD-HH.mm].zip
```

## Content Management

### Updating Content

To update church content:

1. Edit the appropriate JSON file in `cgbc.new/cgbcWeb/wwwroot/data/`
2. Commit changes to `develop` branch for testing
3. Merge to `main` to deploy automatically

### Adding New Pages

1. Create a new `.razor` file in `cgbc.new/cgbcWeb/Pages/`
2. Add the `@page "/route"` directive at the top
3. Update `NavMenu.razor` to include the navigation link
4. Follow existing patterns for loading JSON data if needed

### Adding Images

Place images in the appropriate directory:
- Staff photos: `wwwroot/img/staff/`
- Ministry images: `wwwroot/img/ministry/`
- Detail images: `wwwroot/img/detail/`

## Project Features

### Current Features
- Responsive design with Bootstrap 5
- Homepage carousel with custom slides
- Staff directory with photos
- Ministry listings and details
- Event calendar
- Sermon archive
- Google Analytics integration

### Architecture Highlights
- Client-side rendering (no server load after initial page load)
- Type-safe data models throughout
- Component-based architecture
- Automated deployment with safety backups
- No database required

## C# Language Features

The project uses modern C# features:
- Nullable reference types enabled
- Implicit usings enabled
- Top-level statements in `Program.cs`
- Async/await patterns throughout

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
