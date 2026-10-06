// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
    /// <remarks>The DotNetNuke.Modules.CoreMessaging project has been assigned event IDs from 3,000,000 to 3,099,999.</remarks>
    internal static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 3_000_000, Level = LogLevel.Error)]
        public static partial void CoreMessagingBusinessControllerUpgradeModuleException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_100, Level = LogLevel.Error)]
        public static partial void FileUploadControllerUploadFileException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_200, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to fetch the inbox, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorFetchingInbox(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_201, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to fetch the Sent box, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorFetchingSentBox(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_202, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to fetch the archived box.")]
        public static partial void MessagingServiceControllerUnexpectedErrorFetchingArchivedBox(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_203, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to fetch the thread, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorFetchingThread(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_204, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to reply to a conversation, see the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorReplying(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_205, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to fetch the archived box, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorMarkingArchived(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_206, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to restore an archived conversation, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorRestoringArchivedConversation(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_207, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting mark a conversation as read, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorMarkingConversationAsRead(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_208, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to restore an archived conversation, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorMarkingConversationAsUnread(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_209, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to delete a user from a conversation, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorDeleteUserFromConversation(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_210, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to fetch messaging notifications, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorFetchingNotifications(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_211, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to check the recipient count on a reply, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorCheckRecipientCount(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_212, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to get the notification count, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorGettingNotificationCount(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_213, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to , consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorGettingUnreadCount(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_214, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to get the unread messages and new notifications count, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorGettingUnreadAndNotificationCounts(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_000_215, Level = LogLevel.Error, Message = "An unexpected error occurred while attempting to dismiss notifications, consult the server logs for more information.")]
        public static partial void MessagingServiceControllerUnexpectedErrorDismissingNotifications(this ILogger logger, Exception exception);
    }
}
