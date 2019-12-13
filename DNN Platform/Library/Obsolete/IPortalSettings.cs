// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel;

namespace DotNetNuke.Entities.Portals.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Deprecated in DotNetNuke 7.3.0. Use PortalController.Instance.GetCurrentPortalSettings to get a mockable PortalSettings. Scheduled removal in v10.0.0.")]
    public interface IPortalSettings
    {
        string AdministratorRoleName { get; }
    }
}
