﻿// Licensed to the .NET Foundation under one or more agreements.
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
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>  The HtmlModule Class provides the UI for displaying the Html.</summary>
    public partial class HtmlModule : HtmlModuleBase, IActionable
    {
        private readonly INavigationManager navigationManager;
        private bool editorEnabled;
        private int workflowID;

        /// <summary>Initializes a new instance of the <see cref="HtmlModule"/> class.</summary>
        public HtmlModule()
        {
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>  Gets moduleActions is an interface property that returns the module actions collection for the module.</summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                // add the Edit Text action
                var actions = new ModuleActionCollection();
                actions.Add(
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
                var objHTML = new HtmlTextController(this.navigationManager);
                var objWorkflow = new WorkflowStateController();
                this.workflowID = objHTML.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;

                HtmlTextInfo objContent = objHTML.GetTopHtmlText(this.ModuleId, false, this.workflowID);
                if (objContent != null)
                {
                    // if content is in the first state
                    if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(this.workflowID))
                    {
                        // if not direct publish workflow
                        if (objWorkflow.GetWorkflowStates(this.workflowID).Count > 1)
                        {
                            // add publish action
                            actions.Add(
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
                        if (objContent.StateID != objWorkflow.GetLastWorkflowStateID(this.workflowID))
                        {
                            // if the user has permissions to review the content
                            if (WorkflowStatePermissionController.HasWorkflowStatePermission(WorkflowStatePermissionController.GetWorkflowStatePermissions(objContent.StateID), "REVIEW"))
                            {
                                // add approve and reject actions
                                actions.Add(
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
                                actions.Add(
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
                actions.Add(
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

                return actions;
            }
        }

        /// <summary>  Page_Init runs when the control is initialized.</summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.lblContent.UpdateLabel += this.LblContent_UpdateLabel;
            this.editorEnabled = this.PortalSettings.InlineEditorEnabled;
            try
            {
                this.workflowID = new HtmlTextController(this.navigationManager).GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;

                // Add an Action Event Handler to the Skin
                this.AddActionHandler(this.ModuleAction_Click);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>  Page_Load runs when the control is loaded.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                var objHTML = new HtmlTextController(this.navigationManager);

                // edit in place
                if (this.editorEnabled && this.IsEditable && Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                {
                    this.editorEnabled = true;
                }
                else
                {
                    this.editorEnabled = false;
                }

                // get content
                HtmlTextInfo htmlTextInfo = null;
                string contentString = string.Empty;

                htmlTextInfo = objHTML.GetTopHtmlText(this.ModuleId, !this.IsEditable, this.workflowID);

                if (htmlTextInfo != null)
                {
                    // don't decode yet (this is done in FormatHtmlText)
                    contentString = htmlTextInfo.Content;
                }
                else
                {
                    // get default content from resource file
                    if (Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                    {
                        if (this.editorEnabled)
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
                this.editorEnabled = this.editorEnabled && !this.Settings.ReplaceTokens;

                // localize toolbar
                if (this.editorEnabled)
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

                this.lblContent.EditEnabled = this.editorEnabled;

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

        /// <summary>  lblContent_UpdateLabel allows for inline editing of content.</summary>
        private void LblContent_UpdateLabel(object source, DNNLabelEditEventArgs e)
        {
            try
            {
                // verify security
                if (!PortalSecurity.Instance.InputFilter(e.Text, PortalSecurity.FilterFlag.NoScripting).Equals(e.Text))
                {
                    throw new SecurityException();
                }
                else if (this.editorEnabled && this.IsEditable && Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                {
                    // get content
                    var objHTML = new HtmlTextController(this.navigationManager);
                    var objWorkflow = new WorkflowStateController();
                    HtmlTextInfo objContent = objHTML.GetTopHtmlText(this.ModuleId, false, this.workflowID);
                    if (objContent == null)
                    {
                        objContent = new HtmlTextInfo();
                        objContent.ItemID = -1;
                    }

                    // set content attributes
                    objContent.ModuleID = this.ModuleId;
                    objContent.Content = this.Server.HtmlEncode(e.Text);
                    objContent.WorkflowID = this.workflowID;
                    objContent.StateID = objWorkflow.GetFirstWorkflowStateID(this.workflowID);

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

        /// <summary>  ModuleAction_Click handles all ModuleAction events raised from the action menu.</summary>
        private void ModuleAction_Click(object sender, ActionEventArgs e)
        {
            try
            {
                if (e.Action.CommandArgument == "publish")
                {
                    // verify security
                    if (this.IsEditable && Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                    {
                        // get content
                        var objHTML = new HtmlTextController(this.navigationManager);
                        HtmlTextInfo objContent = objHTML.GetTopHtmlText(this.ModuleId, false, this.workflowID);

                        var objWorkflow = new WorkflowStateController();
                        if (objContent.StateID == objWorkflow.GetFirstWorkflowStateID(this.workflowID))
                        {
                            // publish content
                            objContent.StateID = objWorkflow.GetNextWorkflowStateID(objContent.WorkflowID, objContent.StateID);

                            // save the content
                            objHTML.UpdateHtmlText(objContent, objHTML.GetMaximumVersionHistory(this.PortalId));

                            // refresh page
                            this.Response.Redirect(this.navigationManager.NavigateURL(), true);
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
