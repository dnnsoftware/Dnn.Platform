// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Security.Membership
{
    public enum UserValidStatus
    {
        VALID = 0,
        PASSWORDEXPIRED = 1,
        PASSWORDEXPIRING = 2,
        UPDATEPROFILE = 3,
        UPDATEPASSWORD = 4,
        MUSTAGREETOTERMS = 5
    }
}
