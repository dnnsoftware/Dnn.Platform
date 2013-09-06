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

using System;
using System.Xml.XPath;

using DotNetNuke.Common.Lists;
using DotNetNuke.Framework;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallerFactory is a factory class that is used to instantiate the
    /// appropriate Component Installer
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/24/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class InstallerFactory
    {
		#region Public Shared Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetInstaller method instantiates the relevant Component Installer
        /// </summary>
        /// <param name="installerType">The type of Installer</param>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ComponentInstallerBase GetInstaller(string installerType)
        {
            ComponentInstallerBase installer = null;
            switch (installerType)
            {
                case "File":
                    installer = new FileInstaller();
                    break;
                case "Assembly":
                    installer = new AssemblyInstaller();
                    break;
                case "ResourceFile":
                    installer = new ResourceFileInstaller();
                    break;
                case "AuthenticationSystem":
                case "Auth_System":
                    installer = new AuthenticationInstaller();
                    break;
                case "DashboardControl":
                    installer = new DashboardInstaller();
                    break;
                case "Script":
                    installer = new ScriptInstaller();
                    break;
                case "Config":
                    installer = new ConfigInstaller();
                    break;
                case "Cleanup":
                    installer = new CleanupInstaller();
                    break;
                case "Skin":
                    installer = new SkinInstaller();
                    break;
                case "Container":
                    installer = new ContainerInstaller();
                    break;
                case "Module":
                    installer = new ModuleInstaller();
                    break;
                case "CoreLanguage":
                    installer = new LanguageInstaller(LanguagePackType.Core);
                    break;
                case "ExtensionLanguage":
                    installer = new LanguageInstaller(LanguagePackType.Extension);
                    break;
                case "Provider":
                    installer = new ProviderInstaller();
                    break;
                case "SkinObject":
                    installer = new SkinControlInstaller();
                    break;
                case "UrlProvider":
                    installer = new UrlProviderInstaller();
                    break;
                case "Widget":
                    installer = new WidgetInstaller();
                    break;
                case "JavaScript_Library":
                    installer = new JavaScriptLibraryInstaller();
                    break;
                case "JavaScriptFile":
                    installer = new JavaScriptFileInstaller();
                    break;
                default:
                    //Installer type is defined in the List
                    var listController = new ListController();
                    ListEntryInfo entry = listController.GetListEntryInfo("Installer", installerType);

                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
						//The class for the Installer is specified in the Text property
                        installer = (ComponentInstallerBase) Reflection.CreateObject(entry.Text, "Installer_" + entry.Value);
                    }
                    break;
            }
            return installer;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetInstaller method instantiates the relevant Component Installer
        /// </summary>
        /// <param name="manifestNav">The manifest (XPathNavigator) for the component</param>
        /// <param name="package">The associated PackageInfo instance</param>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ComponentInstallerBase GetInstaller(XPathNavigator manifestNav, PackageInfo package)
        {
            string installerType = Util.ReadAttribute(manifestNav, "type");
            string componentVersion = Util.ReadAttribute(manifestNav, "version");

            ComponentInstallerBase installer = GetInstaller(installerType);
            if (installer != null)
            {
                //Set package
                installer.Package = package;

                //Set type
                installer.Type = installerType;

                if (!string.IsNullOrEmpty(componentVersion))
                {
                    installer.Version = new Version(componentVersion);
                }
                else
                {
                    installer.Version = package.Version;
                }
				
                //Read Manifest
                if (package.InstallerInfo.InstallMode != InstallMode.ManifestOnly || installer.SupportsManifestOnlyInstall)
                {
                    installer.ReadManifest(manifestNav);
                }
            }
            return installer;
        }
		
		#endregion
    }
}
