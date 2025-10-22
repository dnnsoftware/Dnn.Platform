// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;

/// <summary>Provides the ability to manage upgrades of DNN from local upgrade package files.</summary>
public interface ILocalUpgradeService
{
    /// <summary>Gets information about the available local upgrade packages.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which resolves to a list of <see cref="LocalUpgradeInfo"/> instances.</returns>
    Task<IReadOnlyList<LocalUpgradeInfo>> GetLocalUpgrades(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves information about a local upgrade from the specified file.
    /// </summary>
    /// <param name="file">The path to the DNN installation file. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="LocalUpgradeInfo"/>
    /// object with details about the upgrade.</returns>
    Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string file, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves information about a local upgrade package from a stream.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to extract and analyze the contents of the
    /// provided package archive. Ensure that the <paramref name="archiveStream"/> is not disposed or closed before the
    /// operation completes.</remarks>
    /// <param name="packageName">The name of the package to be upgraded. Cannot be null or empty.</param>
    /// <param name="archiveStream">A stream containing the package archive. Must be readable and positioned at the start of the archive.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="LocalUpgradeInfo"/>
    /// object with details about the upgrade package.</returns>
    Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string packageName, Stream archiveStream, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified local upgrade package.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to delete a local upgrade package identified
    /// by its name. Ensure that the package name is valid and that the operation is not cancelled prematurely by the
    /// provided cancellation token.</remarks>
    /// <param name="packageName">The name of the package to be deleted. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteLocalUpgrade(string packageName, CancellationToken cancellationToken);

    /// <summary>Begins the process of upgrading the site to the next applicable version.</summary>
    /// <param name="upgrades">The list of available upgrades (from <see cref="GetLocalUpgrades"/>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating completion.</returns>
    Task StartLocalUpgrade(IReadOnlyList<LocalUpgradeInfo> upgrades, CancellationToken cancellationToken);

    /// <summary>Begins the process of upgrading the site to the specified version.</summary>
    /// <param name="upgrade">The upgrade version (from <see cref="GetLocalUpgrades"/>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating completion.</returns>
    Task StartLocalUpgrade(LocalUpgradeInfo upgrade, CancellationToken cancellationToken);
}
