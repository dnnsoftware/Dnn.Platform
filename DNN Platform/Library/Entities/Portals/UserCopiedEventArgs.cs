// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
