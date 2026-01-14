# DNN Sample Modules

In this directory you'll find several sample modules. The purpose of these modules is to:

1. Showcase how you can build modules for DNN
2. Validate any changes we make to the framework and the impact it would have on module developers

These modules are **NOT MEANT TO BE USED IN PRODUCTION**. There will be no "upgrade path" from one version to the next!

## Components

### Dnn.ContactList.Api

This is a library project that takes care of persisting data to SQL database. It is used in the other ContactList projects.
Note that when you build these projects in release mode a zip file is created for each project that includes this dll.

### Dnn.ContactList.Mvc

This is a DNN MVC module. The main templating launguage is Razor (cshtml).

### Dnn.ContactList.Spa

This is a DNN SPA module. The main templating language is DNN's token replace (plain text into HTML). This project shows
the many ways in which you can use this to create a module.

### Dnn.ContactList.SpaReact

This is a DNN SPA module with a React front-end. This module shows less features of SPA module development than the Spa module above.
Instead it shows how you could use this module pattern to jump to React as quickly as possible.

## Building

These components are built to installable zip files under /Artifacts/SampleModules in release mode.
In debug mode they build to either the /Website folder or the folder you've set up using DNN_Platform.local.build.

Example of DNN_Platform.local.build:
``` xml
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <WebsitePath Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">D:\path\to\my\dnn\www</WebsitePath>
    <CopySampleProjects>True</CopySampleProjects>
  </PropertyGroup>
</Project>
```

Example of settings.local.json:
``` json
{
  ...
  "CopySampleProjects": true,
  ...
}
```

The ```CopySampleProjects``` key will determine if these projects will be built to the destination or ignored.
If you set both to true the sample projects will be included to the build to your dev folder under /Install/module.
