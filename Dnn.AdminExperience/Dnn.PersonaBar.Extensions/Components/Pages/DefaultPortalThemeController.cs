// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;

    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    public class DefaultPortalThemeController : ServiceLocator<IDefaultPortalThemeController, DefaultPortalThemeController>, IDefaultPortalThemeController
    {
        public string GetDefaultPortalContainer()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null) return null;
            return PortalController.GetPortalSetting("DefaultPortalContainer", portalSettings.PortalId, Host.DefaultPortalContainer, portalSettings.CultureCode);
        }

        public string GetDefaultPortalLayout()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null) return null;
            return PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId, Host.DefaultPortalSkin, portalSettings.CultureCode);
        }

        protected override Func<IDefaultPortalThemeController> GetFactory()
        {
            return () => new DefaultPortalThemeController();
        }
    }
}
