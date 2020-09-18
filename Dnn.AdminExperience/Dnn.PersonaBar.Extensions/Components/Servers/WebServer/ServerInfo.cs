// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.WebServer
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.Web;

    using DotNetNuke.Common;

    public class ServerInfo
    {
        public string Framework => Environment.Version.ToString();

        public string HostName => Dns.GetHostName();

        public string Identity => WindowsIdentity.GetCurrent().Name;

        public string IISVersion => HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"];

        public string OSVersion => Environment.OSVersion.ToString();

        public string PhysicalPath => Globals.ApplicationMapPath;

        public string Url => Globals.GetDomainName(HttpContext.Current.Request);

        public string RelativePath => string.IsNullOrEmpty(Globals.ApplicationPath) ? "/" : Globals.ApplicationPath;

        public string ServerTime => DateTime.Now.ToString();
    }
}
