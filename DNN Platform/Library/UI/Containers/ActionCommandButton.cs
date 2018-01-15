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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : ActionCommandButton
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
		#region "Private Members"

        private ActionManager _ActionManager;
        private ModuleAction _ModuleAction;

		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ModuleAction for this Action control
        /// </summary>
        /// <returns>A ModuleAction object</returns>
        /// -----------------------------------------------------------------------------
        public ModuleAction ModuleAction
        {
            get
            {
                if (_ModuleAction == null)
                {
                    _ModuleAction = ModuleControl.ModuleContext.Actions.GetActionByCommandName(CommandName);
                }
                return _ModuleAction;
            }
            set
            {
                _ModuleAction = value;
            }
        }

        #region IActionControl Members

        public event ActionEventHandler Action;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ActionManager instance for this Action control
        /// </summary>
        /// <returns>An ActionManager object</returns>
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
        /// -----------------------------------------------------------------------------
        public IModuleControl ModuleControl { get; set; }

        #endregion
		
		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateChildControls builds the control tree
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
			//Call base class method to ensure Control Tree is built
            base.CreateChildControls();

            //Set Causes Validation and Enables ViewState to false
            CausesValidation = false;
            EnableViewState = false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAction raises the Action Event
        /// </summary>
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
        /// OnButtonClick runs when the underlying CommandButton is clicked
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnButtonClick(EventArgs e)
        {
            base.OnButtonClick(e);
            if (!ActionManager.ProcessAction(ModuleAction))
            {
                OnAction(new ActionEventArgs(ModuleAction, ModuleControl.ModuleContext.Configuration));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs when just before the Render phase of the Page Lifecycle
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (ModuleAction != null && ActionManager.IsVisible(ModuleAction))
            {
                Text = ModuleAction.Title;
                CommandArgument = ModuleAction.ID.ToString();

                if (DisplayIcon && (!string.IsNullOrEmpty(ModuleAction.Icon) || !string.IsNullOrEmpty(ImageUrl)))
                {
                    if (!string.IsNullOrEmpty(ImageUrl))
                    {
                        ImageUrl = ModuleControl.ModuleContext.Configuration.ContainerPath.Substring(0, ModuleControl.ModuleContext.Configuration.ContainerPath.LastIndexOf("/") + 1) + ImageUrl;
                    }
                    else
                    {
                        if (ModuleAction.Icon.IndexOf("/") > Null.NullInteger)
                        {
                            ImageUrl = ModuleAction.Icon;
                        }
                        else
                        {
                            ImageUrl = "~/images/" + ModuleAction.Icon;
                        }
                    }
                }
                ActionManager.GetClientScriptURL(ModuleAction, this);
            }
            else
            {
                Visible = false;
            }
        }
		
		#endregion
    }
}
