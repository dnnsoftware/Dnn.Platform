#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.OutputCache;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// ModuleController provides the Business Layer for Modules
    /// </summary>
    public partial class ModuleController : ServiceLocator<IModuleController, ModuleController>, IModuleController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModuleController));
        private static readonly DataProvider dataProvider = DataProvider.Instance();

        protected override Func<IModuleController> GetFactory()
        {
            return () => new ModuleController();
        }

        private static event EventHandler<ModuleEventArgs> ModuleCreated;
        private static event EventHandler<ModuleEventArgs> ModuleUpdated;
        private static event EventHandler<ModuleEventArgs> ModuleRemoved; // soft delete
        private static event EventHandler<ModuleEventArgs> ModuleDeleted; // hard delete

        static ModuleController()
        {
            foreach (var handlers in EventHandlersContainer<IModuleEventHandler>.Instance.EventHandlers)
            {
                ModuleCreated += handlers.Value.ModuleCreated;
                ModuleUpdated += handlers.Value.ModuleUpdated;
                ModuleRemoved += handlers.Value.ModuleRemoved;
                ModuleDeleted += handlers.Value.ModuleDeleted;
            }
        }

        #region Private Methods

        private static void AddContent(XmlNode nodeModule, ModuleInfo module)
        {
            if (!String.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
            {
                try
                {
                    object businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                    var controller = businessController as IPortable;
                    if (controller != null)
                    {
                        string Content = Convert.ToString(controller.ExportModule(module.ModuleID));
                        if (!String.IsNullOrEmpty(Content))
                        {
                            //add attributes to XML document
                            if (nodeModule.OwnerDocument != null)
                            {
                                var existing = nodeModule.OwnerDocument.GetElementById("content");
                                if (existing != null)
                                {
                                    nodeModule.OwnerDocument.RemoveChild(existing);
                                }

                                XmlNode newnode = nodeModule.OwnerDocument.CreateElement("content");
                                XmlAttribute xmlattr = nodeModule.OwnerDocument.CreateAttribute("type");
                                xmlattr.Value = Globals.CleanName(module.DesktopModule.ModuleName);
                                if (newnode.Attributes != null)
                                {
                                    newnode.Attributes.Append(xmlattr);
                                }
                                xmlattr = nodeModule.OwnerDocument.CreateAttribute("version");
                                xmlattr.Value = module.DesktopModule.Version;
                                if (newnode.Attributes != null)
                                {
                                    newnode.Attributes.Append(xmlattr);
                                }
                                Content = HttpContext.Current.Server.HtmlEncode(Content);
                                newnode.InnerXml = XmlUtils.XMLEncode(Content);
                                nodeModule.AppendChild(newnode);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }
        }

        private void AddModuleInternal(ModuleInfo module)
        {
            // add module
            if (Null.IsNull(module.ModuleID))
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                CreateContentItem(module);

                //Add Module
                module.ModuleID = dataProvider.AddModule(module.ContentItemId,
                                                            module.PortalID,
                                                            module.ModuleDefID,
                                                            module.AllTabs,
                                                            module.StartDate,
                                                            module.EndDate,
                                                            module.InheritViewPermissions,
                                                            module.IsShareable,
                                                            module.IsShareableViewOnly,
                                                            module.IsDeleted,
                                                            currentUser.UserID);

                //Now we have the ModuleID - update the contentItem
                var contentController = Util.GetContentController();
                contentController.UpdateContentItem(module);

                EventLogController.Instance.AddLog(module, PortalController.Instance.GetCurrentPortalSettings(), currentUser.UserID, "", EventLogController.EventLogType.MODULE_CREATED);

                // set module permissions
                ModulePermissionController.SaveModulePermissions(module);
            }

            //Save ModuleSettings
            UpdateModuleSettings(module);

            if (ModuleCreated != null)
                ModuleCreated(null, new ModuleEventArgs { Module = module });
        }

        private static void AddModulePermission(ref ModuleInfo module, int portalId, string roleName, PermissionInfo permission, string permissionKey)
        {
            var perm = module.ModulePermissions.Where(tp => tp.RoleName == roleName && tp.PermissionKey == permissionKey).SingleOrDefault();
            if (permission != null && perm == null)
            {
                var modulePermission = new ModulePermissionInfo(permission);
// ReSharper disable ImplicitlyCapturedClosure
                var role = RoleController.Instance.GetRole(portalId, r => (r.RoleName == roleName));
// ReSharper restore ImplicitlyCapturedClosure
                if (role != null)
                {
                    modulePermission.RoleID = role.RoleID;
                    modulePermission.AllowAccess = true;

                    module.ModulePermissions.Add(modulePermission);
                }
            }
        }

		private ModulePermissionInfo AddModulePermission(ModuleInfo module, PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var modulePermission = new ModulePermissionInfo
            {
				ModuleID = module.ModuleID,
                PermissionID = permission.PermissionID,
                RoleID = roleId,
                UserID = userId,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess
            };

            // add the permission to the collection
			if (!module.ModulePermissions.Contains(modulePermission))
            {
				module.ModulePermissions.Add(modulePermission);
            }

			return modulePermission;
        }

        private static bool CheckIsInstance(int templateModuleID, Hashtable hModules)
        {
            //will be instance or module?
            bool IsInstance = false;
            if (templateModuleID > 0)
            {
                if (hModules[templateModuleID] != null)
                {
                    //this module has already been processed -> process as instance
                    IsInstance = true;
                }
            }
            return IsInstance;
        }

        private void ClearModuleSettingsCache(int moduleId)
        {
            foreach (var tab in TabController.Instance.GetTabsByModuleID(moduleId).Values)
            {
                string cacheKey = String.Format(DataCache.ModuleSettingsCacheKey, tab.TabID);
                DataCache.RemoveCache(cacheKey);
            }
        }

        private void ClearTabModuleSettingsCache(int tabModuleId)
        {
            foreach (var tab in TabController.Instance.GetTabsByTabModuleID(tabModuleId).Values)
            {
                string cacheKey = String.Format(DataCache.TabModuleSettingsCacheKey, tab.TabID);
                DataCache.RemoveCache(cacheKey);
            }
        }

        private void CopyTabModuleSettingsInternal(ModuleInfo fromModule, ModuleInfo toModule)
        {
            //Copy each setting to the new TabModule instance
            foreach (DictionaryEntry setting in fromModule.TabModuleSettings)
            {
                UpdateTabModuleSetting(toModule.TabModuleID, Convert.ToString(setting.Key), Convert.ToString(setting.Value));
            }
        }

        private static ModuleInfo DeserializeModule(XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId, int moduleDefId)
        {
            //Create New Module
            var module = new ModuleInfo
            {
                PortalID = portalId,
                TabID = tabId,
                ModuleOrder = -1,
                ModuleTitle = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "title"),
                PaneName = XmlUtils.GetNodeValue(nodePane.CreateNavigator(), "name"),
                ModuleDefID = moduleDefId,
                CacheTime = XmlUtils.GetNodeValueInt(nodeModule, "cachetime"),
                CacheMethod = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "cachemethod"),
                Alignment = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "alignment"),
                IconFile = Globals.ImportFile(portalId, XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "iconfile")),
                AllTabs = XmlUtils.GetNodeValueBoolean(nodeModule, "alltabs"),
                CultureCode = XmlUtils.GetNodeValue(nodeModule, "cultureCode")
            };

            // Localization
            var oldGuid = XmlUtils.GetNodeValue(nodeModule, "defaultLanguageGuid");
            if (!string.IsNullOrEmpty(oldGuid))
            {
                // get new default module language guid
                if (ParsedLocalizedModuleGuid.ContainsKey(oldGuid))
                {
                    module.DefaultLanguageGuid = new Guid(ParsedLocalizedModuleGuid[oldGuid].ToString());
                }
            }

            switch (XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "visibility"))
            {
                case "Maximized":
                    module.Visibility = VisibilityState.Maximized;
                    break;
                case "Minimized":
                    module.Visibility = VisibilityState.Minimized;
                    break;
                case "None":
                    module.Visibility = VisibilityState.None;
                    break;
            }
            module.Color = XmlUtils.GetNodeValue(nodeModule, "color", "");
            module.Border = XmlUtils.GetNodeValue(nodeModule, "border", "");
            module.Header = XmlUtils.GetNodeValue(nodeModule, "header", "");
            module.Footer = XmlUtils.GetNodeValue(nodeModule, "footer", "");
            module.InheritViewPermissions = XmlUtils.GetNodeValueBoolean(nodeModule, "inheritviewpermissions", false);
            module.IsShareable = XmlUtils.GetNodeValueBoolean(nodeModule, "isshareable", true);
            module.IsShareableViewOnly = XmlUtils.GetNodeValueBoolean(nodeModule, "isshareableviewonly", true);
            module.StartDate = XmlUtils.GetNodeValueDate(nodeModule, "startdate", Null.NullDate);
            module.EndDate = XmlUtils.GetNodeValueDate(nodeModule, "enddate", Null.NullDate);
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeModule, "containersrc", "")))
            {
                module.ContainerSrc = XmlUtils.GetNodeValue(nodeModule, "containersrc", "");
            }
            module.DisplayTitle = XmlUtils.GetNodeValueBoolean(nodeModule, "displaytitle", true);
            module.DisplayPrint = XmlUtils.GetNodeValueBoolean(nodeModule, "displayprint", true);
            module.DisplaySyndicate = XmlUtils.GetNodeValueBoolean(nodeModule, "displaysyndicate", false);
            module.IsWebSlice = XmlUtils.GetNodeValueBoolean(nodeModule, "iswebslice", false);
            if (module.IsWebSlice)
            {
                module.WebSliceTitle = XmlUtils.GetNodeValue(nodeModule, "webslicetitle", module.ModuleTitle);
                module.WebSliceExpiryDate = XmlUtils.GetNodeValueDate(nodeModule, "websliceexpirydate", module.EndDate);
                module.WebSliceTTL = XmlUtils.GetNodeValueInt(nodeModule, "webslicettl", module.CacheTime / 60);
            }
            return module;
        }

        private static void DeserializeModulePermissions(XmlNodeList nodeModulePermissions, int portalId, ModuleInfo module)
        {
            var permissionController = new PermissionController();
            foreach (XmlNode node in nodeModulePermissions)
            {
                string permissionKey = XmlUtils.GetNodeValue(node.CreateNavigator(), "permissionkey");
                string permissionCode = XmlUtils.GetNodeValue(node.CreateNavigator(), "permissioncode");
                string roleName = XmlUtils.GetNodeValue(node.CreateNavigator(), "rolename");
                int roleID = int.MinValue;
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleID = Convert.ToInt32(Globals.glbRoleAllUsers);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleID = Convert.ToInt32(Globals.glbRoleUnauthUser);
                        break;
                    default:
                        var role = RoleController.Instance.GetRole(portalId, r => r.RoleName == roleName);
                        if (role != null)
                        {
                            roleID = role.RoleID;
                        }
                        break;
                }
                if (roleID != int.MinValue)
                {
                    int permissionID = -1;
                    ArrayList permissions = permissionController.GetPermissionByCodeAndKey(permissionCode, permissionKey);
                    for (int i = 0; i <= permissions.Count - 1; i++)
                    {
                        var permission = (PermissionInfo)permissions[i];
                        permissionID = permission.PermissionID;
                    }

                    //if role was found add, otherwise ignore
                    if (permissionID != -1)
                    {
                        var modulePermission = new ModulePermissionInfo
                        {
                            ModuleID = module.ModuleID,
                            PermissionID = permissionID,
                            RoleID = roleID,
                            AllowAccess = Convert.ToBoolean(XmlUtils.GetNodeValue(node.CreateNavigator(), "allowaccess"))
                        };

                        // do not add duplicate ModulePermissions
                        bool canAdd = !module.ModulePermissions.Cast<ModulePermissionInfo>()
                                                    .Any(mp => mp.ModuleID == modulePermission.ModuleID
                                                            && mp.PermissionID == modulePermission.PermissionID
                                                            && mp.RoleID == modulePermission.RoleID
                                                            && mp.UserID == modulePermission.UserID);
                        if (canAdd)
                        {
                            module.ModulePermissions.Add(modulePermission);
                        }
                    }
                }
            }
        }

        private static void DeserializeModuleSettings(XmlNodeList nodeModuleSettings, ModuleInfo objModule)
        {
            foreach (XmlNode moduleSettingNode in nodeModuleSettings)
            {
                string key = XmlUtils.GetNodeValue(moduleSettingNode.CreateNavigator(), "settingname");
                string value = XmlUtils.GetNodeValue(moduleSettingNode.CreateNavigator(), "settingvalue");
                objModule.ModuleSettings[key] = value;
            }
        }

        private static void DeserializeTabModuleSettings(XmlNodeList nodeTabModuleSettings, ModuleInfo objModule)
        {
            foreach (XmlNode tabModuleSettingNode in nodeTabModuleSettings)
            {
                string key = XmlUtils.GetNodeValue(tabModuleSettingNode.CreateNavigator(), "settingname");
                string value = XmlUtils.GetNodeValue(tabModuleSettingNode.CreateNavigator(), "settingvalue");
                objModule.TabModuleSettings[key] = value;
            }
        }

        private static bool FindModule(XmlNode nodeModule, int tabId, PortalTemplateModuleAction mergeTabs)
        {
            var modules = Instance.GetTabModules(tabId);

            bool moduleFound = false;
            string modTitle = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "title");
            if (mergeTabs == PortalTemplateModuleAction.Merge)
            {
                if (modules.Select(kvp => kvp.Value).Any(module => modTitle == module.ModuleTitle))
                {
                    moduleFound = true;
                }
            }
            return moduleFound;
        }

        private static void GetModuleContent(XmlNode nodeModule, int ModuleId, int TabId, int PortalId)
        {
            ModuleInfo module = Instance.GetModule(ModuleId, TabId, true);
            if (nodeModule != null)
            {
                // ReSharper disable PossibleNullReferenceException
                string version = nodeModule.SelectSingleNode("content").Attributes["version"].Value;
                string content = nodeModule.SelectSingleNode("content").InnerXml;
                content = content.Substring(9, content.Length - 12);
                if (!String.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && !String.IsNullOrEmpty(content))
                {
                    var portal = PortalController.Instance.GetPortal(PortalId);

                    content = HttpContext.Current.Server.HtmlDecode(content);

                    //Determine if the Module is copmpletely installed 
                    //(ie are we running in the same request that installed the module).
                    if (module.DesktopModule.SupportedFeatures == Null.NullInteger)
                    {
                        //save content in eventqueue for processing after an app restart,
                        //as modules Supported Features are not updated yet so we
                        //cannot determine if the module supports IsPortable								
                        EventMessageProcessor.CreateImportModuleMessage(module, content, version, portal.AdministratorId);
                    }
                    else
                    {
                        if (module.DesktopModule.IsPortable)
                        {
                            try
                            {
                                object businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                                var controller = businessController as IPortable;
                                if (controller != null)
                                {
                                    controller.ImportModule(module.ModuleID, content, version, portal.AdministratorId);
                                }
                            }
                            catch
                            {
                                //if there is an error then the type cannot be loaded at this time, so add to EventQueue
                                EventMessageProcessor.CreateImportModuleMessage(module, content, version, portal.AdministratorId);
                            }
                        }
                    }
                }
                // ReSharper restore PossibleNullReferenceException
            }
        }

        private static ModuleDefinitionInfo GetModuleDefinition(XmlNode nodeModule)
        {
            ModuleDefinitionInfo moduleDefinition = null;

            //Templates prior to v4.3.5 only have the <definition> node to define the Module Type
            //This <definition> node was populated with the DesktopModuleInfo.ModuleName property
            //Thus there is no mechanism to determine to which module definition the module belongs.
            //
            //Template from v4.3.5 on also have the <moduledefinition> element that is populated
            //with the ModuleDefinitionInfo.FriendlyName.  Therefore the module Instance identifies
            //which Module Definition it belongs to.

            //Get the DesktopModule defined by the <definition> element
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "definition"), Null.NullInteger);
            if (desktopModule != null)
            {
                //Get the moduleDefinition from the <moduledefinition> element
                string friendlyName = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "moduledefinition");
                if (string.IsNullOrEmpty(friendlyName))
                {
                    //Module is pre 4.3.5 so get the first Module Definition (at least it won't throw an error then)
                    foreach (ModuleDefinitionInfo md in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModule.DesktopModuleID).Values)
                    {
                        moduleDefinition = md;
                        break;
                    }
                }
                else
                {
                    //Module is 4.3.5 or later so get the Module Defeinition by its friendly name
                    moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName, desktopModule.DesktopModuleID);
                }
            }
            return moduleDefinition;
        }

        internal Hashtable GetModuleSettings(int moduleId, int tabId)
        {
            string cacheKey = String.Format(DataCache.ModuleSettingsCacheKey, tabId);

            var moduleSettings = CBO.GetCachedObject<Dictionary<int, Hashtable>>(new CacheItemArgs(cacheKey,
                                                                                    DataCache.ModuleCacheTimeOut,
                                                                                    DataCache.ModuleCachePriority),
                            c =>
                            {
                                var moduleSettingsDic = new Dictionary<int, Hashtable>();
                                IDataReader dr = DataProvider.Instance().GetModuleSettingsByTab(tabId);
                                while (dr.Read())
                                {
                                    int mId = dr.GetInt32(0);
                                    Hashtable settings;
                                    if (!moduleSettingsDic.TryGetValue(mId, out settings))
                                    {
                                        settings = new Hashtable();
                                        moduleSettingsDic[mId] = settings;
                                    }

                                    if (!dr.IsDBNull(2))
                                    {
                                        settings[dr.GetString(1)] = dr.GetString(2);
                                    }
                                    else
                                    {
                                        settings[dr.GetString(1)] = "";
                                    }
                                }
                                CBO.CloseDataReader(dr, true);
                                return moduleSettingsDic;
                            });

            return moduleSettings.ContainsKey(moduleId) ? moduleSettings[moduleId] : new Hashtable();
        }

        internal Hashtable GetTabModuleSettings(int tabmoduleId, int tabId)
        {
            string cacheKey = String.Format(DataCache.TabModuleSettingsCacheKey, tabId);

            var tabModuleSettings = CBO.GetCachedObject<Dictionary<int, Hashtable>>(new CacheItemArgs(cacheKey,
                                                                                    DataCache.TabModuleCacheTimeOut,
                                                                                    DataCache.TabModuleCachePriority),
                            c =>
                            {
                                var tabModuleSettingsDic = new Dictionary<int, Hashtable>();
                                IDataReader dr = DataProvider.Instance().GetTabModuleSettingsByTab(tabId);
                                while (dr.Read())
                                {
                                    int tMId = dr.GetInt32(0);
                                    Hashtable settings;
                                    if (!tabModuleSettingsDic.TryGetValue(tMId, out settings))
                                    {
                                        settings = new Hashtable();
                                        tabModuleSettingsDic[tMId] = settings;
                                    }

                                    if (!dr.IsDBNull(2))
                                    {
                                        settings[dr.GetString(1)] = dr.GetString(2);
                                    }
                                    else
                                    {
                                        settings[dr.GetString(1)] = "";
                                    }
                                }
                                CBO.CloseDataReader(dr, true);
                                return tabModuleSettingsDic;
                            });

            return (tabModuleSettings.ContainsKey(tabmoduleId)) ? tabModuleSettings[tabmoduleId] : new Hashtable();
        }

        private int LocalizeModuleInternal(ModuleInfo sourceModule)
        {
            int moduleId = Null.NullInteger;

            if (sourceModule != null)
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                var newModule = sourceModule.Clone();
                newModule.ModuleID = Null.NullInteger;

                string translatorRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", sourceModule.CultureCode), sourceModule.PortalID, "").TrimEnd(';');

                //Add the default translators for this language, view and edit permissions
                var permissionController = new PermissionController();
                var viewPermissionsList = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
                var editPermissionsList = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "EDIT");
                PermissionInfo viewPermisison = null;
                PermissionInfo editPermisison = null;

                //View
                if (viewPermissionsList != null && viewPermissionsList.Count > 0)
                {
                    viewPermisison = (PermissionInfo)viewPermissionsList[0];
                }

                //Edit
                if (editPermissionsList != null && editPermissionsList.Count > 0)
                {
                    editPermisison = (PermissionInfo)editPermissionsList[0];
                }

                if (viewPermisison != null || editPermisison != null)
                {
                    foreach (string translatorRole in translatorRoles.Split(';'))
                    {
                        AddModulePermission(ref newModule, sourceModule.PortalID, translatorRole, viewPermisison, "VIEW");
                        AddModulePermission(ref newModule, sourceModule.PortalID, translatorRole, editPermisison, "EDIT");
                    }
                }

                //Add Module
                AddModuleInternal(newModule);

                //copy module settings
                Hashtable settings = GetModuleSettings(sourceModule.ModuleID, sourceModule.TabID);

                //Copy each setting to the new TabModule instance
                foreach (DictionaryEntry setting in settings)
                {
                    UpdateModuleSetting(newModule.ModuleID, Convert.ToString(setting.Key), Convert.ToString(setting.Value));
                }

                var currentUser = UserController.Instance.GetCurrentUserInfo();

                // update tabmodule
                dataProvider.UpdateTabModule(newModule.TabModuleID,
                                             newModule.TabID,
                                             newModule.ModuleID,
                                             newModule.ModuleTitle,
                                             newModule.Header,
                                             newModule.Footer,
                                             newModule.ModuleOrder,
                                             newModule.PaneName,
                                             newModule.CacheTime,
                                             newModule.CacheMethod,
                                             newModule.Alignment,
                                             newModule.Color,
                                             newModule.Border,
                                             newModule.IconFile,
                                             (int)newModule.Visibility,
                                             newModule.ContainerSrc,
                                             newModule.DisplayTitle,
                                             newModule.DisplayPrint,
                                             newModule.DisplaySyndicate,
                                             newModule.IsWebSlice,
                                             newModule.WebSliceTitle,
                                             newModule.WebSliceExpiryDate,
                                             newModule.WebSliceTTL,
                                             newModule.VersionGuid,
                                             newModule.DefaultLanguageGuid,
                                             newModule.LocalizedVersionGuid,
                                             newModule.CultureCode,
                                             currentUser.UserID);

                if (!string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass))
                {
                    try
                    {
                        object businessController = Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass);
                        var portableModule = businessController as IPortable;
                        if (portableModule != null)
                        {
                            string moduleContent = portableModule.ExportModule(sourceModule.ModuleID);
                            if (!string.IsNullOrEmpty(moduleContent))
                            {
                                portableModule.ImportModule(newModule.ModuleID, moduleContent, newModule.DesktopModule.Version, currentUser.UserID);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }
                }

                moduleId = newModule.ModuleID;

                //Clear Caches
                ClearCache(newModule.TabID);
                ClearCache(sourceModule.TabID);
            }

            return moduleId;
        }

        private static Hashtable ParsedLocalizedModuleGuid
        {
            get
            {
                if (HttpContext.Current.Items["ParsedLocalizedModuleGuid"] == null)
                    HttpContext.Current.Items["ParsedLocalizedModuleGuid"] = new Hashtable();

                return (Hashtable)HttpContext.Current.Items["ParsedLocalizedModuleGuid"];
            }
        }

        private void UpdateModuleSettingInternal(int moduleId, string settingName, string settingValue, bool updateVersion)
        {
            IDataReader dr = null;
            try
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                dr = dataProvider.GetModuleSetting(moduleId, settingName);

	            var settingExist = false;
	            string existValue = null;
                if (dr.Read())
                {
					settingExist = true;
	                existValue = dr.GetString(0);
                }

				dr.Close();

				if (settingExist && existValue != settingValue)
	            {
					dataProvider.UpdateModuleSetting(moduleId, settingName, settingValue, currentUser.UserID);
					EventLogController.AddSettingLog(EventLogController.EventLogType.MODULE_SETTING_UPDATED,
														"ModuleId", moduleId, settingName, settingValue,
														currentUser.UserID);
				}
				else if (!settingExist)
				{
					dataProvider.AddModuleSetting(moduleId, settingName, settingValue, currentUser.UserID);
					EventLogController.AddSettingLog(EventLogController.EventLogType.MODULE_SETTING_CREATED,
													"ModuleId", moduleId, settingName, settingValue,
													currentUser.UserID);
				}

                if (updateVersion)
                {
                    UpdateTabModuleVersionsByModuleID(moduleId);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                // Ensure DataReader is closed
	            if (dr != null && !dr.IsClosed)
	            {
		            CBO.CloseDataReader(dr, true);
	            }
            }

            ClearModuleSettingsCache(moduleId);
        }

        private void UpdateModuleSettings(ModuleInfo updatedModule)
        {
            foreach (string key in updatedModule.ModuleSettings.Keys)
            {
                string sKey = key;
                UpdateModuleSettingInternal(updatedModule.ModuleID, sKey, Convert.ToString(updatedModule.ModuleSettings[sKey]), false);
            }
            UpdateTabModuleVersionsByModuleID(updatedModule.ModuleID);
        }

        private void UpdateTabModuleSettings(ModuleInfo updatedTabModule)
        {
            foreach (string sKey in updatedTabModule.TabModuleSettings.Keys)
            {
                UpdateTabModuleSetting(updatedTabModule.TabModuleID, sKey, Convert.ToString(updatedTabModule.TabModuleSettings[sKey]));
            }
        }

        private static void UpdateTabModuleVersion(int tabModuleId)
        {
            dataProvider.UpdateTabModuleVersion(tabModuleId, Guid.NewGuid());
        }

        private void UpdateTabModuleVersionsByModuleID(int moduleID)
        {
            // Update the version guid of each TabModule linked to the updated module
            foreach (ModuleInfo modInfo in GetAllTabsModulesByModuleID(moduleID))
            {
                ClearCache(modInfo.TabID);
            }
            dataProvider.UpdateTabModuleVersionByModule(moduleID);
        }

        private bool HasModuleOrderOrPaneChanged(ModuleInfo module)
        {
            var storedModuleInfo = GetTabModule(module.TabModuleID);
            return storedModuleInfo == null || storedModuleInfo.ModuleOrder != module.ModuleOrder || storedModuleInfo.PaneName != module.PaneName;
        }

        private void UncopyModule(int tabId, int moduleId, bool softDelete, int originalTabId)
        {
            ModuleInfo moduleInfo = GetModule(moduleId, tabId, false);            
            DeleteTabModuleInternal(moduleInfo, softDelete, true);
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
            TabChangeTracker.Instance.TrackModuleUncopy(moduleInfo, Null.NullInteger, originalTabId, userId);            
        }

        private void DeleteTabModuleInternal(ModuleInfo moduleInfo, bool softDelete, bool uncopy = false)
        {
            //save moduleinfo

            if (moduleInfo != null)
            {
                //delete the module instance for the tab
                dataProvider.DeleteTabModule(moduleInfo.TabID, moduleInfo.ModuleID, softDelete);
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.TABMODULE_DELETED.ToString() };
                log.LogProperties.Add(new LogDetailInfo("tabId", moduleInfo.TabID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("moduleId", moduleInfo.ModuleID.ToString(CultureInfo.InvariantCulture)));
                LogController.Instance.AddLog(log);

                //reorder all modules on tab
                if (!uncopy)
                {
                    UpdateTabModuleOrder(moduleInfo.TabID);
                    //ModuleRemove is only raised when doing a soft delete of the module
                    if (softDelete && ModuleRemoved != null)
                    { 
                        ModuleRemoved(null, new ModuleEventArgs { Module = moduleInfo });
                    }
                }

                //check if all modules instances have been deleted
                if (GetModule(moduleInfo.ModuleID, Null.NullInteger, true).TabID == Null.NullInteger)
                {
                    //hard delete the module
                    DeleteModule(moduleInfo.ModuleID);
                }

                ClearCache(moduleInfo.TabID);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if a ModuleInfo belongs to the referenced Tab or not
        /// </summary>
        /// <param name="module">A ModuleInfo object to be checked</param>
        /// <returns>True is TabId points to a different tab from initial Tab where the module was added. Otherwise, False</returns>
        public bool IsSharedModule(ModuleInfo module)
        {
            var contentController = Util.GetContentController();
            var content = contentController.GetContentItem(module.ContentItemId);
            return module.TabID != content.TabID;
        }

        /// <summary>
        /// Get the Tab ID corresponding to the initial Tab where the module was added
        /// </summary>
        /// <param name="module">A ModuleInfo object to be checked</param>
        /// <returns>The Tab Id from initial Tab where the module was added</returns>
        public int GetMasterTabId(ModuleInfo module)
        {
            var contentController = Util.GetContentController();
            var content = contentController.GetContentItem(module.ContentItemId);
            return content.TabID;
        }

        /// <summary>
        /// add a module to a page
        /// </summary>
        /// <param name="module">moduleInfo for the module to create</param>
        /// <returns>ID of the created module</returns>
        public int AddModule(ModuleInfo module)
        {
            //add module
            AddModuleInternal(module);

            var currentUser = UserController.Instance.GetCurrentUserInfo();

            //Lets see if the module already exists
            ModuleInfo tmpModule = GetModule(module.ModuleID, module.TabID, false);
            if (tmpModule != null)
            {
                //Module Exists already
                if (tmpModule.IsDeleted)
                {
                    var order = module.ModuleOrder;
                    var pane = module.PaneName;

                    //Restore Module
                    RestoreModule(module);

                    TabChangeTracker.Instance.TrackModuleAddition(module, 1, currentUser.UserID);

                    //Set Module Order as expected
                    UpdateModuleOrder(module.TabID, module.ModuleID, order, pane);
                    UpdateTabModuleOrder(module.TabID);
                }
            }
            else
            {
                //add tabmodule
                dataProvider.AddTabModule(module.TabID,
                                          module.ModuleID,
                                          module.ModuleTitle,
                                          module.Header,
                                          module.Footer,
                                          module.ModuleOrder,
                                          module.PaneName,
                                          module.CacheTime,
                                          module.CacheMethod,
                                          module.Alignment,
                                          module.Color,
                                          module.Border,
                                          module.IconFile,
                                          (int)module.Visibility,
                                          module.ContainerSrc,
                                          module.DisplayTitle,
                                          module.DisplayPrint,
                                          module.DisplaySyndicate,
                                          module.IsWebSlice,
                                          module.WebSliceTitle,
                                          module.WebSliceExpiryDate,
                                          module.WebSliceTTL,
                                          module.UniqueId,
                                          module.VersionGuid,
                                          module.DefaultLanguageGuid,
                                          module.LocalizedVersionGuid,
                                          module.CultureCode,
                                          currentUser.UserID);

                var log = new LogInfo
                {
                    LogTypeKey = EventLogController.EventLogType.TABMODULE_CREATED.ToString(),
                    LogPortalID = module.PortalID
                };
                log.LogProperties.Add(new LogDetailInfo("TabPath", module.ParentTab.TabPath));
                log.LogProperties.Add(new LogDetailInfo("Module Type", module.ModuleDefinition.FriendlyName));
                log.LogProperties.Add(new LogDetailInfo("TabId", module.TabID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("ModuleID", module.ModuleID.ToString(CultureInfo.InvariantCulture)));
                LogController.Instance.AddLog(log);

                TabChangeTracker.Instance.TrackModuleAddition(module, 1, currentUser.UserID);

                if (module.ModuleOrder == -1)
                {
                    //position module at bottom of pane
                    UpdateModuleOrder(module.TabID, module.ModuleID, module.ModuleOrder, module.PaneName);
                }
                else
                {
                    //position module in pane
                    UpdateTabModuleOrder(module.TabID);
                }
            }

            //Save ModuleSettings
            if (module.TabModuleID == -1)
            {
                if (tmpModule == null)
                {
                    tmpModule = GetModule(module.ModuleID, module.TabID, false);
                }
                module.TabModuleID = tmpModule.TabModuleID;
            }
            UpdateTabModuleSettings(module);

            ClearCache(module.TabID);
            return module.ModuleID;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        public void ClearCache(int tabId)
        {
            DataCache.ClearModuleCache(tabId);
        }

        /// <summary>
        /// Copies the module to a new page.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="toPaneName">Name of to pane.</param>
        /// <param name="includeSettings">if set to <c>true</c> include settings.</param>
        public void CopyModule(ModuleInfo sourceModule, TabInfo destinationTab, string toPaneName, bool includeSettings)
        {
            PortalInfo portal = PortalController.Instance.GetPortal(destinationTab.PortalID);

            //Clone Module
            ModuleInfo destinationModule = sourceModule.Clone();
            if (!String.IsNullOrEmpty(toPaneName))
            {
                destinationModule.PaneName = toPaneName;
            }

            destinationModule.TabID = destinationTab.TabID;

            //The new reference copy should have the same culture as the destination Tab
            destinationModule.UniqueId = Guid.NewGuid();
            destinationModule.CultureCode = destinationTab.CultureCode;
            destinationModule.VersionGuid = Guid.NewGuid();
            destinationModule.LocalizedVersionGuid = Guid.NewGuid();

            //Figure out the DefaultLanguage Guid
            if (!String.IsNullOrEmpty(sourceModule.CultureCode) && sourceModule.CultureCode == portal.DefaultLanguage && destinationModule.CultureCode != sourceModule.CultureCode &&
                !String.IsNullOrEmpty(destinationModule.CultureCode))
            {
                //Tab is localized so set Default language Guid reference
                destinationModule.DefaultLanguageGuid = sourceModule.UniqueId;
            }
            else if (!String.IsNullOrEmpty(sourceModule.CultureCode) && sourceModule.CultureCode != portal.DefaultLanguage && destinationModule.CultureCode != sourceModule.CultureCode &&
                     !String.IsNullOrEmpty(destinationModule.CultureCode))
            {
                // tab is localized, but the source is not in the default language (it was on a single culture page)
                // this wires up all the connections 
                sourceModule.DefaultLanguageGuid = destinationModule.UniqueId;
                UpdateModule(sourceModule);
            }
            else if (sourceModule.AllTabs && sourceModule.CultureCode != portal.DefaultLanguage)
            {
                if (sourceModule.DefaultLanguageModule != null && destinationTab.DefaultLanguageTab != null)
                {
                    ModuleInfo defaultLanguageModule = GetModule(sourceModule.DefaultLanguageModule.ModuleID, destinationTab.DefaultLanguageTab.TabID, false);

                    if (defaultLanguageModule != null)
                    {
                        destinationModule.DefaultLanguageGuid = defaultLanguageModule.UniqueId;
                    }
                }
            }

            //This will fail if the page already contains this module
            try
            {
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;
                //Add a copy of the module to the bottom of the Pane for the new Tab
                dataProvider.AddTabModule(destinationModule.TabID,
                                          destinationModule.ModuleID,
                                          destinationModule.ModuleTitle,
                                          destinationModule.Header,
                                          destinationModule.Footer,
                                          destinationModule.ModuleOrder,
                                          destinationModule.PaneName,
                                          destinationModule.CacheTime,
                                          destinationModule.CacheMethod,
                                          destinationModule.Alignment,
                                          destinationModule.Color,
                                          destinationModule.Border,
                                          destinationModule.IconFile,
                                          (int)destinationModule.Visibility,
                                          destinationModule.ContainerSrc,
                                          destinationModule.DisplayTitle,
                                          destinationModule.DisplayPrint,
                                          destinationModule.DisplaySyndicate,
                                          destinationModule.IsWebSlice,
                                          destinationModule.WebSliceTitle,
                                          destinationModule.WebSliceExpiryDate,
                                          destinationModule.WebSliceTTL,
                                          destinationModule.UniqueId,
                                          destinationModule.VersionGuid,
                                          destinationModule.DefaultLanguageGuid,
                                          destinationModule.LocalizedVersionGuid,
                                          destinationModule.CultureCode,
                                          userId);
                TabChangeTracker.Instance.TrackModuleCopy(destinationModule, 1, sourceModule.TabID, userId);

                //Optionally copy the TabModuleSettings
                if (includeSettings)
                {
                    CopyTabModuleSettingsInternal(sourceModule, destinationModule);
                }
            }
            catch (Exception exc)
            {
                // module already in the page, ignore error
                Logger.Error(exc);
            }

            ClearCache(sourceModule.TabID);
            ClearCache(destinationTab.TabID);

            //Optionally copy the TabModuleSettings
            if (includeSettings)
            {
                destinationModule = GetModule(destinationModule.ModuleID, destinationModule.TabID, false);
                CopyTabModuleSettingsInternal(sourceModule, destinationModule);
            }
        }

        /// <summary>
        /// Copies all modules in source page to a new page.
        /// </summary>
        /// <param name="sourceTab">The source tab.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="asReference">if set to <c>true</c> will use source module directly, else will create new module info by source module.</param>
        public void CopyModules(TabInfo sourceTab, TabInfo destinationTab, bool asReference)
        {
            CopyModules(sourceTab, destinationTab, asReference, false);
        }

        /// <summary>
        /// Copies all modules in source page to a new page.
        /// </summary>
        /// <param name="sourceTab">The source tab.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="asReference">if set to <c>true</c> will use source module directly, else will create new module info by source module.</param>
        /// <param name="includeAllTabsMobules">if set to <c>true</c> will include modules which shown on all pages, this is used when create localized copy.</param>
        public void CopyModules(TabInfo sourceTab, TabInfo destinationTab, bool asReference, bool includeAllTabsMobules)
        {
            foreach (KeyValuePair<int, ModuleInfo> kvp in GetTabModules(sourceTab.TabID))
            {
                ModuleInfo sourceModule = kvp.Value;

                // if the module shows on all pages does not need to be copied since it will
                // be already added to this page
                if ((includeAllTabsMobules || !sourceModule.AllTabs) && !sourceModule.IsDeleted)
                {
                    if (!asReference)
                    {
                        //Deep Copy
                        var newModule = sourceModule.Clone();
                        newModule.ModuleID = Null.NullInteger;
                        newModule.TabID = destinationTab.TabID;
                        AddModule(sourceModule);
                    }
                    else
                    {
                        //Shallow (Reference Copy)
                        CopyModule(sourceModule, destinationTab, Null.NullString, true);
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name = "module"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vnguyen]   20110-05-10   Modified: Added update tabmodule versionguids
        /// </history>
        public void CreateContentItem(ModuleInfo module)
        {
            IContentTypeController typeController = new ContentTypeController();
            ContentType contentType = (from t in typeController.GetContentTypes()
                                       where t.ContentType == "Module"
                                       select t).SingleOrDefault();
            //This module does not have a valid ContentItem
            //create ContentItem
            IContentController contentController = Util.GetContentController();
            module.Content = module.ModuleTitle;
            module.Indexed = false;
            if (contentType != null)
            {
                module.ContentTypeId = contentType.ContentTypeId;
            }
            module.ContentItemId = contentController.AddContentItem(module);
        }

        /// <summary>
        /// DeleteAllModules deletes all instances of a Module (from a collection), optionally excluding the
        ///	current instance, and optionally including deleting the Module itself.
        /// </summary>
        /// <remarks>
        ///	Note - the base module is not removed unless both the flags are set, indicating
        ///	to delete all instances AND to delete the Base Module
        /// </remarks>
        ///	<param name="moduleId">The Id of the module to copy</param>
        ///	<param name="tabId">The Id of the current tab</param>
        /// <param name="softDelete">A flag that determines whether the instance should be soft-deleted</param>
        ///	<param name="fromTabs">An ArrayList of TabItem objects</param>
        ///	<param name="includeCurrent">A flag to indicate whether to delete from the current tab
        ///		as identified ny tabId</param>
        ///	<param name="deleteBaseModule">A flag to indicate whether to delete the Module itself</param>
        public void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs, bool softDelete, bool includeCurrent, bool deleteBaseModule)
        {
            var moduleInfo = GetModule(moduleId, tabId, false); 

            //Iterate through collection deleting the module from each Tab (except the current)
            foreach (TabInfo objTab in fromTabs)
            {
                if (objTab.TabID != tabId || includeCurrent)
                {
                    UncopyModule(objTab.TabID, moduleId, softDelete, tabId); //uncopy existing modules                    
                }
            }
            //Optionally delete the Module
            if (includeCurrent && deleteBaseModule && !softDelete)
            {
                DeleteModule(moduleId);
                ClearCache(tabId);
            }
            else
            {
                ClearCache(tabId);

                //ModuleRemove is only raised when doing a soft delete of the module
                if (softDelete && ModuleRemoved != null)
                    ModuleRemoved(null, new ModuleEventArgs { Module = moduleInfo });
            }
        }

        /// <summary>
        /// Delete a module instance permanently from the database
        /// </summary>
        /// <param name="moduleId">ID of the module instance</param>
        public void DeleteModule(int moduleId)
        {
            //Get the module
            ModuleInfo module = GetModule(moduleId, Null.NullInteger, true);
            //Delete Module
            dataProvider.DeleteModule(moduleId);

            //Remove the Content Item
            if (module != null && module.ContentItemId > Null.NullInteger)
            {
                IContentController contentController = Util.GetContentController();
                contentController.DeleteContentItem(module);
            }

            //Log deletion
            EventLogController.Instance.AddLog("ModuleId", moduleId.ToString(CultureInfo.InvariantCulture), PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.MODULE_DELETED);

            // queue remove module from search index
            var document = new SearchDocumentToDelete
            {
                ModuleId = moduleId
            };

            DataProvider.Instance().AddSearchDeletedItems(document);

            if (ModuleDeleted != null)
                ModuleDeleted(null, new ModuleEventArgs { Module = module });
        }

        /// <summary>
        /// Delete a Setting of a module instance
        /// </summary>
        /// <param name="moduleId">ID of the affected module</param>
        /// <param name="settingName">Name of the setting to be deleted</param>
        /// <history>
        ///    [sleupold]   2007-09-24   documented
        ///    [vnguyen]    2010-05-10   Modified: Added update tab module version
        /// </history>
        public void DeleteModuleSetting(int moduleId, string settingName)
        {
            dataProvider.DeleteModuleSetting(moduleId, settingName);
            var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.MODULE_SETTING_DELETED.ToString()};
            log.LogProperties.Add(new LogDetailInfo("ModuleId", moduleId.ToString(CultureInfo.InvariantCulture)));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            LogController.Instance.AddLog(log);
            UpdateTabModuleVersionsByModuleID(moduleId);
            ClearModuleSettingsCache(moduleId);
        }

        /// <summary>
        /// Delete a module reference permanently from the database.
        /// if there are no other references, the module instance is deleted as well
        /// </summary>
        /// <param name="tabId">ID of the page</param>
        /// <param name="moduleId">ID of the module instance</param>
        /// <param name="softDelete">A flag that determines whether the instance should be soft-deleted</param>
        public void DeleteTabModule(int tabId, int moduleId, bool softDelete)
        {
            ModuleInfo moduleInfo = GetModule(moduleId, tabId, false);            
            DeleteTabModuleInternal(moduleInfo, softDelete);
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
            TabChangeTracker.Instance.TrackModuleDeletion(moduleInfo, Null.NullInteger, userId);
        }

        /// <summary>
        /// Delete a specific setting of a tabmodule reference
        /// </summary>
        /// <param name="tabModuleId">ID of the affected tabmodule</param>
        /// <param name="settingName">Name of the setting to remove</param>
        public void DeleteTabModuleSetting(int tabModuleId, string settingName)
        {
            dataProvider.DeleteTabModuleSetting(tabModuleId, settingName);
            UpdateTabModuleVersion(tabModuleId);
            var log = new LogInfo
            {
                LogTypeKey = EventLogController.EventLogType.TABMODULE_SETTING_DELETED.ToString()
            };
            log.LogProperties.Add(new LogDetailInfo("TabModuleId", tabModuleId.ToString(CultureInfo.InvariantCulture)));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            LogController.Instance.AddLog(log);
            ClearTabModuleSettingsCache(tabModuleId);
        }

        /// <summary>
        /// Des the localize module.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <returns>new module id</returns>
        public int DeLocalizeModule(ModuleInfo sourceModule)
        {
            int moduleId = Null.NullInteger;

            if (sourceModule != null && sourceModule.DefaultLanguageModule != null)
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo newModule = sourceModule.Clone();

                //Get the Module ID of the default language instance
                newModule.ModuleID = sourceModule.DefaultLanguageModule.ModuleID;

                if (newModule.ModuleID != sourceModule.ModuleID)
                {
                    // update tabmodule
                    dataProvider.UpdateTabModule(newModule.TabModuleID,
                                                 newModule.TabID,
                                                 newModule.ModuleID,
                                                 newModule.ModuleTitle,
                                                 newModule.Header,
                                                 newModule.Footer,
                                                 newModule.ModuleOrder,
                                                 newModule.PaneName,
                                                 newModule.CacheTime,
                                                 newModule.CacheMethod,
                                                 newModule.Alignment,
                                                 newModule.Color,
                                                 newModule.Border,
                                                 newModule.IconFile,
                                                 (int)newModule.Visibility,
                                                 newModule.ContainerSrc,
                                                 newModule.DisplayTitle,
                                                 newModule.DisplayPrint,
                                                 newModule.DisplaySyndicate,
                                                 newModule.IsWebSlice,
                                                 newModule.WebSliceTitle,
                                                 newModule.WebSliceExpiryDate,
                                                 newModule.WebSliceTTL,
                                                 newModule.VersionGuid,
                                                 newModule.DefaultLanguageGuid,
                                                 newModule.LocalizedVersionGuid,
                                                 newModule.CultureCode,
                                                 UserController.Instance.GetCurrentUserInfo().UserID);

                    //check if all modules instances have been deleted
                    if (GetModule(sourceModule.ModuleID, Null.NullInteger, true).TabID == Null.NullInteger)
                    {
                        //delete the deep copy "module info"
                        DeleteModule(sourceModule.ModuleID);
                    }
                }

                moduleId = newModule.ModuleID;

                //Clear Caches
                ClearCache(newModule.TabID);
                ClearCache(sourceModule.TabID);
            }

            return moduleId;
        }

        /// <summary>
        /// get info of all modules in any portal of the installation
        /// </summary>
        /// <returns>moduleInfo of all modules</returns>
        /// <remarks>created for upgrade purposes</remarks>
        public ArrayList GetAllModules()
        {
            return CBO.FillCollection(dataProvider.GetAllModules(), typeof(ModuleInfo));
        }

        /// <summary>
        /// get Module objects of a portal, either only those, to be placed on all tabs or not
        /// </summary>
        /// <param name="portalID">ID of the portal</param>
        /// <param name="allTabs">specify, whether to return modules to be shown on all tabs or those to be shown on specified tabs</param>
        /// <returns>ArrayList of TabModuleInfo objects</returns>
        public ArrayList GetAllTabsModules(int portalID, bool allTabs)
        {
            return CBO.FillCollection(dataProvider.GetAllTabsModules(portalID, allTabs), typeof(ModuleInfo));
        }

        /// <summary>
        ///   get TabModule objects that are linked to a particular ModuleID
        /// </summary>
        /// <param name = "moduleID">ID of the module</param>
        /// <returns>ArrayList of TabModuleInfo objects</returns>
        public ArrayList GetAllTabsModulesByModuleID(int moduleID)
        {
            return CBO.FillCollection(dataProvider.GetAllTabsModulesByModuleID(moduleID), typeof(ModuleInfo));
        }

        /// <summary>
        /// get a Module object
        /// </summary>
        /// <param name="moduleID">ID of the module</param>
        /// <returns>ModuleInfo object</returns>
        /// <remarks>This overload ignores any cached values and always retrieves the latest data
        /// from the database.</remarks>
        public ModuleInfo GetModule(int moduleID)
        {
            return GetModule(moduleID, Null.NullInteger, true);
        }

        /// <summary>
        /// get a Module object
        /// </summary>
        /// <param name="moduleID">ID of the module</param>
        /// <param name="tabID">ID of the page</param>
        /// <returns>ModuleInfo object</returns>
        public ModuleInfo GetModule(int moduleID, int tabID)
        {
            return GetModule(moduleID, tabID, false);
        }

        /// <summary>
        /// get a Module object
        /// </summary>
        /// <param name="moduleID">ID of the module</param>
        /// <param name="tabID">ID of the page</param>
        /// <param name="ignoreCache">flag, if data shall not be taken from cache</param>
        /// <returns>ModuleInfo object</returns>
        public ModuleInfo GetModule(int moduleID, int tabID, bool ignoreCache)
        {
            ModuleInfo modInfo = null;
            bool bFound = false;
            if (!ignoreCache)
            {
                //First try the cache
                var dicModules = GetTabModules(tabID);
                bFound = dicModules.TryGetValue(moduleID, out modInfo);
            }
            if (ignoreCache || !bFound)
            {
                modInfo = CBO.FillObject<ModuleInfo>(dataProvider.GetModule(moduleID, tabID));
            }
            return modInfo;
        }

        /// <summary>
        ///   get Module by specific locale
        /// </summary>
        /// <param name = "ModuleId">ID of the module</param>
        /// <param name = "tabid">ID of the tab</param>
        /// <param name = "portalId">ID of the portal</param>
        /// <param name = "locale">The wanted locale</param>
        /// <returns>ModuleInfo associated to submitted locale</returns>
        public ModuleInfo GetModuleByCulture(int ModuleId, int tabid, int portalId, Locale locale)
        {
            ModuleInfo localizedModule = null;

            //Get Module specified by Id
            ModuleInfo originalModule = GetModule(ModuleId, tabid, false);

            if (locale != null && originalModule != null)
            {
                //Check if tab is in the requested culture
                if (string.IsNullOrEmpty(originalModule.CultureCode) || originalModule.CultureCode == locale.Code)
                {
                    localizedModule = originalModule;
                }
                else
                {
                    //See if tab exists for culture
                    if (originalModule.IsDefaultLanguage)
                    {
                        originalModule.LocalizedModules.TryGetValue(locale.Code, out localizedModule);
                    }
                    else
                    {
                        if (originalModule.DefaultLanguageModule != null)
                        {
                            if (originalModule.DefaultLanguageModule.CultureCode == locale.Code)
                            {
                                localizedModule = originalModule.DefaultLanguageModule;
                            }
                            else
                            {
                                if (!originalModule.DefaultLanguageModule.LocalizedModules.TryGetValue(locale.Code, out localizedModule))
                                {
                                    localizedModule = originalModule.DefaultLanguageModule;
                                }
                            }
                        }
                    }
                }
            }
            return localizedModule;
        }

        /// <summary>
        /// Get ModuleInfo object of first module instance with a given friendly name of the module definition
        /// </summary>
        /// <param name="portalId">ID of the portal, where to look for the module</param>
        /// <param name="definitionName">The name of module definition (NOTE: this looks at <see cref="ModuleDefinitionInfo.DefinitionName"/>, not <see cref="ModuleDefinitionInfo.FriendlyName"/>)</param>
        /// <returns>ModuleInfo of first module instance</returns>
        /// <remarks>preferably used for admin and host modules</remarks>
        public ModuleInfo GetModuleByDefinition(int portalId, string definitionName)
        {
            //declare return object
            ModuleInfo module;

            //format cache key
            string key = string.Format(DataCache.ModuleCacheKey, portalId);

            //get module dictionary from cache
            var modules = DataCache.GetCache<Dictionary<string, ModuleInfo>>(key) ?? new Dictionary<string, ModuleInfo>();
            if (modules.ContainsKey(definitionName))
            {
                module = modules[definitionName];
            }
            else
            {
                //clone the dictionary so that we have a local copy
                var clonemodules = new Dictionary<string, ModuleInfo>();
                foreach (ModuleInfo m in modules.Values)
                {
                    clonemodules[m.ModuleDefinition.DefinitionName] = m;
                }
                //get from database
                IDataReader dr = DataProvider.Instance().GetModuleByDefinition(portalId, definitionName);
                try
                {
                    //hydrate object
                    module = CBO.FillObject<ModuleInfo>(dr);
                }
                finally
                {
                    //close connection
                    CBO.CloseDataReader(dr, true);
                }
                if (module != null)
                {
                    //add the module to the dictionary
                    clonemodules[module.ModuleDefinition.FriendlyName] = module;

                    //set module caching settings
                    Int32 timeOut = DataCache.ModuleCacheTimeOut * Convert.ToInt32(Host.Host.PerformanceSetting);

                    //cache module dictionary
                    if (timeOut > 0)
                    {
                        DataCache.SetCache(key, clonemodules, TimeSpan.FromMinutes(timeOut));
                    }
                }
            }
            return module;
        }

        /// <summary>
        ///   get a Module object
        /// </summary>
        /// <param name = "uniqueID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public ModuleInfo GetModuleByUniqueID(Guid uniqueID)
        {
            return CBO.FillObject<ModuleInfo>(dataProvider.GetModuleByUniqueID(uniqueID));
        }

        /// <summary>
        /// get all Module objects of a portal
        /// </summary>
        /// <param name="portalID">ID of the portal</param>
        /// <returns>ArrayList of ModuleInfo objects</returns>
        public ArrayList GetModules(int portalID)
        {
            return CBO.FillCollection(dataProvider.GetModules(portalID), typeof(ModuleInfo));
        }

        /// <summary>
        /// Gets the modules by definition.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="definitionName">The name of the module definition.</param>
        /// <returns>module collection</returns>
        public ArrayList GetModulesByDefinition(int portalID, string definitionName)
        {
            return CBO.FillCollection(DataProvider.Instance().GetModuleByDefinition(portalID, definitionName), typeof(ModuleInfo));
        }

        /// <summary>
        /// For a portal get a list of all active module and tabmodule references that are Searchable
        /// either by inheriting from ModuleSearchBase or implementing the older ISearchable interface.
        /// </summary>
        /// <param name="portalID">ID of the portal to be searched</param>
        /// <returns>Arraylist of ModuleInfo for modules supporting search.</returns>
        public ArrayList GetSearchModules(int portalID)
        {
            return CBO.FillCollection(dataProvider.GetSearchModules(portalID), typeof(ModuleInfo));
        }

        /// <summary>
        ///   get a Module object
        /// </summary>
        /// <param name = "tabModuleID">ID of the tabmodule</param>
        /// <returns>An ModuleInfo object</returns>
        public ModuleInfo GetTabModule(int tabModuleID)
        {
            return CBO.FillObject<ModuleInfo>(dataProvider.GetTabModule(tabModuleID));
        }

        /// <summary>
        /// Get all Module references on a tab
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns>Dictionary of ModuleID and ModuleInfo</returns>
        public Dictionary<int, ModuleInfo> GetTabModules(int tabId)
        {
            string cacheKey = string.Format(DataCache.TabModuleCacheKey, tabId);
            return CBO.GetCachedObject<Dictionary<int, ModuleInfo>>(new CacheItemArgs(cacheKey,
                                                                            DataCache.TabModuleCacheTimeOut,
                                                                            DataCache.TabModuleCachePriority),
                                                                    c => GetModulesCurrentPage(tabId));
        }

        private Dictionary<int, ModuleInfo> GetModulesCurrentPage(int tabId)
        {
            var modules = CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            
            var dictionary = new Dictionary<int, ModuleInfo>();
            foreach (var module in modules)
            {
                dictionary[module.ModuleID] = module;
            }
            return dictionary;
        }

        /// <summary>
        ///   Get a list of all TabModule references of a module instance
        /// </summary>
        /// <param name = "moduleID">ID of the Module</param>
        /// <returns>ArrayList of ModuleInfo</returns>
        public IList<ModuleInfo> GetTabModulesByModule(int moduleID)
        {
            return CBO.FillCollection<ModuleInfo>(dataProvider.GetModule(moduleID, Null.NullInteger));
        }

        public void InitialModulePermission(ModuleInfo module, int tabId, int permissionType)
        {
            var tabPermissions = TabPermissionController.GetTabPermissions(tabId, module.PortalID);
            var permissionController = new PermissionController();

            module.InheritViewPermissions = permissionType == 0;

            // get the default module view permissions
            ArrayList systemModuleViewPermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");

            // get the permissions from the page
            foreach (TabPermissionInfo tabPermission in tabPermissions)
            {
                if (tabPermission.PermissionKey == "VIEW" && permissionType == 0)
                {
                    //Don't need to explicitly add View permisisons if "Same As Page"
                    continue;
                }

                // get the system module permissions for the permissionkey
                ArrayList systemModulePermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", tabPermission.PermissionKey);
                // loop through the system module permissions
                int j;
                for (j = 0; j <= systemModulePermissions.Count - 1; j++)
                {
                    // create the module permission
                    var systemModulePermission = (PermissionInfo)systemModulePermissions[j];
                    if (systemModulePermission.PermissionKey == "VIEW" && permissionType == 1 && tabPermission.PermissionKey != "EDIT")
                    {
                        //Only Page Editors get View permissions if "Page Editors Only"
                        continue;
                    }

                    ModulePermissionInfo modulePermission = AddModulePermission(module, systemModulePermission, tabPermission.RoleID, tabPermission.UserID, tabPermission.AllowAccess);

                    // ensure that every EDIT permission which allows access also provides VIEW permission
                    if (modulePermission.PermissionKey == "EDIT" && modulePermission.AllowAccess)
                    {
                        AddModulePermission(module, (PermissionInfo)systemModuleViewPermissions[0], modulePermission.RoleID, modulePermission.UserID, true);
                    }
                }

                //Get the custom Module Permissions,  Assume that roles with Edit Tab Permissions
                //are automatically assigned to the Custom Module Permissions
                if (tabPermission.PermissionKey == "EDIT")
                {
                    ArrayList customModulePermissions = permissionController.GetPermissionsByModuleDefID(module.ModuleDefID);

                    // loop through the custom module permissions
                    for (j = 0; j <= customModulePermissions.Count - 1; j++)
                    {
                        // create the module permission
                        var customModulePermission = (PermissionInfo)customModulePermissions[j];

                        AddModulePermission(module, customModulePermission, tabPermission.RoleID, tabPermission.UserID, tabPermission.AllowAccess);
                    }
                }
            }
        }

        public void LocalizeModule(ModuleInfo sourceModule, Locale locale)
        {
            try
            {
                // we could be working from a single culture page that is not in the default language, 
                // so we need to test whether or not the module is going to be localized for the default locale
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(sourceModule.PortalID);
                ModuleInfo defaultModule = locale.Code == defaultLocale.Code ? sourceModule : sourceModule.DefaultLanguageModule;

                if (defaultModule != null)
                {
                    ModuleInfo localizedModule;
                    var alreadyLocalized = defaultModule.LocalizedModules.TryGetValue(locale.Code, out localizedModule)
                                            && localizedModule.ModuleID != defaultModule.ModuleID;
                    var tabModules = GetTabModulesByModule(defaultModule.ModuleID);
                    if (tabModules.Count > 1)
                    {
                        //default language version is a reference copy

                        //Localize first tabModule
                        var newModuleId = alreadyLocalized ? localizedModule.ModuleID : LocalizeModuleInternal(sourceModule);

                        //Update Reference Copies
                        foreach (ModuleInfo tm in tabModules)
                        {
                            if (tm.IsDefaultLanguage)
                            {
                                ModuleInfo localModule;
                                if (tm.LocalizedModules.TryGetValue(locale.Code, out localModule))
                                {
                                    localModule.ModuleID = newModuleId;
                                    UpdateModule(localModule);
                                }
                            }
                        }
                    }
                    else if (!alreadyLocalized)
                    {
                        LocalizeModuleInternal(sourceModule);
				    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error localizing module, moduleId: {0}, full exception: {1}", sourceModule.ModuleID, ex);
            }
        }

        /// <summary>
        /// MoveModule moes a Module from one Tab to another including all the
        ///	TabModule settings
        /// </summary>
        ///	<param name="moduleId">The Id of the module to move</param>
        ///	<param name="fromTabId">The Id of the source tab</param>
        ///	<param name="toTabId">The Id of the destination tab</param>
        ///	<param name="toPaneName">The name of the Pane on the destination tab where the module will end up</param>
        public void MoveModule(int moduleId, int fromTabId, int toTabId, string toPaneName)
        {
            //Move the module to the Tab
            dataProvider.MoveTabModule(fromTabId, moduleId, toTabId, toPaneName, UserController.Instance.GetCurrentUserInfo().UserID);

            //Update Module Order for source tab, also updates the tabmodule version guid
            UpdateTabModuleOrder(fromTabId);

            //Update Module Order for target tab, also updates the tabmodule version guid
            UpdateTabModuleOrder(toTabId);
        }

        /// <summary>
        /// Restores the module.
        /// </summary>
        /// <param name="objModule">The module.</param>
        public void RestoreModule(ModuleInfo objModule)
        {
            dataProvider.RestoreTabModule(objModule.TabID, objModule.ModuleID);
            ClearCache(objModule.TabID);
        }

        /// <summary>
        /// Update module settings and permissions in database from ModuleInfo
        /// </summary>
        /// <param name="module">ModuleInfo of the module to update</param>
        public void UpdateModule(ModuleInfo module)
        {
            //Update ContentItem If neccessary
            if (module.ContentItemId == Null.NullInteger && module.ModuleID != Null.NullInteger)
            {
                CreateContentItem(module);
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();

            //update module
            dataProvider.UpdateModule(module.ModuleID,
                                      module.ModuleDefID,
                                      module.ContentItemId,
                                      module.AllTabs,
                                      module.StartDate,
                                      module.EndDate,
                                      module.InheritViewPermissions,
                                      module.IsShareable,
                                      module.IsShareableViewOnly,
                                      module.IsDeleted,
                                      currentUser.UserID);

            //Update Tags
            ITermController termController = Util.GetTermController();
            termController.RemoveTermsFromContent(module);
            foreach (Term term in module.Terms)
            {
                termController.AddTermToContent(term, module);
            }

            EventLogController.Instance.AddLog(module, PortalController.Instance.GetCurrentPortalSettings(), currentUser.UserID, "", EventLogController.EventLogType.MODULE_UPDATED);

            //save module permissions
            ModulePermissionController.SaveModulePermissions(module);
            UpdateModuleSettings(module);
            module.VersionGuid = Guid.NewGuid();
            module.LocalizedVersionGuid = Guid.NewGuid();

            if (!Null.IsNull(module.TabID))
            {
                var hasModuleOrderOrPaneChanged = HasModuleOrderOrPaneChanged(module);

                //update tabmodule
                dataProvider.UpdateTabModule(module.TabModuleID,
                                             module.TabID,
                                             module.ModuleID,
                                             module.ModuleTitle,
                                             module.Header,
                                             module.Footer,
                                             module.ModuleOrder,
                                             module.PaneName,
                                             module.CacheTime,
                                             module.CacheMethod,
                                             module.Alignment,
                                             module.Color,
                                             module.Border,
                                             module.IconFile,
                                             (int)module.Visibility,
                                             module.ContainerSrc,
                                             module.DisplayTitle,
                                             module.DisplayPrint,
                                             module.DisplaySyndicate,
                                             module.IsWebSlice,
                                             module.WebSliceTitle,
                                             module.WebSliceExpiryDate,
                                             module.WebSliceTTL,
                                             module.VersionGuid,
                                             module.DefaultLanguageGuid,
                                             module.LocalizedVersionGuid,
                                             module.CultureCode,
                                             currentUser.UserID);

                EventLogController.Instance.AddLog(module, PortalController.Instance.GetCurrentPortalSettings(), currentUser.UserID, "", EventLogController.EventLogType.TABMODULE_UPDATED);

                if (hasModuleOrderOrPaneChanged)
                {
                    //update module order in pane
                    UpdateModuleOrder(module.TabID, module.ModuleID, module.ModuleOrder, module.PaneName);   
                }

                //set the default module
                if (PortalSettings.Current != null)
                {
                    if (module.IsDefaultModule)
                    {
                        if (module.ModuleID != PortalSettings.Current.DefaultModuleId)
                        {
                            //Update Setting
                            PortalController.UpdatePortalSetting(module.PortalID, "defaultmoduleid", module.ModuleID.ToString(CultureInfo.InvariantCulture));
                        }
                        if (module.TabID != PortalSettings.Current.DefaultTabId)
                        {
                            //Update Setting
                            PortalController.UpdatePortalSetting(module.PortalID, "defaulttabid", module.TabID.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        if (module.ModuleID == PortalSettings.Current.DefaultModuleId && module.TabID == PortalSettings.Current.DefaultTabId)
                        {
                            //Clear setting
                            PortalController.DeletePortalSetting(module.PortalID, "defaultmoduleid");
                            PortalController.DeletePortalSetting(module.PortalID, "defaulttabid");
                        }
                    }
                }
                //apply settings to all desktop modules in portal
                if (module.AllModules)
                {
                    foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(module.PortalID))
                    {
                        TabInfo tab = tabPair.Value;
                        foreach (KeyValuePair<int, ModuleInfo> modulePair in GetTabModules(tab.TabID))
                        {
                            var targetModule = modulePair.Value;
                            targetModule.VersionGuid = Guid.NewGuid();
                            targetModule.LocalizedVersionGuid = Guid.NewGuid();

                            dataProvider.UpdateTabModule(targetModule.TabModuleID,
                                                         targetModule.TabID,
                                                         targetModule.ModuleID,
                                                         targetModule.ModuleTitle,
                                                         targetModule.Header,
                                                         targetModule.Footer,
                                                         targetModule.ModuleOrder,
                                                         targetModule.PaneName,
                                                         targetModule.CacheTime,
                                                         targetModule.CacheMethod,
                                                         module.Alignment,
                                                         module.Color,
                                                         module.Border,
                                                         module.IconFile,
                                                         (int)module.Visibility,
                                                         module.ContainerSrc,
                                                         module.DisplayTitle,
                                                         module.DisplayPrint,
                                                         module.DisplaySyndicate,
                                                         module.IsWebSlice,
                                                         module.WebSliceTitle,
                                                         module.WebSliceExpiryDate,
                                                         module.WebSliceTTL,
                                                         targetModule.VersionGuid,
                                                         targetModule.DefaultLanguageGuid,
                                                         targetModule.LocalizedVersionGuid,
                                                         targetModule.CultureCode,
                                                         currentUser.UserID);

							ClearCache(targetModule.TabID);
                        }
                    }
                }
            }
            //Clear Cache for all TabModules
            foreach (ModuleInfo tabModule in GetTabModulesByModule(module.ModuleID))
            {
                ClearCache(tabModule.TabID);
            }

            if (ModuleUpdated != null)
                ModuleUpdated(null, new ModuleEventArgs { Module = module });
        }
        
        /// <summary>
        /// set/change the module position within a pane on a page
        /// </summary>
        /// <param name="tabId">ID of the page</param>
        /// <param name="moduleId">ID of the module on the page</param>
        /// <param name="moduleOrder">position within the controls list on page, -1 if to be added at the end</param>
        /// <param name="paneName">name of the pane, the module is placed in on the page</param>
        public void UpdateModuleOrder(int tabId, int moduleId, int moduleOrder, string paneName)
        {
            ModuleInfo moduleInfo = GetModule(moduleId, tabId, false);
            if (moduleInfo != null)
            {
                //adding a module to a new pane - places the module at the bottom of the pane 
                if (moduleOrder == -1)
                {
                    IDataReader dr = null;
                    try
                    {
                        dr = dataProvider.GetTabModuleOrder(tabId, paneName);
                        while (dr.Read())
                        {
                            moduleOrder = Convert.ToInt32(dr["ModuleOrder"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }
                    finally
                    {
                        CBO.CloseDataReader(dr, true);
                    }
                    moduleOrder += 2;
                }
                dataProvider.UpdateModuleOrder(tabId, moduleId, moduleOrder, paneName);
                TabChangeTracker.Instance.TrackModuleModification(GetModule(moduleId, tabId, true), Null.NullInteger, UserController.Instance.GetCurrentUserInfo().UserID);
                //clear cache
                ClearCache(tabId);
            }
        }

        /// <summary>
        /// Adds or updates a module's setting value
        /// </summary>
        /// <param name="moduleId">ID of the module, the setting belongs to</param>
        /// <param name="settingName">name of the setting property</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will remove the setting, if not preserveIfEmpty is true</remarks>
        public void UpdateModuleSetting(int moduleId, string settingName, string settingValue)
        {
            UpdateModuleSettingInternal(moduleId, settingName, settingValue, true);
        }

        /// <summary>
        /// set/change all module's positions within a page
        /// </summary>
        /// <param name="tabId">ID of the page</param>
        public void UpdateTabModuleOrder(int tabId)
        {
            IDataReader dr = dataProvider.GetTabPanes(tabId);
            try
            {
                while (dr.Read())
                {
                    int moduleCounter = 0;
                    IDataReader dr2 = dataProvider.GetTabModuleOrder(tabId, Convert.ToString(dr["PaneName"]));
                    try
                    {
                        while (dr2.Read())
                        {
                            moduleCounter += 1;

                            var moduleId = Convert.ToInt32(dr2["ModuleID"]);
                            var paneName = Convert.ToString(dr["PaneName"]);
                            var isDeleted = Convert.ToBoolean(dr2["IsDeleted"]);
                            var moduleOrder = (moduleCounter * 2) - 1;

                            dataProvider.UpdateModuleOrder(tabId, moduleId, moduleOrder, paneName);

                            if (!isDeleted)
                            {
                                var moduleInfo = GetModule(moduleId, tabId, true);
                                var userInfo = UserController.Instance.GetCurrentUserInfo();
                                TabChangeTracker.Instance.TrackModuleModification(moduleInfo, Null.NullInteger, userInfo.UserID);                                
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        Exceptions.LogException(ex2);
                    }
                    finally
                    {
                        CBO.CloseDataReader(dr2, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            //clear module cache
            ClearCache(tabId);
        }

        /// <summary>
        /// Adds or updates a module's setting value
        /// </summary>
        /// <param name="tabModuleId">ID of the tabmodule, the setting belongs to</param>
        /// <param name="settingName">name of the setting property</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will relove the setting</remarks>
        public void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue)
        {
            IDataReader dr = null;
            try
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                dr = dataProvider.GetTabModuleSetting(tabModuleId, settingName);
                if (dr.Read())
                {
                    if (dr.GetString(1) != settingValue)
                    {
                        dataProvider.UpdateTabModuleSetting(tabModuleId, settingName, settingValue, currentUser.UserID);
                        EventLogController.AddSettingLog(EventLogController.EventLogType.MODULE_SETTING_UPDATED,
                                                        "TabModuleId", tabModuleId, settingName, settingValue,
                                                        currentUser.UserID);
                    }
                }
                else
                {
                    dataProvider.AddTabModuleSetting(tabModuleId, settingName, settingValue, currentUser.UserID);
                    EventLogController.AddSettingLog(EventLogController.EventLogType.TABMODULE_SETTING_CREATED,
                                                    "TabModuleId", tabModuleId, settingName, settingValue,
                                                    currentUser.UserID);
                }
                UpdateTabModuleVersion(tabModuleId);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                //Ensure DataReader is closed
                CBO.CloseDataReader(dr, true);
            }
            ClearTabModuleSettingsCache(tabModuleId);
        }

        /// <summary>
        /// Updates the translation status.
        /// </summary>
        /// <param name="localizedModule">The localized module.</param>
        /// <param name="isTranslated">if set to <c>true</c> will mark the module as translated].</param>
        public void UpdateTranslationStatus(ModuleInfo localizedModule, bool isTranslated)
        {
            if (isTranslated && (localizedModule.DefaultLanguageModule != null))
            {
                localizedModule.LocalizedVersionGuid = localizedModule.DefaultLanguageModule.LocalizedVersionGuid;
            }
            else
            {
                localizedModule.LocalizedVersionGuid = Guid.NewGuid();
            }
            DataProvider.Instance().UpdateTabModuleTranslationStatus(localizedModule.TabModuleID, localizedModule.LocalizedVersionGuid, UserController.Instance.GetCurrentUserInfo().UserID);

            //Clear Tab Caches
            ClearCache(localizedModule.TabID);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Deserializes the module.
        /// </summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="module">ModuleInfo of current module</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        public static void DeserializeModule(XmlNode nodeModule, ModuleInfo module, int portalId, int tabId)
        {
            var moduleDefinition = GetModuleDefinition(nodeModule);

            // Create dummy pane node for private DeserializeModule method
            var docPane = new XmlDocument();
            docPane.LoadXml(String.Format("<pane><name>{0}</name></pane>", module.PaneName));

            // Create ModuleInfo of Xml
            ModuleInfo sourceModule = DeserializeModule(nodeModule, docPane.DocumentElement, portalId, tabId, moduleDefinition.ModuleDefID);

            // Copy properties from sourceModule to given (actual) module
            module.ModuleTitle = sourceModule.ModuleTitle;
            module.ModuleDefID = sourceModule.ModuleDefID;
            module.CacheTime = sourceModule.CacheTime;
            module.CacheMethod = sourceModule.CacheMethod;
            module.Alignment = sourceModule.Alignment;
            module.IconFile = sourceModule.IconFile;
            module.AllTabs = sourceModule.AllTabs;
            module.Visibility = sourceModule.Visibility;
            module.Color = sourceModule.Color;
            module.Border = sourceModule.Border;
            module.Header = sourceModule.Header;
            module.Footer = sourceModule.Footer;
            module.InheritViewPermissions = sourceModule.InheritViewPermissions;
            module.IsShareable = sourceModule.IsShareable;
            module.IsShareableViewOnly = sourceModule.IsShareableViewOnly;
            module.StartDate = sourceModule.StartDate;
            module.EndDate = sourceModule.EndDate;
            module.ContainerSrc = sourceModule.ContainerSrc;
            module.DisplayTitle = sourceModule.DisplayTitle;
            module.DisplayPrint = sourceModule.DisplayPrint;
            module.DisplaySyndicate = sourceModule.DisplaySyndicate;
            module.IsWebSlice = sourceModule.IsWebSlice;

            if (module.IsWebSlice)
            {
                module.WebSliceTitle = sourceModule.WebSliceTitle;
                module.WebSliceExpiryDate = sourceModule.WebSliceExpiryDate;
                module.WebSliceTTL = sourceModule.WebSliceTTL;
            }

            // DNN-24983 get culture from page
            var tabInfo = TabController.Instance.GetTab(tabId, portalId, false);
            if (tabInfo != null)
            {
                module.CultureCode = tabInfo.CultureCode;
            }

            // save changes
            Instance.UpdateModule(module);

            //deserialize Module's settings
            XmlNodeList nodeModuleSettings = nodeModule.SelectNodes("modulesettings/modulesetting");
            DeserializeModuleSettings(nodeModuleSettings, module);

            XmlNodeList nodeTabModuleSettings = nodeModule.SelectNodes("tabmodulesettings/tabmodulesetting");
            DeserializeTabModuleSettings(nodeTabModuleSettings, module);

            //deserialize Content (if included)
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "content")))
            {
                GetModuleContent(nodeModule, module.ModuleID, tabId, portalId);
            }

            //deserialize Permissions
            XmlNodeList nodeModulePermissions = nodeModule.SelectNodes("modulepermissions/permission");
            DeserializeModulePermissions(nodeModulePermissions, portalId, module);

            //Persist the permissions to the Data base
            ModulePermissionController.SaveModulePermissions(module);
        }

        /// <summary>
        /// Deserializes the module.
        /// </summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="nodePane">The node pane.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="hModules">The modules.</param>
        public static void DeserializeModule(XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId, PortalTemplateModuleAction mergeTabs, Hashtable hModules)
        {
            var moduleDefinition = GetModuleDefinition(nodeModule);
            //will be instance or module?
            int templateModuleID = XmlUtils.GetNodeValueInt(nodeModule, "moduleID");
            bool isInstance = CheckIsInstance(templateModuleID, hModules);
            if (moduleDefinition != null)
            {
                //If Mode is Merge Check if Module exists
                if (!FindModule(nodeModule, tabId, mergeTabs))
                {
                    ModuleInfo module = DeserializeModule(nodeModule, nodePane, portalId, tabId, moduleDefinition.ModuleDefID);
                    //if the module is marked as show on all tabs, then check whether the module is exist in current website and it also
                    //still marked as shown on all tabs, this action will make sure there is no duplicate modules created on new tab.
                    if (module.AllTabs)
                    {
                        var existModule = Instance.GetModule(templateModuleID, Null.NullInteger, false);
                        if (existModule != null && !existModule.IsDeleted && existModule.AllTabs && existModule.PortalID == portalId)
                        {
                            return;
                        }
                    }
                    //deserialize Module's settings
                    XmlNodeList nodeModuleSettings = nodeModule.SelectNodes("modulesettings/modulesetting");
                    DeserializeModuleSettings(nodeModuleSettings, module);
                    XmlNodeList nodeTabModuleSettings = nodeModule.SelectNodes("tabmodulesettings/tabmodulesetting");
                    DeserializeTabModuleSettings(nodeTabModuleSettings, module);

                    // DNN-24983 get culture from page
                    var tabInfo = TabController.Instance.GetTab(tabId, portalId, false);
                    if (tabInfo != null)
                    {
                        module.CultureCode = tabInfo.CultureCode;
                    }

                    int intModuleId;
                    if (!isInstance)
                    {
                        //Add new module
                        intModuleId = Instance.AddModule(module);
                        if (templateModuleID > 0)
                        {
                            hModules.Add(templateModuleID, intModuleId);
                        }
                    }
                    else
                    {
                        //Add instance
                        module.ModuleID = Convert.ToInt32(hModules[templateModuleID]);
                        intModuleId = Instance.AddModule(module);
                    }

                    // save localization info
                    string oldGuid = XmlUtils.GetNodeValue(nodeModule, "uniqueId");
                    if (!ParsedLocalizedModuleGuid.ContainsKey(oldGuid))
                        ParsedLocalizedModuleGuid.Add(oldGuid, module.UniqueId.ToString());

                    if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "content")) && !isInstance)
                    {
                        GetModuleContent(nodeModule, intModuleId, tabId, portalId);
                    }
                    //Process permissions only once
                    if (!isInstance && portalId != Null.NullInteger)
                    {
                        XmlNodeList nodeModulePermissions = nodeModule.SelectNodes("modulepermissions/permission");
                        DeserializeModulePermissions(nodeModulePermissions, portalId, module);

                        //Persist the permissions to the Data base
                        ModulePermissionController.SaveModulePermissions(module);
                    }
                }
            }
        }

        /// <summary>
        /// SerializeModule
        /// </summary>
        /// <param name="xmlModule">The Xml Document to use for the Module</param>
        /// <param name="module">The ModuleInfo object to serialize</param>
        /// <param name="includeContent">A flak that determines whether the content of the module is serialised.</param>
        public static XmlNode SerializeModule(XmlDocument xmlModule, ModuleInfo module, bool includeContent)
        {
            var serializer = new XmlSerializer(typeof(ModuleInfo));
            var sw = new StringWriter();
            serializer.Serialize(sw, module);
            xmlModule.LoadXml(sw.GetStringBuilder().ToString());
            XmlNode moduleNode = xmlModule.SelectSingleNode("module");
            if (moduleNode != null)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                if (moduleNode.Attributes != null)
                {
                    moduleNode.Attributes.Remove(moduleNode.Attributes["xmlns:xsd"]);
                    moduleNode.Attributes.Remove(moduleNode.Attributes["xmlns:xsi"]);
                }

                //remove unwanted elements
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("portalid"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("tabid"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("tabmoduleid"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("moduleorder"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("panename"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("isdeleted"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("versionGuid"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("localizedVersionGuid"));
                moduleNode.RemoveChild(moduleNode.SelectSingleNode("content"));

                // support for localized templates
                //moduleNode.RemoveChild(moduleNode.SelectSingleNode("uniqueId"));
                //moduleNode.RemoveChild(moduleNode.SelectSingleNode("defaultLanguageGuid"));
                //moduleNode.RemoveChild(moduleNode.SelectSingleNode("cultureCode"));                

                if (Null.IsNull(module.DefaultLanguageGuid))
                    moduleNode.RemoveChild(moduleNode.SelectSingleNode("defaultLanguageGuid"));

                var xmlNodeList = moduleNode.SelectNodes("modulepermissions/permission");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nodePermission in xmlNodeList)
                    {
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("modulepermissionid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("permissionid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("moduleid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("roleid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("userid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("username"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("displayname"));
                    }
                }
                if (includeContent)
                {
                    AddContent(moduleNode, module);
                }
                //serialize ModuleSettings and TabModuleSettings
                XmlUtils.SerializeHashtable(module.ModuleSettings, xmlModule, moduleNode, "modulesetting", "settingname", "settingvalue");
                XmlUtils.SerializeHashtable(module.TabModuleSettings, xmlModule, moduleNode, "tabmodulesetting", "settingname", "settingvalue");

                // ReSharper restore AssignNullToNotNullAttribute
            }
            XmlNode newNode = xmlModule.CreateElement("definition");
            ModuleDefinitionInfo objModuleDef = ModuleDefinitionController.GetModuleDefinitionByID(module.ModuleDefID);
            newNode.InnerText = DesktopModuleController.GetDesktopModule(objModuleDef.DesktopModuleID, module.PortalID).ModuleName;
            if (moduleNode != null)
            {
                moduleNode.AppendChild(newNode);
            }
            //Add Module Definition Info
            XmlNode definitionNode = xmlModule.CreateElement("moduledefinition");
            definitionNode.InnerText = objModuleDef.FriendlyName;
            if (moduleNode != null)
            {
                moduleNode.AppendChild(definitionNode);
            }
            return moduleNode;
        }

        /// <summary>
        /// Synchronizes the module content between cache and database.
        /// </summary>
        /// <param name="moduleID">The module ID.</param>
        public static void SynchronizeModule(int moduleID)
        {
            var modules = Instance.GetTabModulesByModule(moduleID);
            foreach (ModuleInfo module in modules)
            {
                Hashtable tabSettings = TabController.Instance.GetTabSettings(module.TabID);
                if (tabSettings["CacheProvider"] != null && tabSettings["CacheProvider"].ToString().Length > 0)
                {
                    var outputProvider = OutputCachingProvider.Instance(tabSettings["CacheProvider"].ToString());
                    if (outputProvider != null)
                    {
                        outputProvider.Remove(module.TabID);
                    }
                }

                if (module.CacheTime > 0)
                {
                    var moduleProvider = ModuleCachingProvider.Instance(module.GetEffectiveCacheMethod());
                    if (moduleProvider != null)
                    {
                        moduleProvider.Remove(module.TabModuleID);
                    }
                }

                //Synchronize module is called when a module needs to indicate that the content
                //has changed and the cache's should be refreshed.  So we can update the Version
                //and also the LastContentModificationDate
                UpdateTabModuleVersion(module.TabModuleID);
                dataProvider.UpdateModuleLastContentModifiedOnDate(module.ModuleID);

                //We should also indicate that the Transalation Status has changed
                if (PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", module.PortalID, false))
                {
                    Instance.UpdateTranslationStatus(module, false);
                }
                // and clear the cache
                Instance.ClearCache(module.TabID);
            }
        }


        #endregion
    }
}
