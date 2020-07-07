// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Common.Utils;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.Admin.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Messaging.Data;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    using Host = DotNetNuke.Entities.Host.Host;

    /// <summary>
    /// The Signin UserModuleBase is used to provide a login for a registered user.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Login : UserModuleBase
    {
        private const string LOGIN_PATH = "/login";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Login));

        private static readonly Regex UserLanguageRegex = new Regex(
            "(.*)(&|\\?)(language=)([^&\\?]+)(.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly INavigationManager _navigationManager;

        private readonly List<AuthenticationLoginBase> _loginControls = new List<AuthenticationLoginBase>();
        private readonly List<AuthenticationLoginBase> _defaultauthLogin = new List<AuthenticationLoginBase>();
        private readonly List<OAuthLoginBase> _oAuthControls = new List<OAuthLoginBase>();

        public Login()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>
        /// Gets the Redirect URL (after successful login).
        /// </summary>
        protected string RedirectURL
        {
            get
            {
                var redirectURL = string.Empty;

                var setting = GetSetting(this.PortalId, "Redirect_AfterLogin");

                // first we need to check if there is a returnurl
                if (this.Request.QueryString["returnurl"] != null)
                {
                    // return to the url passed to signin
                    redirectURL = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                    // clean the return url to avoid possible XSS attack.
                    redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                }

                if (this.Request.Cookies["returnurl"] != null)
                {
                    // return to the url passed to signin
                    redirectURL = HttpUtility.UrlDecode(this.Request.Cookies["returnurl"].Value);

                    // clean the return url to avoid possible XSS attack.
                    redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                }

                if (this.Request.Params["appctx"] != null)
                {
                    // HACK return to the url passed to signin (LiveID)
                    redirectURL = HttpUtility.UrlDecode(this.Request.Params["appctx"]);

                    // clean the return url to avoid possible XSS attack.
                    redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                }

                var alias = this.PortalAlias.HTTPAlias;
                var comparison = StringComparison.InvariantCultureIgnoreCase;

                // we need .TrimEnd('/') because a portlalias for a specific culture will not have a trailing /, while a returnurl will.
                var isDefaultPage = redirectURL == "/"
                    || (alias.Contains("/") && redirectURL.TrimEnd('/').Equals(alias.Substring(alias.IndexOf("/", comparison)), comparison));

                if (string.IsNullOrEmpty(redirectURL) || isDefaultPage)
                {
                    if (
                        this.NeedRedirectAfterLogin
                        && (isDefaultPage || this.IsRedirectingFromLoginUrl())
                        && Convert.ToInt32(setting) != Null.NullInteger)
                    {
                        redirectURL = this._navigationManager.NavigateURL(Convert.ToInt32(setting));
                    }
                    else
                    {
                        if (this.PortalSettings.LoginTabId != -1 && this.PortalSettings.HomeTabId != -1)
                        {
                            // redirect to portal home page specified
                            redirectURL = this._navigationManager.NavigateURL(this.PortalSettings.HomeTabId);
                        }
                        else
                        {
                            // redirect to current page
                            redirectURL = this._navigationManager.NavigateURL();
                        }
                    }
                }

                // replace language parameter in querystring, to make sure that user will see page in correct language
                if (this.UserId != -1 && this.User != null)
                {
                    if (!string.IsNullOrEmpty(this.User.Profile.PreferredLocale)
                            && this.User.Profile.PreferredLocale != CultureInfo.CurrentCulture.Name
                            && this.LocaleEnabled(this.User.Profile.PreferredLocale))
                    {
                        redirectURL = ReplaceLanguage(redirectURL, CultureInfo.CurrentCulture.Name, this.User.Profile.PreferredLocale);
                    }
                }

                // check for insecure account defaults
                var qsDelimiter = "?";
                if (redirectURL.Contains("?"))
                {
                    qsDelimiter = "&";
                }

                if (this.LoginStatus == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD)
                {
                    redirectURL = redirectURL + qsDelimiter + "runningDefault=1";
                }
                else if (this.LoginStatus == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD)
                {
                    redirectURL = redirectURL + qsDelimiter + "runningDefault=2";
                }

                return redirectURL;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the Captcha control is used to validate the login.
        /// </summary>
        protected bool UseCaptcha
        {
            get
            {
                object setting = GetSetting(this.PortalId, "Security_CaptchaLogin");
                return Convert.ToBoolean(setting);
            }
        }

        /// <summary>
        /// Gets or sets and sets the current AuthenticationType.
        /// </summary>
        protected string AuthenticationType
        {
            get
            {
                var authenticationType = Null.NullString;
                if (this.ViewState["AuthenticationType"] != null)
                {
                    authenticationType = Convert.ToString(this.ViewState["AuthenticationType"]);
                }

                return authenticationType;
            }

            set
            {
                this.ViewState["AuthenticationType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets and sets a flag that determines whether the user should be automatically registered.
        /// </summary>
        protected bool AutoRegister
        {
            get
            {
                var autoRegister = Null.NullBoolean;
                if (this.ViewState["AutoRegister"] != null)
                {
                    autoRegister = Convert.ToBoolean(this.ViewState["AutoRegister"]);
                }

                return autoRegister;
            }

            set
            {
                this.ViewState["AutoRegister"] = value;
            }
        }

        protected NameValueCollection ProfileProperties
        {
            get
            {
                var profile = new NameValueCollection();
                if (this.ViewState["ProfileProperties"] != null)
                {
                    profile = (NameValueCollection)this.ViewState["ProfileProperties"];
                }

                return profile;
            }

            set
            {
                this.ViewState["ProfileProperties"] = value;
            }
        }

        /// <summary>
        /// Gets or sets and sets the current Page No.
        /// </summary>
        protected int PageNo
        {
            get
            {
                var pageNo = 0;
                if (this.ViewState["PageNo"] != null)
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

        /// <summary>
        /// Gets or sets a value indicating whether gets and sets a flag that determines whether a permanent auth cookie should be created.
        /// </summary>
        protected bool RememberMe
        {
            get
            {
                var rememberMe = Null.NullBoolean;
                if (this.ViewState["RememberMe"] != null)
                {
                    rememberMe = Convert.ToBoolean(this.ViewState["RememberMe"]);
                }

                return rememberMe;
            }

            set
            {
                this.ViewState["RememberMe"] = value;
            }
        }

        protected UserLoginStatus LoginStatus
        {
            get
            {
                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                if (this.ViewState["LoginStatus"] != null)
                {
                    loginStatus = (UserLoginStatus)this.ViewState["LoginStatus"];
                }

                return loginStatus;
            }

            set
            {
                this.ViewState["LoginStatus"] = value;
            }
        }

        /// <summary>
        /// Gets or sets and sets the current UserToken.
        /// </summary>
        protected string UserToken
        {
            get
            {
                var userToken = string.Empty;
                if (this.ViewState["UserToken"] != null)
                {
                    userToken = Convert.ToString(this.ViewState["UserToken"]);
                }

                return userToken;
            }

            set
            {
                this.ViewState["UserToken"] = value;
            }
        }

        /// <summary>
        /// Gets or sets and sets the current UserName.
        /// </summary>
        protected string UserName
        {
            get
            {
                var userName = string.Empty;
                if (this.ViewState["UserName"] != null)
                {
                    userName = Convert.ToString(this.ViewState["UserName"]);
                }

                return userName;
            }

            set
            {
                this.ViewState["UserName"] = value;
            }
        }

        private bool NeedRedirectAfterLogin =>
               this.LoginStatus == UserLoginStatus.LOGIN_SUCCESS
            || this.LoginStatus == UserLoginStatus.LOGIN_SUPERUSER
            || this.LoginStatus == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD
            || this.LoginStatus == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD;

        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.ctlPassword.PasswordUpdated += this.PasswordUpdated;
            this.ctlProfile.ProfileUpdated += this.ProfileUpdated;
            this.ctlUser.UserCreateCompleted += this.UserCreateCompleted;
            this.ctlDataConsent.DataConsentCompleted += this.DataConsentCompleted;

            // Set the User Control Properties
            this.ctlUser.ID = "User";

            // Set the Password Control Properties
            this.ctlPassword.ID = "Password";

            // Set the Profile Control Properties
            this.ctlProfile.ID = "Profile";

            // Set the Data Consent Control Properties
            this.ctlDataConsent.ID = "DataConsent";

            // Override the redirected page title if page has loaded with ctl=Login
            if (this.Request.QueryString["ctl"] != null)
            {
                if (this.Request.QueryString["ctl"].ToLowerInvariant() == "login")
                {
                    var myPage = (CDefault)this.Page;
                    if (myPage.PortalSettings.LoginTabId == this.TabId || myPage.PortalSettings.LoginTabId == -1)
                    {
                        myPage.Title = Localization.GetString("ControlTitle_login", this.LocalResourceFile);
                    }
                }
            }
        }

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdAssociate.Click += this.cmdAssociate_Click;
            this.cmdCreateUser.Click += this.cmdCreateUser_Click;
            this.cmdProceed.Click += this.cmdProceed_Click;

            // Verify if portal has a customized login page
            if (!Null.IsNull(this.PortalSettings.LoginTabId) && Globals.IsAdminControl())
            {
                if (Globals.ValidateLoginTabID(this.PortalSettings.LoginTabId))
                {
                    // login page exists and trying to access this control directly with url param -> not allowed
                    var parameters = new string[3];
                    if (!string.IsNullOrEmpty(this.Request.QueryString["returnUrl"]))
                    {
                        parameters[0] = "returnUrl=" + HttpUtility.UrlEncode(this.Request.QueryString["returnUrl"]);
                    }

                    if (!string.IsNullOrEmpty(this.Request.QueryString["username"]))
                    {
                        parameters[1] = "username=" + HttpUtility.UrlEncode(this.Request.QueryString["username"]);
                    }

                    if (!string.IsNullOrEmpty(this.Request.QueryString["verificationcode"]))
                    {
                        parameters[2] = "verificationcode=" + HttpUtility.UrlEncode(this.Request.QueryString["verificationcode"]);
                    }

                    this.Response.Redirect(this._navigationManager.NavigateURL(this.PortalSettings.LoginTabId, string.Empty, parameters));
                }
            }

            if (this.Page.IsPostBack == false)
            {
                try
                {
                    this.PageNo = 0;
                }
                catch (Exception ex)
                {
                    // control not there
                    Logger.Error(ex);
                }
            }

            if (!this.Request.IsAuthenticated || this.UserNeedsVerification())
            {
                this.ShowPanel();
            }
            else // user is already authenticated
            {
                // if a Login Page has not been specified for the portal
                if (Globals.IsAdminControl())
                {
                    // redirect browser
                    this.Response.Redirect(this.RedirectURL, true);
                }
                else // make module container invisible if user is not a page admin
                {
                    var path = this.RedirectURL.Split('?')[0];
                    if (this.NeedRedirectAfterLogin && path != this._navigationManager.NavigateURL() && path != this._navigationManager.NavigateURL(this.PortalSettings.HomeTabId))
                    {
                        this.Response.Redirect(this.RedirectURL, true);
                    }

                    if (TabPermissionController.CanAdminPage())
                    {
                        this.ShowPanel();
                    }
                    else
                    {
                        this.ContainerControl.Visible = false;
                    }
                }
            }

            this.divCaptcha.Visible = this.UseCaptcha;

            if (this.UseCaptcha)
            {
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", Localization.SharedResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", Localization.SharedResourceFile);
            }
        }

        /// <summary>
        /// cmdAssociate_Click runs when the associate button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdAssociate_Click(object sender, EventArgs e)
        {
            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || (!this.UseCaptcha))
            {
                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                var userRequestIpAddressController = UserRequestIPAddressController.Instance;
                var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));
                UserInfo objUser = UserController.ValidateUser(
                    this.PortalId,
                    this.txtUsername.Text,
                    this.txtPassword.Text,
                    "DNN",
                    string.Empty,
                    this.PortalSettings.PortalName,
                    ipAddress,
                    ref loginStatus);
                if (loginStatus == UserLoginStatus.LOGIN_SUCCESS)
                {
                    // Assocate alternate Login with User and proceed with Login
                    AuthenticationController.AddUserAuthentication(objUser.UserID, this.AuthenticationType, this.UserToken);
                    if (objUser != null)
                    {
                        this.UpdateProfile(objUser, true);
                    }

                    this.ValidateUser(objUser, true);
                }
                else
                {
                    this.AddModuleMessage("AssociationFailed", ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
        }

        /// <summary>
        /// cmdCreateUser runs when the register (as new user) button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdCreateUser_Click(object sender, EventArgs e)
        {
            this.User.Membership.Password = UserController.GeneratePassword();

            if (this.AutoRegister)
            {
                this.ctlUser.User = this.User;

                // Call the Create User method of the User control so that it can create
                // the user and raise the appropriate event(s)
                this.ctlUser.CreateUser();
            }
            else
            {
                if (this.ctlUser.IsValid)
                {
                    // Call the Create User method of the User control so that it can create
                    // the user and raise the appropriate event(s)
                    this.ctlUser.CreateUser();
                }
            }
        }

        /// <summary>
        /// cmdProceed_Click runs when the Proceed Anyway button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdProceed_Click(object sender, EventArgs e)
        {
            var user = this.ctlPassword.User;
            this.ValidateUser(user, true);
        }

        /// <summary>
        /// PasswordUpdated runs when the password is updated.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void PasswordUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            PasswordUpdateStatus status = e.UpdateStatus;
            if (status == PasswordUpdateStatus.Success)
            {
                this.AddModuleMessage("PasswordChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                var user = this.ctlPassword.User;
                user.Membership.LastPasswordChangeDate = DateTime.Now;
                user.Membership.UpdatePassword = false;
                this.LoginStatus = user.IsSuperUser ? UserLoginStatus.LOGIN_SUPERUSER : UserLoginStatus.LOGIN_SUCCESS;
                UserLoginStatus userstatus = UserLoginStatus.LOGIN_FAILURE;
                UserController.CheckInsecurePassword(user.Username, user.Membership.Password, ref userstatus);
                this.LoginStatus = userstatus;
                this.ValidateUser(user, true);
            }
            else
            {
                this.AddModuleMessage(status.ToString(), ModuleMessage.ModuleMessageType.RedError, true);
            }
        }

        /// <summary>
        /// DataConsentCompleted runs after the user has gone through the data consent screen.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void DataConsentCompleted(object sender, DataConsent.DataConsentEventArgs e)
        {
            switch (e.Status)
            {
                case DataConsent.DataConsentStatus.Consented:
                    this.ValidateUser(this.ctlDataConsent.User, true);
                    break;
                case DataConsent.DataConsentStatus.Cancelled:
                case DataConsent.DataConsentStatus.RemovedAccount:
                    this.Response.Redirect(this._navigationManager.NavigateURL(this.PortalSettings.HomeTabId), true);
                    break;
                case DataConsent.DataConsentStatus.FailedToRemoveAccount:
                    this.AddModuleMessage("FailedToRemoveAccount", ModuleMessage.ModuleMessageType.RedError, true);
                    break;
            }
        }

        /// <summary>
        /// ProfileUpdated runs when the profile is updated.
        /// </summary>
        protected void ProfileUpdated(object sender, EventArgs e)
        {
            // Authorize User
            this.ValidateUser(this.ctlProfile.User, true);
        }

        /// <summary>
        /// UserAuthenticated runs when the user is authenticated by one of the child
        /// Authentication controls.
        /// </summary>
        protected void UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            this.LoginStatus = e.LoginStatus;

            // Check the Login Status
            switch (this.LoginStatus)
            {
                case UserLoginStatus.LOGIN_USERNOTAPPROVED:
                    switch (e.Message)
                    {
                        case "UnverifiedUser":
                            if (e.User != null)
                            {
                                // First update the profile (if any properties have been passed)
                                this.AuthenticationType = e.AuthenticationType;
                                this.ProfileProperties = e.Profile;
                                this.RememberMe = e.RememberMe;
                                this.UpdateProfile(e.User, true);
                                this.ValidateUser(e.User, false);
                            }

                            break;
                        case "EnterCode":
                            this.AddModuleMessage(e.Message, ModuleMessage.ModuleMessageType.YellowWarning, true);
                            break;
                        case "InvalidCode":
                        case "UserNotAuthorized":
                            this.AddModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
                            break;
                        default:
                            this.AddLocalizedModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
                            break;
                    }

                    break;
                case UserLoginStatus.LOGIN_USERLOCKEDOUT:
                    if (Host.AutoAccountUnlockDuration > 0)
                    {
                        this.AddLocalizedModuleMessage(string.Format(Localization.GetString("UserLockedOut", this.LocalResourceFile), Host.AutoAccountUnlockDuration), ModuleMessage.ModuleMessageType.RedError, true);
                    }
                    else
                    {
                        this.AddLocalizedModuleMessage(Localization.GetString("UserLockedOut_ContactAdmin", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError, true);
                    }

                    // notify administrator about account lockout ( possible hack attempt )
                    var Custom = new ArrayList { e.UserToken };

                    var message = new Message
                    {
                        FromUserID = this.PortalSettings.AdministratorId,
                        ToUserID = this.PortalSettings.AdministratorId,
                        Subject = Localization.GetSystemMessage(this.PortalSettings, "EMAIL_USER_LOCKOUT_SUBJECT", Localization.GlobalResourceFile, Custom),
                        Body = Localization.GetSystemMessage(this.PortalSettings, "EMAIL_USER_LOCKOUT_BODY", Localization.GlobalResourceFile, Custom),
                        Status = MessageStatusType.Unread,
                    };

                    // _messagingController.SaveMessage(_message);
                    Mail.SendEmail(this.PortalSettings.Email, this.PortalSettings.Email, message.Subject, message.Body);
                    break;
                case UserLoginStatus.LOGIN_FAILURE:
                    // A Login Failure can mean one of two things:
                    //  1 - User was authenticated by the Authentication System but is not "affiliated" with a DNN Account
                    //  2 - User was not authenticated
                    if (e.Authenticated)
                    {
                        this.AutoRegister = e.AutoRegister;
                        this.AuthenticationType = e.AuthenticationType;
                        this.ProfileProperties = e.Profile;
                        this.UserToken = e.UserToken;
                        this.UserName = e.UserName;
                        if (this.AutoRegister)
                        {
                            this.InitialiseUser();
                            this.User.Membership.Password = UserController.GeneratePassword();

                            this.ctlUser.User = this.User;

                            // Call the Create User method of the User control so that it can create
                            // the user and raise the appropriate event(s)
                            this.ctlUser.CreateUser();
                        }
                        else
                        {
                            this.PageNo = 1;
                            this.ShowPanel();
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(e.Message))
                        {
                            this.AddModuleMessage("LoginFailed", ModuleMessage.ModuleMessageType.RedError, true);
                        }
                        else
                        {
                            this.AddLocalizedModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
                        }
                    }

                    break;
                default:
                    if (e.User != null)
                    {
                        // First update the profile (if any properties have been passed)
                        this.AuthenticationType = e.AuthenticationType;
                        this.ProfileProperties = e.Profile;
                        this.RememberMe = e.RememberMe;
                        this.UpdateProfile(e.User, true);
                        this.ValidateUser(e.User, e.AuthenticationType != "DNN");
                    }

                    break;
            }
        }

        /// <summary>
        /// UserCreateCompleted runs when a new user has been Created.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void UserCreateCompleted(object sender, UserUserControlBase.UserCreatedEventArgs e)
        {
            var strMessage = string.Empty;
            try
            {
                if (e.CreateStatus == UserCreateStatus.Success)
                {
                    // Assocate alternate Login with User and proceed with Login
                    AuthenticationController.AddUserAuthentication(e.NewUser.UserID, this.AuthenticationType, this.UserToken);

                    strMessage = this.CompleteUserCreation(e.CreateStatus, e.NewUser, e.Notify, true);
                    if (string.IsNullOrEmpty(strMessage))
                    {
                        // First update the profile (if any properties have been passed)
                        this.UpdateProfile(e.NewUser, true);

                        this.ValidateUser(e.NewUser, true);
                    }
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

        /// <summary>
        /// Replaces the original language with user language.
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="originalLanguage"></param>
        /// <param name="newLanguage"></param>
        /// <returns></returns>
        private static string ReplaceLanguage(string Url, string originalLanguage, string newLanguage)
        {
            var returnValue = Host.UseFriendlyUrls
                ? Regex.Replace(Url, "(.*)(/" + originalLanguage + "/)(.*)", "$1/" + newLanguage + "/$3", RegexOptions.IgnoreCase)
                : UserLanguageRegex.Replace(Url, "$1$2$3" + newLanguage + "$5");
            return returnValue;
        }

        private bool IsRedirectingFromLoginUrl()
        {
            return this.Request.UrlReferrer != null &&
                this.Request.UrlReferrer.LocalPath.ToLowerInvariant().EndsWith(LOGIN_PATH);
        }

        private void AddLoginControlAttributes(AuthenticationLoginBase loginControl)
        {
            // search selected authentication control for username and password fields
            // and inject autocomplete=off so browsers do not remember sensitive details
            var username = loginControl.FindControl("txtUsername") as WebControl;
            if (username != null)
            {
                username.Attributes.Add("AUTOCOMPLETE", "off");
            }

            var password = loginControl.FindControl("txtPassword") as WebControl;
            if (password != null)
            {
                password.Attributes.Add("AUTOCOMPLETE", "off");
            }
        }

        private void BindLogin()
        {
            List<AuthenticationInfo> authSystems = AuthenticationController.GetEnabledAuthenticationServices();
            AuthenticationLoginBase defaultLoginControl = null;
            var defaultAuthProvider = PortalController.GetPortalSetting("DefaultAuthProvider", this.PortalId, "DNN");
            foreach (AuthenticationInfo authSystem in authSystems)
            {
                try
                {
                    // Figure out if known Auth types are enabled (so we can improve perf and stop loading the control)
                    bool enabled = true;
                    if (authSystem.AuthenticationType.Equals("Facebook") || authSystem.AuthenticationType.Equals("Google")
                        || authSystem.AuthenticationType.Equals("Live") || authSystem.AuthenticationType.Equals("Twitter"))
                    {
                        enabled = AuthenticationController.IsEnabledForPortal(authSystem, this.PortalSettings.PortalId);
                    }

                    if (enabled)
                    {
                        var authLoginControl = (AuthenticationLoginBase)this.LoadControl("~/" + authSystem.LoginControlSrc);
                        this.BindLoginControl(authLoginControl, authSystem);
                        if (authSystem.AuthenticationType == "DNN")
                        {
                            defaultLoginControl = authLoginControl;
                        }

                        // Check if AuthSystem is Enabled
                        if (authLoginControl.Enabled)
                        {
                            var oAuthLoginControl = authLoginControl as OAuthLoginBase;
                            if (oAuthLoginControl != null)
                            {
                                // Add Login Control to List
                                this._oAuthControls.Add(oAuthLoginControl);
                            }
                            else
                            {
                                if (authLoginControl.AuthenticationType == defaultAuthProvider)
                                {
                                    this._defaultauthLogin.Add(authLoginControl);
                                }
                                else
                                {
                                    // Add Login Control to List
                                    this._loginControls.Add(authLoginControl);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }

            int authCount = this._loginControls.Count + this._defaultauthLogin.Count;
            switch (authCount)
            {
                case 0:
                    // No enabled controls - inject default dnn control
                    if (defaultLoginControl == null)
                    {
                        // No controls enabled for portal, and default DNN control is not enabled by host, so load system default (DNN)
                        AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByType("DNN");
                        var authLoginControl = (AuthenticationLoginBase)this.LoadControl("~/" + authSystem.LoginControlSrc);
                        this.BindLoginControl(authLoginControl, authSystem);
                        this.DisplayLoginControl(authLoginControl, false, false);
                    }
                    else
                    {
                        // if there are social authprovider only
                        if (this._oAuthControls.Count == 0)
                        {
                            // Portal has no login controls enabled so load default DNN control
                            this.DisplayLoginControl(defaultLoginControl, false, false);
                        }
                    }

                    break;
                case 1:
                    // We don't want the control to render with tabbed interface
                    this.DisplayLoginControl(
                        this._defaultauthLogin.Count == 1
                                            ? this._defaultauthLogin[0]
                                            : this._loginControls.Count == 1
                                                ? this._loginControls[0]
                                                : this._oAuthControls[0],
                        false,
                        false);
                    break;
                default:
                    // make sure defaultAuth provider control is diplayed first
                    if (this._defaultauthLogin.Count > 0)
                    {
                        this.DisplayTabbedLoginControl(this._defaultauthLogin[0], this.tsLogin.Tabs);
                    }

                    foreach (AuthenticationLoginBase authLoginControl in this._loginControls)
                    {
                        this.DisplayTabbedLoginControl(authLoginControl, this.tsLogin.Tabs);
                    }

                    break;
            }

            this.BindOAuthControls();
        }

        private void BindOAuthControls()
        {
            foreach (OAuthLoginBase oAuthLoginControl in this._oAuthControls)
            {
                this.socialLoginControls.Controls.Add(oAuthLoginControl);
            }
        }

        private void BindLoginControl(AuthenticationLoginBase authLoginControl, AuthenticationInfo authSystem)
        {
            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            authLoginControl.AuthenticationType = authSystem.AuthenticationType;
            authLoginControl.ID = Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc) + "_" + authSystem.AuthenticationType;
            authLoginControl.LocalResourceFile = authLoginControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                 Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc);
            authLoginControl.RedirectURL = this.RedirectURL;
            authLoginControl.ModuleConfiguration = this.ModuleConfiguration;
            if (authSystem.AuthenticationType != "DNN")
            {
                authLoginControl.ViewStateMode = ViewStateMode.Enabled;
            }

            // attempt to inject control attributes
            this.AddLoginControlAttributes(authLoginControl);
            authLoginControl.UserAuthenticated += this.UserAuthenticated;
        }

        private void BindRegister()
        {
            this.lblType.Text = this.AuthenticationType;
            this.lblToken.Text = this.UserToken;

            // Verify that the current user has access to this page
            if (this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration && this.Request.IsAuthenticated == false)
            {
                this.Response.Redirect(this._navigationManager.NavigateURL("Access Denied"), true);
            }

            this.lblRegisterHelp.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_REGISTRATION_INSTRUCTIONS");
            switch (this.PortalSettings.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    this.lblRegisterHelp.Text += Localization.GetString("PrivateMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    this.lblRegisterHelp.Text += Localization.GetString("PublicMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    this.lblRegisterHelp.Text += Localization.GetString("VerifiedMembership", Localization.SharedResourceFile);
                    break;
            }

            if (this.AutoRegister)
            {
                this.InitialiseUser();
            }

            bool UserValid = true;
            if (string.IsNullOrEmpty(this.User.Username) || string.IsNullOrEmpty(this.User.Email) || string.IsNullOrEmpty(this.User.FirstName) || string.IsNullOrEmpty(this.User.LastName))
            {
                UserValid = Null.NullBoolean;
            }

            if (this.AutoRegister && UserValid)
            {
                this.ctlUser.Visible = false;
                this.lblRegisterTitle.Text = Localization.GetString("CreateTitle", this.LocalResourceFile);
                this.cmdCreateUser.Text = Localization.GetString("cmdCreate", this.LocalResourceFile);
            }
            else
            {
                this.lblRegisterHelp.Text += Localization.GetString("Required", Localization.SharedResourceFile);
                this.lblRegisterTitle.Text = Localization.GetString("RegisterTitle", this.LocalResourceFile);
                this.cmdCreateUser.Text = Localization.GetString("cmdRegister", this.LocalResourceFile);
                this.ctlUser.ShowPassword = false;
                this.ctlUser.ShowUpdate = false;
                this.ctlUser.User = this.User;
                this.ctlUser.DataBind();
            }
        }

        private void DisplayLoginControl(AuthenticationLoginBase authLoginControl, bool addHeader, bool addFooter)
        {
            // Create a <div> to hold the control
            var container = new HtmlGenericControl { TagName = "div", ID = authLoginControl.AuthenticationType, ViewStateMode = ViewStateMode.Disabled };

            // Add Settings Control to Container
            container.Controls.Add(authLoginControl);

            // Add a Section Header
            SectionHeadControl sectionHeadControl;
            if (addHeader)
            {
                sectionHeadControl = (SectionHeadControl)this.LoadControl("~/controls/SectionHeadControl.ascx");
                sectionHeadControl.IncludeRule = true;
                sectionHeadControl.CssClass = "Head";
                sectionHeadControl.Text = Localization.GetString("Title", authLoginControl.LocalResourceFile);

                sectionHeadControl.Section = container.ID;

                // Add Section Head Control to Container
                this.pnlLoginContainer.Controls.Add(sectionHeadControl);
            }

            // Add Container to Controls
            this.pnlLoginContainer.Controls.Add(container);

            // Add LineBreak
            if (addFooter)
            {
                this.pnlLoginContainer.Controls.Add(new LiteralControl("<br />"));
            }

            // Display the container
            this.pnlLoginContainer.Visible = true;
        }

        private void DisplayTabbedLoginControl(AuthenticationLoginBase authLoginControl, TabStripTabCollection Tabs)
        {
            var tab = new DNNTab(Localization.GetString("Title", authLoginControl.LocalResourceFile)) { ID = authLoginControl.AuthenticationType };

            tab.Controls.Add(authLoginControl);
            Tabs.Add(tab);

            this.tsLogin.Visible = true;
        }

        private void InitialiseUser()
        {
            // Load any Profile properties that may have been returned
            this.UpdateProfile(this.User, false);

            // Set UserName to authentication Token
            this.User.Username = this.GenerateUserName();

            // Set DisplayName to UserToken if null
            if (string.IsNullOrEmpty(this.User.DisplayName))
            {
                this.User.DisplayName = this.UserToken.Replace("http://", string.Empty).TrimEnd('/');
            }

            // Parse DisplayName into FirstName/LastName
            if (this.User.DisplayName.IndexOf(' ') > 0)
            {
                this.User.FirstName = this.User.DisplayName.Substring(0, this.User.DisplayName.IndexOf(' '));
                this.User.LastName = this.User.DisplayName.Substring(this.User.DisplayName.IndexOf(' ') + 1);
            }

            // Set FirstName to Authentication Type (if null)
            if (string.IsNullOrEmpty(this.User.FirstName))
            {
                this.User.FirstName = this.AuthenticationType;
            }

            // Set FirstName to "User" (if null)
            if (string.IsNullOrEmpty(this.User.LastName))
            {
                this.User.LastName = "User";
            }
        }

        private string GenerateUserName()
        {
            if (!string.IsNullOrEmpty(this.UserName))
            {
                return this.UserName;
            }

            // Try Email prefix
            var emailPrefix = string.Empty;
            if (!string.IsNullOrEmpty(this.User.Email))
            {
                if (this.User.Email.IndexOf("@", StringComparison.Ordinal) != -1)
                {
                    emailPrefix = this.User.Email.Substring(0, this.User.Email.IndexOf("@", StringComparison.Ordinal));
                    var user = UserController.GetUserByName(this.PortalId, emailPrefix);
                    if (user == null)
                    {
                        return emailPrefix;
                    }
                }
            }

            // Try First Name
            if (!string.IsNullOrEmpty(this.User.FirstName))
            {
                var user = UserController.GetUserByName(this.PortalId, this.User.FirstName);
                if (user == null)
                {
                    return this.User.FirstName;
                }
            }

            // Try Last Name
            if (!string.IsNullOrEmpty(this.User.LastName))
            {
                var user = UserController.GetUserByName(this.PortalId, this.User.LastName);
                if (user == null)
                {
                    return this.User.LastName;
                }
            }

            // Try First Name + space + First letter last name
            if (!string.IsNullOrEmpty(this.User.LastName) && !string.IsNullOrEmpty(this.User.FirstName))
            {
                var newUserName = this.User.FirstName + " " + this.User.LastName.Substring(0, 1);
                var user = UserController.GetUserByName(this.PortalId, newUserName);
                if (user == null)
                {
                    return newUserName;
                }
            }

            // Try First letter of First Name + lastname
            if (!string.IsNullOrEmpty(this.User.LastName) && !string.IsNullOrEmpty(this.User.FirstName))
            {
                var newUserName = this.User.FirstName.Substring(0, 1) + this.User.LastName;
                var user = UserController.GetUserByName(this.PortalId, newUserName);
                if (user == null)
                {
                    return newUserName;
                }
            }

            // Try Email Prefix + incremental numbers until unique name found
            if (!string.IsNullOrEmpty(emailPrefix))
            {
                for (var i = 1; i < 10000; i++)
                {
                    var newUserName = emailPrefix + i;
                    var user = UserController.GetUserByName(this.PortalId, newUserName);
                    if (user == null)
                    {
                        return newUserName;
                    }
                }
            }

            return this.UserToken.Replace("http://", string.Empty).TrimEnd('/');
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ShowPanel controls what "panel" is to be displayed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void ShowPanel()
        {
            bool showLogin = this.PageNo == 0;
            bool showRegister = this.PageNo == 1;
            bool showPassword = this.PageNo == 2;
            bool showProfile = this.PageNo == 3;
            bool showDataConsent = this.PageNo == 4;
            this.pnlProfile.Visible = showProfile;
            this.pnlPassword.Visible = showPassword;
            this.pnlLogin.Visible = showLogin;
            this.pnlRegister.Visible = showRegister;
            this.pnlAssociate.Visible = showRegister;
            this.pnlDataConsent.Visible = showDataConsent;
            switch (this.PageNo)
            {
                case 0:
                    this.BindLogin();
                    break;
                case 1:
                    this.BindRegister();
                    break;
                case 2:
                    this.ctlPassword.UserId = this.UserId;
                    this.ctlPassword.DataBind();
                    break;
                case 3:
                    this.ctlProfile.UserId = this.UserId;
                    this.ctlProfile.DataBind();
                    break;
                case 4:
                    this.ctlDataConsent.UserId = this.UserId;
                    this.ctlDataConsent.DataBind();
                    break;
            }

            if (showProfile && UrlUtils.InPopUp())
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ResizePopup", "if(parent.$('#iPopUp').length > 0 && parent.$('#iPopUp').dialog('isOpen')){parent.$('#iPopUp').dialog({width: 950, height: 550}).dialog({position: 'center'});};", true);
            }
        }

        private void UpdateProfile(UserInfo objUser, bool update)
        {
            bool bUpdateUser = false;
            if (this.ProfileProperties.Count > 0)
            {
                foreach (string key in this.ProfileProperties)
                {
                    switch (key)
                    {
                        case "FirstName":
                            if (objUser.FirstName != this.ProfileProperties[key])
                            {
                                objUser.FirstName = this.ProfileProperties[key];
                                bUpdateUser = true;
                            }

                            break;
                        case "LastName":
                            if (objUser.LastName != this.ProfileProperties[key])
                            {
                                objUser.LastName = this.ProfileProperties[key];
                                bUpdateUser = true;
                            }

                            break;
                        case "Email":
                            if (objUser.Email != this.ProfileProperties[key])
                            {
                                objUser.Email = this.ProfileProperties[key];
                                bUpdateUser = true;
                            }

                            break;
                        case "DisplayName":
                            if (objUser.DisplayName != this.ProfileProperties[key])
                            {
                                objUser.DisplayName = this.ProfileProperties[key];
                                bUpdateUser = true;
                            }

                            break;
                        default:
                            objUser.Profile.SetProfileProperty(key, this.ProfileProperties[key]);
                            break;
                    }
                }

                if (update)
                {
                    if (bUpdateUser)
                    {
                        UserController.UpdateUser(this.PortalId, objUser);
                    }

                    ProfileController.UpdateUserProfile(objUser);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ValidateUser runs when the user has been authorized by the data store.  It validates for
        /// things such as an expiring password, valid profile, or missing DNN User Association.
        /// </summary>
        /// <param name="objUser">The logged in User.</param>
        /// <param name="ignoreExpiring">Ignore the situation where the password is expiring (but not yet expired).</param>
        /// -----------------------------------------------------------------------------
        private void ValidateUser(UserInfo objUser, bool ignoreExpiring)
        {
            UserValidStatus validStatus = UserValidStatus.VALID;
            string strMessage = Null.NullString;
            DateTime expiryDate = Null.NullDate;
            bool okToShowPanel = true;

            validStatus = UserController.ValidateUser(objUser, this.PortalId, ignoreExpiring);

            if (PasswordConfig.PasswordExpiry > 0)
            {
                expiryDate = objUser.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry);
            }

            this.UserId = objUser.UserID;

            // Check if the User has valid Password/Profile
            switch (validStatus)
            {
                case UserValidStatus.VALID:
                    // check if the user is an admin/host and validate their IP
                    if (Host.EnableIPChecking)
                    {
                        bool isAdminUser = objUser.IsSuperUser || objUser.IsInRole(this.PortalSettings.AdministratorRoleName);
                        if (isAdminUser)
                        {
                            var clientIp = NetworkUtils.GetClientIpAddress(this.Request);
                            if (IPFilterController.Instance.IsIPBanned(clientIp))
                            {
                                PortalSecurity.Instance.SignOut();
                                this.AddModuleMessage("IPAddressBanned", ModuleMessage.ModuleMessageType.RedError, true);
                                okToShowPanel = false;
                                break;
                            }
                        }
                    }

                    // Set the Page Culture(Language) based on the Users Preferred Locale
                    if ((objUser.Profile != null) && (objUser.Profile.PreferredLocale != null) && this.LocaleEnabled(objUser.Profile.PreferredLocale))
                    {
                        Localization.SetLanguage(objUser.Profile.PreferredLocale);
                    }
                    else
                    {
                        Localization.SetLanguage(this.PortalSettings.DefaultLanguage);
                    }

                    // Set the Authentication Type used
                    AuthenticationController.SetAuthenticationType(this.AuthenticationType);

                    // Complete Login
                    var userRequestIpAddressController = UserRequestIPAddressController.Instance;
                    var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));
                    UserController.UserLogin(this.PortalId, objUser, this.PortalSettings.PortalName, ipAddress, this.RememberMe);

                    // check whether user request comes with IPv6 and log it to make sure admin is aware of that
                    if (string.IsNullOrWhiteSpace(ipAddress))
                    {
                        var ipAddressV6 = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request), IPAddressFamily.IPv6);

                        if (!string.IsNullOrWhiteSpace(ipAddressV6))
                        {
                            this.AddEventLog(objUser.UserID, objUser.Username, this.PortalId, "IPv6", ipAddressV6);
                        }
                    }

                    // redirect browser
                    var redirectUrl = this.RedirectURL;

                    // Clear the cookie
                    HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", string.Empty)
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                    });

                    this.Response.Redirect(redirectUrl, true);
                    break;
                case UserValidStatus.PASSWORDEXPIRED:
                    strMessage = string.Format(Localization.GetString("PasswordExpired", this.LocalResourceFile), expiryDate.ToLongDateString());
                    this.AddLocalizedModuleMessage(strMessage, ModuleMessage.ModuleMessageType.YellowWarning, true);
                    this.PageNo = 2;
                    this.pnlProceed.Visible = false;
                    break;
                case UserValidStatus.PASSWORDEXPIRING:
                    strMessage = string.Format(Localization.GetString("PasswordExpiring", this.LocalResourceFile), expiryDate.ToLongDateString());
                    this.AddLocalizedModuleMessage(strMessage, ModuleMessage.ModuleMessageType.YellowWarning, true);
                    this.PageNo = 2;
                    this.pnlProceed.Visible = true;
                    break;
                case UserValidStatus.UPDATEPASSWORD:
                    var portalAlias = Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias);
                    if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                    {
                        UserController.ResetPasswordToken(this.User);
                        objUser = UserController.GetUserById(objUser.PortalID, objUser.UserID);
                    }

                    var redirTo = string.Format("{0}/default.aspx?ctl=PasswordReset&resetToken={1}&forced=true", portalAlias, objUser.PasswordResetToken);
                    this.Response.Redirect(redirTo);
                    break;
                case UserValidStatus.UPDATEPROFILE:
                    // Save UserID in ViewState so that can update profile later.
                    this.UserId = objUser.UserID;

                    // When the user need update its profile to complete login, we need clear the login status because if the logrin is from
                    // 3rd party login provider, it may call UserController.UserLogin because they doesn't check this situation.
                    PortalSecurity.Instance.SignOut();

                    // Admin has forced profile update
                    this.AddModuleMessage("ProfileUpdate", ModuleMessage.ModuleMessageType.YellowWarning, true);
                    this.PageNo = 3;
                    break;
                case UserValidStatus.MUSTAGREETOTERMS:
                    if (this.PortalSettings.DataConsentConsentRedirect == -1)
                    {
                        this.UserId = objUser.UserID;
                        this.AddModuleMessage("MustConsent", ModuleMessage.ModuleMessageType.YellowWarning, true);
                        this.PageNo = 4;
                    }
                    else
                    {
                        // Use the reset password token to identify the user during the redirect
                        UserController.ResetPasswordToken(objUser);
                        objUser = UserController.GetUserById(objUser.PortalID, objUser.UserID);
                        this.Response.Redirect(this._navigationManager.NavigateURL(this.PortalSettings.DataConsentConsentRedirect, string.Empty, string.Format("token={0}", objUser.PasswordResetToken)));
                    }

                    break;
            }

            if (okToShowPanel)
            {
                this.ShowPanel();
            }
        }

        private bool UserNeedsVerification()
        {
            var userInfo = UserController.Instance.GetCurrentUserInfo();

            return !userInfo.IsSuperUser && userInfo.IsInRole("Unverified Users") &&
                this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration &&
                !string.IsNullOrEmpty(this.Request.QueryString["verificationcode"]);
        }

        private bool LocaleEnabled(string locale)
        {
            return LocaleController.Instance.GetLocales(this.PortalSettings.PortalId).ContainsKey(locale);
        }

        private void AddEventLog(int userId, string username, int portalId, string propertyName, string propertyValue)
        {
            var log = new LogInfo
            {
                LogUserID = userId,
                LogUserName = username,
                LogPortalID = portalId,
                LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString(),
            };
            log.AddProperty(propertyName, propertyValue);
            LogController.Instance.AddLog(log);
        }
    }
}
