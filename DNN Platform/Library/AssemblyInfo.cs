#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotNetNuke.Application;
#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Review the values of the assembly attributes

[assembly: AssemblyTitle("DotNetNuke")]
[assembly: AssemblyDescription("Open Source Web Application Framework")]
[assembly: AssemblyCompany("DotNetNuke Corporation")]
[assembly: AssemblyProduct("http://www.dotnetnuke.com")]
[assembly: AssemblyCopyright("DotNetNuke is copyright 2002-2013 by DotNetNuke Corporation. All Rights Reserved.")]
[assembly: AssemblyTrademark("DotNetNuke")]
[assembly: CLSCompliant(true)]
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("7.2.0.0")]
[assembly: AssemblyFileVersion("7.2.0.0")]
[assembly: AssemblyStatus(ReleaseMode.Stable)]
// Allow internal variables to be visible to testing projects
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Core")]
// This assembly is the default dynamic assembly generated Castle DynamicProxy, 
// used by Moq. Paste in a single line.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("DotNetNuke.Web")]
[assembly: InternalsVisibleTo("DotNetNuke.HttpModules")]
[assembly: InternalsVisibleTo("DotNetNuke.Modules.MemberDirectory")]
[assembly: InternalsVisibleTo("DotNetNuke.Provider.AspNetProvider")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Content")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Web")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Urls")]
