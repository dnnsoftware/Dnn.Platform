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
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.UserControls;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.LogViewer
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : EditLogTypes
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Manage the Log Types for the portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [cnurse] 17/9/2004  Updated for localization, Help and 508. 
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class EditLogTypes : PortalModuleBase, IActionable
    {

        protected LabelControl PlThresholdNotificationTime;
        protected LabelControl PlThresholdNotificationTimeType;

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actionCollection = new ModuleActionCollection
                                           {
                                               {
                                                   GetNextActionID(), Localization.GetString(ModuleActionType.AddContent, LocalResourceFile), ModuleActionType.AddContent, "", "", EditUrl("action", "add")
                                                   , false, SecurityAccessLevel.Admin, true, false
                                                   }
                                           };
                return actionCollection;
            }
        }

        #endregion

        #region Private Methods

        private void BindDetailData()
        {
            var pc = new PortalController();
            ddlLogTypePortalID.DataTextField = "PortalName";
            ddlLogTypePortalID.DataValueField = "PortalID";
            ddlLogTypePortalID.DataSource = pc.GetPortals();
            ddlLogTypePortalID.DataBind();

// ReSharper disable LocalizableElement
            var i = new ListItem {Text = Localization.GetString("All"), Value = "*"};
// ReSharper restore LocalizableElement
            ddlLogTypePortalID.Items.Insert(0, i);


            pnlEditLogTypeConfigInfo.Visible = true;
            pnlLogTypeConfigInfo.Visible = false;
            var logController = new LogController();

        	var arrLogTypeInfo = logController.GetLogTypeInfoDictionary().Values.OrderBy(t => t.LogTypeFriendlyName);

            ddlLogTypeKey.DataTextField = "LogTypeFriendlyName";
            ddlLogTypeKey.DataValueField = "LogTypeKey";
            ddlLogTypeKey.DataSource = arrLogTypeInfo;
            ddlLogTypeKey.DataBind();

            int[] items = {1, 2, 3, 4, 5, 10, 25, 100, 250, 500};
            ddlKeepMostRecent.Items.Clear();
            ddlKeepMostRecent.Items.Add(new ListItem(Localization.GetString("All"), "*"));
            foreach (int item in items)
            {
                ddlKeepMostRecent.Items.Add(item == 1
                                                ? new ListItem(item + Localization.GetString("LogEntry", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture))
                                                : new ListItem(item + Localization.GetString("LogEntries", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture)));
            }
            int[] items2 = {1, 2, 3, 4, 5, 10, 25, 100, 250, 500, 1000};
            ddlThreshold.Items.Clear();
            foreach (int item in items2)
            {
                ddlThreshold.Items.Add(item == 1
                                           ? new ListItem(item + Localization.GetString("Occurence", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture))
                                           : new ListItem(item + Localization.GetString("Occurences", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture)));
            }
// ReSharper disable LocalizableElement
            var j = new ListItem {Text = Localization.GetString("All"), Value = "*"};
// ReSharper restore LocalizableElement
            ddlLogTypeKey.Items.Insert(0, j);
        }

        private void BindSummaryData()
        {
            var objLogController = new LogController();
            ArrayList arrLogTypeConfigInfo = objLogController.GetLogTypeConfigInfo();

            dgLogTypeConfigInfo.DataSource = arrLogTypeConfigInfo;
            dgLogTypeConfigInfo.DataBind();
            pnlEditLogTypeConfigInfo.Visible = false;
            pnlLogTypeConfigInfo.Visible = true;
        }

        private void DisableLoggingControls()
        {
            if (chkIsActive.Checked)
            {
                ddlLogTypeKey.Enabled = true;
                ddlLogTypePortalID.Enabled = true;
                ddlKeepMostRecent.Enabled = true;
            }
            else
            {
                ddlLogTypeKey.Enabled = false;
                ddlLogTypePortalID.Enabled = false;
                ddlKeepMostRecent.Enabled = false;
            }
        }

        private void DisableNotificationControls()
        {
            if (chkEmailNotificationStatus.Checked)
            {
                ddlThreshold.Enabled = true;
                ddlThresholdNotificationTime.Enabled = true;
                ddlThresholdNotificationTimeType.Enabled = true;
                txtMailFromAddress.Enabled = true;
                txtMailToAddress.Enabled = true;
            }
            else
            {
                ddlThreshold.Enabled = false;
                ddlThresholdNotificationTime.Enabled = false;
                ddlThresholdNotificationTimeType.Enabled = false;
                txtMailFromAddress.Enabled = false;
                txtMailToAddress.Enabled = false;
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
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            jQuery.RequestDnnPluginsRegistration();

            cmdDelete.Click += OnDeleteClick;
            cmdCancel.Click += OnCancelClick;
            cmdUpdate.Click += OnUpdateClick;
            chkEmailNotificationStatus.CheckedChanged += ChkEmailNotificationStatusCheckedChanged;
            chkIsActive.CheckedChanged += ChkIsActiveCheckedChanged;
            dgLogTypeConfigInfo.EditCommand += DgLogTypeConfigInfoEditCommand;

            try
            {
                if (!Page.IsPostBack)
                {
                    hlReturn.NavigateUrl = Globals.NavigateURL();

                    if (Request.QueryString["action"] == "add")
                    {
                        BindDetailData();
                    }
                    else
                    {
                        BindSummaryData();
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the cancel Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnCancelClick(Object sender, EventArgs e)
        {
            try
            {
                BindSummaryData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the delete Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnDeleteClick(Object sender, EventArgs e)
        {
            var objLogTypeConfigInfo = new LogTypeConfigInfo();
            var l = new LogController();
            objLogTypeConfigInfo.ID = Convert.ToString(ViewState["LogID"]);
            try
            {
                l.DeleteLogTypeConfigInfo(objLogTypeConfigInfo);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ConfigDeleted", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                BindSummaryData();
            }
            catch
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DeleteError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnUpdateClick(Object sender, EventArgs e)
        {
            var objLogTypeConfigInfo = new LogTypeConfigInfo
                                           {
                                               LoggingIsActive = chkIsActive.Checked,
                                               LogTypeKey = ddlLogTypeKey.SelectedItem.Value,
                                               LogTypePortalID = ddlLogTypePortalID.SelectedItem.Value,
                                               KeepMostRecent = ddlKeepMostRecent.SelectedItem.Value,

                                               EmailNotificationIsActive = chkEmailNotificationStatus.Checked,
                                               NotificationThreshold = Convert.ToInt32(ddlThreshold.SelectedItem.Value),
                                               NotificationThresholdTime = Convert.ToInt32(ddlThresholdNotificationTime.SelectedItem.Value),
                                               NotificationThresholdTimeType =
                                                   (LogTypeConfigInfo.NotificationThresholdTimeTypes)
                                                   Enum.Parse(typeof (LogTypeConfigInfo.NotificationThresholdTimeTypes), ddlThresholdNotificationTimeType.SelectedItem.Value),
                                               MailFromAddress = txtMailFromAddress.Text,
                                               MailToAddress = txtMailToAddress.Text
                                           };
            var l = new LogController();

            if (ViewState["LogID"] != null)
            {
                objLogTypeConfigInfo.ID = Convert.ToString(ViewState["LogID"]);
                l.UpdateLogTypeConfigInfo(objLogTypeConfigInfo);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ConfigUpdated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            else
            {
                objLogTypeConfigInfo.ID = Guid.NewGuid().ToString();
                l.AddLogTypeConfigInfo(objLogTypeConfigInfo);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ConfigAdded", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            BindSummaryData();
        }

        protected void ChkEmailNotificationStatusCheckedChanged(Object sender, EventArgs e)
        {
            // TODO: Revisit
            DisableNotificationControls();
        }

        protected void ChkIsActiveCheckedChanged(Object sender, EventArgs e)
        {
            // TODO: Revisit
            DisableLoggingControls();
        }

        protected void DgLogTypeConfigInfoEditCommand(object source, GridCommandEventArgs e)
        {
            var logID = Convert.ToString(((GridDataItem)e.Item).GetDataKeyValue("ID"));
            ViewState["LogID"] = logID;

            BindDetailData();

            var l = new LogController();

            LogTypeConfigInfo objLogTypeConfigInfo = l.GetLogTypeConfigInfoByID(logID);

            chkIsActive.Checked = objLogTypeConfigInfo.LoggingIsActive;
            chkEmailNotificationStatus.Checked = objLogTypeConfigInfo.EmailNotificationIsActive;

            if (ddlLogTypeKey.Items.FindByValue(objLogTypeConfigInfo.LogTypeKey) != null)
            {
                ddlLogTypeKey.ClearSelection();
                ddlLogTypeKey.Items.FindByValue(objLogTypeConfigInfo.LogTypeKey).Selected = true;
            }
            if (ddlLogTypePortalID.Items.FindByValue(objLogTypeConfigInfo.LogTypePortalID) != null)
            {
                ddlLogTypePortalID.ClearSelection();
                ddlLogTypePortalID.Items.FindByValue(objLogTypeConfigInfo.LogTypePortalID).Selected = true;
            }
            if (ddlKeepMostRecent.Items.FindByValue(objLogTypeConfigInfo.KeepMostRecent) != null)
            {
                ddlKeepMostRecent.ClearSelection();
                ddlKeepMostRecent.Items.FindByValue(objLogTypeConfigInfo.KeepMostRecent).Selected = true;
            }
            if (ddlThreshold.Items.FindByValue(objLogTypeConfigInfo.NotificationThreshold.ToString(CultureInfo.InvariantCulture)) != null)
            {
                ddlThreshold.ClearSelection();
                ddlThreshold.Items.FindByValue(objLogTypeConfigInfo.NotificationThreshold.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
            if (ddlThresholdNotificationTime.Items.FindByValue(objLogTypeConfigInfo.NotificationThresholdTime.ToString(CultureInfo.InvariantCulture)) != null)
            {
                ddlThresholdNotificationTime.ClearSelection();
                ddlThresholdNotificationTime.Items.FindByValue(objLogTypeConfigInfo.NotificationThresholdTime.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
            if (ddlThresholdNotificationTimeType.Items.FindByText(objLogTypeConfigInfo.NotificationThresholdTimeType.ToString()) != null)
            {
                ddlThresholdNotificationTimeType.ClearSelection();
                ddlThresholdNotificationTimeType.Items.FindByText(objLogTypeConfigInfo.NotificationThresholdTimeType.ToString()).Selected = true;
            }
            txtMailFromAddress.Text = objLogTypeConfigInfo.MailFromAddress;
            txtMailToAddress.Text = objLogTypeConfigInfo.MailToAddress;
            DisableLoggingControls();

            e.Canceled = true; //disable inline editing in grid
        }

        #endregion

    }
}