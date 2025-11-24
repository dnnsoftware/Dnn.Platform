// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Controls
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Security;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
    using Microsoft.Extensions.DependencyInjection;

    public class EditHTMLControl : RazorModuleControlBase, IPageContributor
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;

        public EditHTMLControl()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.navigationManager);
        }

        public void ConfigurePage(PageConfigurationContext context)
        {
            context.ClientResourceController.CreateStylesheet("~/DesktopModules/HTML/edit.css").Register();
            context.ClientResourceController.CreateStylesheet("~/Portals/_default/Skins/_default/WebControlSkin/Default/GridView.default.css").Register();
            context.ClientResourceController.CreateScript("~/Resources/Shared/scripts/jquery/jquery.form.min.js").Register();
            context.ClientResourceController.CreateScript("~/DesktopModules/HTML/js/edit.js").Register();
        }

        public override IRazorModuleResult Invoke()
        {
            var model = new EditHtmlViewModel();
            try
            {
                model.LocalResourceFile = this.LocalResourceFile;
                model.ShowEditView = true;
                model.ModuleId = this.ModuleId;
                model.TabId = this.TabId;
                model.PortalId = this.PortalId;
                model.RedirectUrl = this.navigationManager.NavigateURL();
                int workflowID = this.htmlTextController.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;

                var htmlContentItemID = Null.NullInteger;
                var htmlContent = this.htmlTextController.GetTopHtmlText(this.ModuleId, false, workflowID);

                if (htmlContent != null)
                {
                    htmlContentItemID = htmlContent.ItemID;
                    var html = System.Web.HttpUtility.HtmlDecode(htmlContent.Content);
                    model.EditorContent = html;
                }

                var workflow = this.workflowManager.GetWorkflow(workflowID);
                var workflowStates = workflow.States.ToList();
                model.MaxVersions = this.htmlTextController.GetMaximumVersionHistory(this.PortalId);
                var userCanEdit = this.UserInfo.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);

                model.PageSize = Math.Min(Math.Max(model.MaxVersions, 5), 10); // min 5, max 10

                switch (workflow.WorkflowKey)
                {
                    case SystemWorkflowManager.DirectPublishWorkflowKey:
                        model.CurrentWorkflowType = WorkflowType.DirectPublish;
                        break;
                    case SystemWorkflowManager.SaveDraftWorkflowKey:
                        model.CurrentWorkflowType = WorkflowType.SaveDraft;
                        break;
                    case SystemWorkflowManager.ContentAprovalWorkflowKey:
                        model.CurrentWorkflowType = WorkflowType.ContentApproval;
                        break;
                }

                if (htmlContentItemID != -1)
                {
                    this.PopulateModelWithContent(model, htmlContent);
                }
                else
                {
                    this.PopulateModelWithInitialContent(model, workflowStates[0]);
                }

                model.ShowPublishOption = model.CurrentWorkflowType != WorkflowType.DirectPublish;
                model.ShowCurrentVersion = model.CurrentWorkflowType != WorkflowType.DirectPublish;
                model.ShowPreviewVersion = model.CurrentWorkflowType != WorkflowType.DirectPublish;
                model.ShowHistoryView = false;
                model.ShowMasterContentButton = false;
            }
            catch (Exception exc)
            {
                // Exceptions.ProcessModuleLoadException(this, exc);
                throw new Exception("EditHTML", exc);
            }

            return this.View(model);

            // TODO: CSP - enable when CSP implementation is ready
            // this.contentSecurityPolicy.StyleSource.AddInline();
            // this.contentSecurityPolicy.ScriptSource.AddSelf().AddInline();
            // this.contentSecurityPolicy.ImgSource.AddScheme("data:");
        }

        private void PopulateModelWithContent(EditHtmlViewModel model, HtmlTextInfo htmlContent)
        {
            model.CurrentWorkflowInUse = htmlContent.WorkflowName;
            model.CurrentWorkflowState = htmlContent.StateName;
            model.CurrentVersion = htmlContent.Version.ToString();

            // model.Content = this.FormatContent(htmlContent.Content);
        }

        private void PopulateModelWithInitialContent(EditHtmlViewModel model, WorkflowState firstState)
        {
            // model.EditorContent = this.LocalizeString("AddContent");
            model.CurrentWorkflowInUse = firstState.StateName;
            model.ShowCurrentWorkflowState = false;
            model.ShowCurrentVersion = false;
        }
    }
}
