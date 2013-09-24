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
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.EventQueue;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class	 : DesktopModuleController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DesktopModuleController provides the Busines Layer for Desktop Modules
    /// </summary>
    /// <history>
    /// 	[cnurse]	01/11/2008   Documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class DesktopModuleController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (DesktopModuleController));
        #region Private Methods

        private static readonly DataProvider DataProvider = DataProvider.Instance();

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
        /// needed for the database call</param>
        /// <history>
        /// 	[cnurse]	01/13/2008   Created
        /// </history>
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
            ContentType contentType = (from t in typeController.GetContentTypes()
                                       where t.ContentType == "DesktopModule"
                                       select t).SingleOrDefault();

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

        #endregion

        #region DesktopModule Methods

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
        /// DeleteDesktopModule deletes a Desktop Module
        /// </summary>
        /// <param name="objDesktopModule">Desktop Module Info</param>
        /// <history>
        /// 	[cnurse]	05/14/2009   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public void DeleteDesktopModule(DesktopModuleInfo objDesktopModule)
        {
            DeleteDesktopModule(objDesktopModule.DesktopModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModule deletes a Desktop Module By ID
        /// </summary>
        /// <param name="desktopModuleID">The ID of the Desktop Module to delete</param>
        /// <history>
        /// 	[cnurse]	01/11/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public void DeleteDesktopModule(int desktopModuleID)
        {
            DataProvider.DeleteDesktopModule(desktopModuleID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("DesktopModuleID",
                               desktopModuleID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULE_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModule deletes a Desktop Module
        /// </summary>
        /// <param name="moduleName">The Name of the Desktop Module to delete</param>
        /// <history>
        /// 	[cnurse]	05/14/2009   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModule(string moduleName)
        {
            DesktopModuleInfo desktopModule = GetDesktopModuleByModuleName(moduleName, Null.NullInteger);
            if (desktopModule != null)
            {
                var controller = new DesktopModuleController();
                controller.DeleteDesktopModule(desktopModule.DesktopModuleID);
                //Delete the Package
                PackageController.Instance.DeleteExtensionPackage(PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModule gets a Desktop Module by its ID
        /// </summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the Dataprovider.</remarks>
        /// <param name="desktopModuleID">The ID of the Desktop Module to get</param>
        /// <param name="portalID">The ID of the portal</param>
        /// <history>
        /// 	[cnurse]	01/13/2008   Created
        /// </history>
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
                Logger.WarnFormat("Unable to find module by module ID. ID:{0} PortalID:{1}", desktopModuleID, portalID);

            return module;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModuleByPackageID gets a Desktop Module by its Package ID
        /// </summary>
        /// <param name="packageID">The ID of the Package</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static DesktopModuleInfo GetDesktopModuleByPackageID(int packageID)
        {
            DesktopModuleInfo desktopModuleByPackageID = (from kvp in GetDesktopModulesInternal(Null.NullInteger)
                                                          where kvp.Value.PackageID == packageID
                                                          select kvp.Value)
                .FirstOrDefault();

            if (desktopModuleByPackageID == null)
                Logger.WarnFormat("Unable to find module by package ID. ID:{0}", packageID);

            return desktopModuleByPackageID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModuleByModuleName gets a Desktop Module by its Name
        /// </summary>
        /// <remarks>This method uses the cached Dictionary of DesktopModules.  It first checks
        /// if the DesktopModule is in the cache.  If it is not in the cache it then makes a call
        /// to the Dataprovider.</remarks>
        /// <param name="moduleName">The name of the Desktop Module to get</param>
        /// <param name="portalID">The ID of the portal</param>
        /// <history>
        /// 	[cnurse]	01/13/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalID)
        {
            DesktopModuleInfo desktopModuleByModuleName = (from kvp in GetDesktopModulesInternal(portalID)
                                                           where kvp.Value.ModuleName == moduleName
                                                           select kvp.Value).FirstOrDefault();

            if (desktopModuleByModuleName == null)
                Logger.WarnFormat("Unable to find module by name. Name:{0} portalId:{1}", moduleName, portalID);

            return desktopModuleByModuleName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModules gets a Dictionary of Desktop Modules
        /// </summary>
        /// <param name="portalID">The ID of the Portal (Use PortalID = Null.NullInteger (-1) to get
        /// all the DesktopModules including Modules not allowed for the current portal</param>
        /// <history>
        /// 	[cnurse]	01/13/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Dictionary<int, DesktopModuleInfo> GetDesktopModules(int portalID)
        {
            return new Dictionary<int, DesktopModuleInfo>(GetDesktopModulesInternal(portalID));
        }

        public static DesktopModuleInfo GetDesktopModuleByFriendlyName(string friendlyName)
        {
            var module = (from kvp in GetDesktopModulesInternal(Null.NullInteger) where kvp.Value.FriendlyName == friendlyName select kvp.Value).FirstOrDefault();

            if (module== null)
                Logger.WarnFormat("Unable to find module by friendly name. Name:{0}", friendlyName);

            return module;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveDesktopModule saves the Desktop Module to the database
        /// </summary>
        /// <param name="desktopModule">The Desktop Module to save</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache</param>
        /// <history>
        /// 	[cnurse]	01/13/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int SaveDesktopModule(DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache)
        {
            return SaveDesktopModule(desktopModule, saveChildren, clearCache, true);
        }

        internal static int SaveDesktopModule(DesktopModuleInfo desktopModule, bool saveChildren, bool clearCache, bool saveTerms)
        {
            var desktopModuleID = desktopModule.DesktopModuleID;
            var eventLogController = new EventLogController();
            if (desktopModuleID == Null.NullInteger)
            {
                CreateContentItem(desktopModule);
                desktopModuleID = DataProvider.AddDesktopModule(desktopModule.PackageID,
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
                                                                UserController.GetCurrentUserInfo().UserID);
                eventLogController.AddLog(desktopModule, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.DESKTOPMODULE_CREATED);
            }
            else
            {
                //Update ContentItem If neccessary
                if (desktopModule.ContentItemId == Null.NullInteger)
                {
                    CreateContentItem(desktopModule);
                }

                DataProvider.UpdateDesktopModule(desktopModule.DesktopModuleID,
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
                                                 UserController.GetCurrentUserInfo().UserID);

                //Update Tags
                if (saveTerms)
                {
                    var termController = Util.GetTermController();
                    termController.RemoveTermsFromContent(desktopModule);
                    foreach (var term in desktopModule.Terms)
                    {
                        termController.AddTermToContent(term, desktopModule);
                    }
                }

                eventLogController.AddLog(desktopModule, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.DESKTOPMODULE_UPDATED);
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

        public void UpdateModuleInterfaces(ref DesktopModuleInfo desktopModuleInfo)
        {
            UpdateModuleInterfaces(ref desktopModuleInfo, (UserController.GetCurrentUserInfo() == null) ? "" : UserController.GetCurrentUserInfo().Username, true);
        }

        public void UpdateModuleInterfaces(ref DesktopModuleInfo desktopModuleInfo, string sender, bool forceAppRestart)
        {
            CheckInterfacesImplementation(ref desktopModuleInfo);
            var oAppStartMessage = new EventMessage
            {
                Sender = sender,
                Priority = MessagePriority.High,
                ExpirationDate = DateTime.Now.AddYears(-1),
                SentDate = DateTime.Now,
                Body = "",
                ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                ProcessorCommand = "UpdateSupportedFeatures"
            };
            oAppStartMessage.Attributes.Add("BusinessControllerClass", desktopModuleInfo.BusinessControllerClass);
            oAppStartMessage.Attributes.Add("DesktopModuleId", desktopModuleInfo.DesktopModuleID.ToString());
            EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
            if ((forceAppRestart))
            {
                Config.Touch();
            }
            
        }

        private void CheckInterfacesImplementation(ref DesktopModuleInfo desktopModuleInfo)
        {
            var businessController = Reflection.CreateType(desktopModuleInfo.BusinessControllerClass);
            var controller = Reflection.CreateObject(desktopModuleInfo.BusinessControllerClass, desktopModuleInfo.BusinessControllerClass);   

            desktopModuleInfo.IsPortable = businessController.GetInterfaces().Contains(typeof (IPortable));
#pragma warning disable 0618
            desktopModuleInfo.IsSearchable = (controller is ModuleSearchBase) || businessController.GetInterfaces().Contains(typeof(ISearchable));
#pragma warning restore 0618
            desktopModuleInfo.IsUpgradeable = businessController.GetInterfaces().Contains(typeof(IUpgradeable));
        }

        #endregion

        #region PortalDesktopModuleMethods

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

        public static int AddDesktopModuleToPortal(int portalID, int desktopModuleID, bool addPermissions, bool clearCache)
        {
            int portalDesktopModuleID;
            PortalDesktopModuleInfo portalDesktopModule = GetPortalDesktopModule(portalID, desktopModuleID);
            if (portalDesktopModule == null)
            {
                portalDesktopModuleID = DataProvider.Instance().AddPortalDesktopModule(portalID, desktopModuleID, UserController.GetCurrentUserInfo().UserID);
                var objEventLog = new EventLogController();
                objEventLog.AddLog("PortalDesktopModuleID",
                                   portalDesktopModuleID.ToString(),
                                   PortalController.GetCurrentPortalSettings(),
                                   UserController.GetCurrentUserInfo().UserID,
                                   EventLogController.EventLogType.PORTALDESKTOPMODULE_CREATED);
                if (addPermissions)
                {
                    ArrayList permissions = PermissionController.GetPermissionsByPortalDesktopModule();
                    if (permissions.Count > 0)
                    {
                        var permission = permissions[0] as PermissionInfo;
                        PortalInfo objPortal = new PortalController().GetPortal(portalID);
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
                DataCache.ClearPortalCache(portalID, true);
            }
            return portalDesktopModuleID;
        }

        public static void AddDesktopModuleToPortals(int desktopModuleID)
        {
            var controller = new PortalController();
            foreach (PortalInfo portal in controller.GetPortals())
            {
                AddDesktopModuleToPortal(portal.PortalID, desktopModuleID, true, false);
            }
            DataCache.ClearHostCache(true);
        }

        public static void AddDesktopModulesToPortal(int portalID)
        {
            foreach (DesktopModuleInfo desktopModule in GetDesktopModulesInternal(Null.NullInteger).Values)
            {
                if (!desktopModule.IsPremium)
                {
                    AddDesktopModuleToPortal(portalID, desktopModule.DesktopModuleID, !desktopModule.IsAdmin, false);
                }
            }
            DataCache.ClearPortalCache(portalID, true);
        }

        public static PortalDesktopModuleInfo GetPortalDesktopModule(int portalID, int desktopModuleID)
        {
            return CBO.FillObject<PortalDesktopModuleInfo>(DataProvider.Instance().GetPortalDesktopModules(portalID, desktopModuleID));
        }

        public static Dictionary<int, PortalDesktopModuleInfo> GetPortalDesktopModulesByDesktopModuleID(int desktopModuleID)
        {
            return CBO.FillDictionary<int, PortalDesktopModuleInfo>("PortalDesktopModuleID", DataProvider.Instance().GetPortalDesktopModules(Null.NullInteger, desktopModuleID));
        }

        public static Dictionary<int, PortalDesktopModuleInfo> GetPortalDesktopModulesByPortalID(int portalID)
        {
            string cacheKey = string.Format(DataCache.PortalDesktopModuleCacheKey, portalID);
            return
                CBO.GetCachedObject<Dictionary<int, PortalDesktopModuleInfo>>(
                    new CacheItemArgs(cacheKey, DataCache.PortalDesktopModuleCacheTimeOut, DataCache.PortalDesktopModuleCachePriority, portalID), GetPortalDesktopModulesByPortalIDCallBack);
        }

        public static SortedList<string, PortalDesktopModuleInfo> GetPortalDesktopModules(int portalID)
        {
            Dictionary<int, PortalDesktopModuleInfo> dicModules = GetPortalDesktopModulesByPortalID(portalID);
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

        public static void RemoveDesktopModuleFromPortal(int portalID, int desktopModuleID, bool clearCache)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalID, desktopModuleID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("DesktopModuleID",
                               desktopModuleID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalID, false);
            }
        }

        public static void RemoveDesktopModuleFromPortals(int desktopModuleID)
        {
            DataProvider.Instance().DeletePortalDesktopModules(Null.NullInteger, desktopModuleID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("DesktopModuleID",
                               desktopModuleID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
            DataCache.ClearHostCache(true);
        }

        public static void RemoveDesktopModulesFromPortal(int portalID)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalID, Null.NullInteger);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("PortalID",
                               portalID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
            DataCache.ClearPortalCache(portalID, true);
        }

        public static void SerializePortalDesktopModules(XmlWriter writer, int portalID)
        {
            writer.WriteStartElement("portalDesktopModules");
            foreach (PortalDesktopModuleInfo portalDesktopModule in GetPortalDesktopModulesByPortalID(portalID).Values)
            {
                writer.WriteStartElement("portalDesktopModule");
                writer.WriteElementString("friendlyname", portalDesktopModule.FriendlyName);
                writer.WriteStartElement("portalDesktopModulePermissions");
                foreach (DesktopModulePermissionInfo permission in portalDesktopModule.Permissions)
                {
                    writer.WriteStartElement("portalDesktopModulePermission");
                    writer.WriteElementString("permissioncode", permission.PermissionCode);
                    writer.WriteElementString("permissionkey", permission.PermissionKey);
                    writer.WriteElementString("allowaccess", permission.AllowAccess.ToString().ToLower());
                    writer.WriteElementString("rolename", permission.RoleName);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        #endregion

        #region Obsolete

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddDesktopModule adds a new Desktop Module to the database
        /// </summary>
        /// <param name="objDesktopModule">The Desktop Module to save</param>
        /// <history>
        /// 	[cnurse]	01/11/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated In DotNetNuke 6.0. Replaced by SaveDesktopModule")]
        public int AddDesktopModule(DesktopModuleInfo objDesktopModule)
        {
            return SaveDesktopModule(objDesktopModule, false, true);
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method AddDesktopModuleToPortal(Integer, Integer)")]
        public void AddPortalDesktopModule(int portalID, int desktopModuleID)
        {
            DataProvider.Instance().AddPortalDesktopModule(portalID, desktopModuleID, UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("DesktopModuleID",
                               desktopModuleID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALDESKTOPMODULE_CREATED);
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method RemoveDesktopModulesFromPortal(Integer)")]
        public void DeletePortalDesktopModules(int portalID, int desktopModuleID)
        {
            DataProvider.Instance().DeletePortalDesktopModules(portalID, desktopModuleID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("DesktopModuleID",
                               desktopModuleID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALDESKTOPMODULE_DELETED);
            DataCache.ClearPortalCache(portalID, true);
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method GetDesktopModule(Integer, Integer)")]
        public DesktopModuleInfo GetDesktopModule(int desktopModuleId)
        {
            return (from kvp in GetDesktopModulesInternal(Null.NullInteger) where kvp.Value.DesktopModuleID == desktopModuleId select kvp.Value).FirstOrDefault();
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method GetDesktopModuleByModuleName(String, Integer)")]
        public DesktopModuleInfo GetDesktopModuleByName(string friendlyName)
        {
            return (from kvp in GetDesktopModulesInternal(Null.NullInteger) where kvp.Value.FriendlyName == friendlyName select kvp.Value).FirstOrDefault();
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method GetDesktopModuleByModuleName(String, Integer)")]
        public DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName)
        {
            return (from kvp in GetDesktopModulesInternal(Null.NullInteger) where kvp.Value.ModuleName == moduleName select kvp.Value).FirstOrDefault();
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method GetDesktopModules(Integer)")]
        public ArrayList GetDesktopModules()
        {
            return new ArrayList(GetDesktopModulesInternal(Null.NullInteger).Values);
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method GetDesktopModules(Integer)")]
        public ArrayList GetDesktopModulesByPortal(int portalID)
        {
            return CBO.FillCollection(DataProvider.GetDesktopModulesByPortal(portalID), typeof(DesktopModuleInfo));
        }

        [Obsolete("This method replaced in DotNetNuke 5.0 by Shared method GetPortalDesktopModulesByPortalID(Integer) and GetPortalDesktopModulesByDesktopModuleID(Integer) And GetPortalDesktopModule(Integer, Integer)")]
        public ArrayList GetPortalDesktopModules(int portalID, int desktopModuleID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetPortalDesktopModules(portalID, desktopModuleID), typeof(PortalDesktopModuleInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateDesktopModule saves the Desktop Module to the database
        /// </summary>
        /// <param name="objDesktopModule">The Desktop Module to save</param>
        /// <history>
        /// 	[cnurse]	01/11/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated In DotNetNuke 6.0. Replaced by SaveDesktopModule")]
        public void UpdateDesktopModule(DesktopModuleInfo objDesktopModule)
        {
            SaveDesktopModule(objDesktopModule, false, true);
        }

        #endregion
    }
}