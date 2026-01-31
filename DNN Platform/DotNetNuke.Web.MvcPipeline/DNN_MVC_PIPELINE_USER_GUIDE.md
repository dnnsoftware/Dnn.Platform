# DNN MVC Pipeline - User Guide

<img width="2752" height="1536" alt="image" src="https://github.com/user-attachments/assets/6672f0f6-fe7c-4d06-8c2e-dfba5d26c05d" />


## Table of Contents

1. [Introduction](#introduction)
   - [What is the DNN MVC Pipeline?](#what-is-the-dnn-mvc-pipeline)
   - [Benefits](#benefits)
   - [When to Use Which Pipeline](#when-to-use-which-pipeline)
2. [Part 1: Adding MVC Support to Existing WebForms Skins](#part-1-adding-mvc-support-to-existing-webforms-skins)
   - [Quick Start Checklist](#quick-start-checklist)
   - [File Structure Changes](#file-structure-changes)
   
3. [Part 2: Adding MVC Support to Existing WebForms Modules](#part-2-adding-mvc-support-to-existing-webforms-modules)
   
---

## Introduction

### What is the DNN MVC Pipeline?

The DNN MVC Pipeline is a modern rendering system that enables DNN Platform to use ASP.NET MVC architecture alongside the traditional WebForms pipeline. It provides a hybrid architecture that allows gradual migration from WebForms to MVC while maintaining full backward compatibility.

**Key Features:**
- Modern MVC-based rendering with Razor views
- Separation of concerns (Model-View-Controller pattern)
- Clean, maintainable code structure
- Dependency injection support
- Easy migration path from WebForms to .NET Core
- Full compatibility with existing DNN infrastructure

### Benefits

**For Developers:**
- **Modern Development Experience**: Use familiar MVC patterns and Razor syntax
- **Testability**: MVC architecture is easier to unit test
- **Dependency Injection**: Built-in support for modern DI patterns
- **Future-Proof**: Follows patterns compatible with .NET Core migration

**For Integrators:**
- **Gradual Migration**: Migrate skins and modules incrementally
- **No Breaking Changes**: Existing WebForms code continues to work
- **Flexibility**: Choose the best approach for each component

**Pipeline Components:**

1. **Skin**   
   - **WebForms Skins**: Uses `.ascx` files
   - **MVC Skins**: Uses `.cshtml` Razor view files in `Views/` folders and HTML helpers instead of server controls

2. **Module Controls**
   - **WebForms Module Controls**: Traditional `.ascx` user controls
   - **MVC pipeline Module Controls**: Classes implementing `IMvcModuleControl`
   - **Hybrid Module controls**: Support both pipelines simultaneously


### How activate the MVC Pipeline ?
- On site settings you can activate it globaly for a portal
- On page settings you can overide the site settings 

[Video showcase the general usage](https://www.youtube.com/watch?v=L7ZHTP8e_7w)

---

## Part 1: Adding MVC Support to Existing WebForms Skins

This section guides you through transforming existing DNN WebForms skins (`.ascx` files) to support the MVC Pipeline using Razor views (`.cshtml` files). Your existing WebForms skins will continue to work while the new MVC views enable the modern pipeline. WebForms Skin files is actually required for each skin.

### Quick Start Checklist

- [ ] Create `Views/` folder in your skin directory
- [ ] Create `Views/` folder in your container directory
- [ ] Add `web.config` files to Views folders
- [ ] Transform skin files: `.ascx` → `Views/{name}.cshtml`
- [ ] Transform container files: `.ascx` → `Views/{name}.cshtml`
- [ ] Transform includes to razar partials 
- [ ] Add required `@using` directives and `@model` declarations
- [ ] Convert DNN controls to HTML helpers
- [ ] Test skin in MVC pipeline
- [ ] Keep original `.ascx` files for backward compatibility

### File Structure Changes

#### Directory Structure

**Before (WebForms Only):**
```
MySkin/
├── default.ascx
└── partials/
    ├── _registers.ascx
    ├── _includes.ascx
    └── _header.ascx

MyContainer/
└── title.ascx
```

**After (Hybrid WebForms + MVC):**
```
MySkin/
├── default.ascx                    (keep for WebForms pipeline)
└── Views/                          (NEW - for MVC pipeline)
    ├── web.config                  (NEW - required)
    ├── default.cshtml              (NEW - MVC Razor view)
    └── partials/
        ├── _includes.cshtml        (NEW)
        └── _header.cshtml          (NEW)
        (no _registers.cshtml - not needed!)

MyContainer/
├── title.ascx                      (keep for WebForms pipeline)
└── Views/                          (NEW - for MVC pipeline)
    ├── web.config                  (NEW - required)
    └── Title.cshtml                (NEW - PascalCase)
```

#### File Naming Conventions

- **Skins**: Keep the same name (e.g., `default.ascx` → `default.cshtml`)
- **Containers**: Keep the same name (e.g., `title.ascx` → `title.cshtml`)
- **Partials**: Keep underscore prefix (e.g., `_header.ascx` → `_header.cshtml`)

> **Note**: For complete details and additional examples, see [SKIN_WEBFORMS_TO_MVC_GUIDE.md](https://github.com/dnnsoftware/Dnn.Platform/blob/feature/mvc-pipeline-old/DNN%20Platform/Skins/Aperture/SKIN_WEBFORMS_TO_MVC_GUIDE.md)

---

## Part 2: Adding MVC Support to Existing WebForms Modules

This section guides you through adding MVC Pipeline support to existing WebForms modules. 

### Module Controls
The migration needs to be done for each ModuleControl of your module.
In DNN we have 3 kinds of Module controls : View, Edit and Settings.

The View control is the only one that is rendered on a page where other modules are also be rendered.
So it's also the only one that needs to support the 2 pipelines.

The Edit Controls are always rendered on a page without other modules, so it is enoug to support only one pipeline. So you can leave them in WebForms or add MVC support. You can also convert this controls gradually.

The settings control is actually not supported in the MVC pipeline. So you have to leave them in webforms or convert them to a Edit Control.

### Module patern
Three module paterns are supported in the MVC pipeline : the existing SPA and MVC module paterns and a new Razor+ patern.
The SPA patern can be used without changes and the MVC patern with small changes. The MVC patern is not really future proof because it is based on Child Action Controllers (in the mvc pipeline) that dousn't exist in .net Core.

The recommended patern is Razor+ with use the same structure then a .net Core Viewcomponent.
The Razor+ use a Razor file for server side rendering and typically interaction are done in javascript with webapi or alternatively with AJAX and classic MVC Controllers.

### Migration paths
To make it work you need to define in module manifest : the Webforms Control and a new MVC Control class name.

There are 2 options :
1. Use a WebForms Control and a MVC Control with 2 UI implementations.
2. Create one Razor+ Module control and use the generic WebForms Wrapper Module Control to render the Razor+ in Webforms.

#### What is the WrapperModule?

`WrapperModule` is a bridge component that allows MVC module controls to run within the traditional WebForms pipeline. 

#### When to Use WrapperModule

✅ **Use the WrapperModule when:**
- You want to migrate a module to MVC
- You need to support both WebForms and MVC pipelines
- Your module doesn't require mvc form submissions
- You use webapi or Ajax for form submissions
- You want to maintain a one code base

❌ **Don't use WrapperModule if:**
- Your module requires `<form>` tags in MVC (not allowed because WebForms pages already have one)
- You need postback functionality
- you don't use a javascript frmaworks with webapi's for form submissions


> **Note**: For complete technical details, see [razor-module-development.md](https://github.com/dnnsoftware/Dnn.Platform/blob/feature/mvc-pipeline-old/DNN%20Platform/DotNetNuke.Web.MvcPipeline/razor-module-development.md)

---

## Additional Resources

### Documentation

- **Skin Transformation Guide:** [SKIN_WEBFORMS_TO_MVC_GUIDE.md](https://github.com/dnnsoftware/Dnn.Platform/blob/feature/mvc-pipeline-old/DNN%20Platform/Skins/Aperture/SKIN_WEBFORMS_TO_MVC_GUIDE.md)
- **Razor Module Development:** [razor-module-development.md](https://github.com/dnnsoftware/Dnn.Platform/blob/feature/mvc-pipeline-old/DNN%20Platform/DotNetNuke.Web.MvcPipeline/razor-module-development.md)
- **Module Control Architecture:** [ModuleControl/README.md](https://github.com/dnnsoftware/Dnn.Platform/blob/feature/mvc-pipeline-old/DNN%20Platform/DotNetNuke.Web.MvcPipeline/ModuleControl/README.md)

### Example Implementations

- **HTML Module:** [DNN Platform/Modules/HTML/](https://github.com/dnnsoftware/Dnn.Platform/tree/feature/mvc-pipeline-old/DNN%20Platform/Modules/HTML)
- **MVC, SPA and Razor+ Samples Modules:** [DNN Platform/Modules/Samples/](https://github.com/dnnsoftware/Dnn.Platform/tree/feature/mvc-pipeline-old/DNN%20Platform/Modules/Samples)
