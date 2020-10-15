// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.ComponentModel;

    public partial class PortalSettings
    {
        [Obsolete("Deprecated in DNN 7.4. Replaced by PortalSettingsController.Instance().GetPortalAliasMappingMode. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static PortalAliasMapping GetPortalAliasMappingMode(int portalId)
        {
            return PortalSettingsController.Instance().GetPortalAliasMappingMode(portalId);
        }
    }
}
