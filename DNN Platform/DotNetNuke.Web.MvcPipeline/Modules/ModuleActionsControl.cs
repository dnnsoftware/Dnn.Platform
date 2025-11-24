// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;


    public class ModuleActionsControl : RazorModuleControlBase, IPageContributor
    {
        private ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleActionsControl));
        private readonly List<int> validIDs = new List<int>();
        private ModuleAction actionRoot;

        private Dictionary<string, string> actionScripts = new Dictionary<string, string>();

        public bool EditMode
        {
            get
            {
                return Personalization.GetUserMode() != PortalSettings.Mode.View;
            }
        }

        protected ModuleAction ActionRoot
        {
            get
            {
                if (this.actionRoot == null)
                {
                    this.actionRoot = new ModuleAction(this.ModuleContext.GetNextActionID(), Localization.GetString("Manage.Text", Localization.GlobalResourceFile), string.Empty, string.Empty, "manage-icn.png");
                }

                return this.actionRoot;
            }
        }

        protected string AdminText
        {
            get { return Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile); }
        }

        protected string CustomText
        {
            get { return Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile); }
        }

        protected string MoveText
        {
            get { return Localization.GetString(ModuleActionType.MoveRoot, Localization.GlobalResourceFile); }
        }

        protected PortalSettings PortalSettings
        {
            get
            {
                return this.ModuleContext.PortalSettings;
            }
        }

        protected string AdminActionsJSON { get; set; }

        protected string CustomActionsJSON { get; set; }

        protected bool DisplayQuickSettings { get; set; }

        protected string Panes { get; set; }

        protected bool SupportsMove { get; set; }

        protected bool SupportsQuickSettings { get; set; }

        protected bool IsShared { get; set; }

        protected string ModuleTitle { get; set; }

        protected ModuleActionCollection Actions
        {
            get
            {
                return this.ModuleControl.ModuleContext.Actions;
            }
        }

        public IModuleControl ModuleControl { get; set; }

        public override IRazorModuleResult Invoke()
        {
            var moduleInfo = ModuleConfiguration;
            this.OnLoad(moduleInfo);

            var viewModel = new Models.ModuleActionsModel
            {
                ModuleContext = moduleInfo,
                SupportsQuickSettings = this.SupportsQuickSettings,
                DisplayQuickSettings = this.DisplayQuickSettings,
                // QuickSettingsModel = this.qu,
                CustomActionsJSON = this.CustomActionsJSON,
                AdminActionsJSON = this.AdminActionsJSON,
                Panes = this.Panes,
                CustomText = this.CustomText,
                AdminText = this.AdminText,
                MoveText = this.MoveText,
                SupportsMove = this.SupportsMove,
                IsShared = this.IsShared,
                ModuleTitle = moduleInfo.ModuleTitle,
                ActionScripts = this.actionScripts,
            };

            return View("ModuleActions", viewModel);
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GlobalResourceFile);
        }
       
        protected void OnLoad(ModuleInfo moduleInfo)
        {
            this.ActionRoot.Actions.AddRange(this.Actions);
            this.AdminActionsJSON = "[]";
            this.CustomActionsJSON = "[]";
            this.Panes = "[]";
            try
            {
                this.SupportsQuickSettings = false;
                this.DisplayQuickSettings = false;
                this.ModuleTitle = this.ModuleContext.Configuration.ModuleTitle;
                var moduleDefinitionId = this.ModuleContext.Configuration.ModuleDefID;
                var quickSettingsControl = ModuleControlController.GetModuleControlByControlKey("QuickSettings", moduleDefinitionId);

                if (quickSettingsControl != null)
                {
                    this.SupportsQuickSettings = true;
                    /*
                    var control = ModuleControlFactory.LoadModuleControl(this.Page, this.ModuleContext.Configuration, "QuickSettings", quickSettingsControl.ControlSrc);
                    control.ID += this.ModuleContext.ModuleId;
                    this.quickSettings.Controls.Add(control);

                    this.DisplayQuickSettings = this.ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("QS_FirstLoad", true);
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleContext.ModuleId, "QS_FirstLoad", "False");

                    ClientResourceManager.RegisterScript(this.Page, "~/admin/menus/ModuleActions/dnnQuickSettings.js");
                    */
                }

                if (this.ActionRoot.Visible && Globals.IsAdminControl() == false)
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
                                if (this.EditMode && Globals.IsAdminControl() == false ||
                                    action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View)
                                {
                                    if (!action.Icon.Contains("://")
                                            && !action.Icon.StartsWith("/")
                                            && !action.Icon.StartsWith("~/"))
                                    {
                                        action.Icon = "~/images/" + action.Icon;
                                    }

                                    if (action.Icon.StartsWith("~/"))
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

                            if (string.IsNullOrEmpty(action.ClientScript) && !string.IsNullOrEmpty(action.Url) && action.Url.StartsWith("javascript:"))
                            {
                                if (!DotNetNuke.UI.UIUtilities.IsLegacyUI(this.ModuleContext.ModuleId, action.ControlKey, this.ModuleContext.PortalId))
                                {
                                    action.ClientScript = UrlUtils.PopUpUrl(action.Url, null, this.PortalSettings, true, false);
                                }
                            }

                            if (!string.IsNullOrEmpty(action.ClientScript) && !string.IsNullOrEmpty(action.Url))
                            {
                                this.actionScripts.Add(action.Url, action.ClientScript);
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

                    this.SupportsMove = true;
                }
                
            }
            catch (Exception exc)
            {
                // Exceptions.ProcessModuleLoadException(this, exc);
                throw exc;
            }
        }

        public void ConfigurePage(PageConfigurationContext context)
        {
            context.ClientResourceController
                  .CreateStylesheet("~/admin/menus/ModuleActions/ModuleActions.css")
                  .SetPriority(FileOrder.Css.ModuleCss)
                  .Register();
            context.ClientResourceController
                .CreateStylesheet("~/Resources/Shared/stylesheets/dnnicons/css/dnnicon.min.css")
                .SetPriority(FileOrder.Css.ModuleCss)
                .Register();
            context.ClientResourceController
                .CreateScript("~/admin/menus/ModuleActions/ModuleActions.js")
                .Register();
            context.JavaScriptLibraryHelper.RequestRegistration(CommonJs.DnnPlugins);
            context.ServicesFramework.RequestAjaxAntiForgerySupport();
        }

    }
}
