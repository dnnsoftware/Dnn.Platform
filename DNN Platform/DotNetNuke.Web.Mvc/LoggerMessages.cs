// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Web.MVC project has been assigned event IDs from 1,500,000 to 1,599,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1_500_000, Level = LogLevel.Trace, Message = "Registered a total of {Count} routes")]
    public static partial void MvcRoutingManagerRegisteredRoutes(this ILogger logger, int count);

    [LoggerMessage(EventId = 1_500_001, Level = LogLevel.Trace, Message = "Mapping route: {FullRouteName} @ {RouteUrl}")]
    public static partial void MvcRoutingManagerMappingRoute(this ILogger logger, string fullRouteName, string routeUrl);

    [LoggerMessage(EventId = 1_500_002, Level = LogLevel.Error, Message = "{FullTypeName}.RegisterRoutes threw an exception.")]
    public static partial void MvcRoutingManagerRegisterRoutesThrewException(this ILogger logger, Exception exception, string fullTypeName);

    [LoggerMessage(EventId = 1_500_002, Level = LogLevel.Error, Message = "Unable to create {fullTypeName} while registering service routes.")]
    public static partial void MvcRoutingManagerUnableToCreateMapper(this ILogger logger, Exception exception, string fullTypeName);

    [LoggerMessage(EventId = 1_500_100, Level = LogLevel.Warning, Message = "The specified moniker ({Moniker}) is not defined in the system")]
    public static partial void StandardTabAndModuleInfoProviderMonikerIsNotDefined(this ILogger logger, string moniker);
}
