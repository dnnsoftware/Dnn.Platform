// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.Helpers;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Internals;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Models;

    public class PageModelFactory : IPageModelFactory
    {
        private static readonly Regex HeaderTextRegex = new Regex(
    "<meta([^>])+name=('|\")robots('|\")",
    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly INavigationManager navigationManager;
        private readonly IPortalController portalController;
        private readonly IModuleControlPipeline moduleControlPipeline;
        private readonly IApplicationInfo applicationInfo;
        private readonly ISkinModelFactory skinModelFactory;
        private readonly IHostSettings hostSettings;

        public PageModelFactory(
            INavigationManager navigationManager,
            IPortalController portalController,
            IModuleControlPipeline moduleControlPipeline,
            IApplicationInfo applicationInfo,
            ISkinModelFactory skinModelFactory,
            IHostSettings hostSettings)
        {
            this.navigationManager = navigationManager;
            this.portalController = portalController;
            this.moduleControlPipeline = moduleControlPipeline;
            this.applicationInfo = applicationInfo;
            this.skinModelFactory = skinModelFactory;
            this.hostSettings = hostSettings;
        }

        public PageModel CreatePageModel(DnnPageController page)
        {
            var ctl = page.Request.QueryString["ctl"] != null ? page.Request.QueryString["ctl"] : string.Empty;
            var pageModel = new PageModel
            {
                IsEditMode = Globals.IsEditMode(),
                AntiForgery = AntiForgery.GetHtml().ToHtmlString(),
                PortalId = page.PortalSettings.PortalId,
                TabId = page.PortalSettings.ActiveTab.TabID,
                Language = Thread.CurrentThread.CurrentCulture.Name,
                //TODO: CSP - enable when CSP implementation is ready
                // ContentSecurityPolicy = this.contentSecurityPolicy,
                NavigationManager = this.navigationManager,
                FavIconLink = FavIcon.GetHeaderLink(this.hostSettings, page.PortalSettings.PortalId),
            };
            if (page.PortalSettings.ActiveTab.PageHeadText != Null.NullString && !Globals.IsAdminControl())
            {
                pageModel.PageHeadText = page.PortalSettings.ActiveTab.PageHeadText;
            }

            if (!string.IsNullOrEmpty(page.PortalSettings.PageHeadText))
            {
                pageModel.PortalHeadText = page.PortalSettings.PageHeadText;
            }

            // set page title
            if (UrlUtils.InPopUp())
            {
                var strTitle = new StringBuilder(page.PortalSettings.PortalName);
                var slaveModule = DotNetNuke.UI.UIUtilities.GetSlaveModule(page.PortalSettings.ActiveTab.TabID);

                // Skip is popup is just a tab (no slave module)
                if (slaveModule.DesktopModuleID != Null.NullInteger)
                {
                    var control = this.moduleControlPipeline.CreateModuleControl(slaveModule) as IModuleControl;
                    string extension = Path.GetExtension(slaveModule.ModuleControl.ControlSrc.ToLowerInvariant());
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

                    strTitle.Append(string.Concat(" > ", page.PortalSettings.ActiveTab.LocalizedTabName));
                    strTitle.Append(string.Concat(" > ", title));
                }
                else
                {
                    strTitle.Append(string.Concat(" > ", page.PortalSettings.ActiveTab.LocalizedTabName));
                }

                // Set to page
                pageModel.Title = strTitle.ToString();
            }
            else
            {
                // If tab is named, use that title, otherwise build it out via breadcrumbs
                if (!string.IsNullOrEmpty(page.PortalSettings.ActiveTab.Title))
                {
                    pageModel.Title = page.PortalSettings.ActiveTab.Title;
                }
                else
                {
                    // Elected for SB over true concatenation here due to potential for long nesting depth
                    var strTitle = new StringBuilder(page.PortalSettings.PortalName);
                    foreach (TabInfo tab in page.PortalSettings.ActiveTab.BreadCrumbs)
                    {
                        strTitle.Append(string.Concat(" > ", tab.TabName));
                    }

                    pageModel.Title = strTitle.ToString();
                }
            }

            // set the background image if there is one selected
            if (!UrlUtils.InPopUp())
            {
                if (!string.IsNullOrEmpty(page.PortalSettings.BackgroundFile))
                {
                    var fileInfo = this.GetBackgroundFileInfo(page.PortalSettings);
                    pageModel.BackgroundUrl = FileManager.Instance.GetUrl(fileInfo);

                    // ((HtmlGenericControl)this.FindControl("Body")).Attributes["style"] = string.Concat("background-image: url('", url, "')");
                }
            }

            // META Refresh
            // Only autorefresh the page if we are in VIEW-mode and if we aren't displaying some module's subcontrol.
            if (page.PortalSettings.ActiveTab.RefreshInterval > 0 && Personalization.GetUserMode() == PortalSettings.Mode.View && string.IsNullOrEmpty(ctl))
            {
                pageModel.MetaRefresh = page.PortalSettings.ActiveTab.RefreshInterval.ToString();
            }

            // META description
            if (!string.IsNullOrEmpty(page.PortalSettings.ActiveTab.Description))
            {
                pageModel.Description = page.PortalSettings.ActiveTab.Description;
            }
            else
            {
                pageModel.Description = page.PortalSettings.Description;
            }

            // META keywords
            if (!string.IsNullOrEmpty(page.PortalSettings.ActiveTab.KeyWords))
            {
                pageModel.KeyWords = page.PortalSettings.ActiveTab.KeyWords;
            }
            else
            {
                pageModel.KeyWords = page.PortalSettings.KeyWords;
            }

            // META copyright
            if (!string.IsNullOrEmpty(page.PortalSettings.FooterText))
            {
                pageModel.Copyright = page.PortalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString());
            }
            else
            {
                pageModel.Copyright = string.Concat("Copyright (c) ", DateTime.Now.Year, " by ", page.PortalSettings.PortalName);
            }

            // META generator
            pageModel.Generator = string.Empty;

            // META Robots - hide it inside popups and if PageHeadText of current tab already contains a robots meta tag
            if (!UrlUtils.InPopUp() &&
                !(HeaderTextRegex.IsMatch(page.PortalSettings.ActiveTab.PageHeadText) ||
                  HeaderTextRegex.IsMatch(page.PortalSettings.PageHeadText)))
            {
                var allowIndex = true;
                if ((page.PortalSettings.ActiveTab.TabSettings.ContainsKey("AllowIndex") &&
                     bool.TryParse(page.PortalSettings.ActiveTab.TabSettings["AllowIndex"].ToString(), out allowIndex) &&
                     !allowIndex) || ctl == "Login" || ctl == "Register")
                {
                    pageModel.MetaRobots = "NOINDEX, NOFOLLOW";
                }
                else
                {
                    pageModel.MetaRobots = "INDEX, FOLLOW";
                }
            }

            // NonProduction Label Injection
            if (this.applicationInfo.Status != Abstractions.Application.ReleaseMode.Stable && Host.DisplayBetaNotice && !UrlUtils.InPopUp())
            {
                string versionString =
                    $" ({this.applicationInfo.Status} Version: {this.applicationInfo.Version})";
                pageModel.Title += versionString;
            }

            pageModel.Skin = this.skinModelFactory.CreateSkinModel(page);

            return pageModel;
        }

        private IFileInfo GetBackgroundFileInfo(PortalSettings portalSettings)
        {
            string cacheKey = string.Format(Common.Utilities.DataCache.PortalCacheKey, portalSettings.PortalId, "BackgroundFile");
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
                new CacheItemArgs(cacheKey, Common.Utilities.DataCache.PortalCacheTimeOut, Common.Utilities.DataCache.PortalCachePriority, portalSettings.PortalId, portalSettings.BackgroundFile),
                this.GetBackgroundFileInfoCallBack);

            return file;
        }

        private IFileInfo GetBackgroundFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile((int)itemArgs.Params[0], (string)itemArgs.Params[1]);
        }
    }
}
