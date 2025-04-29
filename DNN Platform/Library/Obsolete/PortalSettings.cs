// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals;

using System.ComponentModel;

using DotNetNuke.Internal.SourceGenerators;

public partial class PortalSettings
{
    [DnnDeprecated(7, 4, 0, "Replaced by PortalSettingsController.Instance().GetPortalAliasMappingMode", RemovalVersion = 10)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial PortalAliasMapping GetPortalAliasMappingMode(int portalId)
    {
        return PortalSettingsController.Instance().GetPortalAliasMappingMode(portalId);
    }
}
