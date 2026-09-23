// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.BulkInstall.DeployClient;

using DotNetNuke.BulkInstall.DeployClient;

public class TestLogger : ILogger
{
    public List<string> Traces { get; } = [];

    /// <inheritdoc />
    public void LogTrace(LogLevel level, string message)
    {
        this.Traces.Add(message);
    }
}
