// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Upgrade;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>DesktopModuleController provides the Business Layer for Desktop Modules.</summary>
        /// <param name="eventLogger">The event logger.</param>
    public partial class DesktopModuleController(IEventLogger eventLogger)
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DesktopModuleController));
        private static readonly DataProvider DataProvider = DataProvider.Instance();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

        /// <summary>Initializes a new instance of the <see cref="DesktopModuleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public DesktopModuleController()
            : this(null)
        {
        }

        public static void AddModuleCategory(string category)
        {
            var termController = Util.GetTermController();
            var term = (from Term t in termController.GetTermsByVocabulary("Module_Categories")
                        where t.Name == category
                        select t)
                        .FirstOrDefault();

            if (term == null)
            {
                var vocabularyController = Util.GetVocabularyController();
                var vocabulary = (from v in vocabularyController.GetVocabularies()
                                  where v.Name == "Module_Categories"
                                  select v)
                                  .FirstOrDefault();

                if (vocabulary != null)
                {
                    term = new Term(vocabulary.VocabularyId) { Name = category };

                    termController.AddTerm(term);
                }
            }
        }

        /// <summary>DeleteDesktopModule deletes a Desktop Module.</summary>
        /// <param name="moduleName">The Name of the Desktop Module to delete.</param>
        public static void DeleteDesktopModule(string moduleName)
        {
            DesktopModuleInfo desktopModule = GetDesktopModuleByModuleName(moduleName, Null.NullInteger);
            if (desktopModule != null)
            {
                var controller = new DesktopModuleController();
                controller.DeleteDesktopModule(desktopModule.DesktopModuleID);

                // Delete the Package
                PackageController.Instance.DeleteExtensionPackage(PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID));
            }
        }

        /// <summary>GetDesktopModule gets a Desktop Module by its ID.</summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the <see cref="DataProvider"/>.</remarks>
        /// <param name="desktopModuleID">The ID of the Desktop Module to get.</param>
        /// <param name="portalID">The ID of the portal.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial DesktopModuleInfo GetDesktopModule(int desktopModuleID, int portalID)
            => GetDesktopModule(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), desktopModuleID, portalID);

        /// <summary>GetDesktopModule gets a Desktop Module by its ID.</summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the <see cref="DataProvider"/>.</remarks>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="desktopModuleId">The ID of the Desktop Module to get.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        public static DesktopModuleInfo GetDesktopModule(IHostSettings hostSettings, int desktopModuleId, int portalId)
        {
            var module = (from kvp in GetDesktopModulesInternal(hostSettings, portalId)
                          where kvp.Value.DesktopModuleID == desktopModuleId
                          select kvp.Value)
                   .FirstOrDefault();

            if (module == null)
            {
                module = (from kvp in GetDesktopModulesInternal(hostSettings, Null.NullInteger)
                          where kvp.Value.DesktopModuleID == desktopModuleId
                          select kvp.Value)
                   .FirstOrDefault();
            }

            if (module == null)
            {
                Logger.WarnFormat(CultureInfo.InvariantCulture, "Unable to find module by module ID. ID:{0} PortalID:{1}", desktopModuleId, portalId);
            }

            return module;
        }

        /// <summary>GetDesktopModuleByPackageID gets a Desktop Module by its Package ID.</summary>
        /// <param name="packageID">The ID of the Package.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial DesktopModuleInfo GetDesktopModuleByPackageID(int packageID)
            => GetDesktopModuleByPackageID(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), packageID);

        /// <summary>Gets a Desktop Module by its Package ID.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="packageId">The ID of the Package.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        public static DesktopModuleInfo GetDesktopModuleByPackageID(IHostSettings hostSettings, int packageId)
        {
            var desktopModuleByPackageId = (
                    from kvp in GetDesktopModulesInternal(hostSettings, Null.NullInteger)
                    where kvp.Value.PackageID == packageId
                    select kvp.Value)
                .FirstOrDefault();

            if (desktopModuleByPackageId == null)
            {
                Logger.WarnFormat(CultureInfo.InvariantCulture, "Unable to find module by package ID. ID:{0}", packageId);
            }

            return desktopModuleByPackageId;
        }

        /// <summary>GetDesktopModuleByModuleName gets a Desktop Module by its Name.</summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the DataProvider.</remarks>
        /// <param name="moduleName">The name of the Desktop Module to get.</param>
        /// <param name="portalID">The ID of the portal.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalID)
            => GetDesktopModuleByModuleName(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), moduleName, portalID);

        /// <summary>GetDesktopModuleByModuleName gets a Desktop Module by its Name.</summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the DataProvider.</remarks>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="moduleName">The name of the Desktop Module to get.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        public static DesktopModuleInfo GetDesktopModuleByModuleName(IHostSettings hostSettings, string moduleName, int portalId)
        {
            var desktopModuleByModuleName =
                (from kvp in GetDesktopModulesInternal(hostSettings, portalId)
                    where kvp.Value.ModuleName == moduleName
                    select kvp.Value).FirstOrDefault();

            if (desktopModuleByModuleName == null)
            {
                Logger.WarnFormat(CultureInfo.InvariantCulture, "Unable to find module by name. Name:{0} portalId:{1}", moduleName, portalId);
            }

            return desktopModuleByModuleName;
        }

        /// <summary>GetDesktopModules gets a Dictionary of Desktop Modules.</summary>
        /// <param name="portalID">The ID of the Portal (Use PortalID = Null.NullInteger (-1) to get all the DesktopModules including Modules not allowed for the current portal).</param>
        /// <returns>A new <see cref="Dictionary{TKey,TValue}"/> mapping desktop module ID to <see cref="DesktopModuleInfo"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial Dictionary<int, DesktopModuleInfo> GetDesktopModules(int portalID)
            => GetDesktopModules(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalID);

        /// <summary>GetDesktopModules gets a Dictionary of Desktop Modules.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The ID of the Portal (Use PortalID = Null.NullInteger (-1) to get all the DesktopModules including Modules not allowed for the current portal).</param>
        /// <returns>A new <see cref="Dictionary{TKey,TValue}"/> mapping desktop module ID to <see cref="DesktopModuleInfo"/>.</returns>
        public static Dictionary<int, DesktopModuleInfo> GetDesktopModules(IHostSettings hostSettings, int portalId)
        {
            return new Dictionary<int, DesktopModuleInfo>(GetDesktopModulesInternal(hostSettings, portalId));
        }

        /// <summary>Gets a Desktop Module by its friendly name.</summary>
        /// <param name="friendlyName">The friendly name of the Desktop Module to get.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial DesktopModuleInfo GetDesktopModuleByFriendlyName(string friendlyName)
            => GetDesktopModuleByFriendlyName(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), friendlyName);

        /// <summary>Gets a Desktop Module by its friendly name.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="friendlyName">The friendly name of the Desktop Module to get.</param>
        /// <returns>The <see cref="DesktopModuleInfo"/> or <see langword="null"/>.</returns>
        public static DesktopModuleInfo GetDesktopModuleByFriendlyName(IHostSettings hostSettings, string friendlyName)
        {
            var module =
                (from kvp in GetDesktopModulesInternal(hostSettings, Null.NullInteger)
                    where kvp.Value.FriendlyName == friendlyName
                    select kvp.Value).FirstOrDefault();

            if (module == null)
            {
                Logger.WarnFormat(CultureInfo.InvariantCulture, "Unable to find module by friendly name. Name:{0}", friendlyName);
            }

            return module;
        }

        /// <summary>SaveDesktopModule saves the Desktop Module to the database.</summary>
        /// <param name="desktopModule">The Desktop Module to save.</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns>The desktop module ID.</returns>
        public static int SaveDesktopModule(DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache)
            => SaveDesktopModule(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), desktopModule, saveChildren, clearCache);

        /// <summary>SaveDesktopModule saves the Desktop Module to the database.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="desktopModule">The Desktop Module to save.</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns>The desktop module ID.</returns>
        public static int SaveDesktopModule(IEventLogger eventLogger, DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache)
        {
            return SaveDesktopModule(eventLogger, desktopModule, saveChildren, clearCache, true);
        }

        public static int AddDesktopModuleToPortal(int portalID, DesktopModuleInfo desktopModule, DesktopModulePermissionCollection permissions, bool clearCache)
        {
            int portalDesktopModuleID = AddDesktopModuleToPortal(portalID, desktopModule.DesktopModuleID, false, clearCache);
            if (portalDesktopModuleID > Null.NullInteger)
            {
                DesktopModulePermissionController.DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID);
                foreach (DesktopModulePermissionInfo permission in permissions)
                {
                    permission.PortalDesktopModuleID = portalDesktopModuleID;
                    DesktopModulePermissionController.AddDesktopModulePermission(permission);
                }
            }

            return portalDesktopModuleID;
        }

        /// <summary>Add a desktop module to a portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="desktopModuleId">The desktop module ID.</param>
        /// <param name="addPermissions">Whether to add permissions for the administrator role to deploy the module.</param>
        /// <param name="clearCache">Whether to clear the cache after adding.</param>
        /// <returns>The portal desktop module ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddDesktopModuleToPortal(int portalId, int desktopModuleId, bool addPermissions, bool clearCache)
            => AddDesktopModuleToPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), portalId, desktopModuleId, addPermissions, clearCache);

        /// <summary>Add a desktop module to a portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="desktopModuleId">The desktop module ID.</param>
        /// <param name="addPermissions">Whether to add permissions for the administrator role to deploy the module.</param>
        /// <param name="clearCache">Whether to clear the cache after adding.</param>
        /// <returns>The portal desktop module ID.</returns>
        public static int AddDesktopModuleToPortal(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, int portalId, int desktopModuleId, bool addPermissions, bool clearCache)
        {
            int portalDesktopModuleID;
            PortalDesktopModuleInfo portalDesktopModule = GetPortalDesktopModule(portalId, desktopModuleId);
            if (portalDesktopModule == null)
            {
                portalDesktopModuleID = DataProvider.Instance().AddPortalDesktopModule(portalId, desktopModuleId, UserController.Instance.GetCurrentUserInfo().UserID);
                eventLogger.AddLog(
                    "PortalDesktopModuleID",
                    portalDesktopModuleID.ToString(CultureInfo.InvariantCulture),
                    PortalController.Instance.GetCurrentSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogType.PORTALDESKTOPMODULE_CREATED);
                if (addPermissions)
                {
                    var permission = permissionDefinitionService.GetDefinitionsByPortalDesktopModule().FirstOrDefault();
                    if (permission is not null)
                    {
                        PortalInfo objPortal = PortalController.Instance.GetPortal(portalId);
                        if (objPortal != null)
                        {
                            var desktopModulePermission = new DesktopModulePermissionInfo(permission)
                            {
                                RoleID = objPortal.AdministratorRoleId,
                                AllowAccess = true,
                                PortalDesktopModuleID = portalDesktopModuleID,
                            };
                            DesktopModulePermissionController.AddDesktopModulePermission(desktopModulePermission);
                        }
                    }
                }
            }
            else
            {
                portalDesktopModuleID = portalDesktopModule.PortalDesktopModuleID;
            }

            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, true);
            }

            return portalDesktopModuleID;
        }

        public static void AddDesktopModuleToPortals(int desktopModuleId)
        {
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                AddDesktopModuleToPortal(portal.PortalId, desktopModuleId, true, false);
            }

            DataCache.ClearHostCache(true);
        }

        /// <summary>Adds each non-premium desktop module to the specified portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial void AddDesktopModulesToPortal(int portalId)
            => AddDesktopModulesToPortal(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(),
                portalId);

        /// <summary>Adds each non-premium desktop module to the specified portal.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void AddDesktopModulesToPortal(IHostSettings hostSettings, IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, int portalId)
        {
            foreach (DesktopModuleInfo desktopModule in GetDesktopModulesInternal(hostSettings, Null.NullInteger).Values)
            {
                if (!desktopModule.IsPremium)
                {
                    if (desktopModule.Page != null && !string.IsNullOrEmpty(desktopModule.AdminPage))
                    {
                        bool createdNewPage = false, addedNewModule = false;
                        AddDesktopModulePageToPortal(desktopModule, desktopModule.AdminPage, portalId, ref createdNewPage, ref addedNewModule);
                    }
                    else
                    {
                        AddDesktopModuleToPortal(eventLogger, permissionDefinitionService, portalId, desktopModule.DesktopModuleID, !desktopModule.IsAdmin, false);
                    }
                }
            }

            DataCache.ClearPortalCache(portalId, true);
        }

        public static PortalDesktopModuleInfo GetPortalDesktopModule(int portalId, int desktopModuleId)
        {
            return CBO.FillObject<PortalDesktopModuleInfo>(DataProvider.Instance().GetPortalDesktopModules(portalId, desktopModuleId));
        }

        public static Dictionary<int, PortalDesktopModuleInfo> GetPortalDesktopModulesByDesktopModuleID(int desktopModuleId)
        {
            return CBO.FillDictionary<int, PortalDesktopModuleInfo>("PortalDesktopModuleID", DataProvider.Instance().GetPortalDesktopModules(Null.NullInteger, desktopModuleId));
        }

        /// <summary>Gets the <see cref="PortalDesktopModuleInfo"/> values for the specified portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A dictionary of <see cref="PortalDesktopModuleInfo"/> instances by <see cref="PortalDesktopModuleInfo.PortalDesktopModuleID"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial Dictionary<int, PortalDesktopModuleInfo> GetPortalDesktopModulesByPortalID(int portalId)
            => GetPortalDesktopModulesByPortalID(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId);

        /// <summary>Gets the <see cref="PortalDesktopModuleInfo"/> values for the specified portal.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A dictionary of <see cref="PortalDesktopModuleInfo"/> instances by <see cref="PortalDesktopModuleInfo.PortalDesktopModuleID"/>.</returns>
        public static Dictionary<int, PortalDesktopModuleInfo> GetPortalDesktopModulesByPortalID(IHostSettings hostSettings, int portalId)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalDesktopModuleCacheKey, portalId);
            return
                CBO.GetCachedObject<Dictionary<int, PortalDesktopModuleInfo>>(
                    hostSettings,
                    new CacheItemArgs(cacheKey, DataCache.PortalDesktopModuleCacheTimeOut, DataCache.PortalDesktopModuleCachePriority, portalId),
                    GetPortalDesktopModulesByPortalIDCallBack);
        }

        /// <summary>Gets the portal desktop modules.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="SortedList{T,U}"/> of <see cref="PortalDesktopModuleInfo"/> instances by <see cref="PortalDesktopModuleInfo.FriendlyName"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial SortedList<string, PortalDesktopModuleInfo> GetPortalDesktopModules(int portalId)
            => GetPortalDesktopModules(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId);

        /// <summary>Gets the portal desktop modules.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="SortedList{T,U}"/> of <see cref="PortalDesktopModuleInfo"/> instances by <see cref="PortalDesktopModuleInfo.FriendlyName"/>.</returns>
        public static SortedList<string, PortalDesktopModuleInfo> GetPortalDesktopModules(IHostSettings hostSettings, int portalId)
        {
            Dictionary<int, PortalDesktopModuleInfo> dicModules = GetPortalDesktopModulesByPortalID(hostSettings, portalId);
            var lstModules = new SortedList<string, PortalDesktopModuleInfo>();
            foreach (PortalDesktopModuleInfo desktopModule in dicModules.Values)
            {
                if (DesktopModulePermissionController.HasDesktopModulePermission(desktopModule.Permissions, "DEPLOY"))
                {
                    lstModules.Add(desktopModule.FriendlyName, desktopModule);
                }
            }

            return lstModules;
        }

        /// <summary>Remove a desktop module from a portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="desktopModuleId">The desktop module ID.</param>
        /// <param name="clearCache">Whether to clear the cache after the removal.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveDesktopModuleFromPortal(int portalId, int desktopModuleId, bool clearCache)
            => RemoveDesktopModuleFromPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalId, desktopModuleId, clearCache);

        /// <summary>Remove a desktop module from a portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="desktopModuleId">The desktop module ID.</param>
        /// <param name="clearCache">Whether to clear the cache after the removal.</param>
        public static void RemoveDesktopModuleFromPortal(IEventLogger eventLogger, int portalId, int desktopModuleId, bool clearCache)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalId, desktopModuleId);
            eventLogger.AddLog(
                "DesktopModuleID",
                desktopModuleId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.PORTALDESKTOPMODULE_DELETED);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, false);
            }
        }

        /// <summary>Remove the desktop module from all portals.</summary>
        /// <param name="desktopModuleId">The desktop module ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveDesktopModuleFromPortals(int desktopModuleId)
            => RemoveDesktopModuleFromPortals(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), desktopModuleId);

        /// <summary>Remove the desktop module from all portals.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="desktopModuleId">The desktop module ID.</param>
        public static void RemoveDesktopModuleFromPortals(IEventLogger eventLogger, int desktopModuleId)
        {
            DataProvider.Instance().DeletePortalDesktopModules(Null.NullInteger, desktopModuleId);
            eventLogger.AddLog(
                "DesktopModuleID",
                desktopModuleId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.PORTALDESKTOPMODULE_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>Remove all desktop modules from the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveDesktopModulesFromPortal(int portalId)
            => RemoveDesktopModulesFromPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalId);

        /// <summary>Remove all desktop modules from the portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void RemoveDesktopModulesFromPortal(IEventLogger eventLogger, int portalId)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalId, Null.NullInteger);
            eventLogger.AddLog(
                "PortalID",
                portalId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.PORTALDESKTOPMODULE_DELETED);
            DataCache.ClearPortalCache(portalId, true);
        }

        public static void SerializePortalDesktopModules(XmlWriter writer, int portalId)
        {
            writer.WriteStartElement("portalDesktopModules");
            foreach (PortalDesktopModuleInfo portalDesktopModule in GetPortalDesktopModulesByPortalID(portalId).Values)
            {
                writer.WriteStartElement("portalDesktopModule");
                writer.WriteElementString("friendlyname", portalDesktopModule.FriendlyName);
                writer.WriteStartElement("portalDesktopModulePermissions");
                foreach (DesktopModulePermissionInfo permission in portalDesktopModule.Permissions)
                {
                    writer.WriteStartElement("portalDesktopModulePermission");
                    writer.WriteElementString("permissioncode", permission.PermissionCode);
                    writer.WriteElementString("permissionkey", permission.PermissionKey);
                    writer.WriteElementString("allowaccess", permission.AllowAccess.ToString().ToLowerInvariant());
                    writer.WriteElementString("rolename", permission.RoleName);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>DeleteDesktopModule deletes a Desktop Module.</summary>
        /// <param name="objDesktopModule">Desktop Module Info.</param>
        public void DeleteDesktopModule(DesktopModuleInfo objDesktopModule)
        {
            this.DeleteDesktopModule(objDesktopModule.DesktopModuleID);
        }

        /// <summary>DeleteDesktopModule deletes a Desktop Module By ID.</summary>
        /// <param name="desktopModuleID">The ID of the Desktop Module to delete.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void DeleteDesktopModule(int desktopModuleID)
        {
            DataProvider.DeleteDesktopModule(desktopModuleID);
            this.eventLogger.AddLog(
                "DesktopModuleID",
                desktopModuleID.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.DESKTOPMODULE_DELETED);
            DataCache.ClearHostCache(true);
        }

        public void UpdateModuleInterfaces(ref DesktopModuleInfo desktopModuleInfo)
        {
            this.UpdateModuleInterfaces(ref desktopModuleInfo, (UserController.Instance.GetCurrentUserInfo() == null) ? string.Empty : UserController.Instance.GetCurrentUserInfo().Username, true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void UpdateModuleInterfaces(ref DesktopModuleInfo desktopModuleInfo, string sender, bool forceAppRestart)
        {
            CheckInterfacesImplementation(ref desktopModuleInfo);
            var oAppStartMessage = new EventMessage
            {
                Sender = sender,
                Priority = MessagePriority.High,
                ExpirationDate = DateTime.Now.AddYears(-1),
                SentDate = DateTime.Now,
                Body = string.Empty,
                ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                ProcessorCommand = "UpdateSupportedFeatures",
            };
            oAppStartMessage.Attributes.Add("BusinessControllerClass", desktopModuleInfo.BusinessControllerClass);
            oAppStartMessage.Attributes.Add("DesktopModuleId", desktopModuleInfo.DesktopModuleID.ToString(CultureInfo.InvariantCulture));
            EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
            if (forceAppRestart)
            {
                Config.Touch();
            }
        }

        internal static int SaveDesktopModule(IEventLogger eventLogger, DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache, bool saveTerms)
        {
            var desktopModuleID = desktopModule.DesktopModuleID;
            if (desktopModuleID == Null.NullInteger)
            {
                CreateContentItem(desktopModule);
                desktopModuleID = DataProvider.AddDesktopModule(
                    desktopModule.PackageID,
                    desktopModule.ModuleName,
                    desktopModule.FolderName,
                    desktopModule.FriendlyName,
                    desktopModule.Description,
                    desktopModule.Version,
                    desktopModule.IsPremium,
                    desktopModule.IsAdmin,
                    desktopModule.BusinessControllerClass,
                    desktopModule.SupportedFeatures,
                    (int)desktopModule.Shareable,
                    desktopModule.CompatibleVersions,
                    desktopModule.Dependencies,
                    desktopModule.Permissions,
                    desktopModule.ContentItemId,
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    desktopModule.AdminPage,
                    desktopModule.HostPage);
                eventLogger.AddLog(desktopModule, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.DESKTOPMODULE_CREATED);
            }
            else
            {
                // Update ContentItem If necessary
                if (desktopModule.ContentItemId == Null.NullInteger)
                {
                    CreateContentItem(desktopModule);
                }

                DataProvider.UpdateDesktopModule(
                    desktopModule.DesktopModuleID,
                    desktopModule.PackageID,
                    desktopModule.ModuleName,
                    desktopModule.FolderName,
                    desktopModule.FriendlyName,
                    desktopModule.Description,
                    desktopModule.Version,
                    desktopModule.IsPremium,
                    desktopModule.IsAdmin,
                    desktopModule.BusinessControllerClass,
                    desktopModule.SupportedFeatures,
                    (int)desktopModule.Shareable,
                    desktopModule.CompatibleVersions,
                    desktopModule.Dependencies,
                    desktopModule.Permissions,
                    desktopModule.ContentItemId,
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    desktopModule.AdminPage,
                    desktopModule.HostPage);

                // Update Tags
                if (saveTerms)
                {
                    var termController = Util.GetTermController();
                    termController.RemoveTermsFromContent(desktopModule);
                    foreach (var term in desktopModule.Terms)
                    {
                        termController.AddTermToContent(term, desktopModule);
                    }
                }

                eventLogger.AddLog(desktopModule, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.DESKTOPMODULE_UPDATED);
            }

            if (saveChildren)
            {
                foreach (var definition in desktopModule.ModuleDefinitions.Values)
                {
                    definition.DesktopModuleID = desktopModuleID;
                    var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByDefinitionName(definition.DefinitionName, desktopModuleID);
                    if (moduleDefinition != null)
                    {
                        definition.ModuleDefID = moduleDefinition.ModuleDefID;
                    }

                    ModuleDefinitionController.SaveModuleDefinition(definition, true, clearCache);
                }
            }

            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }

            return desktopModuleID;
        }

        internal static void AddDesktopModulePageToPortal(DesktopModuleInfo desktopModule, string pageName, int portalId, ref bool createdNewPage, ref bool addedNewModule)
        {
            var hostTabId = TabController.GetTabByTabPath(Null.NullInteger, "//Host", Null.NullString);
            if (hostTabId == Null.NullInteger)
            {
                return;
            }

            var tabPath = Globals.GenerateTabPath(hostTabId, pageName);

            var tabId = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
            TabInfo existTab = TabController.Instance.GetTab(tabId, portalId);
            if (existTab == null)
            {
                if (portalId == Null.NullInteger)
                {
                    existTab = Upgrade.AddHostPage(
                        pageName,
                        desktopModule.Page.Description,
                        desktopModule.Page.Icon,
                        desktopModule.Page.LargeIcon,
                        true);
                }
                else
                {
                    existTab = Upgrade.AddAdminPage(
                        PortalController.Instance.GetPortal(portalId),
                        pageName,
                        desktopModule.Page.Description,
                        desktopModule.Page.Icon,
                        desktopModule.Page.LargeIcon,
                        true);
                }

                createdNewPage = true;
            }

            if (existTab != null)
            {
                if (desktopModule.Page.IsCommon)
                {
                    TabController.Instance.UpdateTabSetting(existTab.TabID, "ControlBar_CommonTab", "Y");
                }

                AddDesktopModuleToPage(desktopModule, existTab, ref addedNewModule);
            }
        }

        internal static void AddDesktopModuleToPage(DesktopModuleInfo desktopModule, TabInfo tab, ref bool addedNewModule)
        {
            if (tab.PortalID != Null.NullInteger)
            {
                AddDesktopModuleToPortal(tab.PortalID, desktopModule.DesktopModuleID, !desktopModule.IsAdmin, false);
            }

            var moduleDefinitions = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModule.DesktopModuleID).Values;
            var tabModules = ModuleController.Instance.GetTabModules(tab.TabID).Values;
            foreach (var moduleDefinition in moduleDefinitions)
            {
                if (tabModules.All(m => m.ModuleDefinition.ModuleDefID != moduleDefinition.ModuleDefID))
                {
                    Upgrade.AddModuleToPage(
                        tab,
                        moduleDefinition.ModuleDefID,
                        desktopModule.Page.Description,
                        desktopModule.Page.Icon,
                        true);

                    addedNewModule = true;
                }
            }
        }

        private static Dictionary<int, DesktopModuleInfo> GetDesktopModulesInternal(IHostSettings hostSettings, int portalId)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.DesktopModuleCacheKey, portalId);
            var args = new CacheItemArgs(cacheKey, DataCache.DesktopModuleCacheTimeOut, DataCache.DesktopModuleCachePriority, portalId);
            return portalId == Null.NullInteger
                ? CBO.GetCachedObject<Dictionary<int, DesktopModuleInfo>>(hostSettings, args, GetDesktopModulesCallBack)
                : CBO.GetCachedObject<Dictionary<int, DesktopModuleInfo>>(hostSettings, args, GetDesktopModulesByPortalCallBack);
        }

        private static object GetDesktopModulesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary("DesktopModuleID", DataProvider.GetDesktopModules(), new Dictionary<int, DesktopModuleInfo>());
        }

        /// <summary>GetDesktopModulesByPortalCallBack gets a Dictionary of Desktop Modules by Portal from the Database.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private static object GetDesktopModulesByPortalCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillDictionary("DesktopModuleID", DataProvider.GetDesktopModulesByPortal(portalId), new Dictionary<int, DesktopModuleInfo>());
        }

        private static object GetPortalDesktopModulesByPortalIDCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillDictionary("PortalDesktopModuleID", DataProvider.Instance().GetPortalDesktopModules(portalId, Null.NullInteger), new Dictionary<int, PortalDesktopModuleInfo>());
        }

        private static void CreateContentItem(DesktopModuleInfo desktopModule)
        {
            var typeController = new ContentTypeController();
            var contentType = ContentType.DesktopModule;

            if (contentType == null)
            {
                contentType = new ContentType { ContentType = "DesktopModule" };
                contentType.ContentTypeId = typeController.AddContentType(contentType);
            }

            var contentController = Util.GetContentController();
            desktopModule.Content = desktopModule.FriendlyName;
            desktopModule.Indexed = false;
            desktopModule.ContentTypeId = contentType.ContentTypeId;
            desktopModule.ContentItemId = contentController.AddContentItem(desktopModule);
        }

        private static void CheckInterfacesImplementation(ref DesktopModuleInfo desktopModuleInfo)
        {
            var businessControllerType = Reflection.CreateType(desktopModuleInfo.BusinessControllerClass);

            desktopModuleInfo.IsPortable = typeof(IPortable).IsAssignableFrom(businessControllerType);
            desktopModuleInfo.IsSearchable = typeof(ModuleSearchBase).IsAssignableFrom(businessControllerType);
            desktopModuleInfo.IsUpgradeable = typeof(IUpgradeable).IsAssignableFrom(businessControllerType);
        }
    }
}
