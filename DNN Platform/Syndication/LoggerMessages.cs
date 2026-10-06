// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Syndication;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Syndication project has been assigned event IDs from 2,300,000 to 2,399,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 2_300_000, Level = LogLevel.Error)]
    public static partial void OpmlDownloadManagerDeleteException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_300_100, Level = LogLevel.Error)]
    public static partial void RssDownloadManagerDeleteException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_300_200, Level = LogLevel.Error)]
    public static partial void OpmlParseCreatedException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_300_201, Level = LogLevel.Error)]
    public static partial void OpmlParseXmlUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_300_201, Level = LogLevel.Error)]
    public static partial void OpmlParseHtmlUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_300_201, Level = LogLevel.Error)]
    public static partial void OpmlParseUrlException(this ILogger logger, Exception exception);
}
