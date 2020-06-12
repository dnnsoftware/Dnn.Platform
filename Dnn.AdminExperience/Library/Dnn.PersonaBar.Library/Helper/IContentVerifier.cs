// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Library.Helper
{
    public interface IContentVerifier
    {
        bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false);

        bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup);
    }
}
