#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

#endregion

namespace DotNetNuke.Security.Permissions.Controls
{
    public abstract class PermissionsGrid : Control, INamingContainer
    {
        #region Enums

        protected const string PermissionTypeGrant = "True";
        protected const string PermissionTypeDeny = "False";
        protected const string PermissionTypeNull = "Null";

        #endregion
        
        #region Private Members

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
        #endregion

        #region Protected Members
        protected DataGrid rolePermissionsGrid;
        protected DataGrid userPermissionsGrid;
        #endregion

        #region Constructor
        public PermissionsGrid()
        {
            dtUserPermissions = new DataTable();
            dtRolePermissions = new DataTable();
        }

        #endregion

        #region private Properties
        private int unAuthUsersRoleId = Int32.Parse(Globals.glbRoleUnauthUser);
        private int UnAuthUsersRoleId
        {
            get { return unAuthUsersRoleId; }
        }
        
        private int allUsersRoleId = Int32.Parse(Globals.glbRoleAllUsers);
        private int AllUsersRoleId
        {
            get
            {
                return allUsersRoleId;                
            }            
        }
        #endregion

        #region Protected Properties

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

        #endregion

        #region Public Properties

        /// <summary>
        /// Registers the scripts neccesary to make the tri-state controls work inside a RadAjaxPanel
        /// </summary>
        /// <remarks>
        /// No need to call this unless using the PermissionGrid inside an ajax control that omits scripts on postback
        /// See DesktopModules/Admin/Tabs.ascx.cs for an example of usage
        /// </remarks>
        public void RegisterScriptsForAjaxPanel()
        {
            PermissionTriState.RegisterScripts(Page, this);
        }

        #region "DataGrid Properties"

        public TableItemStyle AlternatingItemStyle
        {
            get
            {
                return rolePermissionsGrid.AlternatingItemStyle;
            }
        }

        public bool AutoGenerateColumns
        {
            get
            {
                return rolePermissionsGrid.AutoGenerateColumns;
            }
            set
            {
                rolePermissionsGrid.AutoGenerateColumns = value;
                userPermissionsGrid.AutoGenerateColumns = value;
            }
        }

        public int CellSpacing
        {
            get
            {
                return rolePermissionsGrid.CellSpacing;
            }
            set
            {
                rolePermissionsGrid.CellSpacing = value;
                userPermissionsGrid.CellSpacing = value;
            }
        }

        public DataGridColumnCollection Columns
        {
            get
            {
                return rolePermissionsGrid.Columns;
            }
        }

        public TableItemStyle FooterStyle
        {
            get
            {
                return rolePermissionsGrid.FooterStyle;
            }
        }

        public GridLines GridLines
        {
            get
            {
                return rolePermissionsGrid.GridLines;
            }
            set
            {
                rolePermissionsGrid.GridLines = value;
                userPermissionsGrid.GridLines = value;
            }
        }

        public TableItemStyle HeaderStyle
        {
            get
            {
                return rolePermissionsGrid.HeaderStyle;
            }
        }

        public TableItemStyle ItemStyle
        {
            get
            {
                return rolePermissionsGrid.ItemStyle;
            }
        }

        public DataGridItemCollection Items
        {
            get
            {
                return rolePermissionsGrid.Items;
            }
        }

        public TableItemStyle SelectedItemStyle
        {
            get
            {
                return rolePermissionsGrid.SelectedItemStyle;
            }
        }

        #endregion

        /// <summary>
        /// Gets the Id of the Administrator Role
        /// </summary>
        public int AdministratorRoleId
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().AdministratorRoleId;
            }
        }

        /// <summary>
        /// Gets the Id of the Registered Users Role
        /// </summary>
        public int RegisteredUsersRoleId
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().RegisteredRoleId;
            }
        }

        /// <summary>
        /// Gets and Sets whether a Dynamic Column has been added
        /// </summary>
        public bool DynamicColumnAdded
        {
            get
            {
                return ViewState["ColumnAdded"] != null;
            }
            set
            {
                ViewState["ColumnAdded"] = value;
            }
        }

        /// <summary>
        /// Gets the underlying Permissions Data Table
        /// </summary>
        public DataTable dtRolePermissions { get; private set; }

        /// <summary>
        /// Gets the underlying Permissions Data Table
        /// </summary>
        public DataTable dtUserPermissions { get; private set; }

        /// <summary>
        /// Gets the Id of the Portal
        /// </summary>
        public int PortalId
        {
            get
            {
                //Obtain PortalSettings from Current Context
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                int portalID;
                if (Globals.IsHostTab(portalSettings.ActiveTab.TabID)) //if we are in host filemanager then we need to pass a null portal id
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

        /// <summary>
        /// Gets and Sets the collection of Roles to display
        /// </summary>
        public ArrayList Roles { get; set; }

        /// <summary>
        /// Gets and Sets the ResourceFile to localize permissions
        /// </summary>
        public string ResourceFile { get; set; }

        #endregion

        #region Abstract Members

        /// <summary>
        /// Generate the Data Grid
        /// </summary>
        public abstract void GenerateDataGrid();

        #endregion

        #region Private Methods
        
        private void BindData()
        {
            EnsureChildControls();
            BindRolesGrid();
            BindUsersGrid();
        }

        private void BindRolesGrid()
        {
            dtRolePermissions.Columns.Clear();
            dtRolePermissions.Rows.Clear();

            //Add Roles Column
            dtRolePermissions.Columns.Add(new DataColumn("RoleId"));

            //Add Roles Column
            dtRolePermissions.Columns.Add(new DataColumn("RoleName"));

            for (int i = 0; i <= _permissions.Count - 1; i++)
            {
                var permissionInfo = (PermissionInfo)_permissions[i];

                //Add Enabled Column
                dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName + "_Enabled"));

                //Add Permission Column
                dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName));
            }
            GetRoles();

            UpdateRolePermissions();
            for (int i = 0; i <= Roles.Count - 1; i++)
            {
                var role = (RoleInfo)Roles[i];
                var row = dtRolePermissions.NewRow();
                row["RoleId"] = role.RoleID;
                row["RoleName"] = Localization.LocalizeRole(role.RoleName);
                int j;
                for (j = 0; j <= _permissions.Count - 1; j++)
                {
                    PermissionInfo objPerm;
                    objPerm = (PermissionInfo)_permissions[j];
                    row[objPerm.PermissionName + "_Enabled"] = GetEnabled(objPerm, role, j + 1);
                    if (SupportsDenyPermissions(objPerm))
                    {
                        row[objPerm.PermissionName] = GetPermission(objPerm, role, j + 1, PermissionTypeNull);
                    }
                    else
                    {
                        if (GetPermission(objPerm, role, j + 1))
                        {
                            row[objPerm.PermissionName] = PermissionTypeGrant;
                        }
                        else
                        {
                            row[objPerm.PermissionName] = PermissionTypeNull;
                        }
                    }
                }
                dtRolePermissions.Rows.Add(row);
            }
            rolePermissionsGrid.DataSource = dtRolePermissions;
            rolePermissionsGrid.DataBind();
        }

        private void BindUsersGrid()
        {
            dtUserPermissions.Columns.Clear();
            dtUserPermissions.Rows.Clear();

            //Add Roles Column
            var col = new DataColumn("UserId");
            dtUserPermissions.Columns.Add(col);

            //Add Roles Column
            col = new DataColumn("DisplayName");
            dtUserPermissions.Columns.Add(col);
            int i;
            for (i = 0; i <= _permissions.Count - 1; i++)
            {
                PermissionInfo objPerm;
                objPerm = (PermissionInfo)_permissions[i];

                //Add Enabled Column
                col = new DataColumn(objPerm.PermissionName + "_Enabled");
                dtUserPermissions.Columns.Add(col);

                //Add Permission Column
                col = new DataColumn(objPerm.PermissionName);
                dtUserPermissions.Columns.Add(col);
            }
            if (userPermissionsGrid != null)
            {
                _users = GetUsers();

                if (_users.Count != 0)
                {
                    userPermissionsGrid.Visible = true;
                    UpdateUserPermissions();
                    DataRow row;
                    for (i = 0; i <= _users.Count - 1; i++)
                    {
                        var user = (UserInfo)_users[i];
                        row = dtUserPermissions.NewRow();
                        row["UserId"] = user.UserID;
                        row["DisplayName"] = user.DisplayName;
                        int j;
                        for (j = 0; j <= _permissions.Count - 1; j++)
                        {
                            PermissionInfo objPerm;
                            objPerm = (PermissionInfo)_permissions[j];
                            row[objPerm.PermissionName + "_Enabled"] = GetEnabled(objPerm, user, j + 1);
                            if (SupportsDenyPermissions(objPerm))
                            {
                                row[objPerm.PermissionName] = GetPermission(objPerm, user, j + 1, PermissionTypeNull);
                            }
                            else
                            {
                                if (GetPermission(objPerm, user, j + 1))
                                {
                                    row[objPerm.PermissionName] = PermissionTypeGrant;
                                }
                                else
                                {
                                    row[objPerm.PermissionName] = PermissionTypeNull;
                                }
                            }
                        }
                        dtUserPermissions.Rows.Add(row);
                    }                    
                    userPermissionsGrid.DataSource = dtUserPermissions;
                    userPermissionsGrid.DataBind();
                }
                else
                {
                    dtUserPermissions.Rows.Clear();
                    userPermissionsGrid.DataSource = dtUserPermissions;
                    userPermissionsGrid.DataBind();
                    userPermissionsGrid.Visible = false;                    
                }
            }
        }

        private void EnsureRole(RoleInfo role)
        {
            if (Roles.Cast<RoleInfo>().All(r => r.RoleID != role.RoleID))
            {
                Roles.Add(role);
            }
        }

        private void GetRoles()
        {
            var checkedRoles = GetCheckedRoles();
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            Roles = new ArrayList(RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved && checkedRoles.Contains(r.RoleID)).ToArray());
        
            if (checkedRoles.Contains(UnAuthUsersRoleId))
            {
                Roles.Add(new RoleInfo { RoleID = UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });                    
            }            
            if (checkedRoles.Contains(AllUsersRoleId))
            {
                Roles.Add(new RoleInfo { RoleID = AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName=Globals.glbRoleAllUsersName });                    
            }

            //Administrators Role always has implicit permissions, then it should be always in
            EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId));
            
            //Show also default roles
            EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.RegisteredRoleId));
            EnsureRole(new RoleInfo { RoleID = AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });
            
            Roles.Reverse();
            Roles.Sort(new RoleComparer());
        }

        private IEnumerable<int> GetCheckedRoles()
        {
            if (PermissionsList == null)
            {
                return new List<int>();
            }
            return PermissionsList.Select(r => r.RoleID).Distinct();
        }

        private void SetUpGrid(DataGrid grid, string nameColumnDataField, string idColumnDataField, string permissionHeaderText)
        {
            grid.Columns.Clear();
            var nameColumn = new BoundColumn
                                {
                                    HeaderText = permissionHeaderText,
                                    DataField = nameColumnDataField
                                };            
            nameColumn.ItemStyle.CssClass = "permissionHeader";
            nameColumn.HeaderStyle.CssClass = "permissionHeader";
            grid.Columns.Add(nameColumn);

            var idColumn = new BoundColumn
                                {
                                    HeaderText = "",
                                    DataField = idColumnDataField,
                                    Visible = false
                                };
            grid.Columns.Add(idColumn);

            foreach (PermissionInfo permission in _permissions)
            {
                var templateCol = new TemplateColumn();
                var columnTemplate = new PermissionTriStateTemplate(permission)
                                                {
                                                    IsFullControl = IsFullControl(permission),
                                                    IsView = IsViewPermisison(permission),
                                                    SupportDenyMode = SupportsDenyPermissions(permission)
                                                };
                templateCol.ItemTemplate = columnTemplate;

                var locName = (permission.ModuleDefID <= 0) ? Localization.GetString(permission.PermissionName + ".Permission", PermissionProvider.Instance().LocalResourceFile) //system permission
                                                            : (!String.IsNullOrEmpty(ResourceFile) ? Localization.GetString(permission.PermissionName + ".Permission", ResourceFile) //custom permission
                                                                                                    : "");
                templateCol.HeaderText = !String.IsNullOrEmpty(locName) ? locName : permission.PermissionName;                
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
                HeaderText = Localization.GetString("PermissionActionsHeader.Text", PermissionProvider.Instance().LocalResourceFile)
            };
            grid.Columns.Add(actionsColumn);
            grid.ItemCommand += grid_ItemCommand;
        }

        void grid_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            var entityID = int.Parse(e.CommandArgument.ToString());
            var command = GetGridCommand(e.CommandName);
            var entityType = GetCommandType(e.CommandName);
            switch (command)
            {
                case "DELETE":
                    if (entityType == "ROLE")
                    {
                        DeleteRolePermissions(entityID);
                    }
                    else if (entityType == "USER")
                    {
                        DeleteUserPermissions(entityID);
                    }
                    BindData();
                    break;
            }
        }

        private void DeleteRolePermissions(int entityID)
        {
            //PermissionsList.RemoveAll(p => p.RoleID == entityID);
            var permissionToDelete = PermissionsList.Where(p => p.RoleID == entityID);
            foreach (PermissionInfoBase permission in permissionToDelete)
            {
                RemovePermission(permission.PermissionID, entityID, permission.UserID);
            }
        }
        private void DeleteUserPermissions(int entityID)
        {
            var permissionToDelete = PermissionsList.Where(p => p.UserID == entityID);
            foreach (PermissionInfoBase permission in permissionToDelete)
            {
                RemovePermission(permission.PermissionID, permission.RoleID, entityID);
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
            SetUpGrid(rolePermissionsGrid, "RoleName", "roleid", Localization.GetString("PermissionRoleHeader.Text", PermissionProvider.Instance().LocalResourceFile));
        }

        private void SetUpUsersGrid()
        {
            if (userPermissionsGrid != null)
            {
                SetUpGrid(userPermissionsGrid, "DisplayName", "userid", Localization.GetString("PermissionUserHeader.Text", PermissionProvider.Instance().LocalResourceFile));
            }
        }
        private void FillSelectRoleComboBox(int selectedRoleGroupId)
        {
            cboSelectRole.Items.Clear();
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();            
            var groupRoles = (selectedRoleGroupId > -2) ? RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == selectedRoleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);

            if (selectedRoleGroupId < 0)
            {                
                groupRoles.Add(new RoleInfo { RoleID = UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });                                
                groupRoles.Add(new RoleInfo { RoleID = AllUsersRoleId, RoleName = Globals.glbRoleAllUsersName });            
            }
            
            foreach (var role in groupRoles.OrderBy( r => r.RoleName))
            {
                cboSelectRole.Items.Add(new ListItem(role.RoleName, role.RoleID.ToString(CultureInfo.InvariantCulture)));
            }            
            int[] defaultRoleIds = {AllUsersRoleId, portalSettings.RegisteredRoleId, portalSettings.AdministratorRoleId};            
            var itemToSelect = cboSelectRole.Items.Cast<ListItem>().FirstOrDefault(i => !defaultRoleIds.Contains(int.Parse(i.Value)));
            if (itemToSelect != null)
            {
                cboSelectRole.SelectedValue = itemToSelect.Value;
            }
        }

        private void SetErrorMessage(string errorKey)
        {
            lblErrorMessage = new Label
            {
                //TODO Remove DEBUG test
                Text = "<br />" + (errorKey.StartsWith("DEBUG")? errorKey: Localization.GetString(errorKey)),
                CssClass = "NormalRed"
            };
            pnlPermissions.Controls.Add(lblErrorMessage);
        }

        #endregion

        #region Protected Methods

        protected virtual void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permissions">The permissions collection</param>
        /// <param name="user">The user to add</param>
        protected virtual void AddPermission(ArrayList permissions, UserInfo user)
        {
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permissions">The permissions collection</param>
        /// <param name="role">The role to add</param>
        protected virtual void AddPermission(ArrayList permissions, RoleInfo role)
        {
        }

        /// <summary>
        /// Builds the key used to store the "permission" information in the ViewState
        /// </summary>
        /// <param name="allowAccess">The type of permission ( grant / deny )</param>
        /// <param name="permissionId">The Id of the permission</param>
        /// <param name="objectPermissionId">The Id of the object permission</param>
        /// <param name="roleId">The role id</param>
        /// <param name="roleName">The role name</param>
        protected string BuildKey(bool allowAccess, int permissionId, int objectPermissionId, int roleId, string roleName)
        {
            return BuildKey(allowAccess, permissionId, objectPermissionId, roleId, roleName, Null.NullInteger, Null.NullString);
        }

        /// <summary>
        /// Builds the key used to store the "permission" information in the ViewState
        /// </summary>
        /// <param name="allowAccess">The type of permission ( grant / deny )</param>
        /// <param name="permissionId">The Id of the permission</param>
        /// <param name="objectPermissionId">The Id of the object permission</param>
        /// <param name="roleId">The role id</param>
        /// <param name="roleName">The role name</param>
        /// <param name="userID">The user id</param>
        /// <param name="displayName">The user display name</param>
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
        /// Creates the Child Controls
        /// </summary>
        protected override void CreateChildControls()
        {
            _permissions = GetPermissions();

            pnlPermissions = new Panel { CssClass = "dnnGrid dnnPermissionsGrid" };

            //Optionally Add Role Group Filter
            CreateAddRoleControls();

            rolePermissionsGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CellSpacing = 0,
                CellPadding = 2,
                GridLines = GridLines.None,
                CssClass = "dnnPermissionsGrid"
            };
            rolePermissionsGrid.FooterStyle.CssClass = "dnnGridFooter";
            rolePermissionsGrid.HeaderStyle.CssClass = "dnnGridHeader";
            rolePermissionsGrid.ItemStyle.CssClass = "dnnGridItem";
            rolePermissionsGrid.AlternatingItemStyle.CssClass = "dnnGridAltItem";
            rolePermissionsGrid.ItemDataBound += rolePermissionsGrid_ItemDataBound;
            SetUpRolesGrid();
            pnlPermissions.Controls.Add(rolePermissionsGrid);

            _users = GetUsers();
            if (_users != null)
            {
                userPermissionsGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    CellSpacing = 0,
                    GridLines = GridLines.None,
                    CssClass = "dnnPermissionsGrid"
                };
                userPermissionsGrid.FooterStyle.CssClass = "dnnGridFooter";
                userPermissionsGrid.HeaderStyle.CssClass = "dnnGridHeader";
                userPermissionsGrid.ItemStyle.CssClass = "dnnGridItem";
                userPermissionsGrid.AlternatingItemStyle.CssClass = "dnnGridAltItem";

                SetUpUsersGrid();
                pnlPermissions.Controls.Add(userPermissionsGrid);

                var divAddUser = new Panel { CssClass = "dnnFormItem" };

                lblErrorMessage = new Label { Text = Localization.GetString("DisplayName") };
                txtUser = new TextBox { ID = "txtUser" };
                lblErrorMessage.AssociatedControlID = txtUser.ID;

                hiddenUserIds = new HiddenField { ID = "hiddenUserIds" };

                divAddUser.Controls.Add(lblErrorMessage);
                divAddUser.Controls.Add(txtUser);
                divAddUser.Controls.Add(hiddenUserIds);

                cmdUser = new LinkButton { Text = Localization.GetString("Add"), CssClass = "dnnSecondaryAction" };
                divAddUser.Controls.Add(cmdUser);
                cmdUser.Click += AddUser;

                pnlPermissions.Controls.Add(divAddUser);
            }
            Controls.Add(pnlPermissions);
        }

        void rolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {                
                var roleID = Int32.Parse(((DataRowView)item.DataItem)[0].ToString());
                if (roleID == PortalSettings.Current.AdministratorRoleId || roleID == AllUsersRoleId || roleID == PortalSettings.Current.RegisteredRoleId)
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

            var addRoleControls = new Panel {CssClass = "dnnFormItem"};
            var divRoleGroups = new Panel {CssClass = "leftGroup"};
            var divSelectRole = new Panel { CssClass = "rightGroup" };

            lblGroups = new Label {Text = Localization.GetString("RoleGroupFilter")};
            cboRoleGroups = new DropDownList { AutoPostBack = true, ID = "cboRoleGroups", ViewStateMode = ViewStateMode.Disabled };
            lblGroups.AssociatedControlID = cboRoleGroups.ID;
            divRoleGroups.Controls.Add(lblGroups);

            cboRoleGroups.SelectedIndexChanged += RoleGroupsSelectedIndexChanged;
            cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
            var liItem = new ListItem(Localization.GetString("GlobalRoles"), "-1") {Selected = true};
            cboRoleGroups.Items.Add(liItem);
            foreach (RoleGroupInfo roleGroup in arrGroups)
            {
                cboRoleGroups.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString(CultureInfo.InvariantCulture)));
            }
            divRoleGroups.Controls.Add(cboRoleGroups);
            addRoleControls.Controls.Add(divRoleGroups);

            lblSelectRole = new Label { Text = Localization.GetString("RoleSelect") };
            cboSelectRole = new DropDownList { ID = "cboSelectRole", ViewStateMode = ViewStateMode.Disabled };
            lblSelectRole.AssociatedControlID = cboSelectRole.ID;            
            divSelectRole.Controls.Add(lblSelectRole);

            FillSelectRoleComboBox(-1); //Default Role Group is Global Roles
            divSelectRole.Controls.Add(cboSelectRole);

            cmdRole = new LinkButton { Text = Localization.GetString("Add"), CssClass = "dnnSecondaryAction" };
            cmdRole.Click += AddRole;
            divSelectRole.Controls.Add(cmdRole);
            roleField = new HiddenField() {ID = "roleField"};
            addRoleControls.Controls.Add(roleField);

            addRoleControls.Controls.Add(divSelectRole);

            pnlPermissions.Controls.Add(addRoleControls);
        }

        /// <summary>
        /// Gets the Enabled status of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="role">The role</param>
        /// <param name="column">The column of the Grid</param>
        protected virtual bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return true;
        }

        /// <summary>
        /// Gets the Enabled status of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="user">The user</param>
        /// <param name="column">The column of the Grid</param>
        protected virtual bool GetEnabled(PermissionInfo objPerm, UserInfo user, int column)
        {
            return true;
        }

        /// <summary>
        /// Gets the Value of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="role">The role</param>
        /// <param name="column">The column of the Grid</param>
        protected virtual bool GetPermission(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return Convert.ToBoolean(GetPermission(objPerm, role, column, PermissionTypeDeny));
        }

        /// <summary>
        /// Gets the Value of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="role">The role</param>
        /// <param name="column">The column of the Grid</param>
        /// <param name="defaultState">Default State.</param>
        protected virtual string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string stateKey = defaultState;
            if (PermissionsList != null)
            {
                foreach (PermissionInfoBase permission in PermissionsList)
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
        /// Gets the Value of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="user">The user</param>
        /// <param name="column">The column of the Grid</param>
        protected virtual bool GetPermission(PermissionInfo objPerm, UserInfo user, int column)
        {
            return Convert.ToBoolean(GetPermission(objPerm, user, column, PermissionTypeDeny));
        }

        /// <summary>
        /// Gets the Value of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="user">The user</param>
        /// <param name="column">The column of the Grid</param>
        /// <param name="defaultState">Default State.</param>
        protected virtual string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
        {
            var stateKey = defaultState;
            if (PermissionsList != null)
            {
                foreach (var permission in PermissionsList)
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
        /// Gets the permissions from the Database
        /// </summary>
        protected virtual ArrayList GetPermissions()
        {
            return null;
        }

        /// <summary>
        /// Gets the users from the Database
        /// </summary>
        protected virtual ArrayList GetUsers()
        {
            var arrUsers = new ArrayList();
            UserInfo objUser;
            if (PermissionsList != null)
            {
                foreach (var permission in PermissionsList)
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

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterScript(Page, "~/js/dnn.permissiongrid.js");

            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css", FileOrder.Css.ResourceCss);
        }

        /// <summary>
        /// Overrides the base OnPreRender method to Bind the Grid to the Permissions
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            BindData();

            var script = "var pgm = new dnn.permissionGridManager('" + ClientID + "');";
            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), ClientID + "PermissionGridManager", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "PermissionGridManager", script, true);
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
            //to maintain backward compatibility the base implementation must always call the simple parameterless version of this method
#pragma warning disable 612,618
            return SupportsDenyPermissions();
#pragma warning restore 612,618
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permission">The permission being updated</param>
        /// <param name="roleId">Rold Id.</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="allowAccess">The value of the permission</param>
        protected virtual void UpdatePermission(PermissionInfo permission, int roleId, string roleName, bool allowAccess)
        {
            UpdatePermission(permission, roleId, roleName, allowAccess ? PermissionTypeGrant : PermissionTypeNull);
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permission">The permission being updated</param>
        /// <param name="roleId">Role Id.</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="stateKey">The permission state</param>
        protected virtual void UpdatePermission(PermissionInfo permission, int roleId, string roleName, string stateKey)
        {
            RemovePermission(permission.PermissionID, roleId, Null.NullInteger);
            switch (stateKey)
            {
                case PermissionTypeGrant:
                    AddPermission(permission, roleId, roleName, Null.NullInteger, Null.NullString, true);
                    break;
                case PermissionTypeDeny:
                    AddPermission(permission, roleId, roleName, Null.NullInteger, Null.NullString, false);
                    break;
            }
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permission">The permission being updated</param>
        /// <param name="displayName">The user's displayname</param>
        /// <param name="userId">The user's id</param>
        /// <param name="allowAccess">The value of the permission</param>
        protected virtual void UpdatePermission(PermissionInfo permission, string displayName, int userId, bool allowAccess)
        {
            UpdatePermission(permission, displayName, userId, allowAccess ? PermissionTypeGrant : PermissionTypeNull);
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permission">The permission being updated</param>
        /// <param name="displayName">The user's displayname</param>
        /// <param name="userId">The user's id</param>
        /// <param name="stateKey">The permission state</param>
        protected virtual void UpdatePermission(PermissionInfo permission, string displayName, int userId, string stateKey)
        {
            RemovePermission(permission.PermissionID, int.Parse(Globals.glbRoleNothing), userId);
            switch (stateKey)
            {
                case PermissionTypeGrant:
                    AddPermission(permission, int.Parse(Globals.glbRoleNothing), Null.NullString, userId, displayName, true);
                    break;
                case PermissionTypeDeny:
                    AddPermission(permission, int.Parse(Globals.glbRoleNothing), Null.NullString, userId, displayName, false);
                    break;
            }
        }
        /// <summary>
        /// Updates the permissions
        /// </summary>
        protected void UpdatePermissions()
        {
            EnsureChildControls();
            GetRoles();
            UpdateRolePermissions();
            _users = GetUsers();
            UpdateUserPermissions();
        }

        /// <summary>
        /// Updates the permissions
        /// </summary>
        protected void UpdateRolePermissions()
        {
            if (rolePermissionsGrid != null && !RefreshGrid)
            {
                var rolesList = Roles.Cast<RoleInfo>().ToList();
                foreach (DataGridItem dgi in rolePermissionsGrid.Items)
                {
                    var roleId = int.Parse(dgi.Cells[1].Text);
                    if (rolesList.All(r => r.RoleID != roleId))
                    {
                        continue;
                    }
                    for (int i = 2; i <= dgi.Cells.Count - 2; i++) 
                    {
                        //all except first two cells which is role names and role ids and last column is Actions
                        if (dgi.Cells[i].Controls.Count > 0)
                        {
                            var permissionInfo = (PermissionInfo)_permissions[i - 2];
                            var triState = (PermissionTriState)dgi.Cells[i].Controls[0];
                            if (SupportsDenyPermissions(permissionInfo))
                            {
                                UpdatePermission(permissionInfo, roleId, dgi.Cells[0].Text, triState.Value);
                            }
                            else
                            {
                                UpdatePermission(permissionInfo, roleId, dgi.Cells[0].Text, triState.Value == PermissionTypeGrant);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the permissions
        /// </summary>
        protected void UpdateUserPermissions()
        {
            if (userPermissionsGrid != null && !RefreshGrid)
            {
                var usersList = _users.Cast<UserInfo>().ToList();
                foreach (DataGridItem dgi in userPermissionsGrid.Items)
                {                    
                    var userId = int.Parse(dgi.Cells[1].Text);
                    if (usersList.All(u => u.UserID != userId))
                    {
                        continue;
                    }
                    for (int i = 2; i <= dgi.Cells.Count - 2; i++)
                    {
                        //all except first two cells which is displayname and userid and Last column is Actions
                        if (dgi.Cells[i].Controls.Count > 0)
                        {
                            var permissionInfo = (PermissionInfo)_permissions[i - 2];
                            var triState = (PermissionTriState)dgi.Cells[i].Controls[0];
                            if (SupportsDenyPermissions(permissionInfo))
                            {
                                UpdatePermission(permissionInfo, dgi.Cells[0].Text, userId, triState.Value);
                            }
                            else
                            {
                                UpdatePermission(permissionInfo, dgi.Cells[0].Text, userId, triState.Value == PermissionTypeGrant);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// RoleGroupsSelectedIndexChanged runs when the Role Group is changed
        /// </summary>
        protected virtual void RoleGroupsSelectedIndexChanged(object sender, EventArgs e)
        {
            FillSelectRoleComboBox(Int32.Parse(cboRoleGroups.SelectedValue));
        }

        /// <summary>
        /// AddUser runs when the Add user linkbutton is clicked
        /// </summary>
        protected virtual void AddUser(object sender, EventArgs e)
        {
            UpdatePermissions();
            if (!string.IsNullOrEmpty(hiddenUserIds.Value))
            {
                foreach (var id in hiddenUserIds.Value.Split(','))
                {
                    var userId = Convert.ToInt32(id);
                    var user = UserController.GetUserById(PortalId, userId);
                    if (user != null)
                    {
                        AddPermission(_permissions, user);
                        BindData();
                    }
                }

                txtUser.Text = hiddenUserIds.Value = string.Empty;
            }
        }

        /// <summary>
        /// AddRole runs when the Add Role linkbutton is clicked
        /// </summary>
        void AddRole(object sender, EventArgs e)
        {
            UpdatePermissions();
            int selectedRoleId;
            if (!Int32.TryParse(roleField.Value, out selectedRoleId))
            {
                //Role not selected
                SetErrorMessage("InvalidRoleId");
                return;
            }

            //verify role         
            var role = GetSelectedRole(selectedRoleId);
            if (role != null)
            {
                AddPermission(_permissions, role);
                BindData();
            }
            else
            {
                //role does not exist
                SetErrorMessage("RoleNotFound");
            }            
        }

        private RoleInfo GetSelectedRole(int selectedRoleId)
        {
            RoleInfo role = null;
            if (selectedRoleId == AllUsersRoleId)
            {
                role = new RoleInfo
                {
                    RoleID = AllUsersRoleId,
                    RoleName = Globals.glbRoleAllUsersName
                };
            }
            else if (selectedRoleId == UnAuthUsersRoleId)
            {
                role = new RoleInfo
                {
                    RoleID = UnAuthUsersRoleId,
                    RoleName = Globals.glbRoleUnauthUserName
                };
            }
            else
            {
                role = RoleController.Instance.GetRoleById(PortalId, selectedRoleId);
            }
            return role;
        }

        #endregion

        #region Obsolete Methods

        /// <summary>
        /// returns whether or not the derived grid supports Deny permissions
        /// </summary>
        [Obsolete("Deprecated in 6.2.0 use SupportsDenyPermissions(PermissionInfo) instead.")]
        protected virtual bool SupportsDenyPermissions()
        {
            return false; //to support Deny permissions a derived grid typically needs to implement the new GetPermission and UpdatePermission overload methods which support StateKey
        }

        #endregion
    }
}
