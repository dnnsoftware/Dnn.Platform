// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Website
{
    using System;
    using System.Threading;
    using System.Web.Security;

    using Microsoft.Extensions.Logging;

    /// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
    /// <remarks>The DotNetNuke.Website project has been assigned event IDs from 1,000,000 to 1,199,999.</remarks>
    internal static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 1_000_000, Level = LogLevel.Debug)]
        public static partial void SecurityRolesThreadAbortException(this ILogger logger, ThreadAbortException exception);

        [LoggerMessage(EventId = 1_000_001, Level = LogLevel.Debug)]
        public static partial void ModuleSettingsThreadAbortException(this ILogger logger, ThreadAbortException exception);

        [LoggerMessage(EventId = 1_000_100, Level = LogLevel.Error)]
        public static partial void InstallDeletePortalResourcesFileException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_101, Level = LogLevel.Error)]
        public static partial void InstallNoUpgradeException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_102, Level = LogLevel.Error, Message = "{Message}")]
        public static partial void InstallAddFcnModeErrorMessage(this ILogger logger, string message);

        [LoggerMessage(EventId = 1_000_200, Level = LogLevel.Error)]
        public static partial void UpgradeWizardAntiForgeryTokenException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_300, Level = LogLevel.Error)]
        public static partial void ToastInitializeConfigException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_400, Level = LogLevel.Error)]
        public static partial void PayPalSubscriptionUserAddressException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_500, Level = LogLevel.Error)]
        public static partial void EditUserUpdateException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_600, Level = LogLevel.Error)]
        public static partial void PasswordResetArgumentException(this ILogger logger, ArgumentException exception);

        [LoggerMessage(EventId = 1_000_601, Level = LogLevel.Error)]
        public static partial void PasswordResetGeneralException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_602, Level = LogLevel.Error)]
        public static partial void PasswordUserResetArgumentException(this ILogger logger, ArgumentException exception);

        [LoggerMessage(EventId = 1_000_603, Level = LogLevel.Error)]
        public static partial void PasswordUserResetGeneralException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_604, Level = LogLevel.Error)]
        public static partial void PasswordUserUpdateMembershipPasswordException(this ILogger logger, MembershipPasswordException exception);

        [LoggerMessage(EventId = 1_000_605, Level = LogLevel.Error)]
        public static partial void PasswordUserUpdateGeneralException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_606, Level = LogLevel.Error)]
        public static partial void PasswordUserAdminUpdateMembershipPasswordException(this ILogger logger, MembershipPasswordException exception);

        [LoggerMessage(EventId = 1_000_607, Level = LogLevel.Error)]
        public static partial void PasswordUserAdminUpdateGeneralException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_700, Level = LogLevel.Error)]
        public static partial void UserUpdateException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_800, Level = LogLevel.Error)]
        public static partial void DnnLoginCleanUsernameException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_801, Level = LogLevel.Error)]
        public static partial void DnnLoginSetFormFocusException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_000_900, Level = LogLevel.Error)]
        public static partial void AuthenticationLoginPageNoException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_001_000, Level = LogLevel.Error, Message = "CSP error")]
        public static partial void DefaultCspError(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1_001_100, Level = LogLevel.Error, Message = "WIZARD ERROR:")]
        public static partial void InstallWizardError(this ILogger logger, Exception exception);
    }
}
