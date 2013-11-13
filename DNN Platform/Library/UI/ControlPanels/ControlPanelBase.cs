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
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Personalization;

#endregion

namespace DotNetNuke.UI.ControlPanels
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ControlPanel class defines a custom base class inherited by all
    /// ControlPanel controls.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/11/2008  documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class ControlPanelBase : UserControl
    {
		#region Private Members

        private string _localResourceFile;
		
		#endregion

		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the ControlPanel is Visible
        /// </summary>
        /// <value>A Boolean</value>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool IsVisible
        {
            get
            {
                return PortalSettings.ControlPanelVisible;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the current Portal Settings
        /// </summary>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.GetCurrentPortalSettings();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the User mode of the Control Panel
        /// </summary>
        /// <value>A Boolean</value>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected PortalSettings.Mode UserMode
        {
            get
            {
                return PortalSettings.UserMode;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Local ResourceFile for the Control Panel
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (String.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + ID;
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                return fileRoot;
            }
            set
            {
                _localResourceFile = value;
            }
        }


        public virtual bool IncludeInControlHierarchy
        {
            get { return true; }
        }

        public virtual bool IsDockable
        {
          get { return false; }
          set { }
        }

        protected bool IsModuleAdmin()
        {
            return IsModuleAdminInternal();
        }

        protected bool IsPageAdmin()
        {
            return IsPageAdminInternal();
        }

        internal static bool IsModuleAdminInternal()
        {
            bool _IsModuleAdmin = Null.NullBoolean;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions && objModule.ModuleDefinition.DefaultCacheTime != -1)
                    {
                        _IsModuleAdmin = true;
                        break;
                    }
                }
            }
            return PortalController.GetCurrentPortalSettings().ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && _IsModuleAdmin;
        }

        internal static bool IsPageAdminInternal()
        {
            bool _IsPageAdmin = Null.NullBoolean;
            if (TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage())
            {
                _IsPageAdmin = true;
            }
            return _IsPageAdmin;
        }

		#endregion
		
		#region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Module Permission
        /// </summary>
        /// <param name="objModule">Module Info</param>
        /// <param name="permission">The permission to add</param>
        /// <param name="roleId">The Id of the role to add the permission for.</param>
        /// <param name="userId">Operator</param>
        /// <param name="allowAccess">Whether allow to access the module</param>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private ModulePermissionInfo AddModulePermission(ModuleInfo objModule, PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var objModulePermission = new ModulePermissionInfo();
            objModulePermission.ModuleID = objModule.ModuleID;
            objModulePermission.PermissionID = permission.PermissionID;
            objModulePermission.RoleID = roleId;
            objModulePermission.UserID = userId;
            objModulePermission.PermissionKey = permission.PermissionKey;
            objModulePermission.AllowAccess = allowAccess;

            //add the permission to the collection
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }
            return objModulePermission;
        }
		
		#endregion

		#region Protected Methods
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds an Existing Module to a Pane
        /// </summary>
        /// <param name="align">The alignment for the Modue</param>
        /// <param name="moduleId">The Id of the existing module</param>
        /// <param name="tabId">The id of the tab</param>
        /// <param name="paneName">The pane to add the module to</param>
        /// <param name="position">The relative position within the pane for the module</param>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void AddExistingModule(int moduleId, int tabId, string paneName, int position, string align)
        {
            var objModules = new ModuleController();
            ModuleInfo objModule;
            var objEventLog = new EventLogController();

            int UserId = -1;
            if (Request.IsAuthenticated)
            {
                UserInfo objUserInfo = UserController.GetCurrentUserInfo();
                UserId = objUserInfo.UserID;
            }
            objModule = objModules.GetModule(moduleId, tabId, false);
            if (objModule != null)
            {
                //clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo objClone = objModule.Clone();
                objClone.TabID = PortalSettings.ActiveTab.TabID;
                objClone.ModuleOrder = position;
                objClone.PaneName = paneName;
                objClone.Alignment = align;
                objModules.AddModule(objClone);
                objEventLog.AddLog(objClone, PortalSettings, UserId, "", EventLogController.EventLogType.MODULE_CREATED);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a New Module to a Pane
        /// </summary>
        /// <param name="align">The alignment for the Modue</param>
        /// <param name="desktopModuleId">The Id of the DesktopModule</param>
        /// <param name="permissionType">The View Permission Type for the Module</param>
        /// <param name="title">The Title for the resulting module</param>
        /// <param name="paneName">The pane to add the module to</param>
        /// <param name="position">The relative position within the pane for the module</param>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void AddNewModule(string title, int desktopModuleId, string paneName, int position, ViewPermissionType permissionType, string align)
        {
            TabPermissionCollection objTabPermissions = PortalSettings.ActiveTab.TabPermissions;
            var objPermissionController = new PermissionController();
            var objModules = new ModuleController();
            try
            {
                DesktopModuleInfo desktopModule;
                if (!DesktopModuleController.GetDesktopModules(PortalSettings.PortalId).TryGetValue(desktopModuleId, out desktopModule))
                {
                    throw new ArgumentException("desktopModuleId");
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            int UserId = -1;
            if (Request.IsAuthenticated)
            {
                UserInfo objUserInfo = UserController.GetCurrentUserInfo();
                UserId = objUserInfo.UserID;
            }
            foreach (ModuleDefinitionInfo objModuleDefinition in
                ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
                objModule.Initialize(PortalSettings.PortalId);
                objModule.PortalID = PortalSettings.PortalId;
                objModule.TabID = PortalSettings.ActiveTab.TabID;
                objModule.ModuleOrder = position;
                if (String.IsNullOrEmpty(title))
                {
                    objModule.ModuleTitle = objModuleDefinition.FriendlyName;
                }
                else
                {
                    objModule.ModuleTitle = title;
                }
                objModule.PaneName = paneName;
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                if (objModuleDefinition.DefaultCacheTime > 0)
                {
                    objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                    if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
                    {
                        ModuleInfo defaultModule = objModules.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
                        if (defaultModule != null)
                        {
                            objModule.CacheTime = defaultModule.CacheTime;
                        }
                    }
                }
                switch (permissionType)
                {
                    case ViewPermissionType.View:
                        objModule.InheritViewPermissions = true;
                        break;
                    case ViewPermissionType.Edit:
                        objModule.InheritViewPermissions = false;
                        break;
                }
				
                //get the default module view permissions
                ArrayList arrSystemModuleViewPermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");

                //get the permissions from the page
                foreach (TabPermissionInfo objTabPermission in objTabPermissions)
                {
                    if (objTabPermission.PermissionKey == "VIEW" && permissionType == ViewPermissionType.View)
                    {
						//Don't need to explicitly add View permisisons if "Same As Page"
                        continue;
                    }
					
                    //get the system module permissions for the permissionkey
                    ArrayList arrSystemModulePermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey);
                    //loop through the system module permissions
                    int j;
                    for (j = 0; j <= arrSystemModulePermissions.Count - 1; j++)
                    {
                        PermissionInfo objSystemModulePermission;
                        objSystemModulePermission = (PermissionInfo) arrSystemModulePermissions[j];
                        if (objSystemModulePermission.PermissionKey == "VIEW" && permissionType == ViewPermissionType.Edit && objTabPermission.PermissionKey != "EDIT")
                        {
							//Only Page Editors get View permissions if "Page Editors Only"
                            continue;
                        }
                        ModulePermissionInfo objModulePermission = AddModulePermission(objModule,
                                                                                       objSystemModulePermission,
                                                                                       objTabPermission.RoleID,
                                                                                       objTabPermission.UserID,
                                                                                       objTabPermission.AllowAccess);

                        //ensure that every EDIT permission which allows access also provides VIEW permission
                        if (objModulePermission.PermissionKey == "EDIT" && objModulePermission.AllowAccess)
                        {
                            ModulePermissionInfo objModuleViewperm = AddModulePermission(objModule,
                                                                                         (PermissionInfo) arrSystemModuleViewPermissions[0],
                                                                                         objModulePermission.RoleID,
                                                                                         objModulePermission.UserID,
                                                                                         true);
                        }
                    }
					
                    //Get the custom Module Permissions,  Assume that roles with Edit Tab Permissions
                    //are automatically assigned to the Custom Module Permissions
                    if (objTabPermission.PermissionKey == "EDIT")
                    {
                        ArrayList arrCustomModulePermissions = objPermissionController.GetPermissionsByModuleDefID(objModule.ModuleDefID);

                        //loop through the custom module permissions
                        for (j = 0; j <= arrCustomModulePermissions.Count - 1; j++)
                        {
							//create the module permission
                            PermissionInfo objCustomModulePermission;
                            objCustomModulePermission = (PermissionInfo) arrCustomModulePermissions[j];
                            AddModulePermission(objModule, objCustomModulePermission, objTabPermission.RoleID, objTabPermission.UserID, objTabPermission.AllowAccess);
                        }
                    }
                }

                if (PortalSettings.Current.ContentLocalizationEnabled)
                {
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId);
                    //set the culture of the module to that of the tab
                    var tabInfo = new TabController().GetTab(objModule.TabID, PortalSettings.Current.PortalId, false);
                    objModule.CultureCode = tabInfo != null ? tabInfo.CultureCode : defaultLocale.Code;
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
                }

                objModule.AllTabs = false;
                objModule.Alignment = align;
                objModules.AddModule(objModule);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Builds a URL
        /// </summary>
        /// <param name="FriendlyName">The friendly name of the Module</param>
        /// <param name="PortalID">The ID of the portal</param>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string BuildURL(int PortalID, string FriendlyName)
        {
            string strURL = "~/" + Globals.glbDefaultPage;
            var objModules = new ModuleController();
            ModuleInfo objModule = objModules.GetModuleByDefinition(PortalID, FriendlyName);
            if (objModule != null)
            {
                if (PortalID == Null.NullInteger)
                {
                    strURL = Globals.NavigateURL(objModule.TabID, true);
                }
                else
                {
                    strURL = Globals.NavigateURL(objModule.TabID);
                }
            }
            return strURL;
        }

        protected bool GetModulePermission(int PortalID, string FriendlyName)
        {
            bool AllowAccess = Null.NullBoolean;
            var objModules = new ModuleController();
            ModuleInfo objModule = objModules.GetModuleByDefinition(PortalID, FriendlyName);
            if (objModule != null)
            {
                AllowAccess = ModulePermissionController.CanViewModule(objModule);
            }
            return AllowAccess;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the UserMode
        /// </summary>
        /// <param name="userMode">The userMode to set</param>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void SetUserMode(string userMode)
        {
            Personalization.SetProfile("Usability", "UserMode" + PortalSettings.PortalId, userMode.ToUpper());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the current Visible Mode
        /// </summary>
        /// <param name="isVisible">A flag indicating whether the Control Panel should be visible</param>
        /// <history>
        /// 	[cnurse]	01/11/2008  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void SetVisibleMode(bool isVisible)
        {
            Personalization.SetProfile("Usability", "ControlPanelVisible" + PortalSettings.PortalId, isVisible.ToString());
        }

		protected override void OnInit(EventArgs e)
		{
			if (this.Page.Items.Contains(typeof(ControlPanelBase)) && this.Page.Items[typeof(ControlPanelBase)] is ControlPanelBase)
			{
				this.Parent.Controls.Remove(this);
			}
			else
			{
				this.Page.Items[typeof(ControlPanelBase)] = this;
				base.OnInit(e);
			}
			
		}
		
		#endregion
		
		#region Obsolete

        [Obsolete("Deprecated in 5.0. Replaced by SetMode(UserMode).")]
        protected void SetContentMode(bool showContent)
        {
            Personalization.SetProfile("Usability", "ContentVisible" + PortalSettings.PortalId, showContent.ToString());
        }

        [Obsolete("Deprecated in 5.0. Replaced by SetMode(UserMode).")]
        protected void SetPreviewMode(bool isPreview)
        {
            if (isPreview)
            {
                Personalization.SetProfile("Usability", "UserMode" + PortalSettings.PortalId, "View");
            }
            else
            {
                Personalization.SetProfile("Usability", "UserMode" + PortalSettings.PortalId, "Edit");
            }
        }

        [Obsolete("Deprecated in 5.0. Replaced By UserMode.")]
        protected bool ShowContent
        {
            get
            {
                return PortalSettings.UserMode != PortalSettings.Mode.Layout;
            }
        }

        [Obsolete("Deprecated in 5.0. Replaced By UserMode.")]
        protected bool IsPreview
        {
            get
            {
                if (PortalSettings.UserMode == PortalSettings.Mode.Edit)
                {
                    return false;
                }
                return true;
            }
        }
		
		#endregion

        #region Nested type: ViewPermissionType

        protected enum ViewPermissionType
        {
            View = 0,
            Edit = 1
        }

        #endregion
    }
}
