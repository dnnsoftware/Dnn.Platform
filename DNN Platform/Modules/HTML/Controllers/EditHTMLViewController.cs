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
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Security;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Controller for the HTML module's edit view in DotNetNuke (DNN).
    /// <para>
    /// This controller is responsible for preparing and managing the data and resources required for the HTML module's editing interface.
    /// It interacts with the workflow system, content security policy, and navigation manager to provide a secure and user-friendly editing experience.
    /// </para>
    /// <para>
    /// Key responsibilities:
    /// <list type="bullet">
    ///   <item>Initializes dependencies such as navigation, HTML content controller, workflow manager, and content security policy.</item>
    ///   <item>Builds the <see cref="EditHtmlViewModel"/> for the edit view, including workflow state, content, permissions, and UI options.</item>
    ///   <item>Handles content retrieval, workflow state management, and sets up client-side resources (JavaScript and CSS).</item>
    ///   <item>Configures content security policy for inline styles, scripts, and image sources.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Usage:
    /// This controller is used by the DNN MVC pipeline to render and manage the HTML module's edit interface.
    /// </para>
    /// </summary>
    public partial class EditHTMLViewController : ModuleViewControllerBase
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;

        public EditHTMLViewController()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.navigationManager);
        }

        public override string ControlPath => "~/DesktopModules/Html/";

        public override string ID => "EditHTML";

        protected override object ViewModel()
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

            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/Resources/Shared/scripts/jquery/jquery.form.min.js");
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/Portals/_default/Skins/_default/WebControlSkin/Default/GridView.default.css");
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/DesktopModules/HTML/edit.css");
            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/DesktopModules/HTML/js/edit.js");

            // TODO: CSP - enable when CSP implementation is ready
            // this.contentSecurityPolicy.StyleSource.AddInline();
            // this.contentSecurityPolicy.ScriptSource.AddSelf().AddInline();
            // this.contentSecurityPolicy.ImgSource.AddScheme("data:");
            return model;
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
