// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.Threading.Tasks;

/// <summary>A contract specifying the ability to coordinate a deployment.</summary>
public interface IDeployer
{
    /// <summary>Does the deployment.</summary>
    /// <param name="options">The input options.</param>
    /// <returns>A <see cref="Task{TResult}"/> which resolves to an <see cref="ExitCode"/>.</returns>
    Task<ExitCode> StartAsync(DeployInput options);
}
