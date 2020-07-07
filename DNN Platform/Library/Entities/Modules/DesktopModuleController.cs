// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

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
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Upgrade;
    using Microsoft.VisualBasic.Logging;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class    : DesktopModuleController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DesktopModuleController provides the Busines Layer for Desktop Modules.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DesktopModuleController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DesktopModuleController));
        private static readonly DataProvider DataProvider = DataProvider.Instance();

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModule deletes a Desktop Module.
        /// </summary>
        /// <param name="moduleName">The Name of the Desktop Module to delete.</param>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModule gets a Desktop Module by its ID.
        /// </summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the Dataprovider.</remarks>
        /// <param name="desktopModuleID">The ID of the Desktop Module to get.</param>
        /// <param name="portalID">The ID of the portal.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static DesktopModuleInfo GetDesktopModule(int desktopModuleID, int portalID)
        {
            var module = (from kvp in GetDesktopModulesInternal(portalID)
                          where kvp.Value.DesktopModuleID == desktopModuleID
                          select kvp.Value)
                   .FirstOrDefault();

            if (module == null)
            {
                module = (from kvp in GetDesktopModulesInternal(Null.NullInteger)
                          where kvp.Value.DesktopModuleID == desktopModuleID
                          select kvp.Value)
                   .FirstOrDefault();
            }

            if (module == null)
            {
                Logger.WarnFormat("Unable to find module by module ID. ID:{0} PortalID:{1}", desktopModuleID, portalID);
            }

            return module;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModuleByPackageID gets a Desktop Module by its Package ID.
        /// </summary>
        /// <param name="packageID">The ID of the Package.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static DesktopModuleInfo GetDesktopModuleByPackageID(int packageID)
        {
            DesktopModuleInfo desktopModuleByPackageID = (from kvp in GetDesktopModulesInternal(Null.NullInteger)
                                                          where kvp.Value.PackageID == packageID
                                                          select kvp.Value)
                .FirstOrDefault();

            if (desktopModuleByPackageID == null)
            {
                Logger.WarnFormat("Unable to find module by package ID. ID:{0}", packageID);
            }

            return desktopModuleByPackageID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModuleByModuleName gets a Desktop Module by its Name.
        /// </summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the Dataprovider.</remarks>
        /// <param name="moduleName">The name of the Desktop Module to get.</param>
        /// <param name="portalID">The ID of the portal.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalID)
        {
            DesktopModuleInfo desktopModuleByModuleName = (from kvp in GetDesktopModulesInternal(portalID)
                                                           where kvp.Value.ModuleName == moduleName
                                                           select kvp.Value).FirstOrDefault();

            if (desktopModuleByModuleName == null)
            {
                Logger.WarnFormat("Unable to find module by name. Name:{0} portalId:{1}", moduleName, portalID);
            }

            return desktopModuleByModuleName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModules gets a Dictionary of Desktop Modules.
        /// </summary>
        /// <param name="portalID">The ID of the Portal (Use PortalID = Null.NullInteger (-1) to get
        /// all the DesktopModules including Modules not allowed for the current portal.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<int, DesktopModuleInfo> GetDesktopModules(int portalID)
        {
            return new Dictionary<int, DesktopModuleInfo>(GetDesktopModulesInternal(portalID));
        }

        public static DesktopModuleInfo GetDesktopModuleByFriendlyName(string friendlyName)
        {
            var module = (from kvp in GetDesktopModulesInternal(Null.NullInteger) where kvp.Value.FriendlyName == friendlyName select kvp.Value).FirstOrDefault();

            if (module == null)
            {
                Logger.WarnFormat("Unable to find module by friendly name. Name:{0}", friendlyName);
            }

            return module;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveDesktopModule saves the Desktop Module to the database.
        /// </summary>
        /// <param name="desktopModule">The Desktop Module to save.</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static int SaveDesktopModule(DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache)
        {
            return SaveDesktopModule(desktopModule, saveChildren, clearCache, true);
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

        public static int AddDesktopModuleToPortal(int portalId, int desktopModuleId, bool addPermissions, bool clearCache)
        {
            int portalDesktopModuleID;
            PortalDesktopModuleInfo portalDesktopModule = GetPortalDesktopModule(portalId, desktopModuleId);
            if (portalDesktopModule == null)
            {
                portalDesktopModuleID = DataProvider.Instance().AddPortalDesktopModule(portalId, desktopModuleId, UserController.Instance.GetCurrentUserInfo().UserID);
                EventLogController.Instance.AddLog(
                    "PortalDesktopModuleID",
                    portalDesktopModuleID.ToString(),
                    PortalController.Instance.GetCurrentPortalSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogController.EventLogType.PORTALDESKTOPMODULE_CREATED);
                if (addPermissions)
                {
                    ArrayList permissions = PermissionController.GetPermissionsByPortalDesktopModule();
                    if (permissions.Count > 0)
                    {
                        var permission = permissions[0] as PermissionInfo;
                        PortalInfo objPortal = PortalController.Instance.GetPortal(portalId);
                        if (permission != null && objPortal != null)
                        {
                            var desktopModulePermission = new DesktopModulePermissionInfo(permission) { RoleID = objPortal.AdministratorRoleId, AllowAccess = true, PortalDesktopModuleID = portalDesktopModuleID };
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
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                AddDesktopModuleToPortal(portal.PortalID, desktopModuleId, true, false);
            }

            DataCache.ClearHostCache(true);
        }

        public static void AddDesktopModulesToPortal(int portalId)
        {
            foreach (DesktopModuleInfo desktopModule in GetDesktopModulesInternal(Null.NullInteger).Values)
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
                        AddDesktopModuleToPortal(portalId, desktopModule.DesktopModuleID, !desktopModule.IsAdmin, false);
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

        public static Dictionary<int, PortalDesktopModuleInfo> GetPortalDesktopModulesByPortalID(int portalId)
        {
            string cacheKey = string.Format(DataCache.PortalDesktopModuleCacheKey, portalId);
            return
                CBO.GetCachedObject<Dictionary<int, PortalDesktopModuleInfo>>(
                    new CacheItemArgs(cacheKey, DataCache.PortalDesktopModuleCacheTimeOut, DataCache.PortalDesktopModuleCachePriority, portalId), GetPortalDesktopModulesByPortalIDCallBack);
        }

        public static SortedList<string, PortalDesktopModuleInfo> GetPortalDesktopModules(int portalId)
        {
            Dictionary<int, PortalDesktopModuleInfo> dicModules = GetPortalDesktopModulesByPortalID(portalId);
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

        public static void RemoveDesktopModuleFromPortal(int portalId, int desktopModuleId, bool clearCache)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalId, desktopModuleId);
            EventLogController.Instance.AddLog(
                "DesktopModuleID",
                desktopModuleId.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, false);
            }
        }

        public static void RemoveDesktopModuleFromPortals(int desktopModuleId)
        {
            DataProvider.Instance().DeletePortalDesktopModules(Null.NullInteger, desktopModuleId);
            EventLogController.Instance.AddLog(
                "DesktopModuleID",
                desktopModuleId.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
            DataCache.ClearHostCache(true);
        }

        public static void RemoveDesktopModulesFromPortal(int portalId)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalId, Null.NullInteger);
            EventLogController.Instance.AddLog(
                "PortalID",
                portalId.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModule deletes a Desktop Module.
        /// </summary>
        /// <param name="objDesktopModule">Desktop Module Info.</param>
        /// -----------------------------------------------------------------------------
        public void DeleteDesktopModule(DesktopModuleInfo objDesktopModule)
        {
            this.DeleteDesktopModule(objDesktopModule.DesktopModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModule deletes a Desktop Module By ID.
        /// </summary>
        /// <param name="desktopModuleID">The ID of the Desktop Module to delete.</param>
        /// -----------------------------------------------------------------------------
        public void DeleteDesktopModule(int desktopModuleID)
        {
            DataProvider.DeleteDesktopModule(desktopModuleID);
            EventLogController.Instance.AddLog(
                "DesktopModuleID",
                desktopModuleID.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.DESKTOPMODULE_DELETED);
            DataCache.ClearHostCache(true);
        }

        public void UpdateModuleInterfaces(ref DesktopModuleInfo desktopModuleInfo)
        {
            this.UpdateModuleInterfaces(ref desktopModuleInfo, (UserController.Instance.GetCurrentUserInfo() == null) ? string.Empty : UserController.Instance.GetCurrentUserInfo().Username, true);
        }

        public void UpdateModuleInterfaces(ref DesktopModuleInfo desktopModuleInfo, string sender, bool forceAppRestart)
        {
            this.CheckInterfacesImplementation(ref desktopModuleInfo);
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
            oAppStartMessage.Attributes.Add("DesktopModuleId", desktopModuleInfo.DesktopModuleID.ToString());
            EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
            if (forceAppRestart)
            {
                Config.Touch();
            }
        }

        internal static int SaveDesktopModule(DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache, bool saveTerms)
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
                    desktopModule.AdminPage, desktopModule.HostPage);
                EventLogController.Instance.AddLog(desktopModule, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.DESKTOPMODULE_CREATED);
            }
            else
            {
                // Update ContentItem If neccessary
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

                EventLogController.Instance.AddLog(desktopModule, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.DESKTOPMODULE_UPDATED);
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
            var tabPath = string.Format("//{0}//{1}", portalId == Null.NullInteger ? "Host" : "Admin", pageName);
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

        private static Dictionary<int, DesktopModuleInfo> GetDesktopModulesInternal(int portalID)
        {
            string cacheKey = string.Format(DataCache.DesktopModuleCacheKey, portalID);
            var args = new CacheItemArgs(cacheKey, DataCache.DesktopModuleCacheTimeOut, DataCache.DesktopModuleCachePriority, portalID);
            Dictionary<int, DesktopModuleInfo> desktopModules = (portalID == Null.NullInteger)
                                        ? CBO.GetCachedObject<Dictionary<int, DesktopModuleInfo>>(args, GetDesktopModulesCallBack)
                                        : CBO.GetCachedObject<Dictionary<int, DesktopModuleInfo>>(args, GetDesktopModulesByPortalCallBack);
            return desktopModules;
        }

        private static object GetDesktopModulesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary("DesktopModuleID", DataProvider.GetDesktopModules(), new Dictionary<int, DesktopModuleInfo>());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulesByPortalCallBack gets a Dictionary of Desktop Modules by
        /// Portal from the the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call.</param>
        /// -----------------------------------------------------------------------------
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
            IContentTypeController typeController = new ContentTypeController();
            ContentType contentType = ContentType.DesktopModule;

            if (contentType == null)
            {
                contentType = new ContentType { ContentType = "DesktopModule" };
                contentType.ContentTypeId = typeController.AddContentType(contentType);
            }

            IContentController contentController = Util.GetContentController();
            desktopModule.Content = desktopModule.FriendlyName;
            desktopModule.Indexed = false;
            desktopModule.ContentTypeId = contentType.ContentTypeId;
            desktopModule.ContentItemId = contentController.AddContentItem(desktopModule);
        }

        private void CheckInterfacesImplementation(ref DesktopModuleInfo desktopModuleInfo)
        {
            var businessController = Reflection.CreateType(desktopModuleInfo.BusinessControllerClass);
            var controller = Reflection.CreateObject(desktopModuleInfo.BusinessControllerClass, desktopModuleInfo.BusinessControllerClass);

            desktopModuleInfo.IsPortable = businessController.GetInterfaces().Contains(typeof(IPortable));
#pragma warning disable 0618
            desktopModuleInfo.IsSearchable = (controller is ModuleSearchBase) || businessController.GetInterfaces().Contains(typeof(ISearchable));
#pragma warning restore 0618
            desktopModuleInfo.IsUpgradeable = businessController.GetInterfaces().Contains(typeof(IUpgradeable));
        }
    }
}
