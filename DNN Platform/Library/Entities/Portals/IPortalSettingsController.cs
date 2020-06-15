// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;

    public interface IPortalSettingsController
    {
        void ConfigureActiveTab(PortalSettings portalSetting);

        PortalSettings.PortalAliasMapping GetPortalAliasMappingMode(int portalId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetActiveTab method gets the active Tab for the current request.
        /// </summary>
        /// <returns></returns>
        ///     <param name="tabId">The current tab's id.</param>
        ///     <param name="portalSettings">The current PortalSettings.</param>
        /// -----------------------------------------------------------------------------
        TabInfo GetActiveTab(int tabId, PortalSettings portalSettings);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetTabModules method gets the list of modules for the active Tab.
        /// </summary>
        /// <returns></returns>
        ///     <param name="portalSettings">The current PortalSettings.</param>
        /// -----------------------------------------------------------------------------
        IList<ModuleInfo> GetTabModules(PortalSettings portalSettings);

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  The LoadPortal method loads the properties of the portal object into the Portal Settings.
        ///  </summary>
        /// <param name="portal">The Portal object.</param>
        /// <param name="portalSettings">The Portal Settings object.</param>
        /// -----------------------------------------------------------------------------
        void LoadPortal(PortalInfo portal, PortalSettings portalSettings);

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  The LoadPortalSettings method loads the settings into the Portal Settings.
        ///  </summary>
        /// <param name="portalSettings">The Portal Settings object.</param>
        /// -----------------------------------------------------------------------------
        void LoadPortalSettings(PortalSettings portalSettings);
    }
}
