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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.HttpModules.Config;
using DotNetNuke.HttpModules.Services;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.EventQueue;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;

#endregion

namespace DotNetNuke.HttpModules
{
    public class MobileRedirectModule : IHttpModule
    {
        private IRedirectionController _redirectionController;
        private IList<string> _specialPages = new List<string> { "/login.aspx", "/register.aspx", "/terms.aspx", "/privacy.aspx", "/login", "/register", "/terms", "/privacy" };
        public string ModuleName
        {
            get
            {
                return "MobileRedirectModule";
            }
        }

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            _redirectionController = new RedirectionController();
            application.BeginRequest += OnBeginRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        public void OnBeginRequest(object s, EventArgs e)
        {
            var app = (HttpApplication)s;
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            
            //First check if we are upgrading/installing
            if (app.Request.Url.LocalPath.ToLower().EndsWith("install.aspx")
                    || app.Request.Url.LocalPath.ToLower().Contains("upgradewizard.aspx")
                    || app.Request.Url.LocalPath.ToLower().Contains("installwizard.aspx")
                    || app.Request.Url.LocalPath.ToLower().EndsWith("captcha.aspx")
                    || app.Request.Url.LocalPath.ToLower().EndsWith("scriptresource.axd")
                    || app.Request.Url.LocalPath.ToLower().EndsWith("webresource.axd")
                    || app.Request.Url.LocalPath.ToLower().EndsWith("sitemap.aspx")
                    || app.Request.Url.LocalPath.ToLower().EndsWith(".asmx")
                    || app.Request.Url.LocalPath.ToLower().EndsWith(".ashx")
                    || app.Request.Url.LocalPath.ToLower().EndsWith(".svc")
                    || app.Request.HttpMethod == "POST" 
                    || ServicesModule.ServiceApi.IsMatch(app.Context.Request.RawUrl)
					|| IsSpecialPage(app.Request.RawUrl)
                    || (portalSettings != null && !IsRedirectAllowed(app.Request.RawUrl, app, portalSettings)))
            {
                return;
            } 
            if (_redirectionController != null)
            {
                if (portalSettings != null && portalSettings.ActiveTab != null)
                {
                    if (app != null && app.Request != null && !string.IsNullOrEmpty(app.Request.UserAgent))
                    {
						//Check if redirection has been disabled for the session
                        //This method inspects cookie and query string. It can also setup / clear cookies.
                        if (!_redirectionController.IsRedirectAllowedForTheSession(app))
						{                         
							return;
						}

                        string redirectUrl = _redirectionController.GetRedirectUrl(app.Request.UserAgent);
                        if (!string.IsNullOrEmpty(redirectUrl))
                        {
							//append thr query string from original url
	                        var queryString = app.Request.RawUrl.Contains("?") ? app.Request.RawUrl.Substring(app.Request.RawUrl.IndexOf("?") + 1) : string.Empty;
	                        if (!string.IsNullOrEmpty(queryString))
	                        {
		                        redirectUrl = string.Format("{0}{1}{2}", redirectUrl, redirectUrl.Contains("?") ? "&" : "?", queryString);
	                        }
	                        app.Response.Redirect(redirectUrl);
                        }
                    }
                }
            }
        }

        private bool IsRedirectAllowed(string url, HttpApplication app, PortalSettings portalSettings)
        {
            var urlAction = new UrlAction(app.Request);
            urlAction.SetRedirectAllowed(url, new FriendlyUrlSettings(portalSettings.PortalId));
            return urlAction.RedirectAllowed;
        }

        private bool IsSpecialPage(string url)
		{
			var tabPath = url.ToLowerInvariant();
			if (tabPath.Contains("?"))
			{
				tabPath = tabPath.Substring(0, tabPath.IndexOf("?"));
			}

			var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
			if (portalSettings == null)
			{
				return true;
			}

			var alias = PortalController.Instance.GetCurrentPortalSettings().PortalAlias.HTTPAlias.ToLowerInvariant();
			if (alias.Contains("/"))
			{
				tabPath = tabPath.Replace(alias.Substring(alias.IndexOf("/")), string.Empty);
			}
			return _specialPages.Contains(tabPath);
		}
    }
}