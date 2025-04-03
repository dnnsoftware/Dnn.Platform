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

    // using DotNetNuke.Web.Mvc;
    using DotNetNuke.Web.Mvc.Page;
    using DotNetNuke.Website.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    public class DNN_HTMLController : ModuleSettingsController
    {
        // private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly HtmlTextLogController htmlTextLogController = new HtmlTextLogController();
        private readonly WorkflowStateController workflowStateController = new WorkflowStateController();
        private readonly HtmlModuleSettingsRepository settingsRepository;

        public DNN_HTMLController(IContentSecurityPolicy csp, INavigationManager navigationManager)
            : base(navigationManager)
        {
            // this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.NavigationManager);
            this.settingsRepository = new HtmlModuleSettingsRepository();
        }

        public enum WorkflowType
        {
#pragma warning disable SA1602 // Enumeration items should be documented
            DirectPublish = 1,
#pragma warning restore SA1602 // Enumeration items should be documented
#pragma warning disable SA1602 // Enumeration items should be documented
            ContentStaging = 2,
#pragma warning restore SA1602 // Enumeration items should be documented
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(EditHtmlViewModel model)
        {
            try
            {
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);

                htmlContent.Content = model.EditorContent;

                var draftStateID = this.workflowStateController.GetFirstWorkflowStateID(workflowID);
                var publishedStateID = this.workflowStateController.GetLastWorkflowStateID(workflowID);

                switch (model.WorkflowType)
                {
                    case WorkflowType.DirectPublish:
                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId));
                        break;
                    case WorkflowType.ContentStaging:
                        if (model.Publish)
                        {
                            // if it's already published set it to draft
                            if (htmlContent.StateID == publishedStateID)
                            {
                                htmlContent.StateID = draftStateID;
                            }
                            else
                            {
                                htmlContent.StateID = publishedStateID;

                                // here it's in published mode
                            }
                        }
                        else
                        {
                            // if it's already published set it back to draft
                            if (htmlContent.StateID != draftStateID)
                            {
                                htmlContent.StateID = draftStateID;
                            }
                        }

                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId));
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
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);

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

        public ActionResult ShowEdit(EditHtmlViewModel model)
        {
            model.ShowEditView = true;
            model.LocalResourceFile = "DesktopModules\\HTML\\App_LocalResources/EditHTML";
            model.RedirectUrl = this.NavigationManager.NavigateURL(model.TabId);
            try
            {
                int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
                var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);

                model.ShowPublishOption = model.WorkflowType != WorkflowType.DirectPublish;
                model.ShowCurrentVersion = model.WorkflowType != WorkflowType.DirectPublish;

                var workflowStates = this.workflowStateController.GetWorkflowStates(workflowID);
                var maxVersions = this.htmlTextController.GetMaximumVersionHistory(this.PortalSettings.PortalId);

                var htmlContentItemID = -1;

                if (htmlContent != null)
                {
                    htmlContentItemID = htmlContent.ItemID;
                    var html = System.Web.HttpUtility.HtmlDecode(htmlContent.Content);
                    model.EditorContent = html;
                }

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
            // int workflowID = this.htmlTextController.GetWorkflow(model.ModuleId, model.TabId, this.PortalSettings.PortalId).Value;
            // var htmlContent = this.GetLatestHTMLContent(workflowID, model.ModuleId);
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
            htmlContent.StateID = this.workflowStateController.GetFirstWorkflowStateID(workflowID);
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
                    // Récupérer les paramètres existants
                    var moduleSettings = this.settingsRepository.GetSettings(this.ActiveModule);

                    // Mettre à jour les paramètres dans le repository
                    moduleSettings.ReplaceTokens = model.ReplaceTokens;
                    moduleSettings.UseDecorate = model.UseDecorate;
                    moduleSettings.SearchDescLength = model.SearchDescLength;

                    // Sauvegarder les paramètres mis à jour
                    this.settingsRepository.SaveSettings(this.ActiveModule, moduleSettings);

                    // Gérer le CacheTime
                    this.UpdateCacheTime(model.ReplaceTokens);

                    // Mettre à jour les workflows selon la sélection
                    this.UpdateWorkflow(model.SelectedWorkflow, model.ApplyTo, model.Replace);
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

        private List<WorkflowStateInfo> GetWorkflows()
        {
            // Récupérer les workflows disponibles
            var workflowStateController = new WorkflowStateController();
            var workflows = workflowStateController.GetWorkflows(this.ActiveModule.PortalID);
            return workflows.Cast<WorkflowStateInfo>().Where(w => !w.IsDeleted).ToList(); // Filtrer les workflows non supprimés
        }

        private void UpdateWorkflow(string selectedWorkflow, string applyTo, bool replace)
        {
            var htmlTextController = new HtmlTextController(this.NavigationManager);
            var workflow = this.htmlTextController.GetWorkflow(this.ActiveModule.ModuleID, this.ActiveModule.TabID, this.ActiveModule.PortalID);

            // Mettre à jour le workflow selon la sélection
            switch (applyTo)
            {
                case "Module":
                    htmlTextController.UpdateWorkflow(this.ActiveModule.ModuleID, applyTo, int.Parse(selectedWorkflow), replace);
                    break;
                case "Page":
                    htmlTextController.UpdateWorkflow(this.ActiveModule.TabID, applyTo, int.Parse(selectedWorkflow), replace);
                    break;
                case "Site":
                    htmlTextController.UpdateWorkflow(this.ActiveModule.PortalID, applyTo, int.Parse(selectedWorkflow), replace);
                    break;
            }
        }

        private void UpdateCacheTime(bool replaceTokens)
        {
            // Récupérer le module actuel
            var module = ModuleController.Instance.GetModule(this.ActiveModule.ModuleID, this.ActiveModule.TabID, false);
            if (replaceTokens)
            {
                // Désactiver le cache si ReplaceTokens est activé
                module.CacheTime = 0;
            }
            else
            {
                // Réinitialiser le CacheTime à sa valeur par défaut si nécessaire
                module.CacheTime = 60; // ou toute autre valeur par défaut
            }

            // Mettre à jour le module avec le nouveau CacheTime
            ModuleController.Instance.UpdateModule(module);
        }

        private HtmlTextInfo GetLatestHTMLContent(int workflowID, int moduleId)
        {
            var htmlContent = this.htmlTextController.GetTopHtmlText(moduleId, false, workflowID);
            if (htmlContent == null)
            {
                htmlContent = new HtmlTextInfo();
                htmlContent.ItemID = -1;
                htmlContent.StateID = this.workflowStateController.GetFirstWorkflowStateID(workflowID);
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

        private void PopulateModelWithInitialContent(EditHtmlViewModel model, WorkflowStateInfo firstState)
        {
            // model.EditorContent = this.LocalizeString("AddContent");
            model.CurrentWorkflowInUse = firstState.WorkflowName;
            model.ShowCurrentWorkflowState = false;
            model.ShowCurrentVersion = false;
        }
    }
}
