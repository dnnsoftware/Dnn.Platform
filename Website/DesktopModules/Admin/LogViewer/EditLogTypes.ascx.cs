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
using DotNetNuke.Web.UI.WebControls;
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
                return new ModuleActionCollection();
            }
        }

        #endregion

        #region Private Methods

        private void BindDetailData()
        {
            cboLogTypePortalID.DataTextField = "PortalName";
            cboLogTypePortalID.DataValueField = "PortalID";
            cboLogTypePortalID.DataSource = PortalController.Instance.GetPortals();
            cboLogTypePortalID.DataBind();

// ReSharper disable LocalizableElement
            var i = new DnnComboBoxItem{Text = Localization.GetString("All"), Value = "*"};
// ReSharper restore LocalizableElement
            cboLogTypePortalID.Items.Insert(0, i);


            pnlEditLogTypeConfigInfo.Visible = true;
            pnlLogTypeConfigInfo.Visible = false;

            var arrLogTypeInfo = LogController.Instance.GetLogTypeInfoDictionary().Values.OrderBy(t => t.LogTypeFriendlyName);

            cboLogTypeKey.DataTextField = "LogTypeFriendlyName";
            cboLogTypeKey.DataValueField = "LogTypeKey";
            cboLogTypeKey.DataSource = arrLogTypeInfo;
            cboLogTypeKey.DataBind();

            int[] items = {1, 2, 3, 4, 5, 10, 25, 100, 250, 500};
            cboKeepMostRecent.Items.Clear();
            cboKeepMostRecent.Items.Add(new DnnComboBoxItem(Localization.GetString("All"), "*"));
            foreach (int item in items)
            {
                cboKeepMostRecent.Items.Add(item == 1
                                                ? new DnnComboBoxItem(item + Localization.GetString("LogEntry", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture))
                                                : new DnnComboBoxItem(item + Localization.GetString("LogEntries", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture)));
            }
            int[] items2 = {1, 2, 3, 4, 5, 10, 25, 100, 250, 500, 1000};
            cboThreshold.Items.Clear();
            foreach (int item in items2)
            {
                cboThreshold.Items.Add(item == 1
                                           ? new DnnComboBoxItem(item + Localization.GetString("Occurence", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture))
                                           : new DnnComboBoxItem(item + Localization.GetString("Occurences", LocalResourceFile), item.ToString(CultureInfo.InvariantCulture)));
            }

            cboThresholdNotificationTime.Items.Clear();
            foreach (int item in new []{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 60, 90, 120})
            {
                cboThresholdNotificationTime.Items.Add(new DnnComboBoxItem(item.ToString(CultureInfo.InvariantCulture), item.ToString(CultureInfo.InvariantCulture)));
            }

            cboThresholdNotificationTimeType.Items.Clear();
            foreach (int item in new[] { 1, 2, 3, 4 })
            {
                cboThresholdNotificationTimeType.Items.Add(new DnnComboBoxItem(Localization.GetString(string.Format("TimeType_{0}", item), LocalResourceFile), item.ToString(CultureInfo.InvariantCulture)));
            }
// ReSharper disable LocalizableElement
            var j = new DnnComboBoxItem{Text = Localization.GetString("All"), Value = "*"};
// ReSharper restore LocalizableElement
            cboLogTypeKey.Items.Insert(0, j);
        }

        private void BindSummaryData()
        {
            ArrayList arrLogTypeConfigInfo = LogController.Instance.GetLogTypeConfigInfo();

            dgLogTypeConfigInfo.DataSource = arrLogTypeConfigInfo;
            dgLogTypeConfigInfo.DataBind();
            pnlEditLogTypeConfigInfo.Visible = false;
            pnlLogTypeConfigInfo.Visible = true;
        }

        private void DisableLoggingControls()
        {
            if (chkIsActive.Checked)
            {
                cboLogTypeKey.Enabled = true;
                cboLogTypePortalID.Enabled = true;
                cboKeepMostRecent.Enabled = true;
            }
            else
            {
                cboLogTypeKey.Enabled = false;
                cboLogTypePortalID.Enabled = false;
                cboKeepMostRecent.Enabled = false;
            }
        }

        private void DisableNotificationControls()
        {
            if (chkEmailNotificationStatus.Checked)
            {
                cboThreshold.Enabled = true;
                cboThresholdNotificationTime.Enabled = true;
                cboThresholdNotificationTimeType.Enabled = true;
                txtMailFromAddress.Enabled = true;
                txtMailToAddress.Enabled = true;
            }
            else
            {
                cboThreshold.Enabled = false;
                cboThresholdNotificationTime.Enabled = false;
                cboThresholdNotificationTimeType.Enabled = false;
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
                    hlAdd.NavigateUrl = EditUrl("action", "add");

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
            var logTypeConfigInfo = new LogTypeConfigInfo();
            logTypeConfigInfo.ID = Convert.ToString(ViewState["LogID"]);
            try
            {
                LogController.Instance.DeleteLogTypeConfigInfo(logTypeConfigInfo);
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
                                               LogTypeKey = cboLogTypeKey.SelectedItem.Value,
                                               LogTypePortalID = cboLogTypePortalID.SelectedItem.Value,
                                               KeepMostRecent = cboKeepMostRecent.SelectedItem.Value,

                                               EmailNotificationIsActive = chkEmailNotificationStatus.Checked,
                                               NotificationThreshold = Convert.ToInt32(cboThreshold.SelectedItem.Value),
                                               NotificationThresholdTime = Convert.ToInt32(cboThresholdNotificationTime.SelectedItem.Value),
                                               NotificationThresholdTimeType =
                                                   (LogTypeConfigInfo.NotificationThresholdTimeTypes)
                                                   Enum.Parse(typeof(LogTypeConfigInfo.NotificationThresholdTimeTypes), cboThresholdNotificationTimeType.SelectedItem.Value),
                                               MailFromAddress = txtMailFromAddress.Text,
                                               MailToAddress = txtMailToAddress.Text
                                           };

            if (ViewState["LogID"] != null)
            {
                objLogTypeConfigInfo.ID = Convert.ToString(ViewState["LogID"]);
                LogController.Instance.UpdateLogTypeConfigInfo(objLogTypeConfigInfo);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ConfigUpdated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            else
            {
                objLogTypeConfigInfo.ID = Guid.NewGuid().ToString();
                LogController.Instance.AddLogTypeConfigInfo(objLogTypeConfigInfo);
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

            LogTypeConfigInfo objLogTypeConfigInfo = LogController.Instance.GetLogTypeConfigInfoByID(logID);

            chkIsActive.Checked = objLogTypeConfigInfo.LoggingIsActive;
            chkEmailNotificationStatus.Checked = objLogTypeConfigInfo.EmailNotificationIsActive;

            if (cboLogTypeKey.FindItemByValue(objLogTypeConfigInfo.LogTypeKey) != null)
            {
                cboLogTypeKey.ClearSelection();
                cboLogTypeKey.FindItemByValue(objLogTypeConfigInfo.LogTypeKey).Selected = true;
            }
            if (cboLogTypePortalID.FindItemByValue(objLogTypeConfigInfo.LogTypePortalID) != null)
            {
                cboLogTypePortalID.ClearSelection();
                cboLogTypePortalID.FindItemByValue(objLogTypeConfigInfo.LogTypePortalID).Selected = true;
            }
            if (cboKeepMostRecent.FindItemByValue(objLogTypeConfigInfo.KeepMostRecent) != null)
            {
                cboKeepMostRecent.ClearSelection();
                cboKeepMostRecent.FindItemByValue(objLogTypeConfigInfo.KeepMostRecent).Selected = true;
            }
            if (cboThreshold.FindItemByValue(objLogTypeConfigInfo.NotificationThreshold.ToString(CultureInfo.InvariantCulture)) != null)
            {
                cboThreshold.ClearSelection();
                cboThreshold.FindItemByValue(objLogTypeConfigInfo.NotificationThreshold.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
            if (cboThresholdNotificationTime.FindItemByValue(objLogTypeConfigInfo.NotificationThresholdTime.ToString(CultureInfo.InvariantCulture)) != null)
            {
                cboThresholdNotificationTime.ClearSelection();
                cboThresholdNotificationTime.FindItemByValue(objLogTypeConfigInfo.NotificationThresholdTime.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
            if (cboThresholdNotificationTimeType.FindItemByText(objLogTypeConfigInfo.NotificationThresholdTimeType.ToString()) != null)
            {
                cboThresholdNotificationTimeType.ClearSelection();
                cboThresholdNotificationTimeType.FindItemByText(objLogTypeConfigInfo.NotificationThresholdTimeType.ToString()).Selected = true;
            }
            txtMailFromAddress.Text = objLogTypeConfigInfo.MailFromAddress;
            txtMailToAddress.Text = objLogTypeConfigInfo.MailToAddress;

            DisableLoggingControls();
            DisableNotificationControls();

            e.Canceled = true; //disable inline editing in grid
        }

        #endregion

    }
}