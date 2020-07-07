// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Xml.XPath;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallerFactory is a factory class that is used to instantiate the
    /// appropriate Component Installer.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class InstallerFactory
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetInstaller method instantiates the relevant Component Installer.
        /// </summary>
        /// <param name="installerType">The type of Installer.</param>
        /// <returns></returns>
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
                    // Installer type is defined in the List
                    var listController = new ListController();
                    ListEntryInfo entry = listController.GetListEntryInfo("Installer", installerType);

                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
                        // The class for the Installer is specified in the Text property
                        installer = (ComponentInstallerBase)Reflection.CreateObject(entry.Text, "Installer_" + entry.Value);
                    }

                    break;
            }

            return installer;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetInstaller method instantiates the relevant Component Installer.
        /// </summary>
        /// <param name="manifestNav">The manifest (XPathNavigator) for the component.</param>
        /// <param name="package">The associated PackageInfo instance.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ComponentInstallerBase GetInstaller(XPathNavigator manifestNav, PackageInfo package)
        {
            string installerType = Util.ReadAttribute(manifestNav, "type");
            string componentVersion = Util.ReadAttribute(manifestNav, "version");

            ComponentInstallerBase installer = GetInstaller(installerType);
            if (installer != null)
            {
                // Set package
                installer.Package = package;

                // Set type
                installer.Type = installerType;

                if (!string.IsNullOrEmpty(componentVersion))
                {
                    installer.Version = new Version(componentVersion);
                }
                else
                {
                    installer.Version = package.Version;
                }

                // Read Manifest
                if (package.InstallerInfo.InstallMode != InstallMode.ManifestOnly || installer.SupportsManifestOnlyInstall)
                {
                    installer.ReadManifest(manifestNav);
                }
            }

            return installer;
        }
    }
}
