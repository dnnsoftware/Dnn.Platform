# DNN WebForms to MVC Pipeline Skin Transformation Guide

## Table of Contents
1. [Overview](#overview)
2. [File Structure Changes](#file-structure-changes)
3. [Skin File Transformation](#skin-file-transformation)
4. [Container File Transformation](#container-file-transformation)
5. [Partial File Transformation](#partial-file-transformation)
6. [Control/Tag Reference Guide](#controltag-reference-guide)
7. [Complete Examples](#complete-examples)

---

## Overview

This guide provides instructions for transforming DNN WebForms skins (.ascx files) to MVC Pipeline Razor views (.cshtml files). The MVC pipeline provides a modern, cleaner syntax while maintaining all functionality of traditional WebForms skins.

### Key Differences
- **File Extension**: `.ascx` → `.cshtml`
- **Location**: Root folder → `Views/` subfolder
- **Syntax**: WebForms controls → Razor HTML helpers
- **Code Blocks**: `<script runat="server">` → `@{ }` Razor code blocks
- **Includes**: `<!--#include file="..." -->` → `@Html.SkinPartial("...")`

---

## File Structure Changes

### Directory Structure

**WebForms Structure:**
```
SkinName/
├── default.ascx
└── partials/
    ├── _registers.ascx
    ├── _includes.ascx
    └── _header.ascx

ContainerName/
└── title.ascx
```

**MVC Pipeline Structure:**
```
SkinName/
├── default.ascx (keep for backward compatibility)
└── Views/
    ├── default.cshtml
    └── partials/
        ├── _includes.cshtml
        └── _header.cshtml

ContainerName/
├── title.ascx (keep for backward compatibility)
└── Views/
    └── title.cshtml
```

### File Naming Conventions
- **Skins**: Keep the same name (e.g., `default.ascx` → `default.cshtml`)
- **Containers**: Use PascalCase (e.g., `title.ascx` → `title.cshtml`)
- **Partials**: Keep underscore prefix (e.g., `_header.ascx` → `_header.cshtml`)
- **Note**: `_registers.ascx` is NOT needed in MVC pipeline (registrations are not required)

---

## Skin File Transformation

### Step 1: Create the Views Folder
Create a `Views/` folder within your skin directory.

### Step 2: Add Required Namespaces and Model

**WebForms (default.ascx):**
```aspx
<%@ Control Language="C#" AutoEventWireup="true" Explicit="True" Inherits="DotNetNuke.UI.Skins.Skin" %>
<!-- Register directives -->
```

**MVC Pipeline (Views/default.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins
@model PageModel
```

**Key Points:**
- Add `@using` directives at the top
- Declare `@model PageModel` for skins
- Remove all `<%@ Register %>` directives (not needed)

### Step 3: Transform Includes

**WebForms:**
```aspx
<!--#include file="partials/_includes.ascx" -->
<!--#include file="partials/_header.ascx" -->
```

**MVC Pipeline:**
```csharp
@section head {    
    @Html.SkinPartial("partials/_includes")
}

@Html.SkinPartial("partials/_header")
```

**Key Points:**
- Use `@Html.SkinPartial("path/to/partial")` instead of HTML comments
- Remove the file extension and underscore prefix in the path
- CSS/JS includes should go in `@section head { }` block
- Path is relative to the `Views/` folder

### Step 4: Transform Panes

**WebForms:**
```aspx
<div id="BannerPane" runat="server"></div>
<div id="ContentPane" class="aperture-content-pane" runat="server"></div>
```

**MVC Pipeline:**
```csharp
@Html.Pane(id: "BannerPane")
@Html.Pane(id: "ContentPane", cssClass: "aperture-content-pane")
```

**Key Points:**
- Use `@Html.Pane()` helper method
- Use named parameters: `id:` and `cssClass:`
- Remove `runat="server"` attribute

---

## Container File Transformation

### Step 1: Create the Views Folder
Create a `Views/` folder within your skin directory.

### Step 2: Add Required Namespaces and Model

**WebForms (title.ascx):**
```aspx
<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/title.ascx" %>
```

**MVC Pipeline (Viewstitle.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Containers
@model DotNetNuke.Web.MvcPipeline.Models.ContainerModel
```

**Key Points:**
- Use `@model ContainerModel` for containers (not PageModel)
- Add `@using DotNetNuke.Web.MvcPipeline.Containers` namespace

### Step 3: Transform Container Controls

**WebForms:**
```aspx
<div class="aperture-title-wrapper">
    <h5><dnn:TITLE runat="server" id="apertureTitle" /></h5>
    <div id="ContentPane" runat="server"></div>
</div>
```

**MVC Pipeline:**
```csharp
<div class="aperture-title-wrapper">
    <h5>@Html.Title()</h5>
    <div>@Html.Content()</div>
</div>
```

**Key Points:**
- `<dnn:TITLE>` → `@Html.Title()`
- Container's `ContentPane` → `@Html.Content()`
- Remove `runat="server"` and `id` attributes

---

## Partial File Transformation

### Registers Partial (_registers.ascx)
**Action**: DELETE this file - not needed in MVC pipeline. Register directives are not required in Razor views.

### Includes Partial Transformation

**WebForms (partials/_includes.ascx):**
```aspx
<dnn:META ID="mobileScale" runat="server" Name="viewport" Content="width=device-width, initial-scale=1.0" />

<dnn:DnnCssInclude runat="server" FilePath="css/skin.min.css" Priority="110" PathNameAlias="SkinPath" />

<dnn:DnnJsInclude runat="server" FilePath="js/skin.min.js" ForceProvider="DnnFormBottomProvider" Priority="110" PathNameAlias="SkinPath" />

<script runat="server">
    protected void Page_Init()
    {
        var fonts = new string[]
        {
            "fonts/Ubuntu-Bold",
            "fonts/Ubuntu-Regular"
        };

        var types = new Dictionary<string, string>();
        types.Add("woff2", "font/woff2");

        var defaultPage = (CDefault)this.Page;

        foreach (var type in types)
        {
            foreach (var font in fonts)
            {
                var fontLink = new HtmlLink();
                fontLink.Attributes.Add("rel", "preload");
                fontLink.Attributes.Add("as", "font");
                fontLink.Href = this.SkinPath + font + "." + type.Key;
                fontLink.Attributes.Add("type", type.Value);
                fontLink.Attributes.Add("crossorigin", "anonymous");

                defaultPage.Header.Controls.Add(fontLink);
            }
        }
    }
</script>
```

**MVC Pipeline (Views/partials/_includes.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins

@model PageModel

@{
    @Html.Meta(name: "viewport", content: "width=device-width, initial-scale=1.0")
    
    @* Include skin CSS and JS *@
    @Html.DnnCssInclude(filePath: "css/skin.min.css", pathNameAlias: "SkinPath", priority: 110)
    @Html.DnnJsInclude(filePath: "js/skin.min.js", pathNameAlias: "SkinPath", priority: 110, defer: true)
    
    @* Preload fonts *@
    var fonts = new string[]
    {
        "fonts/Ubuntu-Bold",
        "fonts/Ubuntu-Regular"
    };

    var types = new Dictionary<string, string>
    {
        { "woff2", "font/woff2" },
        { "woff", "font/woff" }
    };

    foreach (var type in types)
    {
        foreach (var font in fonts)
        {
            <link rel="preload" 
                  as="font"
                  href="@(Model.Skin.SkinPath + font + "." + type.Key)"
                  type="@type.Value"
                  crossorigin="anonymous" />
        }
    }
}
```

**Key Points:**
- `<script runat="server">` → `@{ }` Razor code block
- Remove server-side page manipulation (no need for `defaultPage.Header.Controls.Add`)
- Output HTML directly using `<link>` tags within Razor code block
- Use `Model.Skin.SkinPath` instead of `this.SkinPath`
- Use collection initializer syntax for dictionaries: `{ { "key", "value" } }`
- Razor comments: `@* comment *@` instead of `<!-- comment -->`

### Header Partial Transformation

**WebForms (partials/_header.ascx):**
```aspx
<header class="aperture-header">
  <div class="eyebrow-bar">
    <div class="aperture-container">
      <dnn:Login runat="server" id="dnnLogin" />
      <dnn:User runat="server" id="dnnUser" />
    </div>
  </div>
  <div class="logo-menu-bar">
    <div class="aperture-container">
      <dnn:LOGO id="dnnLOGO" runat="server" InjectSvg="true" />
      <dnn:MENU id="menu_desktop" CssClass="aperture-d-none aperture-d-md-block" MenuStyle="menus/desktop" runat="server" NodeSelector="*,0,2"></dnn:MENU>
      <dnn:MENU id="menu_mobile" CssClass="aperture-d-flex aperture-d-md-none" MenuStyle="menus/mobile" runat="server" NodeSelector="*,0,2"></dnn:MENU>
    </div>
  </div>
</header>
```

**MVC Pipeline (Views/partials/_header.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins
@using DotNetNuke.Web.NewDDRMenu

@model PageModel

<header class="aperture-header">
  <div class="eyebrow-bar">
    <div class="aperture-container">
            @Html.Login()
            @Html.User()
        </div>
    </div>
    <div class="logo-menu-bar">
        <div class="aperture-container">
            @Html.Logo(injectSvg: true)
            @Html.DDRMenu(clientID: "menu_desktop", 
                         cssClass: "aperture-d-none aperture-d-md-block", 
                         menuStyle: "menus/desktop",
                         nodeSelector: "*,0,2")
            @Html.DDRMenu(clientID: "menu_mobile", 
                         cssClass: "aperture-d-flex aperture-d-md-none", 
                         menuStyle: "menus/mobile",
                         nodeSelector: "*,0,2")
						 
        </div>
    </div>
</header>
```

**Key Points:**
- Add `@using DotNetNuke.Web.NewDDRMenu` for DDRMenu helpers
- `<dnn:MENU>` → `@Html.DDRMenu()` with named parameters
- Use `clientID:` parameter instead of `id` attribute
- Convert attributes to camelCase named parameters

---

## Control/Tag Reference Guide

This comprehensive reference shows how to transform each DNN WebForms control to its MVC Pipeline equivalent.

### Core Skin Objects

| WebForms | MVC Pipeline | Notes |
|----------|-------------|-------|
| `<dnn:BREADCRUMB runat="server" />` | `@Html.Breadcrumb()` | Displays breadcrumb navigation |
| `<dnn:CONTROLPANEL runat="server" />` | `@Html.ControlPanel()` | Displays the control panel |
| `<dnn:COPYRIGHT runat="server" />` | `@Html.Copyright()` | Displays copyright notice |
| `<dnn:CURRENTDATE runat="server" />` | `@Html.CurrentDate()` | Displays current date |
| `<dnn:DOTNETNUKE runat="server" />` | `@Html.DotNetNuke()` | Displays DNN logo/link |
| `<dnn:HOSTNAME runat="server" />` | `@Html.HostName()` | Displays host name |
| `<dnn:LANGUAGE runat="server" />` | `@Html.Language()` | Language selector |
| `<dnn:LINKS runat="server" />` | `@Html.Links()` | Related links |
| `<dnn:LOGIN runat="server" />` | `@Html.Login()` | Login/logout link |
| `<dnn:LOGO runat="server" />` | `@Html.Logo()` | Site logo |
| `<dnn:PRIVACY runat="server" />` | `@Html.Privacy()` | Privacy link |
| `<dnn:SEARCH runat="server" />` | `@Html.Search()` | Search box |
| `<dnn:TAGS runat="server" />` | `@Html.Tags()` | Content tags |
| `<dnn:TERMS runat="server" />` | `@Html.Terms()` | Terms of use link |
| `<dnn:TEXT runat="server" />` | `@Html.Text()` | Localized text |
| `<dnn:TOAST runat="server" />` | `@Html.Toast()` | Toast notifications |
| `<dnn:USER runat="server" />` | `@Html.User()` | User display name |
| `<dnn:USERANDLOGIN runat="server" />` | `@Html.UserAndLogin()` | Combined user/login control |

### Advanced Controls

| WebForms | MVC Pipeline | Notes |
|----------|-------------|-------|
| `<dnn:LOGO InjectSvg="true" runat="server" />` | `@Html.Logo(injectSvg: true)` | Attributes become parameters |
| `<dnn:MENU MenuStyle="MyMenu" runat="server" />` | `@Html.DDRMenu(menuStyle: "MyMenu")` | DDRMenu implementation |
| `<dnn:META Name="viewport" Content="..." runat="server" />` | `@Html.Meta(name: "viewport", content: "...")` | Meta tags |

### Resource Management

| WebForms | MVC Pipeline | Notes |
|----------|-------------|-------|
| `<dnn:DnnCssInclude FilePath="css/style.css" PathNameAlias="SkinPath" Priority="110" runat="server" />` | `@Html.DnnCssInclude(filePath: "css/style.css", pathNameAlias: "SkinPath", priority: 110)` | CSS includes |
| `<dnn:DnnJsInclude FilePath="js/script.js" PathNameAlias="SkinPath" Priority="110" ForceProvider="DnnFormBottomProvider" runat="server" />` | `@Html.DnnJsInclude(filePath: "js/script.js", pathNameAlias: "SkinPath", priority: 110, defer: true)` | JS includes |
| `<dnn:STYLES runat="server" />` | `@Html.Styles()` | Outputs registered stylesheets |
| `<dnn:JavaScriptLibraryInclude runat="server" />` | `@Html.JavaScriptLibraryInclude()` | JS library includes |
| `<dnn:JQUERY runat="server" />` | `@Html.jQuery()` | jQuery include |

### Container Controls

| WebForms | MVC Pipeline | Notes |
|----------|-------------|-------|
| `<dnn:TITLE runat="server" />` | `@Html.Title()` | Module title |
| `<div id="ContentPane" runat="server"></div>` (in container) | `@Html.Content()` | Module content |

### Panes (in Skins)

| WebForms | MVC Pipeline | Notes |
|----------|-------------|-------|
| `<div id="ContentPane" runat="server"></div>` | `@Html.Pane(id: "ContentPane")` | Basic pane |
| `<div id="ContentPane" class="my-class" runat="server"></div>` | `@Html.Pane(id: "ContentPane", cssClass: "my-class")` | Pane with CSS class |

### Special Attributes Mapping

When transforming controls with attributes, convert them to named parameters in camelCase:

**WebForms:**
```aspx
<dnn:LOGO 
    id="dnnLogo" 
    runat="server" 
    InjectSvg="true" 
    CssClass="my-logo"
    BorderWidth="0" />
```

**MVC Pipeline:**
```csharp
@Html.Logo(
    injectSvg: true,
    cssClass: "my-logo",
    borderWidth: 0)
```

**Key Points:**
- Remove `id` and `runat="server"` attributes
- Convert attribute names to camelCase
- Use named parameters with colon (`:`)
- Convert string numbers to integers where appropriate

---

## Complete Examples

### Example 1: Simple Skin Transformation

**WebForms (simple.ascx):**
```aspx
<%@ Control Language="C#" AutoEventWireup="true" Explicit="True" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LOGO" Src="~/Admin/Skins/Logo.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SEARCH" Src="~/Admin/Skins/Search.ascx" %>
<%@ Register TagPrefix="dnn" TagName="USER" Src="~/Admin/Skins/User.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LOGIN" Src="~/Admin/Skins/Login.ascx" %>
<%@ Register TagPrefix="dnn" TagName="MENU" src="~/DesktopModules/DDRMenu/Menu.ascx" %>

<div class="simple-skin">
    <header>
        <dnn:LOGO runat="server" />
        <dnn:SEARCH runat="server" />
        <dnn:USER runat="server" />
        <dnn:LOGIN runat="server" />
    </header>
    
    <nav>
        <dnn:MENU MenuStyle="Bootstrap" runat="server" />
    </nav>
    
    <main>
        <div id="ContentPane" runat="server"></div>
    </main>
    
    <footer>
        <dnn:COPYRIGHT runat="server" />
    </footer>
</div>
```

**MVC Pipeline (Views/simple.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins
@using DotNetNuke.Web.NewDDRMenu
@model PageModel

<div class="simple-skin">
    <header>
        @Html.Logo()
        @Html.Search()
        @Html.User()
        @Html.Login()
    </header>
    
    <nav>
        @Html.DDRMenu(menuStyle: "Bootstrap")
    </nav>
    
    <main>
        @Html.Pane(id: "ContentPane")
    </main>
    
    <footer>
        @Html.Copyright()
    </footer>
</div>
```

### Example 2: Skin with Multiple Panes and Partials

**WebForms (advanced.ascx):**
```aspx
<!--#include file="partials/_registers.ascx" -->
<!--#include file="partials/_includes.ascx" -->

<div class="advanced-skin">
    <!--#include file="partials/_header.ascx" -->
    
    <div class="banner-area">
        <div id="BannerPane" runat="server"></div>
    </div>
    
    <div class="content-area">
        <aside class="sidebar">
            <div id="LeftPane" runat="server"></div>
        </aside>
        
        <main class="main-content">
            <div id="ContentPane" class="primary-content" runat="server"></div>
        </main>
        
        <aside class="secondary-sidebar">
            <div id="RightPane" runat="server"></div>
        </aside>
    </div>
    
    <div class="footer-widgets">
        <div id="FooterPane" class="footer-columns" runat="server"></div>
    </div>
    
    <!--#include file="partials/_footer.ascx" -->
</div>
```

**MVC Pipeline (Views/advanced.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins
@model PageModel

@section head {
    @Html.SkinPartial("partials/_includes")
}

<div class="advanced-skin">
    @Html.SkinPartial("partials/_header")
    
    <div class="banner-area">
        @Html.Pane(id: "BannerPane")
    </div>
    
    <div class="content-area">
        <aside class="sidebar">
            @Html.Pane(id: "LeftPane")
        </aside>
        
        <main class="main-content">
            @Html.Pane(id: "ContentPane", cssClass: "primary-content")
        </main>
        
        <aside class="secondary-sidebar">
            @Html.Pane(id: "RightPane")
        </aside>
    </div>
    
    <div class="footer-widgets">
        @Html.Pane(id: "FooterPane", cssClass: "footer-columns")
    </div>
    
    @Html.SkinPartial("partials/_footer")
</div>
```

### Example 3: Complex Partial with Code

**WebForms (partials/_navigation.ascx):**
```aspx
<dnn:DnnCssInclude runat="server" FilePath="css/navigation.css" PathNameAlias="SkinPath" />

<script runat="server">
    protected string GetMenuClass()
    {
        return PortalSettings.ActiveTab.Level == 0 ? "top-level-menu" : "sub-menu";
    }
</script>

<nav class="<%= GetMenuClass() %>">
    <dnn:MENU id="mainNav" 
              MenuStyle="menus/main" 
              NodeSelector="*,0,3"
              CssClass="main-navigation"
              runat="server" />
</nav>
```

**MVC Pipeline (Views/partials/_navigation.cshtml):**
```csharp
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins
@using DotNetNuke.Web.NewDDRMenu

@model PageModel

@{
    @Html.DnnCssInclude(filePath: "css/navigation.css", pathNameAlias: "SkinPath")
    
    var menuClass = Model.ActiveTab.Level == 0 ? "top-level-menu" : "sub-menu";
}

<nav class="@menuClass">
    @Html.DDRMenu(clientID: "mainNav",
                 menuStyle: "menus/main",
                 nodeSelector: "*,0,3",
                 cssClass: "main-navigation")
</nav>
```

**Key Points:**
- Functions defined in `<script runat="server">` become variables in `@{ }` blocks
- Use `Model.ActiveTab` instead of `PortalSettings.ActiveTab`
- Inline code `<%= %>` becomes Razor `@variable`

---

## Transformation Checklist

Use this checklist when transforming a WebForms skin:

### Pre-Transformation
- [ ] Review the WebForms skin structure
- [ ] Identify all partials and includes
- [ ] List all DNN controls used
- [ ] Note any custom server-side code

### Skin Files
- [ ] Create `Views/` folder in skin directory
- [ ] Add `@using` directives for required namespaces
- [ ] Add `@model PageModel` declaration
- [ ] Transform all `<!--#include-->` to `@Html.SkinPartial()`
- [ ] Move CSS/JS includes to `@section head { }` block
- [ ] Transform all panes from `<div runat="server">` to `@Html.Pane()`
- [ ] Transform all DNN controls to HTML helpers
- [ ] Test the skin

### Container Files
- [ ] Create `containers/Views/` folder
- [ ] Add `@using` directives including `DotNetNuke.Web.MvcPipeline.Containers`
- [ ] Add `@model ContainerModel` declaration
- [ ] Transform `<dnn:TITLE>` to `@Html.Title()`
- [ ] Transform container's `ContentPane` to `@Html.Content()`
- [ ] Transform all other DNN controls to HTML helpers
- [ ] Test the container

### Partial Files
- [ ] Create `Views/partials/` folder
- [ ] Transform each partial (except `_registers.ascx`)
- [ ] DELETE `_registers.ascx` (not needed)
- [ ] Add `@model PageModel` to each partial
- [ ] Transform `<script runat="server">` blocks to `@{ }` blocks
- [ ] Replace `this.SkinPath` with `Model.Skin.SkinPath`
- [ ] Replace `PortalSettings.*` with `Model.*`
- [ ] Test all partials

### Code Transformation
- [ ] Convert all attribute names to camelCase parameters
- [ ] Remove `id` and `runat="server"` attributes
- [ ] Convert boolean strings ("true"/"false") to boolean values (true/false)
- [ ] Convert inline code `<%= %>` to Razor `@`
- [ ] Convert code blocks `<script runat="server">` to `@{ }`
- [ ] Update property access to use Model

### Final Steps
- [ ] Keep original `.ascx` files for backward compatibility
- [ ] Test all pages with new skin
- [ ] Test all modules with new containers
- [ ] Verify edit mode functionality
- [ ] Check responsive behavior
- [ ] Validate HTML output
- [ ] Create a web.config file in each Views/ folder

---

## Common Pitfalls and Solutions

### Pitfall 1: Forgetting @model Declaration
**Problem:** Missing `@model` declaration causes compilation errors.

**Solution:** Always add appropriate model:
- Skins: `@model PageModel`
- Containers: `@model DotNetNuke.Web.MvcPipeline.Models.ContainerModel`

### Pitfall 2: Incorrect Partial Paths
**Problem:** `@Html.SkinPartial("partials/_header.cshtml")` doesn't work.

**Solution:** Don't include file extension: `@Html.SkinPartial("partials/_header")`

### Pitfall 3: Using `this` in Razor Code
**Problem:** `this.SkinPath` causes errors.

**Solution:** Use `Model.Skin.SkinPath` instead.

### Pitfall 4: Container ContentPane Confusion
**Problem:** Using `@Html.Pane(id: "ContentPane")` in containers.

**Solution:** Use `@Html.Content()` for container content area.

### Pitfall 5: Keeping Register Directives
**Problem:** Keeping `<%@ Register %>` directives in `.cshtml` files.

**Solution:** Remove all register directives; they're not needed in Razor.

### Pitfall 6: Not Using Named Parameters
**Problem:** `@Html.Logo(true)` causes confusion about parameter meaning.

**Solution:** Use named parameters: `@Html.Logo(injectSvg: true)`

### Pitfall 7: Forgetting @section head for Includes
**Problem:** CSS/JS includes in body affect page load order.

**Solution:** Wrap includes in `@section head { }` block.

---

## Additional Resources

### Model Properties Reference

**PageModel Properties:**
- `Model.ActiveTab` - Current page/tab information
- `Model.Skin.SkinPath` - Path to current skin folder
- `Model.PortalSettings` - Portal settings
- `Model.User` - Current user information

**ContainerModel Properties:**
- `Model.ModuleInfo` - Current module information
- `Model.Container` - Container information

### Common HTML Helper Methods

**Skin Helpers:**
- `@Html.SkinPartial(path)` - Include partial view
- `@Html.Pane(id, cssClass)` - Render pane
- `@Html.Meta(name, content)` - Add meta tag
- `@Html.DnnCssInclude(filePath, pathNameAlias, priority)` - Include CSS
- `@Html.DnnJsInclude(filePath, pathNameAlias, priority, defer)` - Include JS

**Container Helpers:**
- `@Html.Title()` - Module title
- `@Html.Content()` - Module content

---

## Version History

- **v1.0** - Initial guide based on Aperture skin transformation
- Created: January 2026

---

## Support and Feedback

This guide is based on the transformation of the DNN Aperture skin from WebForms to MVC Pipeline. For questions or improvements, please refer to the DNN Platform documentation or community forums.

---

**End of Guide**
