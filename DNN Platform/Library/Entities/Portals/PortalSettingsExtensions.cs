// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DotNetNuke.Entities.Portals
{
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
