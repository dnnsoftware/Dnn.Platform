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
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.LogViewer
{

    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : LogViewer
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Supplies the functionality for viewing the Site Logs
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [cnurse] 17/9/2004  Updated for localization, Help and 508. Also 
    ///                       consolidated Send Exceptions into one set of 
    ///                       controls
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class LogViewer : PortalModuleBase, ILogViewer
    {

        #region Private Members

        private int _pageIndex = 1;
        private Dictionary<string, LogTypeInfo> _logTypeDictionary;
        private int _portalId = -1;
        private string _logTypeKey;

        #endregion

        #region Private Methods

        private void BindLogTypeDropDown()
        {
            //ddlLogType.Items.Add(new ListItem(Localization.GetString("All"), "*"));
            ddlLogType.AddItem(Localization.GetString("All"), "*");
            var logController = new LogController();

            List<LogTypeConfigInfo> logTypes;
            if (String.IsNullOrEmpty(EventFilter))
            {
                logTypes = logController.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>()
                    .Where(l => l.LoggingIsActive)
                    .OrderBy(l => l.LogTypeFriendlyName).ToList();
            }
            else
            {
                logTypes = logController.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>()
                    .Where(l => l.LogTypeKey.StartsWith(EventFilter) && l.LoggingIsActive)
                    .OrderBy(l => l.LogTypeFriendlyName).ToList();
            }

            foreach (var logType in logTypes)
            {
                //ddlLogType.Items.Add(new ListItem(logType.LogTypeFriendlyName, logType.LogTypeKey));
                ddlLogType.AddItem(logType.LogTypeFriendlyName, logType.LogTypeKey);
            }
        }

        private void BindPortalDropDown()
        {
            if (UserInfo.IsSuperUser)
            {
                //ddlPortalid.Items.Add(new ListItem(Localization.GetString("All"), "-1"));
                ddlPortalid.AddItem(Localization.GetString("All"), "-1");
                var objPortalController = new PortalController();

                foreach (PortalInfo portal in objPortalController.GetPortals().Cast<PortalInfo>()
                                                .OrderBy(p => p.PortalName)
                                                .ToList())
                {
                    ddlPortalid.AddItem(portal.PortalName, portal.PortalID.ToString());
                }
				
                //check to see if any portalname is empty, otherwise set it to portalid
                for (var i = 0; i <= ddlPortalid.Items.Count - 1; i++)
                {
                    if (String.IsNullOrEmpty(ddlPortalid.Items[i].Text))
                    {
                        ddlPortalid.Items[i].Text = @"Portal: " + ddlPortalid.Items[i].Value;
                    }
                }
            }
            else
            {
                plPortalID.Visible = false;
                ddlPortalid.Visible = false;
            }
        }

        private void DeleteSelectedExceptions()
        {
            try
            {
                var s = Request.Form["Exception"];
                if (s != null)
                {
                    string[] positions = null;
                    if (s.LastIndexOf(",") > 0)
                    {
                        positions = s.Split(Convert.ToChar(","));
                    }
                    else if (!String.IsNullOrEmpty(s))
                    {
                        positions = new[] {s};
                    }
                    var objLoggingController = new LogController();
                    if (positions != null)
                    {
                        var j = positions.Length;
                        for (var i = 0; i < positions.Length; i++)
                        {
                            j -= 1;
                            var excKey = positions[j].Split(Convert.ToChar("|"));
                            var objLogInfo = new LogInfo {LogGUID = excKey[0], LogFileID = excKey[1]};
                            objLoggingController.DeleteLog(objLogInfo);
                        }
                    }
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DeleteSuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
                BindPortalDropDown();
                BindData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private XmlDocument GetSelectedExceptions()
        {
            var objXML = new XmlDocument();
            try
            {
                var s = Request.Form["Exception"];
                if (s != null)
                {
                    string[] excPositions = null;
                    if (s.LastIndexOf(",") > 0)
                    {
                        excPositions = s.Split(Convert.ToChar(","));
                    }
                    else if (!String.IsNullOrEmpty(s))
                    {
                        excPositions = new[] {s};
                    }
                    var objLoggingController = new LogController();
                    objXML.LoadXml("<LogEntries></LogEntries>");
                    if (excPositions != null)
                    {
                        var j = excPositions.Length;
                        for (var i = 0; i < excPositions.Length; i++)
                        {
                            j -= 1;
                            var excKey = excPositions[j].Split(Convert.ToChar("|"));
                            var objLogInfo = new LogInfo {LogGUID = excKey[0], LogFileID = excKey[1]};
                            var objNode = objXML.ImportNode((XmlNode) objLoggingController.GetSingleLog(objLogInfo, LoggingProvider.ReturnType.XML), true);
                            if (objXML.DocumentElement != null)
                            {
                                objXML.DocumentElement.AppendChild(objNode);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return objXML;
        }

        private void InitializePaging(PagingControl ctlPagingControl, int totalRecords, int pageSize)
        {
            ctlPagingControl.TotalRecords = totalRecords;
            ctlPagingControl.PageSize = pageSize;
            ctlPagingControl.CurrentPage = _pageIndex;
            
            var querystring = "";
            if (ddlRecordsPerPage.SelectedIndex != 0)
            {
                querystring += "&PageRecords=" + ddlRecordsPerPage.SelectedValue;
            }
            if (_portalId >= 0)
            {
                querystring += "&pid=" + _portalId;
            }
            if (_logTypeKey != "*" && !String.IsNullOrEmpty(_logTypeKey))
            {
                querystring += "&LogTypeKey=" + _logTypeKey;
            }

            ctlPagingControl.QuerystringParams = querystring;
            ctlPagingControl.TabID = TabId;
            ctlPagingControl.Visible = true;
        }

        private void SendEmail()
        {
            var strFromEmailAddress = !String.IsNullOrEmpty(UserInfo.Email) ? UserInfo.Email : PortalSettings.Email;

            if (string.IsNullOrEmpty(txtSubject.Text))
            {
                txtSubject.Text = PortalSettings.PortalName + @" Exceptions";
            }

            string returnMsg;
            if (Regex.IsMatch(strFromEmailAddress, Globals.glbEmailRegEx))
            {
                const string tempFileName = "errorlog.xml";
                var filePath = PortalSettings.HomeDirectoryMapPath + tempFileName;
                var xmlDoc = GetSelectedExceptions();
                xmlDoc.Save(filePath);

                var attachments = new List<Attachment>();
                var ct = new ContentType {MediaType = MediaTypeNames.Text.Xml, Name = tempFileName};

                using(var attachment = new Attachment(filePath, ct))
                {
                    attachments.Add(attachment);

                    returnMsg = Mail.SendEmail(strFromEmailAddress, strFromEmailAddress, txtEmailAddress.Text, txtSubject.Text, txtMessage.Text, attachments);
                }

                FileSystemUtils.DeleteFile(filePath);

                if (String.IsNullOrEmpty(returnMsg))
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("EmailSuccess", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("EmailFailure", LocalResourceFile) + " " + returnMsg, ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                returnMsg = string.Format(Localization.GetString("InavlidEmailAddress", LocalResourceFile), strFromEmailAddress);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("EmailFailure", LocalResourceFile) + " " + returnMsg, ModuleMessage.ModuleMessageType.RedError);
            }
            BindData();
        }

        public string GetPropertiesText(object obj)
        {
            var objLogInfo = (LogInfo)obj;

            var objLogProperties = objLogInfo.LogProperties;
            var str = new StringBuilder();
            int i;
            for (i = 0; i <= objLogProperties.Count - 1; i++)
            {
				//display the values in the Panel child controls.
                var ldi = (LogDetailInfo)objLogProperties[i];
                if (ldi.PropertyName == "Message")
                {
                    str.Append("<p><strong>" + ldi.PropertyName + "</strong>:</br><pre>" + Server.HtmlEncode(ldi.PropertyValue) + "</pre></p>");
                }
                else
                {
                    str.Append("<p><strong>" + ldi.PropertyName + "</strong>:" + Server.HtmlEncode(ldi.PropertyValue) + "</p>");
                }
            }
            str.Append("<p>" + Localization.GetString("ServerName", LocalResourceFile) + Server.HtmlEncode(objLogInfo.LogServerName) + "</p>");
            return str.ToString();
        }

        #endregion

        protected LogTypeInfo GetMyLogType(string logTypeKey)
        {
            LogTypeInfo logType;
            _logTypeDictionary.TryGetValue(logTypeKey, out logType);

// ReSharper disable ConvertIfStatementToNullCoalescingExpression
            if (logType == null)
// ReSharper restore ConvertIfStatementToNullCoalescingExpression
            {
                logType = new LogTypeInfo();
            }
            return logType;
        }

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();

            btnClear.Click += BtnClearClick;
            btnDelete.Click += BtnDeleteClick;
            btnEmail.Click += BtnEmailClick;
            ddlLogType.SelectedIndexChanged += DdlLogTypeSelectedIndexChanged;
            ddlPortalid.SelectedIndexChanged += DdlPortalIDSelectedIndexChanged;
            ddlRecordsPerPage.SelectedIndexChanged += DdlRecordsPerPageSelectedIndexChanged;

            if (Request.QueryString["CurrentPage"] != null)
            {
                _pageIndex = Convert.ToInt32(Request.QueryString["CurrentPage"]);
            }

            var logController = new LogController();
            _logTypeDictionary = logController.GetLogTypeInfoDictionary();
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load runs when the page loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 17/9/2004  Updated for localization, Help and 508
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
				// If this is the first visit to the page, populate the site data
                if (Page.IsPostBack == false)
                {
                    var logController = new LogController();
                    logController.PurgeLogBuffer();
                    if (Request.QueryString["PageRecords"] != null)
                    {
                        ddlRecordsPerPage.SelectedValue = Request.QueryString["PageRecords"];
                    }
                    BindPortalDropDown();
                    BindLogTypeDropDown();
                    BindData();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BtnClearClick(Object sender, EventArgs e)
        {
            var objLoggingController = new LogController();
            objLoggingController.ClearLog();
            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("LogCleared", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            var objEventLog = new EventLogController();
			
            //add entry to log recording it was cleared
            objEventLog.AddLog(Localization.GetString("LogCleared", LocalResourceFile),
                               Localization.GetString("Username", LocalResourceFile) + ":" + UserInfo.Username,
                               PortalSettings,
                               -1,
                               EventLogController.EventLogType.ADMIN_ALERT);
            BindPortalDropDown();
            dlLog.Visible = false;
            btnDelete.Visible = false;
            btnClear.Visible = false;
            ctlPagingControlBottom.Visible = false;
        }

        private void BtnDeleteClick(Object sender, EventArgs e)
        {
            DeleteSelectedExceptions();
        }

        private void BtnEmailClick(Object sender, EventArgs e)
        {
            SendEmail();
        }

        private void DdlLogTypeSelectedIndexChanged(Object sender, EventArgs e)
        {
            _pageIndex = 1;
            BindData();
        }

        private void DdlPortalIDSelectedIndexChanged(Object sender, EventArgs e)
        {
            _pageIndex = 1;
            BindData();
        }

        private void DdlRecordsPerPageSelectedIndexChanged(Object sender, EventArgs e)
        {
            _pageIndex = 1;
            BindData();
        }

        #endregion

        #region ILogViewer Members

        public void BindData()
        {
            if (UserInfo.IsSuperUser)
            {
                if (!Page.IsPostBack && Request.QueryString["pid"] != null)
                {
                    ddlPortalid.FindItemByValue(Request.QueryString["pid"]).Selected = true;
                }
                _portalId = Int32.Parse(ddlPortalid.SelectedItem.Value);
            }
            else
            {
                _portalId = PortalId;
            }

            var totalRecords = 0;
            var pageSize = Convert.ToInt32(ddlRecordsPerPage.SelectedValue);
            if (!Page.IsPostBack && Request.QueryString["LogTypeKey"] != null)
            {
                var li = ddlLogType.FindItemByValue(Request.QueryString["LogTypeKey"]);
                if (li != null)
                {
                    li.Selected = true;
                }
            }

            _logTypeKey = ddlLogType.SelectedItem.Value;

            var currentPage = _pageIndex;
            if (currentPage > 0)
            {
                currentPage = currentPage - 1;
            }

            var logController = new LogController();
            var filteredKey = _logTypeKey;
            if (_logTypeKey == "*" && !String.IsNullOrEmpty(EventFilter))
            {
                filteredKey = EventFilter + "%";
            }
            var logs = logController.GetLogs(_portalId, filteredKey == "*" ? String.Empty : filteredKey, pageSize, currentPage, ref totalRecords);

            if (logs.Count > 0)
            {
                dlLog.Visible = true;
                btnDelete.Visible = UserInfo.IsSuperUser;
                btnClear.Visible = UserInfo.IsSuperUser;
                dlLog.DataSource = logs;
                dlLog.DataBind();
                InitializePaging(ctlPagingControlBottom, totalRecords, pageSize);
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoEntries", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                dlLog.Visible = false;
                btnDelete.Visible = false;
                btnClear.Visible = false;

                ctlPagingControlBottom.Visible = false;
            }

			editSettings.Visible = UserInfo.IsSuperUser;
        }

        public string EventFilter { get; set; }

        #endregion

    }
}