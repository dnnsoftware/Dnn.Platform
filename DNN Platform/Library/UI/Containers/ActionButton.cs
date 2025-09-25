// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>  ActionButton provides a button (or group of buttons) for action(s).</summary>
    /// <remarks>
    ///   ActionBase inherits from UserControl, and implements the IActionControl Interface.
    /// </remarks>
    [DnnDeprecated(7, 0, 0, "This class has been deprecated in favour of the new ActionCommandButton and ActionButtonList", RemovalVersion = 11)]
    public partial class ActionButton : ActionBase
    {
        private ActionButtonList buttonList;

        /// <summary>  Gets or sets the Command Name.</summary>
        /// <remarks>
        ///   Maps to ModuleActionType in DotNetNuke.Entities.Modules.Actions.
        /// </remarks>
        /// <value>A String.</value>
        public string CommandName
        {
            get
            {
                this.EnsureChildControls();
                return this.buttonList.CommandName;
            }

            set
            {
                this.EnsureChildControls();
                this.buttonList.CommandName = value;
            }
        }

        /// <summary>  Gets or sets the CSS Class.</summary>
        /// <remarks>
        ///   Defaults to 'CommandButton'.
        /// </remarks>
        /// <value>A String.</value>
        public string CssClass
        {
            get
            {
                this.EnsureChildControls();
                return this.buttonList.CssClass;
            }

            set
            {
                this.EnsureChildControls();
                this.buttonList.CssClass = value;
            }
        }

        /// <summary>  Gets or sets a value indicating whether the link is displayed.</summary>
        /// <remarks>
        ///   Defaults to True.
        /// </remarks>
        /// <value>A Boolean.</value>
        public bool DisplayLink
        {
            get
            {
                this.EnsureChildControls();
                return this.buttonList.DisplayLink;
            }

            set
            {
                this.EnsureChildControls();
                this.buttonList.DisplayLink = value;
            }
        }

        /// <summary>  Gets or sets a value indicating whether the icon is displayed.</summary>
        /// <remarks>
        ///   Defaults to False.
        /// </remarks>
        /// <value>A Boolean.</value>
        public bool DisplayIcon
        {
            get
            {
                this.EnsureChildControls();
                return this.buttonList.DisplayIcon;
            }

            set
            {
                this.EnsureChildControls();
                this.buttonList.DisplayIcon = value;
            }
        }

        /// <summary>  Gets or sets the Icon used.</summary>
        /// <remarks>
        ///   Defaults to the icon defined in Action.
        /// </remarks>
        /// <value>A String.</value>
        public string IconFile
        {
            get
            {
                this.EnsureChildControls();
                return this.buttonList.ImageURL;
            }

            set
            {
                this.EnsureChildControls();
                this.buttonList.ImageURL = value;
            }
        }

        /// <summary>  Gets or sets the Separator between Buttons.</summary>
        /// <remarks>
        ///   Defaults to 2 non-breaking spaces.
        /// </remarks>
        /// <value>A String.</value>
        public string ButtonSeparator
        {
            get
            {
                this.EnsureChildControls();
                return this.buttonList.ButtonSeparator;
            }

            set
            {
                this.EnsureChildControls();
                this.buttonList.ButtonSeparator = value;
            }
        }

        /// <summary>  CreateChildControls builds the control tree.</summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.buttonList = new ActionButtonList();
            this.buttonList.Action += this.Action_Click;

            this.Controls.Add(this.buttonList);
        }

        /// <summary>  Action_Click responds to an Action Event in the contained actionButtonList.</summary>
        private void Action_Click(object sender, ActionEventArgs e)
        {
            this.ProcessAction(e.Action.ID.ToString());
        }
    }
}
