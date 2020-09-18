// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LanguagePackWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class LanguagePackWriter : PackageWriterBase
    {
        private bool _IsCore = Null.NullBoolean;
        private Locale _Language;
        private LanguagePackInfo _LanguagePack;

        public LanguagePackWriter(PackageInfo package)
            : base(package)
        {
            this._LanguagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
            if (this.LanguagePack != null)
            {
                this._Language = LocaleController.Instance.GetLocale(this._LanguagePack.LanguageID);
                if (this.LanguagePack.PackageType == LanguagePackType.Core)
                {
                    this.BasePath = Null.NullString;
                }
                else
                {
                    // Get the BasePath of the Dependent Package
                    PackageInfo dependendentPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.LanguagePack.DependentPackageID);
                    PackageWriterBase dependentPackageWriter = PackageWriterFactory.GetWriter(dependendentPackage);
                    this.BasePath = dependentPackageWriter.BasePath;
                }
            }
            else
            {
                this.BasePath = Null.NullString;
            }
        }

        public LanguagePackWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            this._Language = new Locale();
            XPathNavigator cultureNav = manifestNav.SelectSingleNode("Culture");
            this._Language.Text = Util.ReadAttribute(cultureNav, "DisplayName");
            this._Language.Code = Util.ReadAttribute(cultureNav, "Code");
            this._Language.Fallback = Localization.SystemLocale;

            // Create a Package
            this.Package = new PackageInfo(installer);
            this.Package.Name = this.Language.Text;
            this.Package.FriendlyName = this.Language.Text;
            this.Package.Description = Null.NullString;
            this.Package.Version = new Version(1, 0, 0);
            this.Package.License = Util.PACKAGE_NoLicense;

            this.ReadLegacyManifest(manifestNav);

            if (this._IsCore)
            {
                this.Package.PackageType = "CoreLanguagePack";
            }
            else
            {
                this.Package.PackageType = "ExtensionLanguagePack";
            }

            this.BasePath = Null.NullString;
        }

        public LanguagePackWriter(Locale language, PackageInfo package)
            : base(package)
        {
            this._Language = language;
            this.BasePath = Null.NullString;
        }

        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the associated Language.
        /// </summary>
        /// <value>An Locale object.</value>
        /// -----------------------------------------------------------------------------
        public Locale Language
        {
            get
            {
                return this._Language;
            }

            set
            {
                this._Language = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the associated Language Pack.
        /// </summary>
        /// <value>An LanguagePackInfo object.</value>
        /// -----------------------------------------------------------------------------
        public LanguagePackInfo LanguagePack
        {
            get
            {
                return this._LanguagePack;
            }

            set
            {
                this._LanguagePack = value;
            }
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            // Language file starts at the root
            this.ParseFolder(Path.Combine(Globals.ApplicationMapPath, this.BasePath), Globals.ApplicationMapPath);
        }

        protected override void ParseFiles(DirectoryInfo folder, string rootPath)
        {
            if (this.LanguagePack.PackageType == LanguagePackType.Core)
            {
                if ((folder.FullName.ToLowerInvariant().Contains("desktopmodules") && !folder.FullName.ToLowerInvariant().Contains("admin")) || folder.FullName.ToLowerInvariant().Contains("providers"))
                {
                    return;
                }

                if (folder.FullName.ToLowerInvariant().Contains("install") && folder.FullName.ToLowerInvariant().Contains("temp"))
                {
                    return;
                }
            }

            if (folder.Name.ToLowerInvariant() == "app_localresources" || folder.Name.ToLowerInvariant() == "app_globalresources" || folder.Name.ToLowerInvariant() == "_default")
            {
                // Add the Files in the Folder
                FileInfo[] files = folder.GetFiles();
                foreach (FileInfo file in files)
                {
                    string filePath = folder.FullName.Replace(rootPath, string.Empty);
                    if (filePath.StartsWith("\\"))
                    {
                        filePath = filePath.Substring(1);
                    }

                    if (file.Name.ToLowerInvariant().Contains(this.Language.Code.ToLowerInvariant()) || (this.Language.Code.ToLowerInvariant() == "en-us" && !file.Name.Contains("-")))
                    {
                        this.AddFile(Path.Combine(filePath, file.Name));
                    }
                }
            }
        }

        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            LanguageComponentWriter languageFileWriter;
            if (this.LanguagePack == null)
            {
                languageFileWriter = new LanguageComponentWriter(this.Language, this.BasePath, this.Files, this.Package);
            }
            else
            {
                languageFileWriter = new LanguageComponentWriter(this.LanguagePack, this.BasePath, this.Files, this.Package);
            }

            languageFileWriter.WriteManifest(writer);
        }

        private void ReadLegacyManifest(XPathNavigator manifestNav)
        {
            string fileName = Null.NullString;
            string filePath = Null.NullString;
            string sourceFileName = Null.NullString;
            string resourcetype = Null.NullString;
            string moduleName = Null.NullString;
            foreach (XPathNavigator fileNav in manifestNav.Select("Files/File"))
            {
                fileName = Util.ReadAttribute(fileNav, "FileName").ToLowerInvariant();
                resourcetype = Util.ReadAttribute(fileNav, "FileType");
                moduleName = Util.ReadAttribute(fileNav, "ModuleName").ToLowerInvariant();
                sourceFileName = Path.Combine(resourcetype, Path.Combine(moduleName, fileName));
                string extendedExtension = "." + this.Language.Code.ToLowerInvariant() + ".resx";
                switch (resourcetype)
                {
                    case "GlobalResource":
                        filePath = "App_GlobalResources";
                        this._IsCore = true;
                        break;
                    case "ControlResource":
                        filePath = "Controls\\App_LocalResources";
                        break;
                    case "AdminResource":
                        this._IsCore = true;
                        switch (moduleName)
                        {
                            case "authentication":
                                filePath = "DesktopModules\\Admin\\Authentication\\App_LocalResources";
                                break;
                            case "controlpanel":
                                filePath = "Admin\\ControlPanel\\App_LocalResources";
                                break;
                            case "files":
                                filePath = "DesktopModules\\Admin\\FileManager\\App_LocalResources";
                                break;
                            case "host":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "authentication.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "friendlyurls.ascx":
                                        filePath = "DesktopModules\\Admin\\HostSettings\\App_LocalResources";
                                        break;
                                    case "hostsettings.ascx":
                                        filePath = "DesktopModules\\Admin\\HostSettings\\App_LocalResources";
                                        break;
                                    case "requestfilters.ascx":
                                        filePath = "DesktopModules\\Admin\\HostSettings\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "lists":
                                filePath = "DesktopModules\\Admin\\Lists\\App_LocalResources";
                                break;
                            case "localization":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "languageeditor.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "languageeditorext.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "timezoneeditor.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "resourceverifier.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    default:
                                        filePath = string.Empty;
                                        break;
                                }

                                break;
                            case "logging":
                                filePath = "DesktopModules\\Admin\\LogViewer\\App_LocalResources";
                                break;
                            case "moduledefinitions":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "editmodulecontrol.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "importmoduledefinition.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "timezoneeditor.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    default:
                                        filePath = string.Empty;
                                        break;
                                }

                                break;
                            case "modules":
                                filePath = "Admin\\Modules\\App_LocalResources";
                                break;
                            case "packages":
                                filePath = "DesktopModules\\Admin\\Extensions\\App_LocalResources";
                                break;
                            case "portal":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "editportalalias.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "portalalias.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "portals.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "privacy.ascx":
                                        filePath = "Admin\\Portal\\App_LocalResources";
                                        break;
                                    case "signup.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "sitesettings.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "sitewizard.ascx":
                                        filePath = "DesktopModules\\Admin\\SiteWizard\\App_LocalResources";
                                        break;
                                    case "sql.ascx":
                                        filePath = "DesktopModules\\Admin\\SQL\\App_LocalResources";
                                        break;
                                    case "systemmessages.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "template.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "terms.ascx":
                                        filePath = "Admin\\Portal\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "scheduling":
                                filePath = "DesktopModules\\Admin\\Scheduler\\App_LocalResources";
                                break;
                            case "search":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "inputsettings.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchInput\\App_LocalResources";
                                        break;
                                    case "resultssettings.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchResults\\App_LocalResources";
                                        break;
                                    case "searchadmin.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchAdmin\\App_LocalResources";
                                        break;
                                    case "searchinput.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchInput\\App_LocalResources";
                                        break;
                                    case "searchresults.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchResults\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "security":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "accessdenied.ascx":
                                        filePath = "Admin\\Security\\App_LocalResources";
                                        break;
                                    case "authenticationsettings.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "editgroups.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "editroles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "register.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "roles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "securityroles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "sendpassword.ascx":
                                        filePath = "Admin\\Security\\App_LocalResources";
                                        break;
                                    case "signin.ascx":
                                        filePath = string.Empty;
                                        break;
                                }

                                break;
                            case "skins":
                                filePath = "Admin\\Skins\\App_LocalResources";
                                break;
                            case "syndication":
                                filePath = "DesktopModules\\Admin\\FeedExplorer\\App_LocalResources";
                                break;
                            case "tabs":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "export.ascx":
                                        filePath = "Admin\\Tabs\\App_LocalResources";
                                        break;
                                    case "import.ascx":
                                        filePath = "Admin\\Tabs\\App_LocalResources";
                                        break;
                                    case "managetabs.ascx":
                                        filePath = "DesktopModules\\Admin\\Tabs\\App_LocalResources";
                                        break;
                                    case "recyclebin.ascx":
                                        filePath = "DesktopModules\\Admin\\RecycleBin\\App_LocalResources";
                                        break;
                                    case "tabs.ascx":
                                        filePath = "DesktopModules\\Admin\\Tabs\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "users":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "bulkemail.ascx":
                                        filePath = "DesktopModules\\Admin\\Newsletters\\App_LocalResources";
                                        fileName = "Newsletter.ascx" + extendedExtension;
                                        break;
                                    case "editprofiledefinition.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "manageusers.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "memberservices.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "membership.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "password.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "profile.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "profiledefinitions.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "user.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "users.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "usersettings.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "viewprofile.ascx":
                                        filePath = "Admin\\Users\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "vendors":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "adsense.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "editadsense.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "affiliates.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "banneroptions.ascx":
                                        filePath = "DesktopModules\\Admin\\Banners\\App_LocalResources";
                                        break;
                                    case "banners.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "displaybanners.ascx":
                                        filePath = "DesktopModules\\Admin\\Banners\\App_LocalResources";
                                        break;
                                    case "editaffiliate.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "editbanner.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "editvendors.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "vendors.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                }

                                break;
                        }

                        break;
                    case "LocalResource":
                        filePath = Path.Combine("DesktopModules", Path.Combine(moduleName, "App_LocalResources"));

                        // Two assumptions are made here
                        // 1. Core files appear in the package before extension files
                        // 2. Module packages only include one module
                        if (!this._IsCore && this._LanguagePack == null)
                        {
                            // Check if language is installed
                            Locale locale = LocaleController.Instance.GetLocale(this._Language.Code);
                            if (locale == null)
                            {
                                this.LegacyError = "CoreLanguageError";
                            }
                            else
                            {
                                // Attempt to figure out the Extension
                                foreach (KeyValuePair<int, DesktopModuleInfo> kvp in
                                    DesktopModuleController.GetDesktopModules(Null.NullInteger))
                                {
                                    if (kvp.Value.FolderName.ToLowerInvariant() == moduleName)
                                    {
                                        // Found Module - Get Package
                                        var dependentPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == kvp.Value.PackageID);
                                        this.Package.Name += "_" + dependentPackage.Name;
                                        this.Package.FriendlyName += " " + dependentPackage.FriendlyName;
                                        this._LanguagePack = new LanguagePackInfo();
                                        this._LanguagePack.DependentPackageID = dependentPackage.PackageID;
                                        this._LanguagePack.LanguageID = locale.LanguageId;
                                        break;
                                    }
                                }

                                if (this._LanguagePack == null)
                                {
                                    this.LegacyError = "DependencyError";
                                }
                            }
                        }

                        break;
                    case "ProviderResource":
                        filePath = Path.Combine("Providers", Path.Combine(moduleName, "App_LocalResources"));
                        break;
                    case "InstallResource":
                        filePath = "Install\\App_LocalResources";
                        break;
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    this.AddFile(Path.Combine(filePath, fileName), sourceFileName);
                }
            }
        }
    }
}
