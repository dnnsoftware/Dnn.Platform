// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Security.Permissions
{
    public class CorePermissionProvider : PermissionProvider
    {
        public override bool SupportsFullControl()
        {
            return false;
        }
    }
}
