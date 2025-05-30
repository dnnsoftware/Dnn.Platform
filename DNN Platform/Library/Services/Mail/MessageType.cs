// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail
{
    public enum MessageType
    {
        /// <summary>A password reminder.</summary>
        PasswordReminder = 0,

        /// <summary>A user's profile was updated.</summary>
        ProfileUpdated = 1,

        /// <summary>A user registered notification sent to a site admin.</summary>
        UserRegistrationAdmin = 2,

        /// <summary>A user registered on a site with private registration.</summary>
        UserRegistrationPrivate = 3,

        /// <summary>A user registered on a site with private registration but no approval is required.</summary>
        UserRegistrationPrivateNoApprovalRequired = 4,

        /// <summary>A user registered on a site with public registration.</summary>
        UserRegistrationPublic = 5,

        /// <summary>A user registered on a site with verified registration.</summary>
        UserRegistrationVerified = 6,

        /// <summary>A user updated their own password.</summary>
        UserUpdatedOwnPassword = 7,

        /// <summary>A user's password was updated.</summary>
        PasswordUpdated = 8,

        /// <summary>An unapproved user requested a password reminder.</summary>
        PasswordReminderUserIsNotApproved = 9,

        /// <summary>A user was authorized.</summary>
        UserAuthorized = 10,

        /// <summary>A user's authorization was removed.</summary>
        UserUnAuthorized = 11,

        /// <summary>A notification sent to a site admin when an unapproved user requested a password reset.</summary>
        PasswordReminderUserIsNotApprovedAdmin = 12,
    }
}
