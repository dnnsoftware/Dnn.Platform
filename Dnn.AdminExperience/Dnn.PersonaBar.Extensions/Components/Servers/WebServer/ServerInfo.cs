// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.WebServer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Security.Principal;
    using System.Web;

    using DotNetNuke.Common;

    public class ServerInfo
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string Framework => Environment.Version.ToString();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string NetFrameworkVersion => Globals.FormattedNetFrameworkVersion;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string HostName => Dns.GetHostName();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string Identity => WindowsIdentity.GetCurrent().Name;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string IISVersion => HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"];

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string OSVersion => Environment.OSVersion.ToString();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string PhysicalPath => Globals.ApplicationMapPath;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string Url => Globals.GetDomainName(HttpContext.Current.Request);

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string RelativePath => string.IsNullOrEmpty(Globals.ApplicationPath) ? "/" : Globals.ApplicationPath;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ServerTime => DateTime.Now.ToString();
    }
}
