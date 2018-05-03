using System.Linq;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Library.Helper
{
    public class ContentVerifier : IContentVerifier
    {
        private IPortalController _portalController;
        private IPortalGroupController _portalGroupController;

        public ContentVerifier() : this(PortalController.Instance, PortalGroupController.Instance)
        {
        }

        public ContentVerifier(IPortalController portalController, IPortalGroupController portalGroupController)
        {
            this._portalController = portalController;
            this._portalGroupController = portalGroupController;
        }

        public bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {
            var currentPortal = _portalController.GetCurrentPortalSettings();
            return contentPortalId == portalSettings.PortalId
                || portalSettings == currentPortal
                || (checkForSiteGroup && IsRequestForSiteGroup(contentPortalId, portalSettings.PortalId));
        }

        public bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
        {
            const int NO_SITE_GROUPID = -1;
            var isSiteGroupPage = false;
            var portal = _portalController.GetPortal(portalIdSiteGroup);

            if (portal.PortalGroupID != NO_SITE_GROUPID)
            {
                isSiteGroupPage = _portalGroupController.GetPortalsByGroup(portal.PortalGroupID).Any(p => p.PortalID == portalId);
            }

            return isSiteGroupPage;
        }
    }
}
