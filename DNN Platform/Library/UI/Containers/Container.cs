#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

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
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.UI.Containers
{
    using Web.Client;

    /// <summary>
    /// Container is the base for the Containers
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Container : UserControl
    {
        #region Private Members

        private readonly ILog _tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private HtmlContainerControl _contentPane;
        private ModuleInfo _moduleConfiguration;
        private ModuleHost _moduleHost;

		#endregion

		#region Protected Properties

        /// <summary>
        /// Gets the Content Pane Control (Id="ContentPane")
        /// </summary>
        /// <returns>An HtmlContainerControl</returns>
        protected HtmlContainerControl ContentPane
        {
            get
            {
                return _contentPane ?? (_contentPane = FindControl(Globals.glbDefaultPane) as HtmlContainerControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Portal Settings for the current Portal
        /// </summary>
        /// <returns>A PortalSettings object</returns>
        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleControl object that this container is displaying
        /// </summary>
        /// <returns>A ModuleHost object</returns>
        public IModuleControl ModuleControl
        {
            get
            {
                IModuleControl moduleControl = null;
                if (ModuleHost != null)
                {
                    moduleControl = ModuleHost.ModuleControl;
                }
                return moduleControl;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ModuleInfo object that this container is displaying
        /// </summary>
        /// <returns>A ModuleInfo object</returns>
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return _moduleConfiguration;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleHost object that this container is displaying
        /// </summary>
        /// <returns>A ModuleHost object</returns>
        public ModuleHost ModuleHost
        {
            get
            {
                return _moduleHost;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Parent Container for this container
        /// </summary>
        /// <returns>A String</returns>
        public Skins.Skin ParentSkin
        {
            get
            {
				//This finds a reference to the containing skin
                return Skins.Skin.GetParentSkin(this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path for this container
        /// </summary>
        /// <returns>A String</returns>
        public string ContainerPath
        {
            get
            {
                return TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Source for this container
        /// </summary>
        /// <returns>A String</returns>
        public string ContainerSrc { get; set; }

        internal bool InjectActionMenu { get; set; }
		
		#endregion

		#region Private Helper Methods

        private void AddAdministratorOnlyHighlighting(string message)
        {
            ContentPane.Controls.Add(new LiteralControl(string.Format("<div class=\"dnnFormMessage dnnFormInfo dnnFormInfoAdminErrMssg\">{0}</div>", message)));
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
				//check if control is an action control
                actions = childControl as IActionControl;
                if (actions != null)
                {
                    actions.ModuleControl = ModuleControl;
                    actions.Action += ModuleActionClick;
                }

                //check if control is an actionLink control
                var actionLink = childControl as ActionLink;
                if (actionLink != null)
                {
                    actionLink.ModuleControl = ModuleControl;
                }

				//check if control is a skin control
                skinControl = childControl as ISkinControl;
                if (skinControl != null)
                {
                    skinControl.ModuleControl = ModuleControl;
                }
                if (childControl.HasControls())
                {
					//recursive call for child controls
                    ProcessChildControls(childControl);
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
            SetAlignment();

            SetBackground();

            SetBorder();

            //display visual indicator if module is only visible to administrators
			string viewRoles = ModuleConfiguration.InheritViewPermissions
                                   ? TabPermissionController.GetTabPermissions(ModuleConfiguration.TabID, ModuleConfiguration.PortalID).ToString("VIEW")
                                   : ModuleConfiguration.ModulePermissions.ToString("VIEW");

            string pageEditRoles = TabPermissionController.GetTabPermissions(ModuleConfiguration.TabID, ModuleConfiguration.PortalID).ToString("EDIT");
            string moduleEditRoles = ModuleConfiguration.ModulePermissions.ToString("EDIT");

            viewRoles = viewRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            pageEditRoles = pageEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            moduleEditRoles = moduleEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();

            var showMessage = false;
            var adminMessage = Null.NullString;
            if (viewRoles.Equals(PortalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase)
                            && (moduleEditRoles.Equals(PortalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase)
                                    || String.IsNullOrEmpty(moduleEditRoles))
                            && pageEditRoles.Equals(PortalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase))
            {
                adminMessage = Localization.GetString("ModuleVisibleAdministrator.Text");
                showMessage = !ModuleConfiguration.HideAdminBorder && !Globals.IsAdminControl();
            }
            if (ModuleConfiguration.StartDate >= DateTime.Now)
            {
                adminMessage = string.Format(Localization.GetString("ModuleEffective.Text"), ModuleConfiguration.StartDate);
                showMessage = !Globals.IsAdminControl();
            }
            if (ModuleConfiguration.EndDate <= DateTime.Now)
            {
                adminMessage = string.Format(Localization.GetString("ModuleExpired.Text"), ModuleConfiguration.EndDate);
                showMessage = !Globals.IsAdminControl();
            }
            if (showMessage)
            {
                AddAdministratorOnlyHighlighting(adminMessage);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessFooter adds an optional footer (and an End_Module comment)..
        /// </summary>
        private void ProcessFooter()
        {
			//inject the footer
            if (!String.IsNullOrEmpty(ModuleConfiguration.Footer))
            {
                var footer = new Literal {Text = ModuleConfiguration.Footer};
                ContentPane.Controls.Add(footer);
            }
			
            //inject an end comment around the module content
            if (!Globals.IsAdminControl())
            {
                ContentPane.Controls.Add(new LiteralControl("<!-- End_Module_" + ModuleConfiguration.ModuleID + " -->"));
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
				//inject a start comment around the module content
                ContentPane.Controls.Add(new LiteralControl("<!-- Start_Module_" + ModuleConfiguration.ModuleID + " -->"));
            }
			
            //inject the header
            if (!String.IsNullOrEmpty(ModuleConfiguration.Header))
            {
                var header = new Literal {Text = ModuleConfiguration.Header};
                ContentPane.Controls.Add(header);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessModule processes the module which is attached to this container
        /// </summary>
        private void ProcessModule()
        {
            if (_tracelLogger.IsDebugEnabled)
                _tracelLogger.Debug($"Container.ProcessModule Start (TabId:{PortalSettings.ActiveTab.TabID},ModuleID: {ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{ModuleConfiguration.ModuleDefinition.FriendlyName}')");

            if (ContentPane != null)
            {
				//Process Content Pane Attributes
                ProcessContentPane();

                // always add the actions menu as the first item in the content pane.
                if (InjectActionMenu && !ModuleHost.IsViewMode(ModuleConfiguration, PortalSettings) && Request.QueryString["dnnprintmode"] != "true")
                {
                    JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                    ContentPane.Controls.Add(LoadControl(PortalSettings.DefaultModuleActionMenu));

                    //register admin.css
                    ClientResourceManager.RegisterAdminStylesheet(Page, Globals.HostPath + "admin.css");
                }

                //Process Module Header
                ProcessHeader();

                //Try to load the module control
                _moduleHost = new ModuleHost(ModuleConfiguration, ParentSkin, this);
                if (_tracelLogger.IsDebugEnabled)
                    _tracelLogger.Debug($"Container.ProcessModule Info (TabId:{PortalSettings.ActiveTab.TabID},ModuleID: {ModuleConfiguration.ModuleDefinition.DesktopModuleID}): ControlPane.Controls.Add(ModuleHost:{_moduleHost.ID})");

                ContentPane.Controls.Add(ModuleHost);

                //Process Module Footer
                ProcessFooter();
				
				//Process the Action Controls
                if (ModuleHost != null && ModuleControl != null)
                {
                    ProcessChildControls(this);
                }
				
				//Add Module Stylesheets
                ProcessStylesheets(ModuleHost != null);
            }
            if (_tracelLogger.IsDebugEnabled)
                _tracelLogger.Debug($"Container.ProcessModule End (TabId:{PortalSettings.ActiveTab.TabID},ModuleID: {ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{ModuleConfiguration.ModuleDefinition.FriendlyName}')");
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// ProcessStylesheets processes the Module and Container stylesheets and adds
		/// them to the Page.
		/// </summary>
        private void ProcessStylesheets(bool includeModuleCss)
        {
            ClientResourceManager.RegisterStyleSheet(Page, ContainerPath + "container.css", FileOrder.Css.ContainerCss);
            ClientResourceManager.RegisterStyleSheet(Page, ContainerSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificContainerCss);

            //process the base class module properties 
            if (includeModuleCss)
            {
                string controlSrc = ModuleConfiguration.ModuleControl.ControlSrc;
                string folderName = ModuleConfiguration.DesktopModule.FolderName;

                string stylesheet = "";
                if (String.IsNullOrEmpty(folderName)==false)
                {
                    if (controlSrc.EndsWith(".mvc"))
                    {
                        stylesheet = Globals.ApplicationPath + "/DesktopModules/MVC/" + folderName.Replace("\\", "/") + "/module.css";
                    }
                    else
                    {
                        stylesheet = Globals.ApplicationPath + "/DesktopModules/" + folderName.Replace("\\", "/") + "/module.css";
                    }
                    ClientResourceManager.RegisterStyleSheet(Page, stylesheet, FileOrder.Css.ModuleCss);
                }
                var ix = controlSrc.LastIndexOf("/", StringComparison.Ordinal);
                if (ix >= 0)
                {
                    stylesheet = Globals.ApplicationPath + "/" + controlSrc.Substring(0, ix + 1) + "module.css";
                    ClientResourceManager.RegisterStyleSheet(Page, stylesheet, FileOrder.Css.ModuleCss);
                }
            }
        }

        private void SetAlignment()
        {
            if (!String.IsNullOrEmpty(ModuleConfiguration.Alignment))
            {
                if (ContentPane.Attributes["class"] != null)
                {
                    ContentPane.Attributes["class"] = ContentPane.Attributes["class"] + " DNNAlign" + ModuleConfiguration.Alignment.ToLowerInvariant();
                }
                else
                {
                    ContentPane.Attributes["class"] = "DNNAlign" + ModuleConfiguration.Alignment.ToLowerInvariant();
                }
            }
        }

        private void SetBackground()
        {
            if (!String.IsNullOrEmpty(ModuleConfiguration.Color))
            {
                ContentPane.Style["background-color"] = ModuleConfiguration.Color;
            }
        }

        private void SetBorder()
        {
            if (!String.IsNullOrEmpty(ModuleConfiguration.Border))
            {
                ContentPane.Style["border-top"] = String.Format("{0}px #000000 solid", ModuleConfiguration.Border);
                ContentPane.Style["border-bottom"] = String.Format("{0}px #000000 solid", ModuleConfiguration.Border);
                ContentPane.Style["border-right"] = String.Format("{0}px #000000 solid", ModuleConfiguration.Border);
                ContentPane.Style["border-left"] = String.Format("{0}px #000000 solid", ModuleConfiguration.Border);
            }
        }
		
		#endregion

		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnInit runs when the Container is initialised.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InvokeContainerEvents(ContainerEventType.OnContainerInit);
        }

        /// <summary>
        /// OnLoad runs when the Container is loaded.
        /// </summary>
       protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            InvokeContainerEvents(ContainerEventType.OnContainerLoad);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the Container is rendered.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            InvokeContainerEvents(ContainerEventType.OnContainerPreRender);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnUnLoad runs when the Container is unloaded.
        /// </summary>
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            InvokeContainerEvents(ContainerEventType.OnContainerUnLoad);
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
		
		#endregion

		#region Public Methods

        public void SetModuleConfiguration(ModuleInfo configuration)
        {
            _moduleConfiguration = configuration;
            ProcessModule();
        }

		#endregion

		#region Event Handlers

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
			//Search through the listeners
            foreach (ModuleActionEventListener listener in ParentSkin.ActionEventListeners)
            {			
				//If the associated module has registered a listener
                if (e.ModuleConfiguration.ModuleID == listener.ModuleID)
                {
					//Invoke the listener to handle the ModuleAction_Click event
                    listener.ActionEvent.Invoke(sender, e);
                }
            }
        }
		
		#endregion
    }
}
