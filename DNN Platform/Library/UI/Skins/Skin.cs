#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Application;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Modules.Communications;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.ControlPanels;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Skins.EventListeners;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

using Microsoft.VisualBasic;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.UI.Skins
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Skins
    /// Class	 : Skin
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Skin is the base for the Skins
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/04/2005	Documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class Skin : UserControlBase
    {
        #region Public Constants

        // ReSharper disable InconsistentNaming
        public static string MODULELOAD_ERROR = Localization.GetString("ModuleLoad.Error");
        public static string CONTAINERLOAD_ERROR = Localization.GetString("ContainerLoad.Error");
        public static string MODULEADD_ERROR = Localization.GetString("ModuleAdd.Error");

        public const string OnInitMessage = "Skin_InitMessage";
		public const string OnInitMessageType = "Skin_InitMessageType";

        private readonly ModuleCommunicate _communicator = new ModuleCommunicate();
        // ReSharper restore InconsistentNaming

        #endregion

        #region Private Members

        private ArrayList _actionEventListeners;
        private Control _controlPanel;
        private Dictionary<string, Pane> _panes;

        #endregion

        #region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ControlPanel container.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	12/04/2007  created
        ///     [cnurse]    04/17/2009  Refactored from Skin
        /// </history>
        /// -----------------------------------------------------------------------------
        internal Control ControlPanel
        {
            get
            {
                return _controlPanel ?? (_controlPanel = FindControl("ControlPanel"));
            }
        }

        #endregion

        #region Friend Properties
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleCommunicate instance for the skin
        /// </summary>
        /// <returns>The ModuleCommunicate instance for the Skin</returns>
        /// <history>
        /// 	[cnurse]	01/12/2009  created
        /// </history>
        internal ModuleCommunicate Communicator
        {
            get
            {
                return _communicator;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Panes.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	12/04/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        internal Dictionary<string, Pane> Panes
        {
            get
            {
                return _panes ?? (_panes = new Dictionary<string, Pane>());
            }
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an ArrayList of ActionEventListeners
        /// </summary>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public ArrayList ActionEventListeners
        {
            get
            {
                return _actionEventListeners ?? (_actionEventListeners = new ArrayList());
            }
            set
            {
                _actionEventListeners = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path for this skin
        /// </summary>
        /// <returns>A String</returns>
        /// <history>
        /// 	[cnurse]	12/05/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string SkinPath
        {
            get
            {
                return TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Source for this skin
        /// </summary>
        /// <returns>A String</returns>
        /// <history>
        /// 	[cnurse]	12/05/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string SkinSrc { get; set; }

        #endregion

        #region Private Methods

        private static void AddModuleMessage(Control control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType, string iconSrc)
        {
            if (control != null)
            {
                if (!String.IsNullOrEmpty(message))
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
            if (!String.IsNullOrEmpty(message))
            {
                Control contentPane = FindControlRecursive(control, Globals.glbDefaultPane);

                if (contentPane != null)
                {
                    ModuleMessage moduleMessage = GetModuleMessageControl(heading, message, moduleMessageType, iconSrc);
                    contentPane.Controls.AddAt(0, moduleMessage);
                }
            }
        }

        private static Control FindControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl.ID == controlID) return rootControl;

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn = FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null) return controlToReturn;
            }
            return null;
        }


        private bool CheckExpired()
        {
            bool blnExpired = false;
            if (PortalSettings.ExpiryDate != Null.NullDate)
            {
                if (Convert.ToDateTime(PortalSettings.ExpiryDate) < DateTime.Now && !Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                {
                    blnExpired = true;
                }
            }
            return blnExpired;
        }

        private Pane GetPane(ModuleInfo module)
        {
            Pane pane;
            bool found = Panes.TryGetValue(module.PaneName.ToLowerInvariant(), out pane);

            if (!found)
            {
                Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            }

            return pane;
        }

        private void InjectControlPanel()
        {
            //if querystring dnnprintmode=true, controlpanel will not be shown
            if (Request.QueryString["dnnprintmode"] != "true" && Request.QueryString["popUp"] != "true")
            {
                if ((ControlPanelBase.IsPageAdminInternal() || ControlPanelBase.IsModuleAdminInternal())) 
                {
                    //ControlPanel processing
                    var controlPanel = ControlUtilities.LoadControl<ControlPanelBase>(this, Host.ControlPanel);
                    var form = (HtmlForm)Parent.FindControl("Form");

                    if (controlPanel.IncludeInControlHierarchy)
                    {
                        //inject ControlPanel control into skin
                        if (ControlPanel == null)
                        {
                            if (form != null)
                            {
                                form.Controls.AddAt(0, controlPanel);
                            }
                            else
                            {
                                Page.Controls.AddAt(0, controlPanel);
                            }
                        }
                        else
                        {
                            if (form != null)
                            {
                                if (Host.ControlPanel.ToLowerInvariant().EndsWith("controlbar.ascx"))
                                {
                                    form.Controls.AddAt(0, controlPanel);
                                }
                                else
                                {
                                    ControlPanel.Controls.Add(controlPanel);
                                }
                            }
                            else
                            {
                                if (Host.ControlPanel.ToLowerInvariant().EndsWith("controlbar.ascx"))
                                {
                                    Page.Controls.AddAt(0, controlPanel);
                                }
                                else
                                {
                                    ControlPanel.Controls.Add(controlPanel);
                                }
                            }
                        }

                        //register admin.css
                        ClientResourceManager.RegisterAdminStylesheet(Page, Globals.HostPath + "admin.css");
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
            //iterate page controls
            foreach (Control ctlControl in Controls)
            {
                var objPaneControl = ctlControl as HtmlContainerControl;

                //Panes must be runat=server controls so they have to have an ID
                if (objPaneControl != null && !string.IsNullOrEmpty(objPaneControl.ID))
                {
                    //load the skin panes
                    switch (objPaneControl.TagName.ToLowerInvariant())
                    {
                        case "td":
                        case "div":
                        case "span":
                        case "p":
                            //content pane
                            if (objPaneControl.ID.ToLower() != "controlpanel")
                            {
                                //Add to the PortalSettings (for use in the Control Panel)
                                PortalSettings.ActiveTab.Panes.Add(objPaneControl.ID);

                                //Add to the Panes collection
                                Panes.Add(objPaneControl.ID.ToLowerInvariant(), new Pane(objPaneControl));
                            }
                            else
                            {
                                //Control Panel pane
                                _controlPanel = objPaneControl;
                            }
                            break;
                    }
                }
            }
        }

        private static Skin LoadSkin(PageBase page, string skinPath)
        {
            Skin ctlSkin = null;
            try
            {
                string skinSrc = skinPath;
                if (skinPath.ToLower().IndexOf(Globals.ApplicationPath, StringComparison.Ordinal) != -1)
                {
                    skinPath = skinPath.Remove(0, Globals.ApplicationPath.Length);
                }
                ctlSkin = ControlUtilities.LoadControl<Skin>(page, skinPath);
                ctlSkin.SkinSrc = skinSrc;
                //call databind so that any server logic in the skin is executed
                ctlSkin.DataBind();
            }
            catch (Exception exc)
            {
                //could not load user control
                var lex = new PageLoadException("Unhandled error loading page.", exc);
                if (TabPermissionController.CanAdminPage())
                {
                    //only display the error to administrators
                    var skinError = (Label)page.FindControl("SkinError");
                    skinError.Text = string.Format(Localization.GetString("SkinLoadError", Localization.GlobalResourceFile), skinPath, page.Server.HtmlEncode(exc.Message));
                    skinError.Visible = true;
                }
                Exceptions.LogException(lex);
            }
            return ctlSkin;
        }

        private bool ProcessModule(ModuleInfo module)
        {
            bool success = true;
            if (ModuleInjectionManager.CanInjectModule(module, PortalSettings))
            {
                Pane pane = GetPane(module);

                if (pane != null)
                {
                    success = InjectModule(pane, module);
                }
                else
                {
                    var lex = new ModuleLoadException(Localization.GetString("PaneNotFound.Error"));
                    Controls.Add(new ErrorContainer(PortalSettings, MODULELOAD_ERROR, lex).Container);
                    Exceptions.LogException(lex);
                }
            }
            return success;
        }

        private bool ProcessMasterModules()
        {
            bool success = true;
            if (TabPermissionController.CanViewPage())
            {
                //check portal expiry date
                if (!CheckExpired())
                {
                    if ((PortalSettings.ActiveTab.StartDate < DateAndTime.Now && PortalSettings.ActiveTab.EndDate > DateAndTime.Now) || TabPermissionController.CanAdminPage() || Globals.IsLayoutMode())
                    {
                        //dynamically populate the panes with modules
                        if (PortalSettings.ActiveTab.Modules.Count > 0)
                        {
                            foreach (ModuleInfo objModule in PortalSettings.ActiveTab.Modules)
                            {
                                success = ProcessModule(objModule);
                            }
                        }
                    }
                    else
                    {
                        AddPageMessage(this, "", Localization.GetString("TabAccess.Error"), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                }
                else
                {
                    AddPageMessage(this,
                                   "",
                                   string.Format(Localization.GetString("ContractExpired.Error"), PortalSettings.PortalName, Globals.GetMediumDate(PortalSettings.ExpiryDate.ToString()), PortalSettings.Email),
                                   ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                Response.Redirect(Globals.AccessDeniedURL(Localization.GetString("TabAccess.Error")), true);
            }
            return success;
        }

        private void ProcessPanes()
        {
            foreach (KeyValuePair<string, Pane> kvp in Panes)
            {
                kvp.Value.ProcessPane();
            }
        }

        private bool ProcessSlaveModule()
        {
            var success = true;
            var key = UIUtilities.GetControlKey();
            var moduleId = UIUtilities.GetModuleId(key);
            var slaveModule = UIUtilities.GetSlaveModule(moduleId, key, PortalSettings.ActiveTab.TabID);

            Pane pane;
            Panes.TryGetValue(Globals.glbDefaultPane.ToLowerInvariant(), out pane);
            slaveModule.PaneName = Globals.glbDefaultPane;
            slaveModule.ContainerSrc = PortalSettings.ActiveTab.ContainerSrc;
            if (String.IsNullOrEmpty(slaveModule.ContainerSrc))
            {
                slaveModule.ContainerSrc = PortalSettings.DefaultPortalContainer;
            }
            slaveModule.ContainerSrc = SkinController.FormatSkinSrc(slaveModule.ContainerSrc, PortalSettings);
            slaveModule.ContainerPath = SkinController.FormatSkinPath(slaveModule.ContainerSrc);

            var moduleControl = ModuleControlController.GetModuleControlByControlKey(key, slaveModule.ModuleDefID);
            if (moduleControl != null)
            {
                slaveModule.ModuleControlId = moduleControl.ModuleControlID;
                slaveModule.IconFile = moduleControl.IconFile;
                if (ModulePermissionController.HasModuleAccess(slaveModule.ModuleControl.ControlType, Null.NullString, slaveModule))
                {
                    success = InjectModule(pane, slaveModule);
                }
                else
                {
                    Response.Redirect(Globals.AccessDeniedURL(Localization.GetString("ModuleAccess.Error")), true);
                }
            }

            return success;
        }

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnInit runs when the Skin is initialised.
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/04/2005	Documented
        ///     [cnurse]    12/05/2007  Refactored
        ///     [cnurse]    04/17/2009  Refactored to use SkinAdapter
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Load the Panes
            LoadPanes();

            //Load the Module Control(s)
            bool success = Globals.IsAdminControl() ? ProcessSlaveModule() : ProcessMasterModules();

            //Load the Control Panel
            InjectControlPanel();

            //Register any error messages on the Skin
            if (Request.QueryString["error"] != null)
            {
                AddPageMessage(this, Localization.GetString("CriticalError.Error"), Server.HtmlEncode(Request.QueryString["error"]), ModuleMessage.ModuleMessageType.RedError);
            }

            if (!TabPermissionController.CanAdminPage() && !success)
            {
                //only display the warning to non-administrators (administrators will see the errors)
                AddPageMessage(this, Localization.GetString("ModuleLoadWarning.Error"), string.Format(Localization.GetString("ModuleLoadWarning.Text"), PortalSettings.Email), ModuleMessage.ModuleMessageType.YellowWarning);
            }

            InvokeSkinEvents(SkinEventType.OnSkinInit);

            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(OnInitMessage))
            {
	            var messageType = ModuleMessage.ModuleMessageType.YellowWarning;
				if (HttpContext.Current.Items.Contains(OnInitMessageType))
				{
					messageType = (ModuleMessage.ModuleMessageType)Enum.Parse(typeof (ModuleMessage.ModuleMessageType), HttpContext.Current.Items[OnInitMessageType].ToString(), true);
				}
				AddPageMessage(this, string.Empty, HttpContext.Current.Items[OnInitMessage].ToString(), messageType);
            }

            //Process the Panes attributes
            ProcessPanes();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs when the Skin is loaded.
        /// </summary>
        /// <history>
        ///     [cnurse]    04/17/2009  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InvokeSkinEvents(SkinEventType.OnSkinLoad);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the Skin is rendered.
        /// </summary>
        /// <history>
        ///     [cnurse]    04/17/2009  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            InvokeSkinEvents(SkinEventType.OnSkinPreRender);

            if(TabPermissionController.CanAddContentToPage() && Globals.IsEditMode() && !HttpContext.Current.Request.Url.ToString().Contains("popUp=true"))
            {
                //Register Drag and Drop plugin
                jQuery.RegisterDnnJQueryPlugins(Page);
                ClientResourceManager.RegisterStyleSheet(Page, "~/resources/shared/stylesheets/dnn.dragDrop.css", FileOrder.Css.FeatureCss);
                ClientResourceManager.RegisterScript(Page, "~/resources/shared/scripts/dnn.dragDrop.js");

                //Register Client Script
                var sb = new StringBuilder();
                sb.AppendLine(" (function ($) {");
                sb.AppendLine("     $(document).ready(function () {");
                sb.AppendLine("         $('.dnnSortable').dnnModuleDragDrop({");
                sb.AppendLine("             tabId: " + PortalSettings.ActiveTab.TabID + ",");
                sb.AppendLine("             draggingHintText: '" + Localization.GetSafeJSString("DraggingHintText", Localization.GlobalResourceFile) + "',");
                sb.AppendLine("             dragHintText: '" + Localization.GetSafeJSString("DragModuleHint", Localization.GlobalResourceFile) + "',");
                sb.AppendLine("             dropHintText: '" + Localization.GetSafeJSString("DropModuleHint", Localization.GlobalResourceFile) + "',");
                sb.AppendLine("             dropTargetText: '" + Localization.GetSafeJSString("DropModuleTarget", Localization.GlobalResourceFile) + "'");
                sb.AppendLine("         });");
                sb.AppendLine("     });");
                sb.AppendLine(" } (jQuery));");

                var script = sb.ToString();
                if (ScriptManager.GetCurrent(Page) != null)
                {
                    // respect MS AJAX
                    ScriptManager.RegisterStartupScript(Page, GetType(), "DragAndDrop", script, true);
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(GetType(), "DragAndDrop", script, true);
                }
            }

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnUnLoad runs when the Skin is unloaded.
        /// </summary>
        /// <history>
        ///     [cnurse]    04/17/2009  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            InvokeSkinEvents(SkinEventType.OnSkinUnLoad);
        }

        #endregion

        #region Public Methods

        public static void AddModuleMessage(PortalModuleBase control, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, "", message, moduleMessageType, Null.NullString);
        }

        public static void AddModuleMessage(PortalModuleBase control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleMessage adds a Moduel Message control to the Skin
        /// </summary>
        /// <param name="message">The Message Text</param>
        /// <param name="control">The current control</param>
        /// <param name="moduleMessageType">The type of the message</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddModuleMessage(Control control, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, "", message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleMessage adds a Moduel Message control to the Skin
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="message">The Message Text</param>
        /// <param name="control">The current control</param>
        /// <param name="moduleMessageType">The type of the message</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddModuleMessage(Control control, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddModuleMessage(control, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="iconSrc">The Icon to diplay</param>
        /// <param name="message">The Message Text</param>
        /// <param name="page">The Page</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Page page, string heading, string message, string iconSrc)
        {
            AddPageMessage(page, heading, message, ModuleMessage.ModuleMessageType.GreenSuccess, iconSrc);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="iconSrc">The Icon to diplay</param>
        /// <param name="message">The Message Text</param>
        /// <param name="skin">The skin</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Skin skin, string heading, string message, string iconSrc)
        {
            AddPageMessage(skin, heading, message, ModuleMessage.ModuleMessageType.GreenSuccess, iconSrc);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="message">The Message Text</param>
        /// <param name="skin">The skin</param>
        /// <param name="moduleMessageType">The type of the message</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Skin skin, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddPageMessage(skin, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPageMessage adds a Page Message control to the Skin
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="message">The Message Text</param>
        /// <param name="page">The Page</param>
        /// <param name="moduleMessageType">The type of the message</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddPageMessage(Page page, string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            AddPageMessage(page, heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleMessageControl gets an existing Message Control and sets its properties
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="message">The Message Text</param>
        /// <param name="iconImage">The Message Icon</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ModuleMessage GetModuleMessageControl(string heading, string message, string iconImage)
        {
            return GetModuleMessageControl(heading, message, ModuleMessage.ModuleMessageType.GreenSuccess, iconImage);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleMessageControl gets an existing Message Control and sets its properties
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="message">The Message Text</param>
        /// <param name="moduleMessageType">The type of message</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ModuleMessage GetModuleMessageControl(string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            return GetModuleMessageControl(heading, message, moduleMessageType, Null.NullString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleMessageControl gets an existing Message Control and sets its properties
        /// </summary>
        /// <param name="heading">The Message Heading</param>
        /// <param name="message">The Message Text</param>
        /// <param name="iconImage">The Message Icon</param>
        /// <param name="moduleMessageType">The type of message</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ModuleMessage GetModuleMessageControl(string heading, string message, ModuleMessage.ModuleMessageType moduleMessageType, string iconImage)
        {

            //Use this to get a module message control
            //with a standard DotNetNuke icon
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
        /// GetParentSkin gets the Parent Skin for a control
        /// </summary>
        /// <param name="module">The control whose Parent Skin is requested</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Skin GetParentSkin(PortalModuleBase module)
        {
            return GetParentSkin(module as Control);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetParentSkin gets the Parent Skin for a control
        /// </summary>
        /// <param name="control">The control whose Parent Skin is requested</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Skin GetParentSkin(Control control)
        {
            return ControlUtilities.FindParentControl<Skin>(control);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetPopUpSkin gets the Skin that is used in modal popup.
        /// </summary>
        /// <param name="page">The Page</param>
        /// <history>
        /// 	[vnguyen]   06/07/2011      Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Skin GetPopUpSkin(PageBase page)
        {
            Skin skin = null;

            //attempt to find and load a popup skin from the assigned skinned source
            string skinSource = Globals.IsAdminSkin() ? SkinController.FormatSkinSrc(page.PortalSettings.DefaultAdminSkin, page.PortalSettings) : page.PortalSettings.ActiveTab.SkinSrc;
            if (!String.IsNullOrEmpty(skinSource))
            {
                skinSource = SkinController.FormatSkinSrc(SkinController.FormatSkinPath(skinSource) + "popUpSkin.ascx", page.PortalSettings);

                if (File.Exists(HttpContext.Current.Server.MapPath(SkinController.FormatSkinSrc(skinSource, page.PortalSettings))))
                {
                    skin = LoadSkin(page, skinSource);
                }
            }

            //error loading popup skin - load default popup skin
            if (skin == null)
            {
                skinSource = Globals.HostPath + "Skins/_default/popUpSkin.ascx";
                skin = LoadSkin(page, skinSource);
            }

            //set skin path
            page.PortalSettings.ActiveTab.SkinPath = SkinController.FormatSkinPath(skinSource);

            //set skin id to an explicit short name to reduce page payload and make it standards compliant
            skin.ID = "dnn";

            return skin;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSkin gets the Skin
        /// </summary>
        /// <param name="page">The Page</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Skin GetSkin(PageBase page)
        {
            Skin skin = null;
            string skinSource = Null.NullString;

            //skin preview
            if ((page.Request.QueryString["SkinSrc"] != null))
            {
                skinSource = SkinController.FormatSkinSrc(Globals.QueryStringDecode(page.Request.QueryString["SkinSrc"]) + ".ascx", page.PortalSettings);
                skin = LoadSkin(page, skinSource);
            }

            //load user skin ( based on cookie )
            if (skin == null)
            {
                HttpCookie skinCookie = page.Request.Cookies["_SkinSrc" + page.PortalSettings.PortalId];
                if (skinCookie != null)
                {
                    if (!String.IsNullOrEmpty(skinCookie.Value))
                    {
                        skinSource = SkinController.FormatSkinSrc(skinCookie.Value + ".ascx", page.PortalSettings);
                        skin = LoadSkin(page, skinSource);
                    }
                }
            }

            //load assigned skin
            if (skin == null)
            {
                skinSource = Globals.IsAdminSkin() ? SkinController.FormatSkinSrc(page.PortalSettings.DefaultAdminSkin, page.PortalSettings) : page.PortalSettings.ActiveTab.SkinSrc;
                if (!String.IsNullOrEmpty(skinSource))
                {
                    skinSource = SkinController.FormatSkinSrc(skinSource, page.PortalSettings);
                    skin = LoadSkin(page, skinSource);
                }
            }

            //error loading skin - load default
            if (skin == null)
            {
                skinSource = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalSkin(), page.PortalSettings);
                skin = LoadSkin(page, skinSource);
            }

            //set skin path
            page.PortalSettings.ActiveTab.SkinPath = SkinController.FormatSkinPath(skinSource);

            //set skin id to an explicit short name to reduce page payload and make it standards compliant
            skin.ID = "dnn";

            return skin;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InjectModule injects the module into the Pane
        /// </summary>
        /// <param name="module">The module to inject</param>
        /// <param name="pane">The pane</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  created
        ///     [cnurse]    04/17/2009  Refactored to use SkinAdapter
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool InjectModule(Pane pane, ModuleInfo module)
        {
            bool bSuccess = true;

            //try to inject the module into the pane
            try
            {
                if(PortalSettings.ActiveTab.TabID == PortalSettings.UserTabId || PortalSettings.ActiveTab.ParentId == PortalSettings.UserTabId)
                {
                    var profileModule = ModuleControlFactory.LoadModuleControl(Page, module) as IProfileModule;
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
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                bSuccess = false;
            }
            return bSuccess;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RegisterModuleActionEvent registers a Module Action Event
        /// </summary>
        /// <param name="moduleId">The ID of the module</param>
        /// <param name="e">An Action Event Handler</param>
        /// <history>
        /// 	[cnurse]	12/04/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public void RegisterModuleActionEvent(int moduleId, ActionEventHandler e)
        {
            ActionEventListeners.Add(new ModuleActionEventListener(moduleId, e));
        }



        #endregion
    }
}
