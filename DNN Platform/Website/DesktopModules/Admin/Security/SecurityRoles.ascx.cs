// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    using Calendar = DotNetNuke.Common.Utilities.Calendar;
    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SecurityRoles PortalModuleBase is used to manage the users and roles they
    /// have.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class SecurityRoles : PortalModuleBase, IActionable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityRoles));
        private readonly INavigationManager _navigationManager;
        private int RoleId = Null.NullInteger;
        private new int UserId = Null.NullInteger;
        private RoleInfo _Role;
        private int _SelectedUserID = Null.NullInteger;
        private UserInfo _User;

        private int _totalPages = 1;
        private int _totalRecords;

        public SecurityRoles()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ParentModule (if one exists).
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public PortalModuleBase ParentModule { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url for the page.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected string ReturnUrl
        {
            get
            {
                string _ReturnURL;
                var FilterParams = new string[string.IsNullOrEmpty(this.Request.QueryString["filterproperty"]) ? 2 : 3];

                if (string.IsNullOrEmpty(this.Request.QueryString["filterProperty"]))
                {
                    FilterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                    FilterParams.SetValue("currentpage=" + this.Request.QueryString["currentpage"], 1);
                }
                else
                {
                    FilterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                    FilterParams.SetValue("filterProperty=" + this.Request.QueryString["filterProperty"], 1);
                    FilterParams.SetValue("currentpage=" + this.Request.QueryString["currentpage"], 2);
                }

                if (string.IsNullOrEmpty(this.Request.QueryString["filter"]))
                {
                    _ReturnURL = this._navigationManager.NavigateURL(this.TabId);
                }
                else
                {
                    _ReturnURL = this._navigationManager.NavigateURL(this.TabId, string.Empty, FilterParams);
                }

                return _ReturnURL;
            }
        }

        protected RoleInfo Role
        {
            get
            {
                if (this._Role == null)
                {
                    if (this.RoleId != Null.NullInteger)
                    {
                        this._Role = RoleController.Instance.GetRole(this.PortalId, r => r.RoleID == this.RoleId);
                    }
                    else if (this.cboRoles.SelectedItem != null)
                    {
                        this._Role = RoleController.Instance.GetRole(this.PortalId, r => r.RoleID == Convert.ToInt32(this.cboRoles.SelectedItem.Value));
                    }
                }

                return this._Role;
            }
        }

        protected UserInfo User
        {
            get
            {
                if (this._User == null)
                {
                    if (this.UserId != Null.NullInteger)
                    {
                        this._User = UserController.GetUserById(this.PortalId, this.UserId);
                    }
                    else if (this.UsersControl == UsersControl.TextBox && !string.IsNullOrEmpty(this.txtUsers.Text))
                    {
                        this._User = UserController.GetUserByName(this.PortalId, this.txtUsers.Text);
                    }
                    else if (this.UsersControl == UsersControl.Combo && (this.cboUsers.SelectedItem != null))
                    {
                        this._User = UserController.GetUserById(this.PortalId, Convert.ToInt32(this.cboUsers.SelectedItem.Value));
                    }
                }

                return this._User;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the control should use a Combo Box or Text Box to display the users.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected UsersControl UsersControl
        {
            get
            {
                var setting = UserModuleBase.GetSetting(this.PortalId, "Security_UsersControl");
                return (UsersControl)setting;
            }
        }

        protected int PageSize
        {
            get
            {
                var setting = UserModuleBase.GetSetting(this.PortalId, "Records_PerPage");
                return Convert.ToInt32(setting);
            }
        }

        protected int SelectedUserID
        {
            get
            {
                return this._SelectedUserID;
            }

            set
            {
                this._SelectedUserID = value;
            }
        }

        protected int CurrentPage { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            if (!ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration))
            {
                this.Response.Redirect(this._navigationManager.NavigateURL("Access Denied"), true);
            }

            base.DataBind();

            // Localize Headers
            Localization.LocalizeDataGrid(ref this.grdUserRoles, this.LocalResourceFile);

            // Bind the role data to the datalist
            this.BindData();

            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteButonVisible returns a boolean indicating if the delete button for
        /// the specified UserID, RoleID pair should be shown.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="UserID">The ID of the user to check delete button visibility for.</param>
        /// <param name="RoleID">The ID of the role to check delete button visibility for.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool DeleteButtonVisible(int UserID, int RoleID)
        {
            // [DNN-4285] Check if the role can be removed (only handles case of Administrator and Administrator Role
            bool canDelete = RoleController.CanRemoveUserFromRole(this.PortalSettings, UserID, RoleID);
            if (RoleID == this.PortalSettings.AdministratorRoleId && canDelete)
            {
                // User can only delete if in Admin role
                canDelete = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
            }

            return canDelete;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatExpiryDate formats the expiry/effective date and filters out nulls.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="DateTime">The Date object to format.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public string FormatDate(DateTime DateTime)
        {
            if (!Null.IsNull(DateTime))
            {
                return DateTime.ToShortDateString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatExpiryDate formats the expiry/effective date and filters out nulls.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public string FormatUser(int UserID, string DisplayName)
        {
            return "<a href=\"" + Globals.LinkClick("userid=" + UserID, this.TabId, this.ModuleId) + "\" class=\"CommandButton\">" + DisplayName + "</a>";
        }

        public void cmdDeleteUserRole_click(object sender, ImageClickEventArgs e)
        {
            if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) == false)
            {
                return;
            }

            try
            {
                var cmdDeleteUserRole = (ImageButton)sender;
                int roleId = Convert.ToInt32(cmdDeleteUserRole.Attributes["roleId"]);
                int userId = Convert.ToInt32(cmdDeleteUserRole.Attributes["userId"]);

                RoleInfo role = RoleController.Instance.GetRole(this.PortalId, r => r.RoleID == roleId);
                if (!RoleController.DeleteUserRole(UserController.GetUserById(this.PortalId, userId), role, this.PortalSettings, this.chkNotify.Checked))
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("RoleRemoveError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }

                this.BindGrid();
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("RoleRemoveError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Request.QueryString["RoleId"] != null)
            {
                this.RoleId = int.Parse(this.Request.QueryString["RoleId"]);
            }

            if (this.Request.QueryString["UserId"] != null)
            {
                int userId;

                // Use Int32.MaxValue as invalid UserId
                this.UserId = int.TryParse(this.Request.QueryString["UserId"], out userId) ? userId : int.MaxValue;
            }

            this.CurrentPage = 1;
            if (this.Request.QueryString["CurrentPage"] != null)
            {
                var currentPage = 0;
                if (int.TryParse(this.Request.QueryString["CurrentPage"], out currentPage)
                    && currentPage > 0)
                {
                    this.CurrentPage = currentPage;
                }
                else
                {
                    this.CurrentPage = 1;
                }
            }

            this.cboRoles.SelectedIndexChanged += this.cboRoles_SelectedIndexChanged;
            this.cboUsers.SelectedIndexChanged += this.cboUsers_SelectedIndexChanged;
            this.cmdAdd.Click += this.cmdAdd_Click;
            this.cmdValidate.Click += this.cmdValidate_Click;
            this.grdUserRoles.ItemCreated += this.grdUserRoles_ItemCreated;
            this.grdUserRoles.ItemDataBound += this.grdUserRoles_ItemDataBound;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                this.cmdCancel.NavigateUrl = this.ReturnUrl;
                if (this.ParentModule == null)
                {
                    this.DataBind();
                }

                if (this.Role == null)
                {
                    return;
                }

                this.placeIsOwner.Visible = (this.Role.SecurityMode == SecurityMode.SocialGroup) || (this.Role.SecurityMode == SecurityMode.Both);
                this.placeIsOwnerHeader.Visible = (this.Role.SecurityMode == SecurityMode.SocialGroup) || (this.Role.SecurityMode == SecurityMode.Both);
            }
            catch (ThreadAbortException exc) // Do nothing if ThreadAbort as this is caused by a redirect
            {
                Logger.Debug(exc);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void grdUserRoles_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var userRole = (UserRoleInfo)item.DataItem;
                if (this.RoleId == Null.NullInteger)
                {
                    if (userRole.RoleID == Convert.ToInt32(this.cboRoles.SelectedValue))
                    {
                        this.cmdAdd.Text = Localization.GetString("UpdateRole.Text", this.LocalResourceFile);
                    }
                }

                if (this.UserId == Null.NullInteger)
                {
                    if (userRole.UserID == this.SelectedUserID)
                    {
                        this.cmdAdd.Text = Localization.GetString("UpdateRole.Text", this.LocalResourceFile);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// BindData loads the controls from the Database.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void BindData()
        {
            // bind all portal roles to dropdownlist
            if (this.RoleId == Null.NullInteger)
            {
                if (this.cboRoles.Items.Count == 0)
                {
                    var roles = RoleController.Instance.GetRoles(this.PortalId, x => x.Status == RoleStatus.Approved);

                    // Remove access to Admin Role if use is not a member of the role
                    int roleIndex = Null.NullInteger;
                    foreach (RoleInfo tmpRole in roles)
                    {
                        if (tmpRole.RoleName == this.PortalSettings.AdministratorRoleName)
                        {
                            if (!PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName))
                            {
                                roleIndex = roles.IndexOf(tmpRole);
                            }
                        }

                        break;
                    }

                    if (roleIndex > Null.NullInteger)
                    {
                        roles.RemoveAt(roleIndex);
                    }

                    this.cboRoles.DataSource = roles;
                    this.cboRoles.DataBind();
                }
            }
            else
            {
                if (!this.Page.IsPostBack)
                {
                    if (this.Role != null)
                    {
                        // cboRoles.Items.Add(new ListItem(Role.RoleName, Role.RoleID.ToString()));
                        this.cboRoles.AddItem(this.Role.RoleName, this.Role.RoleID.ToString());
                        this.cboRoles.Items[0].Selected = true;
                        this.lblTitle.Text = string.Format(Localization.GetString("RoleTitle.Text", this.LocalResourceFile), this.Role.RoleName, this.Role.RoleID);
                    }

                    this.cboRoles.Visible = false;
                    this.plRoles.Visible = false;
                }
            }

            // bind all portal users to dropdownlist
            if (this.UserId == -1)
            {
                // Make sure user has enough permissions
                if (this.Role.RoleName == this.PortalSettings.AdministratorRoleName && !PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NotAuthorized", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    this.pnlRoles.Visible = false;
                    this.pnlUserRoles.Visible = false;
                    this.chkNotify.Visible = false;
                    return;
                }

                if (this.UsersControl == UsersControl.Combo)
                {
                    if (this.cboUsers.Items.Count == 0)
                    {
                        foreach (UserInfo objUser in UserController.GetUsers(this.PortalId))
                        {
                            // cboUsers.Items.Add(new ListItem(objUser.DisplayName + " (" + objUser.Username + ")", objUser.UserID.ToString()));
                            this.cboUsers.AddItem(objUser.DisplayName + " (" + objUser.Username + ")", objUser.UserID.ToString());
                        }
                    }

                    this.txtUsers.Visible = false;
                    this.cboUsers.Visible = true;
                    this.cmdValidate.Visible = false;
                }
                else
                {
                    this.txtUsers.Visible = true;
                    this.cboUsers.Visible = false;
                    this.cmdValidate.Visible = true;
                }
            }
            else
            {
                if (this.User != null)
                {
                    this.txtUsers.Text = this.User.UserID.ToString();
                    this.lblTitle.Text = string.Format(Localization.GetString("UserTitle.Text", this.LocalResourceFile), this.User.Username, this.User.UserID);
                }

                this.txtUsers.Visible = false;
                this.cboUsers.Visible = false;
                this.cmdValidate.Visible = false;
                this.plUsers.Visible = false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// BindGrid loads the data grid from the Database.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void BindGrid()
        {
            if (this.RoleId != Null.NullInteger)
            {
                this.cmdAdd.Text = Localization.GetString("AddUser.Text", this.LocalResourceFile);
                this.grdUserRoles.DataKeyField = "UserId";
                this.grdUserRoles.Columns[2].Visible = false;
            }

            if (this.UserId != Null.NullInteger)
            {
                this.cmdAdd.Text = Localization.GetString("AddRole.Text", this.LocalResourceFile);
                this.grdUserRoles.DataKeyField = "RoleId";
                this.grdUserRoles.Columns[1].Visible = false;
            }

            this.grdUserRoles.DataSource = this.GetPagedDataSource();
            this.grdUserRoles.DataBind();

            this.ctlPagingControl.TotalRecords = this._totalRecords;
            this.ctlPagingControl.PageSize = this.PageSize;
            this.ctlPagingControl.CurrentPage = this.CurrentPage;
            this.ctlPagingControl.TabID = this.TabId;
            this.ctlPagingControl.QuerystringParams = System.Web.HttpUtility.UrlDecode(string.Join("&", this.Request.QueryString.ToString().Split('&').
                                                                        ToList().
                                                                        Where(s => s.StartsWith("ctl", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("mid", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("RoleId", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("UserId", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("filter", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("popUp", StringComparison.OrdinalIgnoreCase)).ToArray()));
        }

        private IList<UserRoleInfo> GetPagedDataSource()
        {
            var roleName = this.RoleId != Null.NullInteger ? this.Role.RoleName : Null.NullString;
            var userName = this.UserId != Null.NullInteger ? this.User.Username : Null.NullString;

            var userList = RoleController.Instance.GetUserRoles(this.PortalId, userName, roleName);
            this._totalRecords = userList.Count;
            this._totalPages = this._totalRecords % this.PageSize == 0 ? this._totalRecords / this.PageSize : (this._totalRecords / this.PageSize) + 1;

            return userList.Skip((this.CurrentPage - 1) * this.PageSize).Take(this.PageSize).ToList();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetDates gets the expiry/effective Dates of a Users Role membership.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="UserId">The Id of the User.</param>
        /// <param name="RoleId">The Id of the Role.</param>
        /// -----------------------------------------------------------------------------
        private void GetDates(int UserId, int RoleId)
        {
            DateTime? expiryDate = null;
            DateTime? effectiveDate = null;

            UserRoleInfo objUserRole = RoleController.Instance.GetUserRole(this.PortalId, UserId, RoleId);
            if (objUserRole != null)
            {
                if (Null.IsNull(objUserRole.EffectiveDate) == false)
                {
                    effectiveDate = objUserRole.EffectiveDate;
                }

                if (Null.IsNull(objUserRole.ExpiryDate) == false)
                {
                    expiryDate = objUserRole.ExpiryDate;
                }
            }
            else // new role assignment
            {
                RoleInfo objRole = RoleController.Instance.GetRole(this.PortalId, r => r.RoleID == RoleId);

                if (objRole.BillingPeriod > 0)
                {
                    switch (objRole.BillingFrequency)
                    {
                        case "D":
                            expiryDate = DateTime.Now.AddDays(objRole.BillingPeriod);
                            break;
                        case "W":
                            expiryDate = DateTime.Now.AddDays(objRole.BillingPeriod * 7);
                            break;
                        case "M":
                            expiryDate = DateTime.Now.AddMonths(objRole.BillingPeriod);
                            break;
                        case "Y":
                            expiryDate = DateTime.Now.AddYears(objRole.BillingPeriod);
                            break;
                    }
                }
            }

            this.effectiveDatePicker.SelectedDate = effectiveDate;
            this.expiryDatePicker.SelectedDate = expiryDate;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cboUsers_SelectedIndexChanged runs when the selected User is changed in the
        /// Users Drop-Down.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cboUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this.cboUsers.SelectedItem != null) && (this.cboRoles.SelectedItem != null))
            {
                this.SelectedUserID = int.Parse(this.cboUsers.SelectedItem.Value);
                this.GetDates(this.SelectedUserID, int.Parse(this.cboRoles.SelectedItem.Value));
            }

            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdValidate_Click executes when a user selects the Validate link for a username.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdValidate_Click(object sender, EventArgs e)
        {
            if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) == false)
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.txtUsers.Text))
            {
                // validate username
                UserInfo objUser = UserController.GetUserByName(this.PortalId, this.txtUsers.Text);
                if (objUser != null)
                {
                    this.GetDates(objUser.UserID, this.RoleId);
                    this.SelectedUserID = objUser.UserID;
                }
                else
                {
                    this.txtUsers.Text = string.Empty;
                }
            }

            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cboRoles_SelectedIndexChanged runs when the selected Role is changed in the
        /// Roles Drop-Down.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cboRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GetDates(this.UserId, int.Parse(this.cboRoles.SelectedItem.Value));
            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdAdd_Click runs when the Update Button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdAdd_Click(object sender, EventArgs e)
        {
            if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) == false)
            {
                return;
            }

            try
            {
                if (this.Page.IsValid)
                {
                    if ((this.Role != null) && (this.User != null))
                    {
                        // do not modify the portal Administrator account dates
                        if (this.User.UserID == this.PortalSettings.AdministratorId && this.Role.RoleID == this.PortalSettings.AdministratorRoleId)
                        {
                            this.effectiveDatePicker.SelectedDate = null;
                            this.expiryDatePicker.SelectedDate = null;
                        }

                        DateTime datEffectiveDate;
                        if (this.effectiveDatePicker.SelectedDate != null)
                        {
                            datEffectiveDate = this.effectiveDatePicker.SelectedDate.Value;
                        }
                        else
                        {
                            datEffectiveDate = Null.NullDate;
                        }

                        DateTime datExpiryDate;
                        if (this.expiryDatePicker.SelectedDate != null)
                        {
                            datExpiryDate = this.expiryDatePicker.SelectedDate.Value;
                        }
                        else
                        {
                            datExpiryDate = Null.NullDate;
                        }

                        // Add User to Role
                        var isOwner = false;

                        if ((this.Role.SecurityMode == SecurityMode.SocialGroup) || (this.Role.SecurityMode == SecurityMode.Both))
                        {
                            isOwner = this.chkIsOwner.Checked;
                        }

                        RoleController.AddUserRole(this.User, this.Role, this.PortalSettings, RoleStatus.Approved, datEffectiveDate, datExpiryDate, this.chkNotify.Checked, isOwner);
                        this.chkIsOwner.Checked = false; // reset the checkbox
                    }
                }

                this.BindGrid();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdUserRoles_ItemCreated runs when an item in the UserRoles Grid is created.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void grdUserRoles_ItemCreated(object sender, DataGridItemEventArgs e)
        {
            try
            {
                DataGridItem item = e.Item;

                var cmdDeleteUserRole = e.Item.FindControl("cmdDeleteUserRole") as ImageButton;
                var role = e.Item.DataItem as UserRoleInfo;

                if (cmdDeleteUserRole != null)
                {
                    if (this.RoleId == Null.NullInteger)
                    {
                        ClientAPI.AddButtonConfirm(cmdDeleteUserRole, string.Format(Localization.GetString("DeleteRoleFromUser.Text", this.LocalResourceFile), role.FullName, role.RoleName));
                    }
                    else
                    {
                        ClientAPI.AddButtonConfirm(cmdDeleteUserRole, string.Format(Localization.GetString("DeleteUsersFromRole.Text", this.LocalResourceFile), role.FullName, role.RoleName));
                    }

                    cmdDeleteUserRole.Attributes.Add("roleId", role.RoleID.ToString());
                    cmdDeleteUserRole.Attributes.Add("userId", role.UserID.ToString());
                }

                item.Cells[5].Visible = (this.Role.SecurityMode == SecurityMode.SocialGroup) || (this.Role.SecurityMode == SecurityMode.Both);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
