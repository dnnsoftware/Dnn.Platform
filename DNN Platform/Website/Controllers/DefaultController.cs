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
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Installer.Log;
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
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;
        private readonly IUserController userController;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IServicesFramework servicesFramework;
        private readonly IContentSecurityPolicy contentSecurityPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultController"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager for URL generation.</param>
        /// <param name="pageModelFactory">The factory for creating page models.</param>
        /// <param name="clientResourceController">The controller for managing client resources (scripts and stylesheets).</param>
        /// <param name="pageService">The service for page-related operations.</param>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="hostSettings">The host settings configuration.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="contentSecurityPolicy">The ContentSecurityPolicy.</param>
        public DefaultController(
                                INavigationManager navigationManager,
                                IPageModelFactory pageModelFactory,
                                IClientResourceController clientResourceController,
                                IPageService pageService,
                                IServiceProvider serviceProvider,
                                IHostSettings hostSettings,
                                IApplicationStatusInfo appStatus,
                                IEventLogger eventLogger,
                                IPortalController portalController,
                                IUserController userController,
                                IHostSettingsService hostSettingsService,
                                IServicesFramework servicesFramework,
                                IContentSecurityPolicy contentSecurityPolicy)
            : base(serviceProvider)
        {
            this.navigationManager = navigationManager;
            this.pageModelFactory = pageModelFactory;
            this.clientResourceController = clientResourceController;
            this.pageService = pageService;
            this.hostSettings = hostSettings;
            this.appStatus = appStatus;
            this.eventLogger = eventLogger;
            this.portalController = portalController;
            this.userController = userController;
            this.hostSettingsService = hostSettingsService;
            this.servicesFramework = servicesFramework;
            this.contentSecurityPolicy = contentSecurityPolicy;
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
            if (this.PortalSettings.CspHeaderMode == PortalSettings.CspMode.ReportOnly ||
                    this.PortalSettings.CspHeaderMode == PortalSettings.CspMode.On)
            {
                bool.TryParse(Config.GetSetting("DisableCsp"), out bool disableCsp);

                if (!disableCsp)
                {
                    this.AddCspHeaders();
                }
            }

            // There could be a pending installation/upgrade process
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                Exceptions.ProcessHttpException(new HttpException(503, Localization.GetString("SiteAccessedWhileInstallationWasInProgress.Error", Localization.GlobalResourceFile)));
            }

            var user = this.PortalSettings.UserInfo;

            if (PortalSettings.Current.UserId > 0)
            {
                // TODO: should we do this? It creates a dependency towards the PersonaBar which is probably not a great idea
                MvcContentEditorManager.CreateManager(this, this.clientResourceController, this.appStatus, this.eventLogger, this.portalController, this.hostSettings, this.userController, this.hostSettingsService, this.servicesFramework);
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

        private void AddCspHeaders()
        {
            if (!string.IsNullOrEmpty(this.PortalSettings.CspHeader))
            {
                this.contentSecurityPolicy.AddHeader(this.PortalSettings.CspHeader);
            }

            if (!string.IsNullOrEmpty(this.PortalSettings.CspReportingHeader))
            {
                this.contentSecurityPolicy.AddReportEndpointHeader(this.PortalSettings.CspReportingHeader);
            }

            this.contentSecurityPolicy.AddMVCSupport();
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

            if (this.PortalSettings.CspHeaderMode == PortalSettings.CspMode.ReportOnly ||
                    this.PortalSettings.CspHeaderMode == PortalSettings.CspMode.On)
            {
                bool.TryParse(Config.GetSetting("DisableCsp"), out bool disableCsp);

                if (!disableCsp)
                {
                    var header = "Content-Security-Policy";
                    if (this.PortalSettings.CspHeaderMode == PortalSettings.CspMode.ReportOnly)
                    {
                        header = "Content-Security-Policy-Report-Only";
                    }

                    page.CspHeader = header;
                    page.CspHeaderFixed = this.PortalSettings.CspHeaderFixed;

                    // If fixed, we need to clear any existing contributors and just use the fixed headers
                    if (this.PortalSettings.CspHeaderFixed)
                    {
                        this.contentSecurityPolicy.ClearContentSecurityPolicyContributors();
                        this.contentSecurityPolicy.ClearReportingEndpointsContributors();
                        this.AddCspHeaders();
                        var policy = this.contentSecurityPolicy.GeneratePolicy();
                        if (!string.IsNullOrEmpty(policy))
                        {
                            page.CspHeaderValue = policy;
                        }

                        policy = this.contentSecurityPolicy.GenerateReportingEndpoints();
                        if (!string.IsNullOrEmpty(policy))
                        {
                            page.CspReportingHeader = "Reporting-Endpoints";
                            page.CspReportingHeaderValue = policy;
                        }
                    }
                }
            }
        }
    }
}
