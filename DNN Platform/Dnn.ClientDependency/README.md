[![Build status](https://ci.appveyor.com/api/projects/status/qx991ywaat8r8a2r?svg=true)](https://ci.appveyor.com/project/Shandem/clientdependency)

![ClientDependency Framework](ClientDependencyLogo.png?raw=true)

## What is *ClientDependency Framework (CDF)* ?

CDF is a framework for managing CSS & JavaScript dependencies and optimizations for your web application. It allows for each individual component in your web application to declare what CSS and JavaScript files they require instead of relying on a single master page to include all dependencies for all modules. It also manages runtime optimizations: minification, compression, caching, and so on....

This hugely simplifies collaborative development since any developer who is working on a component doesn't need to worry about what CSS/JavaScript is being included on the main page. CDF will automagically wire everything up, ensure that your dependencies are ordered correctly, that there are no duplicates and render your CSS and JavaScript html tags properly on to the rendering page.

CDF is flexible. It allows you to make any web component dependent on any CSS or JavaScript file individually. CDF does not require you to pre-define resources at startup (but you certainly can) and doesn't require you to know c# or VB. If you can write markup for Webforms or Razor then you can use CDF.

## Out of the box you get

* MVC support - Any view engine
* Webforms support
* Runtime dependency resolution
* Pre-defined bundling
* Combining, compressing & minifying output
* Support for external/CDN files
* Debug and release mode rendering
* OutputCaching of the combined files
* Persisting the combined composite files for increased performance when applications restart or when the Cache expires
* Versioning the files ... great for ensuring your clients' browser cache is cleared!
* Prioritizing dependencies
* Pre-defined file paths - great for theming!
* Detecting script and styles that are not explicitly registered with CDF and have the output compressed
* Provider Model so you can choose how you would like your JS and CSS files rendered, combined, compressed & minified
* MIME type compression output for things like JSON services, or anything you want
* Control over how composite file URLs are structured if you need a custom format
* Medium trust compatible (Core, MVC and .Less projects)
* Native .Less, TypeScript, SASS & CoffeeScript support

## Nuget

	PM> Install-Package ClientDependency

	PM> Install-Package ClientDependency-Mvc

	PM> Install-Package ClientDependency-Less

	PM> Install-Package ClientDependency-TypeScript

	PM> Install-Package ClientDependency-Sass

	PM> Install-Package ClientDependency-Coffee

## [Documentation](https://github.com/Shandem/ClientDependency/wiki)

Click the link to see the documentation on how to get started and more advanced techniques

## [Releases](https://github.com/Shandem/ClientDependency/releases)

Shows information about all CDF releases

## Copyright & Licence

&copy; 2014 by Shannon Deminick

This is free software and is licensed under the [Microsoft Public License (Ms-PL)](http://opensource.org/licenses/MS-PL)
