// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
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

    public abstract class URLTrackingControl : UserControlBase
    {
        protected Label Label1;
        protected Label Label2;
        protected Label Label3;
        protected Label Label4;
        protected Label Label5;
        protected Label Label6;
        protected Label Label7;
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
        private string _FormattedURL = string.Empty;
        private int _ModuleID = -2;
        private string _TrackingURL = string.Empty;
        private string _URL = string.Empty;
        private string _localResourceFile;

        public string FormattedURL
        {
            get
            {
                return this._FormattedURL;
            }

            set
            {
                this._FormattedURL = value;
            }
        }

        public string TrackingURL
        {
            get
            {
                return this._TrackingURL;
            }

            set
            {
                this._TrackingURL = value;
            }
        }

        public string URL
        {
            get
            {
                return this._URL;
            }

            set
            {
                this._URL = value;
            }
        }

        public int ModuleID
        {
            get
            {
                int moduleID = this._ModuleID;
                if (moduleID == -2)
                {
                    if (this.Request.QueryString["mid"] != null)
                    {
                        int.TryParse(this.Request.QueryString["mid"], out moduleID);
                    }
                }

                return moduleID;
            }

            set
            {
                this._ModuleID = value;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this._localResourceFile))
                {
                    fileRoot = this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/URLTrackingControl.ascx";
                }
                else
                {
                    fileRoot = this._localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this._localResourceFile = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdDisplay.Click += this.cmdDisplay_Click;

            try
            {
                // this needs to execute always to the client script code is registred in InvokePopupCal
                this.cmdStartCalendar.NavigateUrl = Calendar.InvokePopupCal(this.txtStartDate);
                this.cmdEndCalendar.NavigateUrl = Calendar.InvokePopupCal(this.txtEndDate);
                if (!this.Page.IsPostBack)
                {
                    if (!string.IsNullOrEmpty(this._URL))
                    {
                        this.lblLogURL.Text = this.URL; // saved for loading Log grid
                        TabType URLType = Globals.GetURLType(this._URL);
                        if (URLType == TabType.File && this._URL.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            // to handle legacy scenarios before the introduction of the FileServerHandler
                            var fileName = Path.GetFileName(this._URL);

                            var folderPath = this._URL.Substring(0, this._URL.LastIndexOf(fileName));
                            var folder = FolderManager.Instance.GetFolder(this.PortalSettings.PortalId, folderPath);

                            var file = FileManager.Instance.GetFile(folder, fileName);

                            this.lblLogURL.Text = "FileID=" + file.FileId;
                        }

                        var objUrls = new UrlController();
                        UrlTrackingInfo objUrlTracking = objUrls.GetUrlTracking(this.PortalSettings.PortalId, this.lblLogURL.Text, this.ModuleID);
                        if (objUrlTracking != null)
                        {
                            if (string.IsNullOrEmpty(this._FormattedURL))
                            {
                                this.lblURL.Text = Globals.LinkClick(this.URL, this.PortalSettings.ActiveTab.TabID, this.ModuleID, false);
                                if (!this.lblURL.Text.StartsWith("http") && !this.lblURL.Text.StartsWith("mailto"))
                                {
                                    this.lblURL.Text = Globals.AddHTTP(this.Request.Url.Host) + this.lblURL.Text;
                                }
                            }
                            else
                            {
                                this.lblURL.Text = this._FormattedURL;
                            }

                            this.lblCreatedDate.Text = objUrlTracking.CreatedDate.ToString();

                            if (objUrlTracking.TrackClicks)
                            {
                                this.pnlTrack.Visible = true;
                                if (string.IsNullOrEmpty(this._TrackingURL))
                                {
                                    if (!this.URL.StartsWith("http"))
                                    {
                                        this.lblTrackingURL.Text = Globals.AddHTTP(this.Request.Url.Host);
                                    }

                                    this.lblTrackingURL.Text += Globals.LinkClick(this.URL, this.PortalSettings.ActiveTab.TabID, this.ModuleID, objUrlTracking.TrackClicks);
                                }
                                else
                                {
                                    this.lblTrackingURL.Text = this._TrackingURL;
                                }

                                this.lblClicks.Text = objUrlTracking.Clicks.ToString();
                                if (!Null.IsNull(objUrlTracking.LastClick))
                                {
                                    this.lblLastClick.Text = objUrlTracking.LastClick.ToString();
                                }
                            }

                            if (objUrlTracking.LogActivity)
                            {
                                this.pnlLog.Visible = true;

                                this.txtStartDate.Text = DateTime.Today.AddDays(-6).ToShortDateString();
                                this.txtEndDate.Text = DateTime.Today.AddDays(1).ToShortDateString();
                            }
                        }
                    }
                    else
                    {
                        this.Visible = false;
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdDisplay_Click(object sender, EventArgs e)
        {
            try
            {
                string strStartDate = this.txtStartDate.Text;
                if (!string.IsNullOrEmpty(strStartDate))
                {
                    strStartDate = strStartDate + " 00:00";
                }

                string strEndDate = this.txtEndDate.Text;
                if (!string.IsNullOrEmpty(strEndDate))
                {
                    strEndDate = strEndDate + " 23:59";
                }

                var objUrls = new UrlController();

                // localize datagrid
                Localization.LocalizeDataGrid(ref this.grdLog, this.LocalResourceFile);
                this.grdLog.DataSource = objUrls.GetUrlLog(this.PortalSettings.PortalId, this.lblLogURL.Text, this.ModuleID, Convert.ToDateTime(strStartDate), Convert.ToDateTime(strEndDate));
                this.grdLog.DataBind();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
