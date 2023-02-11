// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Services.Installer;

    /// <summary>An abstraction of the <see cref="Installer"/> class.</summary>
    internal interface IInstaller
    {
        /// <inheritdoc cref="Installer.UnInstall(bool)"/>
        bool UnInstall(bool deleteFiles);
    }
}
