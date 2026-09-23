// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>A contract specifying the log messages.</summary>
public interface ILogger
{
    /// <summary>Renders a trace.</summary>
    /// <param name="level">The current log level.</param>
    /// <param name="message">The friendly message.</param>
    void LogTrace(LogLevel level, string message);
}
