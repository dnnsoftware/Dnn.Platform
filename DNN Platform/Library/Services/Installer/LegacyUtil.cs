// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
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
    using Newtonsoft.Json;

    /// <summary>
    /// The LegacyUtil class is a Utility class that provides helper methods to transfer
    /// legacy packages to Cambrian's Universal Installer based system.
    /// </summary>
    public class LegacyUtil
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LegacyUtil));

        private static string adminModules =
            "Adsense, MarketShare, Authentication, Banners, FeedExplorer, FileManager, HostSettings, Lists, LogViewer, Newsletters, PortalAliases, Portals, RecycleBin, Scheduler, SearchAdmin, SearchInput, SearchResults, Security, SiteLog, SiteWizard, SQL, Tabs, Vendors,";

        private static string coreModules =
            "DNN_Announcements, Blog, DNN_Documents, DNN_Events, DNN_FAQs, DNN_Feedback, DNN_Forum, Help, DNN_HTML, DNN_IFrame, DNN_Links, DNN_Media, DNN_NewsFeeds, DNN_Reports, Repository, Repository Dashboard, Store Admin, Store Account, Store Catalog, Store Mini Cart, Store Menu, DNN_Survey, DNN_UserDefinedTable, DNN_UsersOnline, Wiki, DNN_XML,";

        private static string knownSkinObjects =
            "ACTIONBUTTON, ACTIONS, BANNER, BREADCRUMB, COPYRIGHT, CURRENTDATE, DOTNETNUKE, DROPDOWNACTIONS, HELP, HOSTNAME, ICON, LANGUAGE, LINKACTIONS, LINKS, LOGIN, LOGO, MENU, NAV, PRINTMODULE, PRIVACY, SEARCH, SIGNIN, STYLES, TERMS, TEXT, TITLE, TREEVIEW, USER, VISIBILITY,";

        private static string knownSkins = "DNN-Blue, DNN-Gray, MinimalExtropy,";

        public static string CreateSkinManifest(string skinFolder, string skinType, string tempInstallFolder)
        {
            // Test if there are Skins and Containers folders in TempInstallFolder (ie it is a legacy combi package)
            bool isCombi = false;
            var installFolder = new DirectoryInfo(tempInstallFolder);
            DirectoryInfo[] subFolders = installFolder.GetDirectories();
            if (subFolders.Length > 0)
            {
                if (subFolders[0].Name.Equals("containers", StringComparison.OrdinalIgnoreCase) || subFolders[0].Name.Equals("skins", StringComparison.OrdinalIgnoreCase))
                {
                    isCombi = true;
                }
            }

            // Create a writer to create the processed manifest
            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                PackageWriterBase.WriteManifestStartElement(writer);
                if (isCombi)
                {
                    if (Directory.Exists(Path.Combine(tempInstallFolder, "Skins")))
                    {
                        // Add Skin Package Fragment
                        CreateSkinManifest(writer, skinFolder, "Skin", tempInstallFolder.Replace(Globals.ApplicationMapPath + "\\", string.Empty), "Skins");
                    }

                    if (Directory.Exists(Path.Combine(tempInstallFolder, "Containers")))
                    {
                        // Add Container PAckage Fragment
                        CreateSkinManifest(writer, skinFolder, "Container", tempInstallFolder.Replace(Globals.ApplicationMapPath + "\\", string.Empty), "Containers");
                    }
                }
                else
                {
                    // Add Package Fragment
                    CreateSkinManifest(writer, skinFolder, skinType, tempInstallFolder.Replace(Globals.ApplicationMapPath + "\\", string.Empty), string.Empty);
                }

                PackageWriterBase.WriteManifestEndElement(writer);

                // Close XmlWriter
                writer.Close();

                // Return new manifest
                return sb.ToString();
            }
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

            if ((package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) && adminModules.Contains(package.Name + ",")) || (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) && coreModules.Contains(package.Name + ",")) || ((package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)) && knownSkins.Contains(package.Name + ",")) || (package.PackageType.Equals("SkinObject", StringComparison.OrdinalIgnoreCase) && knownSkinObjects.Contains(package.Name + ",")))
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

            if (package.Owner == "DotNetNuke" || package.Owner == "DNN")
            {
                package.License = Localization.GetString("License", Localization.GlobalResourceFile);
                package.Organization = ".NET Foundation";
                package.Url = "https://dnncommunity.org";
                package.Email = "info@dnncommunity.org";
                package.ReleaseNotes = "There are no release notes for this version.";
            }
            else
            {
                package.License = Util.PACKAGE_NoLicense;
            }
        }

        private static PackageInfo CreateSkinPackage(SkinPackageInfo skin)
        {
            // Create a Package
            var package = new PackageInfo(new InstallerInfo());
            package.Name = skin.SkinName;
            package.FriendlyName = skin.SkinName;
            package.Description = Null.NullString;
            package.Version = new Version(1, 0, 0);
            package.PackageType = skin.SkinType;
            package.License = Util.PACKAGE_NoLicense;

            // See if the Skin is using a Namespace (or is a known skin)
            ParsePackageName(package);

            return package;
        }

        private static void CreateSkinManifest(XmlWriter writer, string skinFolder, string skinType, string tempInstallFolder, string subFolder)
        {
            string skinName = Path.GetFileNameWithoutExtension(skinFolder);
            var skin = new SkinPackageInfo();
            skin.SkinName = skinName;
            skin.SkinType = skinType;

            // Create a Package
            PackageInfo package = CreateSkinPackage(skin);

            // Create a SkinPackageWriter
            var skinWriter = new SkinPackageWriter(skin, package, tempInstallFolder, subFolder);
            skinWriter.GetFiles(false);

            // We need to reset the BasePath so it's using the correct basePath rather than the Temp InstallFolder
            skinWriter.SetBasePath();

            // Writer package manifest fragment to writer
            skinWriter.WriteManifest(writer, true);
        }

        private static void ParsePackageName(PackageInfo package, string separator)
        {
            // See if the Module is using a "Namespace" for its name
            int ownerIndex = package.Name.IndexOf(separator);
            if (ownerIndex > 0)
            {
                package.Owner = package.Name.Substring(0, ownerIndex);
            }
        }
    }
}
