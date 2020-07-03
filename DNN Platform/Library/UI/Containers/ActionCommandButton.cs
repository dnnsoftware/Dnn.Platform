// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class    : ActionCommandButton
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionCommandButton provides a button for a single action.
    /// </summary>
    /// <remarks>
    /// ActionBase inherits from CommandButton, and implements the IActionControl Interface.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ActionCommandButton : CommandButton, IActionControl
    {
        private ActionManager _ActionManager;
        private ModuleAction _ModuleAction;

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
        /// Gets or sets and sets the ModuleAction for this Action control.
        /// </summary>
        /// <returns>A ModuleAction object.</returns>
        /// -----------------------------------------------------------------------------
        public ModuleAction ModuleAction
        {
            get
            {
                if (this._ModuleAction == null)
                {
                    this._ModuleAction = this.ModuleControl.ModuleContext.Actions.GetActionByCommandName(this.CommandName);
                }

                return this._ModuleAction;
            }

            set
            {
                this._ModuleAction = value;
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
        /// CreateChildControls builds the control tree.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            // Call base class method to ensure Control Tree is built
            base.CreateChildControls();

            // Set Causes Validation and Enables ViewState to false
            this.CausesValidation = false;
            this.EnableViewState = false;
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
        /// OnButtonClick runs when the underlying CommandButton is clicked.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnButtonClick(EventArgs e)
        {
            base.OnButtonClick(e);
            if (!this.ActionManager.ProcessAction(this.ModuleAction))
            {
                this.OnAction(new ActionEventArgs(this.ModuleAction, this.ModuleControl.ModuleContext.Configuration));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs when just before the Render phase of the Page Lifecycle.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.ModuleAction != null && this.ActionManager.IsVisible(this.ModuleAction))
            {
                this.Text = this.ModuleAction.Title;
                this.CommandArgument = this.ModuleAction.ID.ToString();

                if (this.DisplayIcon && (!string.IsNullOrEmpty(this.ModuleAction.Icon) || !string.IsNullOrEmpty(this.ImageUrl)))
                {
                    if (!string.IsNullOrEmpty(this.ImageUrl))
                    {
                        this.ImageUrl = this.ModuleControl.ModuleContext.Configuration.ContainerPath.Substring(0, this.ModuleControl.ModuleContext.Configuration.ContainerPath.LastIndexOf("/") + 1) + this.ImageUrl;
                    }
                    else
                    {
                        if (this.ModuleAction.Icon.IndexOf("/") > Null.NullInteger)
                        {
                            this.ImageUrl = this.ModuleAction.Icon;
                        }
                        else
                        {
                            this.ImageUrl = "~/images/" + this.ModuleAction.Icon;
                        }
                    }
                }

                this.ActionManager.GetClientScriptURL(this.ModuleAction, this);
            }
            else
            {
                this.Visible = false;
            }
        }
    }
}
