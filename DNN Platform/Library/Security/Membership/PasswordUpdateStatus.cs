// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership;

public enum PasswordUpdateStatus
{
    Success = 0,
    PasswordMissing = 1,
    PasswordNotDifferent = 2,
    PasswordResetFailed = 3,
    PasswordInvalid = 4,
    PasswordMismatch = 5,
    InvalidPasswordAnswer = 6,
    InvalidPasswordQuestion = 7,
    BannedPasswordUsed = 8,
}
