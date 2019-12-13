// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    using System;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageDependency determines whether the dependent package is installed
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
                return Util.INSTALL_Package + " - " + PackageName;
            }
        }

        public override bool IsValid
        {
            get
            {
                bool _IsValid = true;

                //Get Package from DataStore
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.Name.Equals(PackageName, StringComparison.OrdinalIgnoreCase));
                if (package == null)
                {
                    _IsValid = false;
                }
                return _IsValid;
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            PackageName = dependencyNav.Value;
        }
    }
}
