// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;
using DotNetNuke.HttpModules.Services;
using DotNetNuke.Services.Mobile;

#endregion

namespace DotNetNuke.HttpModules
{
    public class MobileRedirectModule : IHttpModule
    {
        private IRedirectionController _redirectionController;
        private readonly IList<string> _specialPages = new List<string> { "/login.aspx", "/register.aspx", "/terms.aspx", "/privacy.aspx", "/login", "/register", "/terms", "/privacy" };
        private readonly Regex MvcServicePath = new Regex(@"DesktopModules/MVC/", RegexOptions.Compiled);
        public string ModuleName => "MobileRedirectModule";

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
            var rawUrl = app.Request.RawUrl;
            if (!Initialize.ProcessHttpModule(app.Request, false, false)
                    || app.Request.HttpMethod == "POST"
                    || ServicesModule.ServiceApi.IsMatch(rawUrl) 
                    || MvcServicePath.IsMatch(rawUrl)
                    || IsSpecialPage(rawUrl)
                    || (portalSettings != null && !IsRedirectAllowed(rawUrl, app, portalSettings)))
            {
                return;
            }

            //Check if redirection has been disabled for the session
            //This method inspects cookie and query string. It can also setup / clear cookies.
            if (_redirectionController != null &&
                portalSettings?.ActiveTab != null &&
                !string.IsNullOrEmpty(app.Request.UserAgent) &&
                _redirectionController.IsRedirectAllowedForTheSession(app))
            {
                var redirectUrl = _redirectionController.GetRedirectUrl(app.Request.UserAgent);
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    //append the query string from original url
                    var idx = rawUrl.IndexOf("?", StringComparison.Ordinal);
                    var queryString = idx >= 0 ? rawUrl.Substring(idx + 1) : string.Empty;
                    if (!string.IsNullOrEmpty(queryString))
                    {
                        redirectUrl = string.Concat(redirectUrl, redirectUrl.Contains("?") ? "&" : "?", queryString);
                    }
                    app.Response.Redirect(redirectUrl);
                }
            }
        }

        private static bool IsRedirectAllowed(string url, HttpApplication app, PortalSettings portalSettings)
        {
            var urlAction = new UrlAction(app.Request);
            urlAction.SetRedirectAllowed(url, new FriendlyUrlSettings(portalSettings.PortalId));
            return urlAction.RedirectAllowed;
        }

        private bool IsSpecialPage(string url)
        {
            var tabPath = url.ToLowerInvariant();
            var idx = tabPath.IndexOf("?", StringComparison.Ordinal);
            if (idx >= 0)
            {
                tabPath = tabPath.Substring(0, idx);
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                return true;
            }

            var alias = PortalController.Instance.GetCurrentPortalSettings().PortalAlias.HTTPAlias.ToLowerInvariant();
            idx = alias.IndexOf("/", StringComparison.Ordinal);
            if (idx >= 0)
            {
                tabPath = tabPath.Replace(alias.Substring(idx), string.Empty);
            }
            return _specialPages.Contains(tabPath);
        }
    }
}
