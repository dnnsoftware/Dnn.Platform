#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Modules.Html.Components;


#endregion

namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlModule Class provides the UI for displaying the Html
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class HtmlModule : HtmlModuleBase, IActionable
    {
        private bool EditorEnabled;
        private int WorkflowID;

        #region "Private Methods"

        #endregion

        #region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Page_Init runs when the control is initialized
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            lblContent.UpdateLabel += lblContent_UpdateLabel;
            EditorEnabled = PortalSettings.InlineEditorEnabled;
            try
            {
                WorkflowID = new HtmlTextController().GetWorkflow(ModuleId, TabId, PortalId).Value;

                //Add an Action Event Handler to the Skin
                AddActionHandler(ModuleAction_Click);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                var objHTML = new HtmlTextController();

                // edit in place
                if (EditorEnabled && IsEditable && PortalSettings.UserMode == PortalSettings.Mode.Edit)
                {
                    EditorEnabled = true;
                }
                else
                {
                    EditorEnabled = false;
                }

                // get content
                HtmlTextInfo htmlTextInfo = null;
                string contentString = "";

                htmlTextInfo = objHTML.GetTopHtmlText(ModuleId, !IsEditable, WorkflowID);

                if ((htmlTextInfo != null))
                {
                    //don't decode yet (this is done in FormatHtmlText)
                    contentString = htmlTextInfo.Content;
                }
                else
                {
                    // get default content from resource file
                    if (PortalSettings.UserMode == PortalSettings.Mode.Edit)
                    {
                        if (EditorEnabled)
                        {
                            contentString = Localization.GetString("AddContentFromToolBar.Text", LocalResourceFile);
                        }
                        else
                        {
                            contentString = Localization.GetString("AddContentFromActionMenu.Text", LocalResourceFile);
                        }
                    }
                    else
                    {
                        // hide the module if no content and in view mode
                        ContainerControl.Visible = false;
                    }
                }

                // token replace
                EditorEnabled = EditorEnabled && !Settings.ReplaceTokens;

                // localize toolbar
                if (EditorEnabled)
                {
                    foreach (DNNToolBarButton button in editorDnnToobar.Buttons)
                    {
                        button.ToolTip = Localization.GetString(button.ToolTip + ".ToolTip", LocalResourceFile);
                    }
                }
                else
                {
                    editorDnnToobar.Visible = false;
                }

                lblContent.EditEnabled = EditorEnabled;

                // add content to module
                lblContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(ModuleId, contentString, Settings, PortalSettings, Page)));

                //set normalCheckBox on the content wrapper to prevent form decoration if its disabled.
                if (!Settings.UseDecorate)
                {
                    lblContent.CssClass = string.Format("{0} normalCheckBox", lblContent.CssClass);
                }

                if (IsPostBack && AJAX.IsEnabled() && AJAX.GetScriptManager(Page).IsInAsyncPostBack)
                {
                    var resetScript = $@"
if(typeof dnn !== 'undefined' && typeof dnn.controls !== 'undefined' && typeof dnn.controls.controls !== 'undefined'){{
    dnn.controls.controls['{lblContent.ClientID}'] = null;
}};";
                    ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), $"ResetHtmlModule{ClientID}", resetScript, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   lblContent_UpdateLabel allows for inline editing of content
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void lblContent_UpdateLabel(object source, DNNLabelEditEventArgs e)
        {
            try
            {
                // verify security 
                if ((!new PortalSecurity().InputFilter(e.Text, PortalSecurity.FilterFlag.NoScripting).Equals(e.Text)))
                {
                    throw new SecurityException();
                }
                else if (EditorEnabled && IsEditable && PortalSettings.UserMode == PortalSettings.Mode.Edit)
                {
                    // get content
                    var objHTML = new HtmlTextController();
                    var objWorkflow = new WorkflowStateController();
                    HtmlTextInfo objContent = objHTML.GetTopHtmlText(ModuleId, false, WorkflowID);
                    if (objContent == null)
                    {
                        objContent = new HtmlTextInfo();
                        objContent.ItemID = -1;
                    }

                    // set content attributes
                    objContent.ModuleID = ModuleId;
                    objContent.Content = Server.HtmlEncode(e.Text);
                    objContent.WorkflowID = WorkflowID;
                    objContent.StateID = objWorkflow.GetFirstWorkflowStateID(WorkflowID);

                    // save the content
                    objHTML.UpdateHtmlText(objContent, objHTML.GetMaximumVersionHistory(PortalId));
                }
                else
                {
                    throw new SecurityException();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   ModuleAction_Click handles all ModuleAction events raised from the action menu
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void ModuleAction_Click(object sender, ActionEventArgs e)
        {
            try
            {
                if (e.Action.CommandArgument == "publish")
                {
                    // verify security 
                    if (IsEditable && PortalSettings.UserMode == PortalSettings.Mode.Edit)
                    {
                        // get content
                        var objHTML = new HtmlTextController();
                        HtmlTextInfo objContent = objHTML.GetTopHtmlText(ModuleId, false, WorkflowID);

                        var objWorkflow = new WorkflowStateController();
                        if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(WorkflowID))
                        {
                            // publish content
                            objContent.StateID = objWorkflow.GetNextWorkflowStateID(objContent.WorkflowID, objContent.StateID);

                            // save the content
                            objHTML.UpdateHtmlText(objContent, objHTML.GetMaximumVersionHistory(PortalId));

                            // refresh page
                            Response.Redirect(Globals.NavigateURL(), true);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region "Optional Interfaces"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   ModuleActions is an interface property that returns the module actions collection for the module
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public ModuleActionCollection ModuleActions
        {
            get
            {
                // add the Edit Text action
                var Actions = new ModuleActionCollection();
                Actions.Add(GetNextActionID(),
                            Localization.GetString(ModuleActionType.AddContent, LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "",
                            EditUrl(),
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false);

                // get the content
                var objHTML = new HtmlTextController();
                var objWorkflow = new WorkflowStateController();
                WorkflowID = objHTML.GetWorkflow(ModuleId, TabId, PortalId).Value;

                HtmlTextInfo objContent = objHTML.GetTopHtmlText(ModuleId, false, WorkflowID);
                if ((objContent != null))
                {
                    // if content is in the first state
                    if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(WorkflowID))
                    {
                        // if not direct publish workflow
                        if (objWorkflow.GetWorkflowStates(WorkflowID).Count > 1)
                        {
                            // add publish action
                            Actions.Add(GetNextActionID(),
                                        Localization.GetString("PublishContent.Action", LocalResourceFile),
                                        ModuleActionType.AddContent,
                                        "publish",
                                        "grant.gif",
                                        "",
                                        true,
                                        SecurityAccessLevel.Edit,
                                        true,
                                        false);
                        }
                    }
                    else
                    {
                        // if the content is not in the last state of the workflow then review is required
                        if (objContent.StateID != objWorkflow.GetLastWorkflowStateID(WorkflowID))
                        {
                            // if the user has permissions to review the content
                            if (WorkflowStatePermissionController.HasWorkflowStatePermission(WorkflowStatePermissionController.GetWorkflowStatePermissions(objContent.StateID), "REVIEW"))
                            {
                                // add approve and reject actions
                                Actions.Add(GetNextActionID(),
                                            Localization.GetString("ApproveContent.Action", LocalResourceFile),
                                            ModuleActionType.AddContent,
                                            "",
                                            "grant.gif",
                                            EditUrl("action", "approve", "Review"),
                                            false,
                                            SecurityAccessLevel.Edit,
                                            true,
                                            false);
                                Actions.Add(GetNextActionID(),
                                            Localization.GetString("RejectContent.Action", LocalResourceFile),
                                            ModuleActionType.AddContent,
                                            "",
                                            "deny.gif",
                                            EditUrl("action", "reject", "Review"),
                                            false,
                                            SecurityAccessLevel.Edit,
                                            true,
                                            false);
                            }
                        }
                    }
                }

                // add mywork to action menu
                Actions.Add(GetNextActionID(),
                            Localization.GetString("MyWork.Action", LocalResourceFile),
                            "MyWork.Action",
                            "",
                            "view.gif",
                            EditUrl("MyWork"),
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false);

                return Actions;
            }
        }

        #endregion
    }
}