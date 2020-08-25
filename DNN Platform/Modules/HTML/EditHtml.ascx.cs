// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;
    using Telerik.Web.UI;

    /// <summary>
    ///   The EditHtml PortalModuleBase is used to manage Html.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class EditHtml : HtmlModuleBase
    {
        private readonly INavigationManager _navigationManager;

        private readonly HtmlTextController _htmlTextController = new HtmlTextController();
        private readonly HtmlTextLogController _htmlTextLogController = new HtmlTextLogController();
        private readonly WorkflowStateController _workflowStateController = new WorkflowStateController();

        public EditHtml()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private enum WorkflowType
        {
            DirectPublish = 1,
            ContentStaging = 2,
        }

        protected string CurrentView
        {
            get
            {
                if (this.phEdit.Visible)
                {
                    return "EditView";
                }
                else if (this.phPreview.Visible)
                {
                    return "PreviewView";
                }

                if (this.phHistory.Visible)
                {
                    return "HistoryView";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private int WorkflowID
        {
            get
            {
                int workflowID;

                if (this.ViewState["WorkflowID"] == null)
                {
                    workflowID = this._htmlTextController.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;
                    this.ViewState.Add("WorkflowID", workflowID);
                }
                else
                {
                    workflowID = int.Parse(this.ViewState["WorkflowID"].ToString());
                }

                return workflowID;
            }
        }

        private string TempContent
        {
            get
            {
                var content = string.Empty;
                if (this.ViewState["TempContent"] != null)
                {
                    content = this.ViewState["TempContent"].ToString();
                }

                return content;
            }

            set
            {
                this.ViewState["TempContent"] = value;
            }
        }

        private WorkflowType CurrentWorkflowType
        {
            get
            {
                var currentWorkflowType = default(WorkflowType);
                if (this.ViewState["_currentWorkflowType"] != null)
                {
                    currentWorkflowType = (WorkflowType)Enum.Parse(typeof(WorkflowType), this.ViewState["_currentWorkflowType"].ToString());
                }

                return currentWorkflowType;
            }

            set
            {
                this.ViewState["_currentWorkflowType"] = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.hlCancel.NavigateUrl = this._navigationManager.NavigateURL();

            this.cmdEdit.Click += this.OnEditClick;
            this.cmdPreview.Click += this.OnPreviewClick;
            this.cmdHistory.Click += this.OnHistoryClick;
            this.cmdMasterContent.Click += this.OnMasterContentClick;
            this.ddlRender.SelectedIndexChanged += this.OnRenderSelectedIndexChanged;
            this.cmdSave.Click += this.OnSaveClick;
            this.dgHistory.RowDataBound += this.OnHistoryGridItemDataBound;
            this.dgVersions.RowCommand += this.OnVersionsGridItemCommand;
            this.dgVersions.RowDataBound += this.OnVersionsGridItemDataBound;
            this.dgVersions.PageIndexChanged += this.OnVersionsGridPageIndexChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                var htmlContentItemID = -1;
                var htmlContent = this._htmlTextController.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);

                if (htmlContent != null)
                {
                    htmlContentItemID = htmlContent.ItemID;
                }

                if (!this.Page.IsPostBack)
                {
                    var workflowStates = this._workflowStateController.GetWorkflowStates(this.WorkflowID);
                    var maxVersions = this._htmlTextController.GetMaximumVersionHistory(this.PortalId);
                    var userCanEdit = this.UserInfo.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);

                    this.lblMaxVersions.Text = maxVersions.ToString();
                    this.dgVersions.PageSize = Math.Min(Math.Max(maxVersions, 5), 10); // min 5, max 10

                    switch (workflowStates.Count)
                    {
                        case 1:
                            this.CurrentWorkflowType = WorkflowType.DirectPublish;
                            break;
                        case 2:
                            this.CurrentWorkflowType = WorkflowType.ContentStaging;
                            break;
                    }

                    if (htmlContentItemID != -1)
                    {
                        this.DisplayContent(htmlContent);

                        // DisplayPreview(htmlContent);
                        this.DisplayHistory(htmlContent);
                    }
                    else
                    {
                        this.DisplayInitialContent(workflowStates[0] as WorkflowStateInfo);
                    }

                    this.divPublish.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;
                    this.phCurrentVersion.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;
                    this.phPreviewVersion.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;

                    // DisplayVersions();
                    this.BindRenderItems();
                    this.ddlRender.SelectedValue = this.txtContent.Mode;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnSaveClick(object sender, EventArgs e)
        {
            const bool redirect = true;

            try
            {
                // get content
                var htmlContent = this.GetLatestHTMLContent();

                var aliases = from PortalAliasInfo pa in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalSettings.PortalId)
                              select pa.HTTPAlias;
                string content;
                if (this.phEdit.Visible)
                {
                    content = this.txtContent.Text;
                }
                else
                {
                    content = this.hfEditor.Value;
                }

                if (this.Request.QueryString["nuru"] == null)
                {
                    content = HtmlUtils.AbsoluteToRelativeUrls(content, aliases);
                }

                htmlContent.Content = content;

                var draftStateID = this._workflowStateController.GetFirstWorkflowStateID(this.WorkflowID);
                var publishedStateID = this._workflowStateController.GetLastWorkflowStateID(this.WorkflowID);

                switch (this.CurrentWorkflowType)
                {
                    case WorkflowType.DirectPublish:
                        this._htmlTextController.UpdateHtmlText(htmlContent, this._htmlTextController.GetMaximumVersionHistory(this.PortalId));

                        break;
                    case WorkflowType.ContentStaging:
                        if (this.chkPublish.Checked)
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

                        this._htmlTextController.UpdateHtmlText(htmlContent, this._htmlTextController.GetMaximumVersionHistory(this.PortalId));
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(this.Page, "Error occurred: ", exc.Message, ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // redirect back to portal
            if (redirect)
            {
                this.Response.Redirect(this._navigationManager.NavigateURL(), true);
            }
        }

        protected void OnEditClick(object sender, EventArgs e)
        {
            try
            {
                this.DisplayEdit(this.hfEditor.Value);

                if (this.phMasterContent.Visible)
                {
                    this.DisplayMasterLanguageContent();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnPreviewClick(object sender, EventArgs e)
        {
            try
            {
                if (this.phEdit.Visible)
                {
                    this.hfEditor.Value = this.txtContent.Text;
                }

                this.DisplayPreview(this.phEdit.Visible ? this.txtContent.Text : this.hfEditor.Value);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnHistoryGridItemDataBound(object sender, GridViewRowEventArgs e)
        {
            var item = e.Row;

            if (item.RowType == DataControlRowType.DataRow)
            {
                // Localize columns
                item.Cells[2].Text = Localization.GetString(item.Cells[2].Text, this.LocalResourceFile);
                item.Cells[3].Text = Localization.GetString(item.Cells[3].Text, this.LocalResourceFile);
            }
        }

        protected void OnVersionsGridItemCommand(object source, GridViewCommandEventArgs e)
        {
            try
            {
                HtmlTextInfo htmlContent;

                // disable delete button if user doesn't have delete rights???
                switch (e.CommandName.ToLowerInvariant())
                {
                    case "remove":
                        htmlContent = this.GetHTMLContent(e);
                        this._htmlTextController.DeleteHtmlText(this.ModuleId, htmlContent.ItemID);
                        break;
                    case "rollback":
                        htmlContent = this.GetHTMLContent(e);
                        htmlContent.ItemID = -1;
                        htmlContent.ModuleID = this.ModuleId;
                        htmlContent.WorkflowID = this.WorkflowID;
                        htmlContent.StateID = this._workflowStateController.GetFirstWorkflowStateID(this.WorkflowID);
                        this._htmlTextController.UpdateHtmlText(htmlContent, this._htmlTextController.GetMaximumVersionHistory(this.PortalId));
                        break;
                    case "preview":
                        htmlContent = this.GetHTMLContent(e);
                        this.DisplayPreview(htmlContent);
                        break;
                }

                if (e.CommandName.ToLowerInvariant() != "preview")
                {
                    var latestContent = this._htmlTextController.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
                    if (latestContent == null)
                    {
                        this.DisplayInitialContent(this._workflowStateController.GetWorkflowStates(this.WorkflowID)[0] as WorkflowStateInfo);
                    }
                    else
                    {
                        this.DisplayContent(latestContent);

                        // DisplayPreview(latestContent);
                        // DisplayVersions();
                    }
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnVersionsGridItemDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var htmlContent = e.Row.DataItem as HtmlTextInfo;
                var createdBy = "Default";

                if (htmlContent.CreatedByUserID != -1)
                {
                    var createdByByUser = UserController.GetUserById(this.PortalId, htmlContent.CreatedByUserID);
                    if (createdByByUser != null)
                    {
                        createdBy = createdByByUser.DisplayName;
                    }
                }

                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is ImageButton)
                        {
                            var imageButton = cellControl as ImageButton;
                            imageButton.CommandArgument = htmlContent.ItemID.ToString();
                            switch (imageButton.CommandName.ToLowerInvariant())
                            {
                                case "rollback":
                                    // hide rollback for the first item
                                    if (this.dgVersions.CurrentPageIndex == 0)
                                    {
                                        if (e.Row.RowIndex == 0)
                                        {
                                            imageButton.Visible = false;
                                            break;
                                        }
                                    }

                                    imageButton.Visible = true;

                                    break;
                                case "remove":
                                    var msg = this.GetLocalizedString("DeleteVersion.Confirm");
                                    msg =
                                        msg.Replace("[VERSION]", htmlContent.Version.ToString()).Replace("[STATE]", htmlContent.StateName).Replace("[DATECREATED]", htmlContent.CreatedOnDate.ToString())
                                            .Replace("[USERNAME]", createdBy);
                                    imageButton.OnClientClick = "return confirm(\"" + msg + "\");";

                                    // hide the delete button
                                    var showDelete = this.UserInfo.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);

                                    if (!showDelete)
                                    {
                                        showDelete = htmlContent.IsPublished == false;
                                    }

                                    imageButton.Visible = showDelete;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        protected void OnVersionsGridPageIndexChanged(object source, EventArgs e)
        {
            this.DisplayVersions();
        }

        /// <summary>
        ///   Displays the history of an html content item in a grid in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayHistory(HtmlTextInfo htmlContent)
        {
            this.dnnSitePanelEditHTMLHistory.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;
            this.fsEditHtmlHistory.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;

            if (this.CurrentWorkflowType == WorkflowType.DirectPublish)
            {
                return;
            }

            var htmlLogging = this._htmlTextLogController.GetHtmlTextLog(htmlContent.ItemID);
            this.dgHistory.DataSource = htmlLogging;
            this.dgHistory.DataBind();

            this.dnnSitePanelEditHTMLHistory.Visible = htmlLogging.Count != 0;
            this.fsEditHtmlHistory.Visible = htmlLogging.Count != 0;
        }

        /// <summary>
        ///   Displays the versions of the html content in the versions section.
        /// </summary>
        private void DisplayVersions()
        {
            var versions = this._htmlTextController.GetAllHtmlText(this.ModuleId);

            foreach (var item in versions)
            {
                item.StateName = this.GetLocalizedString(item.StateName);
            }

            this.dgVersions.DataSource = versions;
            this.dgVersions.DataBind();

            this.phEdit.Visible = false;
            this.phPreview.Visible = false;
            this.phHistory.Visible = true;
            this.cmdEdit.Enabled = true;
            this.cmdPreview.Enabled = true;
            this.cmdHistory.Enabled = false;
            this.cmdMasterContent.Visible = false;
            this.ddlRender.Visible = false;
        }

        /// <summary>
        ///   Displays the content of the master language if localized content is enabled.
        /// </summary>
        private void DisplayMasterLanguageContent()
        {
            // Get master language
            var objModule = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
            if (objModule.DefaultLanguageModule != null)
            {
                var masterContent = this._htmlTextController.GetTopHtmlText(objModule.DefaultLanguageModule.ModuleID, false, this.WorkflowID);
                if (masterContent != null)
                {
                    this.placeMasterContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(objModule.DefaultLanguageModule.ModuleID, this.FormatContent(masterContent.Content), this.Settings, this.PortalSettings, this.Page)));
                }
            }
        }

        /// <summary>
        ///   Displays the html content in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayContent(HtmlTextInfo htmlContent)
        {
            this.lblCurrentWorkflowInUse.Text = this.GetLocalizedString(htmlContent.WorkflowName);
            this.lblCurrentWorkflowState.Text = this.GetLocalizedString(htmlContent.StateName);
            this.lblCurrentVersion.Text = htmlContent.Version.ToString();
            this.txtContent.Text = this.FormatContent(htmlContent.Content);
            this.phEdit.Visible = true;
            this.phPreview.Visible = false;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = false;
            this.cmdPreview.Enabled = true;
            this.cmdHistory.Enabled = true;

            // DisplayMasterLanguageContent();
            this.DisplayMasterContentButton();
            this.ddlRender.Visible = true;
        }

        private void DisplayMasterContentButton()
        {
            var objModule = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
            if (objModule.DefaultLanguageModule != null)
            {
                this.cmdMasterContent.Visible = true;
                this.cmdMasterContent.Text = Localization.GetString("cmdShowMasterContent", this.LocalResourceFile);

                this.cmdMasterContent.Text = this.phMasterContent.Visible ?
                    Localization.GetString("cmdHideMasterContent", this.LocalResourceFile) :
                    Localization.GetString("cmdShowMasterContent", this.LocalResourceFile);
            }
        }

        /// <summary>
        ///   Displays the content preview in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayPreview(HtmlTextInfo htmlContent)
        {
            this.lblPreviewVersion.Text = htmlContent.Version.ToString();
            this.lblPreviewWorkflowInUse.Text = this.GetLocalizedString(htmlContent.WorkflowName);
            this.lblPreviewWorkflowState.Text = this.GetLocalizedString(htmlContent.StateName);
            this.litPreview.Text = HtmlTextController.FormatHtmlText(this.ModuleId, htmlContent.Content, this.Settings, this.PortalSettings, this.Page);
            this.phEdit.Visible = false;
            this.phPreview.Visible = true;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = true;
            this.cmdPreview.Enabled = false;
            this.cmdHistory.Enabled = true;
            this.DisplayHistory(htmlContent);
            this.cmdMasterContent.Visible = false;
            this.ddlRender.Visible = false;
        }

        /// <summary>
        ///   Displays the preview in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayPreview(string htmlContent)
        {
            this.litPreview.Text = HtmlTextController.FormatHtmlText(this.ModuleId, htmlContent, this.Settings, this.PortalSettings, this.Page);
            this.divPreviewVersion.Visible = false;
            this.divPreviewWorlflow.Visible = false;

            this.divPreviewWorkflowState.Visible = true;
            this.lblPreviewWorkflowState.Text = this.GetLocalizedString("EditPreviewState");

            this.phEdit.Visible = false;
            this.phPreview.Visible = true;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = true;
            this.cmdPreview.Enabled = false;
            this.cmdHistory.Enabled = true;
            this.cmdMasterContent.Visible = false;
            this.ddlRender.Visible = false;
        }

        private void DisplayEdit(string htmlContent)
        {
            this.txtContent.Text = htmlContent;
            this.phEdit.Visible = true;
            this.phPreview.Visible = false;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = false;
            this.cmdPreview.Enabled = true;
            this.cmdHistory.Enabled = true;
            this.DisplayMasterContentButton();
            this.ddlRender.Visible = true;
        }

        /// <summary>
        ///   Displays the content but hide the editor if editing is locked from the current user.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <param name = "lastPublishedContent">Last content of the published.</param>
        private void DisplayLockedContent(HtmlTextInfo htmlContent, HtmlTextInfo lastPublishedContent)
        {
            this.txtContent.Visible = false;
            this.cmdSave.Visible = false;

            // cmdPreview.Enabled = false;
            this.divPublish.Visible = false;

            this.divSubmittedContent.Visible = true;

            this.lblCurrentWorkflowInUse.Text = this.GetLocalizedString(htmlContent.WorkflowName);
            this.lblCurrentWorkflowState.Text = this.GetLocalizedString(htmlContent.StateName);

            this.litCurrentContentPreview.Text = HtmlTextController.FormatHtmlText(this.ModuleId, htmlContent.Content, this.Settings, this.PortalSettings, this.Page);
            this.lblCurrentVersion.Text = htmlContent.Version.ToString();
            this.DisplayVersions();

            if (lastPublishedContent != null)
            {
                this.DisplayPreview(lastPublishedContent);

                // DisplayHistory(lastPublishedContent);
            }
            else
            {
                this.dnnSitePanelEditHTMLHistory.Visible = false;
                this.fsEditHtmlHistory.Visible = false;
                this.DisplayPreview(htmlContent.Content);
            }
        }

        /// <summary>
        ///   Displays the initial content when a module is first added to the page.
        /// </summary>
        /// <param name = "firstState">The first state.</param>
        private void DisplayInitialContent(WorkflowStateInfo firstState)
        {
            this.cmdHistory.Enabled = false;

            this.txtContent.Text = this.GetLocalizedString("AddContent");
            this.litPreview.Text = this.GetLocalizedString("AddContent");
            this.lblCurrentWorkflowInUse.Text = firstState.WorkflowName;
            this.lblPreviewWorkflowInUse.Text = firstState.WorkflowName;
            this.divPreviewVersion.Visible = false;

            this.dnnSitePanelEditHTMLHistory.Visible = false;
            this.fsEditHtmlHistory.Visible = false;

            this.divCurrentWorkflowState.Visible = false;
            this.phCurrentVersion.Visible = false;
            this.divPreviewWorkflowState.Visible = false;

            this.lblPreviewWorkflowState.Text = firstState.StateName;
        }

        /// <summary>
        ///   Formats the content to make it html safe.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private string FormatContent(string htmlContent)
        {
            var strContent = HttpUtility.HtmlDecode(htmlContent);
            strContent = HtmlTextController.ManageRelativePaths(strContent, this.PortalSettings.HomeDirectory, "src", this.PortalId);
            strContent = HtmlTextController.ManageRelativePaths(strContent, this.PortalSettings.HomeDirectory, "background", this.PortalId);
            return HttpUtility.HtmlEncode(strContent);
        }

        /// <summary>
        ///   Gets the localized string from a resource file if it exists.
        /// </summary>
        /// <param name = "str">The STR.</param>
        /// <returns></returns>
        private string GetLocalizedString(string str)
        {
            var localizedString = Localization.GetString(str, this.LocalResourceFile);
            return string.IsNullOrEmpty(localizedString) ? str : localizedString;
        }

        /// <summary>
        ///   Gets the latest html content of the module.
        /// </summary>
        /// <returns></returns>
        private HtmlTextInfo GetLatestHTMLContent()
        {
            var htmlContent = this._htmlTextController.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
            if (htmlContent == null)
            {
                htmlContent = new HtmlTextInfo();
                htmlContent.ItemID = -1;
                htmlContent.StateID = this._workflowStateController.GetFirstWorkflowStateID(this.WorkflowID);
                htmlContent.WorkflowID = this.WorkflowID;
                htmlContent.ModuleID = this.ModuleId;
            }

            return htmlContent;
        }

        /// <summary>
        ///   Returns whether or not the user has review permissions to this module.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private bool UserCanReview(HtmlTextInfo htmlContent)
        {
            return (htmlContent != null) && WorkflowStatePermissionController.HasWorkflowStatePermission(WorkflowStatePermissionController.GetWorkflowStatePermissions(htmlContent.StateID), "REVIEW");
        }

        /// <summary>
        ///   Gets the last published version of this module.
        /// </summary>
        /// <param name = "publishedStateID">The published state ID.</param>
        /// <returns></returns>
        private HtmlTextInfo GetLastPublishedVersion(int publishedStateID)
        {
            return (from version in this._htmlTextController.GetAllHtmlText(this.ModuleId) where version.StateID == publishedStateID orderby version.Version descending select version).ToList()[0];
        }

        private void OnRenderSelectedIndexChanged(object sender, EventArgs e)
        {
            this.txtContent.ChangeMode(this.ddlRender.SelectedValue);
        }

        private void OnHistoryClick(object sender, EventArgs e)
        {
            try
            {
                if (this.phEdit.Visible)
                {
                    this.hfEditor.Value = this.txtContent.Text;
                }

                this.DisplayVersions();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void OnMasterContentClick(object sender, EventArgs e)
        {
            try
            {
                this.phMasterContent.Visible = !this.phMasterContent.Visible;
                this.cmdMasterContent.Text = this.phMasterContent.Visible ?
                    Localization.GetString("cmdHideMasterContent", this.LocalResourceFile) :
                    Localization.GetString("cmdShowMasterContent", this.LocalResourceFile);

                if (this.phMasterContent.Visible)
                {
                    this.DisplayMasterLanguageContent();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private HtmlTextInfo GetHTMLContent(GridViewCommandEventArgs e)
        {
            return this._htmlTextController.GetHtmlText(this.ModuleId, int.Parse(e.CommandArgument.ToString()));
        }

        private void BindRenderItems()
        {
            if (this.txtContent.IsRichEditorAvailable)
            {
                this.ddlRender.Items.Add(new ListItem(this.LocalizeString("liRichText"), "RICH"));
            }

            this.ddlRender.Items.Add(new ListItem(this.LocalizeString("liBasicText"), "BASIC"));
        }
    }
}
