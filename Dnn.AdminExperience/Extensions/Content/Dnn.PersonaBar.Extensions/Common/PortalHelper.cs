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