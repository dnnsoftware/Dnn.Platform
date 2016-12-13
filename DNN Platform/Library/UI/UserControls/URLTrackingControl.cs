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
using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

using Calendar = DotNetNuke.Common.Utilities.Calendar;

#endregion

namespace DotNetNuke.UI.UserControls
{
    public abstract class URLTrackingControl : UserControlBase
    {
		#region "Private Members"
		
        protected Label Label1;
        protected Label Label2;
        protected Label Label3;
        protected Label Label4;
        protected Label Label5;
        protected Label Label6;
        protected Label Label7;
        private string _FormattedURL = "";
        private int _ModuleID = -2;
        private string _TrackingURL = "";
        private string _URL = "";
        private string _localResourceFile;
        protected LinkButton cmdDisplay;
        protected HyperLink cmdEndCalendar;
        protected HyperLink cmdStartCalendar;
        protected DataGrid grdLog;
        protected Label lblClicks;
        protected Label lblCreatedDate;
        protected Label lblLastClick;
        protected Label lblLogURL;
        protected Label lblTrackingURL;
        protected Label lblURL;
        protected Panel pnlLog;
        protected Panel pnlTrack;
        protected TextBox txtEndDate;
        protected TextBox txtStartDate;
        protected CompareValidator valEndDate;
        protected CompareValidator valStartDate;
		
		#endregion

		#region "Public Properties"

        public string FormattedURL
        {
            get
            {
                return _FormattedURL;
            }
            set
            {
                _FormattedURL = value;
            }
        }

        public string TrackingURL
        {
            get
            {
                return _TrackingURL;
            }
            set
            {
                _TrackingURL = value;
            }
        }

        public string URL
        {
            get
            {
                return _URL;
            }
            set
            {
                _URL = value;
            }
        }

        public int ModuleID
        {
            get
            {
                int moduleID = _ModuleID;
                if (moduleID == -2)
                {
                    if (Request.QueryString["mid"] != null)
                    {
                        Int32.TryParse(Request.QueryString["mid"], out moduleID);
                    }
                }
                return moduleID;
            }
            set
            {
                _ModuleID = value;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (String.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/URLTrackingControl.ascx";
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                return fileRoot;
            }
            set
            {
                _localResourceFile = value;
            }
        }
		
		#endregion

		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdDisplay.Click += cmdDisplay_Click;

            try
            {
				//this needs to execute always to the client script code is registred in InvokePopupCal
                cmdStartCalendar.NavigateUrl = Calendar.InvokePopupCal(txtStartDate);
                cmdEndCalendar.NavigateUrl = Calendar.InvokePopupCal(txtEndDate);
                if (!Page.IsPostBack)
                {
                    if (!String.IsNullOrEmpty(_URL))
                    {
                        lblLogURL.Text = URL; //saved for loading Log grid
                        TabType URLType = Globals.GetURLType(_URL);
                        if (URLType == TabType.File && _URL.ToLower().StartsWith("fileid=") == false)
                        {
                            //to handle legacy scenarios before the introduction of the FileServerHandler
                            var fileName = Path.GetFileName(_URL);

                            var folderPath = _URL.Substring(0, _URL.LastIndexOf(fileName));
                            var folder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, folderPath);

                            var file = FileManager.Instance.GetFile(folder, fileName);

                            lblLogURL.Text = "FileID=" + file.FileId;
                        }
                        var objUrls = new UrlController();
                        UrlTrackingInfo objUrlTracking = objUrls.GetUrlTracking(PortalSettings.PortalId, lblLogURL.Text, ModuleID);
                        if (objUrlTracking != null)
                        {
                            if (String.IsNullOrEmpty(_FormattedURL))
                            {
                                lblURL.Text = Globals.LinkClick(URL, PortalSettings.ActiveTab.TabID, ModuleID, false);
                                if (!lblURL.Text.StartsWith("http") && !lblURL.Text.StartsWith("mailto"))
                                {
                                    lblURL.Text = Globals.AddHTTP(Request.Url.Host) + lblURL.Text;
                                }
                            }
                            else
                            {
                                lblURL.Text = _FormattedURL;
                            }
                            lblCreatedDate.Text = objUrlTracking.CreatedDate.ToString();

                            if (objUrlTracking.TrackClicks)
                            {
                                pnlTrack.Visible = true;
                                if (String.IsNullOrEmpty(_TrackingURL))
                                {
                                    if (!URL.StartsWith("http"))
                                    {
                                        lblTrackingURL.Text = Globals.AddHTTP(Request.Url.Host);
                                    }
                                    lblTrackingURL.Text += Globals.LinkClick(URL, PortalSettings.ActiveTab.TabID, ModuleID, objUrlTracking.TrackClicks);
                                }
                                else
                                {
                                    lblTrackingURL.Text = _TrackingURL;
                                }
                                lblClicks.Text = objUrlTracking.Clicks.ToString();
                                if (!Null.IsNull(objUrlTracking.LastClick))
                                {
                                    lblLastClick.Text = objUrlTracking.LastClick.ToString();
                                }
                            }
                            if (objUrlTracking.LogActivity)
                            {
                                pnlLog.Visible = true;

                                txtStartDate.Text = DateTime.Today.AddDays(-6).ToShortDateString();
                                txtEndDate.Text = DateTime.Today.AddDays(1).ToShortDateString();
                            }
                        }
                    }
                    else
                    {
                        Visible = false;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdDisplay_Click(object sender, EventArgs e)
        {
            try
            {
                string strStartDate = txtStartDate.Text;
                if (!String.IsNullOrEmpty(strStartDate))
                {
                    strStartDate = strStartDate + " 00:00";
                }
                string strEndDate = txtEndDate.Text;
                if (!String.IsNullOrEmpty(strEndDate))
                {
                    strEndDate = strEndDate + " 23:59";
                }
                var objUrls = new UrlController();
                //localize datagrid
                Localization.LocalizeDataGrid(ref grdLog, LocalResourceFile);
                grdLog.DataSource = objUrls.GetUrlLog(PortalSettings.PortalId, lblLogURL.Text, ModuleID, Convert.ToDateTime(strStartDate), Convert.ToDateTime(strEndDate));
                grdLog.DataBind();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}