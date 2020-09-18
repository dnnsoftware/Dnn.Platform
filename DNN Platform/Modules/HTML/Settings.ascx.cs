// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Services.Exceptions;

    /// <summary>
    ///   The Settings ModuleSettingsBase is used to manage the
    ///   settings for the HTML Module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Settings : ModuleSettingsBase
    {
        private HtmlModuleSettings _moduleSettings;

        private new HtmlModuleSettings ModuleSettings
        {
            get
            {
                return this._moduleSettings ?? (this._moduleSettings = new HtmlModuleSettingsRepository().GetSettings(this.ModuleConfiguration));
            }
        }

        /// <summary>
        ///   LoadSettings loads the settings from the Database and displays them.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public override void LoadSettings()
        {
            try
            {
                if (!this.Page.IsPostBack)
                {
                    var htmlTextController = new HtmlTextController();
                    var workflowStateController = new WorkflowStateController();

                    this.chkReplaceTokens.Checked = this.ModuleSettings.ReplaceTokens;
                    this.cbDecorate.Checked = this.ModuleSettings.UseDecorate;

                    // get workflow/version settings
                    var workflows = new ArrayList();
                    foreach (WorkflowStateInfo state in workflowStateController.GetWorkflows(this.PortalId))
                    {
                        if (!state.IsDeleted)
                        {
                            workflows.Add(state);
                        }
                    }

                    this.cboWorkflow.DataSource = workflows;
                    this.cboWorkflow.DataBind();
                    var workflow = htmlTextController.GetWorkflow(this.ModuleId, this.TabId, this.PortalId);
                    if (this.cboWorkflow.FindItemByValue(workflow.Value.ToString()) != null)
                    {
                        this.cboWorkflow.FindItemByValue(workflow.Value.ToString()).Selected = true;
                    }

                    this.DisplayWorkflowDetails();

                    if (this.rblApplyTo.Items.FindByValue(workflow.Key) != null)
                    {
                        this.rblApplyTo.Items.FindByValue(workflow.Key).Selected = true;
                    }

                    this.txtSearchDescLength.Text = this.ModuleSettings.SearchDescLength.ToString();
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        ///   UpdateSettings saves the modified settings to the Database.
        /// </summary>
        public override void UpdateSettings()
        {
            try
            {
                var htmlTextController = new HtmlTextController();

                // update replace token setting
                this.ModuleSettings.ReplaceTokens = this.chkReplaceTokens.Checked;
                this.ModuleSettings.UseDecorate = this.cbDecorate.Checked;
                this.ModuleSettings.SearchDescLength = int.Parse(this.txtSearchDescLength.Text);
                var repo = new HtmlModuleSettingsRepository();
                repo.SaveSettings(this.ModuleConfiguration, this.ModuleSettings);

                // disable module caching if token replace is enabled
                if (this.chkReplaceTokens.Checked)
                {
                    ModuleInfo module = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
                    if (module.CacheTime > 0)
                    {
                        module.CacheTime = 0;
                        ModuleController.Instance.UpdateModule(module);
                    }
                }

                // update workflow/version settings
                switch (this.rblApplyTo.SelectedValue)
                {
                    case "Module":
                        htmlTextController.UpdateWorkflow(this.ModuleId, this.rblApplyTo.SelectedValue, int.Parse(this.cboWorkflow.SelectedValue), this.chkReplace.Checked);
                        break;
                    case "Page":
                        htmlTextController.UpdateWorkflow(this.TabId, this.rblApplyTo.SelectedValue, int.Parse(this.cboWorkflow.SelectedValue), this.chkReplace.Checked);
                        break;
                    case "Site":
                        htmlTextController.UpdateWorkflow(this.PortalId, this.rblApplyTo.SelectedValue, int.Parse(this.cboWorkflow.SelectedValue), this.chkReplace.Checked);
                        break;
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cboWorkflow.SelectedIndexChanged += this.OnWorkflowSelectedIndexChanged;
        }

        protected void OnWorkflowSelectedIndexChanged(object sender, EventArgs e)
        {
            this.DisplayWorkflowDetails();
        }

        private void DisplayWorkflowDetails()
        {
            if (this.cboWorkflow.SelectedValue != null)
            {
                var objWorkflow = new WorkflowStateController();
                var strDescription = string.Empty;
                var arrStates = objWorkflow.GetWorkflowStates(int.Parse(this.cboWorkflow.SelectedValue));
                if (arrStates.Count > 0)
                {
                    foreach (WorkflowStateInfo objState in arrStates)
                    {
                        strDescription = strDescription + " >> " + "<strong>" + objState.StateName + "</strong>";
                    }

                    strDescription = strDescription + "<br />" + ((WorkflowStateInfo)arrStates[0]).Description;
                }

                this.lblDescription.Text = strDescription;
            }
        }
    }
}
