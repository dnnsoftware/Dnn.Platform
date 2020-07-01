// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.Admin.Security;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ManageUsers UserModuleBase is used to manage Users.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class EditUser : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditUser));
        private readonly INavigationManager _navigationManager;

        public EditUser()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
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

        public bool ShowVanityUrl { get; private set; }

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cmdDelete.Click += this.cmdDelete_Click;
            this.cmdUpdate.Click += this.cmdUpdate_Click;

            this.ctlServices.SubscriptionUpdated += this.SubscriptionUpdated;
            this.ctlProfile.ProfileUpdateCompleted += this.ProfileUpdateCompleted;
            this.ctlPassword.PasswordUpdated += this.PasswordUpdated;
            this.ctlPassword.PasswordQuestionAnswerUpdated += this.PasswordQuestionAnswerUpdated;

            this.email.ValidationExpression = this.PortalSettings.Registration.EmailValidator;

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.Knockout);

            // Set the Membership Control Properties
            this.ctlMembership.ID = "Membership";
            this.ctlMembership.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlMembership.UserId = this.UserId;

            // Set the Password Control Properties
            this.ctlPassword.ID = "Password";
            this.ctlPassword.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlPassword.UserId = this.UserId;

            // Set the Profile Control Properties
            this.ctlProfile.ID = "Profile";
            this.ctlProfile.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlProfile.UserId = this.UserId;

            // Set the Services Control Properties
            this.ctlServices.ID = "MemberServices";
            this.ctlServices.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlServices.UserId = this.UserId;

            // Define DisplayName filed Enabled Property:
            object setting = GetSetting(this.UserPortalID, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                this.displayName.Enabled = false;
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
                // Bind the User information to the controls
                this.BindData();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            UserInfo user = this.User;
            var success = false;
            if (this.PortalSettings.DataConsentActive && user.UserID == this.UserInfo.UserID)
            {
                switch (this.PortalSettings.DataConsentUserDeleteAction)
                {
                    case PortalSettings.UserDeleteAction.Manual:
                        user.Membership.Approved = false;
                        UserController.UpdateUser(this.PortalSettings.PortalId, user);
                        UserController.UserRequestsRemoval(user, true);
                        success = true;
                        break;
                    case PortalSettings.UserDeleteAction.DelayedHardDelete:
                        success = UserController.DeleteUser(ref user, true, false);
                        UserController.UserRequestsRemoval(user, true);
                        break;
                    case PortalSettings.UserDeleteAction.HardDelete:
                        success = UserController.RemoveUser(user);
                        break;
                    default: // if user delete is switched off under Data Consent then we revert to the old behavior
                        success = UserController.DeleteUser(ref user, true, false);
                        break;
                }
            }
            else
            {
                success = UserController.DeleteUser(ref user, true, false);
            }

            if (!success)
            {
                this.AddModuleMessage("UserDeleteError", ModuleMessage.ModuleMessageType.RedError, true);
            }

            // DNN-26777
            PortalSecurity.Instance.SignOut();
            this.Response.Redirect(this._navigationManager.NavigateURL(this.PortalSettings.HomeTabId));
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (this.userForm.IsValid && (this.User != null))
            {
                if (this.User.UserID == this.PortalSettings.AdministratorId)
                {
                    // Clear the Portal Cache
                    DataCache.ClearPortalCache(this.UserPortalID, true);
                }

                try
                {
                    // Update DisplayName to conform to Format
                    this.UpdateDisplayName();

                    // DNN-5874 Check if unique display name is required
                    if (this.PortalSettings.Registration.RequireUniqueDisplayName)
                    {
                        var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(this.PortalId, 0, 2, "DisplayName", true, "DisplayName", this.User.DisplayName);
                        if (usersWithSameDisplayName.Any(user => user.UserID != this.User.UserID))
                        {
                            throw new Exception("Display Name must be unique");
                        }
                    }

                    UserController.UpdateUser(this.UserPortalID, this.User);

                    // make sure username matches possibly changed email address
                    if (this.PortalSettings.Registration.UseEmailAsUserName)
                    {
                        if (this.User.Username.ToLower() != this.User.Email.ToLower())
                        {
                            UserController.ChangeUsername(this.User.UserID, this.User.Email);

                            // after username changed, should redirect to login page to let user authenticate again.
                            var loginUrl = Globals.LoginURL(HttpUtility.UrlEncode(this.Request.RawUrl), false);
                            var spliter = loginUrl.Contains("?") ? "&" : "?";
                            loginUrl = $"{loginUrl}{spliter}username={this.User.Email}&usernameChanged=true";
                            this.Response.Redirect(loginUrl, true);
                        }
                    }

                    this.Response.Redirect(this.Request.RawUrl);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    if (exc.Message == "Display Name must be unique")
                    {
                        this.AddModuleMessage("DisplayNameNotUnique", ModuleMessage.ModuleMessageType.RedError, true);
                    }
                    else
                    {
                        this.AddModuleMessage("UserUpdatedError", ModuleMessage.ModuleMessageType.RedError, true);
                    }
                }
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

                this.userForm.DataSource = this.User;

                // hide username field in UseEmailAsUserName mode
                bool disableUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);
                if (disableUsername)
                {
                    this.userForm.Items[0].Visible = false;
                }

                if (!this.Page.IsPostBack)
                {
                    this.userForm.DataBind();
                }

                this.ctlPassword.User = this.User;
                this.ctlPassword.DataBind();

                if (!this.DisplayServices)
                {
                    this.servicesTab.Visible = false;
                }
                else
                {
                    this.ctlServices.User = this.User;
                    this.ctlServices.DataBind();
                }

                this.BindUser();
                this.ctlProfile.User = this.User;
                this.ctlProfile.DataBind();

                this.dnnServicesDetails.Visible = this.DisplayServices;

                var urlSettings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(this.PortalSettings.PortalId);
                var showVanityUrl = (Config.GetFriendlyUrlProvider() == "advanced") && !this.User.IsSuperUser;
                if (showVanityUrl)
                {
                    this.VanityUrlRow.Visible = true;
                    if (string.IsNullOrEmpty(this.User.VanityUrl))
                    {
                        // Clean Display Name
                        bool modified;
                        var options = UrlRewriterUtils.GetOptionsFromSettings(urlSettings);
                        var cleanUrl = FriendlyUrlController.CleanNameForUrl(this.User.DisplayName, options, out modified);
                        var uniqueUrl = FriendlyUrlController.ValidateUrl(cleanUrl, -1, this.PortalSettings, out modified).ToLowerInvariant();

                        this.VanityUrlAlias.Text = string.Format("{0}/{1}/", this.PortalSettings.PortalAlias.HTTPAlias, urlSettings.VanityUrlPrefix);
                        this.VanityUrlTextBox.Text = uniqueUrl;
                        this.ShowVanityUrl = true;
                    }
                    else
                    {
                        this.VanityUrl.Text = string.Format("{0}/{1}/{2}", this.PortalSettings.PortalAlias.HTTPAlias, urlSettings.VanityUrlPrefix, this.User.VanityUrl);
                        this.ShowVanityUrl = false;
                    }
                }
            }
            else
            {
                this.AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
            }
        }

        private bool VerifyUserPermissions()
        {
            if (this.IsHostMenu && !this.UserInfo.IsSuperUser)
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
                if (!this.IsUser)
                {
                    if (this.Request.IsAuthenticated)
                    {
                        if (!PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName))
                        {
                            // Display current user's profile
                            this.Response.Redirect(this._navigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, "UserID=" + this.UserInfo.UserID), true);
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
            }

            return true;
        }

        private void BindMembership()
        {
            this.ctlMembership.User = this.User;
            this.ctlMembership.DataBind();
            this.AddModuleMessage("UserLockedOut", ModuleMessage.ModuleMessageType.YellowWarning, this.ctlMembership.UserMembership.LockedOut && (!this.Page.IsPostBack));
        }

        private void BindUser()
        {
            this.BindMembership();
        }

        private void DisableForm()
        {
            this.adminTabNav.Visible = false;
            this.dnnProfileDetails.Visible = false;
            this.dnnServicesDetails.Visible = false;
            this.actionsRow.Visible = false;
            this.ctlMembership.Visible = false;
        }

        private void UpdateDisplayName()
        {
            // Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                this.User.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
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
            if (this.IsUserOrAdmin == false)
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
            if (this.IsUserOrAdmin == false)
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
                        Mail.SendMail(this.User, MessageType.PasswordReminder, this.PortalSettings);
                    }
                    else
                    {
                        // The User changed his own password
                        Mail.SendMail(this.User, MessageType.UserUpdatedOwnPassword, this.PortalSettings);
                        PortalSecurity.Instance.SignIn(this.User, false);
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
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.IsUser)
            {
                // Notify the user that his/her profile was updated
                Mail.SendMail(this.User, MessageType.ProfileUpdated, this.PortalSettings);

                ProfilePropertyDefinition localeProperty = this.User.Profile.GetProperty("PreferredLocale");
                if (localeProperty.IsDirty)
                {
                    // store preferredlocale in cookie, if none specified set to portal default.
                    if (this.User.Profile.PreferredLocale == string.Empty)
                    {
                        Localization.SetLanguage(PortalController.GetPortalDefaultLanguage(this.User.PortalID));
                    }
                    else
                    {
                        Localization.SetLanguage(this.User.Profile.PreferredLocale);
                    }
                }
            }

            // Redirect to same page (this will update all controls for any changes to profile
            // and leave us at Page 0 (User Credentials)
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        private void SubscriptionUpdated(object sender, MemberServices.SubscriptionUpdatedEventArgs e)
        {
            string message;
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
    }
}
