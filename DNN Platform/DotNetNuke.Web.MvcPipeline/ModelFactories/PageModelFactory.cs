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
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ContentSecurityPolicy;
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

    public class PageModelFactory : IPageModelFactory
    {
        private static readonly Regex HeaderTextRegex = new Regex(
    "<meta([^>])+name=('|\")robots('|\")",
    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly IContentSecurityPolicy contentSecurityPolicy;
        private readonly INavigationManager navigationManager;
        private readonly IPortalController portalController;
        private readonly IModuleControlPipeline moduleControlPipeline;
        private readonly IApplicationInfo applicationInfo;
        private readonly ISkinModelFactory skinModelFactory;
        private readonly IHostSettings hostSettings;

        public PageModelFactory(
            IContentSecurityPolicy contentSecurityPolicy,
            INavigationManager navigationManager,
            IPortalController portalController,
            IModuleControlPipeline moduleControlPipeline,
            IApplicationInfo applicationInfo,
            ISkinModelFactory skinModelFactory,
            IHostSettings hostSettings)
        {
            this.contentSecurityPolicy = contentSecurityPolicy;
            this.navigationManager = navigationManager;
            this.portalController = portalController;
            this.moduleControlPipeline = moduleControlPipeline;
            this.applicationInfo = applicationInfo;
            this.skinModelFactory = skinModelFactory;
            this.hostSettings = hostSettings;
        }

        public PageModel CreatePageModel(DnnPageController controller)
        {
            var ctl = controller.Request.QueryString["ctl"] != null ? controller.Request.QueryString["ctl"] : string.Empty;
            var pageModel = new PageModel
            {
                IsEditMode = Globals.IsEditMode(),
                AntiForgery = AntiForgery.GetHtml().ToHtmlString(),
                PortalId = controller.PortalSettings.PortalId,
                TabId = controller.PortalSettings.ActiveTab.TabID,
                Language = Thread.CurrentThread.CurrentCulture.Name,
                ContentSecurityPolicy = this.contentSecurityPolicy,
                NavigationManager = this.navigationManager,
                FavIconLink = FavIcon.GetHeaderLink(hostSettings, controller.PortalSettings.PortalId),
            };
            if (controller.PortalSettings.ActiveTab.PageHeadText != Null.NullString && !Globals.IsAdminControl())
            {
                pageModel.PageHeadText = controller.PortalSettings.ActiveTab.PageHeadText;
            }

            if (!string.IsNullOrEmpty(controller.PortalSettings.PageHeadText))
            {
                pageModel.PortalHeadText = controller.PortalSettings.PageHeadText;
            }

            // set page title
            if (UrlUtils.InPopUp())
            {
                var strTitle = new StringBuilder(controller.PortalSettings.PortalName);
                var slaveModule = DotNetNuke.UI.UIUtilities.GetSlaveModule(controller.PortalSettings.ActiveTab.TabID);

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

                    strTitle.Append(string.Concat(" > ", controller.PortalSettings.ActiveTab.LocalizedTabName));
                    strTitle.Append(string.Concat(" > ", title));
                }
                else
                {
                    strTitle.Append(string.Concat(" > ", controller.PortalSettings.ActiveTab.LocalizedTabName));
                }

                // Set to page
                pageModel.Title = strTitle.ToString();
            }
            else
            {
                // If tab is named, use that title, otherwise build it out via breadcrumbs
                if (!string.IsNullOrEmpty(controller.PortalSettings.ActiveTab.Title))
                {
                    pageModel.Title = controller.PortalSettings.ActiveTab.Title;
                }
                else
                {
                    // Elected for SB over true concatenation here due to potential for long nesting depth
                    var strTitle = new StringBuilder(controller.PortalSettings.PortalName);
                    foreach (TabInfo tab in controller.PortalSettings.ActiveTab.BreadCrumbs)
                    {
                        strTitle.Append(string.Concat(" > ", tab.TabName));
                    }

                    pageModel.Title = strTitle.ToString();
                }
            }

            // set the background image if there is one selected
            if (!UrlUtils.InPopUp())
            {
                if (!string.IsNullOrEmpty(controller.PortalSettings.BackgroundFile))
                {
                    var fileInfo = this.GetBackgroundFileInfo(controller.PortalSettings);
                    pageModel.BackgroundUrl = FileManager.Instance.GetUrl(fileInfo);

                    // ((HtmlGenericControl)this.FindControl("Body")).Attributes["style"] = string.Concat("background-image: url('", url, "')");
                }
            }

            // META Refresh
            // Only autorefresh the page if we are in VIEW-mode and if we aren't displaying some module's subcontrol.
            if (controller.PortalSettings.ActiveTab.RefreshInterval > 0 && Personalization.GetUserMode() == PortalSettings.Mode.View && string.IsNullOrEmpty(ctl))
            {
                pageModel.MetaRefresh = controller.PortalSettings.ActiveTab.RefreshInterval.ToString();
            }

            // META description
            if (!string.IsNullOrEmpty(controller.PortalSettings.ActiveTab.Description))
            {
                pageModel.Description = controller.PortalSettings.ActiveTab.Description;
            }
            else
            {
                pageModel.Description = controller.PortalSettings.Description;
            }

            // META keywords
            if (!string.IsNullOrEmpty(controller.PortalSettings.ActiveTab.KeyWords))
            {
                pageModel.KeyWords = controller.PortalSettings.ActiveTab.KeyWords;
            }
            else
            {
                pageModel.KeyWords = controller.PortalSettings.KeyWords;
            }

            // META copyright
            if (!string.IsNullOrEmpty(controller.PortalSettings.FooterText))
            {
                pageModel.Copyright = controller.PortalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString());
            }
            else
            {
                pageModel.Copyright = string.Concat("Copyright (c) ", DateTime.Now.Year, " by ", controller.PortalSettings.PortalName);
            }

            // META generator
            pageModel.Generator = string.Empty;

            // META Robots - hide it inside popups and if PageHeadText of current tab already contains a robots meta tag
            if (!UrlUtils.InPopUp() &&
                !(HeaderTextRegex.IsMatch(controller.PortalSettings.ActiveTab.PageHeadText) ||
                  HeaderTextRegex.IsMatch(controller.PortalSettings.PageHeadText)))
            {
                var allowIndex = true;
                if (controller.PortalSettings.ActiveTab.TabSettings.ContainsKey("AllowIndex") &&
                     bool.TryParse(controller.PortalSettings.ActiveTab.TabSettings["AllowIndex"].ToString(), out allowIndex) &&
                     !allowIndex || ctl == "Login" || ctl == "Register")
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
                var versionString =
                    $" ({this.applicationInfo.Status} Version: {this.applicationInfo.Version})";
                pageModel.Title += versionString;
            }

            pageModel.Skin = this.skinModelFactory.CreateSkinModel(controller);

            return pageModel;
        }
        
        private IFileInfo GetBackgroundFileInfo(PortalSettings portalSettings)
        {
            var cacheKey = string.Format(DataCache.PortalCacheKey, portalSettings.PortalId, "BackgroundFile");
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
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
