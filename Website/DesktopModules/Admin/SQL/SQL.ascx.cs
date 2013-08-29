#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        ///     [VMasanas]  9/28/2004   Changed redirect to Access Denied
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CheckSecurity();

            cmdExecute.Click += OnExecuteClick;
            cmdUpload.Click += OnUploadClick;
            ResultsRepeater.ItemDataBound += ResultsRepeater_ItemDataBound;

            try
            {
                if (!UserInfo.IsSuperUser)
                {
                    Response.Redirect(Globals.NavigateURL("Access Denied"), true);
                }
                if (!Page.IsPostBack)
                {
                    ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
                    foreach (ConnectionStringSettings connection in connections)
                    {
                        if (connection.Name.ToLower() != "localmysqlserver" && connection.Name.ToLower() != "localsqlserver")
                        {
                            cboConnection.AddItem(connection.Name, connection.Name);
                        }
                    }
                    cboConnection.SelectedIndex = 0;
                    cmdExecute.ToolTip = Localization.GetString("cmdExecute.ToolTip", LocalResourceFile);
                    chkRunAsScript.ToolTip = Localization.GetString("chkRunAsScript.ToolTip", LocalResourceFile);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ResultsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType != ListItemType.AlternatingItem && e.Item.ItemType != ListItemType.Item)
                {
                    return;
                }

                var gvResults = (DnnGrid)e.Item.FindControl("gvResults");
                gvResults.PreRender += this.gvResults_PreRender;
                gvResults.DataSource = e.Item.DataItem;
                gvResults.DataBind();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        void gvResults_PreRender(object sender, EventArgs e)
        {
            ((DnnGrid)sender).ClientSettings.Scrolling.AllowScroll = false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdExecute_Click runs when the Execute button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnExecuteClick(object sender, EventArgs e)
        {
            try
            {

                if (!String.IsNullOrEmpty(txtQuery.Text))
                {
                    var connectionstring = Config.GetConnectionString(cboConnection.SelectedValue);
                    if (chkRunAsScript.Checked)
                    {
                        var strError = DataProvider.Instance().ExecuteScript(connectionstring, txtQuery.Text);
                        if (strError == Null.NullString)
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QuerySuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                        }
                        else
                        {
                            UI.Skins.Skin.AddModuleMessage(this, strError, ModuleMessage.ModuleMessageType.RedError);
                        }
                    }
                    else
                    {
                        try
                        {
                            var dr = DataProvider.Instance().ExecuteSQLTemp(connectionstring, txtQuery.Text);
                            if (dr != null)
                            {
                                var tables = new List<DataTable>();
                                do
                                {
                                    var table = new DataTable { Locale = CultureInfo.CurrentCulture };
                                    table.Load(dr);
                                    tables.Add(table);
                                }
                                while (!dr.IsClosed); // table.Load automatically moves to the next result and closes the reader once there are no more

                                ResultsRepeater.DataSource = tables;
                                ResultsRepeater.DataBind();

                                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QuerySuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                            }
                            else
                            {

                                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("QueryError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            }
                        }
                        catch (SqlException sqlException)
                        {
                            UI.Skins.Skin.AddModuleMessage(this, sqlException.Message, ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                    }
                    RecordAuditEventLog(txtQuery.Text);
                }
            }
            catch (Exception exc) //Module failed to load
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

        #endregion

    }
}