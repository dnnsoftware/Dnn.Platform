// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation;

using System;

/// <summary>A contract specifying the ability to provide <see cref="ILog"/> instances.</summary>
public interface ILoggerSource
{
    /// <summary>Gets the logger for the given <paramref name="type"/>.</summary>
    /// <param name="type">The type providing the name for the logger instance.</param>
    /// <returns>An <see cref="ILog"/> instance.</returns>
    ILog GetLogger(Type type);

    /// <summary>Gets the logger for the given <paramref name="name"/>.</summary>
    /// <param name="name">The name for the logger instance.</param>
    /// <returns>An <see cref="ILog"/> instance.</returns>
    ILog GetLogger(string name);
}
