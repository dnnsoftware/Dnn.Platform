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
using System.Linq;
using System.Net;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Authentication
{

	/// <summary>
	/// The Login AuthenticationLoginBase is used to provide a login for a registered user
	/// portal.
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// 	[cnurse]	9/24/2004	Updated to reflect design changes for Help, 508 support
	///                       and localisation
	///     [cnurse]    08/07/2007  Ported to new Authentication Framework
	/// </history>
	public partial class Login : AuthenticationLoginBase
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Login));

		#region Protected Properties

		/// <summary>
		/// Gets whether the Captcha control is used to validate the login
		/// </summary>
		/// <history>
		/// 	[cnurse]	03/17/2006  Created
		///     [cnurse]    07/03/2007  Moved from Sign.ascx.vb
		/// </history>
		protected bool UseCaptcha
		{
			get
			{
				return AuthenticationConfig.GetConfig(PortalId).UseCaptcha;
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Check if the Auth System is Enabled (for the Portal)
		/// </summary>
		/// <remarks></remarks>
		/// <history>
		/// 	[cnurse]	07/04/2007	Created
		/// </history>
		public override bool Enabled
		{
			get
			{
				return AuthenticationConfig.GetConfig(PortalId).Enabled;
			}
		}

		#endregion

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			cmdLogin.Click += OnLoginClick;

			ClientAPI.RegisterKeyCapture(Parent, cmdLogin, 13);

            if (PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration)
            {
                liRegister.Visible = false;
            }
            lblLogin.Text = Localization.GetSystemMessage(PortalSettings, "MESSAGE_LOGIN_INSTRUCTIONS");

            var returnUrl = Globals.NavigateURL();
            string url;
            if (PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
            {
                if (!string.IsNullOrEmpty(UrlUtils.ValidReturnUrl(Request.QueryString["returnurl"])))
                {
                    returnUrl = Request.QueryString["returnurl"];
                }
                returnUrl = HttpUtility.UrlEncode(returnUrl);

                url = Globals.RegisterURL(returnUrl, Null.NullString);
                registerLink.NavigateUrl = url;
                if (PortalSettings.EnablePopUps && PortalSettings.RegisterTabId == Null.NullInteger
                    && !HasSocialAuthenticationEnabled())
                {
                    registerLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, PortalSettings, true, false, 600, 950));
                }
            }
            else
            {
                registerLink.Visible = false;
            }

            //see if the portal supports persistant cookies
            chkCookie.Visible = Host.RememberCheckbox;

            url = Globals.NavigateURL("SendPassword", "returnurl=" + returnUrl);
            passwordLink.NavigateUrl = url;
            if (PortalSettings.EnablePopUps)
            {
                passwordLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, PortalSettings, true, false, 300, 650));
            }


            if (!IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["verificationcode"]) && 
                    PortalSettings.UserRegistration == (int) Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    if (Request.IsAuthenticated)
                    {
                        Controls.Clear();
                    }

                    var verificationCode = Request.QueryString["verificationcode"];


                    try
                    {
                        UserController.VerifyUser(verificationCode.Replace(".", "+").Replace("-", "/").Replace("_", "="));

                        var redirectTabId = Convert.ToInt32(GetSetting(PortalId, "Redirect_AfterRegistration"));

	                    if (Request.IsAuthenticated)
	                    {
                            Response.Redirect(Globals.NavigateURL(redirectTabId > 0 ? redirectTabId : PortalSettings.HomeTabId, string.Empty, "VerificationSuccess=true"), true);
	                    }
	                    else
	                    {
                            if (redirectTabId > 0)
                            {
                                var redirectUrl = Globals.NavigateURL(redirectTabId, string.Empty, "VerificationSuccess=true");
                                redirectUrl = redirectUrl.Replace(Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias), string.Empty);
                                Response.Cookies.Add(new HttpCookie("returnurl", redirectUrl));
                            }

		                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("VerificationSuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
	                    }
                    }
                    catch (UserAlreadyVerifiedException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserAlreadyVerified", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                    catch (InvalidVerificationCodeException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    catch (UserDoesNotExistException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserDoesNotExist", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    catch (Exception)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }

			if (!Request.IsAuthenticated)
			{
				if (Page.IsPostBack == false)
				{
					try
					{
						if (Request.QueryString["username"] != null)
						{
							txtUsername.Text = Request.QueryString["username"];
						}
					}
					catch (Exception ex)
					{
						//control not there 
						Logger.Error(ex);
					}
				}
				try
				{
					Globals.SetFormFocus(string.IsNullOrEmpty(txtUsername.Text) ? txtUsername : txtPassword);
				}
				catch (Exception ex)
				{
					//Not sure why this Try/Catch may be necessary, logic was there in old setFormFocus location stating the following
					//control not there or error setting focus
					Logger.Error(ex);
				}
			}

		    var registrationType = PortalController.GetPortalSettingAsInteger("Registration_RegistrationFormType", PortalId, 0);
		    bool useEmailAsUserName;
            if (registrationType == 0)
            {
                useEmailAsUserName = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false);
            }
            else
            {
                var registrationFields = PortalController.GetPortalSetting("Registration_RegistrationFields", PortalId, String.Empty);
                useEmailAsUserName = !registrationFields.Contains("Username");
            }

		    plUsername.Text = LocalizeString(useEmailAsUserName ? "Email" : "Username");
		    divCaptcha1.Visible = UseCaptcha;
			divCaptcha2.Visible = UseCaptcha;
		}

		private void OnLoginClick(object sender, EventArgs e)
		{
			if ((UseCaptcha && ctlCaptcha.IsValid) || !UseCaptcha)
			{
				var loginStatus = UserLoginStatus.LOGIN_FAILURE;
                string userName = new PortalSecurity().InputFilter(txtUsername.Text,
                                                         PortalSecurity.FilterFlag.NoScripting |
                                                         PortalSecurity.FilterFlag.NoAngleBrackets |
                                                         PortalSecurity.FilterFlag.NoMarkup);
                var objUser = UserController.ValidateUser(PortalId, userName, txtPassword.Text, "DNN", string.Empty, PortalSettings.PortalName, IPAddress, ref loginStatus);
				var authenticated = Null.NullBoolean;
				var message = Null.NullString;
				if (loginStatus == UserLoginStatus.LOGIN_USERNOTAPPROVED)
				{
				    message = "UserNotAuthorized";
				}
				else
				{
					authenticated = (loginStatus != UserLoginStatus.LOGIN_FAILURE);
				}
				
				//Raise UserAuthenticated Event
                var eventArgs = new UserAuthenticatedEventArgs(objUser, userName, loginStatus, "DNN")
				                    {
				                        Authenticated = authenticated, 
                                        Message = message,
                                        RememberMe = chkCookie.Checked
				                    };
				OnUserAuthenticated(eventArgs);
			}
		}

        private bool HasSocialAuthenticationEnabled()
        {
            return (from a in AuthenticationController.GetEnabledAuthenticationServices()
                    let enabled = (a.AuthenticationType == "Facebook"
                                     || a.AuthenticationType == "Google"
                                     || a.AuthenticationType == "Live"
                                     || a.AuthenticationType == "Twitter")
                                  ? PortalController.GetPortalSettingAsBoolean(a.AuthenticationType + "_Enabled", PortalSettings.PortalId, false)
                                  : !string.IsNullOrEmpty(a.LoginControlSrc) && (LoadControl("~/" + a.LoginControlSrc) as AuthenticationLoginBase).Enabled
                    where a.AuthenticationType != "DNN" && enabled
                    select a).Any();
        }
		
		#endregion

	}
}