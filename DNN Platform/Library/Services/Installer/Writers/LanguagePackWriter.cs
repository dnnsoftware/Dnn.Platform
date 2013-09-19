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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LanguagePackWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/30/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class LanguagePackWriter : PackageWriterBase
    {
		#region "Private Members"

        private bool _IsCore = Null.NullBoolean;
        private Locale _Language;
        private LanguagePackInfo _LanguagePack;
		
		#endregion

		#region "Constructors"


        public LanguagePackWriter(PackageInfo package) : base(package)
        {
            _LanguagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
            if (LanguagePack != null)
            {
                _Language = LocaleController.Instance.GetLocale(_LanguagePack.LanguageID);
                if (LanguagePack.PackageType == LanguagePackType.Core)
                {
                    BasePath = Null.NullString;
                }
                else
                {
					//Get the BasePath of the Dependent Package
                    PackageInfo dependendentPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == LanguagePack.DependentPackageID);
                    PackageWriterBase dependentPackageWriter = PackageWriterFactory.GetWriter(dependendentPackage);
                    BasePath = dependentPackageWriter.BasePath;
                }
            }
            else
            {
                BasePath = Null.NullString;
            }
        }

        public LanguagePackWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            _Language = new Locale();
            XPathNavigator cultureNav = manifestNav.SelectSingleNode("Culture");
            _Language.Text = Util.ReadAttribute(cultureNav, "DisplayName");
            _Language.Code = Util.ReadAttribute(cultureNav, "Code");
            _Language.Fallback = Localization.Localization.SystemLocale;

            //Create a Package
            Package = new PackageInfo(installer);
            Package.Name = Language.Text;
            Package.FriendlyName = Language.Text;
            Package.Description = Null.NullString;
            Package.Version = new Version(1, 0, 0);
            Package.License = Util.PACKAGE_NoLicense;

            ReadLegacyManifest(manifestNav);

            if (_IsCore)
            {
                Package.PackageType = "CoreLanguagePack";
            }
            else
            {
                Package.PackageType = "ExtensionLanguagePack";
            }
            BasePath = Null.NullString;
        }

        public LanguagePackWriter(Locale language, PackageInfo package) : base(package)
        {
            _Language = language;
            BasePath = Null.NullString;
        }
		
		#endregion

		#region "Public Properties"

        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Language
        /// </summary>
        /// <value>An Locale object</value>
        /// <history>
        /// 	[cnurse]	01/30/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Locale Language
        {
            get
            {
                return _Language;
            }
            set
            {
                _Language = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Language Pack
        /// </summary>
        /// <value>An LanguagePackInfo object</value>
        /// <history>
        /// 	[cnurse]	05/28/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public LanguagePackInfo LanguagePack
        {
            get
            {
                return _LanguagePack;
            }
            set
            {
                _LanguagePack = value;
            }
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
                string extendedExtension = "." + Language.Code.ToLowerInvariant() + ".resx";
                switch (resourcetype)
                {
                    case "GlobalResource":
                        filePath = "App_GlobalResources";
                        _IsCore = true;
                        break;
                    case "ControlResource":
                        filePath = "Controls\\App_LocalResources";
                        break;
                    case "AdminResource":
                        _IsCore = true;
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
                                switch (fileName.Replace(extendedExtension, ""))
                                {
                                    case "authentication.ascx":
                                        filePath = "";
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
                                    case "solutions.ascx":
                                        filePath = "DesktopModules\\Admin\\Solutions\\App_LocalResources";
                                        break;
                                }
                                break;
                            case "lists":
                                filePath = "DesktopModules\\Admin\\Lists\\App_LocalResources";
                                break;
                            case "localization":
                                switch (fileName.Replace(extendedExtension, ""))
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
                                        filePath = "";
                                        break;
                                }
                                break;
                            case "log":
                                filePath = "DesktopModules\\Admin\\SiteLog\\App_LocalResources";
                                break;
                            case "logging":
                                filePath = "DesktopModules\\Admin\\LogViewer\\App_LocalResources";
                                break;
                            case "moduledefinitions":
                                switch (fileName.Replace(extendedExtension, ""))
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
                                        filePath = "";
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
                                switch (fileName.Replace(extendedExtension, ""))
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
                                        filePath = "";
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
                                switch (fileName.Replace(extendedExtension, ""))
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
                                switch (fileName.Replace(extendedExtension, ""))
                                {
                                    case "accessdenied.ascx":
                                        filePath = "Admin\\Security\\App_LocalResources";
                                        break;
                                    case "authenticationsettings.ascx":
                                        filePath = "";
                                        break;
                                    case "editgroups.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "editroles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "register.ascx":
                                        filePath = "";
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
                                        filePath = "";
                                        break;
                                }
                                break;
                            case "skins":
                                switch (fileName.Replace(extendedExtension, ""))
                                {
                                    case "attributes.ascx":
                                        filePath = "DesktopModules\\Admin\\SkinDesigner\\App_LocalResources";
                                        break;
                                    case "editskins.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    default:
                                        filePath = "Admin\\Skins\\App_LocalResources";
                                        break;
                                }
                                break;
                            case "syndication":
                                filePath = "DesktopModules\\Admin\\FeedExplorer\\App_LocalResources";
                                break;
                            case "tabs":
                                switch (fileName.Replace(extendedExtension, ""))
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
                                switch (fileName.Replace(extendedExtension, ""))
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
                                switch (fileName.Replace(extendedExtension, ""))
                                {
                                    case "adsense.ascx":
                                        filePath = "";
                                        break;
                                    case "editadsense.ascx":
                                        filePath = "";
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
                        //Two assumptions are made here
                        //1. Core files appear in the package before extension files
                        //2. Module packages only include one module
                        if (!_IsCore && _LanguagePack == null)
                        {
							//Check if language is installed
                            Locale locale = LocaleController.Instance.GetLocale(_Language.Code);
                            if (locale == null)
                            {
                                LegacyError = "CoreLanguageError";
                            }
                            else
                            {
								//Attempt to figure out the Extension
                                foreach (KeyValuePair<int, DesktopModuleInfo> kvp in
                                    DesktopModuleController.GetDesktopModules(Null.NullInteger))
                                {
                                    if (kvp.Value.FolderName.ToLowerInvariant() == moduleName)
                                    {
										//Found Module - Get Package
                                        var dependentPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == kvp.Value.PackageID);
                                        Package.Name += "_" + dependentPackage.Name;
                                        Package.FriendlyName += " " + dependentPackage.FriendlyName;
                                        _LanguagePack = new LanguagePackInfo();
                                        _LanguagePack.DependentPackageID = dependentPackage.PackageID;
                                        _LanguagePack.LanguageID = locale.LanguageId;
                                        break;
                                    }
                                }
                                if (_LanguagePack == null)
                                {
                                    LegacyError = "DependencyError";
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
                    AddFile(Path.Combine(filePath, fileName), sourceFileName);
                }
            }
        }
		
		#endregion

		#region "Protected methods"

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			//Language file starts at the root
            ParseFolder(Path.Combine(Globals.ApplicationMapPath, BasePath), Globals.ApplicationMapPath);
        }

        protected override void ParseFiles(DirectoryInfo folder, string rootPath)
        {
            if (LanguagePack.PackageType == LanguagePackType.Core)
            {
                if (folder.FullName.ToLowerInvariant().Contains("desktopmodules") && !folder.FullName.ToLowerInvariant().Contains("admin") || folder.FullName.ToLowerInvariant().Contains("providers"))
                {
                    return;
                }
                if (folder.FullName.ToLowerInvariant().Contains("install") && folder.FullName.ToLowerInvariant().Contains("temp"))
                {
                    return;
                }
            }
            if (folder.Name.ToLowerInvariant() == "app_localresources" || folder.Name.ToLowerInvariant() == "app_globalresources" || folder.Name.ToLowerInvariant() =="_default")
            {
				//Add the Files in the Folder
                FileInfo[] files = folder.GetFiles();
                foreach (FileInfo file in files)
                {
                    string filePath = folder.FullName.Replace(rootPath, "");
                    if (filePath.StartsWith("\\"))
                    {
                        filePath = filePath.Substring(1);
                    }
                    if (file.Name.ToLowerInvariant().Contains(Language.Code.ToLowerInvariant()) || (Language.Code.ToLowerInvariant() == "en-us" && !file.Name.Contains("-")))
                    {
                        AddFile(Path.Combine(filePath, file.Name));
                    }
                }
            }
        }

        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            LanguageComponentWriter languageFileWriter;
            if (LanguagePack == null)
            {
                languageFileWriter = new LanguageComponentWriter(Language, BasePath, Files, Package);
            }
            else
            {
                languageFileWriter = new LanguageComponentWriter(LanguagePack, BasePath, Files, Package);
            }
            languageFileWriter.WriteManifest(writer);
        }
		#endregion
    }
}
