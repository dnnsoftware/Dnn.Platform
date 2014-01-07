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
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Scheduler
{

    /// <summary>
    /// The EditSchedule PortalModuleBase is used to edit the scheduled items.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    public partial class EditSchedule : PortalModuleBase, IActionable
    {

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actionCollection = new ModuleActionCollection
                                  {
                                      {
                                          GetNextActionID(), Localization.GetString(ModuleActionType.ContentOptions, LocalResourceFile), ModuleActionType.AddContent, "", "icon_scheduler_16px.gif",
                                          EditUrl("", "", "Status"), false, SecurityAccessLevel.Host, true, false
                                          }
                                  };
                if (Request.QueryString["ScheduleID"] != null)
                {
                    var scheduleID = Convert.ToInt32(Request.QueryString["ScheduleID"]);
                    actionCollection.Add(GetNextActionID(),
                                Localization.GetString("ScheduleHistory.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "icon_profile_16px.gif",
                                EditUrl("ScheduleID", scheduleID.ToString(), "History"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                }
                return actionCollection;
            }
        }

        #endregion

        #region Private Methods

        private void BindData()
        {
            if (Request.QueryString["ScheduleID"] != null)
            {
                ViewState["ScheduleID"] = Request.QueryString["ScheduleID"];
                ScheduleItem scheduleItem = SchedulingProvider.Instance().GetSchedule(Convert.ToInt32(Request.QueryString["ScheduleID"]));

                txtFriendlyName.Text = scheduleItem.FriendlyName;
                txtType.Enabled = false;
                txtType.Text = scheduleItem.TypeFullName;
                chkEnabled.Checked = scheduleItem.Enabled;
                if (scheduleItem.TimeLapse == Null.NullInteger)
                {
                    txtTimeLapse.Text = "";
                }
                else
                {
                    txtTimeLapse.Text = Convert.ToString(scheduleItem.TimeLapse);
                }
                if (ddlTimeLapseMeasurement.FindItemByValue(scheduleItem.TimeLapseMeasurement) != null)
                {
                    ddlTimeLapseMeasurement.FindItemByValue(scheduleItem.TimeLapseMeasurement).Selected = true;
                }
                if (scheduleItem.RetryTimeLapse == Null.NullInteger)
                {
                    txtRetryTimeLapse.Text = "";
                }
                else
                {
                    txtRetryTimeLapse.Text = Convert.ToString(scheduleItem.RetryTimeLapse);
                }
                if (ddlRetryTimeLapseMeasurement.FindItemByValue(scheduleItem.RetryTimeLapseMeasurement) != null)
                {
                    ddlRetryTimeLapseMeasurement.FindItemByValue(scheduleItem.RetryTimeLapseMeasurement).Selected = true;
                }
                if (ddlRetainHistoryNum.FindItemByValue(Convert.ToString(scheduleItem.RetainHistoryNum)) != null)
                {
                    ddlRetainHistoryNum.FindItemByValue(Convert.ToString(scheduleItem.RetainHistoryNum)).Selected = true;
                }
                else
                {
                    string scheduleItemRetainHistoryNum = scheduleItem.RetainHistoryNum.ToString();
                    ddlRetainHistoryNum.AddItem(scheduleItemRetainHistoryNum, scheduleItemRetainHistoryNum);
                    ddlRetainHistoryNum.FindItemByValue(Convert.ToString(scheduleItem.RetainHistoryNum)).Selected = true;
                }
                if (ddlAttachToEvent.FindItemByValue(scheduleItem.AttachToEvent) != null)
                {
                    ddlAttachToEvent.FindItemByValue(scheduleItem.AttachToEvent).Selected = true;
                }
                chkCatchUpEnabled.Checked = scheduleItem.CatchUpEnabled;
                txtObjectDependencies.Text = scheduleItem.ObjectDependencies;
                BindServers(scheduleItem.Servers);
            }
            else
            {
                cmdDelete.Visible = false;
                cmdRun.Visible = false;
                txtType.Enabled = true;
            }
        }

        private void BindServers(string selectedServers)
        {
            var servers = ServerController.GetServers();

            foreach (var webServer in servers)
            {
                if (webServer.Enabled)
                {
                    var serverName = ServerController.GetServerName(webServer);
                    var serverItem = new ListItem(serverName, serverName);
                    if (string.IsNullOrEmpty(selectedServers))
                    {
                        serverItem.Selected = true;
                    }
                    else
                    {
                        serverItem.Selected = Null.NullBoolean;
                        foreach (var selectedServer in selectedServers.Split(','))
                        {
                            if (selectedServer == serverName)
                            {
                                serverItem.Selected = true;
                                break;
                            }
                        }
                    }
                    lstServers.Items.Add(serverItem);
                }
            }
        }

        private ScheduleItem CreateScheduleItem()
        {
            var scheduleItem = new ScheduleItem();
            scheduleItem.TypeFullName = txtType.Text;
            scheduleItem.FriendlyName = txtFriendlyName.Text;
            if (String.IsNullOrEmpty(txtTimeLapse.Text) || txtTimeLapse.Text == "0" || txtTimeLapse.Text == "-1")
            {
                scheduleItem.TimeLapse = Null.NullInteger;
            }
            else
            {
                scheduleItem.TimeLapse = Convert.ToInt32(txtTimeLapse.Text);
            }
            scheduleItem.TimeLapseMeasurement = ddlTimeLapseMeasurement.SelectedItem.Value;

            if (String.IsNullOrEmpty(txtRetryTimeLapse.Text) || txtRetryTimeLapse.Text == "0" || txtRetryTimeLapse.Text == "-1")
            {
                scheduleItem.RetryTimeLapse = Null.NullInteger;
            }
            else
            {
                scheduleItem.RetryTimeLapse = Convert.ToInt32(txtRetryTimeLapse.Text);
            }
            scheduleItem.RetryTimeLapseMeasurement = ddlRetryTimeLapseMeasurement.SelectedItem.Value;
            scheduleItem.RetainHistoryNum = Convert.ToInt32(ddlRetainHistoryNum.SelectedItem.Value);
            scheduleItem.AttachToEvent = ddlAttachToEvent.SelectedItem.Value;
            scheduleItem.CatchUpEnabled = chkCatchUpEnabled.Checked;
            scheduleItem.Enabled = chkEnabled.Checked;
            scheduleItem.ObjectDependencies = txtObjectDependencies.Text;

            //if servers are specified, the concatenated string needs to be prefixed and suffixed by commas ( ie. ",SERVER1,SERVER2," )
            var servers = Null.NullString;
            var bAllSelected = true;
            foreach (ListItem item in lstServers.Items)
            {
                if (item.Selected)
                {
                    servers += "," + item.Value;
                }
                else
                {
                    bAllSelected = Null.NullBoolean;
                }
            }
            if (bAllSelected)
            {
                servers = Null.NullString;
            }
            if (!string.IsNullOrEmpty(servers))
            {
                scheduleItem.Servers = servers + ",";
            }
            return scheduleItem;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// [cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support and localisation
        /// </history>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            jQuery.RequestDnnPluginsRegistration();

            cmdDelete.Click += OnDeleteClick;
            cmdRun.Click += OnRunClick;
            cmdUpdate.Click += OnUpdateClick;

            try
            {
                if (!Page.IsPostBack)
                {
                    cmdCancel.NavigateUrl = Globals.NavigateURL();
                    BindData();
                }

                cmdRun.Enabled = chkEnabled.Checked;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// cmdDelete_Click runs when the Delete Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// [cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support and localisation
        /// </history>
        protected void OnDeleteClick(Object sender, EventArgs e)
        {
            var objScheduleItem = new ScheduleItem {ScheduleID = Convert.ToInt32(ViewState["ScheduleID"])};
            SchedulingProvider.Instance().DeleteSchedule(objScheduleItem);
            var strMessage = Localization.GetString("DeleteSuccess", LocalResourceFile);
            UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.GreenSuccess);
        }

        protected void OnRunClick(object sender, EventArgs e)
        {
            var scheduleItem = CreateScheduleItem();
            if (ViewState["ScheduleID"] != null)
            {
                scheduleItem.ScheduleID = Convert.ToInt32(ViewState["ScheduleID"]);
                SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem);
            }
            SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem);

            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("RunNow", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);

            if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
            {
                SchedulingProvider.Instance().ReStart("Change made to schedule.");
            }
        }

        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// [cnurse]	9/28/2004	Updated to reflect design changes for Help, 508 support and localisation
        /// </history>
        protected void OnUpdateClick(Object sender, EventArgs e)
        {
            var objScheduleItem = CreateScheduleItem();
            if (ViewState["ScheduleID"] != null)
            {
                objScheduleItem.ScheduleID = Convert.ToInt32(ViewState["ScheduleID"]);
                SchedulingProvider.Instance().UpdateSchedule(objScheduleItem);
            }
            else
            {
                SchedulingProvider.Instance().AddSchedule(objScheduleItem);
            }
            var strMessage = Localization.GetString("UpdateSuccess", LocalResourceFile);
            UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.GreenSuccess);
            if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
            {
                SchedulingProvider.Instance().ReStart("Change made to schedule.");
            }
            Response.Redirect(Globals.NavigateURL(), true);
        }

        #endregion

    }
}