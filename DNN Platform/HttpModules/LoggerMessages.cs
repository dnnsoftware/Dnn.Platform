// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules;

using System;

using DotNetNuke.Services.Log.EventLog;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.HttpModules project has been assigned event IDs from 2,000,000 to 2,099,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 2_000_000, Level = LogLevel.Error, Message = "{LogInfo}")]
    public static partial void AnalyticsModuleOnPreRequestHandlerExecuteException(this ILogger logger, Exception exception, LogInfo logInfo);

    [LoggerMessage(EventId = 2_000_001, Level = LogLevel.Error)]
    public static partial void AnalyticsModuleOnPagePreRenderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_000_100, Level = LogLevel.Error, Message = "{LogInfo}")]
    public static partial void AnalyticsEngineConfigurationGetConfigException(this ILogger logger, Exception exception, LogInfo logInfo);

    [LoggerMessage(EventId = 2_000_200, Level = LogLevel.Error)]
    public static partial void ExceptionModuleAddLogException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_000_201, Level = LogLevel.Error)]
    public static partial void ExceptionModuleOnErrorRequestException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_000_300, Level = LogLevel.Error)]
    public static partial void BasicUrlRewriterPhysicalPathTooLong(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_000_301, Level = LogLevel.Error)]
    public static partial void BasicUrlRewriterRewriteUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_000_400, Level = LogLevel.Error)]
    public static partial void MembershipModuleRequireLogoutException(this ILogger logger, Exception exception);
}
