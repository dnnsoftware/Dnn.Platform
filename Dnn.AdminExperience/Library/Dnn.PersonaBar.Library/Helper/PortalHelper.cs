using DotNetNuke.Entities.Portals;
using System;

namespace Dnn.PersonaBar.Library.Helper
{
    public class PortalHelper
    {
        private static readonly IContentVerifier _contentVerifier = new ContentVerifier();

        [Obsolete("Deprecated in 9.2.1. Use IContentVerifier.IsContentExistsForRequestedPortal")]
        public static bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {        
            return _contentVerifier.IsContentExistsForRequestedPortal(contentPortalId, portalSettings, checkForSiteGroup);            
        }       
    }
}
