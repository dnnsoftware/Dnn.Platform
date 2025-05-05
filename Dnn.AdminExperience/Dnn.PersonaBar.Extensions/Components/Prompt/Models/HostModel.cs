// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using Microsoft.Extensions.DependencyInjection;

    public class HostModel
    {
        // DNN Platform for example
        public string Product { get; set; }

        public string Version { get; set; }

        public bool UpgradeAvailable { get; set; }

        // .NET Framework: 4.6 for example
        public string Framework { get; set; }

        // Could be IPv6
        public string IpAddress { get; set; }

        // ReflectionPermission, WebPermission, AspNetHostingPermission, etc.
        public string Permissions { get; set; }

        // prompt.com
        public string Site { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string Email { get; set; }

        public string Theme { get; set; }

        public string Container { get; set; }

        public string EditTheme { get; set; }

        public string EditContainer { get; set; }

        public int PortalCount { get; set; }

        public static HostModel Current()
            => Current(null, null, null);

        public static HostModel Current(IHostSettings hostSettings, IPortalController portalController, IUserController userController)
        {
            hostSettings ??= Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            portalController ??= Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            userController ??= Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();

            var application = DotNetNuke.Application.DotNetNukeContext.Current.Application;
            var controlBarController = DotNetNuke.Web.Components.Controllers.ControlBarController.Instance;
            var request = HttpContextSource.Current.Request;
            var upgradeIndicator = controlBarController.GetUpgradeIndicator(application.Version, request.IsLocal, request.IsSecureConnection);
            var hostName = System.Net.Dns.GetHostName();
            var hostPortal = portalController.GetPortal(hostSettings.HostPortalId);
            var portalCount = portalController.GetPortals().Count;
            var isHost = userController.GetCurrentUserInfo()?.IsSuperUser ?? false;

            return new HostModel
            {
                Version = "v." + Globals.FormatVersion(application.Version, true),
                Product = application.Description,
                UpgradeAvailable = upgradeIndicator != null,
                Framework = isHost ? Globals.FormattedNetFrameworkVersion : string.Empty,
                IpAddress = System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString(),
                Permissions = DotNetNuke.Framework.SecurityPolicy.Permissions,
                Site = hostPortal.PortalName,
                Title = hostSettings.HostTitle,
                Url = hostSettings.HostUrl,
                Email = hostSettings.HostEmail,
                Theme = Utilities.FormatSkinName(hostSettings.DefaultPortalSkin),
                EditTheme = Utilities.FormatSkinName(hostSettings.DefaultAdminSkin),
                Container = Utilities.FormatContainerName(hostSettings.DefaultPortalContainer),
                EditContainer = Utilities.FormatContainerName(hostSettings.DefaultAdminContainer),
                PortalCount = portalCount,
            };
        }
    }
}
