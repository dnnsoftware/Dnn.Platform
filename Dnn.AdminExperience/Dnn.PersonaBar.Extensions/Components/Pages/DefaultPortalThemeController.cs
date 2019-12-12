// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Pages.Components
{
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
