// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

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