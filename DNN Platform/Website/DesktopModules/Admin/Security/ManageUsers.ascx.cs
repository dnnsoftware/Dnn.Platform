// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Admin.Security;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Profile;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ManageUsers UserModuleBase is used to manage Users.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class ManageUsers : UserModuleBase, IActionable
    {
        private readonly INavigationManager _navigationManager;

        public ManageUsers()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                if (!this.IsProfile)
                {
                    if (!this.AddUser && !this.IsEdit)
                    {
                        Actions.Add(
                            this.GetNextActionID(),
                            Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                            ModuleActionType.AddContent,
                            string.Empty,
                            "add.gif",
                            this.EditUrl(),
                            false,
                            SecurityAccessLevel.Admin,
                            true,
                            false);
                        if (ProfileProviderConfig.CanEditProviderProperties)
                        {
                            Actions.Add(
                                this.GetNextActionID(),
                                Localization.GetString("ManageProfile.Action", this.LocalResourceFile),
                                ModuleActionType.AddContent,
                                string.Empty,
                                "icon_profile_16px.gif",
                                this.EditUrl("ManageProfile"),
                                false,
                                SecurityAccessLevel.Admin,
                                true,
                                false);
                        }

                        Actions.Add(
                            this.GetNextActionID(),
                            Localization.GetString("Cancel.Action", this.LocalResourceFile),
                            ModuleActionType.AddContent,
                            string.Empty,
                            "lt.gif",
                            this.ReturnUrl,
                            false,
                            SecurityAccessLevel.Admin,
                            true,
                            false);
                    }
                }

                return Actions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the current Page No.
        /// </summary>
        public int PageNo
        {
            get
            {
                int _PageNo = 0;
                if (this.ViewState["PageNo"] != null && !this.IsPostBack)
                {
                    _PageNo = Convert.ToInt32(this.ViewState["PageNo"]);
                }

                return _PageNo;
            }

            set
            {
                this.ViewState["PageNo"] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to display the Manage Services tab.
        /// </summary>
        protected bool DisplayServices
        {
            get
            {
                object setting = GetSetting(this.PortalId, "Profile_ManageServices");
                return Convert.ToBoolean(setting) && !(this.IsEdit || this.User.IsSuperUser);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Redirect URL (after successful registration).
        /// </summary>
        protected string RedirectURL
        {
            get
            {
                string _RedirectURL = string.Empty;

                if (this.PortalSettings.Registration.RedirectAfterRegistration == Null.NullInteger)
                {
                    if (this.Request.QueryString["returnurl"] != null)
                    {
                        // return to the url passed to register
                        _RedirectURL = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                        // clean the return url to avoid possible XSS attack.
                        _RedirectURL = UrlUtils.ValidReturnUrl(_RedirectURL);

                        if (_RedirectURL.Contains("?returnurl"))
                        {
                            string baseURL = _RedirectURL.Substring(0, _RedirectURL.IndexOf("?returnurl"));
                            string returnURL = _RedirectURL.Substring(_RedirectURL.IndexOf("?returnurl") + 11);

                            _RedirectURL = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                        }
                    }

                    if (string.IsNullOrEmpty(_RedirectURL))
                    {
                        // redirect to current page
                        _RedirectURL = this._navigationManager.NavigateURL();
                    }
                }
                else // redirect to after registration page
                {
                    _RedirectURL = this._navigationManager.NavigateURL(this.PortalSettings.Registration.RedirectAfterRegistration);
                }

                return _RedirectURL;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url for the page.
        /// </summary>
        protected string ReturnUrl
        {
            get
            {
                return this._navigationManager.NavigateURL(this.TabId, string.Empty, !string.IsNullOrEmpty(this.UserFilter) ? this.UserFilter : string.Empty);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Filter to use.
        /// </summary>
        protected string UserFilter
        {
            get
            {
                string filterString = !string.IsNullOrEmpty(this.Request["filter"]) ? "filter=" + this.Request["filter"] : string.Empty;
                string filterProperty = !string.IsNullOrEmpty(this.Request["filterproperty"]) ? "filterproperty=" + this.Request["filterproperty"] : string.Empty;
                string page = !string.IsNullOrEmpty(this.Request["currentpage"]) ? "currentpage=" + this.Request["currentpage"] : string.Empty;

                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += "&";
                }

                if (!string.IsNullOrEmpty(filterProperty))
                {
                    filterString += filterProperty + "&";
                }

                if (!string.IsNullOrEmpty(page))
                {
                    filterString += page;
                }

                return filterString;
            }
        }

        /// <summary>
        /// Gets a value indicating whether flag to indicate only edit profile.
        /// </summary>
        protected bool EditProfileMode
        {
            get
            {
                bool editProfile;

                return !string.IsNullOrEmpty(this.Request.QueryString["editProfile"])
                       && bool.TryParse(this.Request["editProfile"], out editProfile)
                       && editProfile;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cmdCancel.Click += this.cmdCancel_Click;
            this.cmdAdd.Click += this.cmdAdd_Click;

            this.ctlUser.UserCreateCompleted += this.UserCreateCompleted;
            this.ctlUser.UserDeleted += this.UserDeleted;
            this.ctlUser.UserRemoved += this.UserRemoved;
            this.ctlUser.UserRestored += this.UserRestored;
            this.ctlUser.UserUpdateCompleted += this.UserUpdateCompleted;
            this.ctlUser.UserUpdateError += this.UserUpdateError;

            this.ctlProfile.ProfileUpdateCompleted += this.ProfileUpdateCompleted;
            this.ctlPassword.PasswordUpdated += this.PasswordUpdated;
            this.ctlPassword.PasswordQuestionAnswerUpdated += this.PasswordQuestionAnswerUpdated;
            this.ctlMembership.MembershipAuthorized += this.MembershipAuthorized;
            this.ctlMembership.MembershipPasswordUpdateChanged += this.MembershipPasswordUpdateChanged;
            this.ctlMembership.MembershipUnAuthorized += this.MembershipUnAuthorized;
            this.ctlMembership.MembershipUnLocked += this.MembershipUnLocked;
            this.ctlMembership.MembershipDemoteFromSuperuser += this.MembershipDemoteFromSuperuser;
            this.ctlMembership.MembershipPromoteToSuperuser += this.MembershipPromoteToSuperuser;

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            // Set the Membership Control Properties
            this.ctlMembership.ID = "Membership";
            this.ctlMembership.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlMembership.UserId = this.UserId;

            // Set the User Control Properties
            this.ctlUser.ID = "User";
            this.ctlUser.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlUser.UserId = this.UserId;

            // Set the Roles Control Properties
            this.ctlRoles.ID = "SecurityRoles";
            this.ctlRoles.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlRoles.ParentModule = this;

            // Set the Password Control Properties
            this.ctlPassword.ID = "Password";
            this.ctlPassword.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlPassword.UserId = this.UserId;

            // Set the Profile Control Properties
            this.ctlProfile.ID = "Profile";
            this.ctlProfile.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlProfile.UserId = this.UserId;

            // Customise the Control Title
            if (this.AddUser)
            {
                if (!this.Request.IsAuthenticated)
                {
                    // Register
                    this.ModuleConfiguration.ModuleTitle = Localization.GetString("Register.Title", this.LocalResourceFile);
                }
                else
                {
                    // Add User
                    this.ModuleConfiguration.ModuleTitle = Localization.GetString("AddUser.Title", this.LocalResourceFile);
                }

                this.userContainer.CssClass += " register";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Add an Action Event Handler to the Skin
                this.AddActionHandler(this.ModuleAction_Click);

                // Bind the User information to the controls
                this.BindData();

                this.loginLink.NavigateUrl = Globals.LoginURL(this.RedirectURL, this.Request.QueryString["override"] != null);

                if (this.PortalSettings.EnablePopUps)
                {
                    this.loginLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(this.loginLink.NavigateUrl, this, this.PortalSettings, true, false, 300, 650));
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this._navigationManager.NavigateURL(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRegister_Click runs when the Register button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            if (this.IsAdmin == false && this.HasManageUsersModulePermission() == false)
            {
                return;
            }

            if (this.ctlUser.IsValid && this.ctlProfile.IsValid)
            {
                this.ctlUser.CreateUser();
            }
            else
            {
                if (this.ctlUser.CreateStatus != UserCreateStatus.AddUser)
                {
                    this.AddLocalizedModuleMessage(UserController.GetUserCreateStatus(this.ctlUser.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipPasswordUpdateChanged runs when the Admin has forced the User to update their password.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void MembershipPasswordUpdateChanged(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            try
            {
                this.AddModuleMessage("UserPasswordUpdateChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                this.BindMembership();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindData()
        {
            if (this.User != null)
            {
                // If trying to add a SuperUser - check that user is a SuperUser
                if (this.VerifyUserPermissions() == false)
                {
                    return;
                }

                if (this.AddUser)
                {
                    this.cmdAdd.Text = Localization.GetString("AddUser", this.LocalResourceFile);
                    this.lblTitle.Text = Localization.GetString("AddUser", this.LocalResourceFile);
                }
                else
                {
                    if (!this.Request.IsAuthenticated)
                    {
                        this.titleRow.Visible = false;
                    }
                    else
                    {
                        if (this.IsProfile)
                        {
                            this.titleRow.Visible = false;
                        }
                        else
                        {
                            this.lblTitle.Text = string.Format(Localization.GetString("UserTitle", this.LocalResourceFile), this.User.Username, this.User.UserID);
                        }
                    }
                }

                if (!this.Page.IsPostBack)
                {
                    if (this.Request.QueryString["pageno"] != null)
                    {
                        this.PageNo = int.Parse(this.Request.QueryString["pageno"]);
                    }
                    else
                    {
                        this.PageNo = 0;
                    }
                }

                this.ShowPanel();
            }
            else
            {
                this.AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
            }
        }

        private bool VerifyUserPermissions()
        {
            if (this.AddUser && this.IsHostMenu && !this.UserInfo.IsSuperUser)
            {
                this.AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
                return false;
            }

            // Check if User is a member of the Current Portal (or a member of the MasterPortal if PortalGroups enabled)
            if (this.User.PortalID != Null.NullInteger && this.User.PortalID != this.PortalId)
            {
                this.AddModuleMessage("InvalidUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
                return false;
            }

            // Check if User is a SuperUser and that the current User is a SuperUser
            if (this.User.IsSuperUser && !this.UserInfo.IsSuperUser)
            {
                this.AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
                return false;
            }

            if (this.IsEdit)
            {
                // Check if user has admin rights
                if (!this.IsAdmin || (this.User.IsInRole(this.PortalSettings.AdministratorRoleName) && !PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName)))
                {
                    this.AddModuleMessage("NotAuthorized", ModuleMessage.ModuleMessageType.YellowWarning, true);
                    this.DisableForm();
                    return false;
                }
            }
            else
            {
                if (this.Request.IsAuthenticated)
                {
                    if (!PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName))
                    {
                        if (this.HasManageUsersModulePermission() == false)
                        {
                            // Display current user's profile
                            this.Response.Redirect(this._navigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, "UserID=" + this.UserInfo.UserID), true);
                        }
                    }
                }
                else
                {
                    if (this.User.UserID > Null.NullInteger)
                    {
                        this.AddModuleMessage("NotAuthorized", ModuleMessage.ModuleMessageType.YellowWarning, true);
                        this.DisableForm();
                        return false;
                    }
                }
            }

            return true;
        }

        private void BindMembership()
        {
            this.ctlMembership.User = this.User;
            this.ctlMembership.DataBind();
            this.AddModuleMessage("UserLockedOut", ModuleMessage.ModuleMessageType.YellowWarning, this.ctlMembership.UserMembership.LockedOut && (!this.Page.IsPostBack));
            this.imgLockedOut.Visible = this.ctlMembership.UserMembership.LockedOut;
            this.imgOnline.Visible = this.ctlMembership.UserMembership.IsOnLine;
        }

        private void BindUser()
        {
            if (this.AddUser)
            {
                this.ctlUser.ShowUpdate = false;
                this.CheckQuota();
            }

            this.ctlUser.User = this.User;
            this.ctlUser.DataBind();

            // Bind the Membership
            if (this.AddUser || (!this.IsAdmin))
            {
                this.membershipRow.Visible = false;
            }
            else
            {
                this.BindMembership();
            }
        }

        private void CheckQuota()
        {
            if (this.PortalSettings.Users < this.PortalSettings.UserQuota || this.UserInfo.IsSuperUser || this.PortalSettings.UserQuota == 0)
            {
                this.cmdAdd.Enabled = true;
            }
            else
            {
                this.cmdAdd.Enabled = false;
                this.AddModuleMessage("ExceededUserQuota", ModuleMessage.ModuleMessageType.YellowWarning, true);
            }
        }

        private void DisableForm()
        {
            this.adminTabNav.Visible = false;
            this.dnnRoleDetails.Visible = false;
            this.dnnPasswordDetails.Visible = false;
            this.dnnProfileDetails.Visible = false;
            this.actionsRow.Visible = false;
            this.ctlMembership.Visible = false;
            this.ctlUser.Visible = false;
        }

        private void ShowPanel()
        {
            if (this.AddUser)
            {
                this.adminTabNav.Visible = false;
                if (this.Request.IsAuthenticated && MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    // Admin adding user
                    this.dnnManageUsers.Visible = false;
                    this.actionsRow.Visible = false;
                    this.AddModuleMessage("CannotAddUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                }
                else
                {
                    this.dnnManageUsers.Visible = true;
                    this.actionsRow.Visible = true;
                }

                this.BindUser();
                this.dnnProfileDetails.Visible = false;
            }
            else
            {
                if (!this.IsAdmin)
                {
                    this.passwordTab.Visible = false;
                }
                else
                {
                    this.ctlPassword.User = this.User;
                    this.ctlPassword.DataBind();
                }

                if (!this.IsEdit || this.User.IsSuperUser)
                {
                    this.rolesTab.Visible = false;
                }
                else
                {
                    this.ctlRoles.DataBind();
                }

                this.BindUser();
                this.ctlProfile.User = this.User;
                this.ctlProfile.DataBind();
            }

            this.dnnRoleDetails.Visible = this.IsEdit && !this.User.IsSuperUser && !this.AddUser;
            this.dnnPasswordDetails.Visible = this.IsAdmin && !this.AddUser;

            if (this.EditProfileMode)
            {
                this.adminTabNav.Visible =
                    this.dnnUserDetails.Visible =
                    this.dnnRoleDetails.Visible =
                    this.dnnPasswordDetails.Visible =
                    this.actionsRow.Visible = false;
            }
        }

        private bool HasManageUsersModulePermission()
        {
            return ModulePermissionController.HasModulePermission(this.ModuleConfiguration.ModulePermissions, "MANAGEUSER");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ModuleAction_Click handles all ModuleAction events raised from the skin.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An ActionEventArgs object.</param>
        private void ModuleAction_Click(object sender, ActionEventArgs e)
        {
            switch (e.Action.CommandArgument)
            {
                case "ManageRoles":
                    // pnlRoles.Visible = true;
                    // pnlUser.Visible = false;
                    break;
                case "Cancel":
                    break;
                case "Delete":
                    break;
                case "Edit":
                    break;
                case "Save":
                    break;
                default:
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipAuthorized runs when the User has been unlocked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void MembershipAuthorized(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            try
            {
                this.AddModuleMessage("UserAuthorized", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                // Send Notification to User
                if (string.IsNullOrEmpty(this.User.Membership.Password) && !MembershipProviderConfig.RequiresQuestionAndAnswer && MembershipProviderConfig.PasswordRetrievalEnabled)
                {
                    UserInfo user = this.User;
                    this.User.Membership.Password = UserController.GetPassword(ref user, string.Empty);
                }

                this.BindMembership();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipPromoteToSuperuser runs when the User has been promoted to a superuser.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void MembershipPromoteToSuperuser(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            try
            {
                this.AddModuleMessage("UserPromotedToSuperuser", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                this.BindMembership();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipDemoteFromSuperuser runs when the User has been demoted to a regular user.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void MembershipDemoteFromSuperuser(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            try
            {
                this.AddModuleMessage("UserDemotedFromSuperuser", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                this.BindMembership();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipUnAuthorized runs when the User has been unlocked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void MembershipUnAuthorized(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            try
            {
                this.AddModuleMessage("UserUnAuthorized", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                this.BindMembership();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipUnLocked runs when the User has been unlocked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void MembershipUnLocked(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            try
            {
                this.AddModuleMessage("UserUnLocked", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                this.BindMembership();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PasswordQuestionAnswerUpdated runs when the Password Q and A have been updated.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void PasswordQuestionAnswerUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            PasswordUpdateStatus status = e.UpdateStatus;
            if (status == PasswordUpdateStatus.Success)
            {
                this.AddModuleMessage("PasswordQAChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);
            }
            else
            {
                this.AddModuleMessage(status.ToString(), ModuleMessage.ModuleMessageType.RedError, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PasswordUpdated runs when the Password has been updated or reset.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void PasswordUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            PasswordUpdateStatus status = e.UpdateStatus;

            if (status == PasswordUpdateStatus.Success)
            {
                // Send Notification to User
                try
                {
                    var accessingUser = (UserInfo)HttpContext.Current.Items["UserInfo"];
                    if (accessingUser.UserID != this.User.UserID)
                    {
                        // The password was changed by someone else
                        Mail.SendMail(this.User, MessageType.PasswordUpdated, this.PortalSettings);
                    }
                    else
                    {
                        // The User changed his own password
                        Mail.SendMail(this.User, MessageType.UserUpdatedOwnPassword, this.PortalSettings);
                    }

                    this.AddModuleMessage("PasswordChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                }
                catch (Exception ex)
                {
                    this.AddModuleMessage("PasswordMailError", ModuleMessage.ModuleMessageType.YellowWarning, true);
                    Exceptions.LogException(ex);
                }
            }
            else
            {
                this.AddModuleMessage(status.ToString(), ModuleMessage.ModuleMessageType.RedError, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProfileUpdateCompleted runs when the Profile has been updated.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void ProfileUpdateCompleted(object sender, EventArgs e)
        {
            if (this.IsAdmin == false)
            {
                return;
            }

            // Redirect to same page (this will update all controls for any changes to profile
            // and leave us at Page 0 (User Credentials)
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void SubscriptionUpdated(object sender, MemberServices.SubscriptionUpdatedEventArgs e)
        {
            string message = Null.NullString;
            if (e.Cancel)
            {
                message = string.Format(Localization.GetString("UserUnSubscribed", this.LocalResourceFile), e.RoleName);
            }
            else
            {
                message = string.Format(Localization.GetString("UserSubscribed", this.LocalResourceFile), e.RoleName);
            }

            this.AddLocalizedModuleMessage(message, ModuleMessage.ModuleMessageType.GreenSuccess, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserCreateCompleted runs when a new user has been Created.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void UserCreateCompleted(object sender, UserUserControlBase.UserCreatedEventArgs e)
        {
            try
            {
                if (e.CreateStatus == UserCreateStatus.Success)
                {
                    this.CompleteUserCreation(e.CreateStatus, e.NewUser, e.Notify, false);
                    this.Response.Redirect(this.ReturnUrl, true);
                }
                else
                {
                    this.AddLocalizedModuleMessage(UserController.GetUserCreateStatus(e.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserDeleted runs when the User has been deleted.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void UserDeleted(object sender, UserUserControlBase.UserDeletedEventArgs e)
        {
            try
            {
                this.Response.Redirect(this.ReturnUrl, true);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserUpdateCompleted runs when a user has been updated.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void UserRestored(object sender, UserUserControlBase.UserRestoredEventArgs e)
        {
            try
            {
                this.Response.Redirect(this.ReturnUrl, true);

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void UserRemoved(object sender, UserUserControlBase.UserRemovedEventArgs e)
        {
            try
            {
                this.Response.Redirect(this.ReturnUrl, true);

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void UserUpdateCompleted(object sender, EventArgs e)
        {
            this.Response.Redirect(this.Request.RawUrl, false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserUpdateError runs when there is an error updating the user.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void UserUpdateError(object sender, UserUserControlBase.UserUpdateErrorArgs e)
        {
            this.AddModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
        }
    }
}
