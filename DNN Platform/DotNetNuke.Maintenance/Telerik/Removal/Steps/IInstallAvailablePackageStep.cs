// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>Step to install packages.</summary>
    internal interface IInstallAvailablePackageStep : IStep
    {
        /// <summary>Gets or sets the package file name pattern.</summary>
        string PackageFileNamePattern { get; set; }

        /// <summary>Gets or sets the package name.</summary>
        string PackageName { get; set; }

        /// <summary>Gets or sets the package type.</summary>
        string PackageType { get; set; }
    }
}
