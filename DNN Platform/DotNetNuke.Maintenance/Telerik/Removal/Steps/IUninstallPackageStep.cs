// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>Uninstalls a package.</summary>
    internal interface IUninstallPackageStep : IStep
    {
        /// <summary>
        /// Gets or sets a value indicating whether to delete the package files or not
        /// in addition to uninstalling the package.
        /// </summary>
        bool DeleteFiles { get; set; }

        /// <summary>Gets or sets the name of the package to uninstall.</summary>
        string PackageName { get; set; }
    }
}
