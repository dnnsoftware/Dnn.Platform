// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail;

public enum MessageType
{
    PasswordReminder = 0,
    ProfileUpdated = 1,
    UserRegistrationAdmin = 2,
    UserRegistrationPrivate = 3,
    UserRegistrationPrivateNoApprovalRequired = 4,
    UserRegistrationPublic = 5,
    UserRegistrationVerified = 6,
    UserUpdatedOwnPassword = 7,
    PasswordUpdated = 8,
    PasswordReminderUserIsNotApproved = 9,
    UserAuthorized = 10,
    UserUnAuthorized = 11,
    PasswordReminderUserIsNotApprovedAdmin = 12,
}
