#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : TabPermissionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// TabPermissionController provides the Business Layer for Tab Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class TabPermissionController
	{
		#region "Private Shared Methods"

		private static void ClearPermissionCache(int tabId)
		{
            var objTab = TabController.Instance.GetTab(tabId, Null.NullInteger, false);
			DataCache.ClearTabPermissionsCache(objTab.PortalID);
		}

		#endregion
		
		#region Private Members
		
        private static readonly PermissionProvider _provider = PermissionProvider.Instance();
		
		#endregion
		
		#region Public Shared Methods

        /// <summary>
        /// Returns a list with all roles with implicit permissions on Tabs
        /// </summary>
        /// <param name="portalId">The Portal Id where the Roles are</param>
        /// <returns>A List with the implicit roles</returns>
        public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
        {
            return _provider.ImplicitRolesForPages(portalId);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can add content to the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAddContentToPage()
        {
            return CanAddContentToPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can add content to a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAddContentToPage(TabInfo tab)
        {
            return _provider.CanAddContentToPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can add a child page to the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAddPage()
        {
            return CanAddPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can add a child page to a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAddPage(TabInfo tab)
        {
            return _provider.CanAddPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can administer the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAdminPage()
        {
            return CanAdminPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can administer a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAdminPage(TabInfo tab)
        {
            return _provider.CanAdminPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can copy the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanCopyPage()
        {
            return CanCopyPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can copy a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanCopyPage(TabInfo tab)
        {
            return _provider.CanCopyPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can delete the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanDeletePage()
        {
            return CanDeletePage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can delete a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanDeletePage(TabInfo tab)
        {
            return _provider.CanDeletePage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can export the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanExportPage()
        {
            return CanExportPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can export a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanExportPage(TabInfo tab)
        {
            return _provider.CanExportPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can import the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanImportPage()
        {
            return CanImportPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can import a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanImportPage(TabInfo tab)
        {
            return _provider.CanImportPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can manage the current page's settings
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanManagePage()
        {
            return CanManagePage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can manage a page's settings
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanManagePage(TabInfo tab)
        {
            return _provider.CanManagePage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can see the current page in a navigation object
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanNavigateToPage()
        {
            return CanNavigateToPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can see a page in a navigation object
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanNavigateToPage(TabInfo tab)
        {
            return _provider.CanNavigateToPage(tab);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can view the current page
        /// </summary>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanViewPage()
        {
            return CanViewPage(TabController.CurrentPage);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can view a page
        /// </summary>
        /// <param name="tab">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanViewPage(TabInfo tab)
        {
            return _provider.CanViewPage(tab);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteTabPermissionsByUser deletes a user's Tab Permissions in the Database
        /// </summary>
        /// <param name="user">The user</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteTabPermissionsByUser(UserInfo user)
        {
            _provider.DeleteTabPermissionsByUser(user);
            EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.TABPERMISSION_DELETED);
            DataCache.ClearTabPermissionsCache(user.PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetTabPermissions gets a TabPermissionCollection
        /// </summary>
        /// <param name="tabId">The ID of the tab</param>
        /// <param name="portalId">The ID of the portal</param>
        /// -----------------------------------------------------------------------------
        public static TabPermissionCollection GetTabPermissions(int tabId, int portalId)
        {
            return _provider.GetTabPermissions(tabId, portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasTabPermission checks whether the current user has a specific Tab Permission
        /// </summary>
        /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE", this will return
        /// true if the user has any one of the permissions.</remarks>
        /// <param name="permissionKey">The Permission to check</param>
        /// -----------------------------------------------------------------------------
        public static bool HasTabPermission(string permissionKey)
        {
            return HasTabPermission(PortalController.Instance.GetCurrentPortalSettings().ActiveTab.TabPermissions, permissionKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasTabPermission checks whether the current user has a specific Tab Permission
        /// </summary>
        /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE", this will return
        /// true if the user has any one of the permissions.</remarks>
        /// <param name="tabPermissions">The Permissions for the Tab</param>
        /// <param name="permissionKey">The Permission(s) to check</param>
        /// -----------------------------------------------------------------------------
        public static bool HasTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
            return _provider.HasTabPermission(tabPermissions, permissionKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveTabPermissions saves a Tab's permissions
        /// </summary>
        /// <param name="tab">The Tab to update</param>
        /// -----------------------------------------------------------------------------
        public static void SaveTabPermissions(TabInfo tab)
        {
            _provider.SaveTabPermissions(tab);
            EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.TABPERMISSION_UPDATED);
            DataCache.ClearTabPermissionsCache(tab.PortalID);
        }

		#endregion

		#region Obsolete Methods

		[Obsolete("Deprecated in DNN 5.1.")]
		public int AddTabPermission(TabPermissionInfo objTabPermission)
		{
			int id = Convert.ToInt32(DataProvider.Instance().AddTabPermission(objTabPermission.TabID, objTabPermission.PermissionID, objTabPermission.RoleID, objTabPermission.AllowAccess, objTabPermission.UserID, UserController.Instance.GetCurrentUserInfo().UserID));
			ClearPermissionCache(objTabPermission.TabID);
			return id;
		}

		[Obsolete("Deprecated in DNN 5.1.")]
		public void DeleteTabPermission(int tabPermissionID)
		{
			DataProvider.Instance().DeleteTabPermission(tabPermissionID);
		}

		[Obsolete("Deprecated in DNN 5.1.")]
		public void DeleteTabPermissionsByTabID(int tabID)
		{
			DataProvider.Instance().DeleteTabPermissionsByTabID(tabID);
			ClearPermissionCache(tabID);
		}

		[Obsolete("Deprecated in DNN 5.0.  Use DeleteTabPermissionsByUser(UserInfo) ")]
		public void DeleteTabPermissionsByUserID(UserInfo objUser)
		{
			DataProvider.Instance().DeleteTabPermissionsByUserID(objUser.PortalID, objUser.UserID);
			DataCache.ClearTabPermissionsCache(objUser.PortalID);
		}

		[Obsolete("Deprecated in DNN 5.0. Please use TabPermissionCollection.ToString(String)")]
		public string GetTabPermissions(TabPermissionCollection tabPermissions, string permissionKey)
		{
			return tabPermissions.ToString(permissionKey);
		}

		[Obsolete("Deprecated in DNN 5.0.  This should have been declared as Friend as it was never meant to be used outside of the core.")]
		public ArrayList GetTabPermissionsByPortal(int PortalID)
		{
			return CBO.FillCollection(DataProvider.Instance().GetTabPermissionsByPortal(PortalID), typeof(TabPermissionInfo));
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use GetTabPermissions(TabId, PortalId)")]
		public ArrayList GetTabPermissionsByTabID(int TabID)
		{
			return CBO.FillCollection(DataProvider.Instance().GetTabPermissionsByTabID(TabID, -1), typeof(TabPermissionInfo));
		}

		[Obsolete("Deprecated in DNN 5.0. Please use TabPermissionCollection.ToString(String)")]
		public string GetTabPermissionsByTabID(ArrayList arrTabPermissions, int TabID, string PermissionKey)
		{
			//Create a Tab Permission Collection from the ArrayList
			TabPermissionCollection tabPermissions = new TabPermissionCollection(arrTabPermissions, TabID);

			//Return the permission string for permissions with specified TabId
			return tabPermissions.ToString(PermissionKey);
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use GetTabPermissions(TabId, PortalId)")]
		public TabPermissionCollection GetTabPermissionsByTabID(ArrayList arrTabPermissions, int TabID)
		{
			return new TabPermissionCollection(arrTabPermissions, TabID);
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use GetTabPermissions(TabId, PortalId)")]
		public TabPermissionCollection GetTabPermissionsCollectionByTabID(int TabID)
		{
			return new TabPermissionCollection(CBO.FillCollection(DataProvider.Instance().GetTabPermissionsByTabID(TabID, -1), typeof(TabPermissionInfo)));
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use GetTabPermissions(TabId, PortalId)")]
		public TabPermissionCollection GetTabPermissionsCollectionByTabID(ArrayList arrTabPermissions, int TabID)
		{
			return new TabPermissionCollection(arrTabPermissions, TabID);
		}

		[Obsolete("Deprecated in DNN 5.1.  Please use GetTabPermissions(TabId, PortalId)")]
		public TabPermissionCollection GetTabPermissionsCollectionByTabID(int tabID, int portalID)
		{
			return GetTabPermissions(tabID, portalID);
		}

		[Obsolete("Deprecated in DNN 5.1.")]
		public void UpdateTabPermission(TabPermissionInfo objTabPermission)
		{
			DataProvider.Instance().UpdateTabPermission(objTabPermission.TabPermissionID, objTabPermission.TabID, objTabPermission.PermissionID, objTabPermission.RoleID, objTabPermission.AllowAccess, objTabPermission.UserID, UserController.Instance.GetCurrentUserInfo().UserID);
			ClearPermissionCache(objTabPermission.TabID);
		}

		#endregion
    }
}
