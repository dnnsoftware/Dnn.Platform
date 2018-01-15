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
