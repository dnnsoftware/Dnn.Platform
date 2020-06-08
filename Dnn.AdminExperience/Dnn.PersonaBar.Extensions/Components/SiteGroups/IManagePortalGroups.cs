// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using Dnn.PersonaBar.SiteGroups.Models;
using System.Collections.Generic;

namespace Dnn.PersonaBar.SiteGroups
{
    public interface IManagePortalGroups
    {
        IEnumerable<PortalInfo> AvailablePortals();
        void Delete(int portalGroupId);
        IEnumerable<PortalGroupInfo> SiteGroups();
        int Save(PortalGroupInfo portalGroup);
    }
}
