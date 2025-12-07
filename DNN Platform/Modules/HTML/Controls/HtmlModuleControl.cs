// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html.Controls
{
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
    using Microsoft.Extensions.DependencyInjection;

    public class HtmlModuleControl : RazorModuleControlBase, IActionable
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly IWorkflowManager workflowManager;
        private readonly IClientResourceController clientResourceController;

        private HtmlModuleSettings settings;

        public HtmlModuleControl(INavigationManager navigationManager, IClientResourceController clientResourceController)
        {
            this.navigationManager = navigationManager;
            this.htmlTextController = new HtmlTextController(this.navigationManager);
            this.workflowManager = WorkflowManager.Instance;
            this.clientResourceController = clientResourceController;
        }

        public HtmlModuleSettings HtmlSettings
        {
            get
            {
                if (this.settings == null)
                {
                    var repo = new HtmlModuleSettingsRepository();
                    this.settings = repo.GetSettings(this.ModuleConfiguration);
                }

                return this.settings;
            }
        }

        /// <summary>  Gets moduleActions is an interface property that returns the module actions collection for the module.</summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                // add the Edit Text action
                var actions = new ModuleActionCollection();
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    string.Empty,
                    this.EditUrl(),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                // add mywork to action menu
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString("MyWork.Action", this.LocalResourceFile),
                    "MyWork.Action",
                    string.Empty,
                    "view.gif",
                    this.EditUrl("MyWork"),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                return actions;
            }
        }

        public override IRazorModuleResult Invoke()
        {
            bool editorEnabled = this.PortalSettings.InlineEditorEnabled;
            if (editorEnabled && this.ModuleContext.IsEditable && Personalization.GetUserMode() == DotNetNuke.Entities.Portals.PortalSettings.Mode.Edit)
            {
                editorEnabled = true;
            }
            else
            {
                editorEnabled = false;
            }

            int workflowID = this.htmlTextController.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;
            HtmlTextInfo content = this.htmlTextController.GetTopHtmlText(this.ModuleId, !this.ModuleContext.IsEditable, workflowID);

            var contentString = string.Empty;
            if (content != null)
            {
                contentString = content.Content;
            }
            else
            {
                if (Personalization.GetUserMode() == DotNetNuke.Entities.Portals.PortalSettings.Mode.Edit)
                {
                    contentString = editorEnabled
                        ? Localization.GetString("AddContentFromToolBar.Text", this.LocalResourceFile)
                        : Localization.GetString("AddContentFromActionMenu.Text", this.LocalResourceFile);
                }
                else
                {
                    return this.Content(string.Empty);
                }
            }

            var html = HtmlTextController.FormatHtmlText(this.ModuleId, contentString, this.HtmlSettings, this.PortalSettings, this.clientResourceController);

            // html = System.Web.HttpUtility.HtmlDecode(html);
            return this.View(new HtmlModuleModel()
            {
                Html = html,
            });
        }
    }
}
