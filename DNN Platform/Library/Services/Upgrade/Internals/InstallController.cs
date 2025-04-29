// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals;

using System;

using DotNetNuke.Framework;

/// <summary>Business Layer to manage Messaging. Also contains CRUD methods.</summary>
public class InstallController : ServiceLocator<IInstallController, InstallController>
{
    /// <inheritdoc/>
    protected override Func<IInstallController> GetFactory()
    {
        return () => new InstallControllerImpl();
    }
}
