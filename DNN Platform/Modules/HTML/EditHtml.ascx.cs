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
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Html.Components;

#endregion

namespace DotNetNuke.Modules.Html
{

    /// <summary>
    ///   The EditHtml PortalModuleBase is used to manage Html
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class EditHtml : HtmlModuleBase
    {

        #region Private Members

        private readonly HtmlTextController _htmlTextController = new HtmlTextController();
        private readonly HtmlTextLogController _htmlTextLogController = new HtmlTextLogController();
        private readonly WorkflowStateController _workflowStateController = new WorkflowStateController();

        #endregion

        #region Nested type: WorkflowType

        private enum WorkflowType
        {
            DirectPublish = 1,
            ContentStaging = 2
        }

        #endregion

        #region Private Properties

        private int WorkflowID
        {
            get
            {
                int workflowID;

                if (ViewState["WorkflowID"] == null)
                {
                    workflowID = _htmlTextController.GetWorkflow(ModuleId, TabId, PortalId).Value;
                    ViewState.Add("WorkflowID", workflowID);
                }
                else
                {
                    workflowID = int.Parse(ViewState["WorkflowID"].ToString());
                }

                return workflowID;
            }
        }

        private string TempContent
        {
            get
            {
                var content = "";
                if ((ViewState["TempContent"] != null))
                {
                    content = ViewState["TempContent"].ToString();
                }
                return content;
            }
            set
            {
                ViewState["TempContent"] = value;
            }
        }

        private WorkflowType CurrentWorkflowType
        {
            get
            {
                var currentWorkflowType = default(WorkflowType);
                if (ViewState["_currentWorkflowType"] != null)
                {
                    currentWorkflowType = (WorkflowType) Enum.Parse(typeof (WorkflowType), ViewState["_currentWorkflowType"].ToString());
                }

                return currentWorkflowType;
            }
            set
            {
                ViewState["_currentWorkflowType"] = value;
            }
        }

        protected string CurrentView
        {
            get
            {
                if (phEdit.Visible)
                    return "EditView";
                else if (phPreview.Visible)
                    return "PreviewView";
                if (phHistory.Visible)
                    return "HistoryView";
                else
                    return "";
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Displays the history of an html content item in a grid in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayHistory(HtmlTextInfo htmlContent)
        {
            dnnSitePanelEditHTMLHistory.Visible = CurrentWorkflowType != WorkflowType.DirectPublish;
            fsEditHtmlHistory.Visible = CurrentWorkflowType != WorkflowType.DirectPublish;

            if (((CurrentWorkflowType == WorkflowType.DirectPublish)))
            {
                return;
            }
            var htmlLogging = _htmlTextLogController.GetHtmlTextLog(htmlContent.ItemID);
            dgHistory.DataSource = htmlLogging;
            dgHistory.DataBind();

            dnnSitePanelEditHTMLHistory.Visible = htmlLogging.Count != 0;
            fsEditHtmlHistory.Visible = htmlLogging.Count != 0;
        }

        /// <summary>
        ///   Displays the versions of the html content in the versions section
        /// </summary>
        private void DisplayVersions()
        {
            var versions = _htmlTextController.GetAllHtmlText(ModuleId);

            foreach (var item in versions)
            {
                item.StateName = GetLocalizedString(item.StateName);
            }
            dgVersions.DataSource = versions;
            dgVersions.DataBind();

            phEdit.Visible = false;
            phPreview.Visible = false;
            phHistory.Visible = true;
            cmdEdit.Enabled = true;
            cmdPreview.Enabled = true;
            cmdHistory.Enabled = false;
            cmdMasterContent.Visible = false;
            ddlRender.Visible = false;
        }

        /// <summary>
        ///   Displays the content of the master language if localized content is enabled.
        /// </summary>
        private void DisplayMasterLanguageContent()
        {
            //Get master language
            var objModule = ModuleController.Instance.GetModule(ModuleId, TabId, false);
            if (objModule.DefaultLanguageModule != null)
            {
                var masterContent = _htmlTextController.GetTopHtmlText(objModule.DefaultLanguageModule.ModuleID, false, WorkflowID);
                if (masterContent != null)
                {
                    placeMasterContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(objModule.DefaultLanguageModule.ModuleID, FormatContent(masterContent.Content), Settings, PortalSettings, Page)));
                }
            }
        }

        /// <summary>
        ///   Displays the html content in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayContent(HtmlTextInfo htmlContent)
        {
            lblCurrentWorkflowInUse.Text = GetLocalizedString(htmlContent.WorkflowName);
            lblCurrentWorkflowState.Text = GetLocalizedString(htmlContent.StateName);
            lblCurrentVersion.Text = htmlContent.Version.ToString();
            txtContent.Text = FormatContent(htmlContent.Content);
            phEdit.Visible = true;
            phPreview.Visible = false;
            phHistory.Visible = false;
            cmdEdit.Enabled = false;
            cmdPreview.Enabled = true;
            cmdHistory.Enabled = true;
            //DisplayMasterLanguageContent();
            DisplayMasterContentButton();
            ddlRender.Visible = true;
        }

        private void DisplayMasterContentButton()
        {
            var objModule = ModuleController.Instance.GetModule(ModuleId, TabId, false);
            if (objModule.DefaultLanguageModule != null)
            {
                cmdMasterContent.Visible = true;
                cmdMasterContent.Text = Localization.GetString("cmdShowMasterContent", LocalResourceFile);
            
                cmdMasterContent.Text = phMasterContent.Visible ?
                    Localization.GetString("cmdHideMasterContent", LocalResourceFile) :
                    Localization.GetString("cmdShowMasterContent", LocalResourceFile);
            
            }
        }

        /// <summary>
        ///   Displays the content preview in the preview section
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayPreview(HtmlTextInfo htmlContent)
        {
            lblPreviewVersion.Text = htmlContent.Version.ToString();
            lblPreviewWorkflowInUse.Text = GetLocalizedString(htmlContent.WorkflowName);
            lblPreviewWorkflowState.Text = GetLocalizedString(htmlContent.StateName);
            litPreview.Text = HtmlTextController.FormatHtmlText(ModuleId, htmlContent.Content, Settings, PortalSettings, Page);
            phEdit.Visible = false;
            phPreview.Visible = true;
            phHistory.Visible = false;
            cmdEdit.Enabled = true;
            cmdPreview.Enabled = false;
            cmdHistory.Enabled = true;
            DisplayHistory(htmlContent);
            cmdMasterContent.Visible = false;
            ddlRender.Visible = false;
        }

        /// <summary>
        ///   Displays the preview in the preview section
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayPreview(string htmlContent)
        {
            litPreview.Text = HtmlTextController.FormatHtmlText(ModuleId, htmlContent, Settings, PortalSettings, Page);
            divPreviewVersion.Visible = false;
            divPreviewWorlflow.Visible = false;

            divPreviewWorkflowState.Visible = true;
            lblPreviewWorkflowState.Text = GetLocalizedString("EditPreviewState");

            phEdit.Visible = false;
            phPreview.Visible = true;
            phHistory.Visible = false;
            cmdEdit.Enabled = true;
            cmdPreview.Enabled = false;
            cmdHistory.Enabled = true;
            cmdMasterContent.Visible = false;
            ddlRender.Visible = false;
        }

        private void DisplayEdit(string htmlContent)
        {
            txtContent.Text = htmlContent;
            phEdit.Visible = true;
            phPreview.Visible = false;
            phHistory.Visible = false;
            cmdEdit.Enabled = false;
            cmdPreview.Enabled = true;
            cmdHistory.Enabled = true;
            DisplayMasterContentButton();
            ddlRender.Visible = true;
                
        }

        /// <summary>
        ///   Displays the content but hide the editor if editing is locked from the current user
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <param name = "lastPublishedContent">Last content of the published.</param>
        private void DisplayLockedContent(HtmlTextInfo htmlContent, HtmlTextInfo lastPublishedContent)
        {
            txtContent.Visible = false;
            cmdSave.Visible = false;
            //cmdPreview.Enabled = false;
            divPublish.Visible = false;

            divSubmittedContent.Visible = true;

            lblCurrentWorkflowInUse.Text = GetLocalizedString(htmlContent.WorkflowName);
            lblCurrentWorkflowState.Text = GetLocalizedString(htmlContent.StateName);

            litCurrentContentPreview.Text = HtmlTextController.FormatHtmlText(ModuleId, htmlContent.Content, Settings, PortalSettings, Page);
            lblCurrentVersion.Text = htmlContent.Version.ToString();
            DisplayVersions();

            if ((lastPublishedContent != null))
            {
                DisplayPreview(lastPublishedContent);
                //DisplayHistory(lastPublishedContent);
            }
            else
            {
                dnnSitePanelEditHTMLHistory.Visible = false;
                fsEditHtmlHistory.Visible = false;
                DisplayPreview(htmlContent.Content);
            }
        }

        /// <summary>
        ///   Displays the initial content when a module is first added to the page.
        /// </summary>
        /// <param name = "firstState">The first state.</param>
        private void DisplayInitialContent(WorkflowStateInfo firstState)
        {
            cmdHistory.Enabled = false;

            txtContent.Text = GetLocalizedString("AddContent");
            litPreview.Text = GetLocalizedString("AddContent");
            lblCurrentWorkflowInUse.Text = firstState.WorkflowName;
            lblPreviewWorkflowInUse.Text = firstState.WorkflowName;
            divPreviewVersion.Visible = false;

            dnnSitePanelEditHTMLHistory.Visible = false;
            fsEditHtmlHistory.Visible = false;

            divCurrentWorkflowState.Visible = false;
            phCurrentVersion.Visible = false;
            divPreviewWorkflowState.Visible = false;

            lblPreviewWorkflowState.Text = firstState.StateName;
        }

        #endregion

        #region Private Functions

        /// <summary>
        ///   Formats the content to make it html safe.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private string FormatContent(string htmlContent)
        {
            var strContent = HttpUtility.HtmlDecode(htmlContent);
            strContent = HtmlTextController.ManageRelativePaths(strContent, PortalSettings.HomeDirectory, "src", PortalId);
            strContent = HtmlTextController.ManageRelativePaths(strContent, PortalSettings.HomeDirectory, "background", PortalId);
            return HttpUtility.HtmlEncode(strContent);
        }

        /// <summary>
        ///   Gets the localized string from a resource file if it exists.
        /// </summary>
        /// <param name = "str">The STR.</param>
        /// <returns></returns>
        private string GetLocalizedString(string str)
        {
            var localizedString = Localization.GetString(str, LocalResourceFile);
            return (string.IsNullOrEmpty(localizedString) ? str : localizedString);
        }

        /// <summary>
        ///   Gets the latest html content of the module
        /// </summary>
        /// <returns></returns>
        private HtmlTextInfo GetLatestHTMLContent()
        {
            var htmlContent = _htmlTextController.GetTopHtmlText(ModuleId, false, WorkflowID);
            if (htmlContent == null)
            {
                htmlContent = new HtmlTextInfo();
                htmlContent.ItemID = -1;
                htmlContent.StateID = _workflowStateController.GetFirstWorkflowStateID(WorkflowID);
                htmlContent.WorkflowID = WorkflowID;
                htmlContent.ModuleID = ModuleId;
            }

            return htmlContent;
        }

        /// <summary>
        ///   Returns whether or not the user has review permissions to this module
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private bool UserCanReview(HtmlTextInfo htmlContent)
        {
            return (htmlContent != null) && WorkflowStatePermissionController.HasWorkflowStatePermission(WorkflowStatePermissionController.GetWorkflowStatePermissions(htmlContent.StateID), "REVIEW");
        }

        /// <summary>
        ///   Gets the last published version of this module
        /// </summary>
        /// <param name = "publishedStateID">The published state ID.</param>
        /// <returns></returns>
        private HtmlTextInfo GetLastPublishedVersion(int publishedStateID)
        {
            return (from version in _htmlTextController.GetAllHtmlText(ModuleId) where version.StateID == publishedStateID orderby version.Version descending select version).ToList()[0];
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            hlCancel.NavigateUrl = Globals.NavigateURL();

            cmdEdit.Click += OnEditClick;
            cmdPreview.Click += OnPreviewClick;
            cmdHistory.Click += OnHistoryClick;
            cmdMasterContent.Click += OnMasterContentClick;
            ddlRender.SelectedIndexChanged += OnRenderSelectedIndexChanged;
            cmdSave.Click += OnSaveClick;
            dgHistory.RowDataBound += OnHistoryGridItemDataBound;
            dgVersions.RowCommand += OnVersionsGridItemCommand;
            dgVersions.RowDataBound += OnVersionsGridItemDataBound;
            dgVersions.PageIndexChanged += OnVersionsGridPageIndexChanged;
        }

        private void OnRenderSelectedIndexChanged(object sender, EventArgs e)
        {
            txtContent.ChangeMode(ddlRender.SelectedValue);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {                
                var htmlContentItemID = -1;
                var htmlContent = _htmlTextController.GetTopHtmlText(ModuleId, false, WorkflowID);

                if ((htmlContent != null))
                {
                    htmlContentItemID = htmlContent.ItemID;
                }

                if (!Page.IsPostBack)
                {
                    var workflowStates = _workflowStateController.GetWorkflowStates(WorkflowID);
                    var maxVersions = _htmlTextController.GetMaximumVersionHistory(PortalId);
                    var userCanEdit = UserInfo.IsSuperUser || PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);

                    lblMaxVersions.Text = maxVersions.ToString();
                    dgVersions.PageSize = Math.Min(Math.Max(maxVersions, 5), 10); //min 5, max 10

                    switch (workflowStates.Count)
                    {
                        case 1:
                            CurrentWorkflowType = WorkflowType.DirectPublish;
                            break;
                        case 2:
                            CurrentWorkflowType = WorkflowType.ContentStaging;
                            break;
                    }

                    if (htmlContentItemID != -1)
                    {
                        DisplayContent(htmlContent);
                        //DisplayPreview(htmlContent);
                        DisplayHistory(htmlContent);
                    }
                    else
                    {
                        DisplayInitialContent(workflowStates[0] as WorkflowStateInfo);
                    }

                    divPublish.Visible = CurrentWorkflowType != WorkflowType.DirectPublish;
                    phCurrentVersion.Visible = CurrentWorkflowType != WorkflowType.DirectPublish;
                    phPreviewVersion.Visible = CurrentWorkflowType != WorkflowType.DirectPublish;
                    //DisplayVersions();

                    BindRenderItems();
                    ddlRender.SelectedValue = txtContent.Mode;
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
                var htmlContent = GetLatestHTMLContent();

                var aliases = from PortalAliasInfo pa in PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalSettings.PortalId) 
                              select pa.HTTPAlias;
                string content;
                if (phEdit.Visible)
                    content = txtContent.Text;
                else
                    content = hfEditor.Value;


                if (Request.QueryString["nuru"] == null)
                {
                    content = HtmlUtils.AbsoluteToRelativeUrls(content, aliases);
                }
                htmlContent.Content = content;

                var draftStateID = _workflowStateController.GetFirstWorkflowStateID(WorkflowID);
                var publishedStateID = _workflowStateController.GetLastWorkflowStateID(WorkflowID);

                switch (CurrentWorkflowType)
                {
                    case WorkflowType.DirectPublish:
                        _htmlTextController.UpdateHtmlText(htmlContent, _htmlTextController.GetMaximumVersionHistory(PortalId));

                        break;
                    case WorkflowType.ContentStaging:
                        if (chkPublish.Checked)
                        {
                            //if it's already published set it to draft
                            if (htmlContent.StateID == publishedStateID)
                            {
                                htmlContent.StateID = draftStateID;
                            }
                            else
                            {
                                htmlContent.StateID = publishedStateID;
                                //here it's in published mode
                            }
                        }
                        else
                        {
                            //if it's already published set it back to draft
                            if ((htmlContent.StateID != draftStateID))
                            {
                                htmlContent.StateID = draftStateID;
                            }
                        }

                        _htmlTextController.UpdateHtmlText(htmlContent, _htmlTextController.GetMaximumVersionHistory(PortalId));
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(Page, "Error occurred: ", exc.Message, ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // redirect back to portal
            if (redirect)
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
        }
        protected void OnEditClick(object sender, EventArgs e)
        {
            try
            {
                DisplayEdit(hfEditor.Value);

                if (phMasterContent.Visible)
                    DisplayMasterLanguageContent();
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
                if (phEdit.Visible)
                    hfEditor.Value = txtContent.Text;
                DisplayPreview(phEdit.Visible ? txtContent.Text : hfEditor.Value);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        private void OnHistoryClick(object sender, EventArgs e)
        {
            try
            {
                if (phEdit.Visible)
                    hfEditor.Value = txtContent.Text;
                DisplayVersions();
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
                phMasterContent.Visible = !phMasterContent.Visible;
                cmdMasterContent.Text = phMasterContent.Visible ?
                    Localization.GetString("cmdHideMasterContent", LocalResourceFile) :
                    Localization.GetString("cmdShowMasterContent", LocalResourceFile);
                
                if (phMasterContent.Visible)
                    DisplayMasterLanguageContent();
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
                //Localize columns
                item.Cells[2].Text = Localization.GetString(item.Cells[2].Text, LocalResourceFile);
                item.Cells[3].Text = Localization.GetString(item.Cells[3].Text, LocalResourceFile);
            }
        }

        protected void OnVersionsGridItemCommand(object source, GridViewCommandEventArgs e)
        {
            try
            {
                HtmlTextInfo htmlContent;

                //disable delete button if user doesn't have delete rights???
                switch (e.CommandName.ToLower())
                {
                    case "remove":
                        htmlContent = GetHTMLContent(e);
                        _htmlTextController.DeleteHtmlText(ModuleId, htmlContent.ItemID);
                        break;
                    case "rollback":
                        htmlContent = GetHTMLContent(e);
                        htmlContent.ItemID = -1;
                        htmlContent.ModuleID = ModuleId;
                        htmlContent.WorkflowID = WorkflowID;
                        htmlContent.StateID = _workflowStateController.GetFirstWorkflowStateID(WorkflowID);
                        _htmlTextController.UpdateHtmlText(htmlContent, _htmlTextController.GetMaximumVersionHistory(PortalId));
                        break;
                    case "preview":
                        htmlContent = GetHTMLContent(e);
                        DisplayPreview(htmlContent);
                        break;
                }

                if ((e.CommandName.ToLower() != "preview"))
                {
                    var latestContent = _htmlTextController.GetTopHtmlText(ModuleId, false, WorkflowID);
                    if (latestContent == null)
                    {
                        DisplayInitialContent(_workflowStateController.GetWorkflowStates(WorkflowID)[0] as WorkflowStateInfo);
                    }
                    else
                    {
                        DisplayContent(latestContent);
                        //DisplayPreview(latestContent);
                        //DisplayVersions();
                    }
                }

                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private HtmlTextInfo GetHTMLContent(GridViewCommandEventArgs e)
        {
            return _htmlTextController.GetHtmlText(ModuleId, int.Parse(e.CommandArgument.ToString()));
        }

        protected void OnVersionsGridItemDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var item = e.Row as GridViewRow;
                var htmlContent = item.DataItem as HtmlTextInfo;
                var createdBy = "Default";

                if ((htmlContent.CreatedByUserID != -1))
                {
                    var createdByByUser = UserController.GetUserById(PortalId, htmlContent.CreatedByUserID);
                    if (createdByByUser != null)
                    {
                        createdBy = createdByByUser.DisplayName;
                    }                    
                }

                foreach (TableCell cell in item.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is ImageButton)
                        {
                            var imageButton = cellControl as ImageButton;
                            imageButton.CommandArgument = htmlContent.ItemID.ToString();
                            switch (imageButton.CommandName.ToLower())
                            {
                                case "rollback":
                                    //hide rollback for the first item
                                    if (dgVersions.PageIndex == 0)
                                    {
                                        if ((item.RowIndex == 0))
                                        {
                                            imageButton.Visible = false;
                                            break;
                                        }
                                    }

                                    imageButton.Visible = true;

                                    break;
                                case "remove":
                                    var msg = GetLocalizedString("DeleteVersion.Confirm");
                                    msg =
                                        msg.Replace("[VERSION]", htmlContent.Version.ToString()).Replace("[STATE]", htmlContent.StateName).Replace("[DATECREATED]", htmlContent.CreatedOnDate.ToString())
                                            .Replace("[USERNAME]", createdBy);
                                    imageButton.OnClientClick = "return confirm(\"" + msg + "\");";
                                    //hide the delete button
                                    var showDelete = UserInfo.IsSuperUser || PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);

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
            DisplayVersions();
        }

        private void BindRenderItems()
        {
            if (txtContent.IsRichEditorAvailable)
            {
                ddlRender.Items.Add(new ListItem(LocalizeString("liRichText"), "RICH"));
            }

            ddlRender.Items.Add(new ListItem(LocalizeString("liBasicText"), "BASIC"));
        }

        #endregion

    }
}