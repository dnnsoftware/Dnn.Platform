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
    /// <history>
    /// 	[cnurse]	10/07/2004	Documented
    ///     [cnurse]    12/15/2007  Refactored 
    /// </history>
    /// -----------------------------------------------------------------------------
    public abstract class ActionBase : UserControl, IActionControl
    {
		#region "Private Members"
		
        private ActionManager _ActionManager;
        private ModuleAction _ActionRoot;

        [Obsolete("Obsoleted in DotNetNuke 5.1.2. The concept of an adminControl no longer exists.")]
        protected bool m_adminControl;

        [Obsolete("Obsoleted in DotNetNuke 5.1.2. The concept of an adminModule no longer exists.")]
        protected bool m_adminModule;

        [Obsolete("Obsoleted in DotNetNuke 5.1.2 Replaced by ActionRoot Property")]
        protected ModuleAction m_menuActionRoot;

        [Obsolete("Obsoleted in DotNetNuke 5.1.2. Replaced by Actions Property")]
        protected ModuleActionCollection m_menuActions;

        protected bool m_supportsIcons = true;

        [Obsolete("Obsoleted in DotNetNuke 5.1.2. No longer neccessary as there is no concept of an Admin Page")]
        protected bool m_tabPreview;
		
		#endregion

		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Actions Collection
        /// </summary>
        /// <returns>A ModuleActionCollection</returns>
        /// <history>
        /// 	[cnurse]	12/15/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/15/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/15/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/15/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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

        [Obsolete("Obsoleted in DotNetNuke 5.0. Use ModuleContext.Configuration")]
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return ModuleContext.Configuration;
            }
        }

        [Obsolete("Obsoleted in DotNetNuke 5.0. Replaced by ModuleControl")]
        public PortalModuleBase PortalModule
        {
            get
            {
                return new PortalModuleBase();
            }
            set
            {
                ModuleControl = value;
            }
        }

        [Obsolete("Obsoleted in DotNetNuke 5.1.2. Replaced by Actions Property")]
        public ModuleActionCollection MenuActions
        {
            get
            {
                return Actions;
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
        /// <history>
        /// 	[cnurse]	12/15/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/15/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public IModuleControl ModuleControl { get; set; }

        #endregion
		
		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DisplayControl determines whether the control should be displayed
        /// </summary>
        /// <history>
        /// 	[cnurse]	12/23/2007  documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool DisplayControl(DNNNodeCollection objNodes)
        {
            return ActionManager.DisplayControl(objNodes);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAction raises the Action Event for this control
        /// </summary>
        /// <history>
        /// 	[cnurse]	12/23/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	12/23/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
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
        /// <history>
        /// 	[cnurse]	05/12/2005	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
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
