// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.DependencyInjection;

using System;
using System.Reflection;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.DependencyInjection project has been assigned event IDs from 2,100,000 to 2,299,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 2_100_000, Level = LogLevel.Warning, Message = "Unable to get all types for {AssemblyFullName}, see exception for details\n{Message}")]
    public static partial void TypeExtensionsUnableToGetAllTypesFor(this ILogger logger, ReflectionTypeLoadException exception, string assemblyFullName, string message);

    [LoggerMessage(EventId = 2_100_001, Level = LogLevel.Error, Message = "Unable to get any types for {AssemblyFullName}, see exception for details")]
    public static partial void TypeExtensionsUnableToGetAnyTypesFor(this ILogger logger, Exception exception, string assemblyFullName);

    [LoggerMessage(EventId = 2_100_002, Level = LogLevel.Error, Message = "Unable to get any types for {AssemblyFullName}, see exception for details")]
    public static partial void TypeExtensionsOtherExceptions(this ILogger logger, Exception exception, string assemblyFullName);
}
