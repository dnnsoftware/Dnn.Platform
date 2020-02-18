// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Security.Membership
{
    public enum UserRegistrationStatus
    {
        AddUser = 0,
        AddUserRoles = -1,
        UsernameAlreadyExists = -2,
        UserAlreadyRegistered = -3,
        UnexpectedError = -4
    }
}
