#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : Container
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Container is the base for the Containers
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/04/2005	Documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class Container : UserControl
    {
		#region Private Members
		
        private HtmlContainerControl _contentPane;
        private ModuleInfo _moduleConfiguration;
        private ModuleHost _moduleHost;

		#endregion

		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Content Pane Control (Id="ContentPane")
        /// </summary>
        /// <returns>An HtmlContainerControl</returns>
        /// <history>
        /// 	[cnurse]	12/05/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	01/12/2009  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	01/12/2009  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	06/10/2009  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ContainerSrc { get; set; }

        [Obsolete("Deprecated in 5.1. Replaced by ContainerPath")]
        public string SkinPath
        {
            get
            {
                return ContainerPath;
            }
        }

        internal bool InjectActionMenu { get; set; }
		
		#endregion

		#region Private Helper Methods

        private void AddAdministratorOnlyHighlighting(string message)
        {
            ContentPane.Controls.Add(new LiteralControl(string.Format("<div class=\"dnnFormMessage dnnFormInfo\">{0}</div>", message)));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessChildControls parses all the controls in the container, and if the
        /// control is an action (IActionControl) it attaches the ModuleControl (IModuleControl)
        /// and an EventHandler to respond to the Actions Action event.  If the control is a
        /// Container Object (IContainerControl) it attaches the ModuleControl.
        /// </summary>
        /// <history>
        /// 	[cnurse]	12/05/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
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
            if (viewRoles == PortalSettings.AdministratorRoleName.ToLowerInvariant()
                            && (moduleEditRoles == PortalSettings.AdministratorRoleName.ToLowerInvariant() 
                                    || String.IsNullOrEmpty(moduleEditRoles))
                            && pageEditRoles == PortalSettings.AdministratorRoleName.ToLowerInvariant())
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
        /// <history>
        /// 	[cnurse]	12/05/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/05/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void ProcessModule()
        {
            if (ContentPane != null)
            {
				//Process Content Pane Attributes
                ProcessContentPane();

                // always add the actions menu as the first item in the content pane.
                if (InjectActionMenu && !ModuleHost.IsViewMode(ModuleConfiguration, PortalSettings) && Request.QueryString["dnnprintmode"] != "true")
                {
                    ContentPane.Controls.Add(LoadControl("~/admin/Menus/ModuleActions/ModuleActions.ascx"));

                    //register admin.css
                    ClientResourceManager.RegisterAdminStylesheet(Page, Globals.HostPath + "admin.css");
                }

                //Process Module Header
                ProcessHeader();

                //Try to load the module control
                _moduleHost = new ModuleHost(ModuleConfiguration, ParentSkin, this);
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
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// ProcessStylesheets processes the Module and Container stylesheets and adds
		/// them to the Page.
		/// </summary>
		/// <history>
		/// 	[cnurse]	12/05/2007	Created
		/// </history>
		/// -----------------------------------------------------------------------------

        private void ProcessStylesheets(bool includeModuleCss)
        {
            ClientResourceManager.RegisterStyleSheet(Page, ContainerPath + "container.css", FileOrder.Css.ContainerCss);
            ClientResourceManager.RegisterStyleSheet(Page, ContainerSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificContainerCss);

            //process the base class module properties 
            if (includeModuleCss)
            {
                string controlSrc = ModuleConfiguration.ModuleControl.ControlSrc;
                string folderName = ModuleConfiguration.DesktopModule.FolderName;

                if (String.IsNullOrEmpty(folderName)==false)
                {
                    ClientResourceManager.RegisterStyleSheet(Page, Globals.ApplicationPath + "/DesktopModules/" + folderName.Replace("\\", "/") + "/module.css", FileOrder.Css.ModuleCss);
                }

                if (controlSrc.LastIndexOf("/") > 0)
                {
                    ClientResourceManager.RegisterStyleSheet(Page, Globals.ApplicationPath + "/" + controlSrc.Substring(0, controlSrc.LastIndexOf("/") + 1) + "module.css", FileOrder.Css.ModuleCss);
                }
            }
        }

        private void SetAlignment()
        {
            if (!String.IsNullOrEmpty(ModuleConfiguration.Alignment))
            {
                if (ContentPane.Attributes["class"] != null)
                {
                    ContentPane.Attributes["class"] = ContentPane.Attributes["class"] + " DNNAlign" + ModuleConfiguration.Alignment.ToLower();
                }
                else
                {
                    ContentPane.Attributes["class"] = "DNNAlign" + ModuleConfiguration.Alignment.ToLower();
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
        /// <history>
        /// 	[cnurse]	07/04/2005	Documented
        ///     [cnurse]    12/05/2007  Refactored
        ///     [cnurse]    04/17/2009  Refactored to use ContainerAdapter
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InvokeContainerEvents(ContainerEventType.OnContainerInit);
        }

        /// <summary>
        /// OnLoad runs when the Container is loaded.
        /// </summary>
        /// <history>
        ///     [cnurse]    04/17/2009  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            InvokeContainerEvents(ContainerEventType.OnContainerLoad);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the Container is rendered.
        /// </summary>
        /// <history>
        ///     [cnurse]    04/17/2009  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            InvokeContainerEvents(ContainerEventType.OnContainerPreRender);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnUnLoad runs when the Container is unloaded.
        /// </summary>
        /// <history>
        ///     [cnurse]    04/17/2009  Created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	07/04/2005	Documented
        ///     [cnurse]    12/05/2007  Moved from Container.vb
        /// </history>
        /// -----------------------------------------------------------------------------
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

		#region Obsolete

        [Obsolete("Deprecated in 5.0. Shouldn't need to be used any more.  ContainerObjects (IContainerControl implementations) have a property ModuleControl.")]
        public static PortalModuleBase GetPortalModuleBase(UserControl control)
        {
            PortalModuleBase moduleControl = null;
            Panel panel;
            if (control is SkinObjectBase)
            {
                panel = (Panel) control.Parent.FindControl("ModuleContent");
            }
            else
            {
                panel = (Panel) control.FindControl("ModuleContent");
            }
            if (panel != null)
            {
                try
                {
                    moduleControl = (PortalModuleBase) panel.Controls[1];
                }
                catch
                {
					//check if it is nested within an UpdatePanel 
                    try
                    {
                        moduleControl = (PortalModuleBase) panel.Controls[0].Controls[0].Controls[1];
                    }
                    catch (Exception exc)
                    {
                        Exceptions.LogException(exc);
                    }
                }
            }
            return moduleControl ?? (new PortalModuleBase {ModuleConfiguration = new ModuleInfo()});
        }
		
		#endregion
    }
}
