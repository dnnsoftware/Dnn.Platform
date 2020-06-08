// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
    [Obsolete("This class has been deprecated in favour of the new ActionCommandButton and ActionButtonList.. Scheduled removal in v11.0.0.")]
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
