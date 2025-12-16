// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
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

    /// <summary>The ManageUsers UserModuleBase is used to manage Users.</summary>
    public partial class EditUser : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditUser));
        private readonly INavigationManager navigationManager;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="EditUser"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditUser()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditUser"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        public EditUser(INavigationManager navigationManager, IJavaScriptLibraryHelper javaScript, IHostSettings hostSettings, IPortalController portalController)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.javaScript = javaScript ?? this.DependencyProvider.GetRequiredService<IJavaScriptLibraryHelper>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
        }

        /// <summary>Gets or sets the current Page No.</summary>
        public int PageNo
        {
            get
            {
                int pageNo = 0;
                if (this.ViewState["PageNo"] != null && !this.IsPostBack)
                {
                    pageNo = Convert.ToInt32(this.ViewState["PageNo"]);
                }

                return pageNo;
            }

            set
            {
                this.ViewState["PageNo"] = value;
            }
        }

        public bool ShowVanityUrl { get; private set; }

        /// <summary>Gets a value indicating whether to display the Manage Services tab.</summary>
        protected bool DisplayServices
        {
            get
            {
                object setting = GetSetting(this.PortalId, "Profile_ManageServices");
                return Convert.ToBoolean(setting) && !(this.IsEdit || this.UserInfo.IsSuperUser);
            }
        }

        /// <summary>Gets the Redirect URL (after successful registration).</summary>
        protected string RedirectURL
        {
            get
            {
                string redirectURL = string.Empty;

                if (this.PortalSettings.Registration.RedirectAfterRegistration == Null.NullInteger)
                {
                    if (this.Request.QueryString["returnurl"] != null)
                    {
                        // return to the url passed to register
                        redirectURL = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                        // clean the return url to avoid possible XSS attack.
                        redirectURL = UrlUtils.ValidReturnUrl(redirectURL);

                        if (redirectURL.Contains("?returnurl"))
                        {
                            string baseURL = redirectURL.Substring(0, redirectURL.IndexOf("?returnurl"));
                            string returnURL = redirectURL.Substring(redirectURL.IndexOf("?returnurl") + 11);

                            redirectURL = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                        }
                    }

                    if (string.IsNullOrEmpty(redirectURL))
                    {
                        // redirect to current page
                        redirectURL = this.navigationManager.NavigateURL();
                    }
                }
                else
                {
                    // redirect to after registration page
                    redirectURL = this.navigationManager.NavigateURL(this.PortalSettings.Registration.RedirectAfterRegistration);
                }

                return redirectURL;
            }
        }

        /// <summary>Gets the Return Url for the page.</summary>
        protected string ReturnUrl
        {
            get
            {
                return this.navigationManager.NavigateURL(this.TabId, string.Empty, !string.IsNullOrEmpty(this.UserFilter) ? this.UserFilter : string.Empty);
            }
        }

        /// <summary>Gets and sets the Filter to use.</summary>
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

        private IPortalAliasInfo CurrentPortalAlias => this.PortalSettings.PortalAlias;

        /// <summary>Page_Init runs when the control is initialised.</summary>
        /// <param name="e">The event arguments.</param>
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

            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);
            this.javaScript.RequestRegistration(CommonJs.Knockout);

            // Set the Membership Control Properties
            this.ctlMembership.ID = "Membership";
            this.ctlMembership.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlMembership.UserId = this.UserInfo.UserID;

            // Set the Password Control Properties
            this.ctlPassword.ID = "Password";
            this.ctlPassword.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlPassword.UserId = this.UserInfo.UserID;

            // Set the Profile Control Properties
            this.ctlProfile.ID = "Profile";
            this.ctlProfile.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlProfile.UserId = this.UserInfo.UserID;

            // Set the Services Control Properties
            this.ctlServices.ID = "MemberServices";
            this.ctlServices.ModuleConfiguration = this.ModuleConfiguration;
            this.ctlServices.UserId = this.UserInfo.UserID;

            // Define DisplayName filed Enabled Property:
            object setting = GetSetting(this.UserPortalID, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                this.displayName.Enabled = false;
            }
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Bind the User information to the controls
                this.BindData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            UserInfo user = this.UserInfo;
            var success = false;
            if (this.PortalSettings.DataConsentActive)
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
            this.Response.Redirect(this.navigationManager.NavigateURL(this.PortalSettings.HomeTabId));
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (this.userForm.IsValid && (this.UserInfo != null))
            {
                if (this.UserInfo.UserID == this.PortalSettings.AdministratorId)
                {
                    // Clear the Portal Cache
                    DataCache.ClearPortalCache(this.UserPortalID, true);
                }
                else
                {
                    DataCache.ClearUserCache(this.PortalId, this.UserInfo.Username);
                }

                try
                {
                    // Update DisplayName to conform to Format
                    this.UpdateDisplayName();

                    // DNN-5874 Check if unique display name is required
                    if (this.PortalSettings.Registration.RequireUniqueDisplayName)
                    {
                        var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(this.PortalId, 0, 2, "DisplayName", true, "DisplayName", this.UserInfo.DisplayName);
                        if (usersWithSameDisplayName.Any(user => user.UserID != this.UserInfo.UserID))
                        {
                            throw new Exception("Display Name must be unique");
                        }
                    }

                    var prevUserEmail = UserController.Instance.GetUserById(this.PortalId, this.UserInfo.UserID)?.Email;

                    if (!string.IsNullOrWhiteSpace(prevUserEmail) && !prevUserEmail.Equals(this.UserInfo.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        // on email address change need to invalidate existing 'reset password' link
                        this.UserInfo.PasswordResetExpiration = Null.NullDate;
                    }

                    UserController.UpdateUser(this.UserPortalID, this.UserInfo);

                    // make sure username matches possibly changed email address
                    if (this.PortalSettings.Registration.UseEmailAsUserName)
                    {
                        if (this.UserInfo.Username.ToLower() != this.UserInfo.Email.ToLower())
                        {
                            UserController.ChangeUsername(this.UserInfo.UserID, this.UserInfo.Email);

                            // after username changed, should redirect to login page to let user authenticate again.
                            var loginUrl = Globals.LoginURL(HttpUtility.UrlEncode(this.Request.RawUrl), false);
                            var spliter = loginUrl.Contains("?") ? "&" : "?";
                            loginUrl = $"{loginUrl}{spliter}username={this.UserInfo.Email}&usernameChanged=true";
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
            if (this.UserInfo != null)
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

                this.userForm.DataSource = this.UserInfo;

                // hide username field in UseEmailAsUserName mode
                bool disableUsername = PortalController.GetPortalSettingAsBoolean(this.portalController, "Registration_UseEmailAsUserName", this.PortalId, false);
                if (disableUsername)
                {
                    this.userForm.Items[0].Visible = false;
                }

                if (!this.Page.IsPostBack)
                {
                    this.userForm.DataBind();
                }

                this.ctlPassword.User = this.UserInfo;
                this.ctlPassword.DataBind();

                if (!this.DisplayServices)
                {
                    this.servicesTab.Visible = false;
                }
                else
                {
                    this.ctlServices.User = this.UserInfo;
                    this.ctlServices.DataBind();
                }

                this.BindUser();
                this.ctlProfile.User = this.UserInfo;
                this.ctlProfile.DataBind();

                this.dnnServicesDetails.Visible = this.DisplayServices;

                var urlSettings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(this.PortalSettings.PortalId);
                var showVanityUrl = (Config.GetFriendlyUrlProvider() == "advanced") && !this.UserInfo.IsSuperUser;
                if (showVanityUrl)
                {
                    this.VanityUrlRow.Visible = true;
                    if (string.IsNullOrEmpty(this.UserInfo.VanityUrl))
                    {
                        // Clean Display Name
                        var options = UrlRewriterUtils.GetOptionsFromSettings(urlSettings);
                        var cleanUrl = FriendlyUrlController.CleanNameForUrl(this.UserInfo.DisplayName, options, out _);
                        var uniqueUrl = FriendlyUrlController.ValidateUrl(cleanUrl, -1, this.PortalSettings, out _).ToLowerInvariant();

                        this.VanityUrlAlias.Text = $"{this.CurrentPortalAlias.HttpAlias}/{urlSettings.VanityUrlPrefix}/";
                        this.VanityUrlTextBox.Text = uniqueUrl;
                        this.ShowVanityUrl = true;
                    }
                    else
                    {
                        this.VanityUrl.Text = $"{this.CurrentPortalAlias.HttpAlias}/{urlSettings.VanityUrlPrefix}/{this.UserInfo.VanityUrl}";
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
            if (this.UserInfo.PortalID != Null.NullInteger && this.UserInfo.PortalID != this.PortalId)
            {
                this.AddModuleMessage("InvalidUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
                return false;
            }

            // Check if User is a SuperUser and that the current User is a SuperUser
            if (this.UserInfo.IsSuperUser && !this.UserInfo.IsSuperUser)
            {
                this.AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                this.DisableForm();
                return false;
            }

            if (this.IsEdit)
            {
                // Check if user has admin rights
                if (!this.IsAdmin || (this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName)))
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
                            this.Response.Redirect(this.navigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, "UserID=" + this.UserInfo.UserID), true);
                        }
                    }
                    else
                    {
                        if (this.UserInfo.UserID > Null.NullInteger)
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
            this.ctlMembership.User = this.UserInfo;
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
                this.UserInfo.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
            }
        }

        /// <summary>PasswordQuestionAnswerUpdated runs when the Password Q and A have been updated.</summary>
        private void PasswordQuestionAnswerUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
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

        /// <summary>PasswordUpdated runs when the Password has been updated or reset.</summary>
        private void PasswordUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            PasswordUpdateStatus status = e.UpdateStatus;

            if (status == PasswordUpdateStatus.Success)
            {
                // Send Notification to User
                try
                {
                    var accessingUser = (UserInfo)HttpContext.Current.Items["UserInfo"];
                    if (accessingUser.UserID != this.UserInfo.UserID)
                    {
                        // The password was changed by someone else
                        Mail.SendMail(this.UserInfo, MessageType.PasswordReminder, this.PortalSettings);
                    }
                    else
                    {
                        // The User changed his own password
                        Mail.SendMail(this.UserInfo, MessageType.UserUpdatedOwnPassword, this.PortalSettings);
                        PortalSecurity.Instance.SignIn(this.UserInfo, false);
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

        /// <summary>ProfileUpdateCompleted runs when the Profile has been updated.</summary>
        private void ProfileUpdateCompleted(object sender, EventArgs e)
        {
            // Notify the user that his/her profile was updated
            Mail.SendMail(this.UserInfo, MessageType.ProfileUpdated, this.PortalSettings);

            ProfilePropertyDefinition localeProperty = this.UserInfo.Profile.GetProperty("PreferredLocale");
            if (localeProperty.IsDirty)
            {
                // store PreferredLocale in cookie, if none specified set to portal default.
                if (this.UserInfo.Profile.PreferredLocale == string.Empty)
                {
                    Localization.SetLanguage(PortalController.GetPortalDefaultLanguage(this.hostSettings, this.UserInfo.PortalID));
                }
                else
                {
                    Localization.SetLanguage(this.UserInfo.Profile.PreferredLocale);
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
                message = string.Format(CultureInfo.CurrentCulture, Localization.GetString("UserUnSubscribed", this.LocalResourceFile), e.RoleName);
            }
            else
            {
                message = string.Format(CultureInfo.CurrentCulture, Localization.GetString("UserSubscribed", this.LocalResourceFile), e.RoleName);
            }

            this.AddLocalizedModuleMessage(message, ModuleMessage.ModuleMessageType.GreenSuccess, true);
        }
    }
}
