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
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Web.MvcPipeline.Controllers;

    public class DNN_HTMLController : ModuleSettingsController
    {
        // private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly HtmlTextLogController htmlTextLogController = new HtmlTextLogController();
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
        private readonly HtmlModuleSettingsRepository settingsRepository;

        public DNN_HTMLController(IContentSecurityPolicy csp, INavigationManager navigationManager)
            : base(navigationManager)
        {
            // this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.NavigationManager);
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
                // return this.View("Error", new ErrorViewModel { Message = exc.Message });
                throw new Exception(exc.Message, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowHistory(EditHtmlViewModel model)
        {
            model.ShowHistoryView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.NavigationManager.NavigateURL(model.TabId);

            // model.LocalResourceFile = Path.Combine(Path.GetDirectoryName(this.ActiveModule.ModuleControl.ControlSrc), Localization.LocalResourceDirectory + "/" + Path.GetFileNameWithoutExtension(this.ActiveModule.ModuleControl.ControlSrc));
            try
            {
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);

                var maxVersions = this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId);
                model.MaxVersions = maxVersions;

                // var htmlLogging = this.htmlTextLogController.GetHtmlTextLog(htmlContent.ItemID);
                var versions = this.htmlTextController.GetAllHtmlText(model.ModuleId);
                model.VersionItems = versions.Cast<HtmlTextInfo>().ToList();

                // return this.PartialView(this.ActiveModule, "_History", model);
                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                // return this.View("Error", new ErrorViewModel { Message = exc.Message });
                throw new Exception(exc.Message, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowPreview(EditHtmlViewModel model)
        {
            model.ShowPreviewView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.NavigationManager.NavigateURL(model.TabId);

            // model.LocalResourceFile = Path.Combine(Path.GetDirectoryName(this.ActiveModule.ModuleControl.ControlSrc), Localization.LocalResourceDirectory + "/" + Path.GetFileNameWithoutExtension(this.ActiveModule.ModuleControl.ControlSrc));
            try
            {
                model.PreviewContent = model.EditorContent; // HttpUtility.HtmlDecode(model.HiddenEditorContent);
                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                // return this.View("Error", new ErrorViewModel { Message = exc.Message });
                throw new Exception(exc.Message, exc);
            }
        }

        public ActionResult ShowEdit(EditHtmlViewModel model)
        {
            model.ShowEditView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.NavigationManager.NavigateURL(model.TabId);
            try
            {
                // return this.PartialView(this.ActiveModule, "_Edit", model);
                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                // return this.View("Error", new ErrorViewModel { Message = exc.Message });
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
            model.RedirectUrl = this.NavigationManager.NavigateURL(model.TabId);

            // model.LocalResourceFile = Path.Combine(Path.GetDirectoryName(this.ActiveModule.ModuleControl.ControlSrc), Localization.LocalResourceDirectory + "/" + Path.GetFileNameWithoutExtension(this.ActiveModule.ModuleControl.ControlSrc));
            try
            {
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.htmlTextController.GetHtmlText(model.ModuleId, model.ItemID);

                var moduleSettings = this.settingsRepository.GetSettings(this.ActiveModule);
                model.PreviewContent = HtmlTextController.FormatHtmlText(model.ModuleId, htmlContent.Content, moduleSettings, this.PortalSettings, null);

                // return this.PartialView(this.ActiveModule, "_History", model);
                return this.PartialView(this.ActiveModule, "EditHtml", model);
            }
            catch (Exception exc)
            {
                // Gérer l'exception
                // return this.View("Error", new ErrorViewModel { Message = exc.Message });
                throw new Exception(exc.Message, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSettings(HtmlModuleSettingsModel model)
        {
            // if (ModelState.IsValid)
            {
                try
                {
                    var moduleSettings = this.settingsRepository.GetSettings(this.ActiveModule);
                    var htmlTextController = new HtmlTextController(this.NavigationManager);
                    moduleSettings.ReplaceTokens = model.ReplaceTokens;
                    moduleSettings.UseDecorate = model.UseDecorate;
                    moduleSettings.SearchDescLength = model.SearchDescLength;

                    var repo = new HtmlModuleSettingsRepository();
                    repo.SaveSettings(this.ActiveModule, moduleSettings);

                    // disable module caching if token replace is enabled
                    if (model.ReplaceTokens)
                    {
                        ModuleInfo module = ModuleController.Instance.GetModule(model.ModuleId, model.TabId, false);
                        if (module.CacheTime > 0)
                        {
                            module.CacheTime = 0;
                            ModuleController.Instance.UpdateModule(module);
                        }
                    }
                }
                catch (Exception exc)
                {
                    // Gérer les exceptions
                    // Exceptions.ProcessModuleLoadException(this, exc);
                    throw new Exception("sModuleLoadException", exc);
                }

                return this.UpdateDefaultSettings(model);
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
