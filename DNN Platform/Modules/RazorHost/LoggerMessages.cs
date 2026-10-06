// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.RazorHost
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
    /// <remarks>The DotNetNuke.Modules.RazorHost project has been assigned event IDs from 3,400,000 to 3,499,999.</remarks>
    internal static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 3_400_000, Level = LogLevel.Error)]
        public static partial void CreateModuleDeleteFileException(this ILogger logger, Exception exception);
    }
}
