// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Mail
{
    public enum MessageType
    {
        PasswordReminder,
        ProfileUpdated,
        UserRegistrationAdmin,
        UserRegistrationPrivate,
        UserRegistrationPrivateNoApprovalRequired,
        UserRegistrationPublic,
        UserRegistrationVerified,
        UserUpdatedOwnPassword,
        PasswordUpdated,
        PasswordReminderUserIsNotApproved,
        UserAuthorized,
        UserUnAuthorized
    }
}
