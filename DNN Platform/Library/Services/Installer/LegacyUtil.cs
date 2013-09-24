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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Installer.Writers;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Services.Installer
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LegacyUtil class is a Utility class that provides helper methods to transfer
    /// legacy packages to Cambrian's Universal Installer based system
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/23/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class LegacyUtil
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (LegacyUtil));
        private static string AdminModules =
            "Adsense, MarketShare, Authentication, Banners, FeedExplorer, FileManager, HostSettings, Lists, LogViewer, Newsletters, PortalAliases, Portals, RecycleBin, Scheduler, SearchAdmin, SearchInput, SearchResults, Security, SiteLog, SiteWizard, SkinDesigner, Solutions, SQL, Tabs, Vendors,";

        private static string CoreModules =
            "DNN_Announcements, Blog, DNN_Documents, DNN_Events, DNN_FAQs, DNN_Feedback, DNN_Forum, Help, DNN_HTML, DNN_IFrame, DNN_Links, DNN_Media, DNN_NewsFeeds, DNN_Reports, Repository, Repository Dashboard, Store Admin, Store Account, Store Catalog, Store Mini Cart, Store Menu, DNN_Survey, DNN_UserDefinedTable, DNN_UsersOnline, Wiki, DNN_XML,";

        private static string KnownSkinObjects =
            "ACTIONBUTTON, ACTIONS, BANNER, BREADCRUMB, COPYRIGHT, CURRENTDATE, DOTNETNUKE, DROPDOWNACTIONS, HELP, HOSTNAME, ICON, LANGUAGE, LINKACTIONS, LINKS, LOGIN, LOGO, MENU, NAV, PRINTMODULE, PRIVACY, SEARCH, SIGNIN, SOLPARTACTIONS, SOLPARTMENU, STYLES, TERMS, TEXT, TITLE, TREEVIEW, USER, VISIBILITY,";

        private static string KnownSkins = "DNN-Blue, DNN-Gray, MinimalExtropy,";

        private static PackageInfo CreateSkinPackage(SkinPackageInfo skin)
        {
			//Create a Package
            var package = new PackageInfo(new InstallerInfo());
            package.Name = skin.SkinName;
            package.FriendlyName = skin.SkinName;
            package.Description = Null.NullString;
            package.Version = new Version(1, 0, 0);
            package.PackageType = skin.SkinType;
            package.License = Util.PACKAGE_NoLicense;

            //See if the Skin is using a Namespace (or is a known skin)
            ParsePackageName(package);

            return package;
        }

        private static void CreateSkinManifest(XmlWriter writer, string skinFolder, string skinType, string tempInstallFolder, string subFolder)
        {
            string skinName = Path.GetFileNameWithoutExtension(skinFolder);
            var skin = new SkinPackageInfo();
            skin.SkinName = skinName;
            skin.SkinType = skinType;

            //Create a Package
            PackageInfo package = CreateSkinPackage(skin);

            //Create a SkinPackageWriter
            var skinWriter = new SkinPackageWriter(skin, package, tempInstallFolder, subFolder);
            skinWriter.GetFiles(false);

            //We need to reset the BasePath so it using the correct basePath rather than the Temp InstallFolder
            skinWriter.SetBasePath();

            //Writer package manifest fragment to writer
            skinWriter.WriteManifest(writer, true);
        }

        private static void ProcessLegacySkin(string skinFolder, string skinType)
        {
            string skinName = Path.GetFileName(skinFolder);
            if (skinName != "_default")
            {
                var skin = new SkinPackageInfo();
                skin.SkinName = skinName;
                skin.SkinType = skinType;

                //Create a Package
                PackageInfo package = CreateSkinPackage(skin);

                //Create a SkinPackageWriter
                var skinWriter = new SkinPackageWriter(skin, package);
                skinWriter.GetFiles(false);

                //Save the manifest
                package.Manifest = skinWriter.WriteManifest(true);

                //Save Package
                PackageController.Instance.SaveExtensionPackage(package);

                //Update Skin Package with new PackageID
                skin.PackageID = package.PackageID;

                //Save Skin Package
                skin.SkinPackageID = SkinController.AddSkinPackage(skin);

                foreach (InstallFile skinFile in skinWriter.Files.Values)
                {
                    if (skinFile.Type == InstallFileType.Ascx)
                    {
                        if (skinType == "Skin")
                        {
                            SkinController.AddSkin(skin.SkinPackageID, Path.Combine("[G]" + SkinController.RootSkin, Path.Combine(skin.SkinName, skinFile.FullName)));
                        }
                        else
                        {
                            SkinController.AddSkin(skin.SkinPackageID, Path.Combine("[G]" + SkinController.RootContainer, Path.Combine(skin.SkinName, skinFile.FullName)));
                        }
                    }
                }
            }
        }

        private static void ParsePackageName(PackageInfo package, string separator)
        {
			//See if the Module is using a "Namespace" for its name
            int ownerIndex = package.Name.IndexOf(separator);
            if (ownerIndex > 0)
            {
                package.Owner = package.Name.Substring(0, ownerIndex);
            }
        }

        public static string CreateSkinManifest(string skinFolder, string skinType, string tempInstallFolder)
        {
            //Test if there are Skins and Containers folders in TempInstallFolder (ie it is a legacy combi package)
            bool isCombi = false;
            var installFolder = new DirectoryInfo(tempInstallFolder);
            DirectoryInfo[] subFolders = installFolder.GetDirectories();
            if (subFolders.Length > 0)
            {
                if ((subFolders[0].Name.ToLowerInvariant() == "containers" || subFolders[0].Name.ToLowerInvariant() == "skins"))
                {
                    isCombi = true;
                }
            }
			
            //Create a writer to create the processed manifest
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment));
            PackageWriterBase.WriteManifestStartElement(writer);
            if (isCombi)
            {
                if (Directory.Exists(Path.Combine(tempInstallFolder, "Skins")))
                {
					//Add Skin Package Fragment
                    CreateSkinManifest(writer, skinFolder, "Skin", tempInstallFolder.Replace(Globals.ApplicationMapPath + "\\", ""), "Skins");
                }
                if (Directory.Exists(Path.Combine(tempInstallFolder, "Containers")))
                {
					//Add Container PAckage Fragment
                    CreateSkinManifest(writer, skinFolder, "Container", tempInstallFolder.Replace(Globals.ApplicationMapPath + "\\", ""), "Containers");
                }
            }
            else
            {
				//Add Package Fragment
                CreateSkinManifest(writer, skinFolder, skinType, tempInstallFolder.Replace(Globals.ApplicationMapPath + "\\", ""), "");
            }
            PackageWriterBase.WriteManifestEndElement(writer);

            //Close XmlWriter
            writer.Close();

            //Return new manifest
            return sb.ToString();
        }

        public static void ParsePackageName(PackageInfo package)
        {
            ParsePackageName(package, ".");
            if (string.IsNullOrEmpty(package.Owner))
            {
                ParsePackageName(package, "\\");
            }
            if (string.IsNullOrEmpty(package.Owner))
            {
                ParsePackageName(package, "_");
            }
            if (package.PackageType == "Module" && AdminModules.Contains(package.Name + ",") || package.PackageType == "Module" && CoreModules.Contains(package.Name + ",") ||
                (package.PackageType == "Container" || package.PackageType == "Skin") && KnownSkins.Contains(package.Name + ",") ||
                package.PackageType == "SkinObject" && KnownSkinObjects.Contains(package.Name + ","))
            {
                if (string.IsNullOrEmpty(package.Owner))
                {
                    package.Owner = "DotNetNuke";
                    package.Name = "DotNetNuke." + package.Name;
                    switch (package.PackageType)
                    {
                        case "Skin":
                            package.Name += ".Skin";
                            package.FriendlyName += " Skin";
                            break;
                        case "Container":
                            package.Name += ".Container";
                            package.FriendlyName += " Container";
                            break;
                        case "SkinObject":
                            package.Name += "SkinObject";
                            package.FriendlyName += " SkinObject";
                            break;
                    }
                }
            }
            if (package.Owner == "DotNetNuke")
            {
                package.License = Localization.Localization.GetString("License", Localization.Localization.GlobalResourceFile);
                package.Organization = "DotNetNuke Corporation";
                package.Url = "www.dotnetnuke.com";
                package.Email = "support@dotnetnuke.com";
                package.ReleaseNotes = "There are no release notes for this version.";
            }
            else
            {
                package.License = Util.PACKAGE_NoLicense;
            }
        }

        public static void ProcessLegacyLanguages()
        {
            string filePath = Globals.ApplicationMapPath + Localization.Localization.SupportedLocalesFile.Substring(1).Replace("/", "\\");
            if (File.Exists(filePath))
            {
                var doc = new XPathDocument(filePath);

                //Check for Browser and Url settings
                XPathNavigator browserNav = doc.CreateNavigator().SelectSingleNode("root/browserDetection");
                if (browserNav != null)
                {
                    HostController.Instance.Update("EnableBrowserLanguage", Util.ReadAttribute(browserNav, "enabled", false, null, Null.NullString, "true"));
                }
                XPathNavigator urlNav = doc.CreateNavigator().SelectSingleNode("root/languageInUrl");
                if (urlNav != null)
                {
                    HostController.Instance.Update("EnableUrlLanguage", Util.ReadAttribute(urlNav, "enabled", false, null, Null.NullString, "true"));
                }
				
                //Process each language
                foreach (XPathNavigator nav in doc.CreateNavigator().Select("root/language"))
                {
                    if (nav.NodeType != XPathNodeType.Comment)
                    {
                        var language = new Locale();
                        language.Text = Util.ReadAttribute(nav, "name");
                        language.Code = Util.ReadAttribute(nav, "key");
                        language.Fallback = Util.ReadAttribute(nav, "fallback");
                        //Save Language
                        Localization.Localization.SaveLanguage(language);
                        if (language.Code != Localization.Localization.SystemLocale)
                        {
                            //Create a Package
                            var package = new PackageInfo(new InstallerInfo())
                                {
                                    Name = language.Text,
                                    FriendlyName = language.Text,
                                    Description = Null.NullString,
                                    Version = new Version(1, 0, 0),
                                    PackageType = "CoreLanguagePack",
                                    License = Util.PACKAGE_NoLicense
                                };

                            //Create a LanguagePackWriter
                            var packageWriter = new LanguagePackWriter(language, package);

                            //Save the manifest
                            package.Manifest = packageWriter.WriteManifest(true);

                            //Save Package
                            PackageController.Instance.SaveExtensionPackage(package);

                            var languagePack = new LanguagePackInfo
                                {
                                    LanguageID = language.LanguageId,
                                    PackageID = package.PackageID,
                                    DependentPackageID = -2
                                };
                            LanguagePackController.SaveLanguagePack(languagePack);
                        }
                    }
                }
            }
			
            //Process Portal Locales files
            foreach (PortalInfo portal in new PortalController().GetPortals())
            {
                int portalID = portal.PortalID;
                filePath = string.Format(Globals.ApplicationMapPath + Localization.Localization.ApplicationResourceDirectory.Substring(1).Replace("/", "\\") + "\\Locales.Portal-{0}.xml", portalID);

                if (File.Exists(filePath))
                {
                    var doc = new XPathDocument(filePath);

                    //Check for Browser and Url settings
                    XPathNavigator browserNav = doc.CreateNavigator().SelectSingleNode("locales/browserDetection");
                    if (browserNav != null)
                    {
                        PortalController.UpdatePortalSetting(portalID, "EnableBrowserLanguage", Util.ReadAttribute(browserNav, "enabled", false, null, Null.NullString, "true"));
                    }
                    XPathNavigator urlNav = doc.CreateNavigator().SelectSingleNode("locales/languageInUrl");
                    if (urlNav != null)
                    {
                        PortalController.UpdatePortalSetting(portalID, "EnableUrlLanguage", Util.ReadAttribute(urlNav, "enabled", false, null, Null.NullString, "true"));
                    }
                    foreach (Locale installedLanguage in LocaleController.Instance.GetLocales(Null.NullInteger).Values)
                    {
                        string code = installedLanguage.Code;
                        bool bFound = false;

                        //Check if this language is "inactive"
                        foreach (XPathNavigator inactiveNav in doc.CreateNavigator().Select("locales/inactive/locale"))
                        {
                            if (inactiveNav.Value == code)
                            {
                                bFound = true;
                                break;
                            }
                        }
                        if (!bFound)
                        {
							//Language is enabled - add to portal
                            Localization.Localization.AddLanguageToPortal(portalID, installedLanguage.LanguageId, false);
                        }
                    }
                }
                else
                {
                    foreach (Locale installedLanguage in LocaleController.Instance.GetLocales(Null.NullInteger).Values)
                    {
						//Language is enabled - add to portal
                        Localization.Localization.AddLanguageToPortal(portalID, installedLanguage.LanguageId, false);
                    }
                }
            }
        }

        public static void ProcessLegacyModule(DesktopModuleInfo desktopModule)
        {
            //Get the Module folder
            string moduleFolder = Path.Combine(Globals.ApplicationMapPath, Path.Combine("DesktopModules", desktopModule.FolderName));

            //Find legacy manifest
            XPathNavigator rootNav = null;
            try
            {
                string hostModules = "Portals, SQL, HostSettings, Scheduler, SearchAdmin, Lists, SkinDesigner, Extensions";
                string[] files = Directory.GetFiles(moduleFolder, "*.dnn.config");
                if (files.Length > 0)
                {
                    //Create an XPathDocument from the Xml
                    var doc = new XPathDocument(new FileStream(files[0], FileMode.Open, FileAccess.Read));
                    rootNav = doc.CreateNavigator().SelectSingleNode("dotnetnuke");
                }

                //Module is not affiliated with a Package
                var package = new PackageInfo(new InstallerInfo());
                package.Name = desktopModule.ModuleName;

                package.FriendlyName = desktopModule.FriendlyName;
                package.Description = desktopModule.Description;
                package.Version = new Version(1, 0, 0);
                if (!string.IsNullOrEmpty(desktopModule.Version))
                {
                    package.Version = new Version(desktopModule.Version);
                }
                if (hostModules.Contains(desktopModule.ModuleName))
                {
                    //Host Module so make this a system package
                    package.IsSystemPackage = true;
                    desktopModule.IsAdmin = true;
                }
                else
                {
                    desktopModule.IsAdmin = false;
                }
                package.PackageType = "Module";

                //See if the Module is using a "Namespace" for its name
                ParsePackageName(package);

                if (files.Length > 0)
                {
                    var modulewriter = new ModulePackageWriter(desktopModule, rootNav, package);
                    package.Manifest = modulewriter.WriteManifest(true);
                }
                else
                {
                    package.Manifest = ""; //module has no manifest
                }

                //Save Package
                PackageController.Instance.SaveExtensionPackage(package);

                //Update Desktop Module with new PackageID
                desktopModule.PackageID = package.PackageID;

                //Save DesktopModule
                DesktopModuleController.SaveDesktopModule(desktopModule, false, false);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

            }            
        }

        public static void ProcessLegacyModules()
        {
            foreach (DesktopModuleInfo desktopModule in DesktopModuleController.GetDesktopModules(Null.NullInteger).Values)
            {
                if (desktopModule.PackageID == Null.NullInteger)
                {
                    ProcessLegacyModule(desktopModule);
                }
            }
        }

        public static void ProcessLegacySkinControls()
        {
            foreach (SkinControlInfo skinControl in SkinControlController.GetSkinControls().Values)
            {
                if (skinControl.PackageID == Null.NullInteger)
                {
                    try
                    {
						//SkinControl is not affiliated with a Package
                        var package = new PackageInfo(new InstallerInfo());
                        package.Name = skinControl.ControlKey;

                        package.FriendlyName = skinControl.ControlKey;
                        package.Description = Null.NullString;
                        package.Version = new Version(1, 0, 0);
                        package.PackageType = "SkinObject";

                        //See if the SkinControl is using a "Namespace" for its name
                        ParsePackageName(package);

                        var skinControlWriter = new SkinControlPackageWriter(skinControl, package);
                        package.Manifest = skinControlWriter.WriteManifest(true);

                        //Save Package
                        PackageController.Instance.SaveExtensionPackage(package);

                        //Update SkinControl with new PackageID
                        skinControl.PackageID = package.PackageID;

                        //Save SkinControl
                        SkinControlController.SaveSkinControl(skinControl);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);

                    }
                }
            }
        }

        public static void ProcessLegacySkins()
        {
			//Process Legacy Skins
            string skinRootPath = Path.Combine(Globals.HostMapPath, SkinController.RootSkin);
            foreach (string skinFolder in Directory.GetDirectories(skinRootPath))
            {
                ProcessLegacySkin(skinFolder, "Skin");
            }
			
            //Process Legacy Containers
            skinRootPath = Path.Combine(Globals.HostMapPath, SkinController.RootContainer);
            foreach (string skinFolder in Directory.GetDirectories(skinRootPath))
            {
                ProcessLegacySkin(skinFolder, "Container");
            }
        }
    }
}