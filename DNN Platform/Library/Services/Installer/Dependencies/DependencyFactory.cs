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
