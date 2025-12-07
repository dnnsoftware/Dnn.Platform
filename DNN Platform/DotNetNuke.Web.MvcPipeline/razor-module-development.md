# Razor Module Development Guide 

## Overview

This guide explains how to create DNN modules using `RazorModuleControlBase`, which provides a modern MVC-based approach to module development using Razor views. This pattern follows the ViewComponent pattern from .NET Core, making it easier to transition to .NET Core in the future.

## Table of Contents

1. [Introduction](#introduction)
2. [Architecture Overview](#architecture-overview)
3. [Creating a Module Control](#creating-a-module-control)
4. [Implementing the Invoke Method](#implementing-the-invoke-method)
5. [Creating Models](#creating-models)
6. [Creating Razor Views](#creating-razor-views)
7. [Module Configuration](#module-configuration)
8. [Available Properties and Methods](#available-properties-and-methods)
9. [Return Types](#return-types)
10. [Best Practices](#best-practices)

## Introduction

`RazorModuleControlBase` is an abstract base class that enables modules to render content using Razor views with MVC 5. It's recommended for migrating WebForms controls and follows a pattern similar to .NET Core ViewComponents.

**Key Benefits:**

- Modern MVC-based rendering
- Separation of concerns (Model-View-Control)
- Easy migration path to .NET Core
- Full access to DNN module context and services
- Dependency injection support

## Architecture Overview

The `RazorModuleControlBase` class hierarchy:

```
DefaultMvcModuleControlBase (abstract)
    └── RazorModuleControlBase (abstract)
        └── YourModuleControl (concrete)
```

**Key Components:**

- **RazorModuleControlBase**: Abstract base class providing Razor view rendering
- **IRazorModuleResult**: Interface for different result types (View, Content, Error)
- **ViewRazorModuleResult**: Renders Razor views with models
- **ContentRazorModuleResult**: Renders plain HTML content
- **ErrorRazorModuleResult**: Renders error messages

## Creating a Module Control

### Step 1: Create the Control Class

Create a class that inherits from `RazorModuleControlBase`:

```csharp
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

namespace YourNamespace.Controls
{
    public class YourModuleControl : RazorModuleControlBase
    {
        // Constructor with dependency injection
        public YourModuleControl()
        {
            // Initialize dependencies
        }

        // Required: Implement the Invoke method
        public override IRazorModuleResult Invoke()
        {
            // Your logic here
            return View();
        }
    }
}
```

### Step 2: Implement the Invoke Method

The `Invoke()` method is where your module's logic resides.
It must return an `IRazorModuleResult`:

```csharp
public override IRazorModuleResult Invoke()
{
    // 1. Get data from services/controllers
    var data = GetYourData();
    
    // 2. Create a model
    var model = new YourModuleModel
    {
        Property = data
    };
    
    // 3. Return a view with the model
    return View(model);
}
```

### Using Default View Name

If you don't specify a view name, the system uses a default path:

```
~/DesktopModules/YourModule/Views/YourModuleControl.cshtml
```

```csharp
public override IRazorModuleResult Invoke()
{
    return View(); // Uses default view path
}
```

### Specifying Custom View Path

```csharp
public override IRazorModuleResult Invoke()
{
    var model = GetModel();
    return View("~/DesktopModules/YourModule/Views/CustomView.cshtml", model);
}
```

### Returning Plain Content

```csharp
public override IRazorModuleResult Invoke()
{
    return Content("<div>Hello World</div>");
}
```

### Error Handling

```csharp
public override IRazorModuleResult Invoke()
{
    try
    {
        var model = GetModel();
        return View(model);
    }
    catch (Exception ex)
    {
        return Error("Error Heading", ex.Message);
    }
}
```

## Creating Models

Create simple POCO classes to pass data to your views:

```csharp
namespace YourNamespace.Models
{
    public class YourModuleModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<Item> Items { get; set; }
    }
}
```

## Creating Razor Views

Create Razor view files (`.cshtml`) in your module's `Views` folder.

### View Location Convention

By default, views are located at:

```
~/DesktopModules/YourModule/Views/YourModuleControl.cshtml
```

### Basic View Example

```razor
@model YourNamespace.Models.YourModuleModel

<div class="your-module">
    <h2>@Model.Title</h2>
    <div>@Html.Raw(Model.Content)</div>
</div>
```

### View with ViewData

The base class automatically provides these ViewData entries:

- `ModuleContext`: The module instance context
- `ModuleId`: The module ID
- `LocalResourceFile`: Path to localization resources

```razor
@model YourNamespace.Models.YourModuleModel
@using DotNetNuke.Entities.Modules

<div>
    <p>Module ID: @ViewData["ModuleId"]</p>
    <p>Content: @Model.Content</p>
</div>
```

## Module Configuration

Configure your module in the `.dnn` manifest file to use the Razor module control.

### Module Control Configuration

In your `dnn_YourModule.dnn` file, add the `mvcControlClass` attribute to your module control:

```xml
<moduleControl>
    <controlKey/>
    <controlSrc>DesktopModules/YourModule/YourModule.ascx</controlSrc>
    <mvcControlClass>YourNamespace.Controls.YourModuleControl, YourAssembly</mvcControlClass>
    <supportsPartialRendering>False</supportsPartialRendering>
    <controlTitle/>
    <controlType>View</controlType>
    <viewOrder>0</viewOrder>
</moduleControl>
```

### Multiple Controls

You can define multiple controls for different actions (View, Edit, Settings):

```xml
<moduleControls>
    <!-- View Control -->
    <moduleControl>
        <controlKey/>
        <controlSrc>DesktopModules/YourModule/YourModule.ascx</controlSrc>
        <mvcControlClass>YourNamespace.Controls.YourModuleControl, YourAssembly</mvcControlClass>
        <controlType>View</controlType>
    </moduleControl>
    
    <!-- Edit Control -->
    <moduleControl>
        <controlKey>Edit</controlKey>
        <controlSrc>DesktopModules/YourModule/Edit.ascx</controlSrc>
        <mvcControlClass>YourNamespace.Controls.EditControl, YourAssembly</mvcControlClass>
        <controlType>Edit</controlType>
    </moduleControl>
</moduleControls>
```

## Available Properties and Methods

### Properties from DefaultMvcModuleControlBase

- `ModuleConfiguration`: Module configuration information
- `TabId`: Current tab/page ID
- `ModuleId`: Current module instance ID
- `TabModuleId`: Tab module ID
- `PortalId`: Portal ID
- `PortalSettings`: Portal settings object
- `UserInfo`: Current user information
- `UserId`: Current user ID
- `Settings`: Module settings hashtable
- `ControlPath`: Path to the control directory
- `ControlName`: Name of the control
- `LocalResourceFile`: Path to localization resource file
- `DependencyProvider`: Service provider for dependency injection

### Properties from RazorModuleControlBase

- `ViewContext`: Razor view context
- `HttpContext`: HTTP context
- `Request`: HTTP request object
- `ViewData`: View data dictionary

### Methods

- `View()`: Returns a view result (multiple overloads)
- `Content(string)`: Returns plain HTML content
- `Error(string, string)`: Returns an error result
- `EditUrl()`: Generate edit URLs (from base class)

## Return Types

### View Result

Renders a Razor view with an optional model:

```csharp
// Default view
return View();

// View with model
return View(model);

// Specific view with model
return View("ViewName", model);
```

### Content Result

Returns plain HTML content (HTML encoded):

```csharp
return Content("<div>Hello</div>");
```

### Error Result

Displays an error message:

```csharp
return Error("Error Heading", "Error message details");
```

## Best Practices

### 1. Dependency Injection

Use constructor injection for dependencies:

```csharp
public class YourModuleControl : RazorModuleControlBase
{
    private readonly IYourService yourService;

    public YourModuleControl(IYourService yourService)
    {
        this.yourService = yourService;
    }
}
```

Or use the `DependencyProvider`:

```csharp
var service = this.DependencyProvider.GetRequiredService<IYourService>();
```

### 2. Error Handling

Always handle errors gracefully:

```csharp
public override IRazorModuleResult Invoke()
{
    try
    {
        var model = GetModel();
        return View(model);
    }
    catch (Exception ex)
    {
        // Log error
        Exceptions.LogException(ex);
        return Error("Error", "An error occurred while loading the module.");
    }
}
```

### 3. Localization

Use the `LocalResourceFile` property for localized strings:

```csharp
var localizedText = Localization.GetString("Key", this.LocalResourceFile);
```

### 4. Module Actions

Implement `IActionable` interface for module actions:

```csharp
public class YourModuleControl : RazorModuleControlBase, IActionable
{
    public ModuleActionCollection ModuleActions
    {
        get
        {
            var actions = new ModuleActionCollection();
            actions.Add(
                this.GetNextActionID(),
                "Edit",
                ModuleActionType.EditContent,
                string.Empty,
                string.Empty,
                this.EditUrl(),
                false,
                SecurityAccessLevel.Edit,
                true,
                false);
            return actions;
        }
    }
}
```

### 5. Resource Management & Page settings

Implement `IPageContributor` interface to register CSS and JavaScript files, request AJAX support, and configure page settings:

```csharp
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;

public class YourModuleControl : RazorModuleControlBase, IPageContributor
{
    public void ConfigurePage(PageConfigurationContext context)
    {
        // Request AJAX support (required for AJAX calls and form submissions)
        context.ServicesFramework.RequestAjaxAntiForgerySupport();
        context.ServicesFramework.RequestAjaxScriptSupport();
        
        // Register CSS stylesheets
        context.ClientResourceController
            .CreateStylesheet("~/DesktopModules/YourModule/styles.css")
            .Register();
        
        // Register JavaScript files
        context.ClientResourceController
            .CreateScript("~/DesktopModules/YourModule/js/edit.js")
            .Register();
        
        // Set page title
        context.PageService.SetTitle("Your Module - Edit");
    }
}
```

**PageConfigurationContext** provides access to:
- **`ClientResourceController`**: Register CSS and JavaScript resources
- **`ServicesFramework`**: Request AJAX support (`RequestAjaxAntiForgerySupport()`, `RequestAjaxScriptSupport()`)
- **`PageService`**: Set page titles and other page-level settings
- **`JavaScriptLibraryHelper`**: Manage JavaScript libraries

### 6. View Organization

- Keep views simple and focused
- Use partial views for reusable components
- Follow the default naming convention when possible
- Place views in the `Views` folder under your module directory

### 7. Model Design

- Keep models simple (POCOs)
- Don't include business logic in models
- Use models to pass data from control to view

## Additional Resources

- [RazorModuleControlBase Source](DNN Platform/DotNetNuke.Web.MvcPipeline/ModuleControl/RazorModuleControlBase.cs)
- [HTML Module Example](DNN Platform/Modules/HTML/Controls/HtmlModuleControl.cs)
- [MVC Module Control README](DNN Platform/DotNetNuke.Web.MvcPipeline/ModuleControl/README.md)

