// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotNetNuke.Application;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Review the values of the assembly attributes
[assembly: AssemblyTitle("DotNetNuke")]
[assembly: AssemblyDescription("Open Source Web Application Framework")]
[assembly: CLSCompliant(true)]

[assembly: AssemblyStatus(ReleaseMode.Alpha)]

// Allow internal variables to be visible to testing projects
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Core")]

// This assembly is the default dynamic assembly generated Castle DynamicProxy,
// used by Moq. Paste in a single line.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("DotNetNuke.Web")]
[assembly: InternalsVisibleTo("DotNetNuke.Web.Mvc")]
[assembly: InternalsVisibleTo("DotNetNuke.Web.Razor")]
[assembly: InternalsVisibleTo("DotNetNuke.HttpModules")]
[assembly: InternalsVisibleTo("DotNetNuke.Modules.MemberDirectory")]
[assembly: InternalsVisibleTo("DotNetNuke.Provider.AspNetProvider")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Content")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Web")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Web.Mvc")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Urls")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Professional")]
[assembly: InternalsVisibleTo("DotNetNuke.SiteExportImport")]
[assembly: InternalsVisibleTo("DotNetNuke.Web.DDRMenu")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("Dnn.PersonaBar.Extensions")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("DotNetNuke.Modules.Html")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("DotNetNuke.Website.Deprecated")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("Dnn.PersonaBar.UI")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("Dnn.PersonaBar.Library")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("DotNetNuke.Modules.Groups")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("DotNetNuke.Modules.Journal")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("DotNetNuke.Modules.RazorHost")] // Once Globals is refactored to Dependency Injection we should be able to remove this
[assembly: InternalsVisibleTo("DotNetNuke.Website")] // Once Globals is refactored to Dependency Injection we should be able to remove this
