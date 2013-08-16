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
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/26/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class PackageController
    {
		#region "Private Members"

        private static readonly DataProvider provider = DataProvider.Instance();

		#endregion

		#region "Public Shared Methods"

        public static int AddPackage(PackageInfo package, bool includeDetail)
        {
            int packageID = provider.AddPackage(package.PortalID,
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
            var objEventLog = new EventLogController();
            objEventLog.AddLog(package, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PACKAGE_CREATED);
            if (includeDetail)
            {
                Locale locale;
                LanguagePackInfo languagePack;
                switch (package.PackageType)
                {
                    case "Auth_System":
                        //Create a new Auth System
                        var authSystem = new AuthenticationInfo();
                        authSystem.AuthenticationType = package.Name;
                        authSystem.IsEnabled = Null.NullBoolean;
                        authSystem.PackageID = packageID;
                        AuthenticationController.AddAuthentication(authSystem);
                        break;
                    case "Container":
                    case "Skin":
                        var skinPackage = new SkinPackageInfo();
                        skinPackage.SkinName = package.Name;
                        skinPackage.PackageID = packageID;
                        skinPackage.SkinType = package.PackageType;
                        SkinController.AddSkinPackage(skinPackage);
                        break;
                    case "CoreLanguagePack":
                        locale = LocaleController.Instance.GetLocale(PortalController.GetCurrentPortalSettings().DefaultLanguage);
                        languagePack = new LanguagePackInfo();
                        languagePack.PackageID = packageID;
                        languagePack.LanguageID = locale.LanguageId;
                        languagePack.DependentPackageID = -2;
                        LanguagePackController.SaveLanguagePack(languagePack);
                        break;
                    case "ExtensionLanguagePack":
                        locale = LocaleController.Instance.GetLocale(PortalController.GetCurrentPortalSettings().DefaultLanguage);
                        languagePack = new LanguagePackInfo();
                        languagePack.PackageID = packageID;
                        languagePack.LanguageID = locale.LanguageId;
                        languagePack.DependentPackageID = Null.NullInteger;
                        LanguagePackController.SaveLanguagePack(languagePack);
                        break;
                    case "Module":
                        //Create a new DesktopModule
                        var desktopModule = new DesktopModuleInfo();
                        desktopModule.PackageID = packageID;
                        desktopModule.ModuleName = package.Name;
                        desktopModule.FriendlyName = package.FriendlyName;
                        desktopModule.FolderName = package.Name;
                        desktopModule.Description = package.Description;
                        desktopModule.Version = package.Version.ToString(3);
                        desktopModule.SupportedFeatures = 0;
                        int desktopModuleId = DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                        if (desktopModuleId > Null.NullInteger)
                        {
                            DesktopModuleController.AddDesktopModuleToPortals(desktopModuleId);
                        }
                        break;
                    case "SkinObject":
                        var skinControl = new SkinControlInfo();
                        skinControl.PackageID = packageID;
                        skinControl.ControlKey = package.Name;
                        SkinControlController.SaveSkinControl(skinControl);
                        break;
                }
            }

			DataCache.ClearPackagesCache(package.PortalID);
            return packageID;
        }

        public static bool CanDeletePackage(PackageInfo package, PortalSettings portalSettings)
        {
            bool bCanDelete = true;
            switch (package.PackageType)
            {
                case "Skin":
                case "Container":
                    //Need to get path of skin being deleted so we can call the public CanDeleteSkin function in the SkinController
                    string strFolderPath = string.Empty;
                    string strRootSkin = package.PackageType == "Skin" ? SkinController.RootSkin : SkinController.RootContainer;
                    SkinPackageInfo _SkinPackageInfo = SkinController.GetSkinByPackageID(package.PackageID);
                    if (_SkinPackageInfo.PortalID == Null.NullInteger)
                    {
                        strFolderPath = Path.Combine(Path.Combine(Globals.HostMapPath, strRootSkin), _SkinPackageInfo.SkinName);
                    }
                    else
                    {
                        strFolderPath = Path.Combine(Path.Combine(portalSettings.HomeDirectoryMapPath, strRootSkin), _SkinPackageInfo.SkinName);
                    }

                    bCanDelete = SkinController.CanDeleteSkin(strFolderPath, portalSettings.HomeDirectoryMapPath);
                    break;
                case "Provider":
                    //Check if the provider is the default provider
                    XmlDocument configDoc = Config.Load();
                    string providerName = package.Name;
                    if (providerName.IndexOf(".") > Null.NullInteger)
                    {
                        providerName = providerName.Substring(providerName.IndexOf(".") + 1);
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

        public static void DeletePackage(PackageInfo package)
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
            DeletePackage(package.PackageID);
        }

        public static void DeletePackage(int packageID)
        {
            provider.DeletePackage(packageID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("packageID",
                               packageID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PACKAGE_DELETED);

	        if (PortalSettings.Current != null)
	        {
		        DataCache.ClearPackagesCache(PortalSettings.Current.PortalId);
	        }
	        DataCache.ClearPackagesCache(Null.NullInteger);
        }

        public static PackageInfo GetPackage(int packageID)
        {
	        return GetPackage(packageID, false);
        }

		public static PackageInfo GetPackage(int packageID, bool ignoreCache)
		{
			if (ignoreCache)
			{
				return CBO.FillObject<PackageInfo>(provider.GetPackage(packageID));
			}

			return GetPackages().FirstOrDefault(p => p.PackageID == packageID);
		}

        public static PackageInfo GetPackageByName(string name)
        {
            return GetPackageByName(Null.NullInteger, name);
        }

        public static PackageInfo GetPackageByName(int portalId, string name)
        {
            //return CBO.FillObject<PackageInfo>(provider.GetPackageByName(portalId, name));
			return GetPackages(portalId).FirstOrDefault(p => p.Name == name);
        }

        public static List<PackageInfo> GetPackages()
        {
            return GetPackages(Null.NullInteger);
        }

        public static List<PackageInfo> GetPackages(int portalID)
        {
	        var cacheKey = string.Format(DataCache.PackagesCacheKey, portalID);
			var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.PackagesCacheTimeout, DataCache.PackagesCachePriority, portalID);
	        return CBO.GetCachedObject<List<PackageInfo>>(cacheItemArgs, GetPackagesCallback);
        }

		private static object GetPackagesCallback(CacheItemArgs cacheItemArgs)
		{
			var portalId = (int)cacheItemArgs.ParamList[0];
			return CBO.FillCollection<PackageInfo>(provider.GetPackages(portalId));
		}

        public static List<PackageInfo> GetPackagesByType(string type)
        {
            return GetPackagesByType(Null.NullInteger, type);
        }

        public static IDictionary<int, PackageInfo> GetModulePackagesInUse(int portalID, bool forHost)
        {
            return CBO.FillDictionary<int, PackageInfo>("PackageID", provider.GetModulePackagesInUse(portalID, forHost));
        }

        public static List<PackageInfo> GetPackagesByType(int portalID, string type)
        {
            //return CBO.FillCollection<PackageInfo>(provider.GetPackagesByType(portalID, type));
	        return GetPackages(portalID).Where(p => p.PackageType == type).ToList();
        }

        public static PackageType GetPackageType(string type)
        {
            return CBO.FillObject<PackageType>(provider.GetPackageType(type));
        }

        public static List<PackageType> GetPackageTypes()
        {
            return CBO.FillCollection<PackageType>(provider.GetPackageTypes());
        }

        public static void SavePackage(PackageInfo package)
        {
            if (package.PackageID == Null.NullInteger)
            {
                package.PackageID = AddPackage(package, false);
            }
            else
            {
                UpdatePackage(package);
            }
        }

        public static void UpdatePackage(PackageInfo package)
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
            var objEventLog = new EventLogController();
            objEventLog.AddLog(package, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PACKAGE_UPDATED);

			DataCache.ClearPackagesCache(package.PortalID);
        }
		
		#endregion
    }
}
