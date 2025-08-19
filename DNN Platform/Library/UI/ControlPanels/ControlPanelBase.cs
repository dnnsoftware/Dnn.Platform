// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanels
{
    using System;
    using System.Collections;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
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

    /// <summary>The ControlPanel class defines a custom base class inherited by all ControlPanel controls.</summary>
    public class ControlPanelBase : UserControl
    {
        private string localResourceFile;

        protected enum ViewPermissionType
        {
            /// <summary>Permission to view.</summary>
            View = 0,

            /// <summary>Permission to edit.</summary>
            Edit = 1,
        }

        public virtual bool IncludeInControlHierarchy
        {
            get { return true; }
        }

        /// <summary>Gets or sets the Local ResourceFile for the Control Panel.</summary>
        /// <value>A String.</value>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + this.ID;
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        public virtual bool IsDockable
        {
            get { return false; }
            set { }
        }

        /// <summary>Gets a value indicating whether the ControlPanel is Visible.</summary>
        protected bool IsVisible
        {
            get
            {
                return this.PortalSettings.ControlPanelVisible;
            }
        }

        /// <summary>Gets the current Portal Settings.</summary>
        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>Gets the User mode of the Control Panel.</summary>
        /// <value>A Boolean.</value>
        protected PortalSettings.Mode UserMode
        {
            get
            {
                return Personalization.GetUserMode();
            }
        }

        internal static bool IsModuleAdminInternal()
        {
            bool isModuleAdmin = Null.NullBoolean;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }

            return PortalController.Instance.GetCurrentPortalSettings().ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }

        internal static bool IsPageAdminInternal()
        {
            bool isPageAdmin = Null.NullBoolean;
            if (TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage())
            {
                isPageAdmin = true;
            }

            return isPageAdmin;
        }

        protected bool IsModuleAdmin()
        {
            return IsModuleAdminInternal();
        }

        protected bool IsPageAdmin()
        {
            return IsPageAdminInternal();
        }

        /// <summary>Adds an Existing Module to a Pane.</summary>
        /// <param name="moduleId">The Id of the existing module.</param>
        /// <param name="tabId">The id of the tab.</param>
        /// <param name="paneName">The pane to add the module to.</param>
        /// <param name="position">The relative position within the pane for the module.</param>
        /// <param name="align">The alignment for the Module.</param>
        protected void AddExistingModule(int moduleId, int tabId, string paneName, int position, string align)
        {
            ModuleInfo objModule;

            int userId = -1;
            if (this.Request.IsAuthenticated)
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                userId = objUserInfo.UserID;
            }

            objModule = ModuleController.Instance.GetModule(moduleId, tabId, false);
            if (objModule != null)
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo objClone = objModule.Clone();
                objClone.TabID = this.PortalSettings.ActiveTab.TabID;
                objClone.ModuleOrder = position;
                objClone.PaneName = paneName;
                objClone.Alignment = align;
                ModuleController.Instance.AddModule(objClone);
                EventLogController.Instance.AddLog(objClone, this.PortalSettings, userId, string.Empty, EventLogController.EventLogType.MODULE_CREATED);
            }
        }

        /// <summary>Adds a New Module to a Pane. </summary>
        /// <param name="title">The Title for the resulting module.</param>
        /// <param name="desktopModuleId">The Id of the DesktopModule.</param>
        /// <param name="paneName">The pane to add the module to.</param>
        /// <param name="position">The relative position within the pane for the module.</param>
        /// <param name="permissionType">The View Permission Type for the Module.</param>
        /// <param name="align">The alignment for the Module.</param>
        protected void AddNewModule(string title, int desktopModuleId, string paneName, int position, ViewPermissionType permissionType, string align)
        {
            TabPermissionCollection objTabPermissions = this.PortalSettings.ActiveTab.TabPermissions;
            var objPermissionController = new PermissionController();
            try
            {
                DesktopModuleInfo desktopModule;
                if (!DesktopModuleController.GetDesktopModules(this.PortalSettings.PortalId).TryGetValue(desktopModuleId, out desktopModule))
                {
                    throw new ArgumentException("desktopModuleId");
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            int userId = -1;
            if (this.Request.IsAuthenticated)
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                userId = objUserInfo.UserID;
            }

            foreach (ModuleDefinitionInfo objModuleDefinition in
                ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
                objModule.Initialize(this.PortalSettings.PortalId);
                objModule.PortalID = this.PortalSettings.PortalId;
                objModule.TabID = this.PortalSettings.ActiveTab.TabID;
                objModule.ModuleOrder = position;
                if (string.IsNullOrEmpty(title))
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
                        ModuleInfo defaultModule = ModuleController.Instance.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
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

                // get the default module view permissions
                ArrayList arrSystemModuleViewPermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");

                // get the permissions from the page
                foreach (TabPermissionInfo objTabPermission in objTabPermissions)
                {
                    if (objTabPermission.PermissionKey == "VIEW" && permissionType == ViewPermissionType.View)
                    {
                        // Don't need to explicitly add View permisisons if "Same As Page"
                        continue;
                    }

                    // get the system module permissions for the permissionkey
                    ArrayList arrSystemModulePermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey);

                    // loop through the system module permissions
                    int j;
                    for (j = 0; j <= arrSystemModulePermissions.Count - 1; j++)
                    {
                        PermissionInfo objSystemModulePermission;
                        objSystemModulePermission = (PermissionInfo)arrSystemModulePermissions[j];
                        if (objSystemModulePermission.PermissionKey == "VIEW" && permissionType == ViewPermissionType.Edit && objTabPermission.PermissionKey != "EDIT")
                        {
                            // Only Page Editors get View permissions if "Page Editors Only"
                            continue;
                        }

                        ModulePermissionInfo objModulePermission = this.AddModulePermission(
                            objModule,
                            objSystemModulePermission,
                            objTabPermission.RoleID,
                            objTabPermission.UserID,
                            objTabPermission.AllowAccess);

                        // ensure that every EDIT permission which allows access also provides VIEW permission
                        if (objModulePermission.PermissionKey == "EDIT" && objModulePermission.AllowAccess)
                        {
                            ModulePermissionInfo objModuleViewperm = this.AddModulePermission(
                                objModule,
                                (PermissionInfo)arrSystemModuleViewPermissions[0],
                                objModulePermission.RoleID,
                                objModulePermission.UserID,
                                true);
                        }
                    }

                    // Get the custom Module Permissions,  Assume that roles with Edit Tab Permissions
                    // are automatically assigned to the Custom Module Permissions
                    if (objTabPermission.PermissionKey == "EDIT")
                    {
                        ArrayList arrCustomModulePermissions = objPermissionController.GetPermissionsByModuleDefID(objModule.ModuleDefID);

                        // loop through the custom module permissions
                        for (j = 0; j <= arrCustomModulePermissions.Count - 1; j++)
                        {
                            // create the module permission
                            PermissionInfo objCustomModulePermission;
                            objCustomModulePermission = (PermissionInfo)arrCustomModulePermissions[j];
                            this.AddModulePermission(objModule, objCustomModulePermission, objTabPermission.RoleID, objTabPermission.UserID, objTabPermission.AllowAccess);
                        }
                    }
                }

                if (PortalSettings.Current.ContentLocalizationEnabled)
                {
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId);

                    // set the culture of the module to that of the tab
                    var tabInfo = TabController.Instance.GetTab(objModule.TabID, PortalSettings.Current.PortalId, false);
                    objModule.CultureCode = tabInfo != null ? tabInfo.CultureCode : defaultLocale.Code;
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
                }

                objModule.AllTabs = false;
                objModule.Alignment = align;
                ModuleController.Instance.AddModule(objModule);
            }
        }

        /// <summary>Builds a URL to a page with a module matching the given definition <paramref name="friendlyName"/>.</summary>
        /// <param name="portalID">The ID of the portal.</param>
        /// <param name="friendlyName">The friendly name of the Module.</param>
        /// <returns>A formatted URL.</returns>
        protected string BuildURL(int portalID, string friendlyName)
        {
            string strURL = "~/" + Globals.glbDefaultPage;
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(portalID, friendlyName);
            if (objModule != null)
            {
                if (portalID == Null.NullInteger)
                {
                    strURL = TestableGlobals.Instance.NavigateURL(objModule.TabID, true);
                }
                else
                {
                    strURL = TestableGlobals.Instance.NavigateURL(objModule.TabID);
                }
            }

            return strURL;
        }

        protected bool GetModulePermission(int portalID, string friendlyName)
        {
            bool allowAccess = Null.NullBoolean;
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(portalID, friendlyName);
            if (objModule != null)
            {
                allowAccess = ModulePermissionController.CanViewModule(objModule);
            }

            return allowAccess;
        }

        /// <summary>Sets the UserMode.</summary>
        /// <param name="userMode">The userMode to set.</param>
        protected void SetUserMode(string userMode)
        {
            Personalization.SetProfile("Usability", "UserMode" + this.PortalSettings.PortalId, userMode.ToUpper());
        }

        /// <summary>Sets the current Visible Mode.</summary>
        /// <param name="isVisible">A flag indicating whether the Control Panel should be visible.</param>
        protected void SetVisibleMode(bool isVisible)
        {
            Personalization.SetProfile("Usability", "ControlPanelVisible" + this.PortalSettings.PortalId, isVisible.ToString());
        }

        /// <inheritdoc/>
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

        /// <summary>Adds a Module Permission.</summary>
        /// <param name="objModule">Module Info.</param>
        /// <param name="permission">The permission to add.</param>
        /// <param name="roleId">The Id of the role to add the permission for.</param>
        /// <param name="userId">Operator.</param>
        /// <param name="allowAccess">Whether allow to access the module.</param>
        private ModulePermissionInfo AddModulePermission(ModuleInfo objModule, PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var objModulePermission = new ModulePermissionInfo();
            objModulePermission.ModuleID = objModule.ModuleID;
            objModulePermission.PermissionID = permission.PermissionID;
            objModulePermission.RoleID = roleId;
            objModulePermission.UserID = userId;
            objModulePermission.PermissionKey = permission.PermissionKey;
            objModulePermission.AllowAccess = allowAccess;

            // add the permission to the collection
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }

            return objModulePermission;
        }
    }
}
