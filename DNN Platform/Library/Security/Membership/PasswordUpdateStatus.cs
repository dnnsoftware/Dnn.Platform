// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Security.Membership
{
    public enum PasswordUpdateStatus
    {
        Success,
        PasswordMissing,
        PasswordNotDifferent,
        PasswordResetFailed,
        PasswordInvalid,
        PasswordMismatch,
        InvalidPasswordAnswer,
        InvalidPasswordQuestion,
        BannedPasswordUsed
    }
}
