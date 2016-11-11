#region Copyright
//
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

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
                                                (p) => p.Name == PackageDependency.PackageName 
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