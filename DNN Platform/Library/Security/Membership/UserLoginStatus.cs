﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
        LOGIN_INSECUREHOSTPASSWORD = 6
    }
}
