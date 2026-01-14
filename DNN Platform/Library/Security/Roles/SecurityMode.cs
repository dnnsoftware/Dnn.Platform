// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles
{
    public enum SecurityMode
    {
        /// <summary>A security role.</summary>
        SecurityRole = 0,

        /// <summary>A social group role.</summary>
        SocialGroup = 1,

        /// <summary>A role that is both a social group and a security role.</summary>
        Both = 2,
    }
}
