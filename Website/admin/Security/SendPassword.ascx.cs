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
using System.Collections;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Security
{

    /// <summary>
    /// The SendPassword UserModuleBase is used to allow a user to retrieve their password
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	03/21/2006  Created
    /// </history>
    public partial class SendPassword : UserModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SendPassword));

        #region Private Members

        private UserInfo _user;
        private int _userCount = Null.NullInteger;
        private string _ipAddress;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the Redirect URL (after successful sending of password)
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/11/2008  Created
        /// </history>
        protected string RedirectURL
        {
            get
            {
                var redirectURL = "";

                if (Request.QueryString["returnurl"] != null)
                {
                    //return to the url passed
                    redirectURL = HttpUtility.UrlDecode(Request.QueryString["returnurl"]);
                    //redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
                    if (redirectURL.Contains("://"))
                    {
                        redirectURL = "";
                    }
                    if (redirectURL.Contains("?returnurl"))
                    {
                        var baseURL = redirectURL.Substring(0, redirectURL.IndexOf("?returnurl"));
                        var returnURL = redirectURL.Substring(redirectURL.IndexOf("?returnurl") + 11);

                        redirectURL = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                    }
                }
                if (String.IsNullOrEmpty(redirectURL))
                {
                    //redirect to current page 
                    redirectURL = Globals.NavigateURL();
                }
                return redirectURL;
            }
        }

        /// <summary>
        /// Gets whether the Captcha control is used to validate the login
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/21/2006  Created
        /// </history>
        protected bool UseCaptcha
        {
            get
            {
                var setting = GetSetting(PortalId, "Security_CaptchaRetrivePassword");
                return Convert.ToBoolean(setting);
            }
        }

        #endregion

        #region Private Methods

        private void GetUser()
        {
            ArrayList arrUsers;
            if (MembershipProviderConfig.RequiresUniqueEmail && !String.IsNullOrEmpty(txtEmail.Text.Trim()) && String.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                arrUsers = UserController.GetUsersByEmail(PortalSettings.PortalId, txtEmail.Text, 0, Int32.MaxValue, ref _userCount);
                if (arrUsers != null && arrUsers.Count == 1)
                {
                    _user = (UserInfo)arrUsers[0];
                }
            }
            else
            {
                _user = UserController.GetUserByName(PortalSettings.PortalId, txtUsername.Text);
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var isEnabled = true;

            //if (MembershipProviderConfig.PasswordRetrievalEnabled)
            //{
            //    lblHelp.Text = Localization.GetString("SendPasswordHelp", LocalResourceFile);
            //    cmdSendPassword.Text = Localization.GetString("SendPassword", LocalResourceFile);
            //}
            //else if (MembershipProviderConfig.PasswordResetEnabled)
            //{
            //    lblHelp.Text = Localization.GetString("ResetPasswordHelp", LocalResourceFile);
            //    cmdSendPassword.Text = Localization.GetString("ResetPassword", LocalResourceFile);
            //}
            //both retrieval and reset now use password token resets
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                lblHelp.Text = Localization.GetString("ResetTokenHelp", LocalResourceFile);
                cmdSendPassword.Text = Localization.GetString("ResetToken", LocalResourceFile);
            }
            else
            {
                isEnabled = false;
                lblHelp.Text = Localization.GetString("DisabledPasswordHelp", LocalResourceFile);
                divPassword.Visible = false;
            }
            if (MembershipProviderConfig.RequiresUniqueEmail && isEnabled)
            {
                lblHelp.Text += Localization.GetString("RequiresUniqueEmail", LocalResourceFile);
            }
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && isEnabled)
            {
                lblHelp.Text += Localization.GetString("RequiresQuestionAndAnswer", LocalResourceFile);
            }
        }

        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/21/2006  Created
        /// </history>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdSendPassword.Click += OnSendPasswordClick;

            if (Request.UserHostAddress != null)
            {
                _ipAddress = Request.UserHostAddress;
            }
            divEmail.Visible = MembershipProviderConfig.RequiresUniqueEmail;
            divCaptcha.Visible = UseCaptcha;

            if (UseCaptcha)
            {
                ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", LocalResourceFile);
                ctlCaptcha.Text = Localization.GetString("CaptchaText", LocalResourceFile);
            }

            var returnUrl = Request.QueryString["returnurl"] ?? string.Empty;
            if (returnUrl.IndexOf("?returnurl=") != -1)
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
            }
            returnUrl = HttpUtility.UrlEncode(returnUrl);

            var url = Globals.LoginURL(returnUrl, (Request.QueryString["override"] != null));
            hlLogin.NavigateUrl = PortalSettings.EnablePopUps && PortalSettings.LoginTabId == Null.NullInteger ? UrlUtils.PopUpUrl(url,this, PortalSettings, false,false): url;

        }

        /// <summary>
        /// cmdSendPassword_Click runs when the Password Reminder button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/21/2006  Created
        /// </history>
        protected void OnSendPasswordClick(Object sender, EventArgs e)
        {
            //pretty much alwasy display the same message to avoid hinting on the existance of a user name
            var message = Localization.GetString("PasswordSent", LocalResourceFile);
            var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
            var canSend = true;

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && String.IsNullOrEmpty(txtAnswer.Text))
            {
                GetUser();
                if (_user != null)
                {
                    lblQuestion.Text = _user.Membership.PasswordQuestion;
                }
                divQA.Visible = true;
                return;
            }

            if ((UseCaptcha && ctlCaptcha.IsValid) || (!UseCaptcha))
            {
                if (String.IsNullOrEmpty(txtUsername.Text.Trim()))
                {
                    //No UserName provided
                    if (MembershipProviderConfig.RequiresUniqueEmail)
                    {
                        if (String.IsNullOrEmpty(txtEmail.Text.Trim()))
                        {
                            //No email address either (cannot retrieve password)
                            canSend = false;
                            message = Localization.GetString("EnterUsernameEmail", LocalResourceFile);
                            moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                        }
                    }
                    else
                    {
                        //Cannot retrieve password
                        canSend = false;
                        message = Localization.GetString("EnterUsername", LocalResourceFile);
                        moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    }
                }

                if (string.IsNullOrEmpty(Host.SMTPServer))
                {
                    //SMTP Server is not configured
                    canSend = false;
                    message = Localization.GetString("OptionUnavailable", LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.YellowWarning;

                    var logMessage = Localization.GetString("SMTPNotConfigured", LocalResourceFile);

                    LogResult(logMessage);
                }

                if (canSend)
                {
                    GetUser();
                    if (_user != null)
                    {
                        if (_user.IsDeleted)
                        {
                            canSend = false;
                        }
                        else
                        {
                            //if (MembershipProviderConfig.PasswordRetrievalEnabled)
                            //{
                            //    try
                            //    {
                            //        _user.Membership.Password = UserController.GetPassword(ref _user, txtAnswer.Text);
                            //    }
                            //    catch (Exception exc)
                            //    {
                            //        Logger.Error(exc);

                            //        canSend = false;
                            //    }
                            //}
                            //else
                            //{
                            //    try
                            //    {
                            //        _user.Membership.Password = UserController.GeneratePassword();
                            //        UserController.ResetPassword(_user, txtAnswer.Text);
                            //    }
                            //    catch (Exception exc)
                            //    {
                            //        Logger.Error(exc);

                            //        canSend = false;
                            //    }
                            //}
                            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                            {
                                UserController.ResetPasswordToken(_user);
                            }
                            if (canSend)
                            {
                                if (Mail.SendMail(_user, MessageType.PasswordReminder, PortalSettings) != string.Empty)
                                {
                                    canSend = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_userCount > 1)
                        {
                            message = Localization.GetString("MultipleUsers", LocalResourceFile);
                        }

                        canSend = false;
                    }

                    if (canSend)
                    {
                        LogSuccess();
                    }
                    else
                    {
                        LogFailure(message);
                    }

                    UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);

                    liLogin.Visible = true;
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
                }
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
            
            objEventLogInfo.AddProperty("IP", _ipAddress);
            objEventLogInfo.LogPortalID = PortalSettings.PortalId;
            objEventLogInfo.LogPortalName = PortalSettings.PortalName;
            objEventLogInfo.LogUserID = UserId;
            objEventLogInfo.LogUserName = portalSecurity.InputFilter(txtUsername.Text,
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

        #endregion

    }
}