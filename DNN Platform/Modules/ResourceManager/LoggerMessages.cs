// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
    /// <remarks>The Dnn.Modules.ResourceManager project has been assigned event IDs from 3,600,000 to 3,699,999.</remarks>
    internal static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 3_600_000, Level = LogLevel.Information, Message = "Adding Global Assets host menu item.")]
        public static partial void ResourceManagerControllerAddingGlobalAssetsHostMenuItem(this ILogger logger);

        [LoggerMessage(EventId = 3_600_001, Level = LogLevel.Information, Message = "Added Global Assets host menu item.")]
        public static partial void ResourceManagerControllerAddedGlobalAssetsHostMenuItem(this ILogger logger);

        [LoggerMessage(EventId = 3_600_002, Level = LogLevel.Information, Message = "Removing old pages.")]
        public static partial void ResourceManagerControllerRemovingOldPages(this ILogger logger);

        [LoggerMessage(EventId = 3_600_003, Level = LogLevel.Error)]
        public static partial void ResourceManagerControllerUpgradeModuleException(this ILogger logger, Exception exception);
    }
}
