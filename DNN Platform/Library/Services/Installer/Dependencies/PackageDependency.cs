// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies;

using System;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;

/// <summary>The PackageDependency determines whether the dependent package is installed.</summary>
public class PackageDependency : DependencyBase
{
    private string packageName;

    /// <inheritdoc/>
    public override string ErrorMessage
    {
        get
        {
            return Util.INSTALL_Package + " - " + this.packageName;
        }
    }

    /// <inheritdoc/>
    public override bool IsValid
    {
        get
        {
            bool isValid = true;

            // Get Package from DataStore
            PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.Name.Equals(this.packageName, StringComparison.OrdinalIgnoreCase));
            if (package == null)
            {
                isValid = false;
            }

            return isValid;
        }
    }

    /// <inheritdoc/>
    public override void ReadManifest(XPathNavigator dependencyNav)
    {
        this.packageName = dependencyNav.Value;
    }
}
