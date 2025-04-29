// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Dependencies;

using System;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;

public class ManagedPackageDependency : DependencyBase, IManagedPackageDependency
{
    /// <inheritdoc/>
    public override string ErrorMessage
    {
        get
        {
            return Util.INSTALL_Package + " - " + this.PackageDependency.PackageName;
        }
    }

    /// <inheritdoc/>
    public override bool IsValid
    {
        get
        {
            bool isValid = true;

            // Get Package from DataStore
            PackageInfo package = PackageController.Instance.GetExtensionPackage(
                Null.NullInteger,
                (p) => p.Name.Equals(this.PackageDependency.PackageName, StringComparison.OrdinalIgnoreCase)
                       && p.Version >= this.PackageDependency.Version);
            if (package == null)
            {
                isValid = false;
            }

            return isValid;
        }
    }

    /// <inheritdoc/>
    public PackageDependencyInfo PackageDependency { get; set; }

    /// <inheritdoc/>
    public override void ReadManifest(XPathNavigator dependencyNav)
    {
        this.PackageDependency = new PackageDependencyInfo
        {
            PackageName = dependencyNav.Value,
            Version = new Version(Util.ReadAttribute(dependencyNav, "version")),
        };
    }
}
