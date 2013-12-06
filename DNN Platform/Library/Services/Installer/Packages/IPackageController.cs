#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Generic;

namespace DotNetNuke.Services.Installer.Packages
{
    public interface IPackageController
    {
        /// <summary>
        /// DeleteExtensionPackage is used to remove an Extension Package for the System
        /// </summary>
        /// <param name="package">The Package you wish to delete</param>
        void DeleteExtensionPackage(PackageInfo package);

        /// <summary>
        /// GetExtensionPackage is used to retrieve a specific package from the data store.
        /// </summary>
        /// <param name="portalId">The Id of the portal.  Most extension packages do not belong to
        /// a specific portal so in most situations developers will need to pass -1 to indicate this.
        /// The main situation where the portal Id will have a specific value is for skins which have
        /// been installed for a spcific portal/site.</param>
        /// <param name="predicate">The "search criteria" to use to identify the extension package to
        /// retrieve.  In most cases this will be a simple lambda method e.g. p => p.Name == "Name"</param>
        /// <returns>The extension package</returns>
        PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate);

        /// <summary>
        /// GetExtensionPackages is used to retrieve packages from the data store.
        /// </summary>
        /// <param name="portalId">The Id of the portal.  Most extension packages do not belong to
        /// a specific portal so in most situations developers will need to pass -1 to indicate this.
        /// The main situation where the portal Id will have a specific value is for skins which have
        /// been installed for a spcific portal/site.</param>
        /// <returns>A list of extension packages</returns>
        IList<PackageInfo> GetExtensionPackages(int portalId);

        /// <summary>
        /// GetExtensionPackages is used to retrieve packages from the data store.
        /// </summary>
        /// <param name="portalId">The Id of the portal.  Most extension packages do not belong to
        /// a specific portal so in most situations developers will need to pass -1 to indicate this.
        /// The main situation where the portal Id will have a specific value is for skins which have
        /// been installed for a spcific portal/site.</param>
        /// <param name="predicate">The "search criteria" to use to identify the extension packages to
        /// retrieve.  In most cases this will be a simple lambda method e.g. p => p.PackageType == "Module"</param>
        /// <returns>A list of extension packages</returns>
        IList<PackageInfo> GetExtensionPackages(int portalId, Func<PackageInfo, bool> predicate);

        /// <summary>
        /// SaveExtensionPackage is used to save an Extension Package.
        /// </summary>
        /// <param name="package">The Package you wish to save</param>
        void SaveExtensionPackage(PackageInfo package);

        /// <summary>
        /// GetExtensionPackage is used to retrieve a specific package type from the data store.
        /// </summary>
        /// <param name="predicate">The "search criteria" to use to identify the package type to
        /// retrieve.  In most cases this will be a simple lambda method e.g. t => t.PackageType == "Modules"</param>
        /// <returns>A package type</returns>
        PackageType GetExtensionPackageType(Func<PackageType, bool> predicate);

        /// <summary>
        /// GetExtensionPackageTypes is used to retrieve package types from the data store.
        /// </summary>
        /// <returns>A list of package types</returns>
        IList<PackageType> GetExtensionPackageTypes();

        /// <summary>
        /// Get the dependencies for a package
        /// </summary>
        /// <returns>A List of PackageDependencyInfo objects</returns>
        IList<PackageDependencyInfo> GetPackageDependencies(Func<PackageDependencyInfo, bool> predicate);
    }
}
