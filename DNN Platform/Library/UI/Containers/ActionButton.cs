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

using DotNetNuke.Entities.Modules.Actions;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : ActionButton
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   ActionButton provides a button (or group of buttons) for action(s).
    /// </summary>
    /// <remarks>
    ///   ActionBase inherits from UserControl, and implements the IActionControl Interface.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("This class has been deprecated in favour of the new ActionCommandButton and ActionButtonList.")]
    public class ActionButton : ActionBase
    {
        private ActionButtonList _ButtonList;

        #region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the Command Name
        /// </summary>
        /// <remarks>
        ///   Maps to ModuleActionType in DotNetNuke.Entities.Modules.Actions
        /// </remarks>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string CommandName
        {
            get
            {
                EnsureChildControls();
                return _ButtonList.CommandName;
            }
            set
            {
                EnsureChildControls();
                _ButtonList.CommandName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the CSS Class
        /// </summary>
        /// <remarks>
        ///   Defaults to 'CommandButton'
        /// </remarks>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string CssClass
        {
            get
            {
                EnsureChildControls();
                return _ButtonList.CssClass;
            }
            set
            {
                EnsureChildControls();
                _ButtonList.CssClass = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets whether the link is displayed
        /// </summary>
        /// <remarks>
        ///   Defaults to True
        /// </remarks>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayLink
        {
            get
            {
                EnsureChildControls();
                return _ButtonList.DisplayLink;
            }
            set
            {
                EnsureChildControls();
                _ButtonList.DisplayLink = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets whether the icon is displayed
        /// </summary>
        /// <remarks>
        ///   Defaults to False
        /// </remarks>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayIcon
        {
            get
            {
                EnsureChildControls();
                return _ButtonList.DisplayIcon;
            }
            set
            {
                EnsureChildControls();
                _ButtonList.DisplayIcon = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the Icon used
        /// </summary>
        /// <remarks>
        ///   Defaults to the icon defined in Action
        /// </remarks>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconFile
        {
            get
            {
                EnsureChildControls();
                return _ButtonList.ImageURL;
            }
            set
            {
                EnsureChildControls();
                _ButtonList.ImageURL = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the Separator between Buttons
        /// </summary>
        /// <remarks>
        ///   Defaults to 2 non-breaking spaces
        /// </remarks>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ButtonSeparator
        {
            get
            {
                EnsureChildControls();
                return _ButtonList.ButtonSeparator;
            }
            set
            {
                EnsureChildControls();
                _ButtonList.ButtonSeparator = value;
            }
        }

        #endregion

        #region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Action_Click responds to an Action Event in the contained actionButtonList
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Action_Click(object sender, ActionEventArgs e)
        {
            ProcessAction(e.Action.ID.ToString());
        }

        #endregion

        #region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateChildControls builds the control tree
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _ButtonList = new ActionButtonList();
            _ButtonList.Action += Action_Click;

            Controls.Add(_ButtonList);
        }

        #endregion
    }
}
