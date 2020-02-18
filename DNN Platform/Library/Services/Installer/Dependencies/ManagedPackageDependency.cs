// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;

namespace DotNetNuke.Services.Installer.Dependencies
{
    public class ManagedPackageDependency : DependencyBase, IManagedPackageDependency
    {

        public override string ErrorMessage
        {
            get
            {
                return Util.INSTALL_Package + " - " + PackageDependency.PackageName;
            }
        }

        public override bool IsValid
        {
            get
            {
                bool _IsValid = true;

                //Get Package from DataStore
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, 
                                                (p) => p.Name.Equals(PackageDependency.PackageName, StringComparison.OrdinalIgnoreCase)
                                                                && p.Version >= PackageDependency.Version);
                if (package == null)
                {
                    _IsValid = false;
                }
                return _IsValid;
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            PackageDependency = new PackageDependencyInfo
            {
                PackageName = dependencyNav.Value,
                Version = new Version(Util.ReadAttribute(dependencyNav, "version"))
            };
        }

        #region IManagedPackageDependency Implementation

        public PackageDependencyInfo PackageDependency { get; set; }

        #endregion
    }
}
