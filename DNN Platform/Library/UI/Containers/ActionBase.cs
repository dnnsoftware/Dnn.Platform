// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : ActionBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ActionBase is an abstract base control for Action objects that inherit from UserControl.
    /// </summary>
    /// <remarks>
    /// ActionBase inherits from UserControl, and implements the IActionControl Interface
    /// </remarks>
    public abstract class ActionBase : UserControl, IActionControl
    {
		#region "Private Members"
		
        private ActionManager _ActionManager;
        private ModuleAction _ActionRoot;
        protected bool m_supportsIcons = true;
		
		#endregion

		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Actions Collection
        /// </summary>
        /// <returns>A ModuleActionCollection</returns>
        protected ModuleActionCollection Actions
        {
            get
            {
                return ModuleContext.Actions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionRoot
        /// </summary>
        /// <returns>A ModuleActionCollection</returns>
        protected ModuleAction ActionRoot
        {
            get
            {
                if (_ActionRoot == null)
                {
                    _ActionRoot = new ModuleAction(ModuleContext.GetNextActionID(), Localization.GetString("Manage.Text", Localization.GlobalResourceFile), string.Empty, string.Empty, "manage-icn.png");
                }
                return _ActionRoot;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleContext
        /// </summary>
        /// <returns>A ModuleInstanceContext</returns>
        protected ModuleInstanceContext ModuleContext
        {
            get
            {
                return ModuleControl.ModuleContext;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PortalSettings
        /// </summary>
        /// <returns>A PortalSettings object</returns>
        protected PortalSettings PortalSettings
        {
            get
            {
                return ModuleControl.ModuleContext.PortalSettings;
            }
        }
		
		#endregion

		#region Public Properties

        public bool EditMode
        {
            get
            {
                return ModuleContext.PortalSettings.UserMode != PortalSettings.Mode.View;
            }
        }

        public bool SupportsIcons
        {
            get
            {
                return m_supportsIcons;
            }
        }
		
		#endregion

        #region IActionControl Members

        public event ActionEventHandler Action;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionManager instance for this Action control
        /// </summary>
        /// <returns>An ActionManager object</returns>
        public ActionManager ActionManager
        {
            get
            {
                if (_ActionManager == null)
                {
                    _ActionManager = new ActionManager(this);
                }
                return _ActionManager;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ModuleControl instance for this Action control
        /// </summary>
        /// <returns>An IModuleControl object</returns>
        public IModuleControl ModuleControl { get; set; }

        #endregion
		
		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DisplayControl determines whether the control should be displayed
        /// </summary>
        protected bool DisplayControl(DNNNodeCollection objNodes)
        {
            return ActionManager.DisplayControl(objNodes);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAction raises the Action Event for this control
        /// </summary>
        protected virtual void OnAction(ActionEventArgs e)
        {
            if (Action != null)
            {
                Action(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProcessAction processes the action event
        /// </summary>
        protected void ProcessAction(string ActionID)
        {
            int output;
            if (Int32.TryParse(ActionID, out output))
            {
                ModuleAction action = Actions.GetActionByID(output);
                if (action != null)
                {
                    if (!ActionManager.ProcessAction(action))
                    {
                        OnAction(new ActionEventArgs(action, ModuleContext.Configuration));
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the class is loaded
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

                ActionRoot.Actions.AddRange(Actions);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

			base.OnLoad(e);
        }
		
		
		#endregion
    }
}
