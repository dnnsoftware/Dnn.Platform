#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Configuration;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.UI.WebControls;
using System.Web.UI.WebControls;
using System.Data;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;
using DotNetNuke.Modules.SQL.Components;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using System.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.SQL
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SQL PortalModuleBase is used run SQL Scripts on the Database
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    // ReSharper disable InconsistentNaming
    public partial class SQL : PortalModuleBase
    // ReSharper restore InconsistentNaming
    {

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CheckSecurity();
            LoadPlugins();

            cmdExecute.Click += OnExecuteClick;
            cmdUpload.Click += OnUploadClick;
            lnkDelete.Click += lnkDelete_Click;
            lnkSave.Click += lnkSave_Click;
            rptResults.ItemDataBound += rptResults_ItemDataBound;
            ddlSavedQuery.SelectedIndexChanged += ddlSavedQuery_SelectedIndexChanged;

            try
            {
                if (!Page.IsPostBack)
                {
                    LoadConnectionStrings();
                    LoadSavedQueries();

                    cmdExecute.ToolTip = Localization.GetString("cmdExecute.ToolTip", LocalResourceFile);
                    DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(lnkDelete, LocalizeString("DeleteItem"));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnExecuteClick(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid)
                {
                    return;
                }

                var connectionstring = Config.GetConnectionString(ddlConnection.SelectedValue);

                if (RunAsScript())
                {
                    var strError = DataProvider.Instance().ExecuteScript(connectionstring, txtQuery.Text);
                    if (strError == Null.NullString)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QuerySuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QueryError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        errorRow.Visible = true;
                        txtError.Text = strError;
                    }
                }
                else
                {
                    try
                    {
                        string errorMessage;
                        var dr = DataProvider.Instance().ExecuteSQLTemp(connectionstring, txtQuery.Text, out errorMessage);
                        if (dr != null)
                        {
                            var tables = new List<DataTable>();
                            string tabs = "";
                            int numTabs = 1;
                            do
                            {
                                var table = new DataTable { Locale = CultureInfo.CurrentCulture };
                                table.Load(dr);
                                tables.Add(table);
                                tabs += string.Format("<li><a href='#result_{0}'>{1} {0}</a></li>", numTabs.ToString(), LocalizeString("ResultTitle"));
                                numTabs++;
                            }
                            while (!dr.IsClosed); // table.Load automatically moves to the next result and closes the reader once there are no more

                            plTabs.Controls.Add(new LiteralControl(tabs));
                            rptResults.DataSource = tables;
                            rptResults.DataBind();

                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QuerySuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                        }
                        else
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QueryError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            errorRow.Visible = true;
                            txtError.Text = errorMessage;
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QueryError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        errorRow.Visible = true;
                        txtError.Text = sqlException.Message;
                        return;
                    }
                }
                RecordAuditEventLog(txtQuery.Text);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void rptResults_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
                {
                    var gvResults = (GridView)e.Item.FindControl("grdResults");
                    gvResults.DataSource = e.Item.DataItem;
                    gvResults.DataBind();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ddlSavedQuery_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlSavedQuery.SelectedValue == "")
            {
                txtQuery.Text = "";
                lnkDelete.Visible = false;
                ddlConnection.SelectedValue = "SiteSqlServer";
            }
            else
            {
                SqlQueryController ctl = new SqlQueryController();
                SqlQuery query = ctl.GetQuery(int.Parse(ddlSavedQuery.SelectedValue));
                if (query != null)
                {
                    txtQuery.Text = query.Query;
                    lnkDelete.Visible = true;
                    if (ddlConnection.Items.FindByValue(query.ConnectionStringName) != null)
                    {
                        ddlConnection.SelectedValue = query.ConnectionStringName;
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, string.Format(LocalizeString("CnnStringNotFound"), query.ConnectionStringName), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                }
            }
        }

        private void lnkSave_Click(object sender, EventArgs e)
        {
            try
            {
                SqlQueryController ctl = new SqlQueryController();
                SqlQuery query = ctl.GetQuery(txtName.Text);
                if (query == null)
                {
                    query = new SqlQuery();
                    query.CreatedByUserId = UserId;
                    query.CreatedOnDate = DateTime.Now;
                }
                query.Name = txtName.Text;
                query.Query = txtQuery.Text;
                query.ConnectionStringName = ddlConnection.SelectedValue;
                query.LastModifiedByUserId = UserId;
                query.LastModifiedOnDate = DateTime.Now;

                if (query.QueryId == 0)
                {
                    ctl.AddQuery(query);
                }
                else
                {
                    ctl.UpdateQuery(query);
                }
                LoadSavedQueries();
                ddlSavedQuery.SelectedValue = query.QueryId.ToString();
                lnkDelete.Visible = true;

                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, LocalizeString("Saved"), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void lnkDelete_Click(object sender, EventArgs e)
        {
            try
            {
                SqlQueryController ctl = new SqlQueryController();
                SqlQuery query = ctl.GetQuery(int.Parse(ddlSavedQuery.SelectedValue));
                if (query != null)
                {
                    ctl.DeleteQuery(query);
                    LoadSavedQueries();
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, LocalizeString("Deleted"), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

        }

        protected void OnUploadClick(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                if (!String.IsNullOrEmpty(uplSqlScript.PostedFile.FileName))
                {
                    var scriptFile = new StreamReader(uplSqlScript.PostedFile.InputStream);
                    txtQuery.Text = scriptFile.ReadToEnd();
                }
            }
        }

        #endregion

        #region Private Methods

        private void RecordAuditEventLog(string query)
        {
            var props = new LogProperties { new LogDetailInfo("User", UserInfo.Username), new LogDetailInfo("SQL Query", query) };

            var elc = new EventLogController();
            elc.AddLog(props, PortalSettings, UserId, EventLogController.EventLogType.HOST_SQL_EXECUTED.ToString(), true);
        }

        private void CheckSecurity()
        {
            if (!UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        private void LoadConnectionStrings()
        {
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings connection in connections)
            {
                if (connection.Name.ToLower() != "localmysqlserver" && connection.Name.ToLower() != "localsqlserver")
                {
                    ddlConnection.Items.Add(new ListItem(connection.Name, connection.Name));
                }
            }
            ddlConnection.SelectedIndex = 0;
        }

        private void LoadSavedQueries()
        {
            SqlQueryController ctl = new SqlQueryController();
            ddlSavedQuery.DataSource = ctl.GetQueries();
            ddlSavedQuery.DataBind();

            ddlSavedQuery.Items.Insert(0, new ListItem(LocalizeString("NewQuery"), ""));
            lnkDelete.Visible = false;

        }

        private bool RunAsScript()
        {
            string _scriptDelimiterRegex = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";
            Regex objRegex = new Regex(_scriptDelimiterRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return objRegex.IsMatch(txtQuery.Text);
        }

        private void LoadPlugins()
        {
            Page.ClientScript.RegisterClientScriptInclude("clipboard", this.TemplateSourceDirectory + "/plugins/clipboard/jquery.zclip.min.js");
        }

        protected string GetClipboardPath()
        {
            return this.TemplateSourceDirectory + "/plugins/clipboard/zeroclipboard.swf";
        }
        #endregion

    }
}