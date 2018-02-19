using DotNetNuke.Entities.Portals;
using System.Linq;

namespace Dnn.PersonaBar.Library.Helper
{
    public class PortalHelper
    {
        public static bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {
            var currentPortal = PortalController.Instance.GetCurrentPortalSettings();
            return contentPortalId == portalSettings.PortalId 
                || portalSettings == currentPortal 
                || (checkForSiteGroup && IsRequestForSiteGroup(contentPortalId, portalSettings.PortalId));                
        }

        private static bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
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
