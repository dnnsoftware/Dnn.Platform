// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum PasswordUpdateStatus
    {
        /// <summary>Password successfully updated.</summary>
        Success = 0,

        /// <summary>The password was missing.</summary>
        PasswordMissing = 1,

        /// <summary>The new password was not different from the old password.</summary>
        PasswordNotDifferent = 2,

        /// <summary>An unexpected error occurred during the password reset.</summary>
        PasswordResetFailed = 3,

        /// <summary>The password was invalid.</summary>
        PasswordInvalid = 4,

        /// <summary>The confirmation password did not match the new password.</summary>
        PasswordMismatch = 5,

        /// <summary>The change password answer was invalid.</summary>
        InvalidPasswordAnswer = 6,

        /// <summary>The change password question was invalid.</summary>
        InvalidPasswordQuestion = 7,

        /// <summary>The supplied password was on the banned password list.</summary>
        BannedPasswordUsed = 8,
    }
}
