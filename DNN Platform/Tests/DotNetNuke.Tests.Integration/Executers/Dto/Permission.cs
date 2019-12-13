// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class Permission
    {
        public int PermissionId { get; set; }
        
        public string PermissionName { get; set; }
        
        public bool FullControl { get; set; }
        
        public bool View { get; set; }
        
        public bool AllowAccess { get; set; }
    }
}
