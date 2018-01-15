#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class DesktopModulePermissionController
    {
        private static readonly PermissionProvider _provider = PermissionProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ClearPermissionCache clears the DesktopModule Permission Cache
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static void ClearPermissionCache()
        {
            DataCache.ClearDesktopModulePermissionsCache();
        }

		#region Public Shared Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddDesktopModulePermission adds a DesktopModule Permission to the Database
        /// </summary>
        /// <param name="objDesktopModulePermission">The DesktopModule Permission to add</param>
        /// -----------------------------------------------------------------------------
        public static int AddDesktopModulePermission(DesktopModulePermissionInfo objDesktopModulePermission)
        {
            int Id = DataProvider.Instance().AddDesktopModulePermission(objDesktopModulePermission.PortalDesktopModuleID,
                                                         objDesktopModulePermission.PermissionID,
                                                         objDesktopModulePermission.RoleID,
                                                         objDesktopModulePermission.AllowAccess,
                                                         objDesktopModulePermission.UserID,
                                                         UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(objDesktopModulePermission,
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
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
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModulePermission(int DesktopModulePermissionID)
        {
            DataProvider.Instance().DeleteDesktopModulePermission(DesktopModulePermissionID);
            EventLogController.Instance.AddLog("DesktopModulePermissionID",
                               DesktopModulePermissionID.ToString(CultureInfo.InvariantCulture),
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULEPERMISSION_DELETED);
            ClearPermissionCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModulePermissionsByPortalDesktopModuleID deletes a DesktopModule's
        /// DesktopModule Permission in the Database
        /// </summary>
        /// <param name="portalDesktopModuleID">The ID of the DesktopModule to delete</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
        {
            DataProvider.Instance().DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID);
            EventLogController.Instance.AddLog("PortalDesktopModuleID",
                               portalDesktopModuleID.ToString(CultureInfo.InvariantCulture),
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULE_DELETED);
            ClearPermissionCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteDesktopModulePermissionsByUserID deletes a user's DesktopModule Permission in the Database
        /// </summary>
        /// <param name="objUser">The user</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteDesktopModulePermissionsByUserID(UserInfo objUser)
        {
            DataProvider.Instance().DeleteDesktopModulePermissionsByUserID(objUser.UserID, objUser.PortalID);
            EventLogController.Instance.AddLog("UserID",
                               objUser.UserID.ToString(CultureInfo.InvariantCulture),
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.DESKTOPMODULE_DELETED);
            ClearPermissionCache();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermission gets a DesktopModule Permission from the Database
        /// </summary>
        /// <param name="DesktopModulePermissionID">The ID of the DesktopModule Permission</param>
        /// -----------------------------------------------------------------------------
        public static DesktopModulePermissionInfo GetDesktopModulePermission(int DesktopModulePermissionID)
        {
            return _provider.GetDesktopModulePermission(DesktopModulePermissionID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDesktopModulePermissions gets a DesktopModulePermissionCollection
        /// </summary>
        /// <param name="portalDesktopModuleID">The ID of the DesktopModule</param>
        /// -----------------------------------------------------------------------------
        public static DesktopModulePermissionCollection GetDesktopModulePermissions(int portalDesktopModuleID)
        {
            return _provider.GetDesktopModulePermissions(portalDesktopModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasDesktopModulePermission checks whether the current user has a specific DesktopModule Permission
        /// </summary>
        /// <param name="objDesktopModulePermissions">The Permissions for the DesktopModule</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// -----------------------------------------------------------------------------
        public static bool HasDesktopModulePermission(DesktopModulePermissionCollection objDesktopModulePermissions, string permissionKey)
        {
            return _provider.HasDesktopModulePermission(objDesktopModulePermissions, permissionKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateDesktopModulePermission updates a DesktopModule Permission in the Database
        /// </summary>
        /// <param name="objDesktopModulePermission">The DesktopModule Permission to update</param>
        /// -----------------------------------------------------------------------------
        public static void UpdateDesktopModulePermission(DesktopModulePermissionInfo objDesktopModulePermission)
        {
            DataProvider.Instance().UpdateDesktopModulePermission(objDesktopModulePermission.DesktopModulePermissionID,
                                                   objDesktopModulePermission.PortalDesktopModuleID,
                                                   objDesktopModulePermission.PermissionID,
                                                   objDesktopModulePermission.RoleID,
                                                   objDesktopModulePermission.AllowAccess,
                                                   objDesktopModulePermission.UserID,
                                                   UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(objDesktopModulePermission,
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               "",
                               EventLogController.EventLogType.DESKTOPMODULEPERMISSION_UPDATED);
            ClearPermissionCache();
        }
		
		#endregion
    }
}
