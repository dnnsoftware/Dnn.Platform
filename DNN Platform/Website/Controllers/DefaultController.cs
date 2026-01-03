// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;

    using Dnn.EditBar.UI.Mvc;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization;

    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Exceptions;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.ModelFactories;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;

    /// <summary>
    /// Default controller for handling page rendering in the MVC pipeline.
    /// </summary>
    public class DefaultController : DnnPageController
    {
        private readonly INavigationManager navigationManager;
        private readonly IPageModelFactory pageModelFactory;
        private readonly IClientResourceController clientResourceController;
        private readonly IPageService pageService;
        private readonly IHostSettings hostSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultController"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager for URL generation.</param>
        /// <param name="pageModelFactory">The factory for creating page models.</param>
        /// <param name="clientResourceController">The controller for managing client resources (scripts and stylesheets).</param>
        /// <param name="pageService">The service for page-related operations.</param>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="hostSettings">The host settings configuration.</param>
        public DefaultController(
                                INavigationManager navigationManager,
                                IPageModelFactory pageModelFactory,
                                IClientResourceController clientResourceController,
                                IPageService pageService,
                                IServiceProvider serviceProvider,
                                IHostSettings hostSettings)
            : base(serviceProvider)
        {
            this.navigationManager = navigationManager;
            this.pageModelFactory = pageModelFactory;
            this.clientResourceController = clientResourceController;
            this.pageService = pageService;
            this.hostSettings = hostSettings;
        }

        /// <summary>
        /// Renders a page for the specified tab and language.
        /// </summary>
        /// <param name="tabid">The tab (page) identifier to render.</param>
        /// <param name="language">The language code for localization.</param>
        /// <returns>
        /// A view result containing the rendered page, or an HTTP status code result if an error occurs.
        /// Returns 403 (Forbidden) for access denied, 404 (Not Found) for missing pages, or redirects as needed.
        /// </returns>
        public ActionResult Page(int tabid, string language)
        {
            // TODO: CSP - enable when CSP implementation is ready
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

            try
            {
                PageModel model = this.pageModelFactory.CreatePageModel(this);
                this.clientResourceController.RegisterPathNameAlias("SkinPath", this.PortalSettings.ActiveTab.SkinPath);
                model.ClientResourceController = this.clientResourceController;
                model.PageService = this.pageService;
                this.InitializePage(model);

                // DotNetNuke.Framework.JavaScriptLibraries.MvcJavaScript.Register(this.ControllerContext);
                model.ClientVariables = MvcClientAPI.GetClientVariableList();
                model.StartupScripts = MvcClientAPI.GetClientStartupScriptList();

                // Register the scripts and stylesheets
                this.RegisterScriptsAndStylesheets(model);

                return this.View(model.Skin.RazorFile, "Layout", model);
            }
            catch (AccesDeniedException)
            {
                return new HttpStatusCodeResult(403, "Access Denied");
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
        }

        /// <summary>
        /// Registers all scripts and stylesheets required for the page rendering.
        /// </summary>
        /// <param name="page">The page model containing skin, container, and resource information.</param>
        private void RegisterScriptsAndStylesheets(PageModel page)
        {
            foreach (var styleSheet in page.Skin.RegisteredStylesheets)
            {
                this.clientResourceController.CreateStylesheet(styleSheet.Stylesheet)
                        .SetPriority((int)styleSheet.FileOrder)
                        .Register();
            }

            foreach (var pane in page.Skin.Panes)
            {
                foreach (var container in pane.Value.Containers)
                {
                    foreach (var stylesheet in container.Value.RegisteredStylesheets)
                    {
                        this.clientResourceController.CreateStylesheet(stylesheet.Stylesheet)
                                .SetPriority((int)stylesheet.FileOrder)
                                .Register();
                    }
                }
            }

            foreach (var script in page.Skin.RegisteredScripts)
            {
                this.clientResourceController.CreateScript(script.Script)
                                .SetPriority((int)script.FileOrder)
                                .Register();
            }
        }

        /// <summary>
        /// Initializes the page by handling tab name redirects, setting cache control headers, and configuring cookie consent.
        /// </summary>
        /// <param name="page">The page model to initialize.</param>
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

                    throw new MvcPageException("redirect to a specific tab based on name", this.navigationManager.NavigateURL(tab.TabID, Null.NullString, parameters.ToArray()));
                }
                else
                {
                    // 404 Error - Redirect to ErrorPage
                    throw new NotFoundException("redirect to a specific tab based on name - tab not found");
                }
            }

            var cacheability = this.Request.IsAuthenticated ? this.hostSettings.AuthenticatedCacheability : this.hostSettings.UnauthenticatedCacheability;

            switch (cacheability)
            {
                case CacheControlHeader.NoCache:
                    this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    break;
                case CacheControlHeader.Private:
                    this.Response.Cache.SetCacheability(HttpCacheability.Private);
                    break;
                case CacheControlHeader.Public:
                    this.Response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case CacheControlHeader.ServerAndNoCache:
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                    break;
                case CacheControlHeader.ServerAndPrivate:
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                    break;
            }

            // Cookie Consent
            if (this.PortalSettings.ShowCookieConsent)
            {
                MvcJavaScript.RegisterClientReference(DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn);
                MvcClientAPI.RegisterClientVariable("cc_morelink", this.PortalSettings.CookieMoreLink, true);
                MvcClientAPI.RegisterClientVariable("cc_message", Localization.GetString("cc_message", Localization.GlobalResourceFile), true);
                MvcClientAPI.RegisterClientVariable("cc_dismiss", Localization.GetString("cc_dismiss", Localization.GlobalResourceFile), true);
                MvcClientAPI.RegisterClientVariable("cc_link", Localization.GetString("cc_link", Localization.GlobalResourceFile), true);
                this.clientResourceController.RegisterScript("~/Resources/Shared/Components/CookieConsent/cookieconsent.min.js", FileOrder.Js.DnnControls);
                this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/CookieConsent/cookieconsent.min.cssdisa", FileOrder.Css.ResourceCss);
                this.clientResourceController.RegisterScript("~/js/dnn.cookieconsent.js");
            }
        }
    }
}
