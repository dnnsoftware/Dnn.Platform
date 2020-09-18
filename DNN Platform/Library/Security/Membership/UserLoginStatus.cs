// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum UserLoginStatus
    {
        LOGIN_FAILURE = 0,
        LOGIN_SUCCESS = 1,
        LOGIN_SUPERUSER = 2,
        LOGIN_USERLOCKEDOUT = 3,
        LOGIN_USERNOTAPPROVED = 4,
        LOGIN_INSECUREADMINPASSWORD = 5,
        LOGIN_INSECUREHOSTPASSWORD = 6,
    }
}
