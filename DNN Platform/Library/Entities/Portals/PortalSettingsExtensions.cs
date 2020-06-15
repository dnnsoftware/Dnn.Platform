// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;

    public static class PortalSettingsExtensions
    {
        /// <summary>
        /// Detect whether current page is custom error page.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <returns></returns>
        public static bool InErrorPageRequest(this PortalSettings portalSettings)
        {
            return portalSettings.ActiveTab.TabID == portalSettings.ErrorPage404
                   || portalSettings.ActiveTab.TabID == portalSettings.ErrorPage500;
        }
    }
}
