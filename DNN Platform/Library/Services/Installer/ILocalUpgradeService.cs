// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Provides the ability to manage upgrades of DNN from local upgrade package files.</summary>
public interface ILocalUpgradeService
{
    /// <summary>Gets information about the available local upgrade packages.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task which resolves to a list of <see cref="LocalUpgradeInfo"/> instances.</returns>
    Task<IReadOnlyList<LocalUpgradeInfo>> GetLocalUpgrades(CancellationToken cancellationToken);

    /// <summary>Begins the process of upgrading the site to the next applicable version.</summary>
    /// <param name="upgrades">The list of available upgrades (from <see cref="GetLocalUpgrades"/>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating completion.</returns>
    Task StartLocalUpgrade(IReadOnlyList<LocalUpgradeInfo> upgrades, CancellationToken cancellationToken);
}
