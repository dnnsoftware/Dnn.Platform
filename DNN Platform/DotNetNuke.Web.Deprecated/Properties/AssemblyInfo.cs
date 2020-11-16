// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.UI;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Review the values of the assembly attributes
[assembly: AssemblyTitle("DotNetNuke.Web.Deprecated")]
[assembly: AssemblyDescription("Open Source Web Application Framework")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a5e0864a-df43-4b6a-8e5f-3ba6b368d876")]

[assembly: InternalsVisibleTo("DotNetNuke.Tests.Content")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Messaging")]
[assembly: InternalsVisibleTo("DotNetNuke.Tests.Web")]
[assembly: WebResource("DotNetNuke.Web.UI.WebControls.Resources.TermsSelector.js", "application/x-javascript")]
[assembly: WebResource("DotNetNuke.Web.UI.WebControls.Resources.TermsSelector.css", "text/css")]
