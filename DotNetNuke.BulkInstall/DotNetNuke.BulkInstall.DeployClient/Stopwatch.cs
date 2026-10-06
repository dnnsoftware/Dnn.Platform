// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;

/// <summary>The <see cref="IStopwatch"/> implementation, using <see cref="System.Diagnostics.Stopwatch"/>.</summary>
public class Stopwatch : IStopwatch
{
    private System.Diagnostics.Stopwatch? stopwatch;

    /// <inheritdoc/>
    public TimeSpan Elapsed => this.stopwatch?.Elapsed ?? TimeSpan.Zero;

    /// <inheritdoc/>
    public void StartNew()
    {
        this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
    }
}
