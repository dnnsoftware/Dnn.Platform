// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Security.Membership
{
    public enum UserCreateStatus
    {
        AddUser = 0,
        UsernameAlreadyExists = 1,
        UserAlreadyRegistered = 2,
        DuplicateEmail = 3,
        DuplicateProviderUserKey = 4,
        DuplicateUserName = 5,
        InvalidAnswer = 6,
        InvalidEmail = 7,
        InvalidPassword = 8,
        InvalidProviderUserKey = 9,
        InvalidQuestion = 10,
        InvalidUserName = 11,
        ProviderError = 12,
        Success = 13,
        UnexpectedError = 14,
        UserRejected = 15,
        PasswordMismatch = 16,
        AddUserToPortal = 17,
        InvalidDisplayName = 18,
        DuplicateDisplayName = 19,
        BannedPasswordUsed = 20,
        InvalidFirstName = 21,
        InvalidLastName = 22
    }
}
