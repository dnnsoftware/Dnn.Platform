#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Modules.Html.Components;

#endregion

namespace DotNetNuke.Modules.Html
{

    /// <summary>
    ///   The Settings ModuleSettingsBase is used to manage the 
    ///   settings for the HTML Module
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
                return _moduleSettings ?? (_moduleSettings = new HtmlModuleSettingsRepository().GetSettings(this.ModuleConfiguration));
            }
        }


        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cboWorkflow.SelectedIndexChanged += OnWorkflowSelectedIndexChanged;
        }

        protected void OnWorkflowSelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayWorkflowDetails();
        }

        #endregion

        #region Private Methods

        private void DisplayWorkflowDetails()
        {
            if ((cboWorkflow.SelectedValue != null))
            {
                var objWorkflow = new WorkflowStateController();
                var strDescription = "";
                var arrStates = objWorkflow.GetWorkflowStates(int.Parse(cboWorkflow.SelectedValue));
                if (arrStates.Count > 0)
                {
                    foreach (WorkflowStateInfo objState in arrStates)
                    {
                        strDescription = strDescription + " >> " + "<strong>" + objState.StateName + "</strong>";
                    }
                    strDescription = strDescription + "<br />" + ((WorkflowStateInfo)arrStates[0]).Description;
                }
                lblDescription.Text = strDescription;
            }
        }

        #endregion

        #region Base Method Implementations

        /// <summary>
        ///   LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// <remarks>
        /// </remarks>
        public override void LoadSettings()
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    var htmlTextController = new HtmlTextController();
                    var workflowStateController = new WorkflowStateController();

                    chkReplaceTokens.Checked = ModuleSettings.ReplaceTokens;
                    cbDecorate.Checked = ModuleSettings.UseDecorate;

                    // get workflow/version settings
                    var workflows = new ArrayList();
                    foreach (WorkflowStateInfo state in workflowStateController.GetWorkflows(PortalId))
                    {
                        if (!state.IsDeleted)
                        {
                            workflows.Add(state);
                        }
                    }
                    cboWorkflow.DataSource = workflows;
                    cboWorkflow.DataBind();
                    var workflow = htmlTextController.GetWorkflow(ModuleId, TabId, PortalId);
                    if ((cboWorkflow.FindItemByValue(workflow.Value.ToString()) != null))
                    {
                        cboWorkflow.FindItemByValue(workflow.Value.ToString()).Selected = true;
                    }
                    DisplayWorkflowDetails();


                    if (rblApplyTo.Items.FindByValue(workflow.Key) != null)
                    {
                        rblApplyTo.Items.FindByValue(workflow.Key).Selected = true;
                    }

                    txtSearchDescLength.Text = ModuleSettings.SearchDescLength.ToString();
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        ///   UpdateSettings saves the modified settings to the Database
        /// </summary>
        public override void UpdateSettings()
        {
            try
            {
                var htmlTextController = new HtmlTextController();

                // update replace token setting
                ModuleSettings.ReplaceTokens = chkReplaceTokens.Checked;
                ModuleSettings.UseDecorate = cbDecorate.Checked;
                ModuleSettings.SearchDescLength = int.Parse(txtSearchDescLength.Text);
                var repo = new HtmlModuleSettingsRepository();
                repo.SaveSettings(this.ModuleConfiguration, ModuleSettings);

                // disable module caching if token replace is enabled
                if (chkReplaceTokens.Checked)
                {
                    ModuleInfo module = ModuleController.Instance.GetModule(ModuleId, TabId, false);
                    if (module.CacheTime > 0)
                    {
                        module.CacheTime = 0;
                        ModuleController.Instance.UpdateModule(module);
                    }
                }

                // update workflow/version settings
                switch (rblApplyTo.SelectedValue)
                {
                    case "Module":
                        htmlTextController.UpdateWorkflow(ModuleId, rblApplyTo.SelectedValue, Int32.Parse(cboWorkflow.SelectedValue), chkReplace.Checked);
                        break;
                    case "Page":
                        htmlTextController.UpdateWorkflow(TabId, rblApplyTo.SelectedValue, Int32.Parse(cboWorkflow.SelectedValue), chkReplace.Checked);
                        break;
                    case "Site":
                        htmlTextController.UpdateWorkflow(PortalId, rblApplyTo.SelectedValue, Int32.Parse(cboWorkflow.SelectedValue), chkReplace.Checked);
                        break;
                }

                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

    }
}
