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

using System;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Tabs
{
    public class TabPublishingController: ServiceLocator<ITabPublishingController,TabPublishingController>, ITabPublishingController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPublishingController));

        public void PublishTab(int tabID, int portalID)
        {
            var tab = TabController.Instance.GetTab(tabID, portalID);
            if (!TabPermissionController.CanAdminPage(tab))
            {
                var errorMessage = Localization.GetExceptionMessage("PublishPagePermissionsNotMet", "Permissions are not met. The page has not been published.");
                var permissionsNotMetExc =  new PermissionsNotMetException(tabID, errorMessage );
                Logger.Error(errorMessage, permissionsNotMetExc);
                throw permissionsNotMetExc;
            }
            var allUsersRoleId = Int32.Parse(Globals.glbRoleAllUsers);

            var existPermission = GetAlreadyPermission(tab, "VIEW", allUsersRoleId);
            if (existPermission != null)
            {
                if (existPermission.AllowAccess)
                {
                    return;
                }
                else
                {
                    tab.TabPermissions.Remove(existPermission);
                }
            }

            tab.TabPermissions.Add(GetTabPermissionByRole(tab.TabID, "VIEW", allUsersRoleId));            
            TabPermissionController.SaveTabPermissions(tab);
            ClearTabCache(tab);
        }
        
        #region private Methods

        private void ClearTabCache(TabInfo tabInfo)
        {
            new TabController().ClearCache(tabInfo.PortalID);
            //Clear the Tab's Cached modules
            DataCache.ClearModuleCache(tabInfo.TabID);
        }

        private TabPermissionInfo GetAlreadyPermission(TabInfo tab, string permissionKey, int roleId)
        {
            var permission = PermissionController.GetPermissionsByTab().Cast<PermissionInfo>().SingleOrDefault<PermissionInfo>(p => p.PermissionKey == permissionKey);

            return
                tab.TabPermissions.Cast<TabPermissionInfo>()
                    .FirstOrDefault(tp => tp.RoleID == roleId && tp.PermissionID == permission.PermissionID);
        }

        private TabPermissionInfo GetTabPermissionByRole(int tabID, string permissionKey, int roleID)
        {
            var permission = PermissionController.GetPermissionsByTab().Cast<PermissionInfo>().SingleOrDefault<PermissionInfo>(p => p.PermissionKey == permissionKey);
            var tabPermission = new TabPermissionInfo
                        {
                            TabID = tabID,
                            PermissionID = permission.PermissionID,
                            PermissionKey = permission.PermissionKey,
                            PermissionName = permission.PermissionName,
                            RoleID = roleID,
                            UserID = Null.NullInteger,
                            AllowAccess = true
                        };
            return tabPermission;
        }
        #endregion

        protected override Func<ITabPublishingController> GetFactory()
        {
            return () => new TabPublishingController();
        }
    }
}
