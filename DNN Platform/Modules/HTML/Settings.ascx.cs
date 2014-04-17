#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Modules.Html
{

    /// <summary>
    ///   The Settings ModuleSettingsBase is used to manage the 
    ///   settings for the HTML Module
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [leupold]	    08/12/2007	created
    /// </history>
    public partial class Settings : ModuleSettingsBase
    {

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
                    strDescription = strDescription + "<br />" + ((WorkflowStateInfo) arrStates[0]).Description;
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
        /// <history>
        /// </history>
        public override void LoadSettings()
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    var htmlTextController = new HtmlTextController();
                    var workflowStateController = new WorkflowStateController();

                    // get replace token settings
                    if (ModuleSettings["HtmlText_ReplaceTokens"] != null)
                    {
                        chkReplaceTokens.Checked = Convert.ToBoolean(ModuleSettings["HtmlText_ReplaceTokens"]);
                    }

					//get decoration setting, set to true as default.
					cbDecorate.Checked = !ModuleSettings.ContainsKey("HtmlText_UseDecorate")
											|| ModuleSettings["HtmlText_UseDecorate"].ToString() == "1";

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

					txtSearchDescLength.Text = ModuleSettings.ContainsKey("HtmlText_SearchDescLength") 
                                                && !string.IsNullOrEmpty(ModuleSettings["HtmlText_SearchDescLength"].ToString())
								                    ? ModuleSettings["HtmlText_SearchDescLength"].ToString() 
                                                    : HtmlTextController.MAX_DESCRIPTION_LENGTH.ToString();
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
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "HtmlText_ReplaceTokens", chkReplaceTokens.Checked.ToString());
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "HtmlText_UseDecorate", cbDecorate.Checked ? "1" : "0");
                ModuleController.Instance.UpdateModuleSetting(ModuleId, "HtmlText_SearchDescLength", txtSearchDescLength.Text);

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