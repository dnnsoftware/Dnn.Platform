// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.WebControls

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Localization;

    public class RolesSelectionGrid : Control, INamingContainer
    {
        private readonly DataTable _dtRoleSelections = new DataTable();
        private IList<RoleInfo> _roles;
        private IList<string> _selectedRoles;
        private DropDownList cboRoleGroups;
        private DataGrid dgRoleSelection;
        private Label lblGroups;
        private Panel pnlRoleSlections;

        public TableItemStyle AlternatingItemStyle
        {
            get
            {
                return this.dgRoleSelection.AlternatingItemStyle;
            }
        }

        public DataGridColumnCollection Columns
        {
            get
            {
                return this.dgRoleSelection.Columns;
            }
        }

        public TableItemStyle FooterStyle
        {
            get
            {
                return this.dgRoleSelection.FooterStyle;
            }
        }

        public TableItemStyle HeaderStyle
        {
            get
            {
                return this.dgRoleSelection.HeaderStyle;
            }
        }

        public TableItemStyle ItemStyle
        {
            get
            {
                return this.dgRoleSelection.ItemStyle;
            }
        }

        public DataGridItemCollection Items
        {
            get
            {
                return this.dgRoleSelection.Items;
            }
        }

        public TableItemStyle SelectedItemStyle
        {
            get
            {
                return this.dgRoleSelection.SelectedItemStyle;
            }
        }

        public bool AutoGenerateColumns
        {
            get
            {
                return this.dgRoleSelection.AutoGenerateColumns;
            }

            set
            {
                this.dgRoleSelection.AutoGenerateColumns = value;
            }
        }

        public int CellSpacing
        {
            get
            {
                return this.dgRoleSelection.CellSpacing;
            }

            set
            {
                this.dgRoleSelection.CellSpacing = value;
            }
        }

        public GridLines GridLines
        {
            get
            {
                return this.dgRoleSelection.GridLines;
            }

            set
            {
                this.dgRoleSelection.GridLines = value;
            }
        }

        /// <summary>
        /// Gets or sets list of the Names of the selected Roles.
        /// </summary>
        public ArrayList SelectedRoleNames
        {
            get
            {
                this.UpdateRoleSelections();
                return new ArrayList(this.CurrentRoleSelection.ToArray());
            }

            set
            {
                this.CurrentRoleSelection = value.Cast<string>().ToList();
            }
        }

        /// <summary>
        /// Gets or sets and Sets the ResourceFile to localize permissions.
        /// </summary>
        public string ResourceFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable ShowAllUsers to display the virtuell "Unauthenticated Users" role.
        /// </summary>
        public bool ShowUnauthenticatedUsers
        {
            get
            {
                if (this.ViewState["ShowUnauthenticatedUsers"] == null)
                {
                    return false;
                }

                return Convert.ToBoolean(this.ViewState["ShowUnauthenticatedUsers"]);
            }

            set
            {
                this.ViewState["ShowUnauthenticatedUsers"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether enable ShowAllUsers to display the virtuell "All Users" role.
        /// </summary>
        public bool ShowAllUsers
        {
            get
            {
                if (this.ViewState["ShowAllUsers"] == null)
                {
                    return false;
                }

                return Convert.ToBoolean(this.ViewState["ShowAllUsers"]);
            }

            set
            {
                this.ViewState["ShowAllUsers"] = value;
            }
        }

        private DataTable dtRolesSelection
        {
            get
            {
                return this._dtRoleSelections;
            }
        }

        private IList<string> CurrentRoleSelection
        {
            get
            {
                return this._selectedRoles ?? (this._selectedRoles = new List<string>());
            }

            set
            {
                this._selectedRoles = value;
            }
        }

        /// <summary>
        /// Load the ViewState.
        /// </summary>
        /// <param name="savedState">The saved state.</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load TabPermissions
                if (myState[1] != null)
                {
                    string state = Convert.ToString(myState[1]);
                    this.CurrentRoleSelection = state != string.Empty
                                        ? new List<string>(state.Split(new[] { "##" }, StringSplitOptions.None))
                                        : new List<string>();
                }
            }
        }

        /// <summary>
        /// Saves the ViewState.
        /// </summary>
        /// <returns></returns>
        protected override object SaveViewState()
        {
            var allStates = new object[2];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Persist the TabPermisisons
            var sb = new StringBuilder();
            bool addDelimiter = false;
            foreach (string role in this.CurrentRoleSelection)
            {
                if (addDelimiter)
                {
                    sb.Append("##");
                }
                else
                {
                    addDelimiter = true;
                }

                sb.Append(role);
            }

            allStates[1] = sb.ToString();
            return allStates;
        }

        /// <summary>
        /// Creates the Child Controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.pnlRoleSlections = new Panel { CssClass = "dnnRolesGrid" };

            // Optionally Add Role Group Filter
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            ArrayList arrGroups = RoleController.GetRoleGroups(_portalSettings.PortalId);
            if (arrGroups.Count > 0)
            {
                this.lblGroups = new Label { Text = Localization.GetString("RoleGroupFilter") };
                this.pnlRoleSlections.Controls.Add(this.lblGroups);

                this.pnlRoleSlections.Controls.Add(new LiteralControl("&nbsp;&nbsp;"));

                this.cboRoleGroups = new DropDownList { AutoPostBack = true };

                this.cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
                var liItem = new ListItem(Localization.GetString("GlobalRoles"), "-1") { Selected = true };
                this.cboRoleGroups.Items.Add(liItem);
                foreach (RoleGroupInfo roleGroup in arrGroups)
                {
                    this.cboRoleGroups.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString(CultureInfo.InvariantCulture)));
                }

                this.pnlRoleSlections.Controls.Add(this.cboRoleGroups);

                this.pnlRoleSlections.Controls.Add(new LiteralControl("<br/><br/>"));
            }

            this.dgRoleSelection = new DataGrid { AutoGenerateColumns = false, CellSpacing = 0, GridLines = GridLines.None };
            this.dgRoleSelection.FooterStyle.CssClass = "dnnGridFooter";
            this.dgRoleSelection.HeaderStyle.CssClass = "dnnGridHeader";
            this.dgRoleSelection.ItemStyle.CssClass = "dnnGridItem";
            this.dgRoleSelection.AlternatingItemStyle.CssClass = "dnnGridAltItem";
            this.SetUpRolesGrid();
            this.pnlRoleSlections.Controls.Add(this.dgRoleSelection);

            this.Controls.Add(this.pnlRoleSlections);
        }

        /// <summary>
        /// Overrides the base OnPreRender method to Bind the Grid to the Permissions.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            this.BindData();
        }

        /// <summary>
        /// Updates a Selection.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="Selected"></param>
        protected virtual void UpdateSelection(string roleName, bool Selected)
        {
            var isMatch = false;
            foreach (string currentRoleName in this.CurrentRoleSelection)
            {
                if (currentRoleName == roleName)
                {
                    // role is in collection
                    if (!Selected)
                    {
                        // Remove from collection as we only keep selected roles
                        this.CurrentRoleSelection.Remove(currentRoleName);
                    }

                    isMatch = true;
                    break;
                }
            }

            // Rolename not found so add new
            if (!isMatch && Selected)
            {
                this.CurrentRoleSelection.Add(roleName);
            }
        }

        /// <summary>
        /// Updates the Selections.
        /// </summary>
        protected void UpdateSelections()
        {
            this.EnsureChildControls();

            this.UpdateRoleSelections();
        }

        /// <summary>
        /// Updates the permissions.
        /// </summary>
        protected void UpdateRoleSelections()
        {
            if (this.dgRoleSelection != null)
            {
                foreach (DataGridItem dgi in this.dgRoleSelection.Items)
                {
                    const int i = 2;
                    if (dgi.Cells[i].Controls.Count > 0)
                    {
                        var cb = (CheckBox)dgi.Cells[i].Controls[0];
                        this.UpdateSelection(dgi.Cells[0].Text, cb.Checked);
                    }
                }
            }
        }

        /// <summary>
        /// Bind the data to the controls.
        /// </summary>
        private void BindData()
        {
            this.EnsureChildControls();

            this.BindRolesGrid();
        }

        /// <summary>
        /// Bind the Roles data to the Grid.
        /// </summary>
        private void BindRolesGrid()
        {
            this.dtRolesSelection.Columns.Clear();
            this.dtRolesSelection.Rows.Clear();

            // Add Roles Column
            var col = new DataColumn("RoleId", typeof(string));
            this.dtRolesSelection.Columns.Add(col);

            // Add Roles Column
            col = new DataColumn("RoleName", typeof(string));
            this.dtRolesSelection.Columns.Add(col);

            // Add Selected Column
            col = new DataColumn("Selected", typeof(bool));
            this.dtRolesSelection.Columns.Add(col);

            this.GetRoles();

            this.UpdateRoleSelections();
            for (int i = 0; i <= this._roles.Count - 1; i++)
            {
                var role = this._roles[i];
                DataRow row = this.dtRolesSelection.NewRow();
                row["RoleId"] = role.RoleID;
                row["RoleName"] = Localization.LocalizeRole(role.RoleName);
                row["Selected"] = this.GetSelection(role.RoleName);

                this.dtRolesSelection.Rows.Add(row);
            }

            this.dgRoleSelection.DataSource = this.dtRolesSelection;
            this.dgRoleSelection.DataBind();
        }

        private bool GetSelection(string roleName)
        {
            return this.CurrentRoleSelection.Any(r => r == roleName);
        }

        /// <summary>
        /// Gets the roles from the Database and loads them into the Roles property.
        /// </summary>
        private void GetRoles()
        {
            int roleGroupId = -2;
            if ((this.cboRoleGroups != null) && (this.cboRoleGroups.SelectedValue != null))
            {
                roleGroupId = int.Parse(this.cboRoleGroups.SelectedValue);
            }

            this._roles = roleGroupId > -2
                    ? RoleController.Instance.GetRoles(PortalController.Instance.GetCurrentPortalSettings().PortalId, r => r.RoleGroupID == roleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                    : RoleController.Instance.GetRoles(PortalController.Instance.GetCurrentPortalSettings().PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);

            if (roleGroupId < 0)
            {
                if (this.ShowUnauthenticatedUsers)
                {
                    this._roles.Add(new RoleInfo { RoleID = int.Parse(Globals.glbRoleUnauthUser), RoleName = Globals.glbRoleUnauthUserName });
                }

                if (this.ShowAllUsers)
                {
                    this._roles.Add(new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers), RoleName = Globals.glbRoleAllUsersName });
                }
            }

            this._roles = this._roles.OrderBy(r => r.RoleName).ToList();
        }

        /// <summary>
        /// Sets up the columns for the Grid.
        /// </summary>
        private void SetUpRolesGrid()
        {
            this.dgRoleSelection.Columns.Clear();
            var textCol = new BoundColumn { HeaderText = "&nbsp;", DataField = "RoleName" };
            textCol.ItemStyle.Width = Unit.Parse("150px");
            this.dgRoleSelection.Columns.Add(textCol);
            var idCol = new BoundColumn { HeaderText = string.Empty, DataField = "roleid", Visible = false };
            this.dgRoleSelection.Columns.Add(idCol);
            var checkCol = new TemplateColumn();
            var columnTemplate = new CheckBoxColumnTemplate { DataField = "Selected" };
            checkCol.ItemTemplate = columnTemplate;
            checkCol.HeaderText = Localization.GetString("SelectedRole");
            checkCol.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            checkCol.HeaderStyle.Wrap = true;
            this.dgRoleSelection.Columns.Add(checkCol);
        }
    }
}
