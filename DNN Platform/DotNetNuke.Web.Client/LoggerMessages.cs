// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Web.Client project has been assigned event IDs from 1,600,000 to 1,649,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1_600_000, Level = LogLevel.Information, Message = "Removing ClientDependency from web.config")]
    public static partial void ClientResourceManagerRemovingClientDependencyFromWebConfig(this ILogger logger);

    [LoggerMessage(EventId = 1_600_001, Level = LogLevel.Information, Message = "Removing configSections/clientDependency")]
    public static partial void ClientResourceManagerRemovingClientDependencyConfigSection(this ILogger logger);

    [LoggerMessage(EventId = 1_600_002, Level = LogLevel.Information, Message = "Removing system.webServer/modules/ClientDependencyModule")]
    public static partial void ClientResourceManagerRemovingClientDependencyModule(this ILogger logger);

    [LoggerMessage(EventId = 1_600_003, Level = LogLevel.Information, Message = "Removing system.webServer/handlers/ClientDependencyHandler")]
    public static partial void ClientResourceManagerRemovingClientDependencyHandler(this ILogger logger);

    [LoggerMessage(EventId = 1_600_004, Level = LogLevel.Information, Message = "Removing clientDependency")]
    public static partial void ClientResourceManagerRemovingClientDependencyElement(this ILogger logger);

    [LoggerMessage(EventId = 1_600_100, Level = LogLevel.Warning, Message = "Failed to get get types for reflection")]
    public static partial void DependencyInjectionFailedToGetTypesForReflection(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_600_200, Level = LogLevel.Warning, Message = "Failed to get get types for reflection")]
    public static partial void ClientResourceSettingsFailedToGetTypesForReflection(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_600_201, Level = LogLevel.Warning, Message = "Failed to Get Portal Setting Through Reflection")]
    public static partial void ClientResourceSettingsFailedToGetPortalSettingThroughReflection(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_600_202, Level = LogLevel.Warning, Message = "Failed to Get Portal ID Through Reflection")]
    public static partial void ClientResourceSettingsFailedToGetPortalIdThroughReflection(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_600_203, Level = LogLevel.Warning, Message = "Failed to Get Host Setting Through Reflection")]
    public static partial void ClientResourceSettingsFailedToGetHostSettingThroughReflection(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_600_204, Level = LogLevel.Warning, Message = "Failed to Get Status By Reflection")]
    public static partial void ClientResourceSettingsFailedToGetStatusThroughReflection(this ILogger logger, Exception exception);
}
