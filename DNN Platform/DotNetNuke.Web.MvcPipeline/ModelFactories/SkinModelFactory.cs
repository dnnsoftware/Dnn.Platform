// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Extensions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Permissions;
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
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using DotNetNuke.Services.Pages;
    using System.Globalization;
    using DotNetNuke.Services.ClientDependency;

    /// <summary>
    /// Builds <see cref="SkinModel"/> instances from portal, tab, and module configuration.
    /// </summary>
    public class SkinModelFactory : ISkinModelFactory
    {
        public const string OnInitMessage = "Skin_InitMessage";
        public const string OnInitMessageType = "Skin_InitMessageType";

        private readonly INavigationManager navigationManager;
        private readonly IPaneModelFactory paneModelFactory;
        private readonly IPageService PageService;
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinModelFactory"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="paneModelFactory">The pane model factory.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="pageService">The page service used for messages and meta data.</param>
        public SkinModelFactory(
                                INavigationManager navigationManager, 
                                IPaneModelFactory paneModelFactory,
                                IClientResourceController clientResourceController,
                                IPageService pageService)
        {
            this.navigationManager = navigationManager;
            this.paneModelFactory = paneModelFactory;
            this.clientResourceController = clientResourceController;
            this.PageService = pageService;
        }

        /// <inheritdoc/>
        public SkinModel CreateSkinModel(DnnPageController page)
        {
            SkinModel skin = null;
            var skinSource = Null.NullString;

            if (page.PortalSettings.EnablePopUps && UrlUtils.InPopUp())
            {
                // attempt to find and load a popup skin from the assigned skinned source
                skinSource = Globals.IsAdminSkin() ? SkinController.FormatSkinSrc(page.PortalSettings.DefaultAdminSkin, page.PortalSettings) : page.PortalSettings.ActiveTab.SkinSrc;
                if (!string.IsNullOrEmpty(skinSource))
                {
                    skinSource = SkinController.FormatSkinSrc(SkinController.FormatSkinPath(skinSource) + "popUpSkin.ascx", page.PortalSettings);

                    if (File.Exists(HttpContext.Current.Server.MapPath(SkinController.FormatSkinSrc(skinSource, page.PortalSettings))))
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
                page.PortalSettings.ActiveTab.SkinPath = SkinController.FormatSkinPath(skinSource);
            }
            else
            {
                // skin preview
                if (page.Request.QueryString["SkinSrc"] != null)
                {
                    skinSource = SkinController.FormatSkinSrc(Globals.QueryStringDecode(page.Request.QueryString["SkinSrc"]) + ".ascx", page.PortalSettings);
                    skin = this.LoadSkin(page, skinSource);
                }

                // load user skin ( based on cookie )
                if (skin == null)
                {
                    var skinCookie = page.Request.Cookies["_SkinSrc" + page.PortalSettings.PortalId];
                    if (skinCookie != null)
                    {
                        if (!string.IsNullOrEmpty(skinCookie.Value))
                        {
                            skinSource = SkinController.FormatSkinSrc(skinCookie.Value + ".ascx", page.PortalSettings);
                            skin = this.LoadSkin(page, skinSource);
                        }
                    }
                }

                // load assigned skin
                if (skin == null)
                {
                    // DNN-6170 ensure skin value is culture specific
                    skinSource = Globals.IsAdminSkin() ? PortalController.GetPortalSetting("DefaultAdminSkin", page.PortalSettings.PortalId, Host.DefaultPortalSkin, page.PortalSettings.CultureCode) : page.PortalSettings.ActiveTab.SkinSrc;
                    if (!string.IsNullOrEmpty(skinSource))
                    {
                        skinSource = SkinController.FormatSkinSrc(skinSource, page.PortalSettings);
                        skin = this.LoadSkin(page, skinSource);
                    }
                }

                // error loading skin - load default
                if (skin == null)
                {
                    skinSource = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalSkin(), page.PortalSettings);
                    skin = this.LoadSkin(page, skinSource);
                }

                // set skin path
                page.PortalSettings.ActiveTab.SkinPath = SkinController.FormatSkinPath(skinSource);
            }

            if (page.PortalSettings.ActiveTab.DisableLink)
            {
                if (TabPermissionController.CanAdminPage())
                {
                    var heading = Localization.GetString("PageDisabled.Header");
                    var message = Localization.GetString("PageDisabled.Text");
                    this.PageService.AddWarningMessage(heading, message);
                }
            }


            // add CSS links
            this.clientResourceController.CreateStylesheet("~/Resources/Shared/stylesheets/dnndefault/10.0.0/default.css")
                .SetNameAndVersion("dnndefault", "10.0.0", false)
                .SetPriority(FileOrder.Css.DefaultCss)
                .Register();

            this.clientResourceController.RegisterStylesheet(string.Concat(page.PortalSettings.ActiveTab.SkinPath, "skin.css"), FileOrder.Css.SkinCss, true);
            this.clientResourceController.RegisterStylesheet(page.PortalSettings.ActiveTab.SkinSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificSkinCss, true);


            // register css variables
            var cssVariablesStyleSheet = this.GetCssVariablesStylesheet(page.PortalSettings.PortalId, page.PortalSettings.GetStyles(), page.PortalSettings.HomeSystemDirectory);
            skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = cssVariablesStyleSheet, FileOrder = FileOrder.Css.DefaultCss });

            // register the custom stylesheet of current page
            if (page.PortalSettings.ActiveTab.TabSettings.ContainsKey("CustomStylesheet") && !string.IsNullOrEmpty(page.PortalSettings.ActiveTab.TabSettings["CustomStylesheet"].ToString()))
            {
                var styleSheet = page.PortalSettings.ActiveTab.TabSettings["CustomStylesheet"].ToString();

                // Try and go through the FolderProvider first
                var stylesheetFile = this.GetPageStylesheetFileInfo(styleSheet, page.PortalSettings.PortalId);
                if (stylesheetFile != null)
                {
                    skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = FileManager.Instance.GetUrl(stylesheetFile), FileOrder = FileOrder.Css.DefaultCss });
                }
                else
                {
                    skin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = styleSheet, FileOrder = FileOrder.Css.DefaultCss });
                }
            }

            if (page.PortalSettings.EnablePopUps)
            {
                JavaScript.RequestRegistration(CommonJs.jQueryUI);
                var popupFilePath = HttpContext.Current.IsDebuggingEnabled
                                   ? "~/js/Debug/dnn.modalpopup.js"
                                   : "~/js/dnn.modalpopup.js";
                skin.RegisteredScripts.Add(new RegisteredScript() { Script = popupFilePath, FileOrder = FileOrder.Js.DnnModalPopup });
            }
            return skin;
        }

        private SkinModel LoadSkin(DnnPageController page, string skinPath)
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

                // Load the Panes
                this.LoadPanes(page.PortalSettings);

                // Load the Module Control(s)
                var success = Globals.IsAdminControl() ? this.ProcessSlaveModule(page.PortalSettings, ctlSkin) : this.ProcessMasterModules(page.PortalSettings, ctlSkin);

                // Load the Control Panel
                this.InjectControlPanel(ctlSkin, page.Request);

                
                // Register any error messages on the Skin
                if (page.Request.QueryString["error"] != null && Host.ShowCriticalErrors)
                {
                    this.PageService.AddErrorMessage(" ", Localization.GetString("CriticalError.Error"));

                    if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                    {
                        ServicesFramework.Instance.RequestAjaxScriptSupport();
                        ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

                        JavaScript.RequestRegistration(CommonJs.jQueryUI);
                        MvcJavaScript.RegisterClientReference(DotNetNuke.UI.Utilities.ClientAPI.ClientNamespaceReferences.dnn_dom);
                        this.clientResourceController.RegisterScript("~/resources/shared/scripts/dnn.logViewer.js");
                    }
                }
                
                if (!success && !TabPermissionController.CanAdminPage())
                {
                    // only display the warning to non-administrators (administrators will see the errors)
                    this.PageService.AddWarningMessage(Localization.GetString("ModuleLoadWarning.Error"), string.Format(Localization.GetString("ModuleLoadWarning.Text"), page.PortalSettings.Email));
                }

                
                if (HttpContext.Current != null && HttpContext.Current.Items.Contains(OnInitMessage))
                {
                    var messageType = PageMessageType.Warning;
                    if (HttpContext.Current.Items.Contains(OnInitMessageType))
                    {
                        messageType = (PageMessageType)Enum.Parse(typeof(PageMessageType), HttpContext.Current.Items[OnInitMessageType].ToString(), true);
                    }

                    this.PageService.AddMessage(new PageMessage(string.Empty, HttpContext.Current.Items[OnInitMessage].ToString(), messageType, string.Empty, PagePriority.Default));

                    JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                    ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                }
                

                // Process the Panes attributes
                foreach (var key in ctlSkin.Panes.Keys)
                {
                    this.paneModelFactory.ProcessPane(ctlSkin.Panes[key]);
                }

                var isSpecialPageMode = UrlUtils.InPopUp() || page.Request.QueryString["dnnprintmode"] == "true";
                if (TabPermissionController.CanAddContentToPage() && Globals.IsEditMode() && !isSpecialPageMode)
                {
                    // Register Drag and Drop plugin
                    JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                    // MvcClientResourceManager.RegisterStyleSheet(page.ControllerContext, "~/resources/shared/stylesheets/dnn.dragDrop.css", FileOrder.Css.FeatureCss);
                    ctlSkin.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = "~/resources/shared/stylesheets/dnn.dragDrop.css", FileOrder = FileOrder.Css.FeatureCss });

                    // MvcClientResourceManager.RegisterScript(page.ControllerContext, "~/resources/shared/scripts/dnn.dragDrop.js");
                    ctlSkin.RegisteredScripts.Add(new RegisteredScript() { Script = "~/resources/shared/scripts/dnn.dragDrop.js" });

                    // Register Client Script
                    var sb = new StringBuilder();
                    sb.AppendLine(" (function ($) {");
                    sb.AppendLine("     $(document).ready(function () {");
                    sb.AppendLine("         $('.dnnSortable').dnnModuleDragDrop({");
                    sb.AppendLine("             tabId: " + page.PortalSettings.ActiveTab.TabID + ",");
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
                throw mvcExc;
            }
            catch (Exception exc)
            {
                // could not load user control
                var lex = new PageLoadException("Unhandled error loading page.", exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    /*
                    var skinError = (Label)page.FindControl("SkinError");
                    skinError.Text = string.Format(Localization.GetString("SkinLoadError", Localization.GlobalResourceFile), skinPath, page.Server.HtmlEncode(exc.Message));
                    skinError.Visible = true;
                    */
                    ctlSkin.SkinError = string.Format(Localization.GetString("SkinLoadError", Localization.GlobalResourceFile), skinPath, page.Server.HtmlEncode(exc.Message));
                }

                Exceptions.LogException(lex);
            }

            return ctlSkin;
        }

        private void LoadPanes(PortalSettings portalSettings)
        {
            /*
            portalSettings.ActiveTab.Panes.Add("HeaderPane");
            portalSettings.ActiveTab.Panes.Add("ContentPane");
            portalSettings.ActiveTab.Panes.Add("ContentPaneLower");
            */

            /*
            // iterate page controls
            foreach (Control ctlControl in this.Controls)
            {
                var objPaneControl = ctlControl as HtmlContainerControl;

                // Panes must be runat=server controls so they have to have an ID
                if (objPaneControl != null && !string.IsNullOrEmpty(objPaneControl.ID))
                {
                    // load the skin panes
                    switch (objPaneControl.TagName.ToLowerInvariant())
                    {
                        case "td":
                        case "div":
                        case "span":
                        case "p":
                        case "section":
                        case "header":
                        case "footer":
                        case "main":
                        case "article":
                        case "aside":
                            // content pane
                            if (!objPaneControl.ID.Equals("controlpanel", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Add to the PortalSettings (for use in the Control Panel)
                                portalSettings.ActiveTab.Panes.Add(objPaneControl.ID);

                                // Add to the Panes collection
                                this.Panes.Add(objPaneControl.ID.ToLowerInvariant(), new Pane(objPaneControl));
                            }
                            else
                            {
                                // Control Panel pane
                                this.controlPanel = objPaneControl;
                            }

                            break;
                    }
                }
            }
            */
        }

        private bool ProcessMasterModules(PortalSettings portalSettings, SkinModel skin)
        {
            var success = true;
            if (TabPermissionController.CanViewPage())
            {
                // We need to ensure that Content Item exists since in old versions Content Items are not needed for tabs
                this.EnsureContentItemForTab(portalSettings.ActiveTab);

                // Versioning checks.
                if (!TabController.CurrentPage.HasAVisibleVersion)
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

                    if (TabVersionController.Instance.GetTabVersions(TabController.CurrentPage.TabID).All(tabVersion => tabVersion.Version != urlVersion))
                    {
                        throw new NotFoundException("ErrorPage404", this.navigationManager.NavigateURL(portalSettings.ErrorPage404, string.Empty, "status=404"));
                        /*
                        this.Response.Redirect(this.NavigationManager.NavigateURL(portalSettings.ErrorPage404, string.Empty, "status=404"));
                        */
                    }
                }

                // check portal expiry date
                if (!this.CheckExpired(portalSettings))
                {
                    if (portalSettings.ActiveTab.StartDate < DateTime.Now && portalSettings.ActiveTab.EndDate > DateTime.Now || TabPermissionController.CanAdminPage() || Globals.IsLayoutMode())
                    {
                        foreach (var objModule in PortalSettingsController.Instance().GetTabModules(portalSettings))
                        {
                            success = this.ProcessModule(portalSettings, skin, objModule);
                        }
                    }
                    else
                    {
                        this.HandleAccesDenied(false);
                    }
                }
                else
                {
                    this.PageService.AddErrorMessage(
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

                throw new AccesDeniedException("TabAccess.Error", redirectUrl);
                /*
                this.Response.Redirect(redirectUrl, true);
                */
            }

            return success;
        }

        private bool ProcessSlaveModule(PortalSettings portalSettings, SkinModel skin)
        {
            var success = true;
            var key = UIUtilities.GetControlKey();
            var moduleId = UIUtilities.GetModuleId(key);
            var slaveModule = UIUtilities.GetSlaveModule(moduleId, key, portalSettings.ActiveTab.TabID);

            PaneModel pane;
            skin.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            if (pane == null)
            {
                skin.Panes.Add(Globals.glbDefaultPane.ToLowerInvariant(), this.paneModelFactory.CreatePane(Globals.glbDefaultPane.ToLowerInvariant()));
                skin.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            }

            slaveModule.PaneName = Globals.glbDefaultPane;
            slaveModule.ContainerSrc = portalSettings.ActiveTab.ContainerSrc;
            if (string.IsNullOrEmpty(slaveModule.ContainerSrc))
            {
                slaveModule.ContainerSrc = portalSettings.DefaultPortalContainer;
            }

            slaveModule.ContainerSrc = SkinController.FormatSkinSrc(slaveModule.ContainerSrc, portalSettings);
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
                    success = this.InjectModule(portalSettings, pane, slaveModule);
                }
                else
                {
                    throw new AccesDeniedException("AccesDenied", Globals.AccessDeniedURL(Localization.GetString("ModuleAccess.Error")));
                    /*
                    this.Response.Redirect(Globals.AccessDeniedURL(Localization.GetString("ModuleAccess.Error")), true);
                    */
                }
            }

            return success;
        }

        private bool ProcessModule(PortalSettings portalSettings, SkinModel skin, ModuleInfo module)
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
                    success = this.InjectModule(portalSettings, pane, module);
                }
                else
                {
                    var lex = new ModuleLoadException(Localization.GetString("PaneNotFound.Error"));

                    // this.Controls.Add(new ErrorContainer(portalSettings, MODULELOAD_ERROR, lex).Container);
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
                // this.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
                skin.Panes.Add(module.PaneName.ToLowerInvariant(), this.paneModelFactory.CreatePane(module.PaneName.ToLowerInvariant()));
                found = skin.Panes.TryGetValue(module.PaneName.ToLowerInvariant(), out pane);
            }

            return pane;
        }

        private void HandleAccesDenied(bool v)
        {
            throw new NotImplementedException();
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
            // if querystring dnnprintmode=true, controlpanel will not be shown
            if (request.QueryString["dnnprintmode"] != "true" && !UrlUtils.InPopUp() && request.QueryString["hidecommandbar"] != "true")
            {
                // if (Host.AllowControlPanelToDetermineVisibility || (ControlPanelBase.IsPageAdminInternal() || ControlPanelBase.IsModuleAdminInternal()))
                if (ControlPanelBase.IsPageAdminInternal() || ControlPanelBase.IsModuleAdminInternal())
                {
                    // ControlPanel processing
                    skin.ControlPanelRazor = Path.GetFileNameWithoutExtension(Host.ControlPanel);
                    ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

                    /*
                    var controlPanel = ControlUtilities.LoadControl<ControlPanelBase>(this, Host.ControlPanel);
                    var form = (HtmlForm)this.Parent.FindControl("Form");

                    if (controlPanel.IncludeInControlHierarchy)
                    {
                        // inject ControlPanel control into skin
                        if (this.ControlPanel == null || HostController.Instance.GetBoolean("IgnoreControlPanelWrapper", false))
                        {
                            if (form != null)
                            {
                                form.Controls.AddAt(0, controlPanel);
                            }
                            else
                            {
                                this.Page.Controls.AddAt(0, controlPanel);
                            }
                        }
                        else
                        {
                            this.ControlPanel.Controls.Add(controlPanel);
                        }

                        // register admin.css
                        ClientResourceManager.RegisterAdminStylesheet(this.Page, Globals.HostPath + "admin.css");
                    }
                    */
                }
            }
        }

        private bool InjectModule(PortalSettings portalSettings, PaneModel pane, ModuleInfo module)
        {
            var bSuccess = true;

            // try to inject the module into the pane
            try
            {
                if (portalSettings.ActiveTab.TabID == portalSettings.UserTabId || portalSettings.ActiveTab.ParentId == portalSettings.UserTabId)
                {
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
                    this.paneModelFactory.InjectModule(pane, module, portalSettings);
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
            var filePath = CBO.GetCachedObject<string>(cacheArgs, this.GetCssVariablesStylesheetCallback);
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
