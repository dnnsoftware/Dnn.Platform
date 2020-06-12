// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
