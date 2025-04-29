// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Helper;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Internal.SourceGenerators;

public partial class PortalHelper
{
    private static readonly IContentVerifier ContentVerifier = new ContentVerifier();

#pragma warning disable CS1066
    [DnnDeprecated(9, 2, 1, "Use IContentVerifier.IsContentExistsForRequestedPortal")]
    public static partial bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
    {
        return ContentVerifier.IsContentExistsForRequestedPortal(contentPortalId, portalSettings, checkForSiteGroup);
    }
#pragma warning restore CS1066
}
