// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;

/// <summary>A contact specifying the ability to determine how long an operation takes.</summary>
public interface IStopwatch
{
    /// <summary>Gets the length of time elapsed since the stopwatch was started.</summary>
    TimeSpan Elapsed { get; }

    /// <summary>Starts the stopwatch.</summary>
    void StartNew();
}
