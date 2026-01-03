// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Helpers;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Internals;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Builds <see cref="PageModel"/> instances from DNN portal and tab state for the MVC pipeline.
    /// </summary>
    public class PageModelFactory : IPageModelFactory
    {
        private static readonly Regex HeaderTextRegex = new Regex(
    "<meta([^>])+name=('|\")robots('|\")",
    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly INavigationManager navigationManager;
        private readonly IPortalController portalController;
        private readonly IPortalSettingsController portalSettingsController;
        private readonly IModuleControlPipeline moduleControlPipeline;
        private readonly IApplicationInfo applicationInfo;
        private readonly ISkinModelFactory skinModelFactory;
        private readonly IHostSettings hostSettings;
        private readonly IPageService pageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageModelFactory"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalSettingsController">The portal settings controller.</param>
        /// <param name="moduleControlPipeline">The module control pipeline.</param>
        /// <param name="applicationInfo">The application information service.</param>
        /// <param name="skinModelFactory">The skin model factory.</param>
        /// <param name="hostSettings">The host settings service.</param>
        /// <param name="pageService">The page service used for meta data.</param>
        public PageModelFactory(
            INavigationManager navigationManager,
            IPortalController portalController,
            IPortalSettingsController portalSettingsController,
            IModuleControlPipeline moduleControlPipeline,
            IApplicationInfo applicationInfo,
            ISkinModelFactory skinModelFactory,
            IHostSettings hostSettings,
            IPageService pageService)
        {
            this.navigationManager = navigationManager;
            this.portalController = portalController;
            this.portalSettingsController = portalSettingsController;
            this.moduleControlPipeline = moduleControlPipeline;
            this.applicationInfo = applicationInfo;
            this.skinModelFactory = skinModelFactory;
            this.hostSettings = hostSettings;
            this.pageService = pageService;
        }

        /// <inheritdoc/>
        public PageModel CreatePageModel(DnnPageController controller)
        {
            var ctl = controller.Request.QueryString["ctl"] != null ? controller.Request.QueryString["ctl"] : string.Empty;
            IPortalSettings portalSettings = controller.PortalSettings;
            TabInfo activeTab = controller.PortalSettings.ActiveTab;
            var pageModel = new PageModel
            {
                IsEditMode = Globals.IsEditMode(),
                AntiForgery = AntiForgery.GetHtml().ToHtmlString(),
                PortalId = portalSettings.PortalId,
                TabId = activeTab.TabID,
                Language = Thread.CurrentThread.CurrentCulture.Name,

                // TODO: CSP - enable when CSP implementation is ready
                // ContentSecurityPolicy = this.contentSecurityPolicy,
                NavigationManager = this.navigationManager,
                PageService = this.pageService,
                FavIconLink = FavIcon.GetHeaderLink(this.hostSettings, portalSettings.PortalId),
            };
            if (activeTab.PageHeadText != Null.NullString && !Globals.IsAdminControl())
            {
                pageModel.PageHeadText = activeTab.PageHeadText;
            }

            if (!string.IsNullOrEmpty(portalSettings.PageHeadText))
            {
                pageModel.PortalHeadText = portalSettings.PageHeadText;
            }

            // set page title
            if (UrlUtils.InPopUp())
            {
                var strTitle = new StringBuilder(portalSettings.PortalName);
                var slaveModule = DotNetNuke.UI.UIUtilities.GetSlaveModule(activeTab.TabID);

                // Skip is popup is just a tab (no slave module)
                if (slaveModule.DesktopModuleID != Null.NullInteger)
                {
                    var control = this.moduleControlPipeline.CreateModuleControl(slaveModule) as IModuleControl;
                    var extension = Path.GetExtension(slaveModule.ModuleControl.ControlSrc.ToLowerInvariant());
                    switch (extension)
                    {
                        case ".mvc":
                            var segments = slaveModule.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');

                            control.LocalResourceFile = string.Format(
                                "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                                slaveModule.DesktopModule.FolderName,
                                Localization.LocalResourceDirectory,
                                segments[0]);
                            break;
                        default:
                            control.LocalResourceFile = string.Concat(
                                slaveModule.ModuleControl.ControlSrc.Replace(
                                    Path.GetFileName(slaveModule.ModuleControl.ControlSrc),
                                    string.Empty),
                                Localization.LocalResourceDirectory,
                                "/",
                                Path.GetFileName(slaveModule.ModuleControl.ControlSrc));
                            break;
                    }

                    var title = Localization.LocalizeControlTitle(control);

                    strTitle.Append(string.Concat(" > ", activeTab.LocalizedTabName));
                    strTitle.Append(string.Concat(" > ", title));
                }
                else
                {
                    strTitle.Append(string.Concat(" > ", activeTab.LocalizedTabName));
                }

                this.pageService.SetTitle(strTitle.ToString(), PagePriority.Page);
            }
            else
            {
                // If tab is named, use that title, otherwise build it out via breadcrumbs
                if (!string.IsNullOrEmpty(activeTab.Title))
                {
                    this.pageService.SetTitle(activeTab.Title, PagePriority.Page);
                }
                else
                {
                    // Elected for SB over true concatenation here due to potential for long nesting depth
                    var strTitle = new StringBuilder(portalSettings.PortalName);
                    foreach (TabInfo tab in activeTab.BreadCrumbs)
                    {
                        strTitle.Append(string.Concat(" > ", tab.TabName));
                    }

                    this.pageService.SetTitle(strTitle.ToString(), PagePriority.Page);
                }
            }

            // Set to page
            pageModel.Title = this.pageService.GetTitle();

            // set the background image if there is one selected
            if (!UrlUtils.InPopUp())
            {
                if (!string.IsNullOrEmpty(portalSettings.BackgroundFile))
                {
                    var fileInfo = this.GetBackgroundFileInfo(portalSettings);
                    pageModel.BackgroundUrl = FileManager.Instance.GetUrl(fileInfo);

                    // ((HtmlGenericControl)this.FindControl("Body")).Attributes["style"] = string.Concat("background-image: url('", url, "')");
                }
            }

            // META Refresh
            // Only autorefresh the page if we are in VIEW-mode and if we aren't displaying some module's subcontrol.
            if (activeTab.RefreshInterval > 0 && Personalization.GetUserMode() == PortalSettings.Mode.View && string.IsNullOrEmpty(ctl))
            {
                pageModel.MetaRefresh = activeTab.RefreshInterval.ToString();
            }

            // META description
            if (!string.IsNullOrEmpty(activeTab.Description))
            {
                this.pageService.SetDescription(activeTab.Description, PagePriority.Page);
            }
            else
            {
                this.pageService.SetDescription(portalSettings.Description, PagePriority.Site);
            }

            pageModel.Description = this.pageService.GetDescription();

            // META keywords
            if (!string.IsNullOrEmpty(activeTab.KeyWords))
            {
                this.pageService.SetKeyWords(activeTab.KeyWords, PagePriority.Page);
            }
            else
            {
                this.pageService.SetKeyWords(portalSettings.KeyWords, PagePriority.Site);
            }

            pageModel.KeyWords = this.pageService.GetKeyWords();

            // META copyright
            if (!string.IsNullOrEmpty(portalSettings.FooterText))
            {
                pageModel.Copyright = portalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString());
            }
            else
            {
                pageModel.Copyright = string.Concat("Copyright (c) ", DateTime.Now.Year, " by ", portalSettings.PortalName);
            }

            // META generator
            pageModel.Generator = string.Empty;

            // META Robots - hide it inside popups and if PageHeadText of current tab already contains a robots meta tag
            if (!UrlUtils.InPopUp() &&
                !(HeaderTextRegex.IsMatch(activeTab.PageHeadText) ||
                  HeaderTextRegex.IsMatch(portalSettings.PageHeadText)))
            {
                var allowIndex = true;
                if ((activeTab.TabSettings.ContainsKey("AllowIndex") &&
                     bool.TryParse(activeTab.TabSettings["AllowIndex"].ToString(), out allowIndex) &&
                     !allowIndex) || ctl == "Login" || ctl == "Register")
                {
                    pageModel.MetaRobots = "NOINDEX, NOFOLLOW";
                }
                else
                {
                    pageModel.MetaRobots = "INDEX, FOLLOW";
                }
            }

            pageModel.CanonicalLinkUrl = this.pageService.GetCanonicalLinkUrl();

            // NonProduction Label Injection
            if (this.applicationInfo.Status != Abstractions.Application.ReleaseMode.Stable && this.hostSettings.DisplayBetaNotice && !UrlUtils.InPopUp())
            {
                var versionString =
                    $" ({this.applicationInfo.Status} Version: {this.applicationInfo.Version})";
                pageModel.Title += versionString;
            }

            pageModel.Skin = this.skinModelFactory.CreateSkinModel(controller);

            return pageModel;
        }

        private IFileInfo GetBackgroundFileInfo(IPortalSettings portalSettings)
        {
            var cacheKey = string.Format(DataCache.PortalCacheKey, portalSettings.PortalId, "BackgroundFile");
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, portalSettings.PortalId, portalSettings.BackgroundFile),
                this.GetBackgroundFileInfoCallBack);

            return file;
        }

        private IFileInfo GetBackgroundFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile((int)itemArgs.Params[0], (string)itemArgs.Params[1]);
        }
    }
}
