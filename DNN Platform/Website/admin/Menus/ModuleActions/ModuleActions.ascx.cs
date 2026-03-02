// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Admin.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Modules;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which renders module actions.</summary>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="moduleControlPipeline">The module control pipeline.</param>
    /// <param name="javaScript">The JavaScript library helper.</param>
    /// <param name="clientResourceController">The client resources controller.</param>
    /// <param name="servicesFramework">The web API service framework.</param>
    /// <param name="hostSettings">The host settings.</param>
    public partial class ModuleActions(IEventLogger eventLogger, IModuleControlPipeline moduleControlPipeline, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IServicesFramework servicesFramework, IHostSettings hostSettings)
        : ActionBase(eventLogger)
    {
        private readonly List<int> validIDs = [];
        private readonly IModuleControlPipeline moduleControlPipeline = moduleControlPipeline ?? Globals.GetCurrentServiceProvider().GetRequiredService<IModuleControlPipeline>();
        private readonly IJavaScriptLibraryHelper javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
        private readonly IClientResourceController clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        private readonly IServicesFramework servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="ModuleActions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ModuleActions()
            : this(null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleActions"/> class.</summary>
        /// <param name="moduleControlPipeline">The module control pipeline.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ModuleActions(IModuleControlPipeline moduleControlPipeline, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
            : this(null, moduleControlPipeline, javaScript, clientResourceController, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleActions"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="moduleControlPipeline">The module control pipeline.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModuleActions(IEventLogger eventLogger, IModuleControlPipeline moduleControlPipeline, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IServicesFramework servicesFramework)
            : this(eventLogger, moduleControlPipeline, javaScript, clientResourceController, servicesFramework, null)
        {
        }

        protected string AdminText => Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile);

        protected string CustomText => Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile);

        protected string MoveText => Localization.GetString(ModuleActionType.MoveRoot, Localization.GlobalResourceFile);

        protected string AdminActionsJSON { get; set; }

        protected string CustomActionsJSON { get; set; }

        protected bool DisplayQuickSettings { get; set; }

        protected string Panes { get; set; }

        protected bool SupportsMove { get; set; }

        protected bool SupportsQuickSettings { get; set; }

        protected bool IsShared { get; set; }

        protected string ModuleTitle { get; set; }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GlobalResourceFile);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.ID = "ModuleActions";

            this.actionButton.Click += this.ActionButton_Click;

            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);

            this.clientResourceController.RegisterStylesheet("~/admin/menus/ModuleActions/ModuleActions.css", FileOrder.Css.ModuleCss);
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/stylesheets/dnnicons/css/dnnicon.min.css", FileOrder.Css.ModuleCss);
            this.clientResourceController.RegisterScript("~/admin/menus/ModuleActions/ModuleActions.js");

            this.servicesFramework.RequestAjaxAntiForgerySupport();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.AdminActionsJSON = "[]";
            this.CustomActionsJSON = "[]";
            this.Panes = "[]";
            try
            {
                this.SupportsQuickSettings = false;
                this.DisplayQuickSettings = false;
                this.ModuleTitle = this.ModuleContext.Configuration.ModuleTitle;
                var moduleDefinitionId = this.ModuleContext.Configuration.ModuleDefID;
                var quickSettingsControl = ModuleControlController.GetModuleControlByControlKey(this.hostSettings, "QuickSettings", moduleDefinitionId);

                if (quickSettingsControl != null)
                {
                    this.SupportsQuickSettings = true;
                    var control = this.moduleControlPipeline.LoadModuleControl(this.Page, this.ModuleContext.Configuration, "QuickSettings", quickSettingsControl.ControlSrc);
                    control.ID += this.ModuleContext.ModuleId;
                    this.quickSettings.Controls.Add(control);

                    this.DisplayQuickSettings = this.ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("QS_FirstLoad", true);

                    // If we are displaying settings due to first load, then we need to update the setting to false, this WILL trigger a cache clear for the tab
                    if (this.DisplayQuickSettings)
                    {
                        ModuleController.Instance.UpdateModuleSetting(this.ModuleContext.ModuleId, "QS_FirstLoad", "False");
                    }

                    this.clientResourceController.RegisterScript("~/admin/menus/ModuleActions/dnnQuickSettings.js");
                }

                if (this.ActionRoot.Visible)
                {
                    // Add Menu Items
                    foreach (ModuleAction rootAction in this.ActionRoot.Actions)
                    {
                        // Process Children
                        var actions = new List<ModuleAction>();
                        foreach (ModuleAction action in rootAction.Actions)
                        {
                            if (action.Visible)
                            {
                                if ((this.EditMode && Globals.IsAdminControl() == false) ||
                                    (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                {
                                    if (!action.Icon.Contains("://")
                                            && !action.Icon.StartsWith("/", StringComparison.Ordinal)
                                            && !action.Icon.StartsWith("~/", StringComparison.Ordinal))
                                    {
                                        action.Icon = "~/images/" + action.Icon;
                                    }

                                    if (action.Icon.StartsWith("~/", StringComparison.Ordinal))
                                    {
                                        action.Icon = Globals.ResolveUrl(action.Icon);
                                    }

                                    actions.Add(action);

                                    if (string.IsNullOrEmpty(action.Url))
                                    {
                                        this.validIDs.Add(action.ID);
                                    }
                                }
                            }
                        }

                        var oSerializer = new JavaScriptSerializer();
                        if (rootAction.Title == Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile))
                        {
                            this.AdminActionsJSON = oSerializer.Serialize(actions);
                        }
                        else
                        {
                            if (rootAction.Title == Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile))
                            {
                                this.CustomActionsJSON = oSerializer.Serialize(actions);
                            }
                            else
                            {
                                this.SupportsMove = actions.Count > 0;
                                this.Panes = oSerializer.Serialize(this.PortalSettings.ActiveTab.Panes);
                            }
                        }
                    }

                    this.IsShared = this.ModuleContext.Configuration.AllTabs
                        || PortalGroupController.Instance.IsModuleShared(this.ModuleContext.ModuleId, PortalController.Instance.GetPortal(this.PortalSettings.PortalId))
                        || TabController.Instance.GetTabsByModuleID(this.ModuleContext.ModuleId).Count > 1;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            foreach (int id in this.validIDs)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.actionButton.UniqueID, id.ToString());
            }
        }

        private void ActionButton_Click(object sender, EventArgs e)
        {
            this.ProcessAction(this.Request.Params["__EVENTARGUMENT"]);
        }
    }
}
