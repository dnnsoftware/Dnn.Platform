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
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Services.Installer.Packages
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageController class provides the business class for the packages
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PackageController : ServiceLocator<IPackageController, PackageController>, IPackageController
    {
		#region Private Members

        private static readonly DataProvider provider = DataProvider.Instance();

		#endregion

        protected override Func<IPackageController> GetFactory()
        {
            return (() => new PackageController());
        }

        #region Private Methods

        private static void AddLog(PackageInfo package, EventLogController.EventLogType logType)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog(package, 
                        PortalController.GetCurrentPortalSettings(), 
                        UserController.GetCurrentUserInfo().UserID, 
                        "",
                        logType);
            
        }

        private static void AddPackageInternal(PackageInfo package)
        {
            package.PackageID = provider.AddPackage(package.PortalID,
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
                                                UserController.GetCurrentUserInfo().UserID,
                                                package.FolderName,
                                                package.IconFile);

            AddLog(package, EventLogController.EventLogType.PACKAGE_CREATED);

            ClearCache(package.PortalID);
        }

        private static void ClearCache(int portalId)
        {
            DataCache.ClearPackagesCache(portalId);
            
        }

        private static void DeletePackageInternal(PackageInfo package)
        {
            provider.DeletePackage(package.PackageID);
            AddLog(package, EventLogController.EventLogType.PACKAGE_DELETED);

            if (PortalSettings.Current != null)
            {
                ClearCache(PortalSettings.Current.PortalId);
            }
            ClearCache(Null.NullInteger);
            
        }

        private static void UpdatePackageInternal(PackageInfo package)
        {
            provider.UpdatePackage(package.PortalID,
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
                                   UserController.GetCurrentUserInfo().UserID,
                                   package.FolderName,
                                   package.IconFile);

            AddLog(package, EventLogController.EventLogType.PACKAGE_UPDATED);

            ClearCache(package.PortalID);
        }
        #endregion

        #region IPackageController Implementation

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

        public PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate)
        {
            return GetExtensionPackages(portalId).SingleOrDefault(predicate);
        }

        public IList<PackageInfo> GetExtensionPackages(int portalId)
        {
            var cacheKey = string.Format(DataCache.PackagesCacheKey, portalId);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.PackagesCacheTimeout, DataCache.PackagesCachePriority, portalId);
            return CBO.GetCachedObject<List<PackageInfo>>(cacheItemArgs,
                                                            c => CBO.FillCollection<PackageInfo>(provider.GetPackages(portalId)));
        }

        public IList<PackageInfo> GetExtensionPackages(int portalId, Func<PackageInfo, bool> predicate)
        {
            return GetExtensionPackages(portalId).Where(predicate).ToList();
        }

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

        #endregion

        #region Deprecated Methods

        [Obsolete("Deprecated in DNN 7.2, Replaced by SaveExtensionPackage(PackageInfo package)")]
        public static int AddPackage(PackageInfo package, bool includeDetail)
        {
            Instance.SaveExtensionPackage(package);
            return package.PackageID;
        }

        public static bool CanDeletePackage(PackageInfo package, PortalSettings portalSettings)
        {
            bool bCanDelete = true;
            switch (package.PackageType)
            {
                case "Skin":
                case "Container":
                    //Need to get path of skin being deleted so we can call the public CanDeleteSkin function in the SkinController
                    string strRootSkin = package.PackageType == "Skin" ? SkinController.RootSkin : SkinController.RootContainer;
                    SkinPackageInfo _SkinPackageInfo = SkinController.GetSkinByPackageID(package.PackageID);
                    string strFolderPath = Path.Combine(_SkinPackageInfo.PortalID == Null.NullInteger 
                                                            ? Path.Combine(Globals.HostMapPath, strRootSkin) 
                                                            : Path.Combine(portalSettings.HomeDirectoryMapPath, strRootSkin), _SkinPackageInfo.SkinName);

                    bCanDelete = SkinController.CanDeleteSkin(strFolderPath, portalSettings.HomeDirectoryMapPath);
                    break;
                case "Provider":
                    //Check if the provider is the default provider
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
                    bCanDelete = (providerNavigator == null);
                    break;
            }
            return bCanDelete;
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by DeleteExtensionPackage(PackageInfo package)")]
        public static void DeletePackage(PackageInfo package)
        {
            Instance.DeleteExtensionPackage(package);
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by DeleteExtensionPackage(PackageInfo package)")]
        public static void DeletePackage(int packageID)
        {
            Instance.DeleteExtensionPackage(Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageID));
        }

        public static IDictionary<int, PackageInfo> GetModulePackagesInUse(int portalID, bool forHost)
        {
            return CBO.FillDictionary<int, PackageInfo>("PackageID", provider.GetModulePackagesInUse(portalID, forHost));
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackage(int portalId, Func<PackageInfo, bool> predicate)")]
        public static PackageInfo GetPackage(int packageID)
        {
	        return Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageID);
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackage(int portalId, Func<PackageInfo, bool> predicate)")]
        public static PackageInfo GetPackage(int packageID, bool ignoreCache)
        {
            return Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageID);
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackage(int portalId, Func<PackageInfo, bool> predicate)")]
        public static PackageInfo GetPackageByName(string name)
        {
            return Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == name);
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackage(int portalId, Func<PackageInfo, bool> predicate)")]
        public static PackageInfo GetPackageByName(int portalId, string name)
        {
            return Instance.GetExtensionPackage(portalId, p => p.Name == name);
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackages(int portalId, Func<PackageInfo, bool> predicate)")]
        public static List<PackageInfo> GetPackages()
        {
            return Instance.GetExtensionPackages(Null.NullInteger).ToList();
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackages(int portalId, Func<PackageInfo, bool> predicate)")]
        public static List<PackageInfo> GetPackages(int portalId)
        {
            return Instance.GetExtensionPackages(portalId).ToList();
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackages(int portalId, Func<PackageInfo, bool> predicate)")]
        public static List<PackageInfo> GetPackagesByType(string type)
        {
            return Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == type).ToList();
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by GetPackages(int portalId, Func<PackageInfo, bool> predicate)")]
        public static List<PackageInfo> GetPackagesByType(int portalId, string type)
        {
            return Instance.GetExtensionPackages(portalId, p => p.PackageType == type).ToList();
        }

        public static PackageType GetPackageType(string type)
        {
            return CBO.FillObject<PackageType>(provider.GetPackageType(type));
        }

        public static List<PackageType> GetPackageTypes()
        {
            return CBO.FillCollection<PackageType>(provider.GetPackageTypes());
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by SaveExtensionPackage(PackageInfo package)")]
        public static void SavePackage(PackageInfo package)
        {
            Instance.SaveExtensionPackage(package);
        }

        [Obsolete("Deprecated in DNN 7.2, Replaced by SaveExtensionPackage(PackageInfo package)")]
        public static void UpdatePackage(PackageInfo package)
        {
            Instance.SaveExtensionPackage(package);
        }
		
		#endregion

    }
}
