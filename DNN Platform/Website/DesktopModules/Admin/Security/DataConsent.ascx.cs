// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    public partial class DataConsent : UserModuleBase
    {
        public string DeleteMeConfirmString
        {
            get
            {
                switch (PortalSettings.DataConsentUserDeleteAction)
                {
                    case PortalSettings.UserDeleteAction.Manual:
                        return LocalizeString("ManualDelete.Confirm");
                    case PortalSettings.UserDeleteAction.DelayedHardDelete:
                        return LocalizeString("DelayedHardDelete.Confirm");
                    case PortalSettings.UserDeleteAction.HardDelete:
                        return LocalizeString("HardDelete.Confirm");
                }
                return "";
            }
        }

        #region Delegate and event

        public delegate void DataConsentEventHandler(object sender, DataConsentEventArgs e);
        public event DataConsentEventHandler DataConsentCompleted;
        public void OnDataConsentComplete(DataConsentEventArgs e)
        {
            DataConsentCompleted?.Invoke(this, e);
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdCancel.Click += cmdCancel_Click;
            cmdSubmit.Click += cmdSubmit_Click;
            cmdDeleteMe.Click += cmdDeleteMe_Click;
            cmdDeleteMe.Visible = PortalSettings.DataConsentUserDeleteAction != PortalSettings.UserDeleteAction.Off;
            if (!Page.IsPostBack)
            {
                chkAgree.Checked = false;
                cmdSubmit.Enabled = false;
                pnlNoAgreement.Visible = false;
            }
            chkAgree.Attributes.Add("onclick", string.Format("document.getElementById('{0}').disabled = !this.checked;", cmdSubmit.ClientID));
            cmdDeleteMe.Attributes.Add("onclick", string.Format("if (!confirm('{0}')) this.preventDefault();", DeleteMeConfirmString));
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Cancelled));
        }

        private void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (chkAgree.Checked)
            {
                UserController.UserAgreedToTerms(User);
                User.HasAgreedToTerms = true;
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Consented));
            }
        }

        private void cmdDeleteMe_Click(object sender, EventArgs e)
        {
            var success = false;
            switch (PortalSettings.DataConsentUserDeleteAction)
            {
                case PortalSettings.UserDeleteAction.Manual:
                    User.Membership.Approved = false;
                    UserController.UpdateUser(PortalSettings.PortalId, User);
                    UserController.UserRequestsRemoval(User, true);
                    success = true;
                    break;
                case PortalSettings.UserDeleteAction.DelayedHardDelete:
                    var user = User;
                    success = UserController.DeleteUser(ref user, true, false);
                    UserController.UserRequestsRemoval(User, true);
                    break;
                case PortalSettings.UserDeleteAction.HardDelete:
                    success = UserController.RemoveUser(User);
                    break;
            }
            if (success)
            {
                PortalSecurity.Instance.SignOut();
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.RemovedAccount));
            }
            else
            {
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.FailedToRemoveAccount));
            }
        }

        #region DataConsentEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DataConsentEventArgs class provides a customised EventArgs class for
        /// the DataConsent Event
        /// </summary>
        public class DataConsentEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new DataConsentEventArgs
            /// </summary>
            /// <param name="status">The Data Consent Status</param>
            public DataConsentEventArgs(DataConsentStatus status)
            {
                Status = status;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the Update Status
            /// </summary>
            public DataConsentStatus Status { get; set; }
        }

        public enum DataConsentStatus
        {
            Consented,
            Cancelled,
            RemovedAccount,
            FailedToRemoveAccount
        }

        #endregion

    }
}
