// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ActionCommandButton provides a button for a single action.</summary>
    /// <remarks>Inherits from <see cref="CommandButton"/>, and implements the <see cref="IActionControl"/> Interface.</remarks>
    public class ActionCommandButton : CommandButton, IActionControl
    {
        private readonly IEventLogger eventLogger;
        private ActionManager actionManager;
        private ModuleAction moduleAction;

        /// <summary>Initializes a new instance of the <see cref="ActionCommandButton"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ActionCommandButton()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ActionCommandButton"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        public ActionCommandButton(IEventLogger eventLogger)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <inheritdoc />
        public event ActionEventHandler Action;

        /// <summary>Gets the ActionManager instance for this Action control.</summary>
        /// <returns>An ActionManager object.</returns>
        public ActionManager ActionManager => this.actionManager ??= new ActionManager(this.eventLogger, this);

        /// <summary>Gets or sets the ModuleAction for this Action control.</summary>
        /// <returns>A ModuleAction object.</returns>
        public ModuleAction ModuleAction
        {
            get => this.moduleAction ??= this.ModuleControl.ModuleContext.Actions.GetActionByCommandName(this.CommandName);
            set => this.moduleAction = value;
        }

        /// <summary>Gets or sets the ModuleControl instance for this Action control.</summary>
        /// <returns>An IModuleControl object.</returns>
        public IModuleControl ModuleControl { get; set; }

        /// <summary>CreateChildControls builds the control tree.</summary>
        protected override void CreateChildControls()
        {
            // Call base class method to ensure Control Tree is built
            base.CreateChildControls();

            // Set Causes Validation and Enables ViewState to false
            this.CausesValidation = false;
            this.EnableViewState = false;
        }

        /// <summary>OnAction raises the Action Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnAction(ActionEventArgs e)
        {
            this.Action?.Invoke(this, e);
        }

        /// <summary>OnButtonClick runs when the underlying CommandButton is clicked.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnButtonClick(EventArgs e)
        {
            base.OnButtonClick(e);
            if (!this.ActionManager.ProcessAction(this.ModuleAction))
            {
                this.OnAction(new ActionEventArgs(this.ModuleAction, this.ModuleControl.ModuleContext.Configuration));
            }
        }

        /// <summary>OnPreRender runs when just before the Render phase of the Page Lifecycle.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.ModuleAction != null && this.ActionManager.IsVisible(this.ModuleAction))
            {
                this.Text = this.ModuleAction.Title;
                this.CommandArgument = this.ModuleAction.ID.ToString(CultureInfo.InvariantCulture);

                if (this.DisplayIcon && (!string.IsNullOrEmpty(this.ModuleAction.Icon) || !string.IsNullOrEmpty(this.ImageUrl)))
                {
                    if (!string.IsNullOrEmpty(this.ImageUrl))
                    {
                        this.ImageUrl = this.ModuleControl.ModuleContext.Configuration.ContainerPath.Substring(0, this.ModuleControl.ModuleContext.Configuration.ContainerPath.LastIndexOf("/", StringComparison.Ordinal) + 1) + this.ImageUrl;
                    }
                    else
                    {
                        if (this.ModuleAction.Icon.IndexOf("/", StringComparison.Ordinal) > Null.NullInteger)
                        {
                            this.ImageUrl = this.ModuleAction.Icon;
                        }
                        else
                        {
                            this.ImageUrl = "~/images/" + this.ModuleAction.Icon;
                        }
                    }
                }

                this.ActionManager.GetClientScriptURL(this.ModuleAction, this);
            }
            else
            {
                this.Visible = false;
            }
        }
    }
}
