// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;

    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Skins
    /// Class    : Pane
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Pane class represents a Pane within the Skin.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class Pane
    {
        private const string CPaneOutline = "paneOutline";
        private HtmlGenericControl _containerWrapperControl;
        private Dictionary<string, Containers.Container> _containers;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Pane"/> class.
        /// Constructs a new Pane object from the Control in the Skin.
        /// </summary>
        /// <param name="pane">The HtmlContainerControl in the Skin.</param>
        /// -----------------------------------------------------------------------------
        public Pane(HtmlContainerControl pane)
        {
            this.PaneControl = pane;

            // Disable ViewState (we enable it later in the process)
            this.PaneControl.ViewStateMode = ViewStateMode.Disabled;
            this.Name = pane.ID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Pane"/> class.
        /// Constructs a new Pane object from the Control in the Skin.
        /// </summary>
        /// <param name="name">The name (ID) of the HtmlContainerControl.</param>
        /// <param name="pane">The HtmlContainerControl in the Skin.</param>
        /// -----------------------------------------------------------------------------
        public Pane(string name, HtmlContainerControl pane)
        {
            this.PaneControl = pane;
            this.Name = name;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Containers.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected Dictionary<string, Containers.Container> Containers
        {
            get
            {
                return this._containers ?? (this._containers = new Dictionary<string, Containers.Container>());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PortalSettings of the Portal.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the name (ID) of the Pane.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected string Name { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the HtmlContainerControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected HtmlContainerControl PaneControl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InjectModule injects a Module (and its container) into the Pane.
        /// </summary>
        /// <param name="module">The Module.</param>
        /// -----------------------------------------------------------------------------
        public void InjectModule(ModuleInfo module)
        {
            this._containerWrapperControl = new HtmlGenericControl("div");
            this.PaneControl.Controls.Add(this._containerWrapperControl);

            // inject module classes
            string classFormatString = "DnnModule DnnModule-{0} DnnModule-{1}";
            string sanitizedModuleName = Null.NullString;

            if (!string.IsNullOrEmpty(module.DesktopModule.ModuleName))
            {
                sanitizedModuleName = Globals.CreateValidClass(module.DesktopModule.ModuleName, false);
            }

            if (this.IsVesionableModule(module))
            {
                classFormatString += " DnnVersionableControl";
            }

            this._containerWrapperControl.Attributes["class"] = string.Format(classFormatString, sanitizedModuleName, module.ModuleID);

            try
            {
                if (!Globals.IsAdminControl() && (this.PortalSettings.InjectModuleHyperLink || this.PortalSettings.UserMode != PortalSettings.Mode.View))
                {
                    this._containerWrapperControl.Controls.Add(new LiteralControl("<a name=\"" + module.ModuleID + "\"></a>"));
                }

                // Load container control
                Containers.Container container = this.LoadModuleContainer(module);

                // Add Container to Dictionary
                this.Containers.Add(container.ID, container);

                // hide anything of type ActionsMenu - as we're injecting our own menu now.
                container.InjectActionMenu = container.Controls.OfType<ActionBase>().Count() == 0;
                if (!container.InjectActionMenu)
                {
                    foreach (var actionControl in container.Controls.OfType<IActionControl>())
                    {
                        if (actionControl is ActionsMenu)
                        {
                            Control control = actionControl as Control;
                            if (control != null)
                            {
                                control.Visible = false;
                                container.InjectActionMenu = true;
                            }
                        }
                    }
                }

                if (Globals.IsLayoutMode() && Globals.IsAdminControl() == false)
                {
                    // provide Drag-N-Drop capabilities
                    var dragDropContainer = new Panel();
                    Control title = container.FindControl("dnnTitle");

                    // Assume that the title control is named dnnTitle.  If this becomes an issue we could loop through the controls looking for the title type of skin object
                    dragDropContainer.ID = container.ID + "_DD";
                    this._containerWrapperControl.Controls.Add(dragDropContainer);

                    // inject the container into the page pane - this triggers the Pre_Init() event for the user control
                    dragDropContainer.Controls.Add(container);

                    if (title != null)
                    {
                        if (title.Controls.Count > 0)
                        {
                            title = title.Controls[0];
                        }
                    }

                    // enable drag and drop
                    if (title != null)
                    {
                        // The title ID is actually the first child so we need to make sure at least one child exists
                        DNNClientAPI.EnableContainerDragAndDrop(title, dragDropContainer, module.ModuleID);
                        ClientAPI.RegisterPostBackEventHandler(this.PaneControl, "MoveToPane", this.ModuleMoveToPanePostBack, false);
                    }
                }
                else
                {
                    this._containerWrapperControl.Controls.Add(container);
                    if (Globals.IsAdminControl())
                    {
                        this._containerWrapperControl.Attributes["class"] += " DnnModule-Admin";
                    }
                }

                // Attach Module to Container
                container.SetModuleConfiguration(module);

                // display collapsible page panes
                if (this.PaneControl.Visible == false)
                {
                    this.PaneControl.Visible = true;
                }
            }
            catch (ThreadAbortException)
            {
                // Response.Redirect may called in module control's OnInit method, so it will cause ThreadAbortException, no need any action here.
            }
            catch (Exception exc)
            {
                var lex = new ModuleLoadException(string.Format(Skin.MODULEADD_ERROR, this.PaneControl.ID), exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    this._containerWrapperControl.Controls.Add(new ErrorContainer(this.PortalSettings, Skin.MODULELOAD_ERROR, lex).Container);
                }

                Exceptions.LogException(exc);
                throw lex;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessPane processes the Attributes for the PaneControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ProcessPane()
        {
            if (this.PaneControl != null)
            {
                // remove excess skin non-validating attributes
                this.PaneControl.Attributes.Remove("ContainerType");
                this.PaneControl.Attributes.Remove("ContainerName");
                this.PaneControl.Attributes.Remove("ContainerSrc");

                if (Globals.IsLayoutMode())
                {
                    this.PaneControl.Visible = true;

                    // display pane border
                    string cssclass = this.PaneControl.Attributes["class"];
                    if (string.IsNullOrEmpty(cssclass))
                    {
                        this.PaneControl.Attributes["class"] = CPaneOutline;
                    }
                    else
                    {
                        this.PaneControl.Attributes["class"] = cssclass.Replace(CPaneOutline, string.Empty).Trim().Replace("  ", " ") + " " + CPaneOutline;
                    }

                    // display pane name
                    var ctlLabel = new Label { Text = "<center>" + this.Name + "</center><br />", CssClass = "SubHead" };
                    this.PaneControl.Controls.AddAt(0, ctlLabel);
                }
                else
                {
                    if (this.PaneControl.Visible == false && TabPermissionController.CanAddContentToPage())
                    {
                        this.PaneControl.Visible = true;
                    }

                    if (this.CanCollapsePane())
                    {
                        // This pane has no controls so set the width to 0
                        if (this.PaneControl.Attributes["style"] != null)
                        {
                            this.PaneControl.Attributes.Remove("style");
                        }

                        if (this.PaneControl.Attributes["class"] != null)
                        {
                            this.PaneControl.Attributes["class"] = this.PaneControl.Attributes["class"] + " DNNEmptyPane";
                        }
                        else
                        {
                            this.PaneControl.Attributes["class"] = "DNNEmptyPane";
                        }
                    }

                    // Add support for drag and drop
                    if (Globals.IsEditMode()) // this call also checks for permission
                    {
                        if (this.PaneControl.Attributes["class"] != null)
                        {
                            this.PaneControl.Attributes["class"] = this.PaneControl.Attributes["class"] + " dnnSortable";
                        }
                        else
                        {
                            this.PaneControl.Attributes["class"] = "dnnSortable";
                        }
                    }
                }
            }
        }

        private bool CanCollapsePane()
        {
            // This section sets the width to "0" on panes that have no modules.
            // This preserves the integrity of the HTML syntax so we don't have to set
            // the visiblity of a pane to false. Setting the visibility of a pane to
            // false where there are colspans and rowspans can render the skin incorrectly.
            bool canCollapsePane = true;
            if (this.Containers.Count > 0)
            {
                canCollapsePane = false;
            }
            else if (this.PaneControl.Controls.Count == 1)
            {
                // Pane contains 1 control
                canCollapsePane = false;
                var literal = this.PaneControl.Controls[0] as LiteralControl;
                if (literal != null)
                {
                    // Check  if the literal control is just whitespace - if so we can collapse panes
                    if (string.IsNullOrEmpty(HtmlUtils.StripWhiteSpace(literal.Text, false)))
                    {
                        canCollapsePane = true;
                    }
                }
            }
            else if (this.PaneControl.Controls.Count > 1)
            {
                // Pane contains more than 1 control
                canCollapsePane = false;
            }

            return canCollapsePane;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadContainerByPath gets the Container from its Url(Path).
        /// </summary>
        /// <param name="containerPath">The Url to the Container control.</param>
        /// <returns>A Container.</returns>
        /// -----------------------------------------------------------------------------
        private Containers.Container LoadContainerByPath(string containerPath)
        {
            if (containerPath.IndexOf("/skins/", StringComparison.InvariantCultureIgnoreCase) != -1 || containerPath.IndexOf("/skins\\", StringComparison.InvariantCultureIgnoreCase) != -1 || containerPath.IndexOf("\\skins\\", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                containerPath.IndexOf("\\skins/", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                throw new Exception();
            }

            Containers.Container container = null;

            try
            {
                string containerSrc = containerPath;
                if (containerPath.IndexOf(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    containerPath = containerPath.Remove(0, Globals.ApplicationPath.Length);
                }

                container = ControlUtilities.LoadControl<Containers.Container>(this.PaneControl.Page, containerPath);
                container.ContainerSrc = containerSrc;

                // call databind so that any server logic in the container is executed
                container.DataBind();
            }
            catch (Exception exc)
            {
                // could not load user control
                var lex = new ModuleLoadException(Skin.MODULELOAD_ERROR, exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    this._containerWrapperControl.Controls.Add(new ErrorContainer(this.PortalSettings, string.Format(Skin.CONTAINERLOAD_ERROR, containerPath), lex).Container);
                }

                Exceptions.LogException(lex);
            }

            return container;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadModuleContainer gets the Container for cookie.
        /// </summary>
        /// <param name="request">Current Http Request.</param>
        /// <returns>A Container.</returns>
        /// -----------------------------------------------------------------------------
        private Containers.Container LoadContainerFromCookie(HttpRequest request)
        {
            Containers.Container container = null;
            HttpCookie cookie = request.Cookies["_ContainerSrc" + this.PortalSettings.PortalId];
            if (cookie != null)
            {
                if (!string.IsNullOrEmpty(cookie.Value))
                {
                    container = this.LoadContainerByPath(SkinController.FormatSkinSrc(cookie.Value + ".ascx", this.PortalSettings));
                }
            }

            return container;
        }

        private Containers.Container LoadContainerFromPane()
        {
            Containers.Container container = null;
            string containerSrc;
            var validSrc = false;

            if ((this.PaneControl.Attributes["ContainerType"] != null) && (this.PaneControl.Attributes["ContainerName"] != null))
            {
                containerSrc = "[" + this.PaneControl.Attributes["ContainerType"] + "]" + SkinController.RootContainer + "/" + this.PaneControl.Attributes["ContainerName"] + "/" +
                               this.PaneControl.Attributes["ContainerSrc"];
                validSrc = true;
            }
            else
            {
                containerSrc = this.PaneControl.Attributes["ContainerSrc"];
                if (containerSrc.Contains("/") && !(containerSrc.StartsWith("[g]", StringComparison.InvariantCultureIgnoreCase) || containerSrc.StartsWith("[l]", StringComparison.InvariantCultureIgnoreCase)))
                {
                    containerSrc = string.Format(SkinController.IsGlobalSkin(this.PortalSettings.ActiveTab.SkinSrc) ? "[G]containers/{0}" : "[L]containers/{0}", containerSrc.TrimStart('/'));
                    validSrc = true;
                }
            }

            if (validSrc)
            {
                containerSrc = SkinController.FormatSkinSrc(containerSrc, this.PortalSettings);
                container = this.LoadContainerByPath(containerSrc);
            }

            return container;
        }

        private Containers.Container LoadContainerFromQueryString(ModuleInfo module, HttpRequest request)
        {
            Containers.Container container = null;
            int previewModuleId = -1;
            if (request.QueryString["ModuleId"] != null)
            {
                int.TryParse(request.QueryString["ModuleId"], out previewModuleId);
            }

            // load user container ( based on cookie )
            if ((request.QueryString["ContainerSrc"] != null) && (module.ModuleID == previewModuleId || previewModuleId == -1))
            {
                string containerSrc = SkinController.FormatSkinSrc(Globals.QueryStringDecode(request.QueryString["ContainerSrc"]) + ".ascx", this.PortalSettings);
                container = this.LoadContainerByPath(containerSrc);
            }

            return container;
        }

        private Containers.Container LoadNoContainer(ModuleInfo module)
        {
            string noContainerSrc = "[G]" + SkinController.RootContainer + "/_default/No Container.ascx";
            Containers.Container container = null;

            // if the module specifies that no container should be used
            if (module.DisplayTitle == false)
            {
                // always display container if the current user is the administrator or the module is being used in an admin case
                bool displayTitle = ModulePermissionController.CanEditModuleContent(module) || Globals.IsAdminSkin();

                // unless the administrator is in view mode
                if (displayTitle)
                {
                    displayTitle = this.PortalSettings.UserMode != PortalSettings.Mode.View;
                }

                if (displayTitle == false)
                {
                    container = this.LoadContainerByPath(SkinController.FormatSkinSrc(noContainerSrc, this.PortalSettings));
                }
            }

            return container;
        }

        private Containers.Container LoadModuleContainer(ModuleInfo module)
        {
            var containerSrc = Null.NullString;
            var request = this.PaneControl.Page.Request;
            Containers.Container container = null;

            if (this.PortalSettings.EnablePopUps && UrlUtils.InPopUp())
            {
                containerSrc = module.ContainerPath + "popUpContainer.ascx";

                // Check Skin for a popup Container
                if (module.ContainerSrc == this.PortalSettings.ActiveTab.ContainerSrc)
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(containerSrc)))
                    {
                        container = this.LoadContainerByPath(containerSrc);
                    }
                }

                // error loading container - load default popup container
                if (container == null)
                {
                    containerSrc = Globals.HostPath + "Containers/_default/popUpContainer.ascx";
                    container = this.LoadContainerByPath(containerSrc);
                }
            }
            else
            {
                container = (this.LoadContainerFromQueryString(module, request) ?? this.LoadContainerFromCookie(request)) ?? this.LoadNoContainer(module);
                if (container == null)
                {
                    // Check Skin for Container
                    var masterModules = this.PortalSettings.ActiveTab.ChildModules;
                    if (masterModules.ContainsKey(module.ModuleID) && string.IsNullOrEmpty(masterModules[module.ModuleID].ContainerSrc))
                    {
                        // look for a container specification in the skin pane
                        if (this.PaneControl != null)
                        {
                            if (this.PaneControl.Attributes["ContainerSrc"] != null)
                            {
                                container = this.LoadContainerFromPane();
                            }
                        }
                    }
                }

                // else load assigned container
                if (container == null)
                {
                    containerSrc = module.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, this.PortalSettings);
                        container = this.LoadContainerByPath(containerSrc);
                    }
                }

                // error loading container - load from tab
                if (container == null)
                {
                    containerSrc = this.PortalSettings.ActiveTab.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, this.PortalSettings);
                        container = this.LoadContainerByPath(containerSrc);
                    }
                }

                // error loading container - load default
                if (container == null)
                {
                    containerSrc = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalContainer(), this.PortalSettings);
                    container = this.LoadContainerByPath(containerSrc);
                }
            }

            // Set container path
            module.ContainerPath = SkinController.FormatSkinPath(containerSrc);

            // set container id to an explicit short name to reduce page payload
            container.ID = "ctr";

            // make the container id unique for the page
            if (module.ModuleID > -1)
            {
                container.ID += module.ModuleID.ToString();
            }

            return container;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ModuleMoveToPanePostBack excutes when a module is moved by Drag-and-Drop.
        /// </summary>
        /// <param name="args">A ClientAPIPostBackEventArgs object.</param>
        /// -----------------------------------------------------------------------------
        private void ModuleMoveToPanePostBack(ClientAPIPostBackEventArgs args)
        {
            var portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            if (TabPermissionController.CanAdminPage())
            {
                var moduleId = Convert.ToInt32(args.EventArguments["moduleid"]);
                var paneName = Convert.ToString(args.EventArguments["pane"]);
                var moduleOrder = Convert.ToInt32(args.EventArguments["order"]);

                ModuleController.Instance.UpdateModuleOrder(portalSettings.ActiveTab.TabID, moduleId, moduleOrder, paneName);
                ModuleController.Instance.UpdateTabModuleOrder(portalSettings.ActiveTab.TabID);

                // Redirect to the same page to pick up changes
                this.PaneControl.Page.Response.Redirect(this.PaneControl.Page.Request.RawUrl, true);
            }
        }

        private bool IsVesionableModule(ModuleInfo moduleInfo)
        {
            if (string.IsNullOrEmpty(moduleInfo.DesktopModule.BusinessControllerClass))
            {
                return false;
            }

            object controller = Framework.Reflection.CreateObject(moduleInfo.DesktopModule.BusinessControllerClass, string.Empty);
            return controller is IVersionable;
        }
    }
}
