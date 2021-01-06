// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Security.Membership
{
    public enum UserLoginStatus
    {
        LOGIN_FAILURE = 0,
        LOGIN_SUCCESS = 1,
        LOGIN_SUPERUSER = 2,
        LOGIN_USERLOCKEDOUT = 3,
        LOGIN_USERNOTAPPROVED = 4,
        [Obsolete("Deprecated in 9.8.1.  Scheduled removal in v11.0.0.  No alternative method implemented.")]
        LOGIN_INSECUREADMINPASSWORD = 5,
        [Obsolete("Deprecated in 9.8.1.  Scheduled removal in v11.0.0.  No alternative method implemented.")]
        LOGIN_INSECUREHOSTPASSWORD = 6,
    }
}
