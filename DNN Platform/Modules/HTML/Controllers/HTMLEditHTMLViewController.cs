// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Mvc;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Mvc;

    // using DotNetNuke.Web.Mvc;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Website.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    using static DotNetNuke.Modules.Html.Controllers.DNN_HTMLController;

    public class HTMLEditHTMLViewController : ModuleViewControllerBase
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly WorkflowStateController workflowStateController = new WorkflowStateController();
        private readonly IContentSecurityPolicy contentSecurityPolicy;

        public HTMLEditHTMLViewController(IContentSecurityPolicy csp)
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.navigationManager);

            // this.contentSecurityPolicy = Globals.DependencyProvider.GetRequiredService<IContentSecurityPolicy>();
            this.contentSecurityPolicy = csp;
        }

        protected override object ViewModel(ModuleInfo module)
        {
            var model = new EditHtmlViewModel();

            // model.LocalResourceFile = "/DesktopModules/Html/" + Localization.LocalResourceDirectory + "/EditHTML";
            model.LocalResourceFile = Path.Combine(Path.GetDirectoryName(module.ModuleControl.ControlSrc), Localization.LocalResourceDirectory + "/" + Path.GetFileNameWithoutExtension(module.ModuleControl.ControlSrc));
            model.ShowEditView = true;
            model.ModuleId = module.ModuleID;
            model.TabId = module.TabID;
            model.PortalId = this.PortalSettings.PortalId;
            model.RedirectUrl = this.navigationManager.NavigateURL();
            int workflowID = this.htmlTextController.GetWorkflow(module.ModuleID, module.TabID, module.PortalID).Value;

            try
            {
                var htmlContentItemID = -1;
                var htmlContent = this.htmlTextController.GetTopHtmlText(module.ModuleID, false, workflowID);

                if (htmlContent != null)
                {
                    htmlContentItemID = htmlContent.ItemID;
                    var html = System.Web.HttpUtility.HtmlDecode(htmlContent.Content);
                    model.EditorContent = html;
                }

                var workflowStates = this.workflowStateController.GetWorkflowStates(workflowID);
                var maxVersions = this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId);

                model.MaxVersions = maxVersions;

                model.WorkflowType = workflowStates.Count == 1 ? WorkflowType.DirectPublish : WorkflowType.ContentStaging;
                if (htmlContentItemID != -1)
                {
                    this.PopulateModelWithContent(model, htmlContent);
                }
                else
                {
                    this.PopulateModelWithInitialContent(model, workflowStates[0] as WorkflowStateInfo);
                }

                model.ShowPublishOption = model.WorkflowType != WorkflowType.DirectPublish;
                model.ShowCurrentVersion = model.WorkflowType != WorkflowType.DirectPublish;
                model.ShowPreviewVersion = model.WorkflowType != WorkflowType.DirectPublish;
                model.ShowHistoryView = false;
                model.ShowMasterContentButton = false;

                // model.RenderOptions = this.GetRenderOptions();
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                // Exceptions.ProcessModuleLoadException(this, exc);
                throw new Exception("EditHTML", exc);
            }

            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/Resources/Shared/scripts/jquery/jquery.form.min.js");
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/Portals/_default/Skins/_default/WebControlSkin/Default/GridView.default.css");
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/DesktopModules/HTML/edit.css");
            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/DesktopModules/HTML/js/edit.js");

            this.contentSecurityPolicy.StyleSource.AddInline();
            this.contentSecurityPolicy.ScriptSource.AddSelf().AddInline();
            this.contentSecurityPolicy.ImgSource.AddScheme("data:");
            return model;
        }

        private void PopulateModelWithContent(EditHtmlViewModel model, HtmlTextInfo htmlContent)
        {
            model.CurrentWorkflowInUse = htmlContent.WorkflowName;
            model.CurrentWorkflowState = htmlContent.StateName;
            model.CurrentVersion = htmlContent.Version.ToString();

            // model.Content = this.FormatContent(htmlContent.Content);
        }

        private void PopulateModelWithInitialContent(EditHtmlViewModel model, WorkflowStateInfo firstState)
        {
            // model.EditorContent = this.LocalizeString("AddContent");
            model.CurrentWorkflowInUse = firstState.WorkflowName;
            model.ShowCurrentWorkflowState = false;
            model.ShowCurrentVersion = false;
        }
    }
}
