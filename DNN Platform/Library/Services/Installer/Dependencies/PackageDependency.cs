// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageDependency determines whether the dependent package is installed.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PackageDependency : DependencyBase
    {
        private string PackageName;

        public override string ErrorMessage
        {
            get
            {
                return Util.INSTALL_Package + " - " + this.PackageName;
            }
        }

        public override bool IsValid
        {
            get
            {
                bool _IsValid = true;

                // Get Package from DataStore
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.Name.Equals(this.PackageName, StringComparison.OrdinalIgnoreCase));
                if (package == null)
                {
                    _IsValid = false;
                }

                return _IsValid;
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            this.PackageName = dependencyNav.Value;
        }
    }
}
