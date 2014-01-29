#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Password UserModuleBase is used to manage Users Passwords
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	03/03/2006  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Password : UserModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Password));
        
        #region Delegates

        public delegate void PasswordUpdatedEventHandler(object sender, PasswordUpdatedEventArgs e);

        #endregion
		
		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserMembership associated with this control
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public UserMembership Membership
        {
            get
            {
                UserMembership _Membership = null;
                if (User != null)
                {
                    _Membership = User.Membership;
                }
                return _Membership;
            }
        }
		
		#endregion

		#region Events


        public event PasswordUpdatedEventHandler PasswordUpdated;
        public event PasswordUpdatedEventHandler PasswordQuestionAnswerUpdated;

		#endregion

		#region Event Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the PasswordUpdated Event
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/08/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void OnPasswordUpdated(PasswordUpdatedEventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            if (PasswordUpdated != null)
            {
                PasswordUpdated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the PasswordQuestionAnswerUpdated Event
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/09/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void OnPasswordQuestionAnswerUpdated(PasswordUpdatedEventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            if (PasswordQuestionAnswerUpdated != null)
            {
                PasswordQuestionAnswerUpdated(this, e);
            }
        }

		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            lblLastChanged.Text = User.Membership.LastPasswordChangeDate.ToLongDateString();

            //Set Password Expiry Label
            if (User.Membership.UpdatePassword)
            {
                lblExpires.Text = Localization.GetString("ForcedExpiry", LocalResourceFile);
            }
            else
            {
                lblExpires.Text = PasswordConfig.PasswordExpiry > 0 ? User.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry).ToLongDateString() : Localization.GetString("NoExpiry", LocalResourceFile);
            }
			
           if (((!MembershipProviderConfig.PasswordRetrievalEnabled) && IsAdmin && (!IsUser)))
            {
                pnlChange.Visible = true;
                cmdUpdate.Visible = true;
                oldPasswordRow.Visible = false;
                lblChangeHelp.Text = Localization.GetString("AdminChangeHelp", LocalResourceFile);
            }
            else
            {
                pnlChange.Visible = true;
                cmdUpdate.Visible = true;
				
				//Set up Change Password
                if (IsAdmin && !IsUser)
                {
                    lblChangeHelp.Text = Localization.GetString("AdminChangeHelp", LocalResourceFile);
                    oldPasswordRow.Visible = false;
                }
                else
                {
                    lblChangeHelp.Text = Localization.GetString("UserChangeHelp", LocalResourceFile);
                    if (Request.IsAuthenticated)
                    {
                        pnlChange.Visible = true;
                        cmdUserReset.Visible = false;
                        cmdUpdate.Visible = true;
                    }
                    else
                    {
                        pnlChange.Visible = false;
                        cmdUserReset.Visible = true;
                        cmdUpdate.Visible = false;
                    }
                }
            }
			
            //If Password Reset is not enabled then only the Admin can reset the 
            //Password, a User must Update
            if (!MembershipProviderConfig.PasswordResetEnabled)
            {
                pnlReset.Visible = false;
                cmdReset.Visible = false;
            }
            else
            {
                pnlReset.Visible = true;
                cmdReset.Visible = true;
				
				//Set up Reset Password
                if (IsAdmin && !IsUser)
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                    {
                        pnlReset.Visible = false;
                        cmdReset.Visible = false;
                    }
                    else
                    {
                        lblResetHelp.Text = Localization.GetString("AdminResetHelp", LocalResourceFile);
                    }
                    questionRow.Visible = false;
                    answerRow.Visible = false;
                }
                else
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer && IsUser)
                    {
                        lblResetHelp.Text = Localization.GetString("UserResetHelp", LocalResourceFile);
                        lblQuestion.Text = User.Membership.PasswordQuestion;
                        questionRow.Visible = true;
                        answerRow.Visible = true;
                    }
                    else
                    {
                        pnlReset.Visible = false;
                        cmdReset.Visible = false;
                    }
                }
            }
			
            //Set up Edit Question and Answer area
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && IsUser)
            {
                pnlQA.Visible = true;
                cmdUpdateQA.Visible = true;
            }
            else
            {
                pnlQA.Visible = false;
                cmdUpdateQA.Visible = false;
            }
        }

		#endregion

		#region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");
			ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            jQuery.RequestDnnPluginsRegistration();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //ClientAPI.RegisterKeyCapture(Parent, cmdUpdate.Controls[0], 13);
            //ClientAPI.RegisterKeyCapture(this, cmdUpdate.Controls[0], 13);
            cmdReset.Click += cmdReset_Click;
            cmdUserReset.Click += cmdUserReset_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            cmdUpdateQA.Click += cmdUpdateQA_Click;

			if (MembershipProviderConfig.RequiresQuestionAndAnswer && User.UserID != UserController.GetCurrentUserInfo().UserID)
			{
				pnlChange.Visible = false;
			    cmdUpdate.Visible = false;
				CannotChangePasswordMessage.Visible = true;
			}
           
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);


			if (Host.EnableStrengthMeter)
			{
				passwordContainer.CssClass = "password-strength-container";
				txtNewPassword.CssClass = "password-strength";

				var options = new DnnPaswordStrengthOptions();
				var optionsAsJsonString = Json.Serialize(options);
				var script = string.Format("dnn.initializePasswordStrength('.{0}', {1});{2}",
					"password-strength", optionsAsJsonString, Environment.NewLine);

				if (ScriptManager.GetCurrent(Page) != null)
				{
					// respect MS AJAX
					ScriptManager.RegisterStartupScript(Page, GetType(), "PasswordStrength", script, true);
				}
				else
				{
					Page.ClientScript.RegisterStartupScript(GetType(), "PasswordStrength", script, true);
				}
			}

			var confirmPasswordOptions = new DnnConfirmPasswordOptions()
			{
				FirstElementSelector = "#" + passwordContainer.ClientID + " input[type=password]",
				SecondElementSelector = ".password-confirm",
				ContainerSelector = ".dnnPassword",
				UnmatchedCssClass = "unmatched",
				MatchedCssClass = "matched"
			};

			var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			var confirmScript = string.Format("dnn.initializePasswordComparer({0});{1}", confirmOptionsAsJsonString, Environment.NewLine);

			if (ScriptManager.GetCurrent(Page) != null)
			{
				// respect MS AJAX
				ScriptManager.RegisterStartupScript(Page, GetType(), "ConfirmPassword", confirmScript, true);
			}
			else
			{
				Page.ClientScript.RegisterStartupScript(GetType(), "ConfirmPassword", confirmScript, true);
			}
        }


        private void cmdReset_Click(object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            string answer = "";
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && !IsAdmin)
            {
                if (String.IsNullOrEmpty(txtAnswer.Text))
                {
                    OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    return;
                }
                answer = txtAnswer.Text;
            }
            try
            {
                //create resettoken valid for 24hrs
                UserController.ResetPasswordToken(User,1440);

                bool canSend = Mail.SendMail(User, MessageType.PasswordReminder, PortalSettings) == string.Empty;
                var message = String.Empty;
                var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
                if (canSend)
                {
                    message = Localization.GetString("PasswordSent", LocalResourceFile);
                    LogSuccess();
                }
                else
                {
                    message = Localization.GetString("OptionUnavailable", LocalResourceFile);
                    moduleMessageType=ModuleMessage.ModuleMessageType.RedError;
                    LogFailure(message);
                }

               
                UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
            }
            catch (ArgumentException exc)
            {
                Logger.Error(exc);
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
            }
        }

        private void cmdUserReset_Click(object sender, EventArgs e)
        {
            try
            {
                //send fresh resettoken copy
                bool canSend = UserController.ResetPasswordToken(User,true);

                var message = String.Empty;
                var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
                if (canSend)
                {
                    message = Localization.GetString("PasswordSent", LocalResourceFile);
                    LogSuccess();
                }
                else
                {
                    message = Localization.GetString("OptionUnavailable", LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    LogFailure(message);
                }


                UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
            }
            catch (ArgumentException exc)
            {
                Logger.Error(exc);
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
            }
        }

        private void LogSuccess()
        {
            LogResult(string.Empty);
        }

        private void LogFailure(string reason)
        {
            LogResult(reason);
        }

        private void LogResult(string message)
        {
            var portalSecurity = new PortalSecurity();

            var objEventLog = new EventLogController();
            var objEventLogInfo = new LogInfo();

            objEventLogInfo.LogPortalID = PortalSettings.PortalId;
            objEventLogInfo.LogPortalName = PortalSettings.PortalName;
            objEventLogInfo.LogUserID = UserId;
            objEventLogInfo.LogUserName = portalSecurity.InputFilter(User.Username,
                                                                     PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            if (string.IsNullOrEmpty(message))
            {
                objEventLogInfo.LogTypeKey = "PASSWORD_SENT_SUCCESS";
            }
            else
            {
                objEventLogInfo.LogTypeKey = "PASSWORD_SENT_FAILURE";
                objEventLogInfo.LogProperties.Add(new LogDetailInfo("Cause", message));
            }

            objEventLog.AddLog(objEventLogInfo);
        }

        private void cmdUpdate_Click(Object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            //1. Check New Password and Confirm are the same
            if (txtNewPassword.Text != txtNewConfirm.Text)
            {
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordMismatch));
                return;
            }
			
			//2. Check New Password is Valid
            if (!UserController.ValidatePassword(txtNewPassword.Text))
            {
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordInvalid));
                return;
            }
			
			//3. Check old Password is Provided
            if (!IsAdmin && String.IsNullOrEmpty(txtOldPassword.Text))
            {
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordMissing));
                return;
            }
			
			//4. Check New Password is ddifferent
            if (!IsAdmin && txtNewPassword.Text == txtOldPassword.Text)
            {
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordNotDifferent));
                return;
            }
            //5. Check New Password is not same as username or banned
            var settings = new MembershipPasswordSettings(User.PortalID);

            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController();
                if (m.FoundBannedPassword(txtNewPassword.Text) || User.Username == txtNewPassword.Text)
                {
                    OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.BannedPasswordUsed));
                    return;
                }

            }
            if (!IsAdmin && txtNewPassword.Text == txtOldPassword.Text)
            {
                OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordNotDifferent));
                return;
            }
            if (!IsAdmin)
            {
                try
                {
                    OnPasswordUpdated(UserController.ChangePassword(User, txtOldPassword.Text, txtNewPassword.Text)
                                          ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                          : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                }
                catch (MembershipPasswordException exc)
                {
                    //Password Answer missing
                    Logger.Error(exc);

                    OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                }
                catch (ThreadAbortException)
                {
                    //Do nothing we are not logging ThreadAbortxceptions caused by redirects    
                }
                catch (Exception exc)
                {
                    //Fail
                    Logger.Error(exc);

                    OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                }
            }
            else
            {
                try
                {
                    OnPasswordUpdated(UserController.ResetAndChangePassword(User, txtNewPassword.Text)
                                          ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                          : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                }
                catch (MembershipPasswordException exc)
                {
                    //Password Answer missing
                    Logger.Error(exc);

                    OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                }
                catch (ThreadAbortException)
                {
                    //Do nothing we are not logging ThreadAbortxceptions caused by redirects    
                }
                catch (Exception exc)
                {
                    //Fail
                    Logger.Error(exc);

                    OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                }
            }
           
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Question and Answer  Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/09/2006  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdateQA_Click(object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            if (String.IsNullOrEmpty(txtQAPassword.Text))
            {
                OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordInvalid));
                return;
            }
            if (String.IsNullOrEmpty(txtEditQuestion.Text))
            {
                OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordQuestion));
                return;
            }
            if (String.IsNullOrEmpty(txtEditAnswer.Text))
            {
                OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                return;
            }
			
            //Try and set password Q and A
            UserInfo objUser = UserController.GetUserById(PortalId, UserId);
            OnPasswordQuestionAnswerUpdated(UserController.ChangePasswordQuestionAndAnswer(objUser, txtQAPassword.Text, txtEditQuestion.Text, txtEditAnswer.Text)
                                                ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                                : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
        }
		
		#endregion

        #region Nested type: PasswordUpdatedEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The PasswordUpdatedEventArgs class provides a customised EventArgs class for
        /// the PasswordUpdated Event
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/08/2006  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public class PasswordUpdatedEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new PasswordUpdatedEventArgs
            /// </summary>
            /// <param name="status">The Password Update Status</param>
            /// <history>
            /// 	[cnurse]	03/08/2006  Created
            /// </history>
            /// -----------------------------------------------------------------------------
            public PasswordUpdatedEventArgs(PasswordUpdateStatus status)
            {
                UpdateStatus = status;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the Update Status
            /// </summary>
            /// <history>
            /// 	[cnurse]	03/08/2006  Created
            /// </history>
            /// -----------------------------------------------------------------------------
            public PasswordUpdateStatus UpdateStatus { get; set; }
        }

        #endregion
    }
}