// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Security.Roles
{
    public class RoleEventArgs : EventArgs
    {
        public RoleInfo Role { get; set; }

        public UserInfo User { get; set; }
    }
}
