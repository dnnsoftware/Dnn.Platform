// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System;

    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>An implementation of <see cref="IInstaller"/> that relies on the <see cref="Installer"/> class.</summary>
    internal sealed class InstallerShim : IInstaller, IDisposable
    {
        private readonly Installer installer;

        /// <inheritdoc cref="Installer(PackageInfo, string)"/>
        public InstallerShim(PackageInfo package, string physicalSitePath)
        {
            this.installer = new Installer(package, physicalSitePath);
        }

        /// <inheritdoc/>
        public bool UnInstall(bool deleteFiles)
        {
            return this.installer.UnInstall(deleteFiles);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.installer?.Dispose();
        }
    }
}
