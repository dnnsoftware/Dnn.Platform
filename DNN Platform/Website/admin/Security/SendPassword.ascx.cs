// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Collections;
    using System.Net;
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

    /// <summary>The SendPassword UserModuleBase is used to allow a user to retrieve their password.</summary>
    public partial class SendPassword : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SendPassword));
        private readonly INavigationManager navigationManager;

        private UserInfo user;
        private int userCount = Null.NullInteger;
        private string ipAddress;

        /// <summary>Initializes a new instance of the <see cref="SendPassword"/> class.</summary>
        public SendPassword()
        {
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>Gets the Redirect URL (after successful sending of password).</summary>
        protected string RedirectURL
        {
            get
            {
                var redirectURL = string.Empty;

                object setting = GetSetting(this.PortalId, "Redirect_AfterRegistration");

                if (Convert.ToInt32(setting) > 0)
                {
                    // redirect to after registration page
                    redirectURL = this.navigationManager.NavigateURL(Convert.ToInt32(setting));
                }
                else
                {
                    if (Convert.ToInt32(setting) <= 0)
                    {
                        if (this.Request.QueryString["returnurl"] != null)
                        {
                            // return to the url passed to register
                            redirectURL = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                            // clean the return url to avoid possible XSS attack.
                            redirectURL = UrlUtils.ValidReturnUrl(redirectURL);

                            if (redirectURL.Contains("?returnurl"))
                            {
                                string baseURL = redirectURL.Substring(
                                    0,
                                    redirectURL.IndexOf("?returnurl", StringComparison.Ordinal));
                                string returnURL =
                                    redirectURL.Substring(redirectURL.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

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
                        redirectURL = this.navigationManager.NavigateURL(Convert.ToInt32(setting));
                    }
                }

                return redirectURL;
            }
        }

        /// <summary>Gets a value indicating whether the Captcha control is used to validate the login.</summary>
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

        /// <inheritdoc/>
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

        /// <summary>Page_Load runs when the control is loaded.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdSendPassword.Click += this.OnSendPasswordClick;
            this.lnkCancel.NavigateUrl = this.navigationManager.NavigateURL();

            this.ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));

            this.divEmail.Visible = this.ShowEmailField;
            this.divUsername.Visible = !this.UsernameDisabled;
            this.divCaptcha.Visible = this.UseCaptcha;

            if (this.UseCaptcha)
            {
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }
        }

        /// <summary>cmdSendPassword_Click runs when the Password Reminder button is clicked.</summary>
        protected void OnSendPasswordClick(object sender, EventArgs e)
        {
            // pretty much always display the same message to avoid hinting on the existance of a user name
            var input = string.IsNullOrEmpty(this.txtUsername.Text) ? this.txtEmail.Text : this.txtUsername.Text;
            var message = string.Format(Localization.GetString("PasswordSent", this.LocalResourceFile), WebUtility.HtmlEncode(input));
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
                    if (this.user != null)
                    {
                        if (this.user.IsDeleted)
                        {
                            canSend = false;
                        }
                        else
                        {
                            if (this.user.Membership.Approved == false)
                            {
                                Mail.SendMail(this.user, MessageType.PasswordReminderUserIsNotApproved, this.PortalSettings);
                                if (this.PortalSettings.EnableUnapprovedPasswordReminderNotification)
                                {
                                    Mail.SendMail(
                                        this.user,
                                        MessageType.PasswordReminderUserIsNotApprovedAdmin,
                                        this.PortalSettings);
                                }

                                canSend = false;
                            }

                            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                            {
                                UserController.ResetPasswordToken(this.user);
                            }

                            if (canSend)
                            {
                                if (Mail.SendMail(this.user, MessageType.PasswordReminder, this.PortalSettings) != string.Empty)
                                {
                                    canSend = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.userCount > 1)
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
                arrUsers = UserController.GetUsersByEmail(this.PortalSettings.PortalId, this.txtEmail.Text, 0, int.MaxValue, ref this.userCount);
                if (arrUsers != null && arrUsers.Count == 1)
                {
                    this.user = (UserInfo)arrUsers[0];
                }
            }
            else
            {
                this.user = UserController.GetUserByName(this.PortalSettings.PortalId, this.txtUsername.Text);
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

            log.AddProperty("IP", this.ipAddress);

            LogController.Instance.AddLog(log);
        }
    }
}
