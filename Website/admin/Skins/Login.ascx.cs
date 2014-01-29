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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// <history>
    /// 	[smcculloch]10/15/2004	Fixed Logoff Link for FriendlyUrls
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Login : SkinObjectBase
    {

        public Login()
        {
            LegacyMode = true;
        }

		#region Private Members

        private const string MyFileName = "Login.ascx";
		#endregion

		#region Public Members
		
        public string Text { get; set; }

        public string CssClass { get; set; }

        public string LogoffText { get; set; }

        /// <summary>
        /// Set this to false in the skin to take advantage of the enhanced markup
        /// </summary>
        public bool LegacyMode { get; set; }

		#endregion

		#region Event Handlers

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			Visible = !PortalSettings.HideLoginControl || Request.IsAuthenticated;
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

	        if (Visible)
	        {
		        try
		        {
			        if (LegacyMode)
			        {
				        loginLink.Visible = true;
				        loginGroup.Visible = false;
			        }
			        else
			        {
				        loginLink.Visible = false;
				        loginGroup.Visible = true;
			        }

			        if (!String.IsNullOrEmpty(CssClass))
			        {
				        loginLink.CssClass = CssClass;
				        enhancedLoginLink.CssClass = CssClass;
			        }

			        if (Request.IsAuthenticated)
			        {
				        if (!String.IsNullOrEmpty(LogoffText))
				        {
					        if (LogoffText.IndexOf("src=") != -1)
					        {
						        LogoffText = LogoffText.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					        }
					        loginLink.Text = LogoffText;
					        enhancedLoginLink.Text = LogoffText;
				        }
				        else
				        {
					        loginLink.Text = Localization.GetString("Logout", Localization.GetResourceFile(this, MyFileName));
					        enhancedLoginLink.Text = loginLink.Text;
					        loginLink.ToolTip = loginLink.Text;
					        enhancedLoginLink.ToolTip = loginLink.Text;
				        }
				        loginLink.NavigateUrl = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Logoff");
				        enhancedLoginLink.NavigateUrl = loginLink.NavigateUrl;
			        }
			        else
			        {
				        if (!String.IsNullOrEmpty(Text))
				        {
					        if (Text.IndexOf("src=") != -1)
					        {
						        Text = Text.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					        }
					        loginLink.Text = Text;
					        enhancedLoginLink.Text = Text;
				        }
				        else
				        {
					        loginLink.Text = Localization.GetString("Login", Localization.GetResourceFile(this, MyFileName));
					        enhancedLoginLink.Text = loginLink.Text;
					        loginLink.ToolTip = loginLink.Text;
					        enhancedLoginLink.ToolTip = loginLink.Text;
				        }

				        string returnUrl = HttpContext.Current.Request.RawUrl;
				        if (returnUrl.IndexOf("?returnurl=") != -1)
				        {
					        returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
				        }
				        returnUrl = HttpUtility.UrlEncode(returnUrl);

				        loginLink.NavigateUrl = Globals.LoginURL(returnUrl, (Request.QueryString["override"] != null));
				        enhancedLoginLink.NavigateUrl = loginLink.NavigateUrl;

				        if (PortalSettings.EnablePopUps && PortalSettings.LoginTabId == Null.NullInteger && !HasSocialAuthenticationEnabled())
				        {
					        //To avoid duplicated encodes of URL
					        var clickEvent = "return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(loginLink.NavigateUrl), this, PortalSettings, true, false, 300, 650);
					        loginLink.Attributes.Add("onclick", clickEvent);
					        enhancedLoginLink.Attributes.Add("onclick", clickEvent);
				        }
			        }
		        }
		        catch (Exception exc)
		        {
			        Exceptions.ProcessModuleLoadException(this, exc);
		        }
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