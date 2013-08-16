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
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Security.Permissions
{
    public class PermissionProvider
    {
        #region Private Members

        //Folder Permission Codes
        private const string AdminFolderPermissionCode = "WRITE";
        private const string AddFolderPermissionCode = "WRITE";
        private const string CopyFolderPermissionCode = "WRITE";
        private const string DeleteFolderPermissionCode = "WRITE";
        private const string ManageFolderPermissionCode = "WRITE";
        private const string ViewFolderPermissionCode = "READ";

        //Module Permission Codes
        private const string AdminModulePermissionCode = "EDIT";
        private const string ContentModulePermissionCode = "EDIT";
        private const string DeleteModulePermissionCode = "EDIT";
        private const string ExportModulePermissionCode = "EDIT";
        private const string ImportModulePermissionCode = "EDIT";
        private const string ManageModulePermissionCode = "EDIT";
        private const string ViewModulePermissionCode = "VIEW";

        //Page Permission Codes
        private const string AddPagePermissionCode = "EDIT";
        private const string AdminPagePermissionCode = "EDIT";
        private const string ContentPagePermissionCode = "EDIT";
        private const string CopyPagePermissionCode = "EDIT";
        private const string DeletePagePermissionCode = "EDIT";
        private const string ExportPagePermissionCode = "EDIT";
        private const string ImportPagePermissionCode = "EDIT";
        private const string ManagePagePermissionCode = "EDIT";
        private const string NavigatePagePermissionCode = "VIEW";
        private const string ViewPagePermissionCode = "VIEW";
        private readonly DataProvider dataProvider = DataProvider.Instance();

        #endregion

        #region Shared/Static Methods

        //return the provider
        public virtual string LocalResourceFile
        {
            get
            {
                return Localization.GlobalResourceFile;
            }
        }

        public static PermissionProvider Instance()
        {
            return ComponentFactory.GetComponent<PermissionProvider>();
        }

        #endregion

        #region Private Methods

        private object GetFolderPermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            var PortalID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = dataProvider.GetFolderPermissionsByPortal(PortalID);
            var dic = new Dictionary<string, FolderPermissionCollection>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var folderPermissionInfo = CBO.FillObject<FolderPermissionInfo>(dr, false);
                    string dictionaryKey = folderPermissionInfo.FolderPath;
                    if (string.IsNullOrEmpty(dictionaryKey))
                    {
                        dictionaryKey = "[PortalRoot]";
                    }

                    //add Folder Permission to dictionary
                    if (dic.ContainsKey(dictionaryKey))
                    {
                        //Add FolderPermission to FolderPermission Collection already in dictionary for FolderPath
                        dic[dictionaryKey].Add(folderPermissionInfo);
                    }
                    else
                    {
                        //Create new FolderPermission Collection for TabId
                        var collection = new FolderPermissionCollection {folderPermissionInfo};

                        //Add Permission to Collection

                        //Add Collection to Dictionary
                        dic.Add(dictionaryKey, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            return dic;
        }

        private Dictionary<string, FolderPermissionCollection> GetFolderPermissions(int PortalID)
        {
            string cacheKey = string.Format(DataCache.FolderPermissionCacheKey, PortalID);
            return
                CBO.GetCachedObject<Dictionary<string, FolderPermissionCollection>>(
                    new CacheItemArgs(cacheKey, DataCache.FolderPermissionCacheTimeOut, DataCache.FolderPermissionCachePriority, PortalID), GetFolderPermissionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModulePermissions gets a Dictionary of ModulePermissionCollections by
        /// Module.
        /// </summary>
        /// <param name="tabID">The ID of the tab</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private Dictionary<int, ModulePermissionCollection> GetModulePermissions(int tabID)
        {
            string cacheKey = string.Format(DataCache.ModulePermissionCacheKey, tabID);
            return CBO.GetCachedObject<Dictionary<int, ModulePermissionCollection>>(
                new CacheItemArgs(cacheKey, DataCache.ModulePermissionCacheTimeOut, DataCache.ModulePermissionCachePriority, tabID), GetModulePermissionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModulePermissionsCallBack gets a Dictionary of ModulePermissionCollections by
        /// Module from the the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private object GetModulePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            var tabID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = dataProvider.GetModulePermissionsByTabID(tabID);
            var dic = new Dictionary<int, ModulePermissionCollection>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var modulePermissionInfo = CBO.FillObject<ModulePermissionInfo>(dr, false);

                    //add Module Permission to dictionary
                    if (dic.ContainsKey(modulePermissionInfo.ModuleID))
                    {
                        dic[modulePermissionInfo.ModuleID].Add(modulePermissionInfo);
                    }
                    else
                    {
                        //Create new ModulePermission Collection for ModuleId
                        var collection = new ModulePermissionCollection {modulePermissionInfo};

                        //Add Permission to Collection

                        //Add Collection to Dictionary
                        dic.Add(modulePermissionInfo.ModuleID, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return dic;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetTabPermissions gets a Dictionary of TabPermissionCollections by
        /// Tab.
        /// </summary>
        /// <param name="portalID">The ID of the portal</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private Dictionary<int, TabPermissionCollection> GetTabPermissions(int portalID)
        {
            string cacheKey = string.Format(DataCache.TabPermissionCacheKey, portalID);
            return CBO.GetCachedObject<Dictionary<int, TabPermissionCollection>>(new CacheItemArgs(cacheKey, DataCache.TabPermissionCacheTimeOut, DataCache.TabPermissionCachePriority, portalID),
                                                                                 GetTabPermissionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetTabPermissionsCallBack gets a Dictionary of TabPermissionCollections by
        /// Tab from the the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private object GetTabPermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = dataProvider.GetTabPermissionsByPortal(portalID);
            var dic = new Dictionary<int, TabPermissionCollection>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var tabPermissionInfo = CBO.FillObject<TabPermissionInfo>(dr, false);

                    //add Tab Permission to dictionary
                    if (dic.ContainsKey(tabPermissionInfo.TabID))
                    {
                        //Add TabPermission to TabPermission Collection already in dictionary for TabId
                        dic[tabPermissionInfo.TabID].Add(tabPermissionInfo);
                    }
                    else
                    {
                        //Create new TabPermission Collection for TabId
                        var collection = new TabPermissionCollection {tabPermissionInfo};

                        //Add Collection to Dictionary
                        dic.Add(tabPermissionInfo.TabID, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return dic;
        }

        #endregion

        #region Public Methods

        #region FolderPermission Methods

        public virtual bool CanAdminFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AdminFolderPermissionCode));
        }

        public virtual bool CanAddFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AddFolderPermissionCode));
        }

        public virtual bool CanCopyFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(CopyFolderPermissionCode));
        }

        public virtual bool CanDeleteFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(DeleteFolderPermissionCode));
        }

        public virtual bool CanManageFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(ManageFolderPermissionCode));
        }

        public virtual bool CanViewFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(ViewFolderPermissionCode));
        }

        public virtual void DeleteFolderPermissionsByUser(UserInfo objUser)
        {
            dataProvider.DeleteFolderPermissionsByUserID(objUser.PortalID, objUser.UserID);
        }

        public virtual FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int PortalID, string Folder)
        {
            string dictionaryKey = Folder;
            if (string.IsNullOrEmpty(dictionaryKey))
            {
                dictionaryKey = "[PortalRoot]";
            }
            //Get the Portal FolderPermission Dictionary
            Dictionary<string, FolderPermissionCollection> dicFolderPermissions = GetFolderPermissions(PortalID);

            //Get the Collection from the Dictionary
            FolderPermissionCollection folderPermissions;
            bool bFound = dicFolderPermissions.TryGetValue(dictionaryKey, out folderPermissions);
            if (!bFound)
            {
                //Return empty collection
                folderPermissions = new FolderPermissionCollection();
            }
            return folderPermissions;
        }

        public virtual bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string PermissionKey)
        {
            return PortalSecurity.IsInRoles(objFolderPermissions.ToString(PermissionKey));
        }

        /// <summary>
        /// SaveFolderPermissions updates a Folder's permissions
        /// </summary>
        /// <param name="folder">The Folder to update</param>
        public virtual void SaveFolderPermissions(FolderInfo folder)
        {
            SaveFolderPermissions((IFolderInfo)folder);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveFolderPermissions updates a Folder's permissions
        /// </summary>
        /// <param name="folder">The Folder to update</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void SaveFolderPermissions(IFolderInfo folder)
        {
            if ((folder.FolderPermissions != null))
            {
                FolderPermissionCollection folderPermissions = GetFolderPermissionsCollectionByFolder(folder.PortalID, folder.FolderPath);

                //Ensure that if role/user has been given a permission that is not Read/Browse then they also need Read/Browse
                var permController = new PermissionController();
                ArrayList permArray = permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "READ");
                PermissionInfo readPerm = null;
                if (permArray.Count == 1)
                {
                    readPerm = permArray[0] as PermissionInfo;
                }

                PermissionInfo browsePerm = null;
                permArray = permController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "BROWSE");
                if (permArray.Count == 1)
                {
                    browsePerm = permArray[0] as PermissionInfo;
                }

                var additionalPermissions = new FolderPermissionCollection();
                foreach (FolderPermissionInfo folderPermission in folder.FolderPermissions)
                {
                    if (folderPermission.PermissionKey != "BROWSE" && folderPermission.PermissionKey != "READ")
                    {
                        //Try to add Read permission
                        var newFolderPerm = new FolderPermissionInfo(readPerm)
                                                {
                                                    FolderID = folderPermission.FolderID, 
                                                    RoleID = folderPermission.RoleID, 
                                                    UserID = folderPermission.UserID, 
                                                    AllowAccess = folderPermission.AllowAccess
                                                };

                        additionalPermissions.Add(newFolderPerm);

                        //Try to add Browse permission
                        newFolderPerm = new FolderPermissionInfo(browsePerm)
                                            {
                                                FolderID = folderPermission.FolderID, 
                                                RoleID = folderPermission.RoleID, 
                                                UserID = folderPermission.UserID, 
                                                AllowAccess = folderPermission.AllowAccess
                                            };

                        additionalPermissions.Add(newFolderPerm);
                    }
                }

                foreach (FolderPermissionInfo folderPermission in additionalPermissions)
                {
                    folder.FolderPermissions.Add(folderPermission, true);
                }

                if (!folderPermissions.CompareTo(folder.FolderPermissions))
                {
                    dataProvider.DeleteFolderPermissionsByFolderPath(folder.PortalID, folder.FolderPath);

                    foreach (FolderPermissionInfo folderPermission in folder.FolderPermissions)
                    {
                        dataProvider.AddFolderPermission(folder.FolderID,
                                                         folderPermission.PermissionID,
                                                         folderPermission.RoleID,
                                                         folderPermission.AllowAccess,
                                                         folderPermission.UserID,
                                                         UserController.GetCurrentUserInfo().UserID);
                    }
                }
            }
        }

        #endregion

        #region ModulePermission Methods

        public virtual bool CanAdminModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionCode));
        }

        public virtual bool CanDeleteModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(DeleteModulePermissionCode));
        }

        public virtual bool CanEditModuleContent(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ContentModulePermissionCode));
        }

        public virtual bool CanExportModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ExportModulePermissionCode));
        }

        public virtual bool CanImportModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ImportModulePermissionCode));
        }

        public virtual bool CanManageModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ManageModulePermissionCode));
        }

        public virtual bool CanViewModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ViewModulePermissionCode));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteModulePermissionsByUser deletes a user's Module Permission in the Database
        /// </summary>
        /// <param name="user">The user</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void DeleteModulePermissionsByUser(UserInfo user)
        {
            dataProvider.DeleteModulePermissionsByUserID(user.PortalID, user.UserID);
            DataCache.ClearModulePermissionsCachesByPortal(user.PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModulePermissions gets a ModulePermissionCollection
        /// </summary>
        /// <param name="moduleID">The ID of the module</param>
        /// <param name="tabID">The ID of the tab</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual ModulePermissionCollection GetModulePermissions(int moduleID, int tabID)
        {
            //Get the Tab ModulePermission Dictionary
            Dictionary<int, ModulePermissionCollection> dictionary = GetModulePermissions(tabID);

            //Get the Collection from the Dictionary
            ModulePermissionCollection modulePermissions;
            bool found = dictionary.TryGetValue(moduleID, out modulePermissions);
            if (!found)
            {
                //Return empty collection
                modulePermissions = new ModulePermissionCollection();
            }
            return modulePermissions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasModulePermission checks whether the current user has a specific Module Permission
        /// </summary>
        /// <param name="modulePermissions">The Permissions for the Module</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual bool HasModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
            return PortalSecurity.IsInRoles(modulePermissions.ToString(permissionKey));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveModulePermissions updates a Module's permissions
        /// </summary>
        /// <param name="module">The Module to update</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void SaveModulePermissions(ModuleInfo module)
        {
            if (module.ModulePermissions != null)
            {
                ModulePermissionCollection modulePermissions = ModulePermissionController.GetModulePermissions(module.ModuleID, module.TabID);
                if (!modulePermissions.CompareTo(module.ModulePermissions))
                {
                    dataProvider.DeleteModulePermissionsByModuleID(module.ModuleID, module.PortalID);

                    foreach (ModulePermissionInfo modulePermission in module.ModulePermissions)
                    {
                        if (!module.IsShared && module.InheritViewPermissions && modulePermission.PermissionKey == "VIEW")
                        {
                            dataProvider.DeleteModulePermission(modulePermission.ModulePermissionID);
                        }
                        else
                        {
                            dataProvider.AddModulePermission(module.ModuleID,
                                                             module.PortalID,
                                                             modulePermission.PermissionID,
                                                             modulePermission.RoleID,
                                                             modulePermission.AllowAccess,
                                                             modulePermission.UserID,
                                                             UserController.GetCurrentUserInfo().UserID);
                        }
                    }
                }
            }
        }

        #endregion

        #region TabPermission Methods

        public virtual bool CanAddContentToPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(ContentPagePermissionCode));
        }

        public virtual bool CanAddPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AddPagePermissionCode));
        }

        public virtual bool CanAdminPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AdminPagePermissionCode));
        }

        public virtual bool CanCopyPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(CopyPagePermissionCode));
        }

        public virtual bool CanDeletePage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(DeletePagePermissionCode));
        }

        public virtual bool CanExportPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(ExportPagePermissionCode));
        }

        public virtual bool CanImportPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(ImportPagePermissionCode));
        }

        public virtual bool CanManagePage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(ManagePagePermissionCode));
        }

        public virtual bool CanNavigateToPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(NavigatePagePermissionCode));
        }

        public virtual bool CanViewPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(ViewPagePermissionCode));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteTabPermissionsByUser deletes a user's Tab Permissions in the Database
        /// </summary>
        /// <param name="user">The user</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void DeleteTabPermissionsByUser(UserInfo user)
        {
            dataProvider.DeleteTabPermissionsByUserID(user.PortalID, user.UserID);
            DataCache.ClearTabPermissionsCache(user.PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetTabPermissions gets a TabPermissionCollection
        /// </summary>
        /// <param name="tabID">The ID of the tab</param>
        /// <param name="portalID">The ID of the portal</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual TabPermissionCollection GetTabPermissions(int tabID, int portalID)
        {
            //Get the Portal TabPermission Dictionary
            Dictionary<int, TabPermissionCollection> dicTabPermissions = GetTabPermissions(portalID);

            //Get the Collection from the Dictionary
            TabPermissionCollection tabPermissions;
            bool bFound = dicTabPermissions.TryGetValue(tabID, out tabPermissions);
            if (!bFound)
            {
                //Return empty collection
                tabPermissions = new TabPermissionCollection();
            }
            return tabPermissions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasTabPermission checks whether the current user has a specific Tab Permission
        /// </summary>
        /// <param name="tabPermissions">The Permissions for the Tab</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual bool HasTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
            return PortalSecurity.IsInRoles(tabPermissions.ToString(permissionKey));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveTabPermissions saves a Tab's permissions
        /// </summary>
        /// <param name="tab">The Tab to update</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void SaveTabPermissions(TabInfo tab)
        {
            TabPermissionCollection objCurrentTabPermissions = GetTabPermissions(tab.TabID, tab.PortalID);
            var objEventLog = new EventLogController();
            if (!objCurrentTabPermissions.CompareTo(tab.TabPermissions))
            {
                dataProvider.DeleteTabPermissionsByTabID(tab.TabID);
                objEventLog.AddLog(tab, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.TABPERMISSION_DELETED);
                if (tab.TabPermissions != null)
                {
                    foreach (TabPermissionInfo objTabPermission in tab.TabPermissions)
                    {
                        dataProvider.AddTabPermission(tab.TabID,
                                                      objTabPermission.PermissionID,
                                                      objTabPermission.RoleID,
                                                      objTabPermission.AllowAccess,
                                                      objTabPermission.UserID,
                                                      UserController.GetCurrentUserInfo().UserID);
                        objEventLog.AddLog(tab, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.TABPERMISSION_CREATED);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
