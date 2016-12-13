#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Entities.Portals
{
    public interface IPortalSettingsController
    {
        void ConfigureActiveTab(PortalSettings portalSetting);

        PortalSettings.PortalAliasMapping GetPortalAliasMappingMode(int portalId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetActiveTab method gets the active Tab for the current request
        /// </summary>
        /// <returns></returns>
        ///	<param name="tabId">The current tab's id</param>
        ///	<param name="portalSettings">The current PortalSettings</param>
        /// -----------------------------------------------------------------------------
        TabInfo GetActiveTab(int tabId, PortalSettings portalSettings);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetTabModules method gets the list of modules for the active Tab
        /// </summary>
        /// <returns></returns>
        ///	<param name="portalSettings">The current PortalSettings</param>
        /// -----------------------------------------------------------------------------
        IList<ModuleInfo> GetTabModules(PortalSettings portalSettings);

        ///  -----------------------------------------------------------------------------
        ///  <summary>
        ///  The LoadPortal method loads the properties of the portal object into the Portal Settings
        ///  </summary>
        /// <param name="portal">The Portal object</param>
        /// <param name="portalSettings">The Portal Settings object</param>
        ///  -----------------------------------------------------------------------------
        void LoadPortal(PortalInfo portal, PortalSettings portalSettings);

        ///  -----------------------------------------------------------------------------
        ///  <summary>
        ///  The LoadPortalSettings method loads the settings into the Portal Settings
        ///  </summary>
        /// <param name="portalSettings">The Portal Settings object</param>
        ///  -----------------------------------------------------------------------------
        void LoadPortalSettings(PortalSettings portalSettings);
    }
}
