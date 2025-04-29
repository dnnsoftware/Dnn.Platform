// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    using Host = DotNetNuke.Entities.Host.Host;

    public partial class PasswordReset : UserModuleBase
    {
        private const int RedirectTimeout = 3000;

        private readonly INavigationManager navigationManager;
        private string ipAddress;

        /// <summary>Initializes a new instance of the <see cref="PasswordReset"/> class.</summary>
        public PasswordReset()
        {
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private string ResetToken
        {
            get
            {
                return this.ViewState["ResetToken"] != null ? this.Request.QueryString["resetToken"] : string.Empty;
            }

            set
            {
                this.ViewState.Add("ResetToken", value);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/stylesheets/dnn.PasswordStrength.css", FileOrder.Css.ResourceCss);

            if (this.PortalSettings.LoginTabId != -1 && this.PortalSettings.ActiveTab.TabID != this.PortalSettings.LoginTabId)
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(this.PortalSettings.LoginTabId) + this.Request.Url.Query);
            }

            this.cmdChangePassword.Click += this.CmdChangePassword_Click;

            this.hlCancel.NavigateUrl = this.navigationManager.NavigateURL();

            if (this.Request.QueryString["resetToken"] != null)
            {
                this.ResetToken = this.Request.QueryString["resetToken"];
                this.txtUsername.Enabled = false;
            }

            var useEmailAsUserName = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);
            if (useEmailAsUserName)
            {
                this.valUsername.Text = Localization.GetString("Email.Required", this.LocalResourceFile);
            }
            else
            {
                this.valUsername.Text = Localization.GetString("Username.Required", this.LocalResourceFile);
            }

            if (this.Request.QueryString["forced"] == "true")
            {
                this.lblInfo.Text = Localization.GetString("ForcedResetInfo", this.LocalResourceFile);
            }

            this.txtUsername.Attributes.Add("data-default", useEmailAsUserName ? this.LocalizeText("Email") : this.LocalizeText("Username"));
            this.txtPassword.Attributes.Add("data-default", this.LocalizeText("Password"));
            this.txtConfirmPassword.Attributes.Add("data-default", this.LocalizeText("Confirm"));
            this.txtAnswer.Attributes.Add("data-default", this.LocalizeText("Answer"));

            if (!this.Page.IsPostBack)
            {
                this.LoadUserInfo();
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!string.IsNullOrEmpty(this.lblHelp.Text) || !string.IsNullOrEmpty(this.lblInfo.Text))
            {
                this.resetMessages.Visible = true;
            }

            if (Host.EnableStrengthMeter)
            {
                this.passwordContainer.CssClass = "password-strength-container";
                this.txtPassword.CssClass = "password-strength";

                var options = new DnnPaswordStrengthOptions();
                var optionsAsJsonString = Json.Serialize(options);
                var script = string.Format("dnn.initializePasswordStrength('.{0}', {1});{2}", "password-strength", optionsAsJsonString, Environment.NewLine);

                if (ScriptManager.GetCurrent(this.Page) != null)
                {
                    // respect MS AJAX
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "PasswordStrength", script, true);
                }
                else
                {
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), "PasswordStrength", script, true);
                }
            }

            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = ".password-strength",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnPasswordReset",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
            var confirmScript = string.Format("dnn.initializePasswordComparer({0});{1}", confirmOptionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ConfirmPassword", confirmScript, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", confirmScript, true);
            }
        }

        /// <summary>After a successful password change will redirect the user to requested returnurl OR the login page.</summary>
        protected void RedirectAfterPasswordChange()
        {
            var redirectUrl = string.Empty;

            if (this.Request.QueryString["returnurl"] != null)
            {
                // return to the url passed to signin
                redirectUrl = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                // clean the return url to avoid possible XSS attack.
                redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);
            }

            if (this.Request.Cookies["returnurl"] != null)
            {
                // return to the url passed to signin
                redirectUrl = HttpUtility.UrlDecode(this.Request.Cookies["returnurl"].Value);

                // clean the return url to avoid possible XSS attack.
                redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);
            }

            if (string.IsNullOrEmpty(redirectUrl))
            {
                // return to the login page by default to allow users to login
                redirectUrl = this.navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Login");
            }

            this.AddModuleMessage("ChangeSuccessful", ModuleMessage.ModuleMessageType.GreenSuccess, true);
            this.resetMessages.Visible = this.divPassword.Visible = false;
            this.lblHelp.Text = this.lblInfo.Text = string.Empty;

            // redirect page after 5 seconds
            var script = string.Format(
                "setTimeout(function(){{location.href = {0};}}, {1});",
                HttpUtility.JavaScriptStringEncode(redirectUrl, addDoubleQuotes: true),
                RedirectTimeout);
            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ChangePasswordSuccessful", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ChangePasswordSuccessful", script, true);
            }
        }

        private void LoadUserInfo()
        {
            var user = UserController.GetUserByPasswordResetToken(this.PortalId, this.ResetToken);

            if (user == null || user.PasswordResetExpiration < DateTime.Now)
            {
                this.divPassword.Visible = false;
                this.resetMessages.Visible = true;
                this.lblHelp.Text = Localization.GetString("ResetLinkExpired", this.LocalResourceFile);
                return;
            }

            this.txtUsername.Text = user.Username;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                this.lblQuestion.Text = user.Membership.PasswordQuestion;
                this.divQA.Visible = true;
            }
        }

        private void CmdChangePassword_Click(object sender, EventArgs e)
        {
            var username = this.txtUsername.Text;

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && string.IsNullOrEmpty(this.txtAnswer.Text))
            {
                return;
            }

            // 1. Check New Password and Confirm are the same
            if (this.txtPassword.Text != this.txtConfirmPassword.Text)
            {
                this.resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordMismatch");
                this.LogFailure(failed);
                this.lblHelp.Text = failed;
                return;
            }

            var newPassword = this.txtPassword.Text.Trim();
            if (UserController.ValidatePassword(newPassword) == false)
            {
                this.resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordResetFailed");
                this.LogFailure(failed);
                this.lblHelp.Text = failed;
                return;
            }

            // Check New Password is not same as username or banned
            var settings = new MembershipPasswordSettings(this.User.PortalID);

            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController();
                if (m.FoundBannedPassword(newPassword) || this.txtUsername.Text == newPassword)
                {
                    this.resetMessages.Visible = true;
                    var failed = Localization.GetString("PasswordResetFailed");
                    this.LogFailure(failed);
                    this.lblHelp.Text = failed;
                    return;
                }
            }

            if (PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false))
            {
                var testUser = UserController.GetUserByEmail(this.PortalId, username); // one additonal call to db to see if an account with that email actually exists
                if (testUser != null)
                {
                    username = testUser.Username; // we need the username of the account in order to change the password in the next step
                }
            }

            string errorMessage;
            var answer = string.Empty;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                answer = this.txtAnswer.Text;
            }

            if (UserController.ChangePasswordByToken(this.PortalSettings.PortalId, username, newPassword, answer, this.ResetToken, out errorMessage) == false)
            {
                this.resetMessages.Visible = true;
                var failed = errorMessage;
                this.LogFailure(failed);
                this.lblHelp.Text = failed;
            }
            else
            {
                // check user has a valid profile
                var user = UserController.GetUserByName(this.PortalSettings.PortalId, username);
                var validStatus = UserController.ValidateUser(user, this.PortalSettings.PortalId, false);
                if (validStatus == UserValidStatus.UPDATEPROFILE)
                {
                    this.LogSuccess();
                    this.ViewState.Add("PageNo", 3);
                    this.Response.Redirect(this.navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Login"));
                }
                else
                {
                    // Log user in to site
                    this.LogSuccess();
                    this.RedirectAfterPasswordChange();
                }
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
            var log = new LogInfo
            {
                LogPortalID = this.PortalSettings.PortalId,
                LogPortalName = this.PortalSettings.PortalName,
                LogUserID = this.UserId,
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
