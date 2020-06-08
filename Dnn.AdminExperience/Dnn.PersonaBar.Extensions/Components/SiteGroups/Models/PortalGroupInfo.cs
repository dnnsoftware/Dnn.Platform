// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections.Generic;

namespace Dnn.PersonaBar.SiteGroups.Models
{
    public class PortalGroupInfo
    {
        public int PortalGroupId { get; set; }
        public string PortalGroupName { get; set; }
        public string AuthenticationDomain { get; set; }
        public PortalInfo MasterPortal { get; set; }
        public IEnumerable<PortalInfo> Portals { get; set; }
        public string Description { get; set; }
    }
}
