// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Application;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Modules.Communications;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Skins.EventListeners;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualBasic;

    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Skins
    /// Class    : Skin
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Skin is the base for the Skins.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class Skin : UserControlBase
    {
        public const string OnInitMessage = "Skin_InitMessage";
        public const string OnInitMessageType = "Skin_InitMessageType";
        private ArrayList _actionEventListeners;
        private Control _controlPanel;
        private Dictionary<string, Pane> _panes;

        public Skin()
        {
            this.ModuleControlPipeline = Globals.DependencyProvider.GetRequiredService<IModuleControlPipeline>();
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Panes.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, Pane> Panes
        {
            get
            {
                return this._panes ?? (this._panes = new Dictionary<string, Pane>());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path for this skin.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string SkinPath
        {
            get
            {
                return this.TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets an ArrayList of ActionEventListeners.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ArrayList ActionEventListeners
        {
            get
            {
                return this._actionEventListeners ?? (this._actionEventListeners = new ArrayList());
            }

            set
            {
                this._actionEventListeners = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Source for this skin.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string SkinSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ControlPanel container.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        internal Control ControlPanel
        {
            get
            {
                return this._controlPanel ?? (this._controlPanel = this.FindControl("ControlPanel"));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleCommunicate instance for the skin.
        /// </summary>
        /// <returns>The ModuleCommunicate instance for the Skin.</returns>
        internal ModuleCommunicate Communicator
        {
            get
            {
                return this._communicator;
            }
        }

        protected IModuleControlPipeline ModuleControlPipeline { get; }

        protected INavigationManager NavigationManager { get; }

        public static void AddModuleMessage(PortalModuleBase control, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, string.Empty, message, moduleMessageType, Null.NullString);
        }

        public static void AddModuleMessage(PortalModuleBase control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleMessage adds a Moduel Message control to the Skin.
        /// </summary>
        /// <param name="message">The Message Text.</param>
        /// <param name="control">The current control.</param>
        /// <param name="moduleMessageType">The type of the message.</param>
        /// -----------------------------------------------------------------------------
        public static void AddModuleMessage(Control control, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, string.Empty, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleMessage adds a Moduel Message control to the Skin.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="control">The current control.</param>
        /// <param name="moduleMessageType">The type of the message.</param>
        /// -----------------------------------------------------------------------------
        public static void AddModuleMessage(Control control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="iconSrc">The Icon to diplay.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="page">The Page.</param>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Page page, string heading, string message, string iconSrc)
        {
            AddPageMessage(page, heading, message, ModuleMessage.ModuleMessageType.GreenSuccess, iconSrc);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="iconSrc">The Icon to diplay.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="skin">The skin.</param>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Skin skin, string heading, string message, string iconSrc)
        {
            AddPageMessage(skin, heading, message, ModuleMessage.ModuleMessageType.GreenSuccess, iconSrc);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="skin">The skin.</param>
        /// <param name="moduleMessageType">The type of the message.</param>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Skin skin, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddPageMessage(skin, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="page">The Page.</param>
        /// <param name="moduleMessageType">The type of the message.</param>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Page page, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddPageMessage(page, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleMessageControl gets an existing Message Control and sets its properties.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="iconImage">The Message Icon.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleMessage GetModuleMessageControl(string heading, string message, string iconImage)
        {
            return GetModuleMessageControl(heading, message, ModuleMessage.ModuleMessageType.GreenSuccess, iconImage);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleMessageControl gets an existing Message Control and sets its properties.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="moduleMessageType">The type of message.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleMessage GetModuleMessageControl(string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            return GetModuleMessageControl(heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleMessageControl gets an existing Message Control and sets its properties.
        /// </summary>
        /// <param name="heading">The Message Heading.</param>
        /// <param name="message">The Message Text.</param>
        /// <param name="iconImage">The Message Icon.</param>
        /// <param name="moduleMessageType">The type of message.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleMessage GetModuleMessageControl(string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType, string iconImage)
        {
            // Use this to get a module message control
            // with a standard DotNetNuke icon
            var s = new Skin();
            var moduleMessage = (ModuleMessage)s.LoadControl("~/admin/skins/ModuleMessage.ascx");
            moduleMessage.Heading = heading;
            moduleMessage.Text = message;
            moduleMessage.IconImage = iconImage;
            moduleMessage.IconType = moduleMessageType;
            return moduleMessage;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetParentSkin gets the Parent Skin for a control.
        /// </summary>
        /// <param name="module">The control whose Parent Skin is requested.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Skin GetParentSkin(PortalModuleBase module)
        {
            return GetParentSkin(module as Control);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetParentSkin gets the Parent Skin for a control.
        /// </summary>
        /// <param name="control">The control whose Parent Skin is requested.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Skin GetParentSkin(Control control)
        {
            return ControlUtilities.FindParentControl<Skin>(control);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetPopUpSkin gets the Skin that is used in modal popup.
        /// </summary>
        /// <param name="page">The Page.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Skin GetPopUpSkin(PageBase page)
        {
            Skin skin = null;

            // attempt to find and load a popup skin from the assigned skinned source
            string skinSource = Globals.IsAdminSkin() ? SkinController.FormatSkinSrc(page.PortalSettings.DefaultAdminSkin, page.PortalSettings) : page.PortalSettings.ActiveTab.SkinSrc;
            if (!string.IsNullOrEmpty(skinSource))
            {
                skinSource = SkinController.FormatSkinSrc(SkinController.FormatSkinPath(skinSource) + "popUpSkin.ascx", page.PortalSettings);

                if (File.Exists(HttpContext.Current.Server.MapPath(SkinController.FormatSkinSrc(skinSource, page.PortalSettings))))
                {
                    skin = LoadSkin(page, skinSource);
                }
            }

            // error loading popup skin - load default popup skin
            if (skin == null)
            {
                skinSource = Globals.HostPath + "Skins/_default/popUpSkin.ascx";
                skin = LoadSkin(page, skinSource);
            }

            // set skin path
            page.PortalSettings.ActiveTab.SkinPath = SkinController.FormatSkinPath(skinSource);

            // set skin id to an explicit short name to reduce page payload and make it standards compliant
            skin.ID = "dnn";

            return skin;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSkin gets the Skin.
        /// </summary>
        /// <param name="page">The Page.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Skin GetSkin(PageBase page)
        {
            Skin skin = null;
            string skinSource = Null.NullString;

            // skin preview
            if (page.Request.QueryString["SkinSrc"] != null)
            {
                skinSource = SkinController.FormatSkinSrc(Globals.QueryStringDecode(page.Request.QueryString["SkinSrc"]) + ".ascx", page.PortalSettings);
                skin = LoadSkin(page, skinSource);
            }

            // load user skin ( based on cookie )
            if (skin == null)
            {
                HttpCookie skinCookie = page.Request.Cookies["_SkinSrc" + page.PortalSettings.PortalId];
                if (skinCookie != null)
                {
                    if (!string.IsNullOrEmpty(skinCookie.Value))
                    {
                        skinSource = SkinController.FormatSkinSrc(skinCookie.Value + ".ascx", page.PortalSettings);
                        skin = LoadSkin(page, skinSource);
                    }
                }
            }

            // load assigned skin
            if (skin == null)
            {
                // DNN-6170 ensure skin value is culture specific
                // skinSource = Globals.IsAdminSkin() ? SkinController.FormatSkinSrc(page.PortalSettings.DefaultAdminSkin, page.PortalSettings) : page.PortalSettings.ActiveTab.SkinSrc;
                skinSource = Globals.IsAdminSkin() ? PortalController.GetPortalSetting("DefaultAdminSkin", page.PortalSettings.PortalId,
                    Host.DefaultPortalSkin, page.PortalSettings.CultureCode) : page.PortalSettings.ActiveTab.SkinSrc;
                if (!string.IsNullOrEmpty(skinSource))
                {
                    skinSource = SkinController.FormatSkinSrc(skinSource, page.PortalSettings);
                    skin = LoadSkin(page, skinSource);
                }
            }

            // error loading skin - load default
            if (skin == null)
            {
                skinSource = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalSkin(), page.PortalSettings);
                skin = LoadSkin(page, skinSource);
            }

            // set skin path
            page.PortalSettings.ActiveTab.SkinPath = SkinController.FormatSkinPath(skinSource);

            // set skin id to an explicit short name to reduce page payload and make it standards compliant
            skin.ID = "dnn";

            return skin;
        }

        public static List<InstalledSkinInfo> GetInstalledSkins()
        {
            var list = new List<InstalledSkinInfo>();
            foreach (string folder in Directory.GetDirectories(Path.Combine(Globals.HostMapPath, "Skins")))
            {
                if (!folder.EndsWith(Globals.glbHostSkinFolder))
                {
                    var skin = new InstalledSkinInfo();
                    skin.SkinName = folder.Substring(folder.LastIndexOf("\\") + 1);
                    skin.InUse = isFallbackSkin(folder) || !SkinController.CanDeleteSkin(folder, string.Empty);
                    list.Add(skin);
                }
            }

            return list;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InjectModule injects the module into the Pane.
        /// </summary>
        /// <param name="module">The module to inject.</param>
        /// <param name="pane">The pane.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool InjectModule(Pane pane, ModuleInfo module)
        {
            bool bSuccess = true;

            // try to inject the module into the pane
            try
            {
                if (this.PortalSettings.ActiveTab.TabID == this.PortalSettings.UserTabId || this.PortalSettings.ActiveTab.ParentId == this.PortalSettings.UserTabId)
                {
                    var profileModule = this.ModuleControlPipeline.LoadModuleControl(this.Page, module) as IProfileModule;
                    if (profileModule == null || profileModule.DisplayModule)
                    {
                        pane.InjectModule(module);
                    }
                }
                else
                {
                    pane.InjectModule(module);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RegisterModuleActionEvent registers a Module Action Event.
        /// </summary>
        /// <param name="moduleId">The ID of the module.</param>
        /// <param name="e">An Action Event Handler.</param>
        /// -----------------------------------------------------------------------------
        public void RegisterModuleActionEvent(int moduleId, ActionEventHandler e)
        {
            this.ActionEventListeners.Add(new ModuleActionEventListener(moduleId, e));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnInit runs when the Skin is initialised.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Load the Panes
            this.LoadPanes();

            // Load the Module Control(s)
            bool success = Globals.IsAdminControl() ? this.ProcessSlaveModule() : this.ProcessMasterModules();

            // Load the Control Panel
            this.InjectControlPanel();

            // Register any error messages on the Skin
            if (this.Request.QueryString["error"] != null && Host.ShowCriticalErrors)
            {
                AddPageMessage(this, Localization.GetString("CriticalError.Error"), " ", ModuleMessage.ModuleMessageType.RedError);

                if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                {
                    ServicesFramework.Instance.RequestAjaxScriptSupport();
                    ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

                    JavaScript.RequestRegistration(CommonJs.jQueryUI);
                    JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn_dom);
                    ClientResourceManager.RegisterScript(this.Page, "~/resources/shared/scripts/dnn.logViewer.js");
                }
            }

            if (!TabPermissionController.CanAdminPage() && !success)
            {
                // only display the warning to non-administrators (administrators will see the errors)
                AddPageMessage(this, Localization.GetString("ModuleLoadWarning.Error"), string.Format(Localization.GetString("ModuleLoadWarning.Text"), this.PortalSettings.Email), ModuleMessage.ModuleMessageType.YellowWarning);
            }

            this.InvokeSkinEvents(SkinEventType.OnSkinInit);

            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(OnInitMessage))
            {
                var messageType = ModuleMessage.ModuleMessageType.YellowWarning;
                if (HttpContext.Current.Items.Contains(OnInitMessageType))
                {
                    messageType = (ModuleMessage.ModuleMessageType)Enum.Parse(typeof(ModuleMessage.ModuleMessageType), HttpContext.Current.Items[OnInitMessageType].ToString(), true);
                }

                AddPageMessage(this, string.Empty, HttpContext.Current.Items[OnInitMessage].ToString(), messageType);

                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }

            // Process the Panes attributes
            this.ProcessPanes();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs when the Skin is loaded.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.InvokeSkinEvents(SkinEventType.OnSkinLoad);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the Skin is rendered.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.InvokeSkinEvents(SkinEventType.OnSkinPreRender);
            var isSpecialPageMode = UrlUtils.InPopUp() || this.Request.QueryString["dnnprintmode"] == "true";
            if (TabPermissionController.CanAddContentToPage() && Globals.IsEditMode() && !isSpecialPageMode)
            {
                // Register Drag and Drop plugin
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                ClientResourceManager.RegisterStyleSheet(this.Page, "~/resources/shared/stylesheets/dnn.dragDrop.css", FileOrder.Css.FeatureCss);
                ClientResourceManager.RegisterScript(this.Page, "~/resources/shared/scripts/dnn.dragDrop.js");

                // Register Client Script
                var sb = new StringBuilder();
                sb.AppendLine(" (function ($) {");
                sb.AppendLine("     $(document).ready(function () {");
                sb.AppendLine("         $('.dnnSortable').dnnModuleDragDrop({");
                sb.AppendLine("             tabId: " + this.PortalSettings.ActiveTab.TabID + ",");
                sb.AppendLine("             draggingHintText: '" + Localization.GetSafeJSString("DraggingHintText", Localization.GlobalResourceFile) + "',");
                sb.AppendLine("             dragHintText: '" + Localization.GetSafeJSString("DragModuleHint", Localization.GlobalResourceFile) + "',");
                sb.AppendLine("             dropHintText: '" + Localization.GetSafeJSString("DropModuleHint", Localization.GlobalResourceFile) + "',");
                sb.AppendLine("             dropTargetText: '" + Localization.GetSafeJSString("DropModuleTarget", Localization.GlobalResourceFile) + "'");
                sb.AppendLine("         });");
                sb.AppendLine("     });");
                sb.AppendLine(" } (jQuery));");

                var script = sb.ToString();
                if (ScriptManager.GetCurrent(this.Page) != null)
                {
                    // respect MS AJAX
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "DragAndDrop", script, true);
                }
                else
                {
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), "DragAndDrop", script, true);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnUnLoad runs when the Skin is unloaded.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            this.InvokeSkinEvents(SkinEventType.OnSkinUnLoad);
        }

        private static void AddModuleMessage(Control control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType, string iconSrc)
        {
            if (control != null)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    var messagePlaceHolder = ControlUtilities.FindControl<PlaceHolder>(control, "MessagePlaceHolder", true);
                    if (messagePlaceHolder != null)
                    {
                        messagePlaceHolder.Visible = true;
                        ModuleMessage moduleMessage = GetModuleMessageControl(heading, message, moduleMessageType, iconSrc);
                        messagePlaceHolder.Controls.Add(moduleMessage);
                    }
                }
            }
        }

        private static void AddPageMessage(Control control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType, string iconSrc)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Control contentPane = FindControlRecursive(control, Globals.glbDefaultPane);

                if (contentPane != null)
                {
                    ModuleMessage moduleMessage = GetModuleMessageControl(heading, message, moduleMessageType, iconSrc);
                    contentPane.Controls.AddAt(0, moduleMessage);
                }
            }
        }

        private static Control FindControlRecursive(Control rootControl, string controlId)
        {
            if (rootControl.ID == controlId)
            {
                return rootControl;
            }

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn = FindControlRecursive(controlToSearch, controlId);
                if (controlToReturn != null)
                {
                    return controlToReturn;
                }
            }

            return null;
        }

        private static Skin LoadSkin(PageBase page, string skinPath)
        {
            Skin ctlSkin = null;
            try
            {
                string skinSrc = skinPath;
                if (skinPath.IndexOf(Globals.ApplicationPath, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    skinPath = skinPath.Remove(0, Globals.ApplicationPath.Length);
                }

                ctlSkin = ControlUtilities.LoadControl<Skin>(page, skinPath);
                ctlSkin.SkinSrc = skinSrc;

                // call databind so that any server logic in the skin is executed
                ctlSkin.DataBind();
            }
            catch (Exception exc)
            {
                // could not load user control
                var lex = new PageLoadException("Unhandled error loading page.", exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    var skinError = (Label)page.FindControl("SkinError");
                    skinError.Text = string.Format(Localization.GetString("SkinLoadError", Localization.GlobalResourceFile), skinPath, page.Server.HtmlEncode(exc.Message));
                    skinError.Visible = true;
                }

                Exceptions.LogException(lex);
            }

            return ctlSkin;
        }

        private static bool isFallbackSkin(string skinPath)
        {
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            string defaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + defaultSkin.Folder).Replace("/", "\\");
            if (defaultSkinPath.EndsWith("\\"))
            {
                defaultSkinPath = defaultSkinPath.Substring(0, defaultSkinPath.Length - 1);
            }

            return skinPath.IndexOf(defaultSkinPath, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        private bool CheckExpired()
        {
            bool blnExpired = false;
            if (this.PortalSettings.ExpiryDate != Null.NullDate)
            {
                if (Convert.ToDateTime(this.PortalSettings.ExpiryDate) < DateTime.Now && !Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID))
                {
                    blnExpired = true;
                }
            }

            return blnExpired;
        }

        private Pane GetPane(ModuleInfo module)
        {
            Pane pane;
            bool found = this.Panes.TryGetValue(module.PaneName.ToLowerInvariant(), out pane);

            if (!found)
            {
                this.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            }

            return pane;
        }

        private void InjectControlPanel()
        {
            // if querystring dnnprintmode=true, controlpanel will not be shown
            if (this.Request.QueryString["dnnprintmode"] != "true" && !UrlUtils.InPopUp() && this.Request.QueryString["hidecommandbar"] != "true")
            {
                if (Host.AllowControlPanelToDetermineVisibility || (ControlPanelBase.IsPageAdminInternal() || ControlPanelBase.IsModuleAdminInternal()))
                {
                    // ControlPanel processing
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
                }
            }
        }

        private void InvokeSkinEvents(SkinEventType skinEventType)
        {
            SharedList<SkinEventListener> list = ((NaiveLockingList<SkinEventListener>)DotNetNukeContext.Current.SkinEventListeners).SharedList;

            using (list.GetReadLock())
            {
                foreach (var listener in list.Where(x => x.EventType == skinEventType))
                {
                    listener.SkinEvent.Invoke(this, new SkinEventArgs(this));
                }
            }
        }

        private void LoadPanes()
        {
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
                                this.PortalSettings.ActiveTab.Panes.Add(objPaneControl.ID);

                                // Add to the Panes collection
                                this.Panes.Add(objPaneControl.ID.ToLowerInvariant(), new Pane(objPaneControl));
                            }
                            else
                            {
                                // Control Panel pane
                                this._controlPanel = objPaneControl;
                            }

                            break;
                    }
                }
            }
        }

        private bool ProcessModule(ModuleInfo module)
        {
            var success = true;
            if (ModuleInjectionManager.CanInjectModule(module, this.PortalSettings))
            {
                // We need to ensure that Content Item exists since in old versions Content Items are not needed for modules
                this.EnsureContentItemForModule(module);

                Pane pane = this.GetPane(module);

                if (pane != null)
                {
                    success = this.InjectModule(pane, module);
                }
                else
                {
                    var lex = new ModuleLoadException(Localization.GetString("PaneNotFound.Error"));
                    this.Controls.Add(new ErrorContainer(this.PortalSettings, MODULELOAD_ERROR, lex).Container);
                    Exceptions.LogException(lex);
                }
            }

            return success;
        }

        /// <summary>
        /// Handle access denied errors by displaying an error message
        /// or by performing a redirect to a predefined "access denied URL".
        /// </summary>
        /// <param name="redirect"></param>
        private void HandleAccesDenied(bool redirect = false)
        {
            var message = Localization.GetString("TabAccess.Error");
            if (redirect)
            {
                var redirectUrl = Globals.AccessDeniedURL(message);
                this.Response.Redirect(redirectUrl, true);
            }
            else
            {
                AddPageMessage(this, string.Empty, message, ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        private bool ProcessMasterModules()
        {
            bool success = true;
            if (TabPermissionController.CanViewPage())
            {
                // We need to ensure that Content Item exists since in old versions Content Items are not needed for tabs
                this.EnsureContentItemForTab(this.PortalSettings.ActiveTab);

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
                        this.Response.Redirect(this.NavigationManager.NavigateURL(this.PortalSettings.ErrorPage404, string.Empty, "status=404"));
                    }
                }

                // check portal expiry date
                if (!this.CheckExpired())
                {
                    if ((this.PortalSettings.ActiveTab.StartDate < DateAndTime.Now && this.PortalSettings.ActiveTab.EndDate > DateAndTime.Now) || TabPermissionController.CanAdminPage() || Globals.IsLayoutMode())
                    {
                        foreach (var objModule in PortalSettingsController.Instance().GetTabModules(this.PortalSettings))
                        {
                            success = this.ProcessModule(objModule);
                        }
                    }
                    else
                    {
                        this.HandleAccesDenied(false);
                    }
                }
                else
                {
                    AddPageMessage(
                        this,
                        string.Empty,
                        string.Format(Localization.GetString("ContractExpired.Error"), this.PortalSettings.PortalName, Globals.GetMediumDate(this.PortalSettings.ExpiryDate.ToString(CultureInfo.InvariantCulture)), this.PortalSettings.Email),
                        ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                // If request localized page which haven't complete translate yet, redirect to default language version.
                var redirectUrl = Globals.AccessDeniedURL(Localization.GetString("TabAccess.Error"));

                // Current locale will use default if did'nt find any
                Locale currentLocale = LocaleController.Instance.GetCurrentLocale(this.PortalSettings.PortalId);
                if (this.PortalSettings.ContentLocalizationEnabled &&
                    TabController.CurrentPage.CultureCode != currentLocale.Code)
                {
                    redirectUrl = new LanguageTokenReplace { Language = currentLocale.Code }.ReplaceEnvironmentTokens("[URL]");
                }

                this.Response.Redirect(redirectUrl, true);
            }

            return success;
        }

        private void EnsureContentItemForTab(Entities.Tabs.TabInfo tabInfo)
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

        private void ProcessPanes()
        {
            foreach (KeyValuePair<string, Pane> kvp in this.Panes)
            {
                kvp.Value.ProcessPane();
            }
        }

        private bool ProcessSlaveModule()
        {
            var success = true;
            var key = UIUtilities.GetControlKey();
            var moduleId = UIUtilities.GetModuleId(key);
            var slaveModule = UIUtilities.GetSlaveModule(moduleId, key, this.PortalSettings.ActiveTab.TabID);

            Pane pane;
            this.Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            slaveModule.PaneName = Globals.glbDefaultPane;
            slaveModule.ContainerSrc = this.PortalSettings.ActiveTab.ContainerSrc;
            if (string.IsNullOrEmpty(slaveModule.ContainerSrc))
            {
                slaveModule.ContainerSrc = this.PortalSettings.DefaultPortalContainer;
            }

            slaveModule.ContainerSrc = SkinController.FormatSkinSrc(slaveModule.ContainerSrc, this.PortalSettings);
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
                    success = this.InjectModule(pane, slaveModule);
                }
                else
                {
                    this.Response.Redirect(Globals.AccessDeniedURL(Localization.GetString("ModuleAccess.Error")), true);
                }
            }

            return success;
        }

        // ReSharper disable InconsistentNaming
        public static string MODULELOAD_ERROR = Localization.GetString("ModuleLoad.Error");
        public static string CONTAINERLOAD_ERROR = Localization.GetString("ContainerLoad.Error");
        public static string MODULEADD_ERROR = Localization.GetString("ModuleAdd.Error");

        private readonly ModuleCommunicate _communicator = new ModuleCommunicate();

        // ReSharper restore InconsistentNaming
    }
}
