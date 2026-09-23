// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;
using System.Threading.Tasks;

/// <summary>A contract specifying the ability to delay execution.</summary>
public interface IDelayer
{
    /// <summary>Produces a task which completes after the delay.</summary>
    /// <param name="delay">The amount of delay.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task Delay(TimeSpan delay);
}
