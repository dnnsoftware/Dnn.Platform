// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        
        public string RoleName { get; set; }
        
        public IList<Permission> Permissions { get; set; }
        
        public bool Locked { get; set; }
        
        public bool IsDefault { get; set; }
    }
}
