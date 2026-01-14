// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles
{
    public enum RoleStatus
    {
        /// <summary>A role which needs to be approved.</summary>
        Pending = -1,

        /// <summary>A role which has been disabled.</summary>
        Disabled = 0,

        /// <summary>A role which has been approved.</summary>
        Approved = 1,
    }
}
