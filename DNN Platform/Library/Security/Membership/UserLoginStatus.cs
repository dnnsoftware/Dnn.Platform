// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    using System;

    public enum UserLoginStatus
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>The login failed.</summary>
        LOGIN_FAILURE = 0,

        /// <summary>The login succeeded.</summary>
        LOGIN_SUCCESS = 1,

        /// <summary>The login for the superuser succeeded.</summary>
        LOGIN_SUPERUSER = 2,

        /// <summary>The user is locked out.</summary>
        LOGIN_USERLOCKEDOUT = 3,

        /// <summary>The user is not approved for login.</summary>
        LOGIN_USERNOTAPPROVED = 4,

        /// <summary>The user's password is a well-known default for admin users.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. No alternative method implemented. Scheduled for removal in v11.0.0.")]
        LOGIN_INSECUREADMINPASSWORD = 5,

        /// <summary>The user's password is a well-known default for host users.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. No alternative method implemented. Scheduled for removal in v11.0.0.")]
        LOGIN_INSECUREHOSTPASSWORD = 6,
#pragma warning restore CA1707
    }
}
