// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Policy;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        private const string LoginFileName = "Login.ascx";

        public static IHtmlString Login(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string text = "", string logoffText = "", bool legacyMode = true, bool showInErrorPage = false)
        {
            var navigationManager = helper.ViewData.Model.NavigationManager;
            //TODO: CSP - enable when CSP implementation is ready
            var nonce = string.Empty; //helper.ViewData.Model.ContentSecurityPolicy.Nonce;
            var portalSettings = PortalSettings.Current;
            var request = HttpContext.Current.Request;

            var isVisible = (!portalSettings.HideLoginControl || request.IsAuthenticated)
                        && (!portalSettings.InErrorPageRequest() || showInErrorPage);

            if (!isVisible)
            {
                return MvcHtmlString.Empty;
            }

            if (legacyMode)
            {
                return BuildLegacyLogin(text, cssClass, logoffText, nonce, navigationManager);
            }

            return BuildEnhancedLogin(text, cssClass, logoffText, nonce, navigationManager);
        }

        private static MvcHtmlString BuildLegacyLogin(string text, string cssClass, string logoffText, string nonce, INavigationManager navigationManager)
        {
            var link = new TagBuilder("a");
            ConfigureLoginLink(link, text, cssClass, logoffText, out string loginScript, nonce, navigationManager);
            return new MvcHtmlString(link.ToString() + loginScript);
        }

        private static MvcHtmlString BuildEnhancedLogin(string text, string cssClass, string logoffText, string nonce, INavigationManager navigationManager)
        {
            var container = new TagBuilder("div");
            container.AddCssClass("loginGroup");

            var link = new TagBuilder("a");
            link.AddCssClass("secondaryActionsList");
            ConfigureLoginLink(link, text, cssClass, logoffText, out string loginScript, nonce, navigationManager);

            container.InnerHtml = link.ToString();
            return new MvcHtmlString(container.ToString() + loginScript);
        }

        private static void ConfigureLoginLink(TagBuilder link, string text, string cssClass, string logoffText, out string loginScript, string nonce, INavigationManager navigationManager)
        {
            var portalSettings = PortalSettings.Current;
            var request = HttpContext.Current.Request;

            loginScript = string.Empty;

            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }
            else
            {
                link.AddCssClass("SkinObject");
            }

            link.Attributes["rel"] = "nofollow";

            if (request.IsAuthenticated)
            {
                var displayText = !string.IsNullOrEmpty(logoffText)
                    ? logoffText.Replace("src=\"", "src=\"" + portalSettings.ActiveTab.SkinPath)
                    : Localization.GetString("Logout", GetSkinsResourceFile(LoginFileName));

                link.SetInnerText(displayText);
                link.Attributes["title"] = displayText;
                link.Attributes["href"] = navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Logoff");
            }
            else
            {
                var displayText = !string.IsNullOrEmpty(text)
                    ? text.Replace("src=\"", "src=\"" + portalSettings.ActiveTab.SkinPath)
                    : Localization.GetString("Login", GetSkinsResourceFile(LoginFileName));

                link.SetInnerText(displayText);
                link.Attributes["title"] = displayText;

                string returnUrl = request.RawUrl;
                if (returnUrl.IndexOf("?returnurl=", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl=", StringComparison.OrdinalIgnoreCase));
                }

                returnUrl = HttpUtility.UrlEncode(returnUrl);

                var loginUrl = Globals.LoginURL(returnUrl, request.QueryString["override"] != null);
                link.Attributes["href"] = loginUrl;

                // link.Attributes["data-url"] = loginUrl;
                link.Attributes["class"] += " dnnLoginLink";

                loginScript = GetLoginScript(loginUrl, nonce);
            }
        }

        private static string GetLoginScript(string loginUrl, string nonce)
        {
            var portalSettings = PortalSettings.Current;
            var request = HttpContext.Current.Request;

            if (!request.IsAuthenticated)
            {
                var nonceAttribute = string.Empty;
                if (!string.IsNullOrEmpty(nonce))
                {
                    nonceAttribute = $"nonce=\"{nonce}\"";
                }
                var script = string.Format(
                    @"
                    <script {0} >
                    (function() {{
                        var loginLinks = document.querySelectorAll('.dnnLoginLink');
                        if (loginLinks.length > 0) {{
                            loginLinks.forEach(function(link) {{
                                link.addEventListener('click', function(e) {{
                                    e.preventDefault();
                                    var url = this.getAttribute('href');
                                    
                                    if (!navigator.userAgent.match(/MSIE 8.0/)) {{
                                        this.disabled = true;
                                    }}
                                ",
                    nonceAttribute);

                if (portalSettings.EnablePopUps &&
                    portalSettings.LoginTabId == Null.NullInteger &&
                    !AuthenticationController.HasSocialAuthenticationEnabled(null))
                {
                    script += UrlUtils.PopUpUrl(loginUrl, null, portalSettings, true, false, 300, 650);
                }
                else
                {
                    script += "window.location = url;";
                }

                script += @"
                                return false;
                                });
                            });
                        }
                    });
                    </script>";

                return script;
            }

            return string.Empty;
        }
    }
}
