// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper
{
    using System.Linq;

    using DotNetNuke.Entities.Portals;

    public class ContentVerifier : IContentVerifier
    {
        private IPortalController _portalController;
        private IPortalGroupController _portalGroupController;

        public ContentVerifier()
            : this(PortalController.Instance, PortalGroupController.Instance)
        {
        }

        public ContentVerifier(IPortalController portalController, IPortalGroupController portalGroupController)
        {
            this._portalController = portalController;
            this._portalGroupController = portalGroupController;
        }

        public bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {
            var currentPortal = this._portalController.GetCurrentPortalSettings();
            return contentPortalId == portalSettings.PortalId
                || portalSettings == currentPortal
                || (checkForSiteGroup && this.IsRequestForSiteGroup(contentPortalId, portalSettings.PortalId));
        }

        public bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
        {
            const int NO_SITE_GROUPID = -1;
            var isSiteGroupPage = false;
            var portal = this._portalController.GetPortal(portalIdSiteGroup);

            if (portal.PortalGroupID != NO_SITE_GROUPID)
            {
                isSiteGroupPage = this._portalGroupController.GetPortalsByGroup(portal.PortalGroupID).Any(p => p.PortalID == portalId);
            }

            return isSiteGroupPage;
        }
    }
}
