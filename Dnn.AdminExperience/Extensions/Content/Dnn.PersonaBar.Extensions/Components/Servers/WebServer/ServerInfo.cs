using System;
using System.Net;
using System.Security.Principal;
using System.Web;
using DotNetNuke.Common;

namespace Dnn.PersonaBar.Servers.Components.WebServer
{
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
