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
using System.Linq;
using System.Threading;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using Telerik.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Modules.Admin.Scheduler
{

    /// <summary>
    /// The ViewSchedule PortalModuleBase is used to manage the scheduled items.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    public partial class ViewSchedule : PortalModuleBase
    {

		#region Protected Methods

        /// <summary>
        /// GetTimeLapse formats the time lapse as a string
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        protected string GetTimeLapse(int timeLapse, string timeLapseMeasurement)
        {
            if (timeLapse != Null.NullInteger)
            {
                var str = Null.NullString;
                var strPrefix = Localization.GetString("TimeLapsePrefix", LocalResourceFile);
                var strSec = Localization.GetString("Second", LocalResourceFile);
                var strMn = Localization.GetString("Minute", LocalResourceFile);
                var strHour = Localization.GetString("Hour", LocalResourceFile);
                var strDay = Localization.GetString("Day", LocalResourceFile);
                var strWeek = Localization.GetString("Week", LocalResourceFile);
                var strMonth = Localization.GetString("Month", LocalResourceFile);
                var strYear = Localization.GetString("Year", LocalResourceFile);
                var strSecs = Localization.GetString("Seconds");
                var strMns = Localization.GetString("Minutes");
                var strHours = Localization.GetString("Hours");
                var strDays = Localization.GetString("Days");
                var strWeeks = Localization.GetString("Weeks");
                var strMonths = Localization.GetString("Months");
                var strYears = Localization.GetString("Years");
                switch (timeLapseMeasurement)
                {
                    case "s":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strSecs : strSec);
                        break;
                    case "m":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strMns : strMn);
                        break;
                    case "h":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strHours : strHour);
                        break;
                    case "d":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strDays : strDay);
                        break;
                    case "w":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strWeeks : strWeek);
                        break;
                    case "mo":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strMonths : strMonth);
                        break;
                    case "y":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strYears : strYear);
                        break;
                }
                return str;
            }
            return Localization.GetString("n/a", LocalResourceFile);
        }

		#endregion

		#region Event Handlers

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdAdd.NavigateUrl = EditUrl();
            cmdStatus.NavigateUrl = EditUrl("", "", "Status");
            cmdHistory.NavigateUrl = EditUrl("", "", "History");
            cmdUpdate.Click+=cmdUpdate_Click;
            ddlServerName.SelectedIndexChanged+=ddlServerName_SelectedIndexChanged;

            try
            {

                dgSchedule.NeedDataSource += OnGridNeedDataSource;
                dgSchedule.ItemDataBound += OnGridItemDataBound;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            if (!Page.IsPostBack)
            {
                BindSettings();
                BindServers();
            }
        }

        private void ddlServerName_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            dgSchedule.Rebind();
        }

        protected void UpdateSchedule()
        {
         var originalSchedulerMode = (SchedulerMode)Convert.ToInt32(ViewState["SelectedSchedulerMode"]);
            var newSchedulerMode = (SchedulerMode)Enum.Parse(typeof(SchedulerMode), cboSchedulerMode.SelectedItem.Value);
            if (originalSchedulerMode != newSchedulerMode)
            {
                switch (newSchedulerMode)
                {
                    case SchedulerMode.DISABLED:
                        SchedulingProvider.Instance().Halt("Host Settings");
                        break;
                    case SchedulerMode.TIMER_METHOD:
                        var newThread = new Thread(SchedulingProvider.Instance().Start) { IsBackground = true };
                        newThread.Start();
                        break;
                    default:
                        SchedulingProvider.Instance().Halt("Host Settings");
                        break;
                }
            }
            }
        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            UpdateSchedule();
            HostController.Instance.Update("SchedulerMode", cboSchedulerMode.SelectedItem.Value, false);
            HostController.Instance.Update("SchedulerdelayAtAppStart", txtScheduleAppStartDelay.Text);
        }

        private void BindSettings()
        {
            if (cboSchedulerMode.FindItemByValue(((int)Entities.Host.Host.SchedulerMode).ToString()) != null)
            {
                cboSchedulerMode.FindItemByValue(((int)Entities.Host.Host.SchedulerMode).ToString()).Selected = true;
            }
            else
            {
                cboSchedulerMode.FindItemByValue("1").Selected = true;
            }
            ViewState["SelectedSchedulerMode"] = cboSchedulerMode.SelectedItem.Value;
            txtScheduleAppStartDelay.Text = HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1).ToString();
        }

        private void BindServers()
        {
            var servers = ServerController.GetServers();
            foreach (var webServer in servers)
            {
                if (webServer.Enabled)
                {
                    var serverName = ServerController.GetServerName(webServer);
                    ddlServerName.AddItem(serverName, serverName);
                }
            }
        }

        protected void OnGridNeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            List<ScheduleItem> scheduleviews;
            if (ddlServerName.SelectedIndex == 0)
            {
                scheduleviews = SchedulingController.GetSchedule();
            }
            else
            {
                scheduleviews = SchedulingController.GetSchedule(ddlServerName.SelectedValue);
            }

            foreach (var item in scheduleviews.Where(x => x.NextStart == Null.NullDate))
                if (item.ScheduleStartDate != Null.NullDate)
                    item.NextStart = item.ScheduleStartDate;
            var arrSchedule = scheduleviews.ToArray();
            dgSchedule.DataSource = arrSchedule;
        }

        protected void OnGridItemDataBound(object sender, GridItemEventArgs e)
        {
            if (!(e.Item is GridDataItem)) return;
            var scheduleKey = (int)e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["ScheduleID"];
            var enabledKey = (bool)e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["Enabled"];
            var timeLapseKey = (int)e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["TimeLapse"];
            var retryTimeLapseKey = (int)e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["RetryTimeLapse"];
            var timeLapseMeasurementKey = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["TimeLapseMeasurement"].ToString();
            var retryTimeLapseMeasurementKey = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["RetryTimeLapseMeasurement"].ToString();
            var nextStartKey = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["NextStart"].ToString();

            var dataItem = (GridDataItem)e.Item;

            var hlEdit = ((HyperLink)(dataItem)["EditItem"].FindControl("hlEdit"));
            hlEdit.NavigateUrl = EditUrl("ScheduleID", scheduleKey.ToString());
            hlEdit.Visible = IsEditable;

            var imgEdit = ((Image)(dataItem)["EditItem"].FindControl("imgEdit"));
            imgEdit.AlternateText = Localization.GetString("Edit", LocalResourceFile);
            imgEdit.ToolTip = Localization.GetString("Edit", LocalResourceFile);
            imgEdit.Visible = IsEditable;

            var lblFrequency = ((Label)(dataItem)["Frequency"].FindControl("lblFrequency"));
            lblFrequency.Text = GetTimeLapse(timeLapseKey, timeLapseMeasurementKey);

            var lblRetryTimeLapse = ((Label)(dataItem)["RetryTimeLapse"].FindControl("lblRetryTimeLapse"));
            lblRetryTimeLapse.Text = GetTimeLapse(retryTimeLapseKey, retryTimeLapseMeasurementKey);

            var lblNextStart = ((Label)(dataItem)["NextStart"].FindControl("lblNextStart"));
            lblNextStart.Text = nextStartKey;
            lblNextStart.Visible = enabledKey;

            var hlHistory = ((HyperLink)(dataItem)["ViewHistory"].FindControl("hlHistory"));
            hlHistory.NavigateUrl = EditUrl("ScheduleID", scheduleKey.ToString(), "History");

            var imgHistory = ((Image)(dataItem)["ViewHistory"].FindControl("imgHistory"));
            imgHistory.AlternateText = Localization.GetString("History", LocalResourceFile);
            imgHistory.ToolTip = Localization.GetString("History", LocalResourceFile);
        }
		
		#endregion

    }
}