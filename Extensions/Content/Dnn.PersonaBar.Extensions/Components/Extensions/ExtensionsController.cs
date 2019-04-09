#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.Services.Upgrade.Internals;

#endregion

namespace Dnn.PersonaBar.Extensions.Components
{
    public class ExtensionsController
    {
        private const string OwnerUpdateService = "DotNetNuke Update Service";

        public IDictionary<string, PackageType> GetPackageTypes()
        {
            IDictionary<string, PackageType> installedPackageTypes = new Dictionary<string, PackageType>();
            foreach (var packageType in PackageController.Instance.GetExtensionPackageTypes())
            {
                installedPackageTypes[packageType.PackageType] = packageType;
            }
            return installedPackageTypes;
        }

        public bool HasAvailablePackage(string packageType, out string rootPath)
        {
            var type = packageType;
            switch (packageType.ToLowerInvariant())
            {
                case "authsystem":
                case "auth_system":
                    type = PackageTypes.AuthSystem.ToString();
                    rootPath = Globals.ApplicationMapPath + "\\Install\\AuthSystem";
                    break;
                case "javascriptlibrary":
                case "javascript_library":
                    rootPath = Globals.ApplicationMapPath + "\\Install\\JavaScriptLibrary";
                    break;
                case "extensionlanguagepack":
                    type = PackageTypes.Language.ToString();
                    rootPath = Globals.ApplicationMapPath + "\\Install\\Language";
                    break;
                case "corelanguagepack":
                    rootPath = Globals.ApplicationMapPath + "\\Install\\Language";
                    return true; //core languages should always marked as have available packages.
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    rootPath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
                    break;
                default:
                    type = string.Empty;
                    rootPath = string.Empty;
                    break;
            }
            if (!string.IsNullOrEmpty(type) && Directory.Exists(rootPath) &&
                (Directory.GetFiles(rootPath, "*.zip").Length > 0 ||
                    Directory.GetFiles(rootPath, "*.resources").Length > 0))
            {
                return true;
            }

            return false;
        }

        public IList<PackageInfoSlimDto> GetInstalledPackages(int portalId, string packageType)
        {
            var typePackages = new List<PackageInfo>();
            switch (packageType.ToLowerInvariant())
            {
                case "module":
                    if (portalId == Null.NullInteger)
                    {
                        typePackages = PackageController.Instance.GetExtensionPackages(
                            Null.NullInteger, p => "Module".Equals(p.PackageType, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        AddModulesToList(portalId, typePackages);
                    }
                    break;
                case "skin":
                case "container":
                    typePackages = PackageController.Instance.GetExtensionPackages(portalId, p => p.PackageType == packageType).ToList();
                    break;
                default:
                    typePackages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == packageType).ToList();
                    break;
            }

            var typePackageDtos = typePackages.Select(p => new PackageInfoSlimDto(portalId, p));
            return typePackageDtos.ToList();
        }

        public List<AvailablePackagesDto> GetAvailablePackages(string packageType)
        {
            var packages = new List<AvailablePackagesDto>();
            string packagePath;
            if (HasAvailablePackage(packageType, out packagePath))
            {
                var validpackages = new Dictionary<string, PackageInfo>();
                var invalidPackages = new List<string>();

                foreach (string file in Directory.GetFiles(packagePath))
                {
                    if (file.ToLower().EndsWith(".zip") || file.ToLower().EndsWith(".resources"))
                    {
                        PackageController.ParsePackage(file, packagePath, validpackages, invalidPackages);
                    }
                }

                if (packageType.ToLowerInvariant() == "corelanguagepack")
                {
                    GetAvaialableLanguagePacks(validpackages);
                }

                packages.Add(new AvailablePackagesDto()
                {
                    PackageType = packageType,
                    ValidPackages = validpackages.Values.Select(p => new PackageInfoSlimDto(Null.NullInteger, p)).ToList(),
                    InvalidPackages = invalidPackages
                });
            }
            return packages;
        }

        private void GetAvaialableLanguagePacks(IDictionary<string, PackageInfo> validPackages)
        {
            try
            {
                StreamReader myResponseReader = UpdateService.GetLanguageList();
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.Load(myResponseReader);
                XmlNodeList languages = xmlDoc.SelectNodes("available/language");

                if (languages != null)
                {
                    var installedPackages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "CoreLanguagePack");
                    var installedLanguages = installedPackages.Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID)).ToList();
                    foreach (XmlNode language in languages)
                    {
                        string cultureCode = "";
                        string version = "";
                        foreach (XmlNode child in language.ChildNodes)
                        {
                            if (child.Name == "culturecode")
                            {
                                cultureCode = child.InnerText;
                            }

                            if (child.Name == "version")
                            {
                                version = child.InnerText;
                            }
                        }
                        if (!string.IsNullOrEmpty(cultureCode) && !string.IsNullOrEmpty(version) && version.Length == 6)
                        {
                            var myCIintl = new CultureInfo(cultureCode, true);
                            version = version.Insert(4, ".").Insert(2, ".");
                            var package = new PackageInfo { Owner = OwnerUpdateService, Name = "LanguagePack-" + myCIintl.Name, FriendlyName = myCIintl.NativeName };
                            package.Name = myCIintl.NativeName;
                            package.PackageType = "CoreLanguagePack";
                            package.Description = cultureCode;
                            Version ver = null;
                            Version.TryParse(version, out ver);
                            package.Version = ver;

                            if (
                                installedLanguages.Any(
                                    l =>
                                    LocaleController.Instance.GetLocale(l.LanguageID).Code.ToLowerInvariant().Equals(cultureCode.ToLowerInvariant())
                                    && installedPackages.First(p => p.PackageID == l.PackageID).Version >= ver))
                            {
                                continue;
                            }

                            if (validPackages.Values.Any(p => p.Name == package.Name))
                            {
                                var existPackage = validPackages.Values.First(p => p.Name == package.Name);
                                if (package.Version > existPackage.Version)
                                {
                                    existPackage.Version = package.Version;
                                }
                            }
                            else
                            {
                                validPackages.Add(cultureCode, package);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //suppress for now - need to decide what to do when webservice is unreachable
                //throw;
                //same problem happens in InstallWizard.aspx.cs in BindLanguageList method
            }
        }

        public List<TabInfo> GetPackageUsage(int portalId, int packageId)
        {
            IDictionary<int, TabInfo> tabs = BuildData(portalId, packageId);
            if (tabs != null && tabs.Count > 0)
            {
                return tabs.Values.ToList();
            }
            return null;
        }

        public string GetFormattedTabLink(int portalId, TabInfo tab)
        {
            var returnValue = new StringBuilder();

            int index = 0;
            TabController.Instance.PopulateBreadCrumbs(ref tab);
            foreach (TabInfo t in tab.BreadCrumbs)
            {
                if (index > 0)
                {
                    returnValue.Append(" &gt; ");
                }
                if (index < tab.BreadCrumbs.Count - 1)
                {
                    returnValue.AppendFormat("{0}", t.LocalizedTabName);
                }
                else
                {
                    //use the current portal alias for host tabs
                    var alias = t.PortalID == Null.NullInteger || t.PortalID == portalId
                                    ? PortalSettings.Current.PortalAlias
                                    : PortalAliasController.Instance.GetPortalAliasesByPortalId(t.PortalID)
                                                            .OrderBy(pa => pa.IsPrimary ? 0 : 1)
                                                            .First();
                    var url = Globals.NavigateURL(t.TabID, new PortalSettings(t.PortalID, alias), string.Empty);
                    returnValue.AppendFormat("<a href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                }
                index = index + 1;
            }

            return returnValue.ToString();
        }

        #region Private Functions

        private static void AddModulesToList(int portalId, List<PackageInfo> packages)
        {
            Dictionary<int, PortalDesktopModuleInfo> portalModules = DesktopModuleController.GetPortalDesktopModulesByPortalID(portalId);
            packages.AddRange(from modulePackage in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Module")
                              let desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(modulePackage.PackageID)
                              from portalModule in portalModules.Values
                              where desktopModule != null && portalModule.DesktopModuleID == desktopModule.DesktopModuleID
                              select modulePackage);
        }

        internal static string IsPackageInUse(PackageInfo packageInfo, int portalId)
        {
            if (packageInfo.PackageID == Null.NullInteger)
            {
                return string.Empty;
            }

            if ((packageInfo.PackageType.ToUpper() == "MODULE"))
            {
                if (portalId == Null.NullInteger)
                {
                    return GetPackagesInUse(true).ContainsKey(packageInfo.PackageID) ? "Yes" : "No";
                }
                else
                {
                    return GetPackagesInUse(false).ContainsKey(packageInfo.PackageID) ? "Yes" : "No";
                }
            }
            return string.Empty;
        }

        internal static string UpgradeRedirect(Version version, string packageType, string packageName)
        {
            return Upgrade.UpgradeRedirect(version, packageType, packageName, "");
        }

        internal static string UpgradeIndicator(Version version, string packageType, string packageName)
        {
            var url = Upgrade.UpgradeIndicator(version, packageType, packageName, "", false, false); // last 2 params are unused
            if (string.IsNullOrEmpty(url))
            {
                url = Globals.ApplicationPath + "/images/spacer.gif";
            }
            return url;
        }

        internal static string GetPackageIcon(PackageInfo package)
        {
            switch (package.PackageType.ToLowerInvariant())
            {
                case "module":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultExtensionImage;
                case "container":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultContainerImage;
                case "skin":
                case "skinobject":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultSkinImage;
                case "authenticationsystem":
                case "auth_system":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultAuthenicationImage;
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return package.IconFile != "N\\A"  && IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultLanguageImage;
                case "provider":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultProviderImage;
                case "widget":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultWidgetImage;
                case "dashboardcontrol":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultDashboardImage;
                case "library":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultLibraryImage;
                default:
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultExtensionImage;
            }
        }

        private static string FixIconUrl(string url)
        {
            return !string.IsNullOrEmpty(Globals.ApplicationPath)
                ? $"{Globals.ApplicationPath}/{url.TrimStart('~').TrimStart('/')}"
                : url;
        }

        internal static IDictionary<int, PackageInfo> GetPackagesInUse(bool forHost)
        {
            return PackageController.GetModulePackagesInUse(PortalController.Instance.GetCurrentPortalSettings().PortalId, forHost);
        }

        private static IDictionary<int, TabInfo> BuildData(int portalId, int packageId)
        {
            IDictionary<int, TabInfo> tabsWithModule = TabController.Instance.GetTabsByPackageID(portalId, packageId, false);
            TabCollection allPortalTabs = TabController.Instance.GetTabsByPortal(portalId);
            IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();

            //must get each tab, they parent may not exist
            foreach (TabInfo tab in allPortalTabs.Values)
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }
            return tabsInOrder;
        }

        private static void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
            if ((tabsWithModule.ContainsKey(currentTab.TabID) && !tabsInOrder.ContainsKey(currentTab.TabID)))
            {
                //add current tab
                tabsInOrder.Add(currentTab.TabID, currentTab);
                //add children of current tab
                foreach (TabInfo tab in allPortalTabs.WithParentId(currentTab.TabID))
                {
                    AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
                }
            }
        }

        private static bool IconExists(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || HttpContext.Current == null)
            {
                return false;
            }

            try
            {
                var path = HttpContext.Current.Server.MapPath(imagePath);
                return File.Exists(path);
            }
            catch (HttpException)
            {
                return false;
            }

        }

        #endregion
    }
}