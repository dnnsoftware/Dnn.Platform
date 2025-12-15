// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Dependencies;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;

    /// <summary>The PackageController class provides the business class for the packages.</summary>
    public partial class PackageController : ServiceLocator<IPackageController, PackageController>, IPackageController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        public static bool CanDeletePackage(PackageInfo package, PortalSettings portalSettings)
        {
            bool bCanDelete = true;

            var dependencies = Instance.GetPackageDependencies(d => d.PackageName.Equals(package.Name, StringComparison.OrdinalIgnoreCase) && d.Version <= package.Version);
            if (dependencies.Count > 0)
            {
                // There is at least one package dependent on this package.
                foreach (var dependency in dependencies)
                {
                    var dep = dependency;

                    // Check if there is an alternative package
                    var packages = Instance.GetExtensionPackages(
                        package.PortalID,
                        p => p.Name.Equals(dep.PackageName, StringComparison.OrdinalIgnoreCase)
                                                                        && p.Version >= dep.Version
                                                                        && p.PackageID != package.PackageID);
                    if (packages.Count == 0)
                    {
                        bCanDelete = false;
                    }
                }
            }

            if (bCanDelete)
            {
                switch (package.PackageType)
                {
                    case "Skin":
                    case "Container":
                        // Need to get path of skin being deleted so we can call the public CanDeleteSkin function in the SkinController
                        string strRootSkin = package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase) ? SkinController.RootSkin : SkinController.RootContainer;
                        SkinPackageInfo skinPackageInfo = SkinController.GetSkinByPackageID(package.PackageID);
                        string strFolderPath = Path.Combine(skinPackageInfo.PortalID == Null.NullInteger ? Path.Combine(Globals.HostMapPath, strRootSkin) : Path.Combine(portalSettings.HomeSystemDirectoryMapPath, strRootSkin), skinPackageInfo.SkinName);

                        bCanDelete = SkinController.CanDeleteSkin(strFolderPath, portalSettings.HomeSystemDirectoryMapPath);
                        if (skinPackageInfo.PortalID != Null.NullInteger)
                        {
                            // To be compliant with all versions
                            strFolderPath = Path.Combine(Path.Combine(portalSettings.HomeDirectoryMapPath, strRootSkin), skinPackageInfo.SkinName);
                            bCanDelete = bCanDelete && SkinController.CanDeleteSkin(strFolderPath, portalSettings.HomeDirectoryMapPath);
                        }

                        break;
                    case "Provider":
                        // Check if the provider is the default provider
                        XmlDocument configDoc = Config.Load();
                        string providerName = package.Name;
                        if (providerName.IndexOf(".", StringComparison.Ordinal) > Null.NullInteger)
                        {
                            providerName = providerName.Substring(providerName.IndexOf(".", StringComparison.Ordinal) + 1);
                        }

                        switch (providerName)
                        {
                            case "SchedulingProvider":
                                providerName = "DNNScheduler";
                                break;
                            case "SearchIndexProvider":
                                providerName = "ModuleIndexProvider";
                                break;
                            case "SearchProvider":
                                providerName = "SearchDataStoreProvider";
                                break;
                        }

                        XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*[@defaultProvider='" + providerName + "']");
                        bCanDelete = providerNavigator == null;
                        break;
                }
            }

            return bCanDelete;
        }

        public static IDictionary<int, PackageInfo> GetModulePackagesInUse(int portalID, bool forHost)
        {
            return CBO.FillDictionary<int, PackageInfo>("PackageID", Provider.GetModulePackagesInUse(portalID, forHost));
        }

        public static void ParsePackage(string file, string installPath, Dictionary<string, PackageInfo> packages, List<string> invalidPackages)
        {
            var unzip = new ZipArchive(new FileStream(file, FileMode.Open, FileAccess.Read));
            try
            {
                foreach (var entry in unzip.FileEntries())
                {
                    var fileName = entry.FullName;
                    string extension = Path.GetExtension(fileName);
                    if (extension != null && (extension.Equals(".dnn", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".dnn5", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        // Manifest
                        var manifest = entry.ReadTextFile();

                        var package = new PackageInfo { Manifest = manifest };
                        if (!string.IsNullOrEmpty(manifest))
                        {
                            var doc = new XPathDocument(new StringReader(manifest));
                            XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode("dotnetnuke");
                            string packageType = string.Empty;
                            if (rootNav.Name == "dotnetnuke")
                            {
                                packageType = XmlUtils.GetAttributeValue(rootNav, "type");
                            }
                            else if (rootNav.Name.Equals("languagepack", StringComparison.InvariantCultureIgnoreCase))
                            {
                                packageType = "LanguagePack";
                            }

                            XPathNavigator nav = null;
                            switch (packageType.ToLowerInvariant())
                            {
                                case "package":
                                    nav = rootNav.SelectSingleNode("packages/package");
                                    break;
                                case "module":
                                case "languagepack":
                                case "skinobject":
                                    nav = Installer.ConvertLegacyNavigator(rootNav, new InstallerInfo()).SelectSingleNode("packages/package");
                                    break;
                            }

                            if (nav != null)
                            {
                                package.Name = XmlUtils.GetAttributeValue(nav, "name");
                                package.PackageType = XmlUtils.GetAttributeValue(nav, "type");
                                package.IsSystemPackage = XmlUtils.GetAttributeValueAsBoolean(nav, "isSystem", false);
                                package.Version = new Version(XmlUtils.GetAttributeValue(nav, "version"));
                                package.FriendlyName = XmlUtils.GetNodeValue(nav, "friendlyName");
                                if (string.IsNullOrEmpty(package.FriendlyName))
                                {
                                    package.FriendlyName = package.Name;
                                }

                                package.Description = XmlUtils.GetNodeValue(nav, "description");
                                package.FileName = file.Replace(installPath + "\\", string.Empty);

                                XPathNavigator foldernameNav;
                                switch (package.PackageType)
                                {
                                    case "Module":
                                        // In Dynamics moduels, a component:type=File can have a basePath pointing to the App_Conde folder. This is not a correct FolderName
                                        // To ensure that FolderName is DesktopModules...
                                        var folderNameValue = GetSpecificFolderName(nav, "components/component/files|components/component/resourceFiles", "basePath", "DesktopModules");
                                        if (!string.IsNullOrEmpty(folderNameValue))
                                        {
                                            package.FolderName = folderNameValue.Replace('\\', '/');
                                        }

                                        break;
                                    case "Auth_System":
                                        foldernameNav = nav.SelectSingleNode("components/component/files");
                                        if (foldernameNav != null)
                                        {
                                            package.FolderName = Util.ReadElement(foldernameNav, "basePath").Replace('\\', '/');
                                        }

                                        break;
                                    case "Container":
                                        foldernameNav = nav.SelectSingleNode("components/component/containerFiles");
                                        if (foldernameNav != null)
                                        {
                                            package.FolderName = Globals.glbContainersPath + Util.ReadElement(foldernameNav, "containerName").Replace('\\', '/');
                                        }

                                        break;
                                    case "Skin":
                                        foldernameNav = nav.SelectSingleNode("components/component/skinFiles");
                                        if (foldernameNav != null)
                                        {
                                            package.FolderName = Globals.glbSkinsPath + Util.ReadElement(foldernameNav, "skinName").Replace('\\', '/');
                                        }

                                        break;
                                    default:
                                        break;
                                }

                                XPathNavigator iconFileNav = nav.SelectSingleNode("iconFile");
                                if (package.FolderName != string.Empty && iconFileNav != null)
                                {
                                    if ((iconFileNav.Value != string.Empty) && (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Auth_System", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        if (iconFileNav.Value.StartsWith("~/"))
                                        {
                                            package.IconFile = iconFileNav.Value;
                                        }
                                        else if (iconFileNav.Value.StartsWith("DesktopModules", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            package.IconFile = string.Format("~/{0}", iconFileNav.Value);
                                        }
                                        else
                                        {
                                            package.IconFile = (string.IsNullOrEmpty(package.FolderName) ? string.Empty : package.FolderName + "/") + iconFileNav.Value;
                                            package.IconFile = (!package.IconFile.StartsWith("~/")) ? "~/" + package.IconFile : package.IconFile;
                                        }
                                    }
                                }

                                // Parse the Dependencies
                                foreach (XPathNavigator dependencyNav in nav.CreateNavigator().Select("dependencies/dependency"))
                                {
                                    var dependency = DependencyFactory.GetDependency(dependencyNav);
                                    var packageDependecy = dependency as IManagedPackageDependency;

                                    if (packageDependecy != null)
                                    {
                                        package.Dependencies.Add(packageDependecy.PackageDependency);
                                    }
                                }

                                packages.Add(file, package);
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception)
            {
                invalidPackages.Add(file);
            }
            finally
            {
                unzip.Dispose();
            }
        }

        /// <inheritdoc/>
        public void DeleteExtensionPackage(PackageInfo package)
        {
            switch (package.PackageType)
            {
                case "Auth_System":
                    AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
                    if (authSystem != null)
                    {
                        AuthenticationController.DeleteAuthentication(authSystem);
                    }

                    break;
                case "CoreLanguagePack":
                    LanguagePackInfo languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
                    if (languagePack != null)
                    {
                        LanguagePackController.DeleteLanguagePack(languagePack);
                    }

                    break;
                case "Module":
                    var controller = new DesktopModuleController();
                    DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);
                    if (desktopModule != null)
                    {
                        controller.DeleteDesktopModule(desktopModule);
                    }

                    break;
                case "SkinObject":
                    SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
                    if (skinControl != null)
                    {
                        SkinControlController.DeleteSkinControl(skinControl);
                    }

                    break;
            }

            DeletePackageInternal(package);
        }

        /// <inheritdoc/>
        public PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate)
        {
            return this.GetExtensionPackage(portalId, predicate, false);
        }

        /// <inheritdoc/>
        public PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate, bool useCopy)
        {
            var package = this.GetExtensionPackages(portalId).FirstOrDefault(predicate);

            if (package != null && useCopy)
            {
                return package.Clone();
            }

            return package;
        }

        /// <inheritdoc/>
        public IList<PackageInfo> GetExtensionPackages(int portalId)
        {
            var cacheKey = string.Format(DataCache.PackagesCacheKey, portalId);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.PackagesCacheTimeout, DataCache.PackagesCachePriority, portalId);
            return CBO.GetCachedObject<List<PackageInfo>>(
                cacheItemArgs,
                c => CBO.FillCollection<PackageInfo>(Provider.GetPackages(portalId)));
        }

        /// <inheritdoc/>
        public IList<PackageInfo> GetExtensionPackages(int portalId, Func<PackageInfo, bool> predicate)
        {
            return this.GetExtensionPackages(portalId).Where(predicate).ToList();
        }

        /// <summary>Save or update the package.</summary>
        /// <param name="package">The package.</param>
        public void SaveExtensionPackage(PackageInfo package)
        {
            if (package.PackageID == Null.NullInteger)
            {
                AddPackageInternal(package);
            }
            else
            {
                UpdatePackageInternal(package);
            }
        }

        /// <inheritdoc/>
        public PackageType GetExtensionPackageType(Func<PackageType, bool> predicate)
        {
            return this.GetExtensionPackageTypes().SingleOrDefault(predicate);
        }

        /// <inheritdoc/>
        public IList<PackageType> GetExtensionPackageTypes()
        {
            return CBO.GetCachedObject<List<PackageType>>(
                new CacheItemArgs(
                DataCache.PackageTypesCacheKey,
                DataCache.PackageTypesCacheTimeout,
                DataCache.PackageTypesCachePriority),
                c => CBO.FillCollection<PackageType>(Provider.GetPackageTypes()));
        }

        /// <inheritdoc/>
        public IList<PackageDependencyInfo> GetPackageDependencies(Func<PackageDependencyInfo, bool> predicate)
        {
            return GetPackageDependencies().Where(predicate).ToList();
        }

        internal static string GetSpecificFolderName(XPathNavigator manifestNav, string xpath, string elementName, string startWith)
        {
            string result = string.Empty;
            var foldernameNav = manifestNav.Select(xpath);

            if (foldernameNav != null)
            {
                while (foldernameNav.MoveNext())
                {
                    var elementValue = Util.ReadElement(foldernameNav.Current, elementName);
                    if (!string.IsNullOrEmpty(elementValue) && elementValue.StartsWith(startWith))
                    {
                        result = elementValue;
                        break;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override Func<IPackageController> GetFactory()
        {
            return () => new PackageController();
        }

        private static void AddLog(PackageInfo package, EventLogController.EventLogType logType)
        {
            EventLogController.Instance.AddLog(
                package,
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                logType);
        }

        private static void AddPackageInternal(PackageInfo package)
        {
            package.PackageID = Provider.AddPackage(
                package.PortalID,
                package.Name,
                package.FriendlyName,
                package.Description,
                package.PackageType,
                package.Version.ToString(3),
                package.License,
                package.Manifest,
                package.Owner,
                package.Organization,
                package.Url,
                package.Email,
                package.ReleaseNotes,
                package.IsSystemPackage,
                UserController.Instance.GetCurrentUserInfo().UserID,
                package.FolderName,
                package.IconFile);

            foreach (var dependency in package.Dependencies)
            {
                dependency.PackageId = package.PackageID;
                SavePackageDependency(dependency);
            }

            AddLog(package, EventLogController.EventLogType.PACKAGE_CREATED);

            ClearCache(package.PortalID);
        }

        private static void ClearCache(int portalId)
        {
            DataCache.ClearPackagesCache(portalId);
        }

        private static void ClearDependenciesCache()
        {
            DataCache.RemoveCache(DataCache.PackageDependenciesCacheKey);
        }

        private static void DeletePackageInternal(PackageInfo package)
        {
            Provider.DeletePackage(package.PackageID);
            AddLog(package, EventLogController.EventLogType.PACKAGE_DELETED);

            if (PortalSettings.Current != null)
            {
                ClearCache(PortalSettings.Current.PortalId);
            }

            ClearCache(Null.NullInteger);
        }

        private static List<PackageDependencyInfo> GetPackageDependencies()
        {
            return CBO.GetCachedObject<List<PackageDependencyInfo>>(
                new CacheItemArgs(
                DataCache.PackageDependenciesCacheKey,
                DataCache.PackagesCacheTimeout,
                DataCache.PackagesCachePriority),
                c => CBO.FillCollection<PackageDependencyInfo>(Provider.GetPackageDependencies()));
        }

        private static void UpdatePackageInternal(PackageInfo package)
        {
            Provider.UpdatePackage(
                package.PackageID,
                package.PortalID,
                package.FriendlyName,
                package.Description,
                package.PackageType,
                package.Version.ToString(3),
                package.License,
                package.Manifest,
                package.Owner,
                package.Organization,
                package.Url,
                package.Email,
                package.ReleaseNotes,
                package.IsSystemPackage,
                UserController.Instance.GetCurrentUserInfo().UserID,
                package.FolderName,
                package.IconFile);

            foreach (var dependency in package.Dependencies)
            {
                dependency.PackageId = package.PackageID;
                SavePackageDependency(dependency);
            }

            AddLog(package, EventLogController.EventLogType.PACKAGE_UPDATED);

            ClearCache(package.PortalID);
        }

        private static void SavePackageDependency(PackageDependencyInfo dependency)
        {
            dependency.PackageDependencyId = Provider.SavePackageDependency(
                dependency.PackageDependencyId,
                dependency.PackageId,
                dependency.PackageName,
                dependency.Version.ToString());

            ClearDependenciesCache();
        }
    }
}
