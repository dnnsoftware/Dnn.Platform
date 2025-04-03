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

    using DotNetNuke.Web.MvcPipeline.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    public class HTMLSettingsViewController : ModuleControllerBase
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly WorkflowStateController workflowStateController = new WorkflowStateController();
        private readonly HtmlModuleSettingsRepository settingsRepository;

        public HTMLSettingsViewController()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.navigationManager);
            this.settingsRepository = new HtmlModuleSettingsRepository();
        }

        [HttpGet]
        [ChildActionOnly]
        public ActionResult Invoke(int moduleId)
        {
            var moduleSettings = this.settingsRepository.GetSettings(this.ActiveModule);
            var workflow = this.htmlTextController.GetWorkflow(this.ActiveModule.ModuleID, this.ActiveModule.TabID, this.ActiveModule.PortalID);

            var model = new HtmlModuleSettingsModel
            {
                // Assigner les valeurs des paramètres au modèle directement depuis le repository
                ReplaceTokens = moduleSettings.ReplaceTokens,
                UseDecorate = moduleSettings.UseDecorate,
                SearchDescLength = moduleSettings.SearchDescLength,
                Workflows = this.GetWorkflows(), // Récupérer les workflows disponibles
                ApplyTo = workflow.Key,
                SelectedWorkflow = workflow.Value.ToString(),
            };

            return this.PartialView(this.ActiveModule, "LoadSettings", model);
        }

        private List<WorkflowStateInfo> GetWorkflows()
        {
            // Récupérer les workflows disponibles
            var workflows = this.workflowStateController.GetWorkflows(this.ActiveModule.PortalID);
            return workflows.Cast<WorkflowStateInfo>().Where(w => !w.IsDeleted).ToList(); // Filtrer les workflows non supprimés
        }
    }
}
