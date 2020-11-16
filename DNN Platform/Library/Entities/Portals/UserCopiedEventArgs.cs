// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    public class UserCopiedEventArgs
    {
        public bool Cancel { get; set; }

        public string PortalName { get; set; }

        public float TotalUsers { get; set; }

        public string UserName { get; set; }

        public float UserNo { get; set; }

        public string Stage { get; set; }

        public int PortalGroupId { get; set; }
    }
}
