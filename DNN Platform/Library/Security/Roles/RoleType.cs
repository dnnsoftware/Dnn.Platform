// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles
{
    public enum RoleType
    {
        /// <summary>Site administrators.</summary>
        Administrator = 0,

        /// <summary>Site subscribers.</summary>
        Subscriber = 1,

        /// <summary>Site users.</summary>
        RegisteredUser = 2,

        /// <summary>No role type.</summary>
        None = 3,

        /// <summary>Unverified users.</summary>
        UnverifiedUser = 4,
    }
}
