// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum UserCreateStatus
    {
        /// <summary>The user was successfully created.</summary>
        AddUser = 0,

        /// <summary>A user with the username already exists.</summary>
        UsernameAlreadyExists = 1,

        /// <summary>The user is already registered.</summary>
        UserAlreadyRegistered = 2,

        /// <summary>A user with the email already exists.</summary>
        DuplicateEmail = 3,

        /// <summary>The user provider already has a user with a matching key.</summary>
        DuplicateProviderUserKey = 4,

        /// <summary>A user with the username already exists.</summary>
        DuplicateUserName = 5,

        /// <summary>The change password answer is invalid.</summary>
        InvalidAnswer = 6,

        /// <summary>The email address is invalid.</summary>
        InvalidEmail = 7,

        /// <summary>The password is invalid.</summary>
        InvalidPassword = 8,

        /// <summary>The user provider produced an invalid user key.</summary>
        InvalidProviderUserKey = 9,

        /// <summary>The change password question is invalid.</summary>
        InvalidQuestion = 10,

        /// <summary>The username is invalid.</summary>
        InvalidUserName = 11,

        /// <summary>There was an unexpected error from the user provider.</summary>
        ProviderError = 12,

        /// <summary>The user was successfully created.</summary>
        Success = 13,

        /// <summary>There was an unexpected error.</summary>
        UnexpectedError = 14,

        /// <summary>The user was rejected.</summary>
        UserRejected = 15,

        /// <summary>The confirm password did not match the password.</summary>
        PasswordMismatch = 16,

        /// <summary>The existing user was added to the portal.</summary>
        AddUserToPortal = 17,

        /// <summary>The user's display name was invalid.</summary>
        InvalidDisplayName = 18,

        /// <summary>A user with the display name already exists.</summary>
        DuplicateDisplayName = 19,

        /// <summary>The supplied password was on the banned password list.</summary>
        BannedPasswordUsed = 20,

        /// <summary>The user's first name was invalid.</summary>
        InvalidFirstName = 21,

        /// <summary>The user's last name was invalid.</summary>
        InvalidLastName = 22,
    }
}
