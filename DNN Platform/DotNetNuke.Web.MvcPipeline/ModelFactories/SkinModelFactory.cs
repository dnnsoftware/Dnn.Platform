// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Extensions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Pages;
    using DotNetNuke.UI;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Exceptions;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.Skins;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Builds <see cref="SkinModel"/> instances from portal, tab, and module configuration.
    /// </summary>
    public class SkinModelFactory : ISkinModelFactory
    {
        /// <summary>Gets the key used to store initialization messages in HttpContext.Items.</summary>
        public const string OnInitMessage = "Skin_InitMessage";

        /// <summary>Gets the key used to store initialization message types in HttpContext.Items.</summary>
        public const string OnInitMessageType = "Skin_InitMessageType";

        private readonly INavigationManager navigationManager;
        private readonly IPaneModelFactory paneModelFactory;
        private readonly IPageService pageService;
        private readonly IClientResourceController clientResourceController;
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;

        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinModelFactory"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="paneModelFactory">The pane model factory.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="pageService">The page service used for messages and meta data.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status information.</param>
        /// <param name="eventLogger">The event logger.</param>
        public SkinModelFactory(
                                INavigationManager navigationManager,
                                IPaneModelFactory paneModelFactory,
                                IClientResourceController clientResourceController,
                                IPageService pageService,
                                IHostSettings hostSettings,
                                IPortalController portalController,
                                IApplicationStatusInfo appStatus,
                                IEventLogger eventLogger)
        {
            this.navigationManager = navigationManager;
            this.paneModelFactory = paneModelFactory;
            this.clientResourceController = clientResourceController;
            this.pageService = pageService;
            this.hostSettings = hostSettings;
            this.portalController = portalController;
            this.appStatus = appStatus;
            this.eventLogger = eventLogger;
        }

        /// <inheritdoc/>
        public SkinModel CreateSkinModel(DnnPageController page)
        {
            SkinModel skin = null;
            var skinSource = Null.NullString;
            IPortalSettings portalSettings = page.PortalSettings;

            if (portalSettings.EnablePopUps && UrlUtils.InPopUp())
            {
                // attempt to find and load a popup skin from the assigned skinned source
                skinSource = Globals.IsAdminSkin() ? SkinController.FormatSkinSrc(portalSettings.DefaultAdminSkin, PortalSettings.Current) : TabController.CurrentPage.SkinSrc;
                if (!string.IsNullOrEmpty(skinSource))
                {
                    skinSource = SkinController.FormatSkinSrc(SkinController.FormatSkinPath(skinSource) + "popUpSkin.ascx", PortalSettings.Current);

                    if (File.Exists(HttpContext.Current.Server.MapPath(SkinController.FormatSkinSrc(skinSource, PortalSettings.Current))))
                    {
                        skin = this.LoadSkin(page, skinSource);
                    }
                }

                // error loading popup skin - load default popup skin
                if (skin == null)
                {
                    skinSource = Globals.HostPath + "Skins/_default/popUpSkin.ascx";
                    skin = this.LoadSkin(page, skinSource);
                }

                // set skin path
                TabController.CurrentPage.SkinPath = SkinController.FormatSkinPath(skinSource);
            }
            else
            {
                // skin preview
                if (page.Request.QueryString["SkinSrc"] != null)
                {
                    skinSource = SkinController.FormatSkinSrc(Globals.QueryStringDecode(page.Request.QueryString["SkinSrc"]) + ".ascx", PortalSettings.Current);
                    skin = this.LoadSkin(page, skinSource);
                }

                // load user skin ( based on cookie )
                if (skin == null)
                {
                    var skinCookie = page.Request.Cookies["_SkinSrc" + portalSettings.PortalId];
                    if (skinCookie != null)
                    {
                        if (!string.IsNullOrEmpty(skinCookie.Value))
                        {
                            skinSource = SkinController.FormatSkinSrc(skinCookie.Value + ".ascx", PortalSettings.Current);
                            skin = this.LoadSkin(page, skinSource);
                        }
                    }
                }

                // load assigned skin
                if (skin == null)
                {
                    // DNN-6170 ensure skin value is culture specific
                    skinSource = Globals.IsAdminSkin() ? PortalController.GetPortalSetting(this.portalController, "DefaultAdminSkin", portalSettings.PortalId, this.hostSettings.DefaultPortalSkin, portalSettings.CultureCode) : TabController.CurrentPage.SkinSrc;
                    if (!string.IsNullOrEmpty(skinSource))
                    {
                        skinSource = SkinController.FormatSkinSrc(skinSource, PortalSettings.Current);
                        skin = this.LoadSkin(page, skinSource);
                    }
                }

                // error loading skin - load default
                if (skin == null)
                {
                    skinSource = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalSkin(), PortalSettings.Current);
                    skin = this.LoadSkin(page, skinSource);
                }

                // set skin path
                TabController.CurrentPage.SkinPath = SkinController.FormatSkinPath(skinSource);
            }

            if (TabController.CurrentPage.DisableLink)
            {
                if (TabPermissionController.CanAdminPage())
                {
                    var heading = Localization.GetString("PageDisabled.Header");
                    var message = Localization.GetString("PageDisabled.Text");
                    this.pageService.AddWarningMessage(heading, message);
                }
            }

            // add CSS links
            this.clientResourceController.CreateStylesheet("~/Resources/Shared/stylesheets/dnndefault/10.0.0/default.css")
                .SetNameAndVersion("dnndefault", "10.0.0", false)
                .SetPriority(FileOrder.Css.DefaultCss)
                .Register();

            this.clientResourceController.RegisterStylesheet(string.Concat(TabController.CurrentPage.SkinPath, "skin.css"), FileOrder.Css.SkinCss, true);
            this.clientResourceController.RegisterStylesheet(TabController.CurrentPage.SkinSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificSkinCss, true);

            // portal.css
            skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = string.Concat(portalSettings.HomeDirectory, "portal.css"), FileOrder = FileOrder.Css.PortalCss });

            // register css variables
            var cssVariablesStyleSheet = this.GetCssVariablesStylesheet(portalSettings.PortalId, portalSettings.GetStyles(), portalSettings.HomeSystemDirectory);
            skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = cssVariablesStyleSheet, FileOrder = FileOrder.Css.DefaultCss });

            // register the custom stylesheet of current page
            if (TabController.CurrentPage.TabSettings.ContainsKey("CustomStylesheet") && !string.IsNullOrEmpty(TabController.CurrentPage.TabSettings["CustomStylesheet"].ToString()))
            {
                var styleSheet = TabController.CurrentPage.TabSettings["CustomStylesheet"].ToString();

                // Try and go through the FolderProvider first
                var stylesheetFile = this.GetPageStylesheetFileInfo(styleSheet, portalSettings.PortalId);
                if (stylesheetFile != null)
                {
                    skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = FileManager.Instance.GetUrl(stylesheetFile), FileOrder = FileOrder.Css.DefaultCss });
                }
                else
                {
                    skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = styleSheet, FileOrder = FileOrder.Css.DefaultCss });
                }
            }

            if (portalSettings.EnablePopUps)
            {
                JavaScript.RequestRegistration(this.appStatus, this.eventLogger, portalSettings, CommonJs.jQueryUI);
                var popupFilePath = HttpContext.Current.IsDebuggingEnabled
                                   ? "~/js/Debug/dnn.modalpopup.js"
                                   : "~/js/dnn.modalpopup.js";
                skin.RegisteredScripts.Add(new RegisteredScript() { Script = popupFilePath, FileOrder = FileOrder.Js.DnnModalPopup });
            }

            return skin;
        }

        protected virtual SkinModel LoadSkin(DnnPageController page, string skinPath)
        {
            SkinModel ctlSkin = null;
            try
            {
                var skinSrc = skinPath;
                if (skinPath.IndexOf(Globals.ApplicationPath, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    skinPath = skinPath.Remove(0, Globals.ApplicationPath.Length);
                }

                ctlSkin = new SkinModel();
                ctlSkin.SkinSrc = skinSrc;
                ctlSkin.SkinPath = SkinController.FormatSkinPath(skinSrc);
                ctlSkin.RazorPath = SkinHelpers.SkinPathToRazorPath(SkinController.FormatSkinPath(skinPath));
                ctlSkin.RazorFile = ctlSkin.RazorPath + Path.GetFileName(ctlSkin.SkinSrc).Replace(".ascx", ".cshtml");
                ctlSkin.BodyCssClass = Globals.IsEditMode() ? "dnnEditState" : string.Empty;
                ctlSkin.PaneCssClass = /*Globals.IsEditMode() ? "dnnSortable" : */string.Empty;

                // Load the Module Control(s)
                var success = Globals.IsAdminControl() ? this.ProcessSlaveModule(page, ctlSkin) : this.ProcessMasterModules(page, ctlSkin);

                // Load the Control Panel
                this.InjectControlPanel(ctlSkin, page.Request);

                // Register any error messages on the Skin
                if (page.Request.QueryString["error"] != null && this.hostSettings.ShowCriticalErrors)
                {
                    this.pageService.AddErrorMessage(" ", Localization.GetString("CriticalError.Error"));

                    if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                    {
                        ServicesFramework.Instance.RequestAjaxScriptSupport();
                        ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

                        JavaScript.RequestRegistration(this.appStatus, this.eventLogger, page.PortalSettings, CommonJs.jQueryUI);
                        MvcJavaScript.RegisterClientReference(DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn_dom);
                        this.clientResourceController.RegisterScript("~/resources/shared/scripts/dnn.logViewer.js");
                    }
                }

                if (!success && !TabPermissionController.CanAdminPage())
                {
                    // only display the warning to non-administrators (administrators will see the errors)
                    this.pageService.AddWarningMessage(Localization.GetString("ModuleLoadWarning.Error"), string.Format(Localization.GetString("ModuleLoadWarning.Text"), page.PortalSettings.Email));
                }

                if (HttpContext.Current != null && HttpContext.Current.Items.Contains(OnInitMessage))
                {
                    var messageType = PageMessageType.Warning;
                    if (HttpContext.Current.Items.Contains(OnInitMessageType))
                    {
                        messageType = (PageMessageType)Enum.Parse(typeof(PageMessageType), HttpContext.Current.Items[OnInitMessageType].ToString(), true);
                    }

                    this.pageService.AddMessage(new PageMessage(string.Empty, HttpContext.Current.Items[OnInitMessage].ToString(), messageType, string.Empty, PagePriority.Default));

                    JavaScript.RequestRegistration(this.appStatus, this.eventLogger, page.PortalSettings, CommonJs.DnnPlugins);
                    ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                }

                var isSpecialPageMode = UrlUtils.InPopUp() || page.Request.QueryString["dnnprintmode"] == "true";
                if (TabPermissionController.CanAddContentToPage() && Globals.IsEditMode() && !isSpecialPageMode)
                {
                    // Register Drag and Drop plugin
                    JavaScript.RequestRegistration(this.appStatus, this.eventLogger, page.PortalSettings, CommonJs.DnnPlugins);

                    ctlSkin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = "~/resources/shared/stylesheets/dnn.dragDrop.css", FileOrder = FileOrder.Css.FeatureCss });
                    ctlSkin.RegisteredScripts.Add(new RegisteredScript() { Script = "~/resources/shared/scripts/dnn.dragDrop.js" });

                    // Register Client Script
                    var sb = new StringBuilder();
                    sb.AppendLine(" (function ($) {");
                    sb.AppendLine("     $(document).ready(function () {");
                    sb.AppendLine("         $('.dnnSortable').dnnModuleDragDrop({");
                    sb.AppendLine("             tabId: " + TabController.CurrentPage.TabID + ",");
                    sb.AppendLine("             draggingHintText: '" + Localization.GetSafeJSString("DraggingHintText", Localization.GlobalResourceFile) + "',");
                    sb.AppendLine("             dragHintText: '" + Localization.GetSafeJSString("DragModuleHint", Localization.GlobalResourceFile) + "',");
                    sb.AppendLine("             dropHintText: '" + Localization.GetSafeJSString("DropModuleHint", Localization.GlobalResourceFile) + "',");
                    sb.AppendLine("             dropTargetText: '" + Localization.GetSafeJSString("DropModuleTarget", Localization.GlobalResourceFile) + "'");
                    sb.AppendLine("         });");
                    sb.AppendLine("     });");
                    sb.AppendLine(" } (jQuery));");

                    var script = sb.ToString();
                    MvcClientAPI.RegisterStartupScript("DragAndDrop", script);
                }
            }
            catch (MvcPageException mvcExc)
            {
                throw new MvcPageException("LoadSkin", mvcExc);
            }
            catch (Exception exc)
            {
                // could not load user control
                var lex = new PageLoadException("Unhandled error loading page.", exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    ctlSkin.SkinError = string.Format(Localization.GetString("SkinLoadError", Localization.GlobalResourceFile), skinPath, page.Server.HtmlEncode(exc.Message));
                }

                Exceptions.LogException(lex);
            }

            return ctlSkin;
        }

        private bool ProcessMasterModules(DnnPageController page, SkinModel skin)
        {
            var success = true;
            var portalSettings = page.PortalSettings;
            var currentTab = TabController.CurrentPage;
            if (TabPermissionController.CanViewPage())
            {
                // We need to ensure that Content Item exists since in old versions Content Items are not needed for tabs
                this.EnsureContentItemForTab(currentTab);

                // Versioning checks.
                if (!currentTab.HasAVisibleVersion)
                {
                    this.HandleAccesDenied(true);
                }

                int urlVersion;
                if (TabVersionUtils.TryGetUrlVersion(out urlVersion))
                {
                    if (!TabVersionUtils.CanSeeVersionedPages())
                    {
                        this.HandleAccesDenied(false);
                        return true;
                    }

                    if (TabVersionController.Instance.GetTabVersions(currentTab.TabID).All(tabVersion => tabVersion.Version != urlVersion))
                    {
                        throw new NotFoundException("ErrorPage404", this.navigationManager.NavigateURL(portalSettings.ErrorPage404, string.Empty, "status=404"));
                    }
                }

                // check portal expiry date
                if (!this.CheckExpired(PortalSettings.Current))
                {
                    if ((TabController.CurrentPage.StartDate < DateTime.Now && TabController.CurrentPage.EndDate > DateTime.Now) ||
                        TabPermissionController.CanAdminPage() ||
                        Globals.IsLayoutMode())
                    {
                        foreach (var objModule in PortalSettingsController.Instance().GetTabModules(PortalSettings.Current))
                        {
                            success = this.ProcessModule(page, PortalSettings.Current, skin, objModule);
                        }
                    }
                    else
                    {
                        this.HandleAccesDenied(false);
                    }
                }
                else
                {
                    this.pageService.AddErrorMessage(
                        string.Empty,
                        string.Format(Localization.GetString("ContractExpired.Error"), portalSettings.PortalName, Globals.GetMediumDate(portalSettings.ExpiryDate.ToString(CultureInfo.InvariantCulture)), portalSettings.Email));
                }
            }
            else
            {
                // If request localized page which haven't complete translate yet, redirect to default language version.
                var redirectUrl = Globals.AccessDeniedURL(Localization.GetString("TabAccess.Error"));

                // Current locale will use default if did'nt find any
                var currentLocale = LocaleController.Instance.GetCurrentLocale(portalSettings.PortalId);
                if (portalSettings.ContentLocalizationEnabled &&
                    TabController.CurrentPage.CultureCode != currentLocale.Code)
                {
                    redirectUrl = new LanguageTokenReplace { Language = currentLocale.Code }.ReplaceEnvironmentTokens("[URL]");
                }

                throw new AccesDeniedException(Localization.GetString("TabAccess.Error"), redirectUrl);
            }

            return success;
        }

        private bool ProcessSlaveModule(DnnPageController page, SkinModel skin)
        {
            var success = true;
            var key = UIUtilities.GetControlKey();
            var moduleId = UIUtilities.GetModuleId(key);
            var portalSettings = page.PortalSettings;
            var currentTab = TabController.CurrentPage;
            var slaveModule = UIUtilities.GetSlaveModule(moduleId, key, currentTab.TabID);

            PaneModel pane;
            skin.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            if (pane == null)
            {
                skin.Panes.Add(Globals.glbDefaultPane.ToLowerInvariant(), this.paneModelFactory.CreatePane(Globals.glbDefaultPane.ToLowerInvariant()));
                skin.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            }

            slaveModule.PaneName = Globals.glbDefaultPane;
            slaveModule.ContainerSrc = currentTab.ContainerSrc;
            if (string.IsNullOrEmpty(slaveModule.ContainerSrc))
            {
                slaveModule.ContainerSrc = portalSettings.DefaultPortalContainer;
            }

            slaveModule.ContainerSrc = SkinController.FormatSkinSrc(slaveModule.ContainerSrc, PortalSettings.Current);
            slaveModule.ContainerPath = SkinController.FormatSkinPath(slaveModule.ContainerSrc);

            var moduleControl = ModuleControlController.GetModuleControlByControlKey(key, slaveModule.ModuleDefID);
            if (moduleControl != null)
            {
                slaveModule.ModuleControlId = moduleControl.ModuleControlID;
                slaveModule.IconFile = moduleControl.IconFile;

                string permissionKey;
                switch (slaveModule.ModuleControl.ControlSrc)
                {
                    case "Admin/Modules/ModuleSettings.ascx":
                        permissionKey = "MANAGE";
                        break;
                    case "Admin/Modules/Import.ascx":
                        permissionKey = "IMPORT";
                        break;
                    case "Admin/Modules/Export.ascx":
                        permissionKey = "EXPORT";
                        break;
                    default:
                        permissionKey = "CONTENT";
                        break;
                }

                if (ModulePermissionController.HasModuleAccess(slaveModule.ModuleControl.ControlType, permissionKey, slaveModule))
                {
                    success = this.InjectModule(page, portalSettings, pane, slaveModule);
                }
                else
                {
                    var message = Localization.GetString("AccesDenied");
                    throw new AccesDeniedException(message, Globals.AccessDeniedURL(Localization.GetString("ModuleAccess.Error")));
                }
            }

            return success;
        }

        private bool ProcessModule(DnnPageController page, PortalSettings portalSettings, SkinModel skin, ModuleInfo module)
        {
            var success = true;
            var x = Globals.GetCurrentServiceProvider().GetService<ModuleInjectionManager>();
            if (x.CanInjectModule(module, portalSettings))
            {
                // We need to ensure that Content Item exists since in old versions Content Items are not needed for modules
                this.EnsureContentItemForModule(module);

                var pane = this.GetPane(skin, module);

                if (pane != null)
                {
                    success = this.InjectModule(page, portalSettings, pane, module);
                }
                else
                {
                    var lex = new ModuleLoadException(Localization.GetString("PaneNotFound.Error"));
                    Exceptions.LogException(lex);
                }
            }

            return success;
        }

        private PaneModel GetPane(SkinModel skin, ModuleInfo module)
        {
            PaneModel pane;
            var found = skin.Panes.TryGetValue(module.PaneName.ToLowerInvariant(), out pane);

            if (!found)
            {
                skin.Panes.Add(module.PaneName.ToLowerInvariant(), this.paneModelFactory.CreatePane(module.PaneName.ToLowerInvariant()));
                found = skin.Panes.TryGetValue(module.PaneName.ToLowerInvariant(), out pane);
            }

            return pane;
        }

        private void HandleAccesDenied(bool redirect)
        {
            var message = Localization.GetString("TabAccess.Error");
            if (redirect)
            {
                var redirectUrl = Globals.AccessDeniedURL(message);
                throw new AccesDeniedException(message, redirectUrl);
            }
            else
            {
                this.pageService.AddMessage(string.Empty, message, PageMessageType.Warning, string.Empty);
            }
        }

        private bool CheckExpired(PortalSettings portalSettings)
        {
            var blnExpired = false;
            if (portalSettings.ExpiryDate != Null.NullDate)
            {
                if (Convert.ToDateTime(portalSettings.ExpiryDate) < DateTime.Now && !Globals.IsHostTab(portalSettings.ActiveTab.TabID))
                {
                    blnExpired = true;
                }
            }

            return blnExpired;
        }

        private void EnsureContentItemForTab(TabInfo tabInfo)
        {
            // If tab exists but ContentItem not, then we create it
            if (tabInfo.ContentItemId == Null.NullInteger && tabInfo.TabID != Null.NullInteger)
            {
                TabController.Instance.CreateContentItem(tabInfo);
                TabController.Instance.UpdateTab(tabInfo);
            }
        }

        private void EnsureContentItemForModule(ModuleInfo module)
        {
            // If module exists but ContentItem not, then we create it
            if (module.ContentItemId == Null.NullInteger && module.ModuleID != Null.NullInteger)
            {
                ModuleController.Instance.CreateContentItem(module);
                ModuleController.Instance.UpdateModule(module);
            }
        }

        private void InjectControlPanel(SkinModel skin, HttpRequestBase request)
        {
            if (request.QueryString["dnnprintmode"] != "true" && !UrlUtils.InPopUp() && request.QueryString["hidecommandbar"] != "true")
            {
                if (ControlPanelBase.IsPageAdminInternal() || ControlPanelBase.IsModuleAdminInternal())
                {
                    // ControlPanel processing
                    skin.ControlPanelRazor = Path.GetFileNameWithoutExtension(this.hostSettings.ControlPanel);
                    ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                }
            }
        }

        private bool InjectModule(DnnPageController page, IPortalSettings portalSettings, PaneModel pane, ModuleInfo module)
        {
            var bSuccess = true;
            var currentTab = TabController.CurrentPage;

            // try to inject the module into the pane
            try
            {
                if (portalSettings.UserTabId != Null.NullInteger && (currentTab.TabID == portalSettings.UserTabId || currentTab.ParentId == portalSettings.UserTabId))
                {
                    // @todo - we should have a better way to determine whether the module is in profile page, instead of hardcoding the user tab check here, which is not accurate and may cause issue if user tab is changed or removed. We can consider to add a new property in ModuleInfo to indicate whether it's a profile module, and set it when loading modules.
                    /*
                    var profileModule = this.ModuleControlPipeline.LoadModuleControl(this.Page, module) as IProfileModule;
                    if (profileModule == null || profileModule.DisplayModule)
                    {
                        pane.InjectModule(module);
                    }
                    */
                }
                else
                {
                    this.paneModelFactory.InjectModule(page, pane, module, portalSettings);
                }
            }
            catch (ThreadAbortException)
            {
                // Response.Redirect may called in module control's OnInit method, so it will cause ThreadAbortException, no need any action here.
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                bSuccess = false;
            }

            return bSuccess;
        }

        private IFileInfo GetPageStylesheetFileInfo(string styleSheet, int portalId)
        {
            var cacheKey = string.Format(DataCache.PortalCacheKey, portalId, "PageStylesheet" + styleSheet);
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, styleSheet, portalId),
                this.GetPageStylesheetInfoCallBack);

            return file;
        }

        private IFileInfo GetPageStylesheetInfoCallBack(CacheItemArgs itemArgs)
        {
            var styleSheet = itemArgs.Params[0].ToString();
            return FileManager.Instance.GetFile((int)itemArgs.Params[1], styleSheet);
        }

        private string GetCssVariablesStylesheet(int portalId, Abstractions.Portals.IPortalStyles portalStyles, string homeSystemDirectory)
        {
            var cacheKey = string.Format(DataCache.PortalStylesCacheKey, portalId);
            var cacheArgs = new CacheItemArgs(
                cacheKey,
                DataCache.PortalCacheTimeOut,
                DataCache.PortalCachePriority,
                portalStyles,
                homeSystemDirectory);
            var filePath = CBO.GetCachedObject<string>(this.hostSettings, cacheArgs, this.GetCssVariablesStylesheetCallback);
            return filePath;
        }

        private string GetCssVariablesStylesheetCallback(CacheItemArgs args)
        {
            var portalStyles = (PortalStyles)args.Params[0];
            var directory = (string)args.Params[1];

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var webPath = $"{directory}{portalStyles.FileName}";

            var physicalPath = $"{directory}{portalStyles.FileName}";
            if (File.Exists(physicalPath))
            {
                return webPath;
            }

            var styles = portalStyles.ToString();
            File.WriteAllText(physicalPath, styles);

            return webPath;
        }
    }
}
