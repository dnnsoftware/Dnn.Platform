﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml.XPath;

using DotNetNuke.Common.Lists;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DependencyFactory is a factory class that is used to instantiate the
    /// appropriate Dependency
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DependencyFactory
    {
        #region Public Shared Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetDependency method instantiates (and returns) the relevant Dependency
        /// </summary>
        /// <param name="dependencyNav">The manifest (XPathNavigator) for the dependency</param>
        /// -----------------------------------------------------------------------------
        public static IDependency GetDependency(XPathNavigator dependencyNav)
        {
            IDependency dependency = null;
            string dependencyType = Util.ReadAttribute(dependencyNav, "type");
            switch (dependencyType.ToLowerInvariant())
            {
                case "coreversion":
                    dependency = new CoreVersionDependency();
                    break;
                case "package":
                    dependency = new PackageDependency();
                    break;
                case "managedpackage":
                    dependency = new ManagedPackageDependency();
                    break;
                case "permission":
                    dependency = new PermissionsDependency();
                    break;
                case "type":
                    dependency = new TypeDependency();
                    break;
                default:
                    //Dependency type is defined in the List
                    var listController = new ListController();
                    ListEntryInfo entry = listController.GetListEntryInfo("Dependency", dependencyType);
                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
                        //The class for the Installer is specified in the Text property
                        dependency = (DependencyBase)Reflection.CreateObject(entry.Text, "Dependency_" + entry.Value);
                    }
                    break;
            }
            if (dependency == null)
            {
                //Could not create dependency, show generic error message
                dependency = new InvalidDependency(Util.INSTALL_Dependencies);
            }
            //Read Manifest
            dependency.ReadManifest(dependencyNav);

            return dependency;
        }

        #endregion
    }
}
