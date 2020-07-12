// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication.DNN
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;
    using Host = DotNetNuke.Entities.Host.Host;

    /// <summary>
    /// The Login AuthenticationLoginBase is used to provide a login for a registered user
    /// portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Login : AuthenticationLoginBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Login));
        private readonly INavigationManager _navigationManager;

        public Login()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>
        /// Gets a value indicating whether check if the Auth System is Enabled (for the Portal).
        /// </summary>
        /// <remarks></remarks>
        public override bool Enabled
        {
            get
            {
                return AuthenticationConfig.GetConfig(this.PortalId).Enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the Captcha control is used to validate the login.
        /// </summary>
        protected bool UseCaptcha
        {
            get
            {
                return AuthenticationConfig.GetConfig(this.PortalId).UseCaptcha;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdLogin.Click += this.OnLoginClick;

            this.cancelLink.NavigateUrl = this.GetRedirectUrl(false);

            if (this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration)
            {
                this.liRegister.Visible = false;
            }

            this.lblLogin.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_LOGIN_INSTRUCTIONS");
            if (string.IsNullOrEmpty(this.lblLogin.Text))
            {
                this.lblLogin.AssociatedControlID = string.Empty;
            }

            if (this.Request.QueryString["usernameChanged"] == "true")
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_USERNAME_CHANGED_INSTRUCTIONS"), ModuleMessage.ModuleMessageType.BlueInfo);
            }

            var returnUrl = this._navigationManager.NavigateURL();
            string url;
            if (this.PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
            {
                if (!string.IsNullOrEmpty(UrlUtils.ValidReturnUrl(this.Request.QueryString["returnurl"])))
                {
                    returnUrl = this.Request.QueryString["returnurl"];
                }

                returnUrl = HttpUtility.UrlEncode(returnUrl);

                url = Globals.RegisterURL(returnUrl, Null.NullString);
                this.registerLink.NavigateUrl = url;
                if (this.PortalSettings.EnablePopUps && this.PortalSettings.RegisterTabId == Null.NullInteger
                    && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                {
                    this.registerLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, this.PortalSettings, true, false, 600, 950));
                }
            }
            else
            {
                this.registerLink.Visible = false;
            }

            // see if the portal supports persistant cookies
            this.chkCookie.Visible = Host.RememberCheckbox;

            // no need to show password link if feature is disabled, let's check this first
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                url = this._navigationManager.NavigateURL("SendPassword", "returnurl=" + returnUrl);
                this.passwordLink.NavigateUrl = url;
                if (this.PortalSettings.EnablePopUps)
                {
                    this.passwordLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, this.PortalSettings, true, false, 300, 650));
                }
            }
            else
            {
                this.passwordLink.Visible = false;
            }

            if (!this.IsPostBack)
            {
                if (!string.IsNullOrEmpty(this.Request.QueryString["verificationcode"]) && this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    if (this.Request.IsAuthenticated)
                    {
                        this.Controls.Clear();
                    }

                    var verificationCode = this.Request.QueryString["verificationcode"];

                    try
                    {
                        UserController.VerifyUser(verificationCode.Replace(".", "+").Replace("-", "/").Replace("_", "="));

                        var redirectTabId = this.PortalSettings.Registration.RedirectAfterRegistration;

                        if (this.Request.IsAuthenticated)
                        {
                            this.Response.Redirect(this._navigationManager.NavigateURL(redirectTabId > 0 ? redirectTabId : this.PortalSettings.HomeTabId, string.Empty, "VerificationSuccess=true"), true);
                        }
                        else
                        {
                            if (redirectTabId > 0)
                            {
                                var redirectUrl = this._navigationManager.NavigateURL(redirectTabId, string.Empty, "VerificationSuccess=true");
                                redirectUrl = redirectUrl.Replace(Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias), string.Empty);
                                this.Response.Cookies.Add(new HttpCookie("returnurl", redirectUrl) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
                            }

                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("VerificationSuccess", this.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                        }
                    }
                    catch (UserAlreadyVerifiedException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserAlreadyVerified", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                    catch (InvalidVerificationCodeException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    catch (UserDoesNotExistException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserDoesNotExist", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    catch (Exception)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }

            if (!this.Request.IsAuthenticated)
            {
                if (!this.Page.IsPostBack)
                {
                    try
                    {
                        if (this.Request.QueryString["username"] != null)
                        {
                            this.txtUsername.Text = this.Request.QueryString["username"];
                        }
                    }
                    catch (Exception ex)
                    {
                        // control not there
                        Logger.Error(ex);
                    }
                }

                try
                {
                    Globals.SetFormFocus(string.IsNullOrEmpty(this.txtUsername.Text) ? this.txtUsername : this.txtPassword);
                }
                catch (Exception ex)
                {
                    // Not sure why this Try/Catch may be necessary, logic was there in old setFormFocus location stating the following
                    // control not there or error setting focus
                    Logger.Error(ex);
                }
            }

            var registrationType = this.PortalSettings.Registration.RegistrationFormType;
            bool useEmailAsUserName;
            if (registrationType == 0)
            {
                useEmailAsUserName = this.PortalSettings.Registration.UseEmailAsUserName;
            }
            else
            {
                var registrationFields = this.PortalSettings.Registration.RegistrationFields;
                useEmailAsUserName = !registrationFields.Contains("Username");
            }

            this.plUsername.Text = this.LocalizeString(useEmailAsUserName ? "Email" : "Username");
            this.divCaptcha1.Visible = this.UseCaptcha;
            this.divCaptcha2.Visible = this.UseCaptcha;
        }

        protected string GetRedirectUrl(bool checkSettings = true)
        {
            var redirectUrl = string.Empty;
            var redirectAfterLogin = this.PortalSettings.Registration.RedirectAfterLogin;
            if (checkSettings && redirectAfterLogin > 0) // redirect to after registration page
            {
                redirectUrl = this._navigationManager.NavigateURL(redirectAfterLogin);
            }
            else
            {
                if (this.Request.QueryString["returnurl"] != null)
                {
                    // return to the url passed to register
                    redirectUrl = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                    // clean the return url to avoid possible XSS attack.
                    redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);

                    if (redirectUrl.Contains("?returnurl"))
                    {
                        string baseURL = redirectUrl.Substring(
                            0,
                            redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal));
                        string returnURL =
                            redirectUrl.Substring(redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                        redirectUrl = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                    }
                }

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    // redirect to current page
                    redirectUrl = this._navigationManager.NavigateURL();
                }
            }

            return redirectUrl;
        }

        private void OnLoginClick(object sender, EventArgs e)
        {
            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || !this.UseCaptcha)
            {
                var loginStatus = UserLoginStatus.LOGIN_FAILURE;
                string userName = PortalSecurity.Instance.InputFilter(
                    this.txtUsername.Text,
                    PortalSecurity.FilterFlag.NoScripting |
                                        PortalSecurity.FilterFlag.NoAngleBrackets |
                                        PortalSecurity.FilterFlag.NoMarkup);

                // DNN-6093
                // check if we use email address here rather than username
                UserInfo userByEmail = null;
                var emailUsedAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);

                if (emailUsedAsUsername)
                {
                    // one additonal call to db to see if an account with that email actually exists
                    userByEmail = UserController.GetUserByEmail(PortalController.GetEffectivePortalId(this.PortalId), userName);

                    if (userByEmail != null)
                    {
                        // we need the username of the account in order to authenticate in the next step
                        userName = userByEmail.Username;
                    }
                }

                UserInfo objUser = null;

                if (!emailUsedAsUsername || userByEmail != null)
                {
                    objUser = UserController.ValidateUser(this.PortalId, userName, this.txtPassword.Text, "DNN", string.Empty, this.PortalSettings.PortalName, this.IPAddress, ref loginStatus);
                }

                var authenticated = Null.NullBoolean;
                var message = Null.NullString;
                if (loginStatus == UserLoginStatus.LOGIN_USERNOTAPPROVED)
                {
                    message = "UserNotAuthorized";
                }
                else
                {
                    authenticated = loginStatus != UserLoginStatus.LOGIN_FAILURE;
                }

                // Raise UserAuthenticated Event
                var eventArgs = new UserAuthenticatedEventArgs(objUser, userName, loginStatus, "DNN")
                {
                    Authenticated = authenticated,
                    Message = message,
                    RememberMe = this.chkCookie.Checked,
                };
                this.OnUserAuthenticated(eventArgs);
            }
        }
    }
}
