// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Services.Upgrade.Internals;
    using Microsoft.Extensions.DependencyInjection;

    public class ExtensionsController
    {
        private const string OwnerUpdateService = "DotNetNuke Update Service";
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalAliasController portalAliasController;
        private readonly IPackageController packageController;
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="ExtensionsController"/> class.</summary>
        public ExtensionsController()
            : this(null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExtensionsController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalAliasController">The portal alias controller.</param>
        /// <param name="packageController">The package controller.</param>
        /// <param name="tabController">The tab controller.</param>
        public ExtensionsController(
            INavigationManager navigationManager,
            IApplicationStatusInfo appStatus,
            IHostSettings hostSettings,
            IHostSettingsService hostSettingsService,
            IPortalController portalController,
            IPortalAliasController portalAliasController,
            IPackageController packageController,
            ITabController tabController)
        {
            this.NavigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.portalAliasController = portalAliasController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasController>();
            this.packageController = packageController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPackageController>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        protected INavigationManager NavigationManager { get; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IDictionary<string, PackageType> GetPackageTypes()
        {
            var installedPackageTypes = new Dictionary<string, PackageType>();
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
                    type = nameof(PackageTypes.AuthSystem);
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\AuthSystem";
                    break;
                case "javascriptlibrary":
                case "javascript_library":
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\JavaScriptLibrary";
                    break;
                case "extensionlanguagepack":
                    type = nameof(PackageTypes.Language);
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\Language";
                    break;
                case "corelanguagepack":
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\Language";
                    return true; // core languages should always marked as have available packages.
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\" + packageType;
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
                        typePackages = this.packageController.GetExtensionPackages(
                            Null.NullInteger, p => "Module".Equals(p.PackageType, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        AddModulesToList(portalId, typePackages);
                    }

                    break;
                case "skin":
                case "container":
                    typePackages = this.packageController.GetExtensionPackages(portalId, p => p.PackageType == packageType).ToList();
                    break;
                default:
                    typePackages = this.packageController.GetExtensionPackages(Null.NullInteger, p => p.PackageType == packageType).ToList();
                    break;
            }

            return typePackages
                .Select(p => new PackageInfoSlimDto(this.hostSettings, this.hostSettingsService, this.portalController, portalId, p))
                .ToList();
        }

        public List<AvailablePackagesDto> GetAvailablePackages(string packageType)
        {
            var packages = new List<AvailablePackagesDto>();
            if (this.HasAvailablePackage(packageType, out var packagePath))
            {
                var validPackages = new Dictionary<string, PackageInfo>();
                var invalidPackages = new List<string>();

                foreach (string file in Directory.GetFiles(packagePath))
                {
                    if (file.ToLower().EndsWith(".zip") || file.ToLower().EndsWith(".resources"))
                    {
                        PackageController.ParsePackage(file, packagePath, validPackages, invalidPackages);
                    }
                }

                if (packageType.Equals("corelanguagepack", StringComparison.OrdinalIgnoreCase))
                {
                    GetAvailableLanguagePacks(validPackages);
                }

                packages.Add(new AvailablePackagesDto
                {
                    PackageType = packageType,
                    ValidPackages = validPackages.Values.Select(p => new PackageInfoSlimDto(this.hostSettings, this.hostSettingsService, this.portalController, Null.NullInteger, p)).ToList(),
                    InvalidPackages = invalidPackages,
                });
            }

            return packages;
        }

        public List<TabInfo> GetPackageUsage(int portalId, int packageId)
        {
            var tabs = BuildData(this.tabController, portalId, packageId);
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
            this.tabController.PopulateBreadCrumbs(ref tab);
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
                    // use the current portal alias for host tabs
                    var alias = t.PortalID == Null.NullInteger || t.PortalID == portalId
                                    ? PortalSettings.Current.PortalAlias
                                    : this.portalAliasController.GetPortalAliasesByPortalId(t.PortalID)
                                                            .OrderBy(pa => pa.IsPrimary ? 0 : 1)
                                                            .First();
                    var url = this.NavigationManager.NavigateURL(t.TabID, new PortalSettings(t.PortalID, alias), string.Empty);
                    returnValue.AppendFormat("<a href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                }

                index = index + 1;
            }

            return returnValue.ToString();
        }

        internal static string IsPackageInUse(PackageInfo packageInfo, int portalId)
        {
            if (packageInfo.PackageID == Null.NullInteger)
            {
                return string.Empty;
            }

            if (packageInfo.PackageType.Equals("MODULE", StringComparison.OrdinalIgnoreCase))
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
            return Upgrade.UpgradeRedirect(version, packageType, packageName, string.Empty);
        }

        internal static string UpgradeIndicator(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, Version version, string packageType, string packageName)
        {
            var url = Upgrade.UpgradeIndicator(hostSettings, hostSettingsService, portalController, version, packageType, packageName, string.Empty, false, false); // last 2 params are unused
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
                    return package.IconFile != "N\\A" && IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultLanguageImage;
                case "provider":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultProviderImage;
                case "widget":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultWidgetImage;
                case "library":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultLibraryImage;
                default:
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultExtensionImage;
            }
        }

        internal static IDictionary<int, PackageInfo> GetPackagesInUse(bool forHost)
        {
            return PackageController.GetModulePackagesInUse(PortalController.Instance.GetCurrentSettings().PortalId, forHost);
        }

        private static void AddModulesToList(int portalId, List<PackageInfo> packages)
        {
            packages.AddRange(from modulePackage in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Module")
                              let desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(modulePackage.PackageID)
                              from portalModule in DesktopModuleController.GetPortalDesktopModulesByPortalID(portalId).Values
                              where desktopModule != null && portalModule.DesktopModuleID == desktopModule.DesktopModuleID
                              select modulePackage);
        }

        private static string FixIconUrl(string url)
        {
            return !string.IsNullOrEmpty(Globals.ApplicationPath)
                ? $"{Globals.ApplicationPath}/{url.TrimStart('~').TrimStart('/')}"
                : url;
        }

        private static IDictionary<int, TabInfo> BuildData(ITabController tabController, int portalId, int packageId)
        {
            var tabsWithModule = tabController.GetTabsByPackageID(portalId, packageId, false);
            var allPortalTabs = tabController.GetTabsByPortal(portalId);
            IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();

            // must get each tab, they parent may not exist
            foreach (var tab in allPortalTabs.Values)
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }

            return tabsInOrder;
        }

        private static void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
            if (tabsWithModule.ContainsKey(currentTab.TabID) && !tabsInOrder.ContainsKey(currentTab.TabID))
            {
                // add current tab
                tabsInOrder.Add(currentTab.TabID, currentTab);

                // add children of current tab
                foreach (var tab in allPortalTabs.WithParentId(currentTab.TabID))
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
                var path = HttpContextSource.Current.Server.MapPath(imagePath);
                return File.Exists(path);
            }
            catch (HttpException)
            {
                return false;
            }
        }

        private static void GetAvailableLanguagePacks(IDictionary<string, PackageInfo> validPackages)
        {
            try
            {
                var myResponseReader = UpdateService.GetLanguageList();
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.Load(myResponseReader);
                var languages = xmlDoc.SelectNodes("available/language");

                if (languages != null)
                {
                    var installedPackages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "CoreLanguagePack");
                    var installedLanguages = installedPackages.Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID)).ToList();
                    foreach (XmlNode language in languages)
                    {
                        string cultureCode = string.Empty;
                        string version = string.Empty;
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
                            if (!Version.TryParse(version, out var ver))
                            {
                                ver = null;
                            }

                            package.Version = ver;

                            if (
                                installedLanguages.Any(
                                    l =>
                                        LocaleController.Instance.GetLocale(l.LanguageID).Code.ToLowerInvariant().Equals(cultureCode.ToLowerInvariant(), StringComparison.Ordinal)
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
                // suppress for now - need to decide what to do when webservice is unreachable
                // throw;
                // same problem happens in InstallWizard.aspx.cs in BindLanguageList method
            }
        }
    }
}
