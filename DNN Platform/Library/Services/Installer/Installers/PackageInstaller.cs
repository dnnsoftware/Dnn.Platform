// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Installer.Dependencies;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageInstaller class is an Installer for Packages.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PackageInstaller : ComponentInstallerBase
    {
        private readonly SortedList<int, ComponentInstallerBase> _componentInstallers = new SortedList<int, ComponentInstallerBase>();
        private PackageInfo _installedPackage;
        private EventMessage _eventMessage;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstaller"/> class.
        /// This Constructor creates a new PackageInstaller instance.
        /// </summary>
        /// <param name="package">A PackageInfo instance.</param>
        /// -----------------------------------------------------------------------------
        public PackageInstaller(PackageInfo package)
        {
            this.IsValid = true;
            this.DeleteFiles = Null.NullBoolean;
            this.Package = package;
            if (!string.IsNullOrEmpty(package.Manifest))
            {
                // Create an XPathDocument from the Xml
                var doc = new XPathDocument(new StringReader(package.Manifest));
                XPathNavigator nav = doc.CreateNavigator().SelectSingleNode("package");
                this.ReadComponents(nav);
            }
            else
            {
                ComponentInstallerBase installer = InstallerFactory.GetInstaller(package.PackageType);
                if (installer != null)
                {
                    // Set package
                    installer.Package = package;

                    // Set type
                    installer.Type = package.PackageType;
                    this._componentInstallers.Add(0, installer);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstaller"/> class.
        /// This Constructor creates a new PackageInstaller instance.
        /// </summary>
        /// <param name="info">An InstallerInfo instance.</param>
        /// <param name="packageManifest">The manifest as a string.</param>
        /// -----------------------------------------------------------------------------
        public PackageInstaller(string packageManifest, InstallerInfo info)
        {
            this.IsValid = true;
            this.DeleteFiles = Null.NullBoolean;
            this.Package = new PackageInfo(info);
            this.Package.Manifest = packageManifest;

            if (!string.IsNullOrEmpty(packageManifest))
            {
                // Create an XPathDocument from the Xml
                var doc = new XPathDocument(new StringReader(packageManifest));
                XPathNavigator nav = doc.CreateNavigator().SelectSingleNode("package");
                this.ReadManifest(nav);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Packages files are deleted when uninstalling the
        /// package.
        /// </summary>
        /// <value>A Boolean value.</value>
        /// -----------------------------------------------------------------------------
        public bool DeleteFiles { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Package is Valid.
        /// </summary>
        /// <value>A Boolean value.</value>
        /// -----------------------------------------------------------------------------
        public bool IsValid { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method commits the package installation.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            for (int index = 0; index <= this._componentInstallers.Count - 1; index++)
            {
                ComponentInstallerBase compInstaller = this._componentInstallers.Values[index];
                if (compInstaller.Version >= this.Package.InstalledVersion && compInstaller.Completed)
                {
                    compInstaller.Commit();
                }
            }

            // Add Event Message
            if (this._eventMessage != null && !string.IsNullOrEmpty(this._eventMessage.Attributes["UpgradeVersionsList"]))
            {
                this._eventMessage.Attributes.Set("desktopModuleID", Null.NullInteger.ToString());
                EventQueueController.SendMessage(this._eventMessage, "Application_Start");
            }

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.INSTALL_Committed);
            }
            else
            {
                this.Log.AddFailure(Util.INSTALL_Aborted);
            }

            this.Package.InstallerInfo.PackageID = this.Package.PackageID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the components of the package.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            bool isCompleted = true;
            try
            {
                // Save the Package Information
                if (this._installedPackage != null)
                {
                    this.Package.PackageID = this._installedPackage.PackageID;
                }

                // Save Package
                PackageController.Instance.SaveExtensionPackage(this.Package);

                // Iterate through all the Components
                for (int index = 0; index <= this._componentInstallers.Count - 1; index++)
                {
                    ComponentInstallerBase compInstaller = this._componentInstallers.Values[index];
                    if ((this._installedPackage == null) || (compInstaller.Version > this.Package.InstalledVersion) || this.Package.InstallerInfo.RepairInstall)
                    {
                        this.Log.AddInfo(Util.INSTALL_Start + " - " + compInstaller.Type);
                        compInstaller.Install();
                        if (compInstaller.Completed)
                        {
                            if (compInstaller.Skipped)
                            {
                                this.Log.AddInfo(Util.COMPONENT_Skipped + " - " + compInstaller.Type);
                            }
                            else
                            {
                                this.Log.AddInfo(Util.COMPONENT_Installed + " - " + compInstaller.Type);
                            }
                        }
                        else
                        {
                            this.Log.AddFailure(Util.INSTALL_Failed + " - " + compInstaller.Type);
                            isCompleted = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.INSTALL_Aborted + " - " + this.Package.Name + ":" + ex.Message);
            }

            if (isCompleted)
            {
                // All components successfully installed so Commit any pending changes
                this.Commit();
            }
            else
            {
                // There has been a failure so Rollback
                this.Rollback();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file and parses it into components.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            // Get Name Property
            this.Package.Name = Util.ReadAttribute(manifestNav, "name", this.Log, Util.EXCEPTION_NameMissing);

            // Get Type
            this.Package.PackageType = Util.ReadAttribute(manifestNav, "type", this.Log, Util.EXCEPTION_TypeMissing);

            // If Skin or Container then set PortalID
            if (this.Package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase) || this.Package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase))
            {
                this.Package.PortalID = this.Package.InstallerInfo.PortalID;
            }

            this.CheckSecurity();
            if (!this.IsValid)
            {
                return;
            }

            // Get IsSystem
            this.Package.IsSystemPackage = bool.Parse(Util.ReadAttribute(manifestNav, "isSystem", false, this.Log, string.Empty, bool.FalseString));

            // Get Version
            string strVersion = Util.ReadAttribute(manifestNav, "version", this.Log, Util.EXCEPTION_VersionMissing);
            if (string.IsNullOrEmpty(strVersion))
            {
                this.IsValid = false;
            }

            if (this.IsValid)
            {
                this.Package.Version = new Version(strVersion);
            }
            else
            {
                return;
            }

            // Attempt to get the Package from the Data Store (see if its installed)
            var packageType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));

            if (packageType.SupportsSideBySideInstallation)
            {
                this._installedPackage = PackageController.Instance.GetExtensionPackage(this.Package.PortalID, p => p.Name.Equals(this.Package.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                            && p.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase)
                                                                                                            && p.Version == this.Package.Version);
            }
            else
            {
                this._installedPackage = PackageController.Instance.GetExtensionPackage(this.Package.PortalID, p => p.Name.Equals(this.Package.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                            && p.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
            }

            if (this._installedPackage != null)
            {
                this.Package.InstalledVersion = this._installedPackage.Version;
                this.Package.InstallerInfo.PackageID = this._installedPackage.PackageID;

                if (this.Package.InstalledVersion > this.Package.Version)
                {
                    this.Log.AddFailure(Util.INSTALL_Version + " - " + this.Package.InstalledVersion.ToString(3));
                    this.IsValid = false;
                }
                else if (this.Package.InstalledVersion == this.Package.Version)
                {
                    this.Package.InstallerInfo.Installed = true;
                    this.Package.InstallerInfo.PortalID = this._installedPackage.PortalID;
                }
            }

            this.Log.AddInfo(Util.DNN_ReadingPackage + " - " + this.Package.PackageType + " - " + this.Package.Name);
            this.Package.FriendlyName = Util.ReadElement(manifestNav, "friendlyName", this.Package.Name);
            this.Package.Description = Util.ReadElement(manifestNav, "description");

            XPathNavigator foldernameNav = null;
            this.Package.FolderName = string.Empty;
            switch (this.Package.PackageType)
            {
                case "Module":
                    // In Dynamics moduels, a component:type=File can have a basePath pointing to the App_Conde folder. This is not a correct FolderName
                    // To ensure that FolderName is DesktopModules...
                    var folderNameValue = PackageController.GetSpecificFolderName(manifestNav, "components/component/files|components/component/resourceFiles", "basePath", "DesktopModules");
                    if (!string.IsNullOrEmpty(folderNameValue))
                    {
                        this.Package.FolderName = folderNameValue.Replace('\\', '/');
                    }

                    break;
                case "Auth_System":
                    foldernameNav = manifestNav.SelectSingleNode("components/component/files");
                    if (foldernameNav != null)
                    {
                        this.Package.FolderName = Util.ReadElement(foldernameNav, "basePath").Replace('\\', '/');
                    }

                    break;
                case "Container":
                    foldernameNav = manifestNav.SelectSingleNode("components/component/containerFiles");
                    if (foldernameNav != null)
                    {
                        this.Package.FolderName = Globals.glbContainersPath + Util.ReadElement(foldernameNav, "containerName").Replace('\\', '/');
                    }

                    break;
                case "Skin":
                    foldernameNav = manifestNav.SelectSingleNode("components/component/skinFiles");
                    if (foldernameNav != null)
                    {
                        this.Package.FolderName = Globals.glbSkinsPath + Util.ReadElement(foldernameNav, "skinName").Replace('\\', '/');
                    }

                    break;
                default:
                    // copied from "Module" without the extra OR condition
                    folderNameValue = PackageController.GetSpecificFolderName(manifestNav, "components/component/resourceFiles", "basePath", "DesktopModules");
                    if (!string.IsNullOrEmpty(folderNameValue))
                    {
                        this.Package.FolderName = folderNameValue.Replace('\\', '/');
                    }

                    break;
            }

            this._eventMessage = this.ReadEventMessageNode(manifestNav);

            // Get Icon
            XPathNavigator iconFileNav = manifestNav.SelectSingleNode("iconFile");
            if (iconFileNav != null)
            {
                if (iconFileNav.Value != string.Empty)
                {
                    if (iconFileNav.Value.StartsWith("~/"))
                    {
                        this.Package.IconFile = iconFileNav.Value;
                    }
                    else if (iconFileNav.Value.StartsWith("DesktopModules", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Package.IconFile = string.Format("~/{0}", iconFileNav.Value);
                    }
                    else
                    {
                        this.Package.IconFile = (string.IsNullOrEmpty(this.Package.FolderName) ? string.Empty : this.Package.FolderName + "/") + iconFileNav.Value;
                        this.Package.IconFile = (!this.Package.IconFile.StartsWith("~/")) ? "~/" + this.Package.IconFile : this.Package.IconFile;
                    }
                }
            }

            // Get Author
            XPathNavigator authorNav = manifestNav.SelectSingleNode("owner");
            if (authorNav != null)
            {
                this.Package.Owner = Util.ReadElement(authorNav, "name");
                this.Package.Organization = Util.ReadElement(authorNav, "organization");
                this.Package.Url = Util.ReadElement(authorNav, "url");
                this.Package.Email = Util.ReadElement(authorNav, "email");
            }

            // Get License
            XPathNavigator licenseNav = manifestNav.SelectSingleNode("license");
            if (licenseNav != null)
            {
                string licenseSrc = Util.ReadAttribute(licenseNav, "src");
                if (string.IsNullOrEmpty(licenseSrc))
                {
                    // Load from element
                    this.Package.License = licenseNav.Value;
                }
                else
                {
                    this.Package.License = this.ReadTextFromFile(licenseSrc);
                }
            }

            if (string.IsNullOrEmpty(this.Package.License))
            {
                // Legacy Packages have no license
                this.Package.License = Util.PACKAGE_NoLicense;
            }

            // Get Release Notes
            XPathNavigator relNotesNav = manifestNav.SelectSingleNode("releaseNotes");
            if (relNotesNav != null)
            {
                string relNotesSrc = Util.ReadAttribute(relNotesNav, "src");
                if (string.IsNullOrEmpty(relNotesSrc))
                {
                    // Load from element
                    this.Package.ReleaseNotes = relNotesNav.Value;
                }
                else
                {
                    this.Package.ReleaseNotes = this.ReadTextFromFile(relNotesSrc);
                }
            }

            if (string.IsNullOrEmpty(this.Package.ReleaseNotes))
            {
                // Legacy Packages have no Release Notes
                this.Package.ReleaseNotes = Util.PACKAGE_NoReleaseNotes;
            }

            // Parse the Dependencies
            var packageDependencies = this.Package.Dependencies;
            foreach (XPathNavigator dependencyNav in manifestNav.CreateNavigator().Select("dependencies/dependency"))
            {
                var dependency = DependencyFactory.GetDependency(dependencyNav);
                var packageDependecy = dependency as IManagedPackageDependency;

                if (packageDependecy != null)
                {
                    packageDependencies.Add(packageDependecy.PackageDependency);
                }

                if (!dependency.IsValid)
                {
                    this.Log.AddFailure(dependency.ErrorMessage);
                    return;
                }
            }

            // Read Components
            this.ReadComponents(manifestNav);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method rolls back the package installation.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            for (int index = 0; index <= this._componentInstallers.Count - 1; index++)
            {
                ComponentInstallerBase compInstaller = this._componentInstallers.Values[index];
                if (compInstaller.Version > this.Package.InstalledVersion && compInstaller.Completed)
                {
                    this.Log.AddInfo(Util.COMPONENT_RollingBack + " - " + compInstaller.Type);
                    compInstaller.Rollback();
                    this.Log.AddInfo(Util.COMPONENT_RolledBack + " - " + compInstaller.Type);
                }
            }

            // If Previously Installed Package exists then we need to update the DataStore with this
            if (this._installedPackage == null)
            {
                // No Previously Installed Package - Delete newly added Package
                PackageController.Instance.DeleteExtensionPackage(this.Package);
            }
            else
            {
                // Previously Installed Package - Rollback to Previously Installed
                PackageController.Instance.SaveExtensionPackage(this._installedPackage);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Uninstall method uninstalls the components of the package.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            // Iterate through all the Components
            for (int index = 0; index <= this._componentInstallers.Count - 1; index++)
            {
                ComponentInstallerBase compInstaller = this._componentInstallers.Values[index];
                var fileInstaller = compInstaller as FileInstaller;
                if (fileInstaller != null)
                {
                    fileInstaller.DeleteFiles = this.DeleteFiles;
                }

                this.Log.ResetFlags();
                this.Log.AddInfo(Util.UNINSTALL_StartComp + " - " + compInstaller.Type);
                compInstaller.UnInstall();
                this.Log.AddInfo(Util.COMPONENT_UnInstalled + " - " + compInstaller.Type);
                if (this.Log.Valid)
                {
                    this.Log.AddInfo(Util.UNINSTALL_SuccessComp + " - " + compInstaller.Type);
                }
                else
                {
                    this.Log.AddWarning(Util.UNINSTALL_WarningsComp + " - " + compInstaller.Type);
                }
            }

            // Remove the Package information from the Data Store
            PackageController.Instance.DeleteExtensionPackage(this.Package);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CheckSecurity method checks whether the user has the appropriate security.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void CheckSecurity()
        {
            PackageType type = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
            if (type == null)
            {
                // This package type not registered
                this.Log.Logs.Clear();
                this.Log.AddFailure(Util.SECURITY_NotRegistered + " - " + this.Package.PackageType);
                this.IsValid = false;
            }
            else
            {
                if (type.SecurityAccessLevel > this.Package.InstallerInfo.SecurityAccessLevel)
                {
                    this.Log.Logs.Clear();
                    this.Log.AddFailure(Util.SECURITY_Installer);
                    this.IsValid = false;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadComponents method reads the components node of the manifest file.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void ReadComponents(XPathNavigator manifestNav)
        {
            foreach (XPathNavigator componentNav in manifestNav.CreateNavigator().Select("components/component"))
            {
                // Set default order to next value (ie the same as the size of the collection)
                int order = this._componentInstallers.Count;

                string type = componentNav.GetAttribute("type", string.Empty);
                if (this.InstallMode == InstallMode.Install)
                {
                    string installOrder = componentNav.GetAttribute("installOrder", string.Empty);
                    if (!string.IsNullOrEmpty(installOrder))
                    {
                        order = int.Parse(installOrder);
                    }
                }
                else
                {
                    string unInstallOrder = componentNav.GetAttribute("unInstallOrder", string.Empty);
                    if (!string.IsNullOrEmpty(unInstallOrder))
                    {
                        order = int.Parse(unInstallOrder);
                    }
                }

                if (this.Package.InstallerInfo != null)
                {
                    this.Log.AddInfo(Util.DNN_ReadingComponent + " - " + type);
                }

                ComponentInstallerBase installer = InstallerFactory.GetInstaller(componentNav, this.Package);
                if (installer == null)
                {
                    this.Log.AddFailure(Util.EXCEPTION_InstallerCreate);
                }
                else
                {
                    this._componentInstallers.Add(order, installer);
                    this.Package.InstallerInfo.AllowableFiles += ", " + installer.AllowableFiles;
                }
            }
        }

        private string ReadTextFromFile(string source)
        {
            string strText = Null.NullString;
            if (this.Package.InstallerInfo.InstallMode != InstallMode.ManifestOnly)
            {
                // Load from file
                strText = FileSystemUtils.ReadFile(this.Package.InstallerInfo.TempInstallFolder + "\\" + source);
            }

            return strText;
        }
    }
}
