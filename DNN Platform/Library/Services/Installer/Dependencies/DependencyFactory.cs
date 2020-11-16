// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System.Xml.XPath;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Framework;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DependencyFactory is a factory class that is used to instantiate the
    /// appropriate Dependency.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DependencyFactory
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetDependency method instantiates (and returns) the relevant Dependency.
        /// </summary>
        /// <param name="dependencyNav">The manifest (XPathNavigator) for the dependency.</param>
        /// <returns></returns>
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
                    // Dependency type is defined in the List
                    var listController = new ListController();
                    ListEntryInfo entry = listController.GetListEntryInfo("Dependency", dependencyType);
                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
                        // The class for the Installer is specified in the Text property
                        dependency = (DependencyBase)Reflection.CreateObject(entry.Text, "Dependency_" + entry.Value);
                    }

                    break;
            }

            if (dependency == null)
            {
                // Could not create dependency, show generic error message
                dependency = new InvalidDependency(Util.INSTALL_Dependencies);
            }

            // Read Manifest
            dependency.ReadManifest(dependencyNav);

            return dependency;
        }
    }
}
