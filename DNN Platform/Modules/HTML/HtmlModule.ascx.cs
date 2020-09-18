// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlModule Class provides the UI for displaying the Html.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class HtmlModule : HtmlModuleBase, IActionable
    {
        private readonly INavigationManager _navigationManager;
        private bool EditorEnabled;
        private int WorkflowID;

        public HtmlModule()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets moduleActions is an interface property that returns the module actions collection for the module.
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
                Actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    string.Empty,
                    this.EditUrl(),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                // get the content
                var objHTML = new HtmlTextController();
                var objWorkflow = new WorkflowStateController();
                this.WorkflowID = objHTML.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;

                HtmlTextInfo objContent = objHTML.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
                if (objContent != null)
                {
                    // if content is in the first state
                    if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(this.WorkflowID))
                    {
                        // if not direct publish workflow
                        if (objWorkflow.GetWorkflowStates(this.WorkflowID).Count > 1)
                        {
                            // add publish action
                            Actions.Add(
                                this.GetNextActionID(),
                                Localization.GetString("PublishContent.Action", this.LocalResourceFile),
                                ModuleActionType.AddContent,
                                "publish",
                                "grant.gif",
                                string.Empty,
                                true,
                                SecurityAccessLevel.Edit,
                                true,
                                false);
                        }
                    }
                    else
                    {
                        // if the content is not in the last state of the workflow then review is required
                        if (objContent.StateID != objWorkflow.GetLastWorkflowStateID(this.WorkflowID))
                        {
                            // if the user has permissions to review the content
                            if (WorkflowStatePermissionController.HasWorkflowStatePermission(WorkflowStatePermissionController.GetWorkflowStatePermissions(objContent.StateID), "REVIEW"))
                            {
                                // add approve and reject actions
                                Actions.Add(
                                    this.GetNextActionID(),
                                    Localization.GetString("ApproveContent.Action", this.LocalResourceFile),
                                    ModuleActionType.AddContent,
                                    string.Empty,
                                    "grant.gif",
                                    this.EditUrl("action", "approve", "Review"),
                                    false,
                                    SecurityAccessLevel.Edit,
                                    true,
                                    false);
                                Actions.Add(
                                    this.GetNextActionID(),
                                    Localization.GetString("RejectContent.Action", this.LocalResourceFile),
                                    ModuleActionType.AddContent,
                                    string.Empty,
                                    "deny.gif",
                                    this.EditUrl("action", "reject", "Review"),
                                    false,
                                    SecurityAccessLevel.Edit,
                                    true,
                                    false);
                            }
                        }
                    }
                }

                // add mywork to action menu
                Actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString("MyWork.Action", this.LocalResourceFile),
                    "MyWork.Action",
                    string.Empty,
                    "view.gif",
                    this.EditUrl("MyWork"),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                return Actions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Page_Init runs when the control is initialized.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.lblContent.UpdateLabel += this.lblContent_UpdateLabel;
            this.EditorEnabled = this.PortalSettings.InlineEditorEnabled;
            try
            {
                this.WorkflowID = new HtmlTextController().GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;

                // Add an Action Event Handler to the Skin
                this.AddActionHandler(this.ModuleAction_Click);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Page_Load runs when the control is loaded.
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
                if (this.EditorEnabled && this.IsEditable && this.PortalSettings.UserMode == PortalSettings.Mode.Edit)
                {
                    this.EditorEnabled = true;
                }
                else
                {
                    this.EditorEnabled = false;
                }

                // get content
                HtmlTextInfo htmlTextInfo = null;
                string contentString = string.Empty;

                htmlTextInfo = objHTML.GetTopHtmlText(this.ModuleId, !this.IsEditable, this.WorkflowID);

                if (htmlTextInfo != null)
                {
                    // don't decode yet (this is done in FormatHtmlText)
                    contentString = htmlTextInfo.Content;
                }
                else
                {
                    // get default content from resource file
                    if (this.PortalSettings.UserMode == PortalSettings.Mode.Edit)
                    {
                        if (this.EditorEnabled)
                        {
                            contentString = Localization.GetString("AddContentFromToolBar.Text", this.LocalResourceFile);
                        }
                        else
                        {
                            contentString = Localization.GetString("AddContentFromActionMenu.Text", this.LocalResourceFile);
                        }
                    }
                    else
                    {
                        // hide the module if no content and in view mode
                        this.ContainerControl.Visible = false;
                    }
                }

                // token replace
                this.EditorEnabled = this.EditorEnabled && !this.Settings.ReplaceTokens;

                // localize toolbar
                if (this.EditorEnabled)
                {
                    foreach (DNNToolBarButton button in this.editorDnnToobar.Buttons)
                    {
                        button.ToolTip = Localization.GetString(button.ToolTip + ".ToolTip", this.LocalResourceFile);
                    }
                }
                else
                {
                    this.editorDnnToobar.Visible = false;
                }

                this.lblContent.EditEnabled = this.EditorEnabled;

                // add content to module
                this.lblContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(this.ModuleId, contentString, this.Settings, this.PortalSettings, this.Page)));

                // set normalCheckBox on the content wrapper to prevent form decoration if its disabled.
                if (!this.Settings.UseDecorate)
                {
                    this.lblContent.CssClass = string.Format("{0} normalCheckBox", this.lblContent.CssClass);
                }

                if (this.IsPostBack && AJAX.IsEnabled() && AJAX.GetScriptManager(this.Page).IsInAsyncPostBack)
                {
                    var resetScript = $@"
if(typeof dnn !== 'undefined' && typeof dnn.controls !== 'undefined' && typeof dnn.controls.controls !== 'undefined'){{
    var control = dnn.controls.controls['{this.lblContent.ClientID}'];
    if(control && control.container !== $get('{this.lblContent.ClientID}')){{
        dnn.controls.controls['{this.lblContent.ClientID}'] = null;
    }}
}};";
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), $"ResetHtmlModule{this.ClientID}", resetScript, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   lblContent_UpdateLabel allows for inline editing of content.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void lblContent_UpdateLabel(object source, DNNLabelEditEventArgs e)
        {
            try
            {
                // verify security
                if (!PortalSecurity.Instance.InputFilter(e.Text, PortalSecurity.FilterFlag.NoScripting).Equals(e.Text))
                {
                    throw new SecurityException();
                }
                else if (this.EditorEnabled && this.IsEditable && this.PortalSettings.UserMode == PortalSettings.Mode.Edit)
                {
                    // get content
                    var objHTML = new HtmlTextController();
                    var objWorkflow = new WorkflowStateController();
                    HtmlTextInfo objContent = objHTML.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
                    if (objContent == null)
                    {
                        objContent = new HtmlTextInfo();
                        objContent.ItemID = -1;
                    }

                    // set content attributes
                    objContent.ModuleID = this.ModuleId;
                    objContent.Content = this.Server.HtmlEncode(e.Text);
                    objContent.WorkflowID = this.WorkflowID;
                    objContent.StateID = objWorkflow.GetFirstWorkflowStateID(this.WorkflowID);

                    // save the content
                    objHTML.UpdateHtmlText(objContent, objHTML.GetMaximumVersionHistory(this.PortalId));
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
        ///   ModuleAction_Click handles all ModuleAction events raised from the action menu.
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
                    if (this.IsEditable && this.PortalSettings.UserMode == PortalSettings.Mode.Edit)
                    {
                        // get content
                        var objHTML = new HtmlTextController();
                        HtmlTextInfo objContent = objHTML.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);

                        var objWorkflow = new WorkflowStateController();
                        if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(this.WorkflowID))
                        {
                            // publish content
                            objContent.StateID = objWorkflow.GetNextWorkflowStateID(objContent.WorkflowID, objContent.StateID);

                            // save the content
                            objHTML.UpdateHtmlText(objContent, objHTML.GetMaximumVersionHistory(this.PortalId));

                            // refresh page
                            this.Response.Redirect(this._navigationManager.NavigateURL(), true);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
