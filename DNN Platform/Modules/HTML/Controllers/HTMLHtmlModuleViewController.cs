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
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Mvc;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Website.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    public class HTMLHtmlModuleViewController : ModuleViewControllerBase
    {
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;

        public HTMLHtmlModuleViewController()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = new HtmlTextController(this.navigationManager);
        }

        protected override object ViewModel(ModuleInfo module)
        {
            int workflowID = this.htmlTextController.GetWorkflow(module.ModuleID, module.TabID, module.PortalID).Value;
            this.ModuleActionPublish(module, workflowID);
            HtmlTextInfo content = this.htmlTextController.GetTopHtmlText(module.ModuleID, true, workflowID);

            var html = string.Empty;
            if (content != null)
            {
                html = System.Web.HttpUtility.HtmlDecode(content.Content);
            }

            return new HtmlModuleModel()
            {
                Html = html,
            };
        }

        private void ModuleActionPublish(ModuleInfo module, int workflowID)
        {
            try
            {
                if (this.Request.QueryString["act"]?.ToString() == "publish" && this.Request.QueryString["mod"]?.ToString() == module.ModuleID.ToString())
                {
                    var moduleContext = new ModuleInstanceContext();
                    moduleContext.Configuration = module;

                    // verify security
                    if (moduleContext.IsEditable && Personalization.GetUserMode() == DotNetNuke.Entities.Portals.PortalSettings.Mode.Edit)
                    {
                        // get content
                        var objHTML = new HtmlTextController(this.navigationManager);
                        HtmlTextInfo objContent = objHTML.GetTopHtmlText(module.ModuleID, false, workflowID);
                        /*
                        var objWorkflow = new WorkflowStateController();
                        if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(workflowID))
                        {
                            // if not direct publish workflow
                            if (objWorkflow.GetWorkflowStates(workflowID).Count > 1)
                            {
                                // publish content
                                objContent.StateID = objWorkflow.GetNextWorkflowStateID(objContent.WorkflowID, objContent.StateID);

                                // save the content
                                objHTML.UpdateHtmlText(objContent, objHTML.GetMaximumVersionHistory(this.PortalSettings.PortalId));

                                // refresh page
                                // this.Response.Redirect(this.navigationManager.NavigateURL(), true);
                            }
                        }
                        */
                    }
                }
            }
            catch (Exception exc)
            {
                // Exceptions.ProcessModuleLoadException(this, exc);
                throw new Exception("HTML Module Publish", exc);
            }
        }
    }
}
