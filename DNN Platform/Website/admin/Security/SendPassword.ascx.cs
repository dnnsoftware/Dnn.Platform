// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Collections;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    using Host = DotNetNuke.Entities.Host.Host;

    /// <summary>
    /// The SendPassword UserModuleBase is used to allow a user to retrieve their password.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class SendPassword : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SendPassword));
        private readonly INavigationManager _navigationManager;

        private UserInfo _user;
        private int _userCount = Null.NullInteger;
        private string _ipAddress;

        public SendPassword()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>
        /// Gets the Redirect URL (after successful sending of password).
        /// </summary>
        protected string RedirectURL
        {
            get
            {
                var _RedirectURL = string.Empty;

                object setting = GetSetting(this.PortalId, "Redirect_AfterRegistration");

                if (Convert.ToInt32(setting) > 0) // redirect to after registration page
                {
                    _RedirectURL = this._navigationManager.NavigateURL(Convert.ToInt32(setting));
                }
                else
                {
                    if (Convert.ToInt32(setting) <= 0)
                    {
                        if (this.Request.QueryString["returnurl"] != null)
                        {
                            // return to the url passed to register
                            _RedirectURL = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                            // clean the return url to avoid possible XSS attack.
                            _RedirectURL = UrlUtils.ValidReturnUrl(_RedirectURL);

                            if (_RedirectURL.Contains("?returnurl"))
                            {
                                string baseURL = _RedirectURL.Substring(
                                    0,
                                    _RedirectURL.IndexOf("?returnurl", StringComparison.Ordinal));
                                string returnURL =
                                    _RedirectURL.Substring(_RedirectURL.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

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
                        _RedirectURL = this._navigationManager.NavigateURL(Convert.ToInt32(setting));
                    }
                }

                return _RedirectURL;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the Captcha control is used to validate the login.
        /// </summary>
        protected bool UseCaptcha
        {
            get
            {
                var setting = GetSetting(this.PortalId, "Security_CaptchaRetrivePassword");
                return Convert.ToBoolean(setting);
            }
        }

        protected bool UsernameDisabled
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);
            }
        }

        private bool ShowEmailField
        {
            get
            {
                return MembershipProviderConfig.RequiresUniqueEmail || this.UsernameDisabled;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var isEnabled = true;

            // both retrieval and reset now use password token resets
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                this.lblHelp.Text = Localization.GetString("ResetTokenHelp", this.LocalResourceFile);
                this.cmdSendPassword.Text = Localization.GetString("ResetToken", this.LocalResourceFile);
            }
            else
            {
                isEnabled = false;
                this.lblHelp.Text = Localization.GetString("DisabledPasswordHelp", this.LocalResourceFile);
                this.divPassword.Visible = false;
            }

            if (!MembershipProviderConfig.PasswordResetEnabled)
            {
                isEnabled = false;
                this.lblHelp.Text = Localization.GetString("DisabledPasswordHelp", this.LocalResourceFile);
                this.divPassword.Visible = false;
            }

            if (MembershipProviderConfig.RequiresUniqueEmail && isEnabled && !PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false))
            {
                this.lblHelp.Text += Localization.GetString("RequiresUniqueEmail", this.LocalResourceFile);
            }

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && isEnabled)
            {
                this.lblHelp.Text += Localization.GetString("RequiresQuestionAndAnswer", this.LocalResourceFile);
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

            this.cmdSendPassword.Click += this.OnSendPasswordClick;
            this.lnkCancel.NavigateUrl = this._navigationManager.NavigateURL();

            this._ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));

            this.divEmail.Visible = this.ShowEmailField;
            this.divUsername.Visible = !this.UsernameDisabled;
            this.divCaptcha.Visible = this.UseCaptcha;

            if (this.UseCaptcha)
            {
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }
        }

        /// <summary>
        /// cmdSendPassword_Click runs when the Password Reminder button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void OnSendPasswordClick(object sender, EventArgs e)
        {
            // pretty much alwasy display the same message to avoid hinting on the existance of a user name
            var message = Localization.GetString("PasswordSent", this.LocalResourceFile);
            var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
            var canSend = true;

            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || (!this.UseCaptcha))
            {
                if (string.IsNullOrEmpty(this.txtUsername.Text.Trim()))
                {
                    // No UserName provided
                    if (this.ShowEmailField)
                    {
                        if (string.IsNullOrEmpty(this.txtEmail.Text.Trim()))
                        {
                            // No email address either (cannot retrieve password)
                            canSend = false;
                            message = Localization.GetString("EnterUsernameEmail", this.LocalResourceFile);
                            moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                        }
                    }
                    else
                    {
                        // Cannot retrieve password
                        canSend = false;
                        message = Localization.GetString("EnterUsername", this.LocalResourceFile);
                        moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    }
                }

                if (string.IsNullOrEmpty(Host.SMTPServer))
                {
                    // SMTP Server is not configured
                    canSend = false;
                    message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.YellowWarning;

                    var logMessage = Localization.GetString("SMTPNotConfigured", this.LocalResourceFile);

                    this.LogResult(logMessage);
                }

                if (canSend)
                {
                    this.GetUser();
                    if (this._user != null)
                    {
                        if (this._user.IsDeleted)
                        {
                            canSend = false;
                        }
                        else
                        {
                            if (this._user.Membership.Approved == false)
                            {
                                Mail.SendMail(this._user, MessageType.PasswordReminderUserIsNotApproved, this.PortalSettings);
                                canSend = false;
                            }

                            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                            {
                                UserController.ResetPasswordToken(this._user);
                            }

                            if (canSend)
                            {
                                if (Mail.SendMail(this._user, MessageType.PasswordReminder, this.PortalSettings) != string.Empty)
                                {
                                    canSend = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this._userCount > 1)
                        {
                            message = Localization.GetString("MultipleUsers", this.LocalResourceFile);
                        }

                        canSend = false;
                    }

                    if (canSend)
                    {
                        this.LogSuccess();
                        this.lnkCancel.Attributes["resourcekey"] = "cmdClose";
                    }
                    else
                    {
                        this.LogFailure(message);
                    }

                    // always hide panel so as to not reveal if username exists.
                    this.pnlRecover.Visible = false;
                    UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
                    this.liSend.Visible = false;
                    this.liCancel.Visible = true;
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
                }
            }
        }

        private void GetUser()
        {
            ArrayList arrUsers;
            if (this.ShowEmailField && !string.IsNullOrEmpty(this.txtEmail.Text.Trim()) && (string.IsNullOrEmpty(this.txtUsername.Text.Trim()) || this.divUsername.Visible == false))
            {
                arrUsers = UserController.GetUsersByEmail(this.PortalSettings.PortalId, this.txtEmail.Text, 0, int.MaxValue, ref this._userCount);
                if (arrUsers != null && arrUsers.Count == 1)
                {
                    this._user = (UserInfo)arrUsers[0];
                }
            }
            else
            {
                this._user = UserController.GetUserByName(this.PortalSettings.PortalId, this.txtUsername.Text);
            }
        }

        private void LogSuccess()
        {
            this.LogResult(string.Empty);
        }

        private void LogFailure(string reason)
        {
            this.LogResult(reason);
        }

        private void LogResult(string message)
        {
            var portalSecurity = PortalSecurity.Instance;

            var log = new LogInfo
            {
                LogPortalID = this.PortalSettings.PortalId,
                LogPortalName = this.PortalSettings.PortalName,
                LogUserID = this.UserId,
                LogUserName = portalSecurity.InputFilter(this.txtUsername.Text, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup),
            };

            if (string.IsNullOrEmpty(message))
            {
                log.LogTypeKey = "PASSWORD_SENT_SUCCESS";
            }
            else
            {
                log.LogTypeKey = "PASSWORD_SENT_FAILURE";
                log.LogProperties.Add(new LogDetailInfo("Cause", message));
            }

            log.AddProperty("IP", this._ipAddress);

            LogController.Instance.AddLog(log);
        }
    }
}
