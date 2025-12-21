// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.HttpModules.Services;
    using DotNetNuke.Services.Mobile;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IHttpModule"/> for mobile redirection.</summary>
    public class MobileRedirectModule : IHttpModule
    {
        private static readonly List<string> SpecialPages = ["/login.aspx", "/register.aspx", "/terms.aspx", "/privacy.aspx", "/login", "/register", "/terms", "/privacy",];
        private static readonly Regex MvcServicePath = new Regex(@"DesktopModules/MVC/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly IRedirectionController redirectionController;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="MobileRedirectModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IRedirectionController. Scheduled removal in v12.0.0.")]
        public MobileRedirectModule()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MobileRedirectModule"/> class.</summary>
        /// <param name="redirectionController">The redirection controller.</param>
        /// <param name="portalController">The portal controller.</param>
        public MobileRedirectModule(IRedirectionController redirectionController, IPortalController portalController)
        {
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.redirectionController = redirectionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRedirectionController>();
        }

        /// <summary>Gets the HttpModule module name.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ModuleName => "MobileRedirectModule";

        /// <inheritdoc/>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += this.OnBeginRequest;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>Handle the <see cref="HttpApplication.BeginRequest"/> event.</summary>
        /// <param name="s">The sender.</param>
        /// <param name="e">The event args.</param>
        public void OnBeginRequest(object s, EventArgs e)
        {
            var app = (HttpApplication)s;
            var portalSettings = this.portalController.GetCurrentSettings();

            // First check if we are upgrading/installing
            var rawUrl = app.Request.RawUrl;
            if (!Initialize.ProcessHttpModule(app.Request, false, false)
                    || app.Request.HttpMethod == "POST"
                    || ServicesModule.ServiceApi.IsMatch(rawUrl)
                    || MvcServicePath.IsMatch(rawUrl)
                    || this.IsSpecialPage(rawUrl)
                    || (portalSettings != null && !IsRedirectAllowed(rawUrl, app, portalSettings)))
            {
                return;
            }

            // Check if redirection has been disabled for the session
            // This method inspects cookie and query string. It can also setup / clear cookies.
            if (this.redirectionController == null ||
                TabController.CurrentPage == null ||
                string.IsNullOrEmpty(app.Request.UserAgent) ||
                !this.redirectionController.IsRedirectAllowedForTheSession(app))
            {
                return;
            }

            var redirectUrl = this.redirectionController.GetRedirectUrl(app.Request.UserAgent);
            if (string.IsNullOrEmpty(redirectUrl))
            {
                return;
            }

            // append the query string from original url
            var idx = rawUrl.IndexOf("?", StringComparison.Ordinal);
            var queryString = idx >= 0 ? rawUrl.Substring(idx + 1) : string.Empty;
            if (!string.IsNullOrEmpty(queryString))
            {
                redirectUrl = string.Concat(redirectUrl, redirectUrl.Contains("?") ? "&" : "?", queryString);
            }

            app.Response.Redirect(redirectUrl);
        }

        private static bool IsRedirectAllowed(string url, HttpApplication app, IPortalSettings portalSettings)
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

            var portalSettings = this.portalController.GetCurrentSettings();
            if (portalSettings == null)
            {
                return true;
            }

            var alias = ((IPortalAliasInfo)PortalSettings.Current.PortalAlias).HttpAlias.ToLowerInvariant();
            idx = alias.IndexOf("/", StringComparison.Ordinal);
            if (idx >= 0)
            {
                tabPath = tabPath.Replace(alias.Substring(idx), string.Empty);
            }

            return SpecialPages.Contains(tabPath);
        }
    }
}
