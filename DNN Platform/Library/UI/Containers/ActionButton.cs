// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;

    using DotNetNuke.Entities.Modules.Actions;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class    : ActionButton
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the Command Name.
        /// </summary>
        /// <remarks>
        ///   Maps to ModuleActionType in DotNetNuke.Entities.Modules.Actions.
        /// </remarks>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string CommandName
        {
            get
            {
                this.EnsureChildControls();
                return this._ButtonList.CommandName;
            }

            set
            {
                this.EnsureChildControls();
                this._ButtonList.CommandName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the CSS Class.
        /// </summary>
        /// <remarks>
        ///   Defaults to 'CommandButton'.
        /// </remarks>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string CssClass
        {
            get
            {
                this.EnsureChildControls();
                return this._ButtonList.CssClass;
            }

            set
            {
                this.EnsureChildControls();
                this._ButtonList.CssClass = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets a value indicating whether gets or sets whether the link is displayed.
        /// </summary>
        /// <remarks>
        ///   Defaults to True.
        /// </remarks>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayLink
        {
            get
            {
                this.EnsureChildControls();
                return this._ButtonList.DisplayLink;
            }

            set
            {
                this.EnsureChildControls();
                this._ButtonList.DisplayLink = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets a value indicating whether gets or sets whether the icon is displayed.
        /// </summary>
        /// <remarks>
        ///   Defaults to False.
        /// </remarks>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayIcon
        {
            get
            {
                this.EnsureChildControls();
                return this._ButtonList.DisplayIcon;
            }

            set
            {
                this.EnsureChildControls();
                this._ButtonList.DisplayIcon = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the Icon used.
        /// </summary>
        /// <remarks>
        ///   Defaults to the icon defined in Action.
        /// </remarks>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string IconFile
        {
            get
            {
                this.EnsureChildControls();
                return this._ButtonList.ImageURL;
            }

            set
            {
                this.EnsureChildControls();
                this._ButtonList.ImageURL = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the Separator between Buttons.
        /// </summary>
        /// <remarks>
        ///   Defaults to 2 non-breaking spaces.
        /// </remarks>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ButtonSeparator
        {
            get
            {
                this.EnsureChildControls();
                return this._ButtonList.ButtonSeparator;
            }

            set
            {
                this.EnsureChildControls();
                this._ButtonList.ButtonSeparator = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateChildControls builds the control tree.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this._ButtonList = new ActionButtonList();
            this._ButtonList.Action += this.Action_Click;

            this.Controls.Add(this._ButtonList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Action_Click responds to an Action Event in the contained actionButtonList.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Action_Click(object sender, ActionEventArgs e)
        {
            this.ProcessAction(e.Action.ID.ToString());
        }
    }
}
