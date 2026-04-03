# DNN Platform — Claude Code Demo Project

## Overview
DNN (formerly DotNetNuke) is a legacy .NET Framework CMS platform.
- **561K lines of C#** across 3,844 files
- Originally VB.NET, converted to C# over time
- ASP.NET WebForms + some MVC, targeting .NET Framework 4.6+
- Main solution: `DNN_Platform.sln`

## Top-Level Structure
- `DNN Platform/` — Core platform code
  - `Library/` — Core library (largest: entities, services, data access, security)
  - `Providers/` — Authentication, caching, HTML editor, folder providers
  - `Modules/` — Built-in modules (Export/Import, HTML, Journal, etc.)
  - `Tests/` — Unit and integration tests
  - `Website/` — ASP.NET WebForms site root
  - `HttpModules/` — HTTP pipeline modules
  - `Controls/` — Reusable UI controls
- `Dnn.AdminExperience/` — Admin panel (PersonaBar)
- `Build/` — MSBuild scripts and CI configuration
- `DotNetNuke.Internal.SourceGenerators/` — Roslyn source generators

## Key Architecture Patterns
- **Data access**: Abstract `DataProvider` class (4,310 lines) with stored procedure calls
- **Entities**: `UserController`, `TabController`, `PortalController`, `ModuleController` — all 2,000+ lines
- **Security**: `AspNetMembershipProvider` for auth, custom `PortalSecurity`
- **URL Rewriting**: `AdvancedUrlRewriter` (3,160 lines) — complex legacy URL handling
- **DI**: `Microsoft.Extensions.DependencyInjection` + legacy `ComponentModel` ServiceLocator
- **Upgrade system**: `Upgrade.cs` (2,793 lines) handles version-to-version migrations

## Largest Files (for demo purposes)
| File | Lines | Purpose |
|------|-------|---------|
| `Library/Data/DataProvider.cs` | 4,310 | Abstract data access layer — all DB operations |
| `Library/Common/Globals.cs` | 3,679 | Global utilities and constants |
| `Dnn.PersonaBar.Extensions/Services/SiteSettingsController.cs` | 3,554 | Admin site settings API |
| `Providers/.../CKEditorOptions.ascx.cs` | 3,489 | WebForms code-behind for HTML editor |
| `Library/Entities/Urls/AdvancedUrlRewriter.cs` | 3,160 | URL rewriting engine |
| `Library/Entities/Tabs/TabController.cs` | 3,115 | Page/tab management |
| `Library/Entities/Portals/PortalController.cs` | 3,017 | Portal (site) management |
| `Library/Entities/Users/UserController.cs` | 2,495 | User management |

## Conventions
- Namespace: `DotNetNuke.*` (legacy) or `DotNetNuke.Abstractions.*` (newer)
- Controller classes use ServiceLocator pattern for singleton access
- Partial classes with source generators for newer code
- `[Obsolete]` attributes mark migration path from old APIs to new
