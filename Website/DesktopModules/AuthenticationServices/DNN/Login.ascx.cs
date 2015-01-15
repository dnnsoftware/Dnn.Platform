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
		
		protected new string RedirectURL
        {
            get
            {
                string _RedirectURL = "";

				if (PortalSettings.Registration.RedirectAfterRegistration > 0) //redirect to after registration page
                {
					_RedirectURL = Globals.NavigateURL(PortalSettings.Registration.RedirectAfterRegistration);
                }
                else
                {

					if (PortalSettings.Registration.RedirectAfterRegistration <= 0)
                {
                    if (Request.QueryString["returnurl"] != null)
                    {
                        //return to the url passed to register
                        _RedirectURL = HttpUtility.UrlDecode(Request.QueryString["returnurl"]);
                        //redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
                        if (_RedirectURL.Contains("://") &&
                            !_RedirectURL.StartsWith(Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias),
                                StringComparison.InvariantCultureIgnoreCase))
                        {
                            _RedirectURL = "";
                        }
                        if (_RedirectURL.Contains("?returnurl"))
                        {
                            string baseURL = _RedirectURL.Substring(0,
                                _RedirectURL.IndexOf("?returnurl", StringComparison.Ordinal));
                            string returnURL =
                                _RedirectURL.Substring(_RedirectURL.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                            _RedirectURL = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                        }
                    }
                    if (String.IsNullOrEmpty(_RedirectURL))
                    {
                        //redirect to current page 
                        _RedirectURL = Globals.NavigateURL();
                    }
                }
                else //redirect to after registration page
                {
					_RedirectURL = Globals.NavigateURL(PortalSettings.Registration.RedirectAfterRegistration);
                }
                }

                return _RedirectURL;
            }
        
		}

		#endregion

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			cmdLogin.Click += OnLoginClick;
			
			cmdCancel.Click += OnCancelClick;

			ClientAPI.RegisterKeyCapture(Parent, cmdLogin, 13);

            if (PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration)
            {
                liRegister.Visible = false;
            }
            lblLogin.Text = Localization.GetSystemMessage(PortalSettings, "MESSAGE_LOGIN_INSTRUCTIONS");

            if (!string.IsNullOrEmpty(Response.Cookies["USERNAME_CHANGED"].Value))
            {
                txtUsername.Text = Response.Cookies["USERNAME_CHANGED"].Value;
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetSystemMessage(PortalSettings, "MESSAGE_USERNAME_CHANGED_INSTRUCTIONS"), ModuleMessage.ModuleMessageType.BlueInfo);
            }

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



            // no need to show password link if feature is disabled, let's check this first
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                url = Globals.NavigateURL("SendPassword", "returnurl=" + returnUrl);
                passwordLink.NavigateUrl = url;
                if (PortalSettings.EnablePopUps)
                {
                    passwordLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, PortalSettings, true, false, 300, 650));
                }
            }
            else
            {
                passwordLink.Visible = false;
            }


            if (!IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["verificationcode"]) && PortalSettings.UserRegistration == (int) Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    if (Request.IsAuthenticated)
                    {
                        Controls.Clear();
                    }

                    var verificationCode = Request.QueryString["verificationcode"];


                    try
                    {
                        UserController.VerifyUser(verificationCode.Replace(".", "+").Replace("-", "/").Replace("_", "="));

						var redirectTabId = PortalSettings.Registration.RedirectAfterRegistration;

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
                                Response.Cookies.Add(new HttpCookie("returnurl", redirectUrl) { Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/") });
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
				if (!Page.IsPostBack)
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

			var registrationType = PortalSettings.Registration.RegistrationFormType;
		    bool useEmailAsUserName;
            if (registrationType == 0)
            {
				useEmailAsUserName = PortalSettings.Registration.UseEmailAsUserName;
            }
            else
            {
				var registrationFields = PortalSettings.Registration.RegistrationFields;
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

                //DNN-6093
                //check if we use email address here rather than username
                if(PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false))
                {
                    var testUser = UserController.GetUserByEmail(PortalId, userName); // one additonal call to db to see if an account with that email actually exists
                    if(testUser != null)
                    {
                        userName = testUser.Username; //we need the username of the account in order to authenticate in the next step
                    }
                }

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

                if (loginStatus != UserLoginStatus.LOGIN_FAILURE && PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false))
                {
                    //make sure internal username matches current e-mail address
                    if (objUser.Username.ToLower() != objUser.Email.ToLower())
                    {
                        UserController.ChangeUsername(objUser.UserID, objUser.Email);
                    }

                    Response.Cookies.Remove("USERNAME_CHANGED");
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
		
		private void OnCancelClick(object sender, EventArgs e)
		{
			Response.Redirect(RedirectURL, true);
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