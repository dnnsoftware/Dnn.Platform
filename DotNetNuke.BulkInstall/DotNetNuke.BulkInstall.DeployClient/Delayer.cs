// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;
using System.Threading.Tasks;

/// <summary>The <see cref="IDelayer"/> implementation, using <see cref="Task.Delay(TimeSpan)"/>.</summary>
public class Delayer : IDelayer
{
    /// <inheritdoc/>
    public Task Delay(TimeSpan delay)
    {
        return Task.Delay(delay);
    }
}
