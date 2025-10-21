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

    Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string file, CancellationToken cancellationToken);

    Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string packageName, Stream archiveStream, CancellationToken cancellationToken);

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
