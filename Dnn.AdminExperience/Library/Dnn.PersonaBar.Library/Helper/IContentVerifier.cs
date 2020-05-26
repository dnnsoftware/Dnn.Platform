// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Library.Helper
{
    public interface IContentVerifier
    {
        bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false);
        bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup);
    }
}
