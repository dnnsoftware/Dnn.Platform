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
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.SiteLog;
using DotNetNuke.UI.Skins.Controls;

using Calendar = DotNetNuke.Common.Utilities.Calendar;

#endregion

namespace DotNetNuke.Modules.Admin.SiteLog
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SiteLog PortalModuleBase is used to display Logs for the Site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/15/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class SiteLog : PortalModuleBase
	{

		#region "Private Properties"

    	private DateTime DefaultBeginDate
    	{
    		get
    		{
				return DateTime.Today.AddDays(-6);
    		}
    	}

    	private DateTime DefaultEndDate
    	{
    		get
    		{
    			return DateTime.Today.AddDays(1);
    		}
    	}

		#endregion

		#region Private Methods

		/// -----------------------------------------------------------------------------
        /// <summary>
        /// BindData binds the controls to the Data
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/15/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BindData()
        {
            var strPortalAlias = Globals.GetPortalDomainName(PortalAlias.HTTPAlias, Request, false);
            if (strPortalAlias.IndexOf("/") != -1) //child portal
            {
                strPortalAlias = strPortalAlias.Substring(0, strPortalAlias.LastIndexOf("/") - 1);
            }
			var dtStart = DefaultBeginDate;
			var dtEnd = DefaultEndDate.Add(new TimeSpan(0, 23, 59, 59));

			if(diStartDate.SelectedDate.HasValue)
			{
				dtStart = diStartDate.SelectedDate.Value;
			}

			if(diEndDate.SelectedDate.HasValue)
			{
				dtEnd = diEndDate.SelectedDate.Value.Add(new TimeSpan(0, 23, 59, 59));
			}

            ArrayList arrUsers;
            var dt = new DataTable();
            DataView dv;
            switch (cboReportType.SelectedItem.Value)
            {
                case "10":
                    arrUsers = UserController.GetUsers(PortalId);
                    dt = new DataTable();

                    dt.Columns.Add(new DataColumn("Full Name", typeof (string)));
                    dt.Columns.Add(new DataColumn("User Name", typeof (string)));
                    dt.Columns.Add(new DataColumn("Date Registered", typeof (DateTime)));

                    foreach (UserInfo objUser in arrUsers)
                    {
                        if (objUser.Membership.CreatedDate >= dtStart && objUser.Membership.CreatedDate <= dtEnd && objUser.IsSuperUser == false)
                        {
                            var dr = dt.NewRow();

                            dr["Date Registered"] = objUser.Membership.CreatedDate;
                            dr["Full Name"] = objUser.Profile.FullName;
                            dr["User Name"] = objUser.Username;

                            dt.Rows.Add(dr);
                        }
                    }

                    dv = new DataView(dt) {Sort = "Date Registered DESC"};
                    grdLog.DataSource = dv;
                    grdLog.DataBind();
                    break;
                case "11": //User Registrations By Country
                    arrUsers = UserController.GetUsers(PortalId);
                    dt = new DataTable();

                    dt.Columns.Add(new DataColumn("Full Name", typeof (string)));
                    dt.Columns.Add(new DataColumn("User Name", typeof (string)));
                    dt.Columns.Add(new DataColumn("Country", typeof (string)));

                    foreach (UserInfo objUser in arrUsers)
                    {
                        if (objUser.Membership.CreatedDate >= dtStart && objUser.Membership.CreatedDate <= dtEnd && objUser.IsSuperUser == false)
                        {
                            var dr = dt.NewRow();

                            dr["Country"] = objUser.Profile.Country;
                            dr["Full Name"] = objUser.Profile.FullName;
                            dr["User Name"] = objUser.Username;

                            dt.Rows.Add(dr);
                        }
                    }

                    dv = new DataView(dt) {Sort = "Country"};
                    grdLog.DataSource = dv;
                    grdLog.DataBind();
                    break;
                default:
                    var objSiteLog = new SiteLogController();
                    var reader = objSiteLog.GetSiteLog(PortalId,
                                                               strPortalAlias,
                                                               Convert.ToInt32(cboReportType.SelectedItem.Value),
                                                               dtStart,
                                                               dtEnd);
                    grdLog.DataSource = reader; //we are using a DataReader here because the resultset returned by GetSiteLog varies based on the report type selected and therefore does not conform to a static business object
                    grdLog.DataBind();
                    reader.Close();
                    break;
            }
            if (grdLog.Items.Count > 0)
            {
                lblMessage.Visible = false;
                grdLog.Visible = true;
            }
            else
            {
                lblMessage.Visible = true;
                grdLog.Visible = false;
            }
        }

        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/15/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdDisplay.Click += OnDisplayClick;

            try
            {
                //If this is the first visit to the page, bind the role data to the datalist
                if (Page.IsPostBack == false)
                {
                    var strSiteLogStorage = "D";
                    if (!string.IsNullOrEmpty(Host.SiteLogStorage))
                    {
                        strSiteLogStorage = Host.SiteLogStorage;
                    }
                    if (strSiteLogStorage == "F")
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("FileSystem", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                        cmdDisplay.Visible = false;
                    }
                    else
                    {
                        switch (PortalSettings.SiteLogHistory)
                        {
                            case -1: //unlimited
                                break;
                            case 0:
                                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("LogDisabled", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                                break;
                            default:
                                UI.Skins.Skin.AddModuleMessage(this,
                                                               string.Format(Localization.GetString("LogHistory", LocalResourceFile), PortalSettings.SiteLogHistory),
                                                               ModuleMessage.ModuleMessageType.YellowWarning);
                                break;
                        }
                        cmdDisplay.Visible = true;
                    }
                    var ctlList = new ListController();
                    var colSiteLogReports = ctlList.GetListEntryInfoItems("Site Log Reports");

                    cboReportType.DataSource = colSiteLogReports;
                    cboReportType.DataBind();
                    cboReportType.SelectedIndex = 0;

                    diStartDate.SelectedDate = DefaultBeginDate;
                    diEndDate.SelectedDate = DefaultEndDate;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDisplay_Click runs when the Display Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/15/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnDisplayClick(object sender, EventArgs e)
        {
            try
            {
                BindData();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        ///   ensure sitelog grid data is htmlencoded
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        /// <remarks>
        /// </remarks>
        protected void grdLog_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;
            var itemType = item.ItemType;
            if (((itemType == ListItemType.AlternatingItem) || (itemType == ListItemType.Item)))
            {
                var cells = item.Cells;
                foreach (TableCell cell in cells)
                {
                    cell.Text = HttpUtility.HtmlEncode(cell.Text);
                }
            }
        }

        #endregion

    }
}