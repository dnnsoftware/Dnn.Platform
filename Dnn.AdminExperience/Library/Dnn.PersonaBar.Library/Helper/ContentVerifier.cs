// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper;

using System.Linq;

using DotNetNuke.Entities.Portals;

public class ContentVerifier : IContentVerifier
{
    private IPortalController portalController;
    private IPortalGroupController portalGroupController;

    /// <summary>Initializes a new instance of the <see cref="ContentVerifier"/> class.</summary>
    public ContentVerifier()
        : this(PortalController.Instance, PortalGroupController.Instance)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ContentVerifier"/> class.</summary>
    /// <param name="portalController"></param>
    /// <param name="portalGroupController"></param>
    public ContentVerifier(IPortalController portalController, IPortalGroupController portalGroupController)
    {
        this.portalController = portalController;
        this.portalGroupController = portalGroupController;
    }

    /// <inheritdoc/>
    public bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
    {
        var currentPortal = this.portalController.GetCurrentPortalSettings();
        return contentPortalId == portalSettings.PortalId
               || portalSettings == currentPortal
               || (checkForSiteGroup && this.IsRequestForSiteGroup(contentPortalId, portalSettings.PortalId));
    }

    /// <inheritdoc/>
    public bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
    {
        const int NO_SITE_GROUPID = -1;
        var isSiteGroupPage = false;
        var portal = this.portalController.GetPortal(portalIdSiteGroup);

        if (portal.PortalGroupID != NO_SITE_GROUPID)
        {
            isSiteGroupPage = this.portalGroupController.GetPortalsByGroup(portal.PortalGroupID).Any(p => p.PortalID == portalId);
        }

        return isSiteGroupPage;
    }
}
