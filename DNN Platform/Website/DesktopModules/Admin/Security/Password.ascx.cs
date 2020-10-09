// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Threading;
    using System.Web.Security;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls;

    using Host = DotNetNuke.Entities.Host.Host;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Password UserModuleBase is used to manage Users Passwords.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Password : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Password));

        public delegate void PasswordUpdatedEventHandler(object sender, PasswordUpdatedEventArgs e);

        public event PasswordUpdatedEventHandler PasswordUpdated;

        public event PasswordUpdatedEventHandler PasswordQuestionAnswerUpdated;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserMembership associated with this control.
        /// </summary>
        public UserMembership Membership
        {
            get
            {
                UserMembership _Membership = null;
                if (this.User != null)
                {
                    _Membership = this.User.Membership;
                }

                return _Membership;
            }
        }

        protected bool UseCaptcha
        {
            get
            {
                return Convert.ToBoolean(GetSetting(this.PortalId, "Security_CaptchaChangePassword"));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the PasswordUpdated Event.
        /// </summary>
        public void OnPasswordUpdated(PasswordUpdatedEventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.PasswordUpdated != null)
            {
                this.PasswordUpdated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the PasswordQuestionAnswerUpdated Event.
        /// </summary>
        public void OnPasswordQuestionAnswerUpdated(PasswordUpdatedEventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.PasswordQuestionAnswerUpdated != null)
            {
                this.PasswordQuestionAnswerUpdated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls.
        /// </summary>
        public override void DataBind()
        {
            this.lblLastChanged.Text = this.User.Membership.LastPasswordChangeDate.ToLongDateString();

            // Set Password Expiry Label
            if (this.User.Membership.UpdatePassword)
            {
                this.lblExpires.Text = Localization.GetString("ForcedExpiry", this.LocalResourceFile);
            }
            else
            {
                this.lblExpires.Text = PasswordConfig.PasswordExpiry > 0 ? this.User.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry).ToLongDateString() : Localization.GetString("NoExpiry", this.LocalResourceFile);
            }

            if ((!MembershipProviderConfig.PasswordRetrievalEnabled) && this.IsAdmin && (!this.IsUser))
            {
                this.pnlChange.Visible = true;
                this.cmdUpdate.Visible = true;
                this.oldPasswordRow.Visible = false;
                this.lblChangeHelp.Text = Localization.GetString("AdminChangeHelp", this.LocalResourceFile);
            }
            else
            {
                this.pnlChange.Visible = true;
                this.cmdUpdate.Visible = true;

                // Set up Change Password
                if (this.IsAdmin && !this.IsUser)
                {
                    this.lblChangeHelp.Text = Localization.GetString("AdminChangeHelp", this.LocalResourceFile);
                    this.oldPasswordRow.Visible = false;
                }
                else
                {
                    this.lblChangeHelp.Text = Localization.GetString("UserChangeHelp", this.LocalResourceFile);
                    if (this.Request.IsAuthenticated)
                    {
                        this.pnlChange.Visible = true;
                        this.cmdUserReset.Visible = false;
                        this.cmdUpdate.Visible = true;
                    }
                    else
                    {
                        this.pnlChange.Visible = false;
                        this.cmdUserReset.Visible = true;
                        this.cmdUpdate.Visible = false;
                    }
                }
            }

            // If Password Reset is not enabled then only the Admin can reset the
            // Password, a User must Update
            if (!MembershipProviderConfig.PasswordResetEnabled)
            {
                this.pnlReset.Visible = false;
                this.cmdReset.Visible = false;
            }
            else
            {
                this.pnlReset.Visible = true;
                this.cmdReset.Visible = true;

                // Set up Reset Password
                if (this.IsAdmin && !this.IsUser)
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                    {
                        this.pnlReset.Visible = false;
                        this.cmdReset.Visible = false;
                    }
                    else
                    {
                        this.lblResetHelp.Text = Localization.GetString("AdminResetHelp", this.LocalResourceFile);
                    }

                    this.questionRow.Visible = false;
                    this.answerRow.Visible = false;
                }
                else
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer && this.IsUser)
                    {
                        this.lblResetHelp.Text = Localization.GetString("UserResetHelp", this.LocalResourceFile);
                        this.lblQuestion.Text = this.User.Membership.PasswordQuestion;
                        this.questionRow.Visible = true;
                        this.answerRow.Visible = true;
                    }
                    else
                    {
                        this.pnlReset.Visible = false;
                        this.cmdReset.Visible = false;
                    }
                }
            }

            // Set up Edit Question and Answer area
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && this.IsUser)
            {
                this.pnlQA.Visible = true;
                this.cmdUpdateQA.Visible = true;
            }
            else
            {
                this.pnlQA.Visible = false;
                this.cmdUpdateQA.Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cmdReset.Click += this.cmdReset_Click;
            this.cmdUserReset.Click += this.cmdUserReset_Click;
            this.cmdUpdate.Click += this.cmdUpdate_Click;
            this.cmdUpdateQA.Click += this.cmdUpdateQA_Click;

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && this.User.UserID != UserController.Instance.GetCurrentUserInfo().UserID)
            {
                this.pnlChange.Visible = false;
                this.cmdUpdate.Visible = false;
                this.CannotChangePasswordMessage.Visible = true;
            }

            if (this.UseCaptcha)
            {
                this.captchaRow.Visible = true;
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/stylesheets/dnn.PasswordStrength.css", FileOrder.Css.ResourceCss);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            base.OnPreRender(e);

            if (Host.EnableStrengthMeter)
            {
                this.passwordContainer.CssClass = "password-strength-container";
                this.txtNewPassword.CssClass = "password-strength";

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
                FirstElementSelector = "#" + this.passwordContainer.ClientID + " input[type=password]",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnPassword",
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

        private void cmdReset_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            string answer = string.Empty;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && !this.IsAdmin)
            {
                if (string.IsNullOrEmpty(this.txtAnswer.Text))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    return;
                }

                answer = this.txtAnswer.Text;
            }

            try
            {
                // create resettoken
                UserController.ResetPasswordToken(this.User, Entities.Host.Host.AdminMembershipResetLinkValidity);

                bool canSend = Mail.SendMail(this.User, MessageType.PasswordReminder, this.PortalSettings) == string.Empty;
                var message = string.Empty;
                var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
                if (canSend)
                {
                    message = Localization.GetString("PasswordSent", this.LocalResourceFile);
                    this.LogSuccess();
                }
                else
                {
                    message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    this.LogFailure(message);
                }

                UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
            }
            catch (ArgumentException exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
            }
        }

        private void cmdUserReset_Click(object sender, EventArgs e)
        {
            try
            {
                // send fresh resettoken copy
                bool canSend = UserController.ResetPasswordToken(this.User, true);

                var message = string.Empty;
                var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
                if (canSend)
                {
                    message = Localization.GetString("PasswordSent", this.LocalResourceFile);
                    this.LogSuccess();
                }
                else
                {
                    message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    this.LogFailure(message);
                }

                UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
            }
            catch (ArgumentException exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
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
                LogUserName = portalSecurity.InputFilter(this.User.Username, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup),
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

            LogController.Instance.AddLog(log);
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || !this.UseCaptcha)
            {
                if (this.IsUserOrAdmin == false)
                {
                    return;
                }

                // 1. Check New Password and Confirm are the same
                if (this.txtNewPassword.Text != this.txtNewConfirm.Text)
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordMismatch));
                    return;
                }

                // 2. Check New Password is Valid
                if (!UserController.ValidatePassword(this.txtNewPassword.Text))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordInvalid));
                    return;
                }

                // 3. Check old Password is Provided
                if (!this.IsAdmin && string.IsNullOrEmpty(this.txtOldPassword.Text))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordMissing));
                    return;
                }

                // 4. Check New Password is ddifferent
                if (!this.IsAdmin && this.txtNewPassword.Text == this.txtOldPassword.Text)
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordNotDifferent));
                    return;
                }

                // 5. Check New Password is not same as username or banned
                var membershipPasswordController = new MembershipPasswordController();
                var settings = new MembershipPasswordSettings(this.User.PortalID);

                if (settings.EnableBannedList)
                {
                    if (membershipPasswordController.FoundBannedPassword(this.txtNewPassword.Text) || this.User.Username == this.txtNewPassword.Text)
                    {
                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.BannedPasswordUsed));
                        return;
                    }
                }

                // check new password is not in history
                if (membershipPasswordController.IsPasswordInHistory(this.User.UserID, this.User.PortalID, this.txtNewPassword.Text, false))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    return;
                }

                if (!this.IsAdmin && this.txtNewPassword.Text == this.txtOldPassword.Text)
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordNotDifferent));
                    return;
                }

                if (!this.IsAdmin)
                {
                    try
                    {
                        this.OnPasswordUpdated(UserController.ChangePassword(this.User, this.txtOldPassword.Text, this.txtNewPassword.Text)
                                              ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                              : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                    catch (MembershipPasswordException exc)
                    {
                        // Password Answer missing
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    }
                    catch (ThreadAbortException)
                    {
                        // Do nothing we are not logging ThreadAbortxceptions caused by redirects
                    }
                    catch (Exception exc)
                    {
                        // Fail
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                }
                else
                {
                    try
                    {
                        this.OnPasswordUpdated(UserController.ResetAndChangePassword(this.User, this.txtNewPassword.Text)
                                              ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                              : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                    catch (MembershipPasswordException exc)
                    {
                        // Password Answer missing
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    }
                    catch (ThreadAbortException)
                    {
                        // Do nothing we are not logging ThreadAbortxceptions caused by redirects
                    }
                    catch (Exception exc)
                    {
                        // Fail
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Question and Answer  Button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void cmdUpdateQA_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.txtQAPassword.Text))
            {
                this.OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordInvalid));
                return;
            }

            if (string.IsNullOrEmpty(this.txtEditQuestion.Text))
            {
                this.OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordQuestion));
                return;
            }

            if (string.IsNullOrEmpty(this.txtEditAnswer.Text))
            {
                this.OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                return;
            }

            // Try and set password Q and A
            UserInfo objUser = UserController.GetUserById(this.PortalId, this.UserId);
            this.OnPasswordQuestionAnswerUpdated(UserController.ChangePasswordQuestionAndAnswer(objUser, this.txtQAPassword.Text, this.txtEditQuestion.Text, this.txtEditAnswer.Text)
                                                ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                                : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The PasswordUpdatedEventArgs class provides a customised EventArgs class for
        /// the PasswordUpdated Event.
        /// </summary>
        public class PasswordUpdatedEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="PasswordUpdatedEventArgs"/> class.
            /// Constructs a new PasswordUpdatedEventArgs.
            /// </summary>
            /// <param name="status">The Password Update Status.</param>
            public PasswordUpdatedEventArgs(PasswordUpdateStatus status)
            {
                this.UpdateStatus = status;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the Update Status.
            /// </summary>
            public PasswordUpdateStatus UpdateStatus { get; set; }
        }
    }
}
