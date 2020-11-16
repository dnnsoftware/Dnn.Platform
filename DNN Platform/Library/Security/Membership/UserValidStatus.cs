// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum UserValidStatus
    {
        VALID = 0,
        PASSWORDEXPIRED = 1,
        PASSWORDEXPIRING = 2,
        UPDATEPROFILE = 3,
        UPDATEPASSWORD = 4,
        MUSTAGREETOTERMS = 5,
    }
}
