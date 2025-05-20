// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security
{
    /// <summary>
    /// The SecurityAccessLevel enum is used to determine which level of access rights
    /// to assign to a specific module or module action.
    /// </summary>
    public enum SecurityAccessLevel
    {
        /// <summary>Only visible in the control panel.</summary>
        ControlPanel = -3,

        /// <summary>Visible via a skin object.</summary>
        SkinObject = -2,

        /// <summary>Visible to any visitor.</summary>
        Anonymous = -1,

        /// <summary>Visible to users with view permissions for the module.</summary>
        View = 0,

        /// <summary>Visible to users with edit permissions for the module.</summary>
        Edit = 1,

        /// <summary>Visible to users with the Administrators role.</summary>
        Admin = 2,

        /// <summary>Visible to superusers.</summary>
        Host = 3,

        /// <summary>Visible to users who have access to view the permissions of a page.</summary>
        ViewPermissions = 4,
    }
}
