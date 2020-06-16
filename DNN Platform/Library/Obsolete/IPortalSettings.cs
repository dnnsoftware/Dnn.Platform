// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Deprecated in DotNetNuke 7.3.0. Use PortalController.Instance.GetCurrentPortalSettings to get a mockable PortalSettings. Scheduled removal in v10.0.0.")]
    public interface IPortalSettings
    {
        string AdministratorRoleName { get; }
    }
}
