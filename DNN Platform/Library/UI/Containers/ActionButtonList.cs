// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// <summary>
    /// ActionButtonList provides a list of buttons for a group of actions of the same type.
    /// </summary>
    /// <remarks>
    /// ActionButtonList inherits from CompositeControl, and implements the IActionControl
    /// Interface.  It uses a single ActionCommandButton for each Action.
    /// </remarks>
    public class ActionButtonList : CompositeControl, IActionControl
    {
		#region "Private Members"
		
        private ActionManager _ActionManager;
        private ModuleActionCollection _ModuleActions;
        private string _buttonSeparator = "&nbsp;&nbsp;";
        private string _commandName = "";
        private bool _displayLink = true;


		#endregion

		#region "Protected Members"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModuleActionCollection to bind to the list
        /// </summary>
        /// <value>A ModuleActionCollection</value>
        protected ModuleActionCollection ModuleActions
        {
            get
            {
                if (_ModuleActions == null)
                {
                    _ModuleActions = ModuleControl.ModuleContext.Actions.GetActionsByCommandName(CommandName);
                }
                return _ModuleActions;
            }
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Separator between Buttons
        /// </summary>
        /// <remarks>Defaults to 2 non-breaking spaces</remarks>
        /// <value>A String</value>
        public string ButtonSeparator
        {
            get
            {
                return _buttonSeparator;
            }
            set
            {
                _buttonSeparator = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Command Name
        /// </summary>
        /// <remarks>Maps to ModuleActionType in DotNetNuke.Entities.Modules.Actions</remarks>
        /// <value>A String</value>
        public string CommandName
        {
            get
            {
                return _commandName;
            }
            set
            {
                _commandName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the icon is displayed
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// <value>A Boolean</value>
        public bool DisplayIcon { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the link is displayed
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        public bool DisplayLink
        {
            get
            {
                return _displayLink;
            }
            set
            {
                _displayLink = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Icon used
        /// </summary>
        /// <remarks>Defaults to the icon defined in Action</remarks>
        /// <value>A String</value>
        public string ImageURL { get; set; }

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
		
		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAction raises the Action Event
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
        /// OnLoad runs when the control is loaded into the Control Tree
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.ModuleControl == null)
            {
                return;
            }

            foreach (ModuleAction action in ModuleActions)
            {
                if (action != null && ActionManager.IsVisible(action))
                {
					//Create a new ActionCommandButton
                    var actionButton = new ActionCommandButton();

                    //Set all the properties
                    actionButton.ModuleAction = action;
                    actionButton.ModuleControl = ModuleControl;
                    actionButton.CommandName = CommandName;
                    actionButton.CssClass = CssClass;
                    actionButton.DisplayLink = DisplayLink;
                    actionButton.DisplayIcon = DisplayIcon;
                    actionButton.ImageUrl = ImageURL;

                    //Add a handler for the Action Event
                    actionButton.Action += ActionButtonClick;

                    Controls.Add(actionButton);

                    Controls.Add(new LiteralControl(ButtonSeparator));
                }
            }
            Visible = (Controls.Count > 0);
        }
		
		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ActionButtonClick handles the Action event of the contained ActionCommandButton(s)
        /// </summary>
        private void ActionButtonClick(object sender, ActionEventArgs e)
        {
            OnAction(e);
        }
		
		#endregion
    }
}
