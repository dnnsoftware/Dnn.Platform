# MVC Module Control Implementation

## Overview

The MVC Module Control implementation provides a modern alternative to DNN's traditional WebForms-based module rendering pipeline. This new system enables DNN modules to leverage ASP.NET MVC architecture while maintaining compatibility with the existing DNN framework.

## Problem Statement

DNN Platform has historically relied on the WebForms pipeline accessed through `/default.aspx`. As outlined in [GitHub issue #6679](https://github.com/dnnsoftware/Dnn.Platform/issues/6679).

## Solution: Hybrid Pipeline Architecture

The MVC Pipeline introduces a dual-rendering mechanism:

1. **Legacy Pipeline**: Traditional WebForms through `/default.aspx`
2. **New MVC Pipeline**: Modern MVC rendering through `/DesktopModules/Default/Page/{tabId}/{locale}`

### Module Pipeline Support Matrix

Based on the GitHub issue specifications, modules can support different pipeline patterns:

| WebForms Support | MVC Module Support | SPA Module Support |
|------------------|--------------------|--------------------|
| Custom Control + Razor view | Use generic Control + Custom MVC Controller as child controller (shared with WebForms pipeline) | Use generic Control + return directly the html (shared with WebForms pipeline) |  |
| Render Razor Partial | The generic control redirects to the controller defined in Control Src |The generic Control renders the html file defined in Control Src | |

### Module Control Class Configuration

Modules specify their MVC compatibility through:
- **MVC Control Class**: Defined in module control settings and module manifest
- **Interface Implementation**: Must implement `IMvcModuleControl`
- **Optional Interfaces**: Can implement `IActionable` for unified action handling
- **Pipeline Detection**: System can determine module compatibility and show appropriate messages

## Core Components

### 1. IMvcModuleControl Interface

```csharp
public interface IMvcModuleControl : IModuleControl
{
    IHtmlString Html(HtmlHelper htmlHelper);
}
```

The base interface that all MVC module controls must implement, extending the standard `IModuleControl` with MVC-specific rendering capabilities. This interface enables:

- **Pipeline Compatibility Detection**: The system can determine if a module supports the MVC pipeline
- **Unified Rendering**: The `Html()` method provides access to `HtmlHelper` with information about HttpContext, controller, and page model
- **Flexible Rendering Options**: Modules can use HTML helpers to render content (Razor partials, child action controllers, or other helpers)

### 2. DefaultMvcModuleControlBase

The abstract base class that provides common functionality for all MVC module controls:

- **Dependency Injection**: Integrated service provider access
- **Module Context**: Access to DNN module configuration and settings
- **Portal Context**: Portal settings, user information, and localization
- **Resource Management**: Localization helpers and resource file management
- **URL Generation**: Helper methods for creating edit URLs

**Key Features:**
- Service scoped dependency injection
- Automatic resource file path resolution
- Portal and user context access
- Module settings management
- Edit URL generation with MVC support

### 3. Module Control Implementations

#### MvcModuleControl
The standard MVC module control for traditional MVC controllers and actions.

**Features:**
- Parses `.mvc` control source to extract controller and action names
- Supports routing with namespaces: `{namespace}/{controller}/{action}`
- Automatic query string parameter mapping
- Route value dictionary construction for MVC action execution
- Localization resource file resolution

**Control Source Format:**
```
{namespace}/{controller}/{action}.mvc
```

#### SpaModuleControl
Specialized control for Single Page Applications.

**Features:**
- HTML5 file rendering with token replacement
- Automatic CSS and JavaScript file inclusion
- File existence caching for performance
- Support for HTML5 module token system
- Content caching with file dependency tracking

**Supported Files:**
- `.html` or custom HTML5 files
- Automatic `.css` file inclusion (same name)
- Automatic `.js` file inclusion (same name)

#### RazorModuleControlBase
Abstract base for modules using Razor view rendering. 
This use MVC 5 razor views.
Recomended for Weforms control migrations
Folows the ViewComponent patern of .net Core for easy future trasition to .net Core

**Features:**
- Direct Razor view rendering
- Model binding support
- Custom view context management
- Flexible view name resolution
- Request/Response context integration

**Usage Pattern:**
```csharp
public class MyModuleControl : RazorModuleControlBase
{
    public override IRazorModuleResult Invoke()
    {
        var model = GetMyModel();
        return View("MyView", model);
    }
}
```

### 4. Extension Methods (MvcModuleControlExtensions)

Provides convenient extension methods for all MVC module controls:

- **Localization**: `LocalizeString()`, `LocalizeSafeJsString()`
- **URL Generation**: `EditUrl()` with various overloads
- **Settings Access**: `GetModuleSetting<T>()` with type conversion
- **State Checking**: `EditMode()`, `IsEditable()`

### 5. Resource Management

#### IResourcable Interface
Modules can implement this interface to automatically manage CSS and JavaScript resources.

#### ModuleResources System
- Automatic resource registration
- Priority-based loading
- File existence validation
- Caching for performance
- Independent of the pipeline

### 6. Utilities

#### MvcModuleControlRenderer<T>
Provides rendering capabilities for Razor-based module controls outside of the normal MVC pipeline.

#### ViewRenderer
A powerful utility class for rendering MVC views to strings outside of the standard MVC request pipeline. This class is essential for the MVC module control system as it enables view rendering in non-controller contexts.

**Core Methods:**
```csharp
// Render full view with layout
string html = ViewRenderer.RenderView("~/Views/MyView.cshtml", model);

// Render partial view without layout
string partial = ViewRenderer.RenderPartialView("~/Views/_MyPartial.cshtml", model);

// Render HtmlHelper delegates
string html = ViewRenderer.RenderHtmlHelperToString(helper => 
    helper.Action("MyAction", "MyController"), model);
```

## Demo Implementation

The Demo folder includes demonstration classes that show practical implementation examples:

### DemoModule.cs
A WebForms-compatible module that bridges to the MVC pipeline, demonstrating how to integrate MVC module controls within the traditional DNN WebForms infrastructure.

**Key Features:**
- **Hybrid Bridge Pattern**: Inherits from `PortalModuleBase` to maintain WebForms compatibility
- **MVC Integration**: Uses `MvcUtils.CreateModuleControl()` to instantiate MVC module controls
- **ViewRenderer Integration**: Demonstrates `ViewRenderer.RenderHtmlHelperToString()` usage
- **Interface Support**: Handles `IActionable` and `IResourcable` interfaces automatically
- **Lifecycle Management**: Proper ASP.NET control lifecycle implementation

**Implementation Pattern:**
```csharp
public class DemoModule : PortalModuleBase, IActionable
{
    protected override void OnInit(EventArgs e)
    {
        // Create MVC module control
        var mc = MvcUtils.CreateModuleControl(this.ModuleConfiguration);
        
        // Render using ViewRenderer
        html = ViewRenderer.RenderHtmlHelperToString(helper => mc.Html(helper));
        
        // Handle optional interfaces
        if (mc is IActionable actionable)
            this.ModuleActions = actionable.ModuleActions;
            
        if (mc is IResourcable resourcable)
            resourcable.RegisterResources(this.Page);
    }
}
```

### DemoModuleControl.cs
A concrete implementation of `RazorModuleControlBase` showing how to create custom MVC module controls with dynamic view selection.

**Key Features:**
- **Dynamic View Routing**: Uses query string parameters to determine which view to render
- **Multiple View Support**: Demonstrates rendering different views based on user input
- **Custom View Paths**: Shows how to specify custom view file locations
- **Model Passing**: Illustrates passing data models to views

**Implementation Example:**
```csharp
public class DemoModuleControl : RazorModuleControlBase
{
    public override IRazorModuleResult Invoke()
    {
        // Dynamic view selection based on query parameters
        switch (Request.QueryString["view"])
        {
            case "Terms":
                return View("~/Views/Default/Terms.cshtml", "Terms content");
            case "Privacy": 
                return View("~/admin/Portal/Views/Privacy.cshtml", "Privacy content");
            default:
                return View("~/admin/Portal/Views/Terms.cshtml", "Default content");
        }
    }
}
```

**Usage Scenarios:**
- **Migration Testing**: Use DemoModule to test MVC controls within WebForms pages
- **Development Reference**: DemoModuleControl shows best practices for Razor module implementation
- **Integration Patterns**: Demonstrates how to handle multiple interfaces (`IActionable`, `IResourcable`)
- **View Management**: Shows flexible view path configuration and model binding

**Bridge Pattern Benefits:**
The DemoModule demonstrates the bridge pattern that allows:
- Gradual migration from WebForms to MVC
- Using MVC controls within existing WebForms infrastructure
- Maintaining compatibility with existing DNN module architecture
- Automatic handling of module actions and resource registration

## Key Benefits

This implementation provides several key advantages:

### 1. Modern Development Experience
- MVC pattern for better separation of concerns
- Dependency injection support
- Testable architecture
- Familiar development patterns for modern .NET developers

### 2. Gradual Migration Path
- Hybrid architecture allows coexistence of WebForms and MVC
- Module-by-module migration strategy
- Backward compatibility maintained

### 3. Pipeline Transparency & Compatibility
- **Unified Interface Implementation**: MVC modules can implement `IActionable` in a unified way
- **No Legacy Modifications**: No modifications required to the existing module pipeline
- **Custom Module Patterns**: Open to custom module control patterns

### 4. Future-Proof Architecture
- **Configuration-Based**: Uses class config in module manifest rather than simple boolean flags
- **.NET Core Preparation**: Architecture aligns with .NET Core patterns where there's no separate module pipeline
- **Extensible Design**: Open to custom module control patterns and future enhancements
