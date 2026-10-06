// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
    /// <remarks>The DotNetNuke.Modules.MemberDirectory project has been assigned event IDs from 3,300,000 to 3,399,999.</remarks>
    internal static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 3_300_000, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerAdvancedSearchException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_001, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerBasicSearchException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_002, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerGetMemberException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_003, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerGetSuggestionsException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_004, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerAcceptFriendException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_005, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerAddFriendException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_006, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerFollowException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_007, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerRemoveFriendException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_300_008, Level = LogLevel.Error)]
        public static partial void MemberDirectoryControllerUnfollowException(this ILogger logger, Exception exception);
    }
}
