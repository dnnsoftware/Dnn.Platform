// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper
{
    using System;

    using DotNetNuke.Entities.Portals;

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
