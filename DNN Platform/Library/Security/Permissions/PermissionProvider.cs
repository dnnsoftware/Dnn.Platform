#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cache;
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

        //Folder Permission Keys
        private const string AdminFolderPermissionKey = "WRITE";
        private const string AddFolderPermissionKey = "WRITE";
        private const string BrowseFolderPermissionKey = "BROWSE";
        private const string CopyFolderPermissionKey = "WRITE";
        private const string DeleteFolderPermissionKey = "WRITE";
        private const string ManageFolderPermissionKey = "WRITE";
        private const string ViewFolderPermissionKey = "READ";

        //Module Permission Keys
        private const string AdminModulePermissionKey = "EDIT";
        private const string ContentModulePermissionKey = "EDIT";
        private const string DeleteModulePermissionKey = "EDIT";
        private const string ExportModulePermissionKey = "EDIT";
        private const string ImportModulePermissionKey = "EDIT";
        private const string ManageModulePermissionKey = "EDIT";
        private const string ViewModulePermissionKey = "VIEW";

        //Page Permission Keys
        private const string AddPagePermissionKey = "EDIT";
        private const string AdminPagePermissionKey = "EDIT";
        private const string ContentPagePermissionKey = "EDIT";
        private const string CopyPagePermissionKey = "EDIT";
        private const string DeletePagePermissionKey = "EDIT";
        private const string ExportPagePermissionKey = "EDIT";
        private const string ImportPagePermissionKey = "EDIT";
        private const string ManagePagePermissionKey = "EDIT";
        private const string NavigatePagePermissionKey = "VIEW";
        private const string ViewPagePermissionKey = "VIEW";
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

        private static SharedDictionary<int, DNNCacheDependency> _cacheDependencyDict = new SharedDictionary<int, DNNCacheDependency>();

        private static DNNCacheDependency GetCacheDependency(int portalId)
        {
            DNNCacheDependency dependency;
            using (_cacheDependencyDict.GetReadLock())
            {
                _cacheDependencyDict.TryGetValue(portalId, out dependency);
            }

            if (dependency == null)
            {
                var startAt = DateTime.UtcNow;
                var cacheKey = string.Format(DataCache.FolderPermissionCacheKey, portalId);
                DataCache.SetCache(cacheKey, portalId); // no expiration set for this
                dependency = new DNNCacheDependency(null, new[] {cacheKey}, startAt);
                using (_cacheDependencyDict.GetWriteLock())
                {
                    _cacheDependencyDict[portalId] = dependency;
                }
            }
            return dependency;
        }

        internal static void ResetCacheDependency(int portalId, Action cacehClearAction)
        {
            // first execute the cache clear action then check the dependency change
            cacehClearAction.Invoke();
            DNNCacheDependency dependency;
            using (_cacheDependencyDict.GetReadLock())
            {
                _cacheDependencyDict.TryGetValue(portalId, out dependency);
            }
            if (dependency != null)
            {
                using (_cacheDependencyDict.GetWriteLock())
                {
                    _cacheDependencyDict.Remove(portalId);
                }
                dependency.Dispose();
            }
        }

        #endregion

        #region Private Methods

#if false
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
#endif

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModulePermissions gets a Dictionary of ModulePermissionCollections by
        /// Module.
        /// </summary>
        /// <param name="tabID">The ID of the tab</param>
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
        /// -----------------------------------------------------------------------------
        private object GetTabPermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            var dic = new Dictionary<int, TabPermissionCollection>();

            if (portalID > -1)
            {
                IDataReader dr = dataProvider.GetTabPermissionsByPortal(portalID);
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
                            var collection = new TabPermissionCollection { tabPermissionInfo };

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
            }
            return dic;
        }

        private bool HasFolderPermission(FolderInfo folder, string permissionKey)
        {
            if (folder == null) return false;
            return (PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AdminFolderPermissionKey)))
                   && !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(permissionKey));
            //Deny on Edit permission on folder shouldn't take away any other explicitly Allowed
            //&& !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        private bool HasPagePermission(TabInfo tab, string permissionKey)
        {
            return (PortalSecurity.IsInRoles(tab.TabPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AdminPagePermissionKey)))
                   && !PortalSecurity.IsDenied(tab.TabPermissions.ToString(permissionKey));
            //Deny on Edit permission on page shouldn't take away any other explicitly Allowed
            //&&!PortalSecurity.IsDenied(tab.TabPermissions.ToString(AdminPagePermissionKey));
        }

        private bool IsDeniedModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
            bool isDenied = Null.NullBoolean;
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsDenied(modulePermissions.ToString(permission)))
                    {
                        isDenied = true;
                        break;
                    }
                }
            }
            else
            {
                isDenied = PortalSecurity.IsDenied(modulePermissions.ToString(permissionKey));
            }
            return isDenied;
        }

        private bool IsDeniedTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
            bool isDenied = Null.NullBoolean;
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsDenied(tabPermissions.ToString(permission)))
                    {
                        isDenied = true;
                        break;
                    }
                }
            }
            else
            {
                isDenied = PortalSecurity.IsDenied(tabPermissions.ToString(permissionKey));
            }
            return isDenied;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermissions gets a Dictionary of DesktopModulePermissionCollections by
        /// DesktopModule.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, DesktopModulePermissionCollection> GetDesktopModulePermissions()
        {
            return CBO.GetCachedObject<Dictionary<int, DesktopModulePermissionCollection>>(
                new CacheItemArgs(DataCache.DesktopModulePermissionCacheKey, DataCache.DesktopModulePermissionCachePriority), GetDesktopModulePermissionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermissionsCallBack gets a Dictionary of DesktopModulePermissionCollections by
        /// DesktopModule from the the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call</param>
        /// -----------------------------------------------------------------------------
        private static object GetDesktopModulePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            return FillDesktopModulePermissionDictionary(DataProvider.Instance().GetDesktopModulePermissions());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDesktopModulePermissionDictionary fills a Dictionary of DesktopModulePermissions from a
        /// dataReader
        /// </summary>
        /// <param name="dr">The IDataReader</param>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, DesktopModulePermissionCollection> FillDesktopModulePermissionDictionary(IDataReader dr)
        {
            var dic = new Dictionary<int, DesktopModulePermissionCollection>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var desktopModulePermissionInfo = CBO.FillObject<DesktopModulePermissionInfo>(dr, false);

                    //add DesktopModule Permission to dictionary
                    if (dic.ContainsKey(desktopModulePermissionInfo.PortalDesktopModuleID))
                    {
                        //Add DesktopModulePermission to DesktopModulePermission Collection already in dictionary for TabId
                        dic[desktopModulePermissionInfo.PortalDesktopModuleID].Add(desktopModulePermissionInfo);
                    }
                    else
                    {
                        //Create new DesktopModulePermission Collection for DesktopModulePermissionID
                        var collection = new DesktopModulePermissionCollection { desktopModulePermissionInfo };

                        //Add Collection to Dictionary
                        dic.Add(desktopModulePermissionInfo.PortalDesktopModuleID, collection);
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

        private IEnumerable<RoleInfo> DefaultImplicitRoles(int portalId)
        {
            return new List<RoleInfo>
            {
                RoleController.Instance.GetRoleById(portalId,
                    PortalController.Instance.GetPortal(portalId).AdministratorRoleId)
            };
        } 

#endregion

        #region Protected Methods

        protected bool HasModulePermission(ModuleInfo moduleConfiguration, string permissionKey)
        {
            return CanViewModule(moduleConfiguration) &&
                                (HasModulePermission(moduleConfiguration.ModulePermissions, permissionKey)
                                 || HasModulePermission(moduleConfiguration.ModulePermissions, "EDIT"));
        }

        protected bool IsDeniedModulePermission(ModuleInfo moduleConfiguration, string permissionKey)
        {
            return IsDeniedModulePermission(moduleConfiguration.ModulePermissions, "VIEW")
                        || IsDeniedModulePermission(moduleConfiguration.ModulePermissions, permissionKey)
                        || IsDeniedModulePermission(moduleConfiguration.ModulePermissions, "EDIT");
        }

        protected bool IsDeniedTabPermission(TabInfo tab, string permissionKey)
        {
            return IsDeniedTabPermission(tab.TabPermissions, "VIEW")
                        || IsDeniedTabPermission(tab.TabPermissions, permissionKey)
                        || IsDeniedTabPermission(tab.TabPermissions, "EDIT");
        }

        #endregion

        #region Public Methods

        public virtual bool SupportsFullControl()
        {
            return true;
        }

        #region FolderPermission Methods

        /// <summary>
        /// Returns a flag indicating whether the current user can add a folder or file
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanAddFolder(FolderInfo folder)
        {
            return HasFolderPermission(folder, AddFolderPermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can browse the folder
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanBrowseFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return (PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(BrowseFolderPermissionKey))
                || PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(ViewFolderPermissionKey)))
                && !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(BrowseFolderPermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can addmister a folder
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanAdminFolder(FolderInfo folder)
        {
            if (folder == null) return false;
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can copy a folder or file
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanCopyFolder(FolderInfo folder)
        {
            return HasFolderPermission(folder, CopyFolderPermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can delete a folder or file
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanDeleteFolder(FolderInfo folder)
        {
            return HasFolderPermission(folder, DeleteFolderPermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can manage a folder's settings
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanManageFolder(FolderInfo folder)
        {
            return HasFolderPermission(folder, ManageFolderPermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can view a folder or file
        /// </summary>
        /// <param name="folder">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanViewFolder(FolderInfo folder)
        {
            return HasFolderPermission(folder, ViewFolderPermissionKey);
        }

        public virtual void DeleteFolderPermissionsByUser(UserInfo objUser)
        {
            dataProvider.DeleteFolderPermissionsByUserID(objUser.PortalID, objUser.UserID);
        }

        public virtual FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int PortalID, string Folder)
        {
#if fale
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
#else
            var cacheKey = string.Format(DataCache.FolderPathPermissionCacheKey, PortalID, Folder);
            return CBO.GetCachedObject<FolderPermissionCollection>(
                new CacheItemArgs(cacheKey, DataCache.FolderPermissionCacheTimeOut, DataCache.FolderPermissionCachePriority)
                {
                    CacheDependency = GetCacheDependency(PortalID)
                },
                _ =>
                {
                    var collection = new FolderPermissionCollection();
                    try
                    {
                        using (var dr = dataProvider.GetFolderPermissionsByPortalAndPath(PortalID, Folder))
                        {
                            while (dr.Read())
                            {
                                var folderPermissionInfo = CBO.FillObject<FolderPermissionInfo>(dr, false);
                                collection.Add(folderPermissionInfo);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Exceptions.LogException(exc);
                    }
                    return collection;
                });
#endif
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
        /// -----------------------------------------------------------------------------
        public virtual void SaveFolderPermissions(IFolderInfo folder)
        {
            if ((folder.FolderPermissions != null))
            {
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
                    if (folderPermission.PermissionKey != "BROWSE" && folderPermission.PermissionKey != "READ" && folderPermission.AllowAccess)
                    {
                        //Try to add Read permission
                        var newFolderPerm = new FolderPermissionInfo(readPerm)
                                                {
                                                    FolderID = folderPermission.FolderID, 
                                                    RoleID = folderPermission.RoleID, 
                                                    UserID = folderPermission.UserID, 
                                                    AllowAccess = true
                                                };

                        additionalPermissions.Add(newFolderPerm);

                        //Try to add Browse permission
                        newFolderPerm = new FolderPermissionInfo(browsePerm)
                                            {
                                                FolderID = folderPermission.FolderID, 
                                                RoleID = folderPermission.RoleID, 
                                                UserID = folderPermission.UserID, 
                                                AllowAccess = true
                                            };

                        additionalPermissions.Add(newFolderPerm);
                    }
                }

                foreach (FolderPermissionInfo folderPermission in additionalPermissions)
                {
                    folder.FolderPermissions.Add(folderPermission, true);
                }

                dataProvider.DeleteFolderPermissionsByFolderPath(folder.PortalID, folder.FolderPath);
                foreach (FolderPermissionInfo folderPermission in folder.FolderPermissions)
                {
                    dataProvider.AddFolderPermission(folder.FolderID,
                                                        folderPermission.PermissionID,
                                                        folderPermission.RoleID,
                                                        folderPermission.AllowAccess,
                                                        folderPermission.UserID,
                                                        UserController.Instance.GetCurrentUserInfo().UserID);
                }                
            }
        }

        #endregion

        #region ModulePermission Methods

        /// <summary>
        /// Returns a flag indicating whether the current user can administer a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanAdminModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can delete a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanDeleteModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(DeleteModulePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can edit module content
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanEditModuleContent(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ContentModulePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can export a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanExportModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ExportModulePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can import a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanImportModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ImportModulePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can manage a module's settings
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanManageModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ManageModulePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can view a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanViewModule(ModuleInfo module)
        {
            bool canView;
            if (module.InheritViewPermissions)
            {
                TabInfo objTab = TabController.Instance.GetTab(module.TabID, module.PortalID, false);
                canView = TabPermissionController.CanViewPage(objTab);
            }
            else
            {
                canView = PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ViewModulePermissionKey));
            }

            return canView;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteModulePermissionsByUser deletes a user's Module Permission in the Database
        /// </summary>
        /// <param name="user">The user</param>
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

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Determines if user has the necessary permissions to access an item with the
        /// designated AccessLevel.
        /// </summary>
        /// <param name="accessLevel">The SecurityAccessLevel required to access a portal module or module action.</param>
        /// <param name="permissionKey">If Security Access is Edit the permissionKey is the actual "edit" permisison required.</param>
        /// <param name="moduleConfiguration">The ModuleInfo object for the associated module.</param>
        /// <returns>A boolean value indicating if the user has the necessary permissions</returns>
        /// <remarks>Every module control and module action has an associated permission level.  This
        /// function determines whether the user represented by UserName has sufficient permissions, as
        /// determined by the PortalSettings and ModuleSettings, to access a resource with the
        /// designated AccessLevel.</remarks>
        ///-----------------------------------------------------------------------------
        public virtual bool HasModuleAccess(SecurityAccessLevel accessLevel, string permissionKey, ModuleInfo moduleConfiguration)
        {
            bool isAuthorized = false;
            UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
            TabInfo tab = TabController.Instance.GetTab(moduleConfiguration.TabID, moduleConfiguration.PortalID, false);
            if (userInfo != null && userInfo.IsSuperUser)
            {
                isAuthorized = true;
            }
            else
            {
                switch (accessLevel)
                {
                    case SecurityAccessLevel.Anonymous:
                        isAuthorized = true;
                        break;
                    case SecurityAccessLevel.View:
                        if (ModulePermissionController.CanViewModule(moduleConfiguration))
                        {
                            isAuthorized = true;
                        }
                        break;
                    case SecurityAccessLevel.ViewPermissions:
                        isAuthorized = TabPermissionController.CanAddContentToPage(tab);
                        break;
                    case SecurityAccessLevel.Edit:
                        if (!((moduleConfiguration.IsShared && moduleConfiguration.IsShareableViewOnly) && TabPermissionController.CanAddContentToPage(tab)))
                        {
                            if (string.IsNullOrEmpty(permissionKey))
                            {
                                permissionKey = "CONTENT,DELETE,EXPORT,IMPORT,MANAGE";
                            }

                            if (TabPermissionController.CanAddContentToPage(tab))
                            {
                                //Need to check for Deny Edit at the Module Level
                                if (permissionKey == "CONTENT")
                                {
                                    isAuthorized = !IsDeniedModulePermission(moduleConfiguration, permissionKey);
                                }
                                else
                                {
                                    isAuthorized = true;
                                }
                            }
                            else
                            {
                                // Need to check if it was denied at Tab level
                                if (!IsDeniedTabPermission(tab, "CONTENT,EDIT"))
                                {
                                    isAuthorized = HasModulePermission(moduleConfiguration, permissionKey);
                                }
                            }
                        }
                        break;
                    case SecurityAccessLevel.Admin:
                        if (!((moduleConfiguration.IsShared && moduleConfiguration.IsShareableViewOnly) && TabPermissionController.CanAddContentToPage(tab)))
                        {
                            isAuthorized = TabPermissionController.CanAddContentToPage(tab);
                        }
                        break;
                    case SecurityAccessLevel.Host:
                        break;
                }
            }
            return isAuthorized;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasModulePermission checks whether the current user has a specific Module Permission
        /// </summary>
        /// <param name="modulePermissions">The Permissions for the Module</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// -----------------------------------------------------------------------------
        public virtual bool HasModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
            bool hasPermission = Null.NullBoolean;
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsInRoles(modulePermissions.ToString(permission)))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }
            else
            {
                hasPermission = PortalSecurity.IsInRoles(modulePermissions.ToString(permissionKey));
            }
            return hasPermission; 
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveModulePermissions updates a Module's permissions
        /// </summary>
        /// <param name="module">The Module to update</param>
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
                                                             UserController.Instance.GetCurrentUserInfo().UserID);
                        }
                    }
                }
            }
        }

        #endregion

        #region TabPermission Methods

        /// <summary>
        /// Returns a list with all roles with implicit permissions on Tabs
        /// </summary>
        /// <param name="portalId">The Portal Id where the Roles are</param>
        /// <returns>A List with the implicit roles</returns>
        public virtual IEnumerable<RoleInfo> ImplicitRolesForPages(int portalId)
        {
            return DefaultImplicitRoles(portalId);
        }

        /// <summary>
        /// Returns a list with all roles with implicit permissions on Folders
        /// </summary>
        /// <param name="portalId">The Portal Id where the permissions are</param>
        /// <returns>A List with the implicit roles</returns>
        public virtual IEnumerable<RoleInfo> ImplicitRolesForFolders(int portalId)
        {
            return DefaultImplicitRoles(portalId);
        } 

        /// <summary>
        /// Returns a flag indicating whether the current user can add content to a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanAddContentToPage(TabInfo tab)
        {
            return HasPagePermission(tab, ContentPagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can add a child page to a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanAddPage(TabInfo tab)
        {
            return HasPagePermission(tab, AddPagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can administer a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanAdminPage(TabInfo tab)
        {
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AdminPagePermissionKey));
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can copy a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanCopyPage(TabInfo tab)
        {
            return HasPagePermission(tab, CopyPagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can delete a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanDeletePage(TabInfo tab)
        {
            return HasPagePermission(tab, DeletePagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can export a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanExportPage(TabInfo tab)
        {
            return HasPagePermission(tab, ExportPagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can import a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanImportPage(TabInfo tab)
        {
            return HasPagePermission(tab, ImportPagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can manage a page's settings
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanManagePage(TabInfo tab)
        {
            return HasPagePermission(tab, ManagePagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can see a page in a navigation object
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanNavigateToPage(TabInfo tab)
        {
            return HasPagePermission(tab, NavigatePagePermissionKey) || HasPagePermission(tab, ViewPagePermissionKey);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can view a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public virtual bool CanViewPage(TabInfo tab)
        {
            return HasPagePermission(tab, ViewPagePermissionKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteTabPermissionsByUser deletes a user's Tab Permissions in the Database
        /// </summary>
        /// <param name="user">The user</param>
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
        /// <param name="tabId">The ID of the tab</param>
        /// <param name="portalId">The ID of the portal</param>
        /// -----------------------------------------------------------------------------
        public virtual TabPermissionCollection GetTabPermissions(int tabId, int portalId)
        {
            //Get the Portal TabPermission Dictionary
            Dictionary<int, TabPermissionCollection> dicTabPermissions = GetTabPermissions(portalId);

            //Get the Collection from the Dictionary
            TabPermissionCollection tabPermissions;
            bool bFound = dicTabPermissions.TryGetValue(tabId, out tabPermissions);
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
        /// -----------------------------------------------------------------------------
        public virtual bool HasTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
            bool hasPermission = PortalSecurity.IsInRoles(tabPermissions.ToString("EDIT"));
            if (!hasPermission)
            {
                if (permissionKey.Contains(","))
                {
                    foreach (string permission in permissionKey.Split(','))
                    {
                        if (PortalSecurity.IsInRoles(tabPermissions.ToString(permission)))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }
                else
                {
                    hasPermission = PortalSecurity.IsInRoles(tabPermissions.ToString(permissionKey));
                }
            }
            return hasPermission;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveTabPermissions saves a Tab's permissions
        /// </summary>
        /// <param name="tab">The Tab to update</param>
        /// -----------------------------------------------------------------------------
        public virtual void SaveTabPermissions(TabInfo tab)
        {
            TabPermissionCollection objCurrentTabPermissions = GetTabPermissions(tab.TabID, tab.PortalID);
            if (!objCurrentTabPermissions.CompareTo(tab.TabPermissions))
            {
                dataProvider.DeleteTabPermissionsByTabID(tab.TabID);
                EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.TABPERMISSION_DELETED);
                if (tab.TabPermissions != null)
                {
                    foreach (TabPermissionInfo objTabPermission in tab.TabPermissions)
                    {
                        dataProvider.AddTabPermission(tab.TabID,
                                                      objTabPermission.PermissionID,
                                                      objTabPermission.RoleID,
                                                      objTabPermission.AllowAccess,
                                                      objTabPermission.UserID,
                                                      UserController.Instance.GetCurrentUserInfo().UserID);
                        EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.TABPERMISSION_CREATED);
                    }
                }
            }
        }

        #endregion

        #region DesktopModule Permissions Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermission gets a DesktopModule Permission from the Database
        /// </summary>
        /// <param name="desktopModulePermissionId">The ID of the DesktopModule Permission</param>
        /// -----------------------------------------------------------------------------
        public virtual DesktopModulePermissionInfo GetDesktopModulePermission(int desktopModulePermissionId)
        {
            return CBO.FillObject<DesktopModulePermissionInfo>(DataProvider.Instance().GetDesktopModulePermission(desktopModulePermissionId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermissions gets a DesktopModulePermissionCollection
        /// </summary>
        /// <param name="portalDesktopModuleId">The ID of the DesktopModule</param>
        /// -----------------------------------------------------------------------------
        public virtual DesktopModulePermissionCollection GetDesktopModulePermissions(int portalDesktopModuleId)
        {
            //Get the Tab DesktopModulePermission Dictionary
            Dictionary<int, DesktopModulePermissionCollection> dicDesktopModulePermissions = GetDesktopModulePermissions();

            //Get the Collection from the Dictionary
            DesktopModulePermissionCollection desktopModulePermissions;
            bool bFound = dicDesktopModulePermissions.TryGetValue(portalDesktopModuleId, out desktopModulePermissions);
            if (!bFound)
            {
                //Return empty collection
                desktopModulePermissions = new DesktopModulePermissionCollection();
            }
            return desktopModulePermissions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasDesktopModulePermission checks whether the current user has a specific DesktopModule Permission
        /// </summary>
        /// <param name="desktopModulePermissions">The Permissions for the DesktopModule</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// -----------------------------------------------------------------------------
        public virtual bool HasDesktopModulePermission(DesktopModulePermissionCollection desktopModulePermissions, string permissionKey)
        {
            return PortalSecurity.IsInRoles(desktopModulePermissions.ToString(permissionKey));
        }

        #endregion

        #endregion
    }
}
