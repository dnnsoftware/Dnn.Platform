using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Library.Helper
{
    public interface IContentVerifier
    {
        bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false);
        bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup);
    }
}
