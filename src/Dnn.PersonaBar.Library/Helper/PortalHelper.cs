using DotNetNuke.Entities.Portals;
using System;
using System.Linq;

namespace Dnn.PersonaBar.Library.Helper
{
    public class PortalHelper
    {
        static readonly IContentVerifier contentVerifier = new ContentVerifier(PortalController.Instance, PortalGroupController.Instance);

        [Obsolete("Deprecated in 9.2.1. Use IContentVerifier.IsContentExistsForRequestedPortal")]
        public static bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {        
            return contentVerifier.IsContentExistsForRequestedPortal(contentPortalId, portalSettings, checkForSiteGroup);            
        }

        private bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
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
