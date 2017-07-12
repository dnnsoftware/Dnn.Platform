using DotNetNuke.Entities.Host;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class HostModel
    {

        // DNN Platform for example
        public string Product;
        public string Version;
        public bool UpgradeAvailable;
        // .NET Framework: 4.6 for example
        public string Framework;
        // Could be IPv6
        public string IpAddress;
        // ReflectionPermission, WebPermission, AspNetHostingPermission, etc.
        public string Permissions;
        // prompt.com
        public string Site;
        public string Title;
        public string Url;
        public string Email;
        public string Theme;
        public string Container;
        public string EditTheme;
        public string EditContainer;

        public int PortalCount;
        public static HostModel Current()
        {
            var vsn = Utilities.GetDnnVersion().ToString();
            var dnnApp = DotNetNuke.Application.DotNetNukeContext.Current.Application;
            var cbc = DotNetNuke.Web.Components.Controllers.ControlBarController.Instance;
            var req = HttpContext.Current.Request;
            var upgradeIndicator = cbc.GetUpgradeIndicator(dnnApp.Version, req.IsLocal, req.IsSecureConnection);
            var hostName = System.Net.Dns.GetHostName();
            var hostPortal = DotNetNuke.Entities.Portals.PortalController.Instance.GetPortal(Host.HostPortalID);
            var portalCount = DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals().Count;

            var hm = new HostModel
            {
                Version = dnnApp.Version.ToString(),
                Product = dnnApp.Description,
                UpgradeAvailable = upgradeIndicator != null,
                Framework = DotNetNuke.Common.Globals.NETFrameworkVersion.ToString(2),
                IpAddress = System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString(),
                Permissions = DotNetNuke.Framework.SecurityPolicy.Permissions,
                Site = hostPortal.PortalName,
                Title = Host.HostTitle,
                Url = Host.HostURL,
                Email = Host.HostEmail,
                Theme = Utilities.FormatSkinName(Host.DefaultPortalSkin),
                EditTheme = Utilities.FormatSkinName(Host.DefaultAdminSkin),
                Container = Utilities.FormatContainerName(Host.DefaultPortalContainer),
                EditContainer = Utilities.FormatContainerName(Host.DefaultAdminContainer),
                PortalCount = portalCount
            };
            return hm;
        }

    }
}