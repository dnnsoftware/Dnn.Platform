// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

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