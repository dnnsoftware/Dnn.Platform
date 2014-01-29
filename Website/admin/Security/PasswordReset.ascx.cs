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
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;


#endregion

namespace DotNetNuke.Modules.Admin.Security
{

   
    public partial class PasswordReset : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PasswordReset));

        #region Private Members

        private string _ipAddress;

        private string ResetToken
        {
            get
            {
                return ViewState["ResetToken"] != null ? Request.QueryString["resetToken"] : String.Empty;
            }
            set
            {
                ViewState.Add("ResetToken", value);
            }
        }

        #endregion

       
        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdChangePassword.Click +=cmdChangePassword_Click;
            
            hlCancel.NavigateUrl = Globals.NavigateURL();

            if (Request.QueryString["resetToken"] != null)
            {
                ResetToken = Request.QueryString["resetToken"];
                
            }

        }

        private void cmdChangePassword_Click(object sender, EventArgs e)
        {
            //1. Check New Password and Confirm are the same
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordMismatch");
                LogFailure(failed);
                lblHelp.Text = failed;
                return;
            }

            if (UserController.ChangePasswordByToken(PortalSettings.PortalId, txtUsername.Text, txtPassword.Text, ResetToken) == false)
            {
                resetMessages.Visible = true;
                var failed = Localization.GetString("FailedAttempt", LocalResourceFile);
                LogFailure(failed);
                lblHelp.Text = failed;
            }
            else
            {
                //Log user in to site
                LogSuccess();
                var loginStatus = UserLoginStatus.LOGIN_FAILURE;
                UserController.UserLogin(PortalSettings.PortalId, txtUsername.Text, txtPassword.Text, "", "", "", ref loginStatus, false);
                RedirectAfterLogin();
            }           
        }

        protected void RedirectAfterLogin()
        {
            var redirectURL = "";

            var setting = GetSetting(PortalId, "Redirect_AfterLogin");

            if (Convert.ToInt32(setting) == Null.NullInteger)
            {
                if (Request.QueryString["returnurl"] != null)
                {
                    //return to the url passed to signin
                    redirectURL = HttpUtility.UrlDecode(Request.QueryString["returnurl"]);
                    //redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
                    if (redirectURL.Contains("://"))
                    {
                        redirectURL = "";
                    }
                }
                if (Request.Cookies["returnurl"] != null)
                {
                    //return to the url passed to signin
                    redirectURL = HttpUtility.UrlDecode(Request.Cookies["returnurl"].Value);
                    //redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
                    if (redirectURL.Contains("://"))
                    {
                        redirectURL = "";
                    }
                }
                if (String.IsNullOrEmpty(redirectURL))
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
            else //redirect to after login page
            {
                redirectURL = Globals.NavigateURL(Convert.ToInt32(setting));
            }
            Response.Redirect(redirectURL);
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
        //    objEventLogInfo.LogUserName = portalSecurity.InputFilter(txtUsername.Text,
          //                                                           PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
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