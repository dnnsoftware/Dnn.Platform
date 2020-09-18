// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.UI.WebControls.Internal;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Globals = DotNetNuke.Common.Globals;

    public abstract class PermissionsGrid : Control, INamingContainer
    {
        protected const string PermissionTypeGrant = "True";
        protected const string PermissionTypeDeny = "False";
        protected const string PermissionTypeNull = "Null";
        protected DataGrid rolePermissionsGrid;
        protected DataGrid userPermissionsGrid;
        private ArrayList _permissions;
        private ArrayList _users;
        private DropDownList cboRoleGroups;
        private DropDownList cboSelectRole;
        private LinkButton cmdUser;
        private LinkButton cmdRole;
        private Label lblGroups;
        private Label lblSelectRole;
        private Label lblErrorMessage;
        private Panel pnlPermissions;
        private TextBox txtUser;
        private HiddenField hiddenUserIds;
        private HiddenField roleField;

        private int unAuthUsersRoleId = int.Parse(Globals.glbRoleUnauthUser);

        private int allUsersRoleId = int.Parse(Globals.glbRoleAllUsers);

        public PermissionsGrid()
        {
            this.dtUserPermissions = new DataTable();
            this.dtRolePermissions = new DataTable();
        }

        public TableItemStyle AlternatingItemStyle
        {
            get
            {
                return this.rolePermissionsGrid.AlternatingItemStyle;
            }
        }

        public DataGridColumnCollection Columns
        {
            get
            {
                return this.rolePermissionsGrid.Columns;
            }
        }

        public TableItemStyle FooterStyle
        {
            get
            {
                return this.rolePermissionsGrid.FooterStyle;
            }
        }

        public TableItemStyle HeaderStyle
        {
            get
            {
                return this.rolePermissionsGrid.HeaderStyle;
            }
        }

        public TableItemStyle ItemStyle
        {
            get
            {
                return this.rolePermissionsGrid.ItemStyle;
            }
        }

        public DataGridItemCollection Items
        {
            get
            {
                return this.rolePermissionsGrid.Items;
            }
        }

        public TableItemStyle SelectedItemStyle
        {
            get
            {
                return this.rolePermissionsGrid.SelectedItemStyle;
            }
        }

        /// <summary>
        /// Gets the Id of the Administrator Role.
        /// </summary>
        public int AdministratorRoleId
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().AdministratorRoleId;
            }
        }

        /// <summary>
        /// Gets the Id of the Registered Users Role.
        /// </summary>
        public int RegisteredUsersRoleId
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().RegisteredRoleId;
            }
        }

        /// <summary>
        /// Gets the Id of the Portal.
        /// </summary>
        public int PortalId
        {
            get
            {
                // Obtain PortalSettings from Current Context
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                int portalID;
                if (Globals.IsHostTab(portalSettings.ActiveTab.TabID)) // if we are in host filemanager then we need to pass a null portal id
                {
                    portalID = Null.NullInteger;
                }
                else
                {
                    portalID = portalSettings.PortalId;
                }

                return portalID;
            }
        }

        public bool AutoGenerateColumns
        {
            get
            {
                return this.rolePermissionsGrid.AutoGenerateColumns;
            }

            set
            {
                this.rolePermissionsGrid.AutoGenerateColumns = value;
                this.userPermissionsGrid.AutoGenerateColumns = value;
            }
        }

        public int CellSpacing
        {
            get
            {
                return this.rolePermissionsGrid.CellSpacing;
            }

            set
            {
                this.rolePermissionsGrid.CellSpacing = value;
                this.userPermissionsGrid.CellSpacing = value;
            }
        }

        public GridLines GridLines
        {
            get
            {
                return this.rolePermissionsGrid.GridLines;
            }

            set
            {
                this.rolePermissionsGrid.GridLines = value;
                this.userPermissionsGrid.GridLines = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets and Sets whether a Dynamic Column has been added.
        /// </summary>
        public bool DynamicColumnAdded
        {
            get
            {
                return this.ViewState["ColumnAdded"] != null;
            }

            set
            {
                this.ViewState["ColumnAdded"] = value;
            }
        }

        /// <summary>
        /// Gets the underlying Permissions Data Table.
        /// </summary>
        public DataTable dtRolePermissions { get; private set; }

        /// <summary>
        /// Gets the underlying Permissions Data Table.
        /// </summary>
        public DataTable dtUserPermissions { get; private set; }

        /// <summary>
        /// Gets or sets and Sets the collection of Roles to display.
        /// </summary>
        public ArrayList Roles { get; set; }

        /// <summary>
        /// Gets or sets and Sets the ResourceFile to localize permissions.
        /// </summary>
        public string ResourceFile { get; set; }

        protected virtual List<PermissionInfoBase> PermissionsList
        {
            get
            {
                return null;
            }
        }

        protected virtual bool RefreshGrid
        {
            get
            {
                return false;
            }
        }

        private int UnAuthUsersRoleId
        {
            get { return this.unAuthUsersRoleId; }
        }

        private int AllUsersRoleId
        {
            get
            {
                return this.allUsersRoleId;
            }
        }

        /// <summary>
        /// Registers the scripts neccesary to make the tri-state controls work inside a RadAjaxPanel.
        /// </summary>
        /// <remarks>
        /// No need to call this unless using the PermissionGrid inside an ajax control that omits scripts on postback
        /// See DesktopModules/Admin/Tabs.ascx.cs for an example of usage.
        /// </remarks>
        public void RegisterScriptsForAjaxPanel()
        {
            PermissionTriState.RegisterScripts(this.Page, this);
        }

        /// <summary>
        /// Generate the Data Grid.
        /// </summary>
        public abstract void GenerateDataGrid();

        protected virtual void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
        }

        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permissions">The permissions collection.</param>
        /// <param name="user">The user to add.</param>
        protected virtual void AddPermission(ArrayList permissions, UserInfo user)
        {
        }

        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permissions">The permissions collection.</param>
        /// <param name="role">The role to add.</param>
        protected virtual void AddPermission(ArrayList permissions, RoleInfo role)
        {
        }

        /// <summary>
        /// Builds the key used to store the "permission" information in the ViewState.
        /// </summary>
        /// <param name="allowAccess">The type of permission ( grant / deny ).</param>
        /// <param name="permissionId">The Id of the permission.</param>
        /// <param name="objectPermissionId">The Id of the object permission.</param>
        /// <param name="roleId">The role id.</param>
        /// <param name="roleName">The role name.</param>
        /// <returns></returns>
        protected string BuildKey(bool allowAccess, int permissionId, int objectPermissionId, int roleId, string roleName)
        {
            return this.BuildKey(allowAccess, permissionId, objectPermissionId, roleId, roleName, Null.NullInteger, Null.NullString);
        }

        /// <summary>
        /// Builds the key used to store the "permission" information in the ViewState.
        /// </summary>
        /// <param name="allowAccess">The type of permission ( grant / deny ).</param>
        /// <param name="permissionId">The Id of the permission.</param>
        /// <param name="objectPermissionId">The Id of the object permission.</param>
        /// <param name="roleId">The role id.</param>
        /// <param name="roleName">The role name.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="displayName">The user display name.</param>
        /// <returns></returns>
        protected string BuildKey(bool allowAccess, int permissionId, int objectPermissionId, int roleId, string roleName, int userID, string displayName)
        {
            string key;
            if (allowAccess)
            {
                key = "True";
            }
            else
            {
                key = "False";
            }

            key += "|" + Convert.ToString(permissionId);
            key += "|";
            if (objectPermissionId > -1)
            {
                key += Convert.ToString(objectPermissionId);
            }

            key += "|" + roleName;
            key += "|" + roleId;
            key += "|" + userID;
            key += "|" + displayName;

            return key;
        }

        /// <summary>
        /// Creates the Child Controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            this._permissions = this.GetPermissions();

            this.pnlPermissions = new Panel { CssClass = "dnnGrid dnnPermissionsGrid" };

            // Optionally Add Role Group Filter
            this.CreateAddRoleControls();

            this.rolePermissionsGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CellSpacing = 0,
                CellPadding = 2,
                GridLines = GridLines.None,
                CssClass = "dnnPermissionsGrid",
            };
            this.rolePermissionsGrid.FooterStyle.CssClass = "dnnGridFooter";
            this.rolePermissionsGrid.HeaderStyle.CssClass = "dnnGridHeader";
            this.rolePermissionsGrid.ItemStyle.CssClass = "dnnGridItem";
            this.rolePermissionsGrid.AlternatingItemStyle.CssClass = "dnnGridAltItem";
            this.rolePermissionsGrid.ItemDataBound += this.rolePermissionsGrid_ItemDataBound;
            this.SetUpRolesGrid();
            this.pnlPermissions.Controls.Add(this.rolePermissionsGrid);

            this._users = this.GetUsers();
            if (this._users != null)
            {
                this.userPermissionsGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    CellSpacing = 0,
                    GridLines = GridLines.None,
                    CssClass = "dnnPermissionsGrid",
                };
                this.userPermissionsGrid.FooterStyle.CssClass = "dnnGridFooter";
                this.userPermissionsGrid.HeaderStyle.CssClass = "dnnGridHeader";
                this.userPermissionsGrid.ItemStyle.CssClass = "dnnGridItem";
                this.userPermissionsGrid.AlternatingItemStyle.CssClass = "dnnGridAltItem";

                this.SetUpUsersGrid();
                this.pnlPermissions.Controls.Add(this.userPermissionsGrid);

                var divAddUser = new Panel { CssClass = "dnnFormItem" };

                this.lblErrorMessage = new Label { Text = Localization.GetString("DisplayName") };
                this.txtUser = new TextBox { ID = "txtUser" };
                this.lblErrorMessage.AssociatedControlID = this.txtUser.ID;

                this.hiddenUserIds = new HiddenField { ID = "hiddenUserIds" };

                divAddUser.Controls.Add(this.lblErrorMessage);
                divAddUser.Controls.Add(this.txtUser);
                divAddUser.Controls.Add(this.hiddenUserIds);

                this.cmdUser = new LinkButton { Text = Localization.GetString("Add"), CssClass = "dnnSecondaryAction" };
                divAddUser.Controls.Add(this.cmdUser);
                this.cmdUser.Click += this.AddUser;

                this.pnlPermissions.Controls.Add(divAddUser);
            }

            this.Controls.Add(this.pnlPermissions);
        }

        /// <summary>
        /// Gets the Enabled status of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns></returns>
        protected virtual bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return true;
        }

        /// <summary>
        /// Gets the Enabled status of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns></returns>
        protected virtual bool GetEnabled(PermissionInfo objPerm, UserInfo user, int column)
        {
            return true;
        }

        /// <summary>
        /// Gets the Value of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns></returns>
        protected virtual bool GetPermission(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return Convert.ToBoolean(this.GetPermission(objPerm, role, column, PermissionTypeDeny));
        }

        /// <summary>
        /// Gets the Value of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns></returns>
        protected virtual string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string stateKey = defaultState;
            if (this.PermissionsList != null)
            {
                foreach (PermissionInfoBase permission in this.PermissionsList)
                {
                    if (permission.PermissionID == objPerm.PermissionID && permission.RoleID == role.RoleID)
                    {
                        if (permission.AllowAccess)
                        {
                            stateKey = PermissionTypeGrant;
                        }
                        else
                        {
                            stateKey = PermissionTypeDeny;
                        }

                        break;
                    }
                }
            }

            return stateKey;
        }

        /// <summary>
        /// Gets the Value of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns></returns>
        protected virtual bool GetPermission(PermissionInfo objPerm, UserInfo user, int column)
        {
            return Convert.ToBoolean(this.GetPermission(objPerm, user, column, PermissionTypeDeny));
        }

        /// <summary>
        /// Gets the Value of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns></returns>
        protected virtual string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
        {
            var stateKey = defaultState;
            if (this.PermissionsList != null)
            {
                foreach (var permission in this.PermissionsList)
                {
                    if (permission.PermissionID == objPerm.PermissionID && permission.UserID == user.UserID)
                    {
                        if (permission.AllowAccess)
                        {
                            stateKey = PermissionTypeGrant;
                        }
                        else
                        {
                            stateKey = PermissionTypeDeny;
                        }

                        break;
                    }
                }
            }

            return stateKey;
        }

        /// <summary>
        /// Gets the permissions from the Database.
        /// </summary>
        /// <returns></returns>
        protected virtual ArrayList GetPermissions()
        {
            return null;
        }

        /// <summary>
        /// Gets the users from the Database.
        /// </summary>
        /// <returns></returns>
        protected virtual ArrayList GetUsers()
        {
            var arrUsers = new ArrayList();
            UserInfo objUser;
            if (this.PermissionsList != null)
            {
                foreach (var permission in this.PermissionsList)
                {
                    if (!Null.IsNull(permission.UserID))
                    {
                        bool blnExists = false;
                        foreach (UserInfo user in arrUsers)
                        {
                            if (permission.UserID == user.UserID)
                            {
                                blnExists = true;
                            }
                        }

                        if (!blnExists)
                        {
                            objUser = new UserInfo();
                            objUser.UserID = permission.UserID;
                            objUser.Username = permission.Username;
                            objUser.DisplayName = permission.DisplayName;
                            arrUsers.Add(objUser);
                        }
                    }
                }
            }

            return arrUsers;
        }

        protected virtual bool IsFullControl(PermissionInfo permissionInfo)
        {
            return false;
        }

        protected virtual bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return false;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterScript(this.Page, "~/js/dnn.permissiongrid.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css", FileOrder.Css.ResourceCss);
        }

        /// <summary>
        /// Overrides the base OnPreRender method to Bind the Grid to the Permissions.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            this.BindData();

            var script = "var pgm = new dnn.permissionGridManager('" + this.ClientID + "');";
            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "PermissionGridManager", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "PermissionGridManager", script, true);
            }
        }

        protected virtual void ParsePermissionKeys(PermissionInfoBase permission, string[] Settings)
        {
            permission.PermissionID = Convert.ToInt32(Settings[1]);
            permission.RoleID = Convert.ToInt32(Settings[4]);
            permission.RoleName = Settings[3];
            permission.AllowAccess = Convert.ToBoolean(Settings[0]);
            permission.UserID = Convert.ToInt32(Settings[5]);
            permission.DisplayName = Settings[6];
        }

        protected virtual void RemovePermission(int permissionID, int roleID, int userID)
        {
        }

        protected virtual bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            // to maintain backward compatibility the base implementation must always call the simple parameterless version of this method
            return false;
        }

        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="roleId">Rold Id.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="allowAccess">The value of the permission.</param>
        protected virtual void UpdatePermission(PermissionInfo permission, int roleId, string roleName, bool allowAccess)
        {
            this.UpdatePermission(permission, roleId, roleName, allowAccess ? PermissionTypeGrant : PermissionTypeNull);
        }

        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="roleId">Role Id.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="stateKey">The permission state.</param>
        protected virtual void UpdatePermission(PermissionInfo permission, int roleId, string roleName, string stateKey)
        {
            this.RemovePermission(permission.PermissionID, roleId, Null.NullInteger);
            switch (stateKey)
            {
                case PermissionTypeGrant:
                    this.AddPermission(permission, roleId, roleName, Null.NullInteger, Null.NullString, true);
                    break;
                case PermissionTypeDeny:
                    this.AddPermission(permission, roleId, roleName, Null.NullInteger, Null.NullString, false);
                    break;
            }
        }

        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="displayName">The user's displayname.</param>
        /// <param name="userId">The user's id.</param>
        /// <param name="allowAccess">The value of the permission.</param>
        protected virtual void UpdatePermission(PermissionInfo permission, string displayName, int userId, bool allowAccess)
        {
            this.UpdatePermission(permission, displayName, userId, allowAccess ? PermissionTypeGrant : PermissionTypeNull);
        }

        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="displayName">The user's displayname.</param>
        /// <param name="userId">The user's id.</param>
        /// <param name="stateKey">The permission state.</param>
        protected virtual void UpdatePermission(PermissionInfo permission, string displayName, int userId, string stateKey)
        {
            this.RemovePermission(permission.PermissionID, int.Parse(Globals.glbRoleNothing), userId);
            switch (stateKey)
            {
                case PermissionTypeGrant:
                    this.AddPermission(permission, int.Parse(Globals.glbRoleNothing), Null.NullString, userId, displayName, true);
                    break;
                case PermissionTypeDeny:
                    this.AddPermission(permission, int.Parse(Globals.glbRoleNothing), Null.NullString, userId, displayName, false);
                    break;
            }
        }

        /// <summary>
        /// Updates the permissions.
        /// </summary>
        protected void UpdatePermissions()
        {
            this.EnsureChildControls();
            this.GetRoles();
            this.UpdateRolePermissions();
            this._users = this.GetUsers();
            this.UpdateUserPermissions();
        }

        /// <summary>
        /// Updates the permissions.
        /// </summary>
        protected void UpdateRolePermissions()
        {
            if (this.rolePermissionsGrid != null && !this.RefreshGrid)
            {
                var rolesList = this.Roles.Cast<RoleInfo>().ToList();
                foreach (DataGridItem dgi in this.rolePermissionsGrid.Items)
                {
                    var roleId = int.Parse(dgi.Cells[1].Text);
                    if (rolesList.All(r => r.RoleID != roleId))
                    {
                        continue;
                    }

                    for (int i = 2; i <= dgi.Cells.Count - 2; i++)
                    {
                        // all except first two cells which is role names and role ids and last column is Actions
                        if (dgi.Cells[i].Controls.Count > 0)
                        {
                            var permissionInfo = (PermissionInfo)this._permissions[i - 2];
                            var triState = (PermissionTriState)dgi.Cells[i].Controls[0];
                            if (this.SupportsDenyPermissions(permissionInfo))
                            {
                                this.UpdatePermission(permissionInfo, roleId, dgi.Cells[0].Text, triState.Value);
                            }
                            else
                            {
                                this.UpdatePermission(permissionInfo, roleId, dgi.Cells[0].Text, triState.Value == PermissionTypeGrant);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the permissions.
        /// </summary>
        protected void UpdateUserPermissions()
        {
            if (this.userPermissionsGrid != null && !this.RefreshGrid)
            {
                var usersList = this._users.Cast<UserInfo>().ToList();
                foreach (DataGridItem dgi in this.userPermissionsGrid.Items)
                {
                    var userId = int.Parse(dgi.Cells[1].Text);
                    if (usersList.All(u => u.UserID != userId))
                    {
                        continue;
                    }

                    for (int i = 2; i <= dgi.Cells.Count - 2; i++)
                    {
                        // all except first two cells which is displayname and userid and Last column is Actions
                        if (dgi.Cells[i].Controls.Count > 0)
                        {
                            var permissionInfo = (PermissionInfo)this._permissions[i - 2];
                            var triState = (PermissionTriState)dgi.Cells[i].Controls[0];
                            if (this.SupportsDenyPermissions(permissionInfo))
                            {
                                this.UpdatePermission(permissionInfo, dgi.Cells[0].Text, userId, triState.Value);
                            }
                            else
                            {
                                this.UpdatePermission(permissionInfo, dgi.Cells[0].Text, userId, triState.Value == PermissionTypeGrant);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// RoleGroupsSelectedIndexChanged runs when the Role Group is changed.
        /// </summary>
        protected virtual void RoleGroupsSelectedIndexChanged(object sender, EventArgs e)
        {
            this.FillSelectRoleComboBox(int.Parse(this.cboRoleGroups.SelectedValue));
        }

        /// <summary>
        /// AddUser runs when the Add user linkbutton is clicked.
        /// </summary>
        protected virtual void AddUser(object sender, EventArgs e)
        {
            this.UpdatePermissions();
            if (!string.IsNullOrEmpty(this.hiddenUserIds.Value))
            {
                foreach (var id in this.hiddenUserIds.Value.Split(','))
                {
                    var userId = Convert.ToInt32(id);
                    var user = UserController.GetUserById(this.PortalId, userId);
                    if (user != null)
                    {
                        this.AddPermission(this._permissions, user);
                        this.BindData();
                    }
                }

                this.txtUser.Text = this.hiddenUserIds.Value = string.Empty;
            }
        }

        private void BindData()
        {
            this.EnsureChildControls();
            this.BindRolesGrid();
            this.BindUsersGrid();
        }

        private void BindRolesGrid()
        {
            this.dtRolePermissions.Columns.Clear();
            this.dtRolePermissions.Rows.Clear();

            // Add Roles Column
            this.dtRolePermissions.Columns.Add(new DataColumn("RoleId"));

            // Add Roles Column
            this.dtRolePermissions.Columns.Add(new DataColumn("RoleName"));

            for (int i = 0; i <= this._permissions.Count - 1; i++)
            {
                var permissionInfo = (PermissionInfo)this._permissions[i];

                // Add Enabled Column
                this.dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName + "_Enabled"));

                // Add Permission Column
                this.dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName));
            }

            this.GetRoles();

            this.UpdateRolePermissions();
            for (int i = 0; i <= this.Roles.Count - 1; i++)
            {
                var role = (RoleInfo)this.Roles[i];
                var row = this.dtRolePermissions.NewRow();
                row["RoleId"] = role.RoleID;
                row["RoleName"] = Localization.LocalizeRole(role.RoleName);
                int j;
                for (j = 0; j <= this._permissions.Count - 1; j++)
                {
                    PermissionInfo objPerm;
                    objPerm = (PermissionInfo)this._permissions[j];
                    row[objPerm.PermissionName + "_Enabled"] = this.GetEnabled(objPerm, role, j + 1);
                    if (this.SupportsDenyPermissions(objPerm))
                    {
                        row[objPerm.PermissionName] = this.GetPermission(objPerm, role, j + 1, PermissionTypeNull);
                    }
                    else
                    {
                        if (this.GetPermission(objPerm, role, j + 1))
                        {
                            row[objPerm.PermissionName] = PermissionTypeGrant;
                        }
                        else
                        {
                            row[objPerm.PermissionName] = PermissionTypeNull;
                        }
                    }
                }

                this.dtRolePermissions.Rows.Add(row);
            }

            this.rolePermissionsGrid.DataSource = this.dtRolePermissions;
            this.rolePermissionsGrid.DataBind();
        }

        private void BindUsersGrid()
        {
            this.dtUserPermissions.Columns.Clear();
            this.dtUserPermissions.Rows.Clear();

            // Add Roles Column
            var col = new DataColumn("UserId");
            this.dtUserPermissions.Columns.Add(col);

            // Add Roles Column
            col = new DataColumn("DisplayName");
            this.dtUserPermissions.Columns.Add(col);
            int i;
            for (i = 0; i <= this._permissions.Count - 1; i++)
            {
                PermissionInfo objPerm;
                objPerm = (PermissionInfo)this._permissions[i];

                // Add Enabled Column
                col = new DataColumn(objPerm.PermissionName + "_Enabled");
                this.dtUserPermissions.Columns.Add(col);

                // Add Permission Column
                col = new DataColumn(objPerm.PermissionName);
                this.dtUserPermissions.Columns.Add(col);
            }

            if (this.userPermissionsGrid != null)
            {
                this._users = this.GetUsers();

                if (this._users.Count != 0)
                {
                    this.userPermissionsGrid.Visible = true;
                    DataRow row;
                    for (i = 0; i <= this._users.Count - 1; i++)
                    {
                        var user = (UserInfo)this._users[i];
                        row = this.dtUserPermissions.NewRow();
                        row["UserId"] = user.UserID;
                        row["DisplayName"] = user.DisplayName;
                        int j;
                        for (j = 0; j <= this._permissions.Count - 1; j++)
                        {
                            PermissionInfo objPerm;
                            objPerm = (PermissionInfo)this._permissions[j];
                            row[objPerm.PermissionName + "_Enabled"] = this.GetEnabled(objPerm, user, j + 1);
                            if (this.SupportsDenyPermissions(objPerm))
                            {
                                row[objPerm.PermissionName] = this.GetPermission(objPerm, user, j + 1, PermissionTypeNull);
                            }
                            else
                            {
                                if (this.GetPermission(objPerm, user, j + 1))
                                {
                                    row[objPerm.PermissionName] = PermissionTypeGrant;
                                }
                                else
                                {
                                    row[objPerm.PermissionName] = PermissionTypeNull;
                                }
                            }
                        }

                        this.dtUserPermissions.Rows.Add(row);
                    }

                    this.userPermissionsGrid.DataSource = this.dtUserPermissions;
                    this.userPermissionsGrid.DataBind();
                }
                else
                {
                    this.dtUserPermissions.Rows.Clear();
                    this.userPermissionsGrid.DataSource = this.dtUserPermissions;
                    this.userPermissionsGrid.DataBind();
                    this.userPermissionsGrid.Visible = false;
                }
            }
        }

        private void EnsureRole(RoleInfo role)
        {
            if (this.Roles.Cast<RoleInfo>().All(r => r.RoleID != role.RoleID))
            {
                this.Roles.Add(role);
            }
        }

        private void GetRoles()
        {
            var checkedRoles = this.GetCheckedRoles();
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            this.Roles = new ArrayList(RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved && checkedRoles.Contains(r.RoleID)).ToArray());

            if (checkedRoles.Contains(this.UnAuthUsersRoleId))
            {
                this.Roles.Add(new RoleInfo { RoleID = this.UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });
            }

            if (checkedRoles.Contains(this.AllUsersRoleId))
            {
                this.Roles.Add(new RoleInfo { RoleID = this.AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });
            }

            // Administrators Role always has implicit permissions, then it should be always in
            this.EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId));

            // Show also default roles
            this.EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.RegisteredRoleId));
            this.EnsureRole(new RoleInfo { RoleID = this.AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });

            this.Roles.Reverse();
            this.Roles.Sort(new RoleComparer());
        }

        private IEnumerable<int> GetCheckedRoles()
        {
            if (this.PermissionsList == null)
            {
                return new List<int>();
            }

            return this.PermissionsList.Select(r => r.RoleID).Distinct();
        }

        private void SetUpGrid(DataGrid grid, string nameColumnDataField, string idColumnDataField, string permissionHeaderText)
        {
            grid.Columns.Clear();
            var nameColumn = new BoundColumn
            {
                HeaderText = permissionHeaderText,
                DataField = nameColumnDataField,
            };
            nameColumn.ItemStyle.CssClass = "permissionHeader";
            nameColumn.HeaderStyle.CssClass = "permissionHeader";
            grid.Columns.Add(nameColumn);

            var idColumn = new BoundColumn
            {
                HeaderText = string.Empty,
                DataField = idColumnDataField,
                Visible = false,
            };
            grid.Columns.Add(idColumn);

            foreach (PermissionInfo permission in this._permissions)
            {
                var templateCol = new TemplateColumn();
                var columnTemplate = new PermissionTriStateTemplate(permission)
                {
                    IsFullControl = this.IsFullControl(permission),
                    IsView = this.IsViewPermisison(permission),
                    SupportDenyMode = this.SupportsDenyPermissions(permission),
                };
                templateCol.ItemTemplate = columnTemplate;

                var locName = (permission.ModuleDefID <= 0) ? Localization.GetString(permission.PermissionName + ".Permission", PermissionProvider.Instance().LocalResourceFile) // system permission
                                                            : (!string.IsNullOrEmpty(this.ResourceFile) ? Localization.GetString(permission.PermissionName + ".Permission", this.ResourceFile) // custom permission
                                                                                                    : string.Empty);
                templateCol.HeaderText = !string.IsNullOrEmpty(locName) ? locName : permission.PermissionName;
                templateCol.HeaderStyle.Wrap = true;
                grid.Columns.Add(templateCol);
            }

            var actionsColumn = new ImageCommandColumn
            {
                CommandName = "Delete/" + nameColumnDataField,
                KeyField = idColumnDataField,
                IconKey = "Delete",
                IconSize = "16x16",
                IconStyle = "PermissionGrid",
                HeaderText = Localization.GetString("PermissionActionsHeader.Text", PermissionProvider.Instance().LocalResourceFile),
            };
            grid.Columns.Add(actionsColumn);
            grid.ItemCommand += this.grid_ItemCommand;
        }

        private void grid_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            var entityID = int.Parse(e.CommandArgument.ToString());
            var command = this.GetGridCommand(e.CommandName);
            var entityType = this.GetCommandType(e.CommandName);
            switch (command)
            {
                case "DELETE":
                    if (entityType == "ROLE")
                    {
                        this.DeleteRolePermissions(entityID);
                    }
                    else if (entityType == "USER")
                    {
                        this.DeleteUserPermissions(entityID);
                    }

                    this.BindData();
                    break;
            }
        }

        private void DeleteRolePermissions(int entityID)
        {
            // PermissionsList.RemoveAll(p => p.RoleID == entityID);
            var permissionToDelete = this.PermissionsList.Where(p => p.RoleID == entityID);
            foreach (PermissionInfoBase permission in permissionToDelete)
            {
                this.RemovePermission(permission.PermissionID, entityID, permission.UserID);
            }
        }

        private void DeleteUserPermissions(int entityID)
        {
            var permissionToDelete = this.PermissionsList.Where(p => p.UserID == entityID);
            foreach (PermissionInfoBase permission in permissionToDelete)
            {
                this.RemovePermission(permission.PermissionID, permission.RoleID, entityID);
            }
        }

        private string GetCommandType(string commandName)
        {
            var command = commandName.ToLower(CultureInfo.InvariantCulture);
            if (command.Contains("rolename"))
            {
                return "ROLE";
            }

            if (command.Contains("displayname"))
            {
                return "USER";
            }

            return Null.NullString;
        }

        private string GetGridCommand(string commandName)
        {
            var commandParts = commandName.Split('/');
            return commandParts[0].ToUpper(CultureInfo.InvariantCulture);
        }

        private void SetUpRolesGrid()
        {
            this.SetUpGrid(this.rolePermissionsGrid, "RoleName", "roleid", Localization.GetString("PermissionRoleHeader.Text", PermissionProvider.Instance().LocalResourceFile));
        }

        private void SetUpUsersGrid()
        {
            if (this.userPermissionsGrid != null)
            {
                this.SetUpGrid(this.userPermissionsGrid, "DisplayName", "userid", Localization.GetString("PermissionUserHeader.Text", PermissionProvider.Instance().LocalResourceFile));
            }
        }

        private void FillSelectRoleComboBox(int selectedRoleGroupId)
        {
            this.cboSelectRole.Items.Clear();
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var groupRoles = (selectedRoleGroupId > -2) ? RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == selectedRoleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);

            if (selectedRoleGroupId < 0)
            {
                groupRoles.Add(new RoleInfo { RoleID = this.UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });
                groupRoles.Add(new RoleInfo { RoleID = this.AllUsersRoleId, RoleName = Globals.glbRoleAllUsersName });
            }

            foreach (var role in groupRoles.OrderBy(r => r.RoleName))
            {
                this.cboSelectRole.Items.Add(new ListItem(role.RoleName, role.RoleID.ToString(CultureInfo.InvariantCulture)));
            }

            int[] defaultRoleIds = { this.AllUsersRoleId, portalSettings.RegisteredRoleId, portalSettings.AdministratorRoleId };
            var itemToSelect = this.cboSelectRole.Items.Cast<ListItem>().FirstOrDefault(i => !defaultRoleIds.Contains(int.Parse(i.Value)));
            if (itemToSelect != null)
            {
                this.cboSelectRole.SelectedValue = itemToSelect.Value;
            }
        }

        private void SetErrorMessage(string errorKey)
        {
            this.lblErrorMessage = new Label
            {
                // TODO Remove DEBUG test
                Text = "<br />" + (errorKey.StartsWith("DEBUG") ? errorKey : Localization.GetString(errorKey)),
                CssClass = "NormalRed",
            };
            this.pnlPermissions.Controls.Add(this.lblErrorMessage);
        }

        private void rolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var roleID = int.Parse(((DataRowView)item.DataItem)[0].ToString());
                if (roleID == PortalSettings.Current.AdministratorRoleId || roleID == this.AllUsersRoleId || roleID == PortalSettings.Current.RegisteredRoleId)
                {
                    var actionImage = item.Controls.Cast<Control>().Last().Controls[0] as ImageButton;
                    if (actionImage != null)
                    {
                        actionImage.Visible = false;
                    }
                }
            }
        }

        private void CreateAddRoleControls()
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var arrGroups = RoleController.GetRoleGroups(portalSettings.PortalId);

            var addRoleControls = new Panel { CssClass = "dnnFormItem" };
            var divRoleGroups = new Panel { CssClass = "leftGroup" };
            var divSelectRole = new Panel { CssClass = "rightGroup" };

            this.lblGroups = new Label { Text = Localization.GetString("RoleGroupFilter") };
            this.cboRoleGroups = new DropDownList { AutoPostBack = true, ID = "cboRoleGroups", ViewStateMode = ViewStateMode.Disabled };
            this.lblGroups.AssociatedControlID = this.cboRoleGroups.ID;
            divRoleGroups.Controls.Add(this.lblGroups);

            this.cboRoleGroups.SelectedIndexChanged += this.RoleGroupsSelectedIndexChanged;
            this.cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
            var liItem = new ListItem(Localization.GetString("GlobalRoles"), "-1") { Selected = true };
            this.cboRoleGroups.Items.Add(liItem);
            foreach (RoleGroupInfo roleGroup in arrGroups)
            {
                this.cboRoleGroups.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString(CultureInfo.InvariantCulture)));
            }

            divRoleGroups.Controls.Add(this.cboRoleGroups);
            addRoleControls.Controls.Add(divRoleGroups);

            this.lblSelectRole = new Label { Text = Localization.GetString("RoleSelect") };
            this.cboSelectRole = new DropDownList { ID = "cboSelectRole", ViewStateMode = ViewStateMode.Disabled };
            this.lblSelectRole.AssociatedControlID = this.cboSelectRole.ID;
            divSelectRole.Controls.Add(this.lblSelectRole);

            this.FillSelectRoleComboBox(-1); // Default Role Group is Global Roles
            divSelectRole.Controls.Add(this.cboSelectRole);

            this.cmdRole = new LinkButton { Text = Localization.GetString("Add"), CssClass = "dnnSecondaryAction" };
            this.cmdRole.Click += this.AddRole;
            divSelectRole.Controls.Add(this.cmdRole);
            this.roleField = new HiddenField() { ID = "roleField" };
            addRoleControls.Controls.Add(this.roleField);

            addRoleControls.Controls.Add(divSelectRole);

            this.pnlPermissions.Controls.Add(addRoleControls);
        }

        /// <summary>
        /// AddRole runs when the Add Role linkbutton is clicked.
        /// </summary>
        private void AddRole(object sender, EventArgs e)
        {
            this.UpdatePermissions();
            int selectedRoleId;
            if (!int.TryParse(this.roleField.Value, out selectedRoleId))
            {
                // Role not selected
                this.SetErrorMessage("InvalidRoleId");
                return;
            }

            // verify role
            var role = this.GetSelectedRole(selectedRoleId);
            if (role != null)
            {
                this.AddPermission(this._permissions, role);
                this.BindData();
            }
            else
            {
                // role does not exist
                this.SetErrorMessage("RoleNotFound");
            }
        }

        private RoleInfo GetSelectedRole(int selectedRoleId)
        {
            RoleInfo role = null;
            if (selectedRoleId == this.AllUsersRoleId)
            {
                role = new RoleInfo
                {
                    RoleID = this.AllUsersRoleId,
                    RoleName = Globals.glbRoleAllUsersName,
                };
            }
            else if (selectedRoleId == this.UnAuthUsersRoleId)
            {
                role = new RoleInfo
                {
                    RoleID = this.UnAuthUsersRoleId,
                    RoleName = Globals.glbRoleUnauthUserName,
                };
            }
            else
            {
                role = RoleController.Instance.GetRoleById(this.PortalId, selectedRoleId);
            }

            return role;
        }
    }
}
