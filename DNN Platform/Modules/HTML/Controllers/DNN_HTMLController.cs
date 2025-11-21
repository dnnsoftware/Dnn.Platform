// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Web.MvcPipeline.Controllers;

    public class DNN_HTMLController : ModuleControllerBase
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly HtmlTextLogController htmlTextLogController = new HtmlTextLogController();
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
        private readonly HtmlModuleSettingsRepository settingsRepository;

        public DNN_HTMLController(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
            this.htmlTextController = new HtmlTextController(this.navigationManager);
            this.settingsRepository = new HtmlModuleSettingsRepository();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(EditHtmlViewModel model)
        {
            try
            {
                // get content
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);

                var aliases = from PortalAliasInfo pa in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalSettings.PortalId)
                              select pa.HTTPAlias;

                var content = model.EditorContent;

                if (this.Request.QueryString["nuru"] == null)
                {
                    content = HtmlUtils.AbsoluteToRelativeUrls(content, aliases);
                }

                htmlContent.Content = content;
                var workflow = this.workflowManager.GetWorkflow(workflowID);
                var draftStateID = workflow.FirstState.StateID;
                var publishedStateID = workflow.LastState.StateID;

                switch (model.CurrentWorkflowType)
                {
                    case WorkflowType.DirectPublish:
                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(model.PortalId));

                        break;
                    case WorkflowType.SaveDraft:
                    case WorkflowType.ContentApproval:
                        // if it's already published set it back to draft
                        if (htmlContent.StateID == publishedStateID)
                        {
                            htmlContent.StateID = draftStateID;
                        }

                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(model.PortalId));
                        break;
                }

                return new EmptyResult();
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                throw new Exception(exc.Message, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowHistory(EditHtmlViewModel model)
        {
            model.ShowHistoryView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.navigationManager.NavigateURL(model.TabId);

            try
            {
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);

                var maxVersions = this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId);
                model.MaxVersions = maxVersions;

                var versions = this.htmlTextController.GetAllHtmlText(model.ModuleId);
                model.VersionItems = versions.Cast<HtmlTextInfo>().ToList();

                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                throw new Exception(exc.Message, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowPreview(EditHtmlViewModel model)
        {
            model.ShowPreviewView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.navigationManager.NavigateURL(model.TabId);

            try
            {
                model.PreviewContent = model.EditorContent; // HttpUtility.HtmlDecode(model.HiddenEditorContent);
                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                throw new Exception(exc.Message, exc);
            }
        }

        public ActionResult ShowEdit(EditHtmlViewModel model)
        {
            model.ShowEditView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.navigationManager.NavigateURL(model.TabId);
            try
            {
                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                throw new Exception(exc.Message, exc);
            }
        }

        public ActionResult HistoryRemove(EditHtmlViewModel model)
        {
            this.htmlTextController.DeleteHtmlText(model.ModuleId, model.ItemID);
            return this.ShowEdit(model);
        }

        public ActionResult HistoryRollback(EditHtmlViewModel model)
        {
            int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
            var htmlContent = this.htmlTextController.GetHtmlText(model.ModuleId, model.ItemID);
            htmlContent.ItemID = -1;
            htmlContent.ModuleID = model.ModuleId;
            htmlContent.WorkflowID = workflowID;
            htmlContent.StateID = this.workflowManager.GetWorkflow(workflowID).FirstState.StateID;
            this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId));
            return this.ShowEdit(model);
        }

        public ActionResult HistoryPreview(EditHtmlViewModel model)
        {
            model.ShowPreviewView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.navigationManager.NavigateURL(model.TabId);

            try
            {
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.htmlTextController.GetHtmlText(model.ModuleId, model.ItemID);

                var moduleSettings = this.settingsRepository.GetSettings(this.ActiveModule);
                model.PreviewContent = HtmlTextController.FormatHtmlText(model.ModuleId, htmlContent.Content, moduleSettings, this.PortalSettings, null);

                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                throw new Exception(exc.Message, exc);
            }
        }

        private HtmlTextInfo GetLatestHTMLContent(int workflowID, int moduleId)
        {
            var htmlContent = this.htmlTextController.GetTopHtmlText(moduleId, false, workflowID);
            if (htmlContent == null)
            {
                htmlContent = new HtmlTextInfo();
                htmlContent.ItemID = -1;
                htmlContent.StateID = this.workflowManager.GetWorkflow(workflowID).FirstState.StateID;
                htmlContent.WorkflowID = workflowID;
                htmlContent.ModuleID = moduleId;
            }

            return htmlContent;
        }
    }
}
