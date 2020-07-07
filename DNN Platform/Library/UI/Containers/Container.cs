// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Application;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers.EventListeners;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// Container is the base for the Containers.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Container : UserControl
    {
        private readonly ILog _tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private HtmlContainerControl _contentPane;
        private ModuleInfo _moduleConfiguration;
        private ModuleHost _moduleHost;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleControl object that this container is displaying.
        /// </summary>
        /// <returns>A ModuleHost object.</returns>
        public IModuleControl ModuleControl
        {
            get
            {
                IModuleControl moduleControl = null;
                if (this.ModuleHost != null)
                {
                    moduleControl = this.ModuleHost.ModuleControl;
                }

                return moduleControl;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ModuleInfo object that this container is displaying.
        /// </summary>
        /// <returns>A ModuleInfo object.</returns>
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this._moduleConfiguration;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleHost object that this container is displaying.
        /// </summary>
        /// <returns>A ModuleHost object.</returns>
        public ModuleHost ModuleHost
        {
            get
            {
                return this._moduleHost;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Parent Container for this container.
        /// </summary>
        /// <returns>A String.</returns>
        public Skins.Skin ParentSkin
        {
            get
            {
                // This finds a reference to the containing skin
                return Skins.Skin.GetParentSkin(this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path for this container.
        /// </summary>
        /// <returns>A String.</returns>
        public string ContainerPath
        {
            get
            {
                return this.TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Source for this container.
        /// </summary>
        /// <returns>A String.</returns>
        public string ContainerSrc { get; set; }

        internal bool InjectActionMenu { get; set; }

        /// <summary>
        /// Gets the Content Pane Control (Id="ContentPane").
        /// </summary>
        /// <returns>An HtmlContainerControl.</returns>
        protected HtmlContainerControl ContentPane
        {
            get
            {
                return this._contentPane ?? (this._contentPane = this.FindControl(Globals.glbDefaultPane) as HtmlContainerControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Portal Settings for the current Portal.
        /// </summary>
        /// <returns>A PortalSettings object.</returns>
        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public void SetModuleConfiguration(ModuleInfo configuration)
        {
            this._moduleConfiguration = configuration;
            this.ProcessModule();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnInit runs when the Container is initialised.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerInit);
        }

        /// <summary>
        /// OnLoad runs when the Container is loaded.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerLoad);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the Container is rendered.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerPreRender);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnUnLoad runs when the Container is unloaded.
        /// </summary>
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerUnLoad);
        }

        private void AddAdministratorOnlyHighlighting(string message)
        {
            this.ContentPane.Controls.Add(new LiteralControl(string.Format("<div class=\"dnnFormMessage dnnFormInfo dnnFormInfoAdminErrMssg\">{0}</div>", message)));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessChildControls parses all the controls in the container, and if the
        /// control is an action (IActionControl) it attaches the ModuleControl (IModuleControl)
        /// and an EventHandler to respond to the Actions Action event.  If the control is a
        /// Container Object (IContainerControl) it attaches the ModuleControl.
        /// </summary>
        private void ProcessChildControls(Control control)
        {
            IActionControl actions;
            ISkinControl skinControl;
            foreach (Control childControl in control.Controls)
            {
                // check if control is an action control
                actions = childControl as IActionControl;
                if (actions != null)
                {
                    actions.ModuleControl = this.ModuleControl;
                    actions.Action += this.ModuleActionClick;
                }

                // check if control is an actionLink control
                var actionLink = childControl as ActionLink;
                if (actionLink != null)
                {
                    actionLink.ModuleControl = this.ModuleControl;
                }

                // check if control is a skin control
                skinControl = childControl as ISkinControl;
                if (skinControl != null)
                {
                    skinControl.ModuleControl = this.ModuleControl;
                }

                if (childControl.HasControls())
                {
                    // recursive call for child controls
                    this.ProcessChildControls(childControl);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessContentPane processes the ContentPane, setting its style and other
        /// attributes.
        /// </summary>
        private void ProcessContentPane()
        {
            this.SetAlignment();

            this.SetBackground();

            this.SetBorder();

            // display visual indicator if module is only visible to administrators
            string viewRoles = this.ModuleConfiguration.InheritViewPermissions
                                   ? TabPermissionController.GetTabPermissions(this.ModuleConfiguration.TabID, this.ModuleConfiguration.PortalID).ToString("VIEW")
                                   : this.ModuleConfiguration.ModulePermissions.ToString("VIEW");

            string pageEditRoles = TabPermissionController.GetTabPermissions(this.ModuleConfiguration.TabID, this.ModuleConfiguration.PortalID).ToString("EDIT");
            string moduleEditRoles = this.ModuleConfiguration.ModulePermissions.ToString("EDIT");

            viewRoles = viewRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            pageEditRoles = pageEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            moduleEditRoles = moduleEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();

            var showMessage = false;
            var adminMessage = Null.NullString;
            if (viewRoles.Equals(this.PortalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase)
                            && (moduleEditRoles.Equals(this.PortalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase)
                                    || string.IsNullOrEmpty(moduleEditRoles))
                            && pageEditRoles.Equals(this.PortalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase))
            {
                adminMessage = Localization.GetString("ModuleVisibleAdministrator.Text");
                showMessage = !this.ModuleConfiguration.HideAdminBorder && !Globals.IsAdminControl();
            }

            if (this.ModuleConfiguration.StartDate >= DateTime.Now)
            {
                adminMessage = string.Format(Localization.GetString("ModuleEffective.Text"), this.ModuleConfiguration.StartDate);
                showMessage = !Globals.IsAdminControl();
            }

            if (this.ModuleConfiguration.EndDate <= DateTime.Now)
            {
                adminMessage = string.Format(Localization.GetString("ModuleExpired.Text"), this.ModuleConfiguration.EndDate);
                showMessage = !Globals.IsAdminControl();
            }

            if (showMessage)
            {
                this.AddAdministratorOnlyHighlighting(adminMessage);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessFooter adds an optional footer (and an End_Module comment)..
        /// </summary>
        private void ProcessFooter()
        {
            // inject the footer
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Footer))
            {
                var footer = new Literal { Text = this.ModuleConfiguration.Footer };
                this.ContentPane.Controls.Add(footer);
            }

            // inject an end comment around the module content
            if (!Globals.IsAdminControl())
            {
                this.ContentPane.Controls.Add(new LiteralControl("<!-- End_Module_" + this.ModuleConfiguration.ModuleID + " -->"));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessHeader adds an optional header (and a Start_Module_ comment)..
        /// </summary>
        private void ProcessHeader()
        {
            if (!Globals.IsAdminControl())
            {
                // inject a start comment around the module content
                this.ContentPane.Controls.Add(new LiteralControl("<!-- Start_Module_" + this.ModuleConfiguration.ModuleID + " -->"));
            }

            // inject the header
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Header))
            {
                var header = new Literal { Text = this.ModuleConfiguration.Header };
                this.ContentPane.Controls.Add(header);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessModule processes the module which is attached to this container.
        /// </summary>
        private void ProcessModule()
        {
            if (this._tracelLogger.IsDebugEnabled)
            {
                this._tracelLogger.Debug($"Container.ProcessModule Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{this.ModuleConfiguration.ModuleDefinition.FriendlyName}')");
            }

            if (this.ContentPane != null)
            {
                // Process Content Pane Attributes
                this.ProcessContentPane();

                // always add the actions menu as the first item in the content pane.
                if (this.InjectActionMenu && !ModuleHost.IsViewMode(this.ModuleConfiguration, this.PortalSettings) && this.Request.QueryString["dnnprintmode"] != "true")
                {
                    JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                    this.ContentPane.Controls.Add(this.LoadControl(this.PortalSettings.DefaultModuleActionMenu));

                    // register admin.css
                    ClientResourceManager.RegisterAdminStylesheet(this.Page, Globals.HostPath + "admin.css");
                }

                // Process Module Header
                this.ProcessHeader();

                // Try to load the module control
                this._moduleHost = new ModuleHost(this.ModuleConfiguration, this.ParentSkin, this);
                if (this._tracelLogger.IsDebugEnabled)
                {
                    this._tracelLogger.Debug($"Container.ProcessModule Info (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): ControlPane.Controls.Add(ModuleHost:{this._moduleHost.ID})");
                }

                this.ContentPane.Controls.Add(this.ModuleHost);

                // Process Module Footer
                this.ProcessFooter();

                // Process the Action Controls
                if (this.ModuleHost != null && this.ModuleControl != null)
                {
                    this.ProcessChildControls(this);
                }

                // Add Module Stylesheets
                this.ProcessStylesheets(this.ModuleHost != null);
            }

            if (this._tracelLogger.IsDebugEnabled)
            {
                this._tracelLogger.Debug($"Container.ProcessModule End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{this.ModuleConfiguration.ModuleDefinition.FriendlyName}')");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessStylesheets processes the Module and Container stylesheets and adds
        /// them to the Page.
        /// </summary>
        private void ProcessStylesheets(bool includeModuleCss)
        {
            ClientResourceManager.RegisterStyleSheet(this.Page, this.ContainerPath + "container.css", FileOrder.Css.ContainerCss);
            ClientResourceManager.RegisterStyleSheet(this.Page, this.ContainerSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificContainerCss);

            // process the base class module properties
            if (includeModuleCss)
            {
                string controlSrc = this.ModuleConfiguration.ModuleControl.ControlSrc;
                string folderName = this.ModuleConfiguration.DesktopModule.FolderName;

                string stylesheet = string.Empty;
                if (string.IsNullOrEmpty(folderName) == false)
                {
                    if (controlSrc.EndsWith(".mvc"))
                    {
                        stylesheet = Globals.ApplicationPath + "/DesktopModules/MVC/" + folderName.Replace("\\", "/") + "/module.css";
                    }
                    else
                    {
                        stylesheet = Globals.ApplicationPath + "/DesktopModules/" + folderName.Replace("\\", "/") + "/module.css";
                    }

                    ClientResourceManager.RegisterStyleSheet(this.Page, stylesheet, FileOrder.Css.ModuleCss);
                }

                var ix = controlSrc.LastIndexOf("/", StringComparison.Ordinal);
                if (ix >= 0)
                {
                    stylesheet = Globals.ApplicationPath + "/" + controlSrc.Substring(0, ix + 1) + "module.css";
                    ClientResourceManager.RegisterStyleSheet(this.Page, stylesheet, FileOrder.Css.ModuleCss);
                }
            }
        }

        private void SetAlignment()
        {
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Alignment))
            {
                if (this.ContentPane.Attributes["class"] != null)
                {
                    this.ContentPane.Attributes["class"] = this.ContentPane.Attributes["class"] + " DNNAlign" + this.ModuleConfiguration.Alignment.ToLowerInvariant();
                }
                else
                {
                    this.ContentPane.Attributes["class"] = "DNNAlign" + this.ModuleConfiguration.Alignment.ToLowerInvariant();
                }
            }
        }

        private void SetBackground()
        {
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Color))
            {
                this.ContentPane.Style["background-color"] = this.ModuleConfiguration.Color;
            }
        }

        private void SetBorder()
        {
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Border))
            {
                this.ContentPane.Style["border-top"] = string.Format("{0}px #000000 solid", this.ModuleConfiguration.Border);
                this.ContentPane.Style["border-bottom"] = string.Format("{0}px #000000 solid", this.ModuleConfiguration.Border);
                this.ContentPane.Style["border-right"] = string.Format("{0}px #000000 solid", this.ModuleConfiguration.Border);
                this.ContentPane.Style["border-left"] = string.Format("{0}px #000000 solid", this.ModuleConfiguration.Border);
            }
        }

        private void InvokeContainerEvents(ContainerEventType containerEventType)
        {
            SharedList<ContainerEventListener> list = ((NaiveLockingList<ContainerEventListener>)DotNetNukeContext.Current.ContainerEventListeners).SharedList;

            using (list.GetReadLock())
            {
                foreach (var listener in list.Where(x => x.EventType == containerEventType))
                {
                    listener.ContainerEvent.Invoke(this, new ContainerEventArgs(this));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ModuleAction_Click runs when a ModuleAction is clicked.
        /// </summary>
        /// <remarks>The Module Action must be configured to fire an event (it may be configured
        /// to redirect to a new url).  The event handler finds the Parent Container and invokes each
        /// registered ModuleActionEventListener delegate.
        ///
        /// Note: with the refactoring of this to the Container, this could be handled at the container level.
        /// However, for legacy purposes this is left this way, as many modules would have registered their
        /// listeners on the Container directly, rather than through the helper method in PortalModuleBase.</remarks>
        private void ModuleActionClick(object sender, ActionEventArgs e)
        {
            // Search through the listeners
            foreach (ModuleActionEventListener listener in this.ParentSkin.ActionEventListeners)
            {
                // If the associated module has registered a listener
                if (e.ModuleConfiguration.ModuleID == listener.ModuleID)
                {
                    // Invoke the listener to handle the ModuleAction_Click event
                    listener.ActionEvent.Invoke(sender, e);
                }
            }
        }
    }
}
