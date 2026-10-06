// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.AspNetClientCapabilityProvider;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Providers.AspNetCCP project has been assigned event IDs from 2,400,000 to 2,499,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 2_400_000, Level = LogLevel.Error)]
    public static partial void AspNetClientCapabilityDetectOperatingSystemException(this ILogger logger, Exception exception);
}
