// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class    : ActionsMenu
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionsMenu provides a menu for a collection of actions.
    /// </summary>
    /// <remarks>
    /// ActionsMenu inherits from CompositeControl, and implements the IActionControl
    /// Interface. It uses the Navigation Providers to implement the Menu.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ActionsMenu : Control, IActionControl
    {
        private ActionManager _ActionManager;
        private ModuleAction _ActionRoot;
        private int _ExpandDepth = -1;
        private NavigationProvider _ProviderControl;
        private string _ProviderName = "DNNMenuNavigationProvider";

        public event ActionEventHandler Action;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionManager instance for this Action control.
        /// </summary>
        /// <returns>An ActionManager object.</returns>
        /// -----------------------------------------------------------------------------
        public ActionManager ActionManager
        {
            get
            {
                if (this._ActionManager == null)
                {
                    this._ActionManager = new ActionManager(this);
                }

                return this._ActionManager;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Expansion Depth for the Control.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int ExpandDepth
        {
            get
            {
                if (this.PopulateNodesFromClient == false || this.ProviderControl.SupportsPopulateOnDemand == false)
                {
                    return -1;
                }

                return this._ExpandDepth;
            }

            set
            {
                this._ExpandDepth = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Path to the Script Library for the provider.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string PathSystemScript { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and Sets whether the Menu should be populated from the client.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public bool PopulateNodesFromClient { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Name of the provider to use.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ProviderName
        {
            get
            {
                return this._ProviderName;
            }

            set
            {
                this._ProviderName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ModuleControl instance for this Action control.
        /// </summary>
        /// <returns>An IModuleControl object.</returns>
        /// -----------------------------------------------------------------------------
        public IModuleControl ModuleControl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionRoot.
        /// </summary>
        /// <returns>A ModuleActionCollection.</returns>
        /// -----------------------------------------------------------------------------
        protected ModuleAction ActionRoot
        {
            get
            {
                if (this._ActionRoot == null)
                {
                    this._ActionRoot = new ModuleAction(this.ModuleControl.ModuleContext.GetNextActionID(), " ", string.Empty, string.Empty, "action.gif");
                }

                return this._ActionRoot;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Provider Control.
        /// </summary>
        /// <returns>A NavigationProvider.</returns>
        /// -----------------------------------------------------------------------------
        protected NavigationProvider ProviderControl
        {
            get
            {
                return this._ProviderControl;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// BindMenu binds the Navigation Provider to the Node Collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected void BindMenu()
        {
            this.BindMenu(Navigation.GetActionNodes(this.ActionRoot, this, this.ExpandDepth));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAction raises the Action Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnAction(ActionEventArgs e)
        {
            if (this.Action != null)
            {
                this.Action(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnInit runs during the controls initialisation phase.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            this._ProviderControl = NavigationProvider.Instance(this.ProviderName);
            this.ProviderControl.PopulateOnDemand += this.ProviderControl_PopulateOnDemand;
            base.OnInit(e);
            this.ProviderControl.ControlID = "ctl" + this.ID;
            this.ProviderControl.Initialize();
            this.Controls.Add(this.ProviderControl.NavigationControl);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs during the controls load phase.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Add the Actions to the Action Root
            this.ActionRoot.Actions.AddRange(this.ModuleControl.ModuleContext.Actions);

            // Set Menu Defaults
            this.SetMenuDefaults();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs during the controls pre-render phase.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.BindMenu();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// BindMenu binds the Navigation Provider to the Node Collection.
        /// </summary>
        /// <param name="objNodes">The Nodes collection to bind.</param>
        /// -----------------------------------------------------------------------------
        private void BindMenu(DNNNodeCollection objNodes)
        {
            this.Visible = this.ActionManager.DisplayControl(objNodes);
            if (this.Visible)
            {
                // since we always bind we need to clear the nodes for providers that maintain their state
                this.ProviderControl.ClearNodes();
                foreach (DNNNode objNode in objNodes)
                {
                    this.ProcessNodes(objNode);
                }

                this.ProviderControl.Bind(objNodes);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessNodes proceses a single node and its children.
        /// </summary>
        /// <param name="objParent">The Node to process.</param>
        /// -----------------------------------------------------------------------------
        private void ProcessNodes(DNNNode objParent)
        {
            if (!string.IsNullOrEmpty(objParent.JSFunction))
            {
                objParent.JSFunction = string.Format("if({0}){{{1}}};", objParent.JSFunction, this.Page.ClientScript.GetPostBackEventReference(this.ProviderControl.NavigationControl, objParent.ID));
            }

            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                this.ProcessNodes(objNode);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SetMenuDefaults sets up the default values.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void SetMenuDefaults()
        {
            try
            {
                //--- original page set attributes ---
                this.ProviderControl.StyleIconWidth = 15;
                this.ProviderControl.MouseOutHideDelay = 500;
                this.ProviderControl.MouseOverAction = NavigationProvider.HoverAction.Expand;
                this.ProviderControl.MouseOverDisplay = NavigationProvider.HoverDisplay.None;

                // style sheet settings
                this.ProviderControl.CSSControl = "ModuleTitle_MenuBar";
                this.ProviderControl.CSSContainerRoot = "ModuleTitle_MenuContainer";
                this.ProviderControl.CSSNode = "ModuleTitle_MenuItem";
                this.ProviderControl.CSSIcon = "ModuleTitle_MenuIcon";
                this.ProviderControl.CSSContainerSub = "ModuleTitle_SubMenu";
                this.ProviderControl.CSSBreak = "ModuleTitle_MenuBreak";
                this.ProviderControl.CSSNodeHover = "ModuleTitle_MenuItemSel";
                this.ProviderControl.CSSIndicateChildSub = "ModuleTitle_MenuArrow";
                this.ProviderControl.CSSIndicateChildRoot = "ModuleTitle_RootMenuArrow";

                this.ProviderControl.PathImage = Globals.ApplicationPath + "/Images/";
                this.ProviderControl.PathSystemImage = Globals.ApplicationPath + "/Images/";
                this.ProviderControl.IndicateChildImageSub = "action_right.gif";
                this.ProviderControl.IndicateChildren = true;
                this.ProviderControl.StyleRoot = "background-color: Transparent; font-size: 1pt;";
                this.ProviderControl.NodeClick += this.MenuItem_Click;
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MenuItem_Click handles the Menu Click event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void MenuItem_Click(NavigationEventArgs args)
        {
            if (Globals.NumberMatchRegex.IsMatch(args.ID))
            {
                ModuleAction action = this.ModuleControl.ModuleContext.Actions.GetActionByID(Convert.ToInt32(args.ID));
                if (!this.ActionManager.ProcessAction(action))
                {
                    this.OnAction(new ActionEventArgs(action, this.ModuleControl.ModuleContext.Configuration));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProviderControl_PopulateOnDemand handles the Populate On Demand Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void ProviderControl_PopulateOnDemand(NavigationEventArgs args)
        {
            this.SetMenuDefaults();
            this.ActionRoot.Actions.AddRange(this.ModuleControl.ModuleContext.Actions); // Modules how add custom actions in control lifecycle will not have those actions populated...

            ModuleAction objAction = this.ActionRoot;
            if (this.ActionRoot.ID != Convert.ToInt32(args.ID))
            {
                objAction = this.ModuleControl.ModuleContext.Actions.GetActionByID(Convert.ToInt32(args.ID));
            }

            if (args.Node == null)
            {
                args.Node = Navigation.GetActionNode(args.ID, this.ProviderControl.ID, objAction, this);
            }

            this.ProviderControl.ClearNodes(); // since we always bind we need to clear the nodes for providers that maintain their state
            this.BindMenu(Navigation.GetActionNodes(objAction, args.Node, this, this.ExpandDepth));
        }
    }
}
