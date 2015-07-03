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
using System.Web.UI;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Security
{

   
    public partial class PasswordReset : UserModuleBase
    {
        #region Private Members

	    private const int RedirectTimeout = 3000;
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
            _ipAddress = Request.UserHostAddress;

			JavaScript.RequestRegistration(CommonJs.DnnPlugins);
			ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
			ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
			ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");
			ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

			ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/stylesheets/dnn.PasswordStrength.css");

            if (PortalSettings.LoginTabId != -1 && PortalSettings.ActiveTab.TabID != PortalSettings.LoginTabId)
            {
                Response.Redirect(Globals.NavigateURL(PortalSettings.LoginTabId) + Request.Url.Query);
            }
            cmdChangePassword.Click +=cmdChangePassword_Click;
            
            hlCancel.NavigateUrl = Globals.NavigateURL();

            if (Request.QueryString["resetToken"] != null)
            {
                ResetToken = Request.QueryString["resetToken"];
                
            }

	        var useEmailAsUserName = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false);
			if (useEmailAsUserName)
            {
                valUsername.Text = Localization.GetString("Email.Required", LocalResourceFile);
            }
            else
            {
                valUsername.Text = Localization.GetString("Username.Required", LocalResourceFile);
            }

            if (Request.QueryString["forced"] == "true")
            {
                lblInfo.Text = Localization.GetString("ForcedResetInfo", LocalResourceFile);
            }

			txtUsername.Attributes.Add("data-default",useEmailAsUserName ? LocalizeString("Email") : LocalizeString("Username"));
			txtPassword.Attributes.Add("data-default", LocalizeString("Password"));
			txtConfirmPassword.Attributes.Add("data-default", LocalizeString("Confirm"));
            txtAnswer.Attributes.Add("data-default", LocalizeString("Answer"));
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!string.IsNullOrEmpty(lblHelp.Text) || !string.IsNullOrEmpty(lblInfo.Text))
                resetMessages.Visible = true;

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

			var confirmPasswordOptions = new DnnConfirmPasswordOptions()
			{
				FirstElementSelector = ".password-strength",
				SecondElementSelector = ".password-confirm",
				ContainerSelector = ".dnnPasswordReset",
				UnmatchedCssClass = "unmatched",
				MatchedCssClass = "matched"
			};

			optionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			script = string.Format("dnn.initializePasswordComparer({0});{1}", optionsAsJsonString, Environment.NewLine);

			if (ScriptManager.GetCurrent(Page) != null)
			{
				// respect MS AJAX
				ScriptManager.RegisterStartupScript(Page, GetType(), "ConfirmPassword", script, true);
			}
			else
			{
				Page.ClientScript.RegisterStartupScript(GetType(), "ConfirmPassword", script, true);
			}
        }

        private void cmdChangePassword_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && String.IsNullOrEmpty(txtAnswer.Text))
            {
                var user = UserController.GetUserByName(username);
                if (user != null)
                {
                    lblQuestion.Text = user.Membership.PasswordQuestion;
                }
                divQA.Visible = true;
                return;
            }

            //1. Check New Password and Confirm are the same
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordMismatch");
                LogFailure(failed);
                lblHelp.Text = failed;
                return;
            }

            if (UserController.ValidatePassword(txtPassword.Text)==false)
            {
                resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordResetFailed");
                LogFailure(failed);
                lblHelp.Text = failed;
                return;    
            }

            //Check New Password is not same as username or banned
            var settings = new MembershipPasswordSettings(User.PortalID);

            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController();
                if (m.FoundBannedPassword(txtPassword.Text) || txtUsername.Text == txtPassword.Text)
                {
                    resetMessages.Visible = true;
                    var failed = Localization.GetString("PasswordResetFailed");
                    LogFailure(failed);
                    lblHelp.Text = failed;
                    return;  
                }
            }

            if (PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false))
            {
                var testUser = UserController.GetUserByEmail(PortalId, username); // one additonal call to db to see if an account with that email actually exists
                if (testUser != null)
                {
                    username = testUser.Username; //we need the username of the account in order to change the password in the next step
                }
            }
            string errorMessage;

            string answer = String.Empty;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                answer = txtAnswer.Text;
            }

            if (UserController.ChangePasswordByToken(PortalSettings.PortalId, username, txtPassword.Text, answer, ResetToken, out errorMessage) == false)
            {
                resetMessages.Visible = true;
                var failed = errorMessage;
                LogFailure(failed);
                lblHelp.Text = failed;
            }
            else
            {
                //check user has a valid profile
                var user = UserController.GetUserByName(PortalSettings.PortalId, username);
                var validStatus = UserController.ValidateUser(user, PortalSettings.PortalId, false);
                if (validStatus == UserValidStatus.UPDATEPROFILE)
                {
                    LogSuccess();
                    ViewState.Add("PageNo", 3);
                    Response.Redirect(Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Login"));
                }
                else
                {
                    //Log user in to site
                    LogSuccess();
                    var loginStatus = UserLoginStatus.LOGIN_FAILURE;
                    UserController.UserLogin(PortalSettings.PortalId, username, txtPassword.Text, "", "", "", ref loginStatus, false);
                    RedirectAfterLogin();
                }            
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

			AddModuleMessage("ChangeSuccessful", ModuleMessage.ModuleMessageType.GreenSuccess, true);
	        resetMessages.Visible = divPassword.Visible = false;

			//redirect page after 5 seconds
	        var script = string.Format("setTimeout(function(){{location.href = '{0}';}}, {1});", redirectURL, RedirectTimeout);
			if (ScriptManager.GetCurrent(Page) != null)
			{
				// respect MS AJAX
				ScriptManager.RegisterStartupScript(Page, GetType(), "ChangePasswordSuccessful", script, true);
			}
			else
			{
				Page.ClientScript.RegisterStartupScript(GetType(), "ChangePasswordSuccessful", script, true);
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
            var log = new LogInfo
            {
                LogPortalID = PortalSettings.PortalId,
                LogPortalName = PortalSettings.PortalName,
                LogUserID = UserId
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
            log.AddProperty("IP", _ipAddress);
            
            LogController.Instance.AddLog(log);
        }

        #endregion

    }
}