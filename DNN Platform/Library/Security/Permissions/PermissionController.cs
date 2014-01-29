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
using System.Linq;
using System.Text;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Security.Permissions
{
    public class PermissionController
    {
		#region Public Methods
		
        private static readonly DataProvider provider = DataProvider.Instance();

        public int AddPermission(PermissionInfo objPermission)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog(objPermission, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PERMISSION_CREATED);
            return
                Convert.ToInt32(provider.AddPermission(objPermission.PermissionCode,
                                                       objPermission.ModuleDefID,
                                                       objPermission.PermissionKey,
                                                       objPermission.PermissionName,
                                                       UserController.GetCurrentUserInfo().UserID));
        }

        public void DeletePermission(int permissionID)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog("PermissionID",
                               permissionID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PERMISSION_DELETED);
            provider.DeletePermission(permissionID);
        }

        public PermissionInfo GetPermission(int permissionID)
        {
            return CBO.FillObject<PermissionInfo>(provider.GetPermission(permissionID));
        }

        public ArrayList GetPermissionByCodeAndKey(string permissionCode, string permissionKey)
        {
            return CBO.FillCollection(provider.GetPermissionByCodeAndKey(permissionCode, permissionKey), typeof (PermissionInfo));
        }

        public ArrayList GetPermissionsByModuleDefID(int moduleDefID)
        {
            return CBO.FillCollection(provider.GetPermissionsByModuleDefID(moduleDefID), typeof (PermissionInfo));
        }

        public ArrayList GetPermissionsByModuleID(int moduleID)
        {
            return CBO.FillCollection(provider.GetPermissionsByModuleID(moduleID), typeof (PermissionInfo));
        }

        public void UpdatePermission(PermissionInfo objPermission)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog(objPermission, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PERMISSION_UPDATED);
            provider.UpdatePermission(objPermission.PermissionID,
                                      objPermission.PermissionCode,
                                      objPermission.ModuleDefID,
                                      objPermission.PermissionKey,
                                      objPermission.PermissionName,
                                      UserController.GetCurrentUserInfo().UserID);
        }
		
		#endregion
		
		#region Shared Methods

        public static string BuildPermissions(IList Permissions, string PermissionKey)
        {
            var permissionsBuilder = new StringBuilder();
            foreach (PermissionInfoBase permission in Permissions)
            {
                if (permission.PermissionKey.ToLowerInvariant() == PermissionKey.ToLowerInvariant())
                {
					//Deny permissions are prefixed with a "!"
                    string prefix = !permission.AllowAccess ? "!" : "";
					
                    //encode permission
                    string permissionString;
                    if (Null.IsNull(permission.UserID))
                    {
                        permissionString = prefix + permission.RoleName + ";";
                    }
                    else
                    {
                        permissionString = prefix + "[" + permission.UserID + "];";
                    }
					
                    //build permissions string ensuring that Deny permissions are inserted at the beginning and Grant permissions at the end
                    if (prefix == "!")
                    {
                        permissionsBuilder.Insert(0, permissionString);
                    }
                    else
                    {
                        permissionsBuilder.Append(permissionString);
                    }
                }
            }
			
            //get string
            string permissionsString = permissionsBuilder.ToString();

            //ensure leading delimiter
            if (!permissionsString.StartsWith(";"))
            {
                permissionsString.Insert(0, ";");
            }
            return permissionsString;
        }

        public static ArrayList GetPermissionsByFolder()
        {
            return CBO.FillCollection(provider.GetPermissionsByFolder(), typeof (PermissionInfo));
        }

        public static ArrayList GetPermissionsByPortalDesktopModule()
        {
            return CBO.FillCollection(provider.GetPermissionsByPortalDesktopModule(), typeof (PermissionInfo));
        }

        public static ArrayList GetPermissionsByTab()
        {
            return CBO.FillCollection(provider.GetPermissionsByTab(), typeof (PermissionInfo));
        }

        public T RemapPermission<T>(T permission, int portalId) where T : PermissionInfoBase
        {
            PermissionInfo permissionInfo = GetPermissionByCodeAndKey(permission.PermissionCode, permission.PermissionKey).ToArray().Cast<PermissionInfo>().FirstOrDefault();
            T result = null;

            if ((permissionInfo != null))
            {
                int RoleID = int.MinValue;
                int UserID = int.MinValue;

                if ((string.IsNullOrEmpty(permission.RoleName)))
                {
                    UserInfo _user = UserController.GetUserByName(portalId, permission.Username);
                    if ((_user != null))
                    {
                        UserID = _user.UserID;
                    }
                }
                else
                {
                    switch (permission.RoleName)
                    {
                        case Globals.glbRoleAllUsersName:
                            RoleID = Convert.ToInt32(Globals.glbRoleAllUsers);
                            break;
                        case Globals.glbRoleUnauthUserName:
                            RoleID = Convert.ToInt32(Globals.glbRoleUnauthUser);
                            break;
                        default:
                            RoleInfo _role = TestableRoleController.Instance.GetRole(portalId, r => r.RoleName == permission.RoleName);
                            if ((_role != null))
                            {
                                RoleID = _role.RoleID;
                            }
                            break;
                    }
                }

                // if role was found add, otherwise ignore
                if (RoleID != int.MinValue || UserID != int.MinValue)
                {
                    permission.PermissionID = permissionInfo.PermissionID;
                    if ((RoleID != int.MinValue))
                    {
                        permission.RoleID = RoleID;
                    }
                    if ((UserID != int.MinValue))
                    {
                        permission.UserID = UserID;
                    }
                    result = permission;
                }
            }

            return result;
        }
		
		#endregion

        [Obsolete("Deprecated in DNN 5.0.1. Replaced by GetPermissionsByFolder()")]
        public ArrayList GetPermissionsByFolder(int portalID, string folder)
        {
            return CBO.FillCollection(provider.GetPermissionsByFolder(), typeof (PermissionInfo));
        }

        [Obsolete("Deprecated in DNN 5.0.1. Replaced by GetPermissionsByTab()")]
        public ArrayList GetPermissionsByTabID(int tabID)
        {
            return CBO.FillCollection(provider.GetPermissionsByTab(), typeof (PermissionInfo));
        }

        [Obsolete("Deprecated in DNN 5.0.1. Replaced by GetPermissionsByPortalDesktopModule()")]
        public ArrayList GetPermissionsByPortalDesktopModuleID()
        {
            return CBO.FillCollection(provider.GetPermissionsByPortalDesktopModule(), typeof (PermissionInfo));
        }
    }
}