// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.Console
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
    /// <remarks>The Dnn.Modules.Console project has been assigned event IDs from 3,500,000 to 3,599,999.</remarks>
    internal static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 3_500_000, Level = LogLevel.Error)]
        public static partial void SettingsParseWidthException(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 3_500_100, Level = LogLevel.Error)]
        public static partial void ViewConsoleParseConsoleModuleIdException(this ILogger logger, Exception exception);
    }
}
