// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ActionBase is an abstract base control for Action objects that inherit from UserControl.</summary>
    /// <remarks>ActionBase inherits from <see cref="UserControl"/>, and implements the <see cref="IActionControl"/> Interface.</remarks>
    public abstract class ActionBase : UserControl, IActionControl
    {
        /// <summary>Defines if the action supports icons.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. Use SupportsIcons property. Scheduled for removal in v11.0.0.")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "Keeping the name to prevent a breaking change, will be removed in v11.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "In v11, we will make this private and rename.")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected bool m_supportsIcons = true;

        private ActionManager actionManager;
        private ModuleAction actionRoot;

        /// <summary>Initializes a new instance of the <see cref="ActionBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        protected ActionBase()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ActionBase"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        protected ActionBase(IEventLogger eventLogger)
        {
            this.EventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <inheritdoc />
        public event ActionEventHandler Action;

        /// <summary>Gets a value indicating whether the page is in edit mode.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool EditMode => Personalization.GetUserMode() != PortalSettings.Mode.View;

        /// <summary>Gets a value indicating whether the action supports icons.</summary>
        public bool SupportsIcons => this.m_supportsIcons;

        /// <summary>Gets the ActionManager instance for this Action control.</summary>
        /// <returns>An ActionManager object.</returns>
        public ActionManager ActionManager => this.actionManager ??= new ActionManager(this.EventLogger, this);

        /// <summary>Gets or sets the ModuleControl instance for this Action control.</summary>
        /// <returns>An IModuleControl object.</returns>
        public IModuleControl ModuleControl { get; set; }

        /// <summary>Gets the Actions Collection.</summary>
        /// <returns>A ModuleActionCollection.</returns>
        protected ModuleActionCollection Actions => this.ModuleContext.Actions;

        /// <summary>Gets the ActionRoot.</summary>
        /// <returns>A ModuleActionCollection.</returns>
        protected ModuleAction ActionRoot =>
            this.actionRoot ??= new ModuleAction(
                this.ModuleContext.GetNextActionID(),
                Localization.GetString("Manage.Text", Localization.GlobalResourceFile),
                string.Empty,
                string.Empty,
                "manage-icn.png");

        /// <summary>Gets the ModuleContext.</summary>
        /// <returns>A ModuleInstanceContext.</returns>
        protected ModuleInstanceContext ModuleContext => this.ModuleControl.ModuleContext;

        /// <summary>Gets the PortalSettings.</summary>
        /// <returns>A PortalSettings object.</returns>
        protected PortalSettings PortalSettings => this.ModuleControl.ModuleContext.PortalSettings;

        /// <summary>Gets the event logger.</summary>
        protected IEventLogger EventLogger { get; }

        /// <summary>DisplayControl determines whether the control should be displayed.</summary>
        /// <param name="objNodes">A collection of Dnn nodes, <see cref="DNNNodeCollection"/>.</param>
        /// <returns>A value indicating whether the control should be displayed.</returns>
        protected bool DisplayControl(DNNNodeCollection objNodes)
        {
            return this.ActionManager.DisplayControl(objNodes);
        }

        /// <summary>OnAction raises the Action Event for this control.</summary>
        /// <param name="e">The action event arguments.</param>
        protected virtual void OnAction(ActionEventArgs e)
        {
            this.Action?.Invoke(this, e);
        }

        /// <summary>ProcessAction processes the action event.</summary>
        /// <param name="actionID">The id of the action.</param>
        protected void ProcessAction(string actionID)
        {
            if (int.TryParse(actionID, out var output))
            {
                ModuleAction action = this.Actions.GetActionByID(output);
                if (action != null)
                {
                    if (!this.ActionManager.ProcessAction(action))
                    {
                        this.OnAction(new ActionEventArgs(action, this.ModuleContext.Configuration));
                    }
                }
            }
        }

        /// <summary>Page_Load runs when the class is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (this.ModuleControl == null)
                {
                    return;
                }

                this.ActionRoot.Actions.AddRange(this.Actions);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            base.OnLoad(e);
        }
    }
}
