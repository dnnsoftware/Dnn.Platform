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
using System.Collections.Generic;
using System.Data;
using System.Globalization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : DesktopModulePermissionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DesktopModulePermissionController provides the Business Layer for DesktopModule Permissions
    /// </summary>
    /// <history>
    /// 	[cnurse]	01/15/2008   Created
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class DesktopModulePermissionController
    {
		#region Private Members

        private static readonly DataProvider provider = DataProvider.Instance();
		
		#endregion
		
		#region Private Shared Methods
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ClearPermissionCache clears the DesktopModule Permission Cache
        /// </summary>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void ClearPermissionCache()
        {
            DataCache.ClearDesktopModulePermissionsCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDesktopModulePermissionDictionary fills a Dictionary of DesktopModulePermissions from a
        /// dataReader
        /// </summary>
        /// <param name="dr">The IDataReader</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
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
                        var collection = new DesktopModulePermissionCollection {desktopModulePermissionInfo};

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermissions gets a Dictionary of DesktopModulePermissionCollections by
        /// DesktopModule.
        /// </summary>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static object GetDesktopModulePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            return FillDesktopModulePermissionDictionary(provider.GetDesktopModulePermissions());
        }
		
		#endregion
		
		#region Public Shared Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddDesktopModulePermission adds a DesktopModule Permission to the Database
        /// </summary>
        /// <param name="objDesktopModulePermission">The DesktopModule Permission to add</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int AddDesktopModulePermission(DesktopModulePermissionInfo objDesktopModulePermission)
        {
            int Id = provider.AddDesktopModulePermission(objDesktopModulePermission.PortalDesktopModuleID,
                                                         objDesktopModulePermission.PermissionID,
                                                         objDesktopModulePermission.RoleID,
                                                         objDesktopModulePermission.AllowAccess,
                                                         objDesktopModulePermission.UserID,
                                                         UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(objDesktopModulePermission,
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               "",
                               EventLogController.EventLogType.DESKTOPMODULEPERMISSION_CREATED);
            ClearPermissionCache();
            return Id;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModulePermission deletes a DesktopModule Permission in the Database
        /// </summary>
        /// <param name="DesktopModulePermissionID">The ID of the DesktopModule Permission to delete</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModulePermission(int DesktopModulePermissionID)
        {
            provider.DeleteDesktopModulePermission(DesktopModulePermissionID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("DesktopModulePermissionID",
                               DesktopModulePermissionID.ToString(CultureInfo.InvariantCulture),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULEPERMISSION_DELETED);
            ClearPermissionCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModulePermissionsByPortalDesktopModuleID deletes a DesktopModule's
        /// DesktopModule Permission in the Database
        /// </summary>
        /// <param name="portalDesktopModuleID">The ID of the DesktopModule to delete</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
        {
            provider.DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("PortalDesktopModuleID",
                               portalDesktopModuleID.ToString(CultureInfo.InvariantCulture),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULE_DELETED);
            ClearPermissionCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModulePermissionsByUserID deletes a user's DesktopModule Permission in the Database
        /// </summary>
        /// <param name="objUser">The user</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModulePermissionsByUserID(UserInfo objUser)
        {
            provider.DeleteDesktopModulePermissionsByUserID(objUser.UserID, objUser.PortalID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("UserID",
                               objUser.UserID.ToString(CultureInfo.InvariantCulture),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULE_DELETED);
            ClearPermissionCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermission gets a DesktopModule Permission from the Database
        /// </summary>
        /// <param name="DesktopModulePermissionID">The ID of the DesktopModule Permission</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static DesktopModulePermissionInfo GetDesktopModulePermission(int DesktopModulePermissionID)
        {
            return CBO.FillObject<DesktopModulePermissionInfo>(provider.GetDesktopModulePermission(DesktopModulePermissionID), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermissions gets a DesktopModulePermissionCollection
        /// </summary>
        /// <param name="portalDesktopModuleID">The ID of the DesktopModule</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static DesktopModulePermissionCollection GetDesktopModulePermissions(int portalDesktopModuleID)
        {
            //Get the Tab DesktopModulePermission Dictionary
            Dictionary<int, DesktopModulePermissionCollection> dicDesktopModulePermissions = GetDesktopModulePermissions();

            //Get the Collection from the Dictionary
            DesktopModulePermissionCollection DesktopModulePermissions;
            bool bFound = dicDesktopModulePermissions.TryGetValue(portalDesktopModuleID, out DesktopModulePermissions);
            if (!bFound)
            {
                //Return empty collection
                DesktopModulePermissions = new DesktopModulePermissionCollection();
            }
            return DesktopModulePermissions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasDesktopModulePermission checks whether the current user has a specific DesktopModule Permission
        /// </summary>
        /// <param name="objDesktopModulePermissions">The Permissions for the DesktopModule</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static bool HasDesktopModulePermission(DesktopModulePermissionCollection objDesktopModulePermissions, string permissionKey)
        {
            return PortalSecurity.IsInRoles(objDesktopModulePermissions.ToString(permissionKey));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateDesktopModulePermission updates a DesktopModule Permission in the Database
        /// </summary>
        /// <param name="objDesktopModulePermission">The DesktopModule Permission to update</param>
        /// <history>
        /// 	[cnurse]	01/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void UpdateDesktopModulePermission(DesktopModulePermissionInfo objDesktopModulePermission)
        {
            provider.UpdateDesktopModulePermission(objDesktopModulePermission.DesktopModulePermissionID,
                                                   objDesktopModulePermission.PortalDesktopModuleID,
                                                   objDesktopModulePermission.PermissionID,
                                                   objDesktopModulePermission.RoleID,
                                                   objDesktopModulePermission.AllowAccess,
                                                   objDesktopModulePermission.UserID,
                                                   UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(objDesktopModulePermission,
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               "",
                               EventLogController.EventLogType.DESKTOPMODULEPERMISSION_UPDATED);
            ClearPermissionCache();
        }
		
		#endregion
    }
}
