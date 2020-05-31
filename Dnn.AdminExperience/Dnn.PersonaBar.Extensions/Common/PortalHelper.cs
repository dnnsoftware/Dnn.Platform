// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Portals;
using System.Linq;

namespace Dnn.PersonaBar.Prompt.Common
{
    class PortalHelper
    {
        internal static bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
        {
            const int NO_SITE_GROUPID = -1;
            var isSiteGroupPage = false;
            var portal = PortalController.Instance.GetPortal(portalIdSiteGroup);

            if (portal.PortalGroupID != NO_SITE_GROUPID)
            {
                isSiteGroupPage = PortalGroupController.Instance.GetPortalsByGroup(portal.PortalGroupID).Any(p => p.PortalID == portalId);
            }

            return isSiteGroupPage;
        }
    }
}
