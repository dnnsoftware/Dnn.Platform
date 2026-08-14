// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Maintenance project has been assigned event IDs from 2,200,000 to 2,299,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 2_200_000, Level = LogLevel.Warning, Message = "Could not determine Telerik dependencies on some assemblies.")]
    public static partial void TelerikUtilsCountNotDetermineTelerikDependenciesOnSomeAssemblies(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2_200_100, Level = LogLevel.Error)]
    public static partial void StepBaseExecuteException(this ILogger logger, Exception exception);
}
