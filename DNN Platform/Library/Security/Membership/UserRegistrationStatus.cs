// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum UserRegistrationStatus
    {
        /// <summary>User registered successfully.</summary>
        AddUser = 0,

        /// <summary>User successfully added to role(s).</summary>
        AddUserRoles = -1,

        /// <summary>A user with the username already exists.</summary>
        UsernameAlreadyExists = -2,

        /// <summary>The user is already registered.</summary>
        UserAlreadyRegistered = -3,

        /// <summary>There was an unexpected error in the registration process.</summary>
        UnexpectedError = -4,
    }
}
