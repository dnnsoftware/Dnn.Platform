// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class    : ActionBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionBase is an abstract base control for Action objects that inherit from UserControl.
    /// </summary>
    /// <remarks>
    /// ActionBase inherits from UserControl, and implements the IActionControl Interface.
    /// </remarks>
    public abstract class ActionBase : UserControl, IActionControl
    {
        protected bool m_supportsIcons = true;
        private ActionManager _ActionManager;
        private ModuleAction _ActionRoot;

        public event ActionEventHandler Action;

        public bool EditMode
        {
            get
            {
                return this.ModuleContext.PortalSettings.UserMode != PortalSettings.Mode.View;
            }
        }

        public bool SupportsIcons
        {
            get
            {
                return this.m_supportsIcons;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionManager instance for this Action control.
        /// </summary>
        /// <returns>An ActionManager object.</returns>
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
        /// Gets or sets and sets the ModuleControl instance for this Action control.
        /// </summary>
        /// <returns>An IModuleControl object.</returns>
        public IModuleControl ModuleControl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Actions Collection.
        /// </summary>
        /// <returns>A ModuleActionCollection.</returns>
        protected ModuleActionCollection Actions
        {
            get
            {
                return this.ModuleContext.Actions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionRoot.
        /// </summary>
        /// <returns>A ModuleActionCollection.</returns>
        protected ModuleAction ActionRoot
        {
            get
            {
                if (this._ActionRoot == null)
                {
                    this._ActionRoot = new ModuleAction(this.ModuleContext.GetNextActionID(), Localization.GetString("Manage.Text", Localization.GlobalResourceFile), string.Empty, string.Empty, "manage-icn.png");
                }

                return this._ActionRoot;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleContext.
        /// </summary>
        /// <returns>A ModuleInstanceContext.</returns>
        protected ModuleInstanceContext ModuleContext
        {
            get
            {
                return this.ModuleControl.ModuleContext;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PortalSettings.
        /// </summary>
        /// <returns>A PortalSettings object.</returns>
        protected PortalSettings PortalSettings
        {
            get
            {
                return this.ModuleControl.ModuleContext.PortalSettings;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DisplayControl determines whether the control should be displayed.
        /// </summary>
        /// <returns></returns>
        protected bool DisplayControl(DNNNodeCollection objNodes)
        {
            return this.ActionManager.DisplayControl(objNodes);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAction raises the Action Event for this control.
        /// </summary>
        protected virtual void OnAction(ActionEventArgs e)
        {
            if (this.Action != null)
            {
                this.Action(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessAction processes the action event.
        /// </summary>
        protected void ProcessAction(string ActionID)
        {
            int output;
            if (int.TryParse(ActionID, out output))
            {
                ModuleAction action = this.Actions.GetActionByID(output);
                if (action != null)
                {
                    if (!this.ActionManager.ProcessAction(action))
                    {
                        this.OnAction(new ActionEventArgs(action, this.ModuleContext.Configuration));
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the class is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (this.ModuleControl == null)
                {
                    return;
                }

                this.ActionRoot.Actions.AddRange(this.Actions);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            base.OnLoad(e);
        }
    }
}
