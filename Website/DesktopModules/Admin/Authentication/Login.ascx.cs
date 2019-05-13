#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
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
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Messaging.Data;
using DotNetNuke.Services.UserRequest;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.UserControls;
using DotNetNuke.UI.WebControls;
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

#endregion

namespace DotNetNuke.Modules.Admin.Authentication
{
    using Host = DotNetNuke.Entities.Host.Host;

    /// <summary>
    /// The Signin UserModuleBase is used to provide a login for a registered user
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Login : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Login));

        private static readonly Regex UserLanguageRegex = new Regex("(.*)(&|\\?)(language=)([^&\\?]+)(.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Private Members

        private readonly List<AuthenticationLoginBase> _loginControls = new List<AuthenticationLoginBase>();
        private readonly List<AuthenticationLoginBase> _defaultauthLogin = new List<AuthenticationLoginBase>();
        private readonly List<OAuthLoginBase> _oAuthControls = new List<OAuthLoginBase>();
        private const string LOGIN_PATH = "/login";

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets and sets the current AuthenticationType
        /// </summary>
        protected string AuthenticationType
        {
            get
            {
                var authenticationType = Null.NullString;
                if (ViewState["AuthenticationType"] != null)
                {
                    authenticationType = Convert.ToString(ViewState["AuthenticationType"]);
                }
                return authenticationType;
            }
            set
            {
                ViewState["AuthenticationType"] = value;
            }
        }

        /// <summary>
        /// Gets and sets a flag that determines whether the user should be automatically registered
        /// </summary>
        protected bool AutoRegister
        {
            get
            {
                var autoRegister = Null.NullBoolean;
                if (ViewState["AutoRegister"] != null)
                {
                    autoRegister = Convert.ToBoolean(ViewState["AutoRegister"]);
                }
                return autoRegister;
            }
            set
            {
                ViewState["AutoRegister"] = value;
            }
        }

        protected NameValueCollection ProfileProperties
        {
            get
            {
                var profile = new NameValueCollection();
                if (ViewState["ProfileProperties"] != null)
                {
                    profile = (NameValueCollection)ViewState["ProfileProperties"];
                }
                return profile;
            }
            set
            {
                ViewState["ProfileProperties"] = value;
            }
        }

        /// <summary>
        /// Gets and sets the current Page No
        /// </summary>
        protected int PageNo
        {
            get
            {
                var pageNo = 0;
                if (ViewState["PageNo"] != null)
                {
                    pageNo = Convert.ToInt32(ViewState["PageNo"]);
                }
                return pageNo;
            }
            set
            {
                ViewState["PageNo"] = value;
            }
        }

        /// <summary>
        /// Gets the Redirect URL (after successful login)
        /// </summary>
        protected string RedirectURL
        {
            get
            {
                var redirectURL = "";

                var setting = GetSetting(PortalId, "Redirect_AfterLogin");

                //first we need to check if there is a returnurl
                if (Request.QueryString["returnurl"] != null)
                {
                    //return to the url passed to signin
                    redirectURL = HttpUtility.UrlDecode(Request.QueryString["returnurl"]);

                    //clean the return url to avoid possible XSS attack.
                    redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                }
                if (Request.Cookies["returnurl"] != null)
                {
                    //return to the url passed to signin
                    redirectURL = HttpUtility.UrlDecode(Request.Cookies["returnurl"].Value);

                    //clean the return url to avoid possible XSS attack.
                    redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                }
                if (Request.Params["appctx"] != null)
                {
                    //HACK return to the url passed to signin (LiveID) 
                    redirectURL = HttpUtility.UrlDecode(Request.Params["appctx"]);

                    //clean the return url to avoid possible XSS attack.
                    redirectURL = UrlUtils.ValidReturnUrl(redirectURL);
                }

                var alias = PortalAlias.HTTPAlias;
                var comparison = StringComparison.InvariantCultureIgnoreCase;
                // we need .TrimEnd('/') because a portlalias for a specific culture will not have a trailing /, while a returnurl will.
                var isDefaultPage = redirectURL == "/"
                    || (alias.Contains("/") && redirectURL.TrimEnd('/').Equals(alias.Substring(alias.IndexOf("/", comparison)), comparison));

                if (string.IsNullOrEmpty(redirectURL) || isDefaultPage)
                {
                    if (
                        NeedRedirectAfterLogin
                        && (isDefaultPage || IsRedirectingFromLoginUrl())
                        && Convert.ToInt32(setting) != Null.NullInteger
                        )
                    {
                        redirectURL = Globals.NavigateURL(Convert.ToInt32(setting));
                    }
                    else
                    {
                        if (PortalSettings.LoginTabId != -1 && PortalSettings.HomeTabId != -1)
                        {
                            //redirect to portal home page specified
                            redirectURL = Globals.NavigateURL(PortalSettings.HomeTabId);
                        }
                        else
                        {
                            //redirect to current page 
                            redirectURL = Globals.NavigateURL();
                        }
                    }

                }


                //replace language parameter in querystring, to make sure that user will see page in correct language
                if (UserId != -1 && User != null)
                {
                    if (!String.IsNullOrEmpty(User.Profile.PreferredLocale)
                            && User.Profile.PreferredLocale != CultureInfo.CurrentCulture.Name
                            && LocaleEnabled(User.Profile.PreferredLocale))
                    {
                        redirectURL = ReplaceLanguage(redirectURL, CultureInfo.CurrentCulture.Name, User.Profile.PreferredLocale);
                    }
                }

                //check for insecure account defaults
                var qsDelimiter = "?";
                if (redirectURL.Contains("?"))
                {
                    qsDelimiter = "&";
                }
                if (LoginStatus == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD)
                {
                    redirectURL = redirectURL + qsDelimiter + "runningDefault=1";
                }
                else if (LoginStatus == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD)
                {
                    redirectURL = redirectURL + qsDelimiter + "runningDefault=2";
                }
                return redirectURL;
            }
        }

        private bool IsRedirectingFromLoginUrl()
        {
            return Request.UrlReferrer != null &&
                Request.UrlReferrer.LocalPath.ToLowerInvariant().EndsWith(LOGIN_PATH);
        }

        private bool NeedRedirectAfterLogin =>
               LoginStatus == UserLoginStatus.LOGIN_SUCCESS
            || LoginStatus == UserLoginStatus.LOGIN_SUPERUSER
            || LoginStatus == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD
            || LoginStatus == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD;

        /// <summary>
        /// Replaces the original language with user language
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


        /// <summary>
        /// Gets and sets a flag that determines whether a permanent auth cookie should be created
        /// </summary>
        protected bool RememberMe
        {
            get
            {
                var rememberMe = Null.NullBoolean;
                if (ViewState["RememberMe"] != null)
                {
                    rememberMe = Convert.ToBoolean(ViewState["RememberMe"]);
                }
                return rememberMe;
            }
            set
            {
                ViewState["RememberMe"] = value;
            }
        }

        /// <summary>
        /// Gets whether the Captcha control is used to validate the login
        /// </summary>
        protected bool UseCaptcha
        {
            get
            {
                object setting = GetSetting(PortalId, "Security_CaptchaLogin");
                return Convert.ToBoolean(setting);
            }
        }

        protected UserLoginStatus LoginStatus
        {
            get
            {
                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                if (ViewState["LoginStatus"] != null)
                {
                    loginStatus = (UserLoginStatus)ViewState["LoginStatus"];
                }
                return loginStatus;
            }
            set
            {
                ViewState["LoginStatus"] = value;
            }
        }

        /// <summary>
        /// Gets and sets the current UserToken
        /// </summary>
        protected string UserToken
        {
            get
            {
                var userToken = "";
                if (ViewState["UserToken"] != null)
                {
                    userToken = Convert.ToString(ViewState["UserToken"]);
                }
                return userToken;
            }
            set
            {
                ViewState["UserToken"] = value;
            }
        }

        /// <summary>
        /// Gets and sets the current UserName
        /// </summary>
        protected string UserName
        {
            get
            {
                var userName = "";
                if (ViewState["UserName"] != null)
                {
                    userName = Convert.ToString(ViewState["UserName"]);
                }
                return userName;
            }
            set
            {
                ViewState["UserName"] = value;
            }
        }

        #endregion

        #region Private Methods

        private void AddLoginControlAttributes(AuthenticationLoginBase loginControl)
        {
            //search selected authentication control for username and password fields
            //and inject autocomplete=off so browsers do not remember sensitive details
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
            var defaultAuthProvider = PortalController.GetPortalSetting("DefaultAuthProvider", PortalId, "DNN");
            foreach (AuthenticationInfo authSystem in authSystems)
            {
                try
                {
                    //Figure out if known Auth types are enabled (so we can improve perf and stop loading the control)
                    bool enabled = true;
                    if (authSystem.AuthenticationType.Equals("Facebook") || authSystem.AuthenticationType.Equals("Google")
                        || authSystem.AuthenticationType.Equals("Live") || authSystem.AuthenticationType.Equals("Twitter"))
                    {
                        enabled = AuthenticationController.IsEnabledForPortal(authSystem, PortalSettings.PortalId);
                    }

                    if (enabled)
                    {
                        var authLoginControl = (AuthenticationLoginBase)LoadControl("~/" + authSystem.LoginControlSrc);
                        BindLoginControl(authLoginControl, authSystem);
                        if (authSystem.AuthenticationType == "DNN")
                        {
                            defaultLoginControl = authLoginControl;
                            pnlLoginContainer.Visible = true;
                        }

                        //Check if AuthSystem is Enabled
                        if (authLoginControl.Enabled)
                        {
                            var oAuthLoginControl = authLoginControl as OAuthLoginBase;
                            if (oAuthLoginControl != null)
                            {
                                //Add Login Control to List
                                _oAuthControls.Add(oAuthLoginControl);
                            }
                            else
                            {
                                if (authLoginControl.AuthenticationType == defaultAuthProvider)
                                {
                                    _defaultauthLogin.Add(authLoginControl);
                                }
                                else
                                {
                                    //Add Login Control to List
                                    _loginControls.Add(authLoginControl);
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
            int authCount = _loginControls.Count + _defaultauthLogin.Count;
            switch (authCount)
            {
                case 0:
                    //No enabled controls - inject default dnn control
                    if (defaultLoginControl == null)
                    {
                        //No controls enabled for portal, and default DNN control is not enabled by host, so load system default (DNN)
                        AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByType("DNN");
                        var authLoginControl = (AuthenticationLoginBase)LoadControl("~/" + authSystem.LoginControlSrc);
                        BindLoginControl(authLoginControl, authSystem);
                        DisplayLoginControl(authLoginControl, false, false);
                    }
                    else
                    {
                        //if there are social authprovider only
                        if (_oAuthControls.Count == 0)
                        {
                            //Portal has no login controls enabled so load default DNN control
                            DisplayLoginControl(defaultLoginControl, false, false);
                        }
                    }
                    break;
                case 1:
                    //We don't want the control to render with tabbed interface
                    DisplayLoginControl(_defaultauthLogin.Count == 1
                                            ? _defaultauthLogin[0]
                                            : _loginControls.Count == 1
                                                ? _loginControls[0]
                                                : _oAuthControls[0],
                                        false,
                                        false);
                    break;
                default:
                    //make sure defaultAuth provider control is diplayed first
                    if (_defaultauthLogin.Count > 0)
                    {
                        DisplayTabbedLoginControl(_defaultauthLogin[0], tsLogin.Tabs);
                    }

                    foreach (AuthenticationLoginBase authLoginControl in _loginControls)
                    {
                        DisplayTabbedLoginControl(authLoginControl, tsLogin.Tabs);
                    }

                    break;
            }
            BindOAuthControls();
        }

        private void BindOAuthControls()
        {
            foreach (OAuthLoginBase oAuthLoginControl in _oAuthControls)
            {
                socialLoginControls.Controls.Add(oAuthLoginControl);
            }
        }

        private void BindLoginControl(AuthenticationLoginBase authLoginControl, AuthenticationInfo authSystem)
        {
            //set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            //this is necessary for the Localization in PageBase
            authLoginControl.AuthenticationType = authSystem.AuthenticationType;
            authLoginControl.ID = Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc) + "_" + authSystem.AuthenticationType;
            authLoginControl.LocalResourceFile = authLoginControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                 Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc);
            authLoginControl.RedirectURL = RedirectURL;
            authLoginControl.ModuleConfiguration = ModuleConfiguration;
            if (authSystem.AuthenticationType != "DNN")
            {
                authLoginControl.ViewStateMode = ViewStateMode.Enabled;
            }

            //attempt to inject control attributes
            AddLoginControlAttributes(authLoginControl);
            authLoginControl.UserAuthenticated += UserAuthenticated;
        }

        private void BindRegister()
        {
            lblType.Text = AuthenticationType;
            lblToken.Text = UserToken;

            //Verify that the current user has access to this page
            if (PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration && Request.IsAuthenticated == false)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
            lblRegisterHelp.Text = Localization.GetSystemMessage(PortalSettings, "MESSAGE_REGISTRATION_INSTRUCTIONS");
            switch (PortalSettings.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    lblRegisterHelp.Text += Localization.GetString("PrivateMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    lblRegisterHelp.Text += Localization.GetString("PublicMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    lblRegisterHelp.Text += Localization.GetString("VerifiedMembership", Localization.SharedResourceFile);
                    break;
            }
            if (AutoRegister)
            {
                InitialiseUser();
            }
            bool UserValid = true;
            if (string.IsNullOrEmpty(User.Username) || string.IsNullOrEmpty(User.Email) || string.IsNullOrEmpty(User.FirstName) || string.IsNullOrEmpty(User.LastName))
            {
                UserValid = Null.NullBoolean;
            }
            if (AutoRegister && UserValid)
            {
                ctlUser.Visible = false;
                lblRegisterTitle.Text = Localization.GetString("CreateTitle", LocalResourceFile);
                cmdCreateUser.Text = Localization.GetString("cmdCreate", LocalResourceFile);
            }
            else
            {
                lblRegisterHelp.Text += Localization.GetString("Required", Localization.SharedResourceFile);
                lblRegisterTitle.Text = Localization.GetString("RegisterTitle", LocalResourceFile);
                cmdCreateUser.Text = Localization.GetString("cmdRegister", LocalResourceFile);
                ctlUser.ShowPassword = false;
                ctlUser.ShowUpdate = false;
                ctlUser.User = User;
                ctlUser.DataBind();
            }
        }

        private void DisplayLoginControl(AuthenticationLoginBase authLoginControl, bool addHeader, bool addFooter)
        {
            //Create a <div> to hold the control
            var container = new HtmlGenericControl { TagName = "div", ID = authLoginControl.AuthenticationType, ViewStateMode = ViewStateMode.Disabled };

            //Add Settings Control to Container
            container.Controls.Add(authLoginControl);

            //Add a Section Header
            SectionHeadControl sectionHeadControl;
            if (addHeader)
            {
                sectionHeadControl = (SectionHeadControl)LoadControl("~/controls/SectionHeadControl.ascx");
                sectionHeadControl.IncludeRule = true;
                sectionHeadControl.CssClass = "Head";
                sectionHeadControl.Text = Localization.GetString("Title", authLoginControl.LocalResourceFile);

                sectionHeadControl.Section = container.ID;

                //Add Section Head Control to Container
                pnlLoginContainer.Controls.Add(sectionHeadControl);
            }

            //Add Container to Controls
            pnlLoginContainer.Controls.Add(container);


            //Add LineBreak
            if (addFooter)
            {
                pnlLoginContainer.Controls.Add(new LiteralControl("<br />"));
            }
        }

        private void DisplayTabbedLoginControl(AuthenticationLoginBase authLoginControl, TabStripTabCollection Tabs)
        {
            var tab = new DNNTab(Localization.GetString("Title", authLoginControl.LocalResourceFile)) { ID = authLoginControl.AuthenticationType };

            tab.Controls.Add(authLoginControl);
            Tabs.Add(tab);

            tsLogin.Visible = true;
        }

        private void InitialiseUser()
        {
            //Load any Profile properties that may have been returned
            UpdateProfile(User, false);

            //Set UserName to authentication Token            
            User.Username = GenerateUserName();

            //Set DisplayName to UserToken if null
            if (string.IsNullOrEmpty(User.DisplayName))
            {
                User.DisplayName = UserToken.Replace("http://", "").TrimEnd('/');
            }

            //Parse DisplayName into FirstName/LastName
            if (User.DisplayName.IndexOf(' ') > 0)
            {
                User.FirstName = User.DisplayName.Substring(0, User.DisplayName.IndexOf(' '));
                User.LastName = User.DisplayName.Substring(User.DisplayName.IndexOf(' ') + 1);
            }

            //Set FirstName to Authentication Type (if null)
            if (string.IsNullOrEmpty(User.FirstName))
            {
                User.FirstName = AuthenticationType;
            }
            //Set FirstName to "User" (if null)
            if (string.IsNullOrEmpty(User.LastName))
            {
                User.LastName = "User";
            }
        }

        private string GenerateUserName()
        {
            if (!string.IsNullOrEmpty(UserName))
            {
                return UserName;
            }

            //Try Email prefix
            var emailPrefix = string.Empty;
            if (!string.IsNullOrEmpty(User.Email))
            {
                if (User.Email.IndexOf("@", StringComparison.Ordinal) != -1)
                {
                    emailPrefix = User.Email.Substring(0, User.Email.IndexOf("@", StringComparison.Ordinal));
                    var user = UserController.GetUserByName(PortalId, emailPrefix);
                    if (user == null)
                    {
                        return emailPrefix;
                    }
                }
            }

            //Try First Name
            if (!string.IsNullOrEmpty(User.FirstName))
            {
                var user = UserController.GetUserByName(PortalId, User.FirstName);
                if (user == null)
                {
                    return User.FirstName;
                }
            }

            //Try Last Name
            if (!string.IsNullOrEmpty(User.LastName))
            {
                var user = UserController.GetUserByName(PortalId, User.LastName);
                if (user == null)
                {
                    return User.LastName;
                }
            }

            //Try First Name + space + First letter last name            
            if (!string.IsNullOrEmpty(User.LastName) && !string.IsNullOrEmpty(User.FirstName))
            {
                var newUserName = User.FirstName + " " + User.LastName.Substring(0, 1);
                var user = UserController.GetUserByName(PortalId, newUserName);
                if (user == null)
                {
                    return newUserName;
                }
            }

            //Try First letter of First Name + lastname
            if (!string.IsNullOrEmpty(User.LastName) && !string.IsNullOrEmpty(User.FirstName))
            {
                var newUserName = User.FirstName.Substring(0, 1) + User.LastName;
                var user = UserController.GetUserByName(PortalId, newUserName);
                if (user == null)
                {
                    return newUserName;
                }
            }

            //Try Email Prefix + incremental numbers until unique name found
            if (!string.IsNullOrEmpty(emailPrefix))
            {
                for (var i = 1; i < 10000; i++)
                {
                    var newUserName = emailPrefix + i;
                    var user = UserController.GetUserByName(PortalId, newUserName);
                    if (user == null)
                    {
                        return newUserName;
                    }
                }
            }

            return UserToken.Replace("http://", "").TrimEnd('/');
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ShowPanel controls what "panel" is to be displayed
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void ShowPanel()
        {
            bool showLogin = (PageNo == 0);
            bool showRegister = (PageNo == 1);
            bool showPassword = (PageNo == 2);
            bool showProfile = (PageNo == 3);
            bool showDataConsent = (PageNo == 4);
            pnlProfile.Visible = showProfile;
            pnlPassword.Visible = showPassword;
            pnlLogin.Visible = showLogin;
            pnlRegister.Visible = showRegister;
            pnlAssociate.Visible = showRegister;
            pnlDataConsent.Visible = showDataConsent;
            switch (PageNo)
            {
                case 0:
                    BindLogin();
                    break;
                case 1:
                    BindRegister();
                    break;
                case 2:
                    ctlPassword.UserId = UserId;
                    ctlPassword.DataBind();
                    break;
                case 3:
                    ctlProfile.UserId = UserId;
                    ctlProfile.DataBind();
                    break;
                case 4:
                    ctlDataConsent.UserId = UserId;
                    ctlDataConsent.DataBind();
                    break;
            }

            if (showProfile && UrlUtils.InPopUp())
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(), "ResizePopup", "if(parent.$('#iPopUp').length > 0 && parent.$('#iPopUp').dialog('isOpen')){parent.$('#iPopUp').dialog({width: 950, height: 550}).dialog({position: 'center'});};", true);
            }
        }

        private void UpdateProfile(UserInfo objUser, bool update)
        {
            bool bUpdateUser = false;
            if (ProfileProperties.Count > 0)
            {
                foreach (string key in ProfileProperties)
                {
                    switch (key)
                    {
                        case "FirstName":
                            if (objUser.FirstName != ProfileProperties[key])
                            {
                                objUser.FirstName = ProfileProperties[key];
                                bUpdateUser = true;
                            }
                            break;
                        case "LastName":
                            if (objUser.LastName != ProfileProperties[key])
                            {
                                objUser.LastName = ProfileProperties[key];
                                bUpdateUser = true;
                            }
                            break;
                        case "Email":
                            if (objUser.Email != ProfileProperties[key])
                            {
                                objUser.Email = ProfileProperties[key];
                                bUpdateUser = true;
                            }
                            break;
                        case "DisplayName":
                            if (objUser.DisplayName != ProfileProperties[key])
                            {
                                objUser.DisplayName = ProfileProperties[key];
                                bUpdateUser = true;
                            }
                            break;
                        default:
                            objUser.Profile.SetProfileProperty(key, ProfileProperties[key]);
                            break;
                    }
                }
                if (update)
                {
                    if (bUpdateUser)
                    {
                        UserController.UpdateUser(PortalId, objUser);
                    }
                    ProfileController.UpdateUserProfile(objUser);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ValidateUser runs when the user has been authorized by the data store.  It validates for
        /// things such as an expiring password, valid profile, or missing DNN User Association
        /// </summary>
        /// <param name="objUser">The logged in User</param>
        /// <param name="ignoreExpiring">Ignore the situation where the password is expiring (but not yet expired)</param>
        /// -----------------------------------------------------------------------------
        private void ValidateUser(UserInfo objUser, bool ignoreExpiring)
        {
            UserValidStatus validStatus = UserValidStatus.VALID;
            string strMessage = Null.NullString;
            DateTime expiryDate = Null.NullDate;
            bool okToShowPanel = true;

            validStatus = UserController.ValidateUser(objUser, PortalId, ignoreExpiring);

            if (PasswordConfig.PasswordExpiry > 0)
            {
                expiryDate = objUser.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry);
            }
            UserId = objUser.UserID;

            //Check if the User has valid Password/Profile
            switch (validStatus)
            {
                case UserValidStatus.VALID:
                    //check if the user is an admin/host and validate their IP
                    if (Host.EnableIPChecking)
                    {
                        bool isAdminUser = objUser.IsSuperUser || objUser.IsInRole(PortalSettings.AdministratorRoleName);
                        if (isAdminUser)
                        {
                            if (IPFilterController.Instance.IsIPBanned(Request.UserHostAddress))
                            {
                                PortalSecurity.Instance.SignOut();
                                AddModuleMessage("IPAddressBanned", ModuleMessage.ModuleMessageType.RedError, true);
                                okToShowPanel = false;
                                break;
                            }
                        }
                    }

                    //Set the Page Culture(Language) based on the Users Preferred Locale
                    if ((objUser.Profile != null) && (objUser.Profile.PreferredLocale != null) && LocaleEnabled(objUser.Profile.PreferredLocale))
                    {
                        Localization.SetLanguage(objUser.Profile.PreferredLocale);
                    }
                    else
                    {
                        Localization.SetLanguage(PortalSettings.DefaultLanguage);
                    }

                    //Set the Authentication Type used 
                    AuthenticationController.SetAuthenticationType(AuthenticationType);

                    //Complete Login
                    var userRequestIpAddressController = UserRequestIPAddressController.Instance;
                    var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(Request));
                    UserController.UserLogin(PortalId, objUser, PortalSettings.PortalName, ipAddress, RememberMe);

                    //check whether user request comes with IPv6 and log it to make sure admin is aware of that
                    if (string.IsNullOrWhiteSpace(ipAddress))
                    {
                        var ipAddressV6 = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(Request), IPAddressFamily.IPv6);

                        if (!string.IsNullOrWhiteSpace(ipAddressV6))
                        {
                            AddEventLog(objUser.UserID, objUser.Username, PortalId, "IPv6", ipAddressV6);
                        }
                    }

                    //redirect browser
                    var redirectUrl = RedirectURL;

                    //Clear the cookie
                    HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", "")
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                    });

                    Response.Redirect(redirectUrl, true);
                    break;
                case UserValidStatus.PASSWORDEXPIRED:
                    strMessage = string.Format(Localization.GetString("PasswordExpired", LocalResourceFile), expiryDate.ToLongDateString());
                    AddLocalizedModuleMessage(strMessage, ModuleMessage.ModuleMessageType.YellowWarning, true);
                    PageNo = 2;
                    pnlProceed.Visible = false;
                    break;
                case UserValidStatus.PASSWORDEXPIRING:
                    strMessage = string.Format(Localization.GetString("PasswordExpiring", LocalResourceFile), expiryDate.ToLongDateString());
                    AddLocalizedModuleMessage(strMessage, ModuleMessage.ModuleMessageType.YellowWarning, true);
                    PageNo = 2;
                    pnlProceed.Visible = true;
                    break;
                case UserValidStatus.UPDATEPASSWORD:
                    var portalAlias = Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias);
                    if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                    {
                        UserController.ResetPasswordToken(User);
                        objUser = UserController.GetUserById(objUser.PortalID, objUser.UserID);
                    }
                    var redirTo = string.Format("{0}/default.aspx?ctl=PasswordReset&resetToken={1}&forced=true", portalAlias, objUser.PasswordResetToken);
                    Response.Redirect(redirTo);
                    break;
                case UserValidStatus.UPDATEPROFILE:
                    //Save UserID in ViewState so that can update profile later.
                    UserId = objUser.UserID;

                    //When the user need update its profile to complete login, we need clear the login status because if the logrin is from
                    //3rd party login provider, it may call UserController.UserLogin because they doesn't check this situation.
                    PortalSecurity.Instance.SignOut();
                    //Admin has forced profile update
                    AddModuleMessage("ProfileUpdate", ModuleMessage.ModuleMessageType.YellowWarning, true);
                    PageNo = 3;
                    break;
                case UserValidStatus.MUSTAGREETOTERMS:
                    if (PortalSettings.DataConsentConsentRedirect == -1)
                    {
                        UserId = objUser.UserID;
                        AddModuleMessage("MustConsent", ModuleMessage.ModuleMessageType.YellowWarning, true);
                        PageNo = 4;
                    }
                    else
                    {
                        // Use the reset password token to identify the user during the redirect
                        UserController.ResetPasswordToken(objUser);
                        objUser = UserController.GetUserById(objUser.PortalID, objUser.UserID);
                        Response.Redirect(Globals.NavigateURL(PortalSettings.DataConsentConsentRedirect, "", string.Format("token={0}", objUser.PasswordResetToken)));
                    }
                    break;
            }
            if (okToShowPanel)
            {
                ShowPanel();
            }
        }

        private bool UserNeedsVerification()
        {
            var userInfo = UserController.Instance.GetCurrentUserInfo();

            return !userInfo.IsSuperUser && userInfo.IsInRole("Unverified Users") &&
                PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration &&
                !string.IsNullOrEmpty(Request.QueryString["verificationcode"]);
        }

        private bool LocaleEnabled(string locale)
        {
            return LocaleController.Instance.GetLocales(PortalSettings.PortalId).ContainsKey(locale);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ctlPassword.PasswordUpdated += PasswordUpdated;
            ctlProfile.ProfileUpdated += ProfileUpdated;
            ctlUser.UserCreateCompleted += UserCreateCompleted;
            ctlDataConsent.DataConsentCompleted += DataConsentCompleted;

            //Set the User Control Properties
            ctlUser.ID = "User";

            //Set the Password Control Properties
            ctlPassword.ID = "Password";

            //Set the Profile Control Properties
            ctlProfile.ID = "Profile";

            //Set the Data Consent Control Properties
            ctlDataConsent.ID = "DataConsent";

            //Override the redirected page title if page has loaded with ctl=Login
            if (Request.QueryString["ctl"] != null)
            {
                if (Request.QueryString["ctl"].ToLowerInvariant() == "login")
                {
                    var myPage = (CDefault)Page;
                    if (myPage.PortalSettings.LoginTabId == TabId || myPage.PortalSettings.LoginTabId == -1)
                    {
                        myPage.Title = Localization.GetString("ControlTitle_login", LocalResourceFile);
                    }
                }
            }
        }

        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdAssociate.Click += cmdAssociate_Click;
            cmdCreateUser.Click += cmdCreateUser_Click;
            cmdProceed.Click += cmdProceed_Click;

            //Verify if portal has a customized login page
            if (!Null.IsNull(PortalSettings.LoginTabId) && Globals.IsAdminControl())
            {
                if (Globals.ValidateLoginTabID(PortalSettings.LoginTabId))
                {
                    //login page exists and trying to access this control directly with url param -> not allowed
                    var parameters = new string[3];
                    if (!string.IsNullOrEmpty(Request.QueryString["returnUrl"]))
                    {
                        parameters[0] = "returnUrl=" + HttpUtility.UrlEncode(Request.QueryString["returnUrl"]);
                    }
                    if (!string.IsNullOrEmpty(Request.QueryString["username"]))
                    {
                        parameters[1] = "username=" + HttpUtility.UrlEncode(Request.QueryString["username"]);
                    }
                    if (!string.IsNullOrEmpty(Request.QueryString["verificationcode"]))
                    {
                        parameters[2] = "verificationcode=" + HttpUtility.UrlEncode(Request.QueryString["verificationcode"]);
                    }
                    Response.Redirect(Globals.NavigateURL(PortalSettings.LoginTabId, "", parameters));
                }
            }
            if (Page.IsPostBack == false)
            {
                try
                {
                    PageNo = 0;
                }
                catch (Exception ex)
                {
                    //control not there 
                    Logger.Error(ex);
                }
            }
            if (!Request.IsAuthenticated || UserNeedsVerification())
            {
                ShowPanel();
            }
            else //user is already authenticated
            {
                //if a Login Page has not been specified for the portal
                if (Globals.IsAdminControl())
                {
                    //redirect browser 
                    Response.Redirect(RedirectURL, true);
                }
                else //make module container invisible if user is not a page admin
                {
                    var path = RedirectURL.Split('?')[0];
                    if (NeedRedirectAfterLogin && path != Globals.NavigateURL() && path != Globals.NavigateURL(PortalSettings.HomeTabId))
                    {
                        Response.Redirect(RedirectURL, true);
                    }

                    if (TabPermissionController.CanAdminPage())
                    {
                        ShowPanel();
                    }
                    else
                    {
                        ContainerControl.Visible = false;
                    }
                }
            }
            divCaptcha.Visible = UseCaptcha;

            if (UseCaptcha)
            {
                ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", Localization.SharedResourceFile);
                ctlCaptcha.Text = Localization.GetString("CaptchaText", Localization.SharedResourceFile);
            }

        }

        /// <summary>
        /// cmdAssociate_Click runs when the associate button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdAssociate_Click(object sender, EventArgs e)
        {
            if ((UseCaptcha && ctlCaptcha.IsValid) || (!UseCaptcha))
            {
                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                var userRequestIpAddressController = UserRequestIPAddressController.Instance;
                var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(Request));
                UserInfo objUser = UserController.ValidateUser(PortalId,
                                                               txtUsername.Text,
                                                               txtPassword.Text,
                                                               "DNN",
                                                               "",
                                                               PortalSettings.PortalName,
                                                               ipAddress,
                                                               ref loginStatus);
                if (loginStatus == UserLoginStatus.LOGIN_SUCCESS)
                {
                    //Assocate alternate Login with User and proceed with Login
                    AuthenticationController.AddUserAuthentication(objUser.UserID, AuthenticationType, UserToken);
                    if (objUser != null)
                    {
                        UpdateProfile(objUser, true);
                    }
                    ValidateUser(objUser, true);
                }
                else
                {
                    AddModuleMessage("AssociationFailed", ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
        }

        /// <summary>
        /// cmdCreateUser runs when the register (as new user) button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdCreateUser_Click(object sender, EventArgs e)
        {
            User.Membership.Password = UserController.GeneratePassword();

            if (AutoRegister)
            {
                ctlUser.User = User;

                //Call the Create User method of the User control so that it can create
                //the user and raise the appropriate event(s)
                ctlUser.CreateUser();
            }
            else
            {
                if (ctlUser.IsValid)
                {
                    //Call the Create User method of the User control so that it can create
                    //the user and raise the appropriate event(s)
                    ctlUser.CreateUser();
                }
            }
        }

        /// <summary>
        /// cmdProceed_Click runs when the Proceed Anyway button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdProceed_Click(object sender, EventArgs e)
        {
            var user = ctlPassword.User;
            ValidateUser(user, true);
        }

        /// <summary>
        /// PasswordUpdated runs when the password is updated
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void PasswordUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            PasswordUpdateStatus status = e.UpdateStatus;
            if (status == PasswordUpdateStatus.Success)
            {
                AddModuleMessage("PasswordChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                var user = ctlPassword.User;
                user.Membership.LastPasswordChangeDate = DateTime.Now;
                user.Membership.UpdatePassword = false;
                LoginStatus = user.IsSuperUser ? UserLoginStatus.LOGIN_SUPERUSER : UserLoginStatus.LOGIN_SUCCESS;
                UserLoginStatus userstatus = UserLoginStatus.LOGIN_FAILURE;
                UserController.CheckInsecurePassword(user.Username, user.Membership.Password, ref userstatus);
                LoginStatus = userstatus;
                ValidateUser(user, true);
            }
            else
            {
                AddModuleMessage(status.ToString(), ModuleMessage.ModuleMessageType.RedError, true);
            }
        }

        /// <summary>
        /// DataConsentCompleted runs after the user has gone through the data consent screen
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void DataConsentCompleted(object sender, DataConsent.DataConsentEventArgs e)
        {
            switch (e.Status)
            {
                case DataConsent.DataConsentStatus.Consented:
                    ValidateUser(ctlDataConsent.User, true);
                    break;
                case DataConsent.DataConsentStatus.Cancelled:
                case DataConsent.DataConsentStatus.RemovedAccount:
                    Response.Redirect(Globals.NavigateURL(PortalSettings.HomeTabId), true);
                    break;
                case DataConsent.DataConsentStatus.FailedToRemoveAccount:
                    AddModuleMessage("FailedToRemoveAccount", ModuleMessage.ModuleMessageType.RedError, true);
                    break;
            }
        }

        /// <summary>
        /// ProfileUpdated runs when the profile is updated
        /// </summary>
        protected void ProfileUpdated(object sender, EventArgs e)
        {
            //Authorize User
            ValidateUser(ctlProfile.User, true);
        }

        /// <summary>
        /// UserAuthenticated runs when the user is authenticated by one of the child
        /// Authentication controls
        /// </summary>
        protected void UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            LoginStatus = e.LoginStatus;

            //Check the Login Status
            switch (LoginStatus)
            {
                case UserLoginStatus.LOGIN_USERNOTAPPROVED:
                    switch (e.Message)
                    {
                        case "UnverifiedUser":
                            if (e.User != null)
                            {
                                //First update the profile (if any properties have been passed)
                                AuthenticationType = e.AuthenticationType;
                                ProfileProperties = e.Profile;
                                RememberMe = e.RememberMe;
                                UpdateProfile(e.User, true);
                                ValidateUser(e.User, false);
                            }
                            break;
                        case "EnterCode":
                            AddModuleMessage(e.Message, ModuleMessage.ModuleMessageType.YellowWarning, true);
                            break;
                        case "InvalidCode":
                        case "UserNotAuthorized":
                            AddModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
                            break;
                        default:
                            AddLocalizedModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
                            break;
                    }
                    break;
                case UserLoginStatus.LOGIN_USERLOCKEDOUT:
                    if (Host.AutoAccountUnlockDuration > 0)
                    {
                        AddLocalizedModuleMessage(string.Format(Localization.GetString("UserLockedOut", LocalResourceFile), Host.AutoAccountUnlockDuration), ModuleMessage.ModuleMessageType.RedError, true);
                    }
                    else
                    {
                        AddLocalizedModuleMessage(Localization.GetString("UserLockedOut_ContactAdmin", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError, true);
                    }
                    //notify administrator about account lockout ( possible hack attempt )
                    var Custom = new ArrayList { e.UserToken };

                    var message = new Message
                    {
                        FromUserID = PortalSettings.AdministratorId,
                        ToUserID = PortalSettings.AdministratorId,
                        Subject = Localization.GetSystemMessage(PortalSettings, "EMAIL_USER_LOCKOUT_SUBJECT", Localization.GlobalResourceFile, Custom),
                        Body = Localization.GetSystemMessage(PortalSettings, "EMAIL_USER_LOCKOUT_BODY", Localization.GlobalResourceFile, Custom),
                        Status = MessageStatusType.Unread
                    };
                    //_messagingController.SaveMessage(_message);

                    Mail.SendEmail(PortalSettings.Email, PortalSettings.Email, message.Subject, message.Body);
                    break;
                case UserLoginStatus.LOGIN_FAILURE:
                    //A Login Failure can mean one of two things:
                    //  1 - User was authenticated by the Authentication System but is not "affiliated" with a DNN Account
                    //  2 - User was not authenticated
                    if (e.Authenticated)
                    {
                        AutoRegister = e.AutoRegister;
                        AuthenticationType = e.AuthenticationType;
                        ProfileProperties = e.Profile;
                        UserToken = e.UserToken;
                        UserName = e.UserName;
                        if (AutoRegister)
                        {
                            InitialiseUser();
                            User.Membership.Password = UserController.GeneratePassword();

                            ctlUser.User = User;

                            //Call the Create User method of the User control so that it can create
                            //the user and raise the appropriate event(s)
                            ctlUser.CreateUser();
                        }
                        else
                        {
                            PageNo = 1;
                            ShowPanel();
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(e.Message))
                        {
                            AddModuleMessage("LoginFailed", ModuleMessage.ModuleMessageType.RedError, true);
                        }
                        else
                        {
                            AddLocalizedModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
                        }
                    }
                    break;
                default:
                    if (e.User != null)
                    {
                        //First update the profile (if any properties have been passed)
                        AuthenticationType = e.AuthenticationType;
                        ProfileProperties = e.Profile;
                        RememberMe = e.RememberMe;
                        UpdateProfile(e.User, true);
                        ValidateUser(e.User, (e.AuthenticationType != "DNN"));
                    }
                    break;
            }
        }

        /// <summary>
        /// UserCreateCompleted runs when a new user has been Created
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void UserCreateCompleted(object sender, UserUserControlBase.UserCreatedEventArgs e)
        {
            var strMessage = "";
            try
            {
                if (e.CreateStatus == UserCreateStatus.Success)
                {
                    //Assocate alternate Login with User and proceed with Login
                    AuthenticationController.AddUserAuthentication(e.NewUser.UserID, AuthenticationType, UserToken);

                    strMessage = CompleteUserCreation(e.CreateStatus, e.NewUser, e.Notify, true);
                    if ((string.IsNullOrEmpty(strMessage)))
                    {
                        //First update the profile (if any properties have been passed)
                        UpdateProfile(e.NewUser, true);

                        ValidateUser(e.NewUser, true);
                    }
                }
                else
                {
                    AddLocalizedModuleMessage(UserController.GetUserCreateStatus(e.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void AddEventLog(int userId, string username, int portalId, string propertyName, string propertyValue)
        {
            var log = new LogInfo
            {
                LogUserID = userId,
                LogUserName = username,
                LogPortalID = portalId,
                LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString()
            };
            log.AddProperty(propertyName, propertyValue);
            LogController.Instance.AddLog(log);
        }

        #endregion

    }
}