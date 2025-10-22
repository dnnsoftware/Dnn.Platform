// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcWebsite.Controllers
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using Dnn.EditBar.UI.Mvc;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Exceptions;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;

    public class DefaultController : DnnPageController
    {
        private static readonly Regex HeaderTextRegex = new Regex(
            "<meta([^>])+name=('|\")robots('|\")",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly INavigationManager navigationManager;
        private readonly IPageModelFactory pageModelFactory;

        public DefaultController(INavigationManager navigationManager, IPageModelFactory pageModelFactory)
        {
            this.navigationManager = navigationManager;
            this.pageModelFactory = pageModelFactory;
        }

        public ActionResult Page(int tabid, string language)
        {
            //TODO: CSP - enable when CSP implementation is ready
            /*
            this.HttpContext.Items.Add("CSP-NONCE", this.contentSecurityPolicy.Nonce);

            this.contentSecurityPolicy.DefaultSource.AddSelf();
            this.contentSecurityPolicy.ImgSource.AddSelf();
            this.contentSecurityPolicy.FontSource.AddSelf();
            this.contentSecurityPolicy.StyleSource.AddSelf();
            this.contentSecurityPolicy.FrameSource.AddSelf();
            this.contentSecurityPolicy.FormAction.AddSelf();
            this.contentSecurityPolicy.FrameAncestors.AddSelf();
            this.contentSecurityPolicy.ObjectSource.AddNone();
            this.contentSecurityPolicy.BaseUriSource.AddNone();
            this.contentSecurityPolicy.ScriptSource.AddNonce(this.contentSecurityPolicy.Nonce);
            this.contentSecurityPolicy.AddReportTo("csp-endpoint");
            this.contentSecurityPolicy.AddReportEndpoint("csp-endpoint", this.Request.Url.Scheme + "://" + this.Request.Url.Host + "/DesktopModules/Csp/Report");

            if (this.Request.IsAuthenticated)
            {
                this.contentSecurityPolicy.FrameSource.AddHost("https://dnndocs.com").AddHost("https://docs.dnncommunity.org");
            }
            */

            // There could be a pending installation/upgrade process
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                Exceptions.ProcessHttpException(new HttpException(503, Localization.GetString("SiteAccessedWhileInstallationWasInProgress.Error", Localization.GlobalResourceFile)));
            }

            var user = this.PortalSettings.UserInfo;

            if (PortalSettings.Current.UserId > 0)
            {
                // TODO: should we do this? It creates a dependency towards the PersonaBar which is probably not a great idea
                MvcContentEditorManager.CreateManager(this);
            }

            // Configure the ActiveTab with Skin/Container information
            PortalSettingsController.Instance().ConfigureActiveTab(this.PortalSettings);
            PageModel model = this.pageModelFactory.CreatePageModel(this);
            try
            {
                this.InitializePage(model);
            }
            catch (MvcPageException ex)
            {
                if (string.IsNullOrEmpty(ex.RedirectUrl))
                {
                    return this.HttpNotFound(ex.Message);
                }
                else
                {
                    return this.Redirect(ex.RedirectUrl);
                }
            }

            // DotNetNuke.Framework.JavaScriptLibraries.MvcJavaScript.Register(this.ControllerContext);
            model.ClientVariables = MvcClientAPI.GetClientVariableList();
            model.StartupScripts = MvcClientAPI.GetClientStartupScriptList();

            // Register the scripts and stylesheets
            this.RegisterScriptsAndStylesheets(model);

            return this.View(model.Skin.RazorFile, "Layout", model);
        }

        private void RegisterScriptsAndStylesheets(PageModel page)
        {
            foreach (var styleSheet in page.Skin.RegisteredStylesheets)
            {
                MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, styleSheet.Stylesheet, styleSheet.FileOrder);
            }

            foreach (var pane in page.Skin.Panes)
            {
                foreach (var container in pane.Value.Containers)
                {
                    foreach (var stylesheet in container.Value.RegisteredStylesheets)
                    {
                        MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, stylesheet.Stylesheet, stylesheet.FileOrder);
                    }
                }
            }

            foreach (var script in page.Skin.RegisteredScripts)
            {
                MvcClientResourceManager.RegisterScript(this.ControllerContext, script);
            }
        }

        private void InitializePage(PageModel page)
        {
            // redirect to a specific tab based on name
            if (!string.IsNullOrEmpty(this.Request.QueryString["tabname"]))
            {
                var tab = TabController.Instance.GetTabByName(this.Request.QueryString["TabName"], this.PortalSettings.PortalId);
                if (tab != null)
                {
                    var parameters = new List<string>(); // maximum number of elements
                    for (var intParam = 0; intParam <= this.Request.QueryString.Count - 1; intParam++)
                    {
                        switch (this.Request.QueryString.Keys[intParam].ToLowerInvariant())
                        {
                            case "tabid":
                            case "tabname":
                                break;
                            default:
                                parameters.Add(
                                    this.Request.QueryString.Keys[intParam] + "=" + this.Request.QueryString[intParam]);
                                break;
                        }
                    }

                    // this.Response.Redirect(this.NavigationManager.NavigateURL(tab.TabID, Null.NullString, parameters.ToArray()), true);
                    throw new MvcPageException("redirect to a specific tab based on name", this.navigationManager.NavigateURL(tab.TabID, Null.NullString, parameters.ToArray()));
                }
                else
                {
                    // 404 Error - Redirect to ErrorPage
                    // Exceptions.ProcessHttpException(this.Request);
                    throw new NotFoundException("redirect to a specific tab based on name - tab not found");
                }
            }

            var cacheability = this.Request.IsAuthenticated ? Host.AuthenticatedCacheability : Host.UnauthenticatedCacheability;

            switch (cacheability)
            {
                case "0":
                    this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    break;
                case "1":
                    this.Response.Cache.SetCacheability(HttpCacheability.Private);
                    break;
                case "2":
                    this.Response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case "3":
                    this.Response.Cache.SetCacheability(HttpCacheability.Server);
                    break;
                case "4":
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                    break;
                case "5":
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                    break;
            }

            // Cookie Consent
            if (this.PortalSettings.ShowCookieConsent)
            {
                MvcJavaScript.RegisterClientReference(this.ControllerContext, ClientAPI.ClientNamespaceReferences.dnn);
                MvcClientAPI.RegisterClientVariable("cc_morelink", this.PortalSettings.CookieMoreLink, true);
                MvcClientAPI.RegisterClientVariable("cc_message", Localization.GetString("cc_message", Localization.GlobalResourceFile), true);
                MvcClientAPI.RegisterClientVariable("cc_dismiss", Localization.GetString("cc_dismiss", Localization.GlobalResourceFile), true);
                MvcClientAPI.RegisterClientVariable("cc_link", Localization.GetString("cc_link", Localization.GlobalResourceFile), true);
                MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/Resources/Shared/Components/CookieConsent/cookieconsent.min.js", FileOrder.Js.DnnControls);
                MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/Resources/Shared/Components/CookieConsent/cookieconsent.min.css", FileOrder.Css.ResourceCss);
                MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/js/dnn.cookieconsent.js", FileOrder.Js.DefaultPriority);
            }
        }
    }
}
