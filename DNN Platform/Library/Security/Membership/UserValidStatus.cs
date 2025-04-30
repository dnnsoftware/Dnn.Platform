// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum UserValidStatus
    {
        /// <summary>The user is valid.</summary>
        VALID = 0,

        /// <summary>The user's password is expired.</summary>
        PASSWORDEXPIRED = 1,

        /// <summary>The user's password will expire soon.</summary>
        PASSWORDEXPIRING = 2,

        /// <summary>The user's profile is not complete.</summary>
        UPDATEPROFILE = 3,

        /// <summary>The user is updating their password.</summary>
        UPDATEPASSWORD = 4,

        /// <summary>The user needs to address to the site terms.</summary>
        MUSTAGREETOTERMS = 5,
    }
}
