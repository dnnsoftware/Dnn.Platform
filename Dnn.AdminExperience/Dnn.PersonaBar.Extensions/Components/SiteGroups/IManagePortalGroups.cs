// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteGroups
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.SiteGroups.Models;

    public interface IManagePortalGroups
    {
        IEnumerable<PortalInfo> AvailablePortals();
        void Delete(int portalGroupId);
        IEnumerable<PortalGroupInfo> SiteGroups();
        int Save(PortalGroupInfo portalGroup);
    }
}